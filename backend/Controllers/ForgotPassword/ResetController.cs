using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class ResetController : ControllerBase
    {

        private readonly Postgresql _connection;
        private readonly Mail _client;
        private readonly Authentication _authentication;

        public ResetController(Postgresql connection, Mail client, Authentication authentication)
        {
            _connection = connection;
            _client = client;
            _authentication = authentication;
        }

        [HttpPost("api/auth/reset-password")]
        public async Task<IActionResult> Reset([FromBody] ResetRequest request)
        {
            var user_data = await _authentication.VerifyToken(request.Token);

            if(user_data == null)
            {
                return BadRequest(new {error = 8});
            }

            var salt = Convert.ToBase64String(Argon2.GenerateSalt());
            var hash = Argon2.HashPassword(request.Password, salt);
            var conn = await _connection.GetOpenConnectionAsync();

            await using(var update = new NpgsqlCommand("UPDATE users SET password = @password, salt = @salt, token_version = token_version + 1 WHERE id = @id", conn))
            {
                update.Parameters.AddWithValue("password", hash);
                update.Parameters.AddWithValue("salt", salt);
                update.Parameters.AddWithValue("id", user_data.id);

                await update.ExecuteNonQueryAsync();

                var SmtpClient = _client.CreateSMTPClient();

                var message = new MimeMessage();

                message.From.Add(new MailboxAddress("Study Center", Environment.GetEnvironmentVariable("SMTP_USERNAME")));
                message.To.Add(new MailboxAddress("", user_data.email));
                message.Subject = "Password changed";
                message.Body = new TextPart("plain")
                {
                    Text = 
                    $"Hello, {user_data.username} \n" +
                    "Your password has changed. \n" +
                    "If you changed it, just ignore this email. If you didn't change it, change your password immediately."
                };

                SmtpClient.Send(message);
                SmtpClient.Disconnect(true);

                await conn.CloseAsync();
                return Ok(new {status = 1});
            }

        }
    }
}