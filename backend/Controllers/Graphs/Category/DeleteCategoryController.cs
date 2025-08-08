using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.System;
using StudyCenter.Models;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class DeleteCategoryController : ControllerBase 
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public DeleteCategoryController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/graphs/category/delete")]
        public async Task<IActionResult> DeleteCategory([FromBody] DeleteCategoryRequest request)
        {
            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);
            if (result == null)
                return Unauthorized(new { error = 8 });

            var conn = await _connection.GetOpenConnectionAsync();

            await using (var check = new NpgsqlCommand("SELECT * FROM category WHERE id = @id AND users_id = @users_id AND seen = true", conn))
            {
                check.Parameters.AddWithValue("id", request.Id);
                check.Parameters.AddWithValue("users_id", result.id);

                await using(var exists = await check.ExecuteReaderAsync())
                {
                if (await exists.ReadAsync())
                    {

                    } else {
                
                        await conn.CloseAsync();
                        return Unauthorized(new { error = 9 });
                    
                    }
                                
                }

            }

            await using (var deletedocument = new NpgsqlCommand("WITH RECURSIVE descendants AS (SELECT id FROM category WHERE id = @id AND users_id = @users_id UNION ALL SELECT c.id FROM category c INNER JOIN descendants d ON c.parent = d.id WHERE c.users_id = @users_id ) UPDATE documents SET seen = false WHERE parent IN (SELECT id FROM descendants) AND users_id = @users_id", conn))
            {
                deletedocument.Parameters.AddWithValue("id", request.Id);
                deletedocument.Parameters.AddWithValue("users_id", result.id);

                await deletedocument.ExecuteNonQueryAsync();
            }

            await using (var deletecategory = new NpgsqlCommand("WITH RECURSIVE descendants AS (SELECT id FROM category WHERE id = @id AND users_id = @users_id UNION ALL SELECT c.id FROM category c INNER JOIN descendants d ON c.parent = d.id WHERE c.users_id = @users_id ) UPDATE category SET seen = false WHERE id IN (SELECT id FROM descendants)", conn))
            {
                deletecategory.Parameters.AddWithValue("id", request.Id);
                deletecategory.Parameters.AddWithValue("users_id", result.id);

                await deletecategory.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new { status = 1 });
        }
    }
}
