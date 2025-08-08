using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class EditDocumentController : ControllerBase
    {

        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public EditDocumentController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/graphs/document/edit")]
        public async Task<IActionResult> EditDocument([FromBody] EditDocumentRequest request)
        {
            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if(result == null)
            {
                return Unauthorized(new {error = 8});
            }

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

            await using(var update = new NpgsqlCommand("UPDATE documents SET title = @title, text = @text WHERE users_id = @users_id AND id = @id", conn))
            {
                update.Parameters.AddWithValue("users_id", result.id);
                update.Parameters.AddWithValue("id", request.Id);
                update.Parameters.AddWithValue("title", request.Title);
                update.Parameters.AddWithValue("text", request.Text);

                await update.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new {status = 1});

        }

    }
}