using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class DeleteFlashCardController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public DeleteFlashCardController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/wordstudy/flashcard/delete")]
        public async Task<IActionResult> DeleteFlashCard([FromBody] DeleteFlashCardRequest request)
        {

            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if(result == null)
            {
                return Unauthorized(new {error = 8});
            }

            var conn = await _connection.GetOpenConnectionAsync();

            await using(var check = new NpgsqlCommand("SELECT * FROM wordstudy_flashcard WHERE users_id = @users_id AND id = @id AND seen = TRUE", conn))
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

            await using(var delete = new NpgsqlCommand("UPDATE wordstudy_flashcard SET seen = FALSE WHERE users_id = @users_id AND id = @id", conn))
            {
                delete.Parameters.AddWithValue("users_id", result.id);
                delete.Parameters.AddWithValue("id", request.Id);

                await delete.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new {status = 1});
        }
    }
}