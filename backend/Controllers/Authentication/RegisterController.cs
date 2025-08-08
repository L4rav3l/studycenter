using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Text.RegularExpressions;
using StudyCenter.Models;
using StudyCenter.System;
using DotNetEnv;
using Npgsql;
using MimeKit;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;
        private readonly Mail _client;

        public RegisterController(Postgresql connection, Authentication authentication, Mail client)
        {

            _connection = connection;
            _authentication = authentication;
            _client = client;
        }

        [HttpPost("/api/auth/register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    string pattern = @"^(?:[a-zA-Z0-9_'^&/+-])+(?:\.(?:[a-zA-Z0-9_'^&/+-])+)*@(?:[a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$";

    bool isValid = Regex.IsMatch(request.Email, pattern);

    if (!isValid)
        return Conflict(new { error = 4 });

    var conn = await _connection.GetOpenConnectionAsync();
    NpgsqlTransaction transaction = null;

    try
    {
        transaction = await conn.BeginTransactionAsync();

        await using (var username_check = new NpgsqlCommand("SELECT 1 FROM users WHERE username = @username", conn, transaction))
        {
            username_check.Parameters.AddWithValue("username", request.Username);
            var reader = await username_check.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                await reader.CloseAsync();
                await transaction.RollbackAsync();
                return Conflict(new { error = 5 });
            }

            await reader.CloseAsync();
        }

        await using (var email_check = new NpgsqlCommand("SELECT 1 FROM users WHERE email = @email", conn, transaction))
        {
            email_check.Parameters.AddWithValue("email", request.Email);
            var reader = await email_check.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                await reader.CloseAsync();
                await transaction.RollbackAsync();
                return Conflict(new { error = 6 });
            }

            await reader.CloseAsync();
        }

        string salt = Convert.ToBase64String(Argon2.GenerateSalt());
        var encrypted_password = Argon2.HashPassword(request.Password, salt);

        await using (var register = new NpgsqlCommand("INSERT INTO users (username, email, password, salt) VALUES (@username, @email, @password, @salt) RETURNING id", conn, transaction))
        {
            register.Parameters.AddWithValue("username", request.Username);
            register.Parameters.AddWithValue("email", request.Email);
            register.Parameters.AddWithValue("password", encrypted_password);
            register.Parameters.AddWithValue("salt", salt);

            var reader = await register.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var id = reader.GetInt32(reader.GetOrdinal("id"));
                await reader.CloseAsync();

                var token = _authentication.GenerateToken(id, 0);

                var SmtpClient = _client.CreateSMTPClient();

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Study Center", Environment.GetEnvironmentVariable("SMTP_USERNAME")));
                message.To.Add(new MailboxAddress("", request.Email));
                message.Subject = "Confirm registration";
                message.Body = new TextPart("plain")
                {
                    Text =
                        $"Hello, {request.Username} \n" +
                        $"Your confirmation link: {Environment.GetEnvironmentVariable("LINK")}/register/verify?token={token} \n" +
                        "This link is valid for 24 hours."
                };

                SmtpClient.Send(message);
                SmtpClient.Disconnect(true);
            }
        }

        await transaction.CommitAsync();
        return Ok(new { status = 1 });
    }
    catch(Exception ex)
    {   
        Console.WriteLine(ex);
        if (transaction != null)
        {
            await transaction.RollbackAsync();
        }

        await conn.CloseAsync();
        return StatusCode(500, new { error = 0 });
    }
    finally
    {
        await conn.CloseAsync();
            }
        }
    }
}