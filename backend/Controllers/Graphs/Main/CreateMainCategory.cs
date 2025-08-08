using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    public class CreateMainCategory : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public CreateMainCategory(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/graphs/main/create")]
        public async Task<IActionResult> CreateMain([FromBody] CreateMainRequest request)
        {

            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);
        
            if(result == null)
            {
                return Unauthorized(new {error = 8});
            }

            var conn = await _connection.GetOpenConnectionAsync();

            await using(var check = new NpgsqlCommand("SELECT * FROM main_category WHERE users_id = @users_id AND name = @name AND seen = true", conn))
            {
                check.Parameters.AddWithValue("users_id", result.id);
                check.Parameters.AddWithValue("name", request.Name);

                await using(var reader = await check.ExecuteReaderAsync())
                {
                    if(await reader.ReadAsync())
                    {
                        await conn.CloseAsync();
                        return Unauthorized(new {error = 14});
                    } else {

                    }
                }
            }

            await using(var insert = new NpgsqlCommand("INSERT INTO main_category (users_id, name) VALUES (@users_id, @name)", conn))
            {
                insert.Parameters.AddWithValue("users_id", result.id);
                insert.Parameters.AddWithValue("name", request.Name);

                await insert.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new {status = 1});
        }
    }
}