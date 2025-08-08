using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class SetParentCategoryController : ControllerBase
    {

        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public SetParentCategoryController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/graphs/category/setparent")]
        public async Task<IActionResult> SetParentCategory([FromBody] SetParentDocumentRequest request)
        {

            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if(result == null) {
                return Unauthorized(new {error = 8});
            }

            var conn = await _connection.GetOpenConnectionAsync();

            await using(var check = new NpgsqlCommand("SELECT * FROM category WHERE id = @id AND users_id = @users_id AND seen = true", conn))
            {
                check.Parameters.AddWithValue("id", request.Id);
                check.Parameters.AddWithValue("users_id", result.id);

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

            if(request.Parent != null)
            {
            await using(var check_2 = new NpgsqlCommand("SELECT * FROM category WHERE id = @id AND users_id = @users_id AND seen = true", conn))
            {
                check_2.Parameters.AddWithValue("id", request.Parent);
                check_2.Parameters.AddWithValue("users_id", result.id);

                await using(var reader = await check_2.ExecuteReaderAsync())
                {

                if(await reader.ReadAsync())
                {

                } else {
                    await conn.CloseAsync();
                    return Unauthorized(new {error = 9});
                        }
                    }
                }
            }

            await using(var update = new NpgsqlCommand("UPDATE category SET parent = @parent WHERE id = @id AND users_id = @users_id", conn))
            {
                update.Parameters.AddWithValue("parent", (object?)request.Parent ?? DBNull.Value);
                update.Parameters.AddWithValue("id", request.Id);
                update.Parameters.AddWithValue("users_id", result.id);

                await update.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new {status = 1});
        }

    }
}