using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class EditFlashCardController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public EditFlashCardController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/wordstudy/flashcard/edit")]
        public async Task<IActionResult> EditFlashCard([FromBody] EditFlashCardRequest request)
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

            await using(var update = new NpgsqlCommand("UPDATE wordstudy_flashcard SET front = @front, back = @back WHERE users_id = @users_id AND id = @id AND seen = true", conn))
            {
                update.Parameters.AddWithValue("front", request.Front);
                update.Parameters.AddWithValue("back", request.Back);
                update.Parameters.AddWithValue("users_id", result.id);
                update.Parameters.AddWithValue("id", request.Id);

                var reader = await update.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            return Ok(new {status = 1});
        }
    }
}