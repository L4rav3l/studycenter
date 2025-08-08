using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class DeleteTodoController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public DeleteTodoController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/todo/delete")]
        public async Task<IActionResult> DeleteTodo([FromBody] DeleteTodoRequest request)
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
                        await conn.CloseAsync();
                        return Unauthorized(new {error = 9});
                    }
                }
            }

            await using(var delete = new NpgsqlCommand("UPDATE todo SET seen = false WHERE users_id = @users_id AND id = @id", conn))
            {
                delete.Parameters.AddWithValue("users_id", result.id);
                delete.Parameters.AddWithValue("id", request.Id);

                await delete.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new {status = 1});
        }
    }
}