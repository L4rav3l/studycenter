using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System;
using System.Text;
using System.Text.RegularExpressions;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class ChangeEmailController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public ChangeEmailController(Postgresql connection, Authentication authentication, Mail client)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/settings/changemail")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
        {

            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if(result == null)
            {
                return Unauthorized(new {error = 8});
            }

            string pattern = @"^(?:[a-zA-Z0-9_'^&/+-])+(?:\.(?:[a-zA-Z0-9_'^&/+-])+)*@(?:[a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$";
            bool isValid = Regex.IsMatch(request.Email, pattern);

            if(isValid == false)
            {
                return Conflict(new {error = 4});
            }

            var conn = await _connection.GetOpenConnectionAsync();

            await using(var check = new NpgsqlCommand("SELECT * FROM users WHERE email = @email", conn))
            {
                check.Parameters.AddWithValue("email", request.Email);

                await using(var reader = await check.ExecuteReaderAsync())
                {
                    if(await reader.ReadAsync())
                    {
                        await conn.CloseAsync();
                        return Conflict(new {error = 6});
                    } else {

                    }
                }
            }

            await using(var update = new NpgsqlCommand("UPDATE users SET email = @email WHERE id = @id", conn))
            {
                update.Parameters.AddWithValue("email", request.Email);
                update.Parameters.AddWithValue("id", result.id);

                await update.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new {status = 1});
        }
    }
}