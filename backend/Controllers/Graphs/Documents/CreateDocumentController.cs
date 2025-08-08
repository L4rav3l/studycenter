using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class CreateDocumentController : ControllerBase
    {

        private readonly Authentication _authentication;
        private readonly Postgresql _connection;

        public CreateDocumentController(Authentication authentication, Postgresql connection)
        {
            _authentication = authentication;
            _connection = connection;
        }

        [HttpPost("api/graphs/document/create")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateDocumentsRequest request)
        {
                                                                                            
            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if(result == null)
            {
                return Unauthorized(new { error = 8 });
            }   

            var conn = await _connection.GetOpenConnectionAsync();

            await using(var check = new NpgsqlCommand("SELECT * FROM main_category WHERE id = @id and users_id = @users_id and seen = true", conn))
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

            await using(var check_3 = new NpgsqlCommand("SELECT * FROM documents WHERE main_parent = @main_parent AND (parent = @parent OR (parent IS NULL AND @parent IS NULL)) AND users_id = @users_id AND title =@title AND seen = true", conn))
            {
                check_3.Parameters.AddWithValue("main_parent", request.Main_parent);
                check_3.Parameters.AddWithValue("parent", (object?)request.Parent ?? DBNull.Value);
                check_3.Parameters.AddWithValue("users_id", result.id);
                check_3.Parameters.AddWithValue("title", request.Title);

                await using(var reader = await check_3.ExecuteReaderAsync())
                {
                    if(await reader.ReadAsync())
                    {
                        await conn.CloseAsync();
                        return Conflict(new {error = 15});
                    } else {

                    }
                }
            }

            await using(var insert = new NpgsqlCommand("INSERT INTO documents (users_id, parent, main_parent, title, text) VALUES (@users_id, @parent, @main_parent, @title, null)", conn))
            {
                insert.Parameters.AddWithValue("users_id", result.id);
                insert.Parameters.AddWithValue("parent", (object?)request.Parent ?? DBNull.Value);
                insert.Parameters.AddWithValue("main_parent", request.Main_parent);
                insert.Parameters.AddWithValue("title", request.Title);

                await insert.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new {status = 1});


        }
    }
}

