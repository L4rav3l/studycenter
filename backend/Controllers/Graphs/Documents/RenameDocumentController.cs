using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers {
    
    [ApiController]
    public class RenameDocumentController : ControllerBase
    {

        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public RenameDocumentController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/graphs/document/rename")]
        public async Task<IActionResult> RenameDocument([FromBody] RenameDocumentRequest request)
        {

            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            var conn = await _connection.GetOpenConnectionAsync();

            await using(var check = new NpgsqlCommand("SELECT * FROM documents WHERE users_id = @users_id AND id = @id AND seen = true", conn))
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

            await using(var update = new NpgsqlCommand("UPDATE documents SET title = @title WHERE users_id = @users_id AND id = @id", conn))
            {
                update.Parameters.AddWithValue("title", request.Title);
                update.Parameters.AddWithValue("users_id", result.id);
                update.Parameters.AddWithValue("id", request.Id);

                await update.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new {status = 1});

        }



    }
}