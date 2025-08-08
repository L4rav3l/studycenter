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
    public class ChangeUsernameController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public ChangeUsernameController(Postgresql connection, Authentication authentication, Mail client)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/settings/changeusername")]
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameRequest request)
        {

            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if(result == null)
            {
                return Unauthorized(new {error = 8});
            }

            var conn = await _connection.GetOpenConnectionAsync();

            await using(var check = new NpgsqlCommand("SELECT * FROM users WHERE username = @username", conn))
            {
                check.Parameters.AddWithValue("username", request.Username);

                await using(var reader = await check.ExecuteReaderAsync())
                {
                    if(await reader.ReadAsync())
                    {   
                        await conn.CloseAsync();
                        return Conflict(new {error = 5});
                    } else {

                    }
                }
            }

            await using(var update = new NpgsqlCommand("UPDATE users SET username = @username WHERE id = @id", conn))
            {
                update.Parameters.AddWithValue("username", request.Username);
                update.Parameters.AddWithValue("id", result.id);

                await update.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new {status = 1});
        }
    }
}