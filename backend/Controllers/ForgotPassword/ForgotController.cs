using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Npgsql;
using System;
using System.Text;
using System.Text.RegularExpressions;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class ForgotController : ControllerBase
    {

        private readonly Postgresql _connection;
        private readonly Authentication _authentication;
        private readonly Mail _client;

        public ForgotController(Postgresql connection, Authentication authentication, Mail client)
        {
            Env.Load();

            _authentication = authentication;
            _connection = connection;
            _client = client;
        }

        [HttpPost("api/auth/forgot-password")]
        public async Task<IActionResult> Forgot([FromBody] ForgotRequest request)
        {

        string pattern = @"^(?:[a-zA-Z0-9_'^&/+-])+(?:\.(?:[a-zA-Z0-9_'^&/+-])+)*@(?:[a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$";

        bool isValid = Regex.IsMatch(request.Email, pattern);

        if(isValid)
        {

            var conn = await _connection.GetOpenConnectionAsync();
            
            await using(var user_data = new NpgsqlCommand("SELECT * FROM users WHERE email = @email", conn))
            {
                user_data.Parameters.AddWithValue("email", request.Email);

                var reader = await user_data.ExecuteReaderAsync();

                if(await reader.ReadAsync())
                {
                    
                    var token = _authentication.GenerateToken(reader.GetInt32(reader.GetOrdinal("id")), reader.GetInt32(reader.GetOrdinal("token_version")));
                    
                    var SmtpClient = _client.CreateSMTPClient();

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Study Center", Environment.GetEnvironmentVariable("SMTP_USERNAME")));
                    message.To.Add(new MailboxAddress("", request.Email));
                    message.Subject = "Password Reset";
                    message.Body = new TextPart("plain")
                    {
                        Text = 
                        $"Hello, {reader.GetString(reader.GetOrdinal("username"))} \n" +
                        $"Your password reset link: {Environment.GetEnvironmentVariable("LINK")}/reset_password?token={token} \n" +
                        $"This link is valid for 24 hours"
                    };

                    SmtpClient.Send(message);
                    SmtpClient.Disconnect(true);

                    await conn.CloseAsync();
                    return Ok(new {status = 1});

                } else {
                    await conn.CloseAsync();
                    return NotFound(new {error = 1});
                }
            }

        } else {
            return Conflict(new {error = 4});
        }
            
        }
    }
}