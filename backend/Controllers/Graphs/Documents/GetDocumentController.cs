using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{

    [ApiController]
    public class GetDocumentController : ControllerBase
    {

        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public GetDocumentController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpGet("api/graphs/document/get")]
        public async Task<IActionResult> GetDocument([FromQuery] GetDocumentRequest request)
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
                    var titleOrdinal = reader.GetOrdinal("title");
                    var textOrdinal = reader.GetOrdinal("text");

                    var title = reader.IsDBNull(titleOrdinal) ? "" : reader.GetString(titleOrdinal);
                    var text = reader.IsDBNull(textOrdinal) ? "" : reader.GetString(textOrdinal);

                    await conn.CloseAsync();
                    return Ok(new { status = 1, title, text });

                } else {
                    await conn.CloseAsync();
                    return Unauthorized(new {error = 9});
                    }
                }
            }
        }
    }
}