using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers {
    
    [ApiController]
    public class RenameCategoryController : ControllerBase
    {

        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public RenameCategoryController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/graphs/category/rename")]
        public async Task<IActionResult> RenameCategory([FromBody] RenameCategoryRequest request)
        {

            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            var conn = await _connection.GetOpenConnectionAsync();

            await using(var check = new NpgsqlCommand("SELECT * FROM category WHERE users_id = @users_id AND id = @id", conn))
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

            await using(var update = new NpgsqlCommand("UPDATE category SET name = @name WHERE users_id = @users_id AND id = @id", conn))
            {
                update.Parameters.AddWithValue("name", request.Name);
                update.Parameters.AddWithValue("users_id", result.id);
                update.Parameters.AddWithValue("id", request.Id);

                await update.ExecuteNonQueryAsync();
            }
            
            await conn.CloseAsync();
            return Ok(new {status = 1});

        }



    }
}