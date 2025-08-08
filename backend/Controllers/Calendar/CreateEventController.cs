using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class CreateEventController : ControllerBase
    {

        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public CreateEventController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/calendar/event/create")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
        {

            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if(result == null)
            {
                return Unauthorized(new {error = 8});
            }

            var conn = await _connection.GetOpenConnectionAsync();
            var transaction = await conn.BeginTransactionAsync();

            try {
            await using(var insert = new NpgsqlCommand("INSERT INTO calendar (users_id, notification_date, date, title, descriptions) VALUES (@users_id, @notification_date, @date, @title, @descriptions)", conn, transaction))
            {

                insert.Parameters.AddWithValue("users_id", result.id);
                insert.Parameters.AddWithValue("notification_date", (object?)request.Notification_date ?? DBNull.Value);
                insert.Parameters.AddWithValue("date", request.Date);
                insert.Parameters.AddWithValue("title", request.Title);
                insert.Parameters.AddWithValue("descriptions", (object)request.Descriptions ?? DBNull.Value);

                var reader = await insert.ExecuteNonQueryAsync();
                }
                
                await transaction.CommitAsync();
                await conn.CloseAsync();
                return Ok(new {status = 1});
            }

            catch(Exception ex) {
                Console.WriteLine(ex);
                await transaction.RollbackAsync();
                await conn.CloseAsync();
                return StatusCode(500, new {error = 0});
            }
        }
    }
}