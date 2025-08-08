using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    public class DeleteMainCategory : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public DeleteMainCategory(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/graphs/main/delete")]
        public async Task<IActionResult> DeleteMain([FromBody] DeleteMainRequest request)
        {

            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);
        
            if(result == null)
            {
                return Unauthorized(new {error = 8});
            }

            var conn = await _connection.GetOpenConnectionAsync();

            await using(var check = new NpgsqlCommand("SELECT * FROM main_category WHERE users_id = @users_id AND id = @id AND seen = true", conn))
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

            await using(var delete_1 = new NpgsqlCommand("UPDATE documents SET seen = false WHERE main_parent = @main_parent AND users_id = @users_id", conn))
            {
                delete_1.Parameters.AddWithValue("main_parent", request.Id);
                delete_1.Parameters.AddWithValue("users_id", result.id);

                await delete_1.ExecuteNonQueryAsync();
            }

            await using(var delete_2 = new NpgsqlCommand("UPDATE category SET seen = false WHERE main_parent = @main_parent AND users_id = @users_id", conn))
            {
                delete_2.Parameters.AddWithValue("main_parent", request.Id);
                delete_2.Parameters.AddWithValue("users_id", result.id);

                await delete_2.ExecuteNonQueryAsync();
            }

            await using(var delete_3 = new NpgsqlCommand("UPDATE main_category SET seen = false WHERE id = @id AND users_id = @users_id", conn))
            {
                delete_3.Parameters.AddWithValue("id", request.Id);
                delete_3.Parameters.AddWithValue("users_id", result.id);

                await delete_3.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new {status = 1});
        }
    }
}