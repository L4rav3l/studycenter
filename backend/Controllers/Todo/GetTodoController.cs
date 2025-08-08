using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    public class GetTodoController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public GetTodoController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpGet("api/todo/get")]
        public async Task<IActionResult> GetTodo([FromQuery] GetTodoRequest request)
        {
            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if(result == null)
            {
                return Unauthorized(new {error = 8});
            }

            var conn = await _connection.GetOpenConnectionAsync();
            var items = new List<Dictionary<string, object>>();

            await using(var data = new NpgsqlCommand("SELECT * FROM todo WHERE users_id = @users_id AND date = @date AND seen = true", conn))
            {
                data.Parameters.AddWithValue("users_id", result.id);
                data.Parameters.AddWithValue("date", request.Date);

                await using(var reader = await data.ExecuteReaderAsync())
                {

                    while(await reader.ReadAsync())
                    {

                    var item = new Dictionary<string, object>
                    {
                        ["ID"] = reader.GetInt32(reader.GetOrdinal("id")),
                        ["NAME"] = reader.GetString(reader.GetOrdinal("name")),
                        ["DATE"] = reader.GetDateTime(reader.GetOrdinal("date")),
                        ["STATUS"] = reader.GetBoolean(reader.GetOrdinal("status"))
                    };

                    items.Add(item);

                    }
                }
            }

            await conn.CloseAsync();
            return Ok(new {items});

        }
    }
}