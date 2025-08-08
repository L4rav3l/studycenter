using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using DotNetEnv;
using StudyCenter.System;
using StudyCenter.Models;
using Npgsql;

namespace StudyCenter.System {

    public class Authentication {

        private readonly Postgresql _connection;

        public Authentication(Postgresql connection) 
        {
            _connection = connection;
        }

        public string GenerateToken(int id, int token_version) {
            Env.Load();

            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")));
            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            var claims = new []
            {
                new Claim("id", id.ToString()),
                new Claim("token_version", token_version.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "SC:API",
                audience: "SC:USERS",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<UsersData> VerifyToken(string token) {
            Env.Load();

            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")));
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securitykey,
                ValidateIssuer = true,
                ValidIssuer = "SC:API",
                ValidateAudience = true,
                ValidAudience = "SC:USERS",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out SecurityToken validatedtoken);

            var id = int.Parse(principal.FindFirst("id").Value);
            var token_version = int.Parse(principal.FindFirst("token_version").Value);

            var conn = await _connection.GetOpenConnectionAsync();

            await using(var usersdata = new NpgsqlCommand("SELECT * FROM users WHERE id = @id AND token_version = @token_version", conn))
            {
                usersdata.Parameters.AddWithValue("id", id);
                usersdata.Parameters.AddWithValue("token_version", token_version);

                var reader = await usersdata.ExecuteReaderAsync();

                if(await reader.ReadAsync())
                {  
                    if(reader.GetInt32(reader.GetOrdinal("token_version")) == token_version && reader.GetBoolean(reader.GetOrdinal("active")) == true) {

                    return new UsersData{
                        id = id,
                        token_version = token_version,
                        email = reader.GetString(reader.GetOrdinal("email")),
                        username = reader.GetString(reader.GetOrdinal("username"))
                        };
                    } else {
                        await conn.CloseAsync();
                        return null;
                    }


                } else {
                    await conn.CloseAsync();
                    return null;
                    }
                }
            }
            catch {
                return null;
            }
        }
    }

}