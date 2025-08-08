using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.System;
using StudyCenter.Models;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class CreateFolderController : ControllerBase
    {

        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public CreateFolderController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/wordstudy/folder/create")]
        public async Task<IActionResult> CreateFolder([FromBody] CreateFolderRequest request)
        {
            
            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if(result == null)
            {
                return Unauthorized(new {error = 8});
            }

            var conn = await _connection.GetOpenConnectionAsync();

            await using(var check = new NpgsqlCommand("SELECT * FROM wordstudy_folder WHERE name = @name AND users_id = @users_id", conn))
            {
                check.Parameters.AddWithValue("name", request.Name);
                check.Parameters.AddWithValue("users_id", result.id);

                await using(var reader = await check.ExecuteReaderAsync())
                {
                    if(await reader.ReadAsync())
                    {
                        await conn.CloseAsync();
                        return Conflict(new {error = 10});
                    } else {
                        
                    }
                }
            }

            await using(var insert = new NpgsqlCommand("INSERT INTO wordstudy_folder (users_id, name, notification_date, end_date) VALUES (@users_id, @name, @notification_date, @end_date)", conn))
            {
                insert.Parameters.AddWithValue("name", request.Name);
                insert.Parameters.AddWithValue("users_id", result.id);
                insert.Parameters.AddWithValue("notification_date", (object?)request.Notification_date ?? DBNull.Value);
                insert.Parameters.AddWithValue("end_date", (object?)request.End_date ?? DBNull.Value);

                var reader = await insert.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new {status = 1});

        }

    }
}