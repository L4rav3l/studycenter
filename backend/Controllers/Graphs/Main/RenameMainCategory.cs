using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    public class RenameMainCategory : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public RenameMainCategory(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/graphs/main/rename")]
        public async Task<IActionResult> RenameMain([FromBody] RenameMainRequest request)
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
                        return Conflict(new {error = 14});
                    } else {

                    }
                }
            }

            await using(var update = new NpgsqlCommand("UPDATE main_category SET name = @name WHERE users_id = @users_id AND id = @id AND seen = true", conn))
            {
                update.Parameters.AddWithValue("users_id", result.id);
                update.Parameters.AddWithValue("id", request.Id);
                update.Parameters.AddWithValue("name", request.Name);

                await update.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new {status = 1});
        }
    }
}