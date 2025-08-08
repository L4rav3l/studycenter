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
    public class ChangePasswordController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public ChangePasswordController(Postgresql connection, Authentication authentication, Mail client)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/settings/changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {

            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if(result == null)
            {
                return Unauthorized(new {error = 8});
            }

            var conn = await _connection.GetOpenConnectionAsync();
            var salt = Convert.ToBase64String(Argon2.GenerateSalt());
            var encrypted_password = Argon2.HashPassword(request.Password, salt);

            await using(var update = new NpgsqlCommand("UPDATE users SET password = @password, salt = @salt, token_version = token_version + 1 WHERE id = @id", conn))
            {
                update.Parameters.AddWithValue("password", encrypted_password);
                update.Parameters.AddWithValue("salt", salt);
                update.Parameters.AddWithValue("id", result.id);

                await update.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new {status = 1});
        }
    }
}