using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class CreeateSettController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public CreeateSettController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/wordstudy/sett/create")]
        public async Task<IActionResult> CreateSett([FromBody] CreateSettRequest request)
        {

            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if(result == null)
            {
                return Unauthorized(new {error = 8});
            }

            var conn = await _connection.GetOpenConnectionAsync();

            if(request.Folder != null)
            {

            await using(var check_1 = new NpgsqlCommand("SELECT * FROM wordstudy_folder WHERE id = @id AND users_id = @users_id AND seen = true", conn))
            {
                check_1.Parameters.AddWithValue("id", (object?)request.Folder ?? DBNull.Value);
                check_1.Parameters.AddWithValue("users_id", result.id);

                await using(var reader = await check_1.ExecuteReaderAsync())
                {
                    if(await reader.ReadAsync())
                    {

                    } else {
                        await conn.CloseAsync();
                        return Unauthorized(new {error = 9});
                    }
                }
                }
            }

            await using(var check_2 = new NpgsqlCommand("SELECT * FROM wordstudy_sett WHERE folder_id = @folder_id AND name = @name AND users_id = @users_id AND seen = true", conn))
            {
                check_2.Parameters.AddWithValue("folder_id", (object?)request.Folder ?? DBNull.Value);
                check_2.Parameters.AddWithValue("name", request.Name);
                check_2.Parameters.AddWithValue("users_id", result.id);

                await using(var reader = await check_2.ExecuteReaderAsync())
                {

                if(await reader.ReadAsync())
                {   
                    await conn.CloseAsync();
                    return Conflict(new {error = 11});
                } else {

                    }
                }
            }

            await using(var insert = new NpgsqlCommand("INSERT INTO wordstudy_sett (users_id, folder_id, name, notification_date, end_date) VALUES (@users_id, @folder_id, @name, @notification_date, @end_date)", conn))
            {
                insert.Parameters.AddWithValue("users_id", result.id);
                insert.Parameters.AddWithValue("folder_id", (object?)request.Folder ?? DBNull.Value);
                insert.Parameters.AddWithValue("name", request.Name);
                insert.Parameters.AddWithValue("notification_date", (object?)request.Notification_date ?? DBNull.Value);
                insert.Parameters.AddWithValue("end_date", (object?)request.End_date ?? DBNull.Value);

                await insert.ExecuteNonQueryAsync();

            }

            await conn.CloseAsync();
            return Ok(new {status = 1});

        }


    }
}