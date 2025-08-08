using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class CreateTodoController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;
    
    public CreateTodoController(Postgresql connection, Authentication authentication)
    {
        _connection = connection;
        _authentication = authentication;
    }

    [HttpPost("api/todo/create")]
    public async Task<IActionResult> CreateTodo([FromBody] CreateTodoRequest request)
    {
        
        string token = Request.GetBearerToken();
        UsersData result = await _authentication.VerifyToken(token);

        if(result == null)
        {
            return Unauthorized(new {error = 8});
        }

        var conn = await _connection.GetOpenConnectionAsync();

        await using(var insert = new NpgsqlCommand("INSERT INTO todo (users_id, date, name) VALUES (@users_id, @date, @name)", conn))
        {
            insert.Parameters.AddWithValue("users_id", result.id);
            insert.Parameters.AddWithValue("date", request.Date);
            insert.Parameters.AddWithValue("name", request.Name);

            await insert.ExecuteNonQueryAsync();
        }

        await conn.CloseAsync();
        return Ok(new {status = 1});
        }
    }
}