using Microsoft.AspNetCore.Mvc;
using StudyCenter.Models;
using StudyCenter.System;
using MimeKit;
using DotNetEnv;
using Npgsql;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;
        private readonly Mail _client;

        public LoginController(Postgresql connection, Authentication authentication, Mail client)
        {
            _connection = connection;
            _authentication = authentication;
            _client = client;
        }

        [HttpPost("/api/auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var conn = await _connection.GetOpenConnectionAsync();

            try
            {
                await using var user_data = new NpgsqlCommand("SELECT * FROM users WHERE username = @username", conn);
                user_data.Parameters.AddWithValue("username", request.Username);

                var reader = await user_data.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var email = reader.GetString(reader.GetOrdinal("email"));
                    var username = reader.GetString(reader.GetOrdinal("username"));
                    var password = reader.GetString(reader.GetOrdinal("password"));
                    var salt = reader.GetString(reader.GetOrdinal("salt"));
                    var id = reader.GetInt32(reader.GetOrdinal("id"));
                    var token_version = reader.GetInt32(reader.GetOrdinal("token_version"));
                    var active = reader.GetBoolean(reader.GetOrdinal("active"));

                    var encrypted_password = Argon2.HashPassword(request.Password, salt);

                    if (password == encrypted_password)
                    {
                        if (active)
                        {
                            var token = _authentication.GenerateToken(id, token_version);
                            await conn.CloseAsync();
                            return Ok(new { status = 1, token });
                        }
                        else
                        {
                            var token = _authentication.GenerateToken(id, token_version);

                            try
                            {
                                using var smtpClient = _client.CreateSMTPClient();
                                var message = new MimeMessage();
                                message.From.Add(new MailboxAddress("Study Center", Environment.GetEnvironmentVariable("SMTP_USERNAME")));
                                message.To.Add(new MailboxAddress(username, email));
                                message.Subject = "Confirm registration";
                                message.Body = new TextPart("plain")
                                {
                                    Text = $"Hello, {username} \nYour confirmation link: {Environment.GetEnvironmentVariable("LINK")}/register/verify?token={token} \nThis link is valid for 24 hours."
                                };

                                smtpClient.Send(message);
                                smtpClient.Disconnect(true);
                            }
                            catch
                            {
                            }

                            await conn.CloseAsync();
                            return Forbid();
                        }
                    }
                    else
                    {
                        await conn.CloseAsync();
                        return Unauthorized(new { error = 2 });
                    }
                }
                else
                {
                    await conn.CloseAsync();
                    return NotFound(new { error = 1 });
                }
            }
            catch
            {
                await conn.CloseAsync();
                return StatusCode(500, new { error = 0 });
            }
        }
    }
}
