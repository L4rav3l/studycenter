using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class StatusTodoController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public StatusTodoController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/todo/status")]
        public async Task<IActionResult> StatusTodo([FromBody] StatusTodoRequest request)
        {

        string token = Request.GetBearerToken();
        UsersData result = await _authentication.VerifyToken(token);

        if(result == null)
        {
            return Unauthorized(new {error = 8});
        }

        var conn = await _connection.GetOpenConnectionAsync();

        await using(var check = new NpgsqlCommand("SELECT * FROM todo WHERE users_id = @users_id AND id = @id AND seen = true", conn))
        {
            check.Parameters.AddWithValue("users_id", result.id);
            check.Parameters.AddWithValue("id", request.Id);

            await using(var reader = await check.ExecuteReaderAsync())
            {

                if(await reader.ReadAsync())
                {

                } else {
                        return Unauthorized(new {error = 9});
                    }
                }
            }

            await using(var update = new NpgsqlCommand("UPDATE todo SET status = @status WHERE id = @id AND users_id = @users_id AND seen = true", conn))
            {
                update.Parameters.AddWithValue("status", request.Status);
                update.Parameters.AddWithValue("id", request.Id);
                update.Parameters.AddWithValue("users_id", result.id);

                await update.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new {status = 1});
        }
    }
}