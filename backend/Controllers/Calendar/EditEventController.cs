using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class EditEventController : ControllerBase
    {

        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public EditEventController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/calendar/event/edit")]
        public async Task<IActionResult> EditEvent([FromBody] EditEventRequest request)
        {

            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if(result == null)
            {
                return Unauthorized(new {error = 8});
            }

            var conn = await _connection.GetOpenConnectionAsync();

            await using(var check = new NpgsqlCommand("SELECT * FROM calendar WHERE users_id = @users_id AND id = @id AND seen = true", conn))
            {

                check.Parameters.AddWithValue("users_id", result.id);
                check.Parameters.AddWithValue("id", request.Id);

                await using(var reader = await check.ExecuteReaderAsync())
                {
                    if(await reader.ReadAsync())
                    {

                    } else {
                        await conn.CloseAsync();
                        return Unauthorized(new {error = 9});
                    }
                }

            }

            await using(var edit = new NpgsqlCommand("UPDATE calendar SET date = @date, notification_date = @notification_date, title = @title, descriptions = @descriptions WHERE users_id = @users_id AND id = @id", conn))
            {

                edit.Parameters.AddWithValue("users_id", result.id);
                edit.Parameters.AddWithValue("id", request.Id);
                edit.Parameters.AddWithValue("date", request.Date);
                edit.Parameters.AddWithValue("notification_date", request.Notification_date);
                edit.Parameters.AddWithValue("title", request.Title);
                edit.Parameters.AddWithValue("descriptions", (object)request.Descriptions ?? DBNull.Value);

                var reader = await edit.ExecuteNonQueryAsync();

                await conn.CloseAsync();
                return Ok(new {status = 1});
            }
        }
    }
}