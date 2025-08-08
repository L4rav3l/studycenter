using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class CreateCategoryController : ControllerBase
    {

        private readonly Authentication _authentication;
        private readonly Postgresql _connection;

        public CreateCategoryController(Authentication authentication, Postgresql connection)
        {
            _authentication = authentication;
            _connection = connection;
        }

        [HttpPost("api/graphs/category/create")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
                                                                                            
            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if(result == null)
            {
                return Unauthorized(new { error = 8 });
            }   

            var conn = await _connection.GetOpenConnectionAsync();

            await using(var check = new NpgsqlCommand("SELECT * FROM main_category WHERE id = @id and users_id = @users_id AND seen = true", conn))
            {
                check.Parameters.AddWithValue("id", request.Main_parent);
                check.Parameters.AddWithValue("users_id", result.id);

                await using (var reader = await check.ExecuteReaderAsync())
                {

                if(await reader.ReadAsync()) {

                } else {
                    await conn.CloseAsync();
                    return Unauthorized(new {error = 9});
                    }
                }
            }

            if(request.Parent != null) {
            await using(var check_2 = new NpgsqlCommand("SELECT * FROM category WHERE id = @id and users_id = @users_id AND seen = true", conn))
            {
                check_2.Parameters.AddWithValue("id", request.Parent);
                check_2.Parameters.AddWithValue("users_id", result.id);

                await using (var reader = await check_2.ExecuteReaderAsync())
                {

                if(await reader.ReadAsync()) {

                } else {
                    await conn.CloseAsync();
                    return Unauthorized(new {error = 9});
                        }
                    }
                }
            }

            await using(var insert = new NpgsqlCommand("INSERT INTO category (users_id, parent, main_parent, name) VALUES (@users_id, @parent, @main_parent, @name)", conn))
            {
                insert.Parameters.AddWithValue("users_id", result.id);
                insert.Parameters.AddWithValue("parent", (object?)request.Parent ?? DBNull.Value);
                insert.Parameters.AddWithValue("main_parent", request.Main_parent);
                insert.Parameters.AddWithValue("name", request.Name);

                await insert.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new {status = 1});


        }
    }
}

