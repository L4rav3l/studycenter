using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class VerifyController : ControllerBase
    {
        private readonly Postgresql _connection;

        public VerifyController(Postgresql connection)
        {
            _connection = connection;
        }

        [HttpPost("/api/register/verify")]
        public async Task<IActionResult> Verify([FromBody] TokenRequest request)
        {
            var user = await VerifyToken(request.Token);

            if (user == null)
            {
                return Unauthorized(new { error = 8 });
            }

            if (user.token_version == 0)
            {
                var conn = await _connection.GetOpenConnectionAsync();

                await using (var update = new NpgsqlCommand("UPDATE users SET active = true WHERE id = @id AND token_version = @token_version", conn))
                {
                    update.Parameters.AddWithValue("id", user.id);
                    update.Parameters.AddWithValue("token_version", user.token_version);

                    await update.ExecuteNonQueryAsync();

                    await conn.CloseAsync();
                    return Ok(new { status = 1 });
                }
            }
            else
            {
                return Conflict(new { error = 7 });
            }
        }

        private async Task<UsersData?> VerifyToken(string token)
        {
            try
            {
                var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = true,
                    ValidIssuer = "SC:API",
                    ValidateAudience = true,
                    ValidAudience = "SC:USERS",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                var idClaim = principal.FindFirst("id")?.Value;
                var tokenVersionClaim = principal.FindFirst("token_version")?.Value;

                if (idClaim == null || tokenVersionClaim == null)
                    return null;

                var id = int.Parse(idClaim);
                var token_version = int.Parse(tokenVersionClaim);

                var conn = await _connection.GetOpenConnectionAsync();

                await using (var cmd = new NpgsqlCommand("SELECT * FROM users WHERE id = @id AND token_version = @token_version", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("token_version", token_version);

                    var reader = await cmd.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        // Nincs active mező ellenőrzése
                        return new UsersData
                        {
                            id = id,
                            token_version = token_version,
                            email = reader.GetString(reader.GetOrdinal("email")),
                            username = reader.GetString(reader.GetOrdinal("username"))
                        };
                    }
                    else
                    {
                        await conn.CloseAsync();
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
