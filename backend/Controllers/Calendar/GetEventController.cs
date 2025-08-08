using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class GetEventController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public GetEventController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpGet("api/calendar/get")]
        public async Task<IActionResult> GetEvent([FromQuery] GetEventRequest request)
        {

            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            DateTime From = request.From;
            DateTime To = From.AddMonths(1);

            var conn = await _connection.GetOpenConnectionAsync();
            var items = new List<Dictionary<string, object>>();

            await using(var calendarlist = new NpgsqlCommand("SELECT * FROM calendar WHERE date BETWEEN @from AND @to AND users_id = @users_id AND seen = true", conn))
            {
                calendarlist.Parameters.AddWithValue("from", From);
                calendarlist.Parameters.AddWithValue("to", To);
                calendarlist.Parameters.AddWithValue("users_id", result.id);

                var reader = await calendarlist.ExecuteReaderAsync();

                while(await reader.ReadAsync())
                {
                    var item = new Dictionary<string, object?>
                    {
                        ["ID"] = reader.GetInt32(reader.GetOrdinal("id")),
                        ["TITLE"] = reader.GetString(reader.GetOrdinal("title")),
                        ["descriptions"] = reader.IsDBNull(reader.GetOrdinal("descriptions")) ? null : reader.GetString(reader.GetOrdinal("descriptions")),
                        ["date"] = reader.GetDateTime(reader.GetOrdinal("date")),
                    };

                    items.Add(item);
                }
            }

            await conn.CloseAsync();
            return Ok(items);

        }
    }
}