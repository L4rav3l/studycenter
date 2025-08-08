using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class CreateFlashCardController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public CreateFlashCardController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/wordstudy/flashcard/create")]
        public async Task<IActionResult> CreateFlashCard([FromBody] CreateFlashCardRequest request)
        {
            
            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if(result == null)
            {
                return Unauthorized(new {error = 8});
            }

            var conn = await _connection.GetOpenConnectionAsync();

            if(request.Sett_id != null)
            {
                await using(var check = new NpgsqlCommand("SELECT * FROM wordstudy_sett WHERE id = @id AND users_id = @users_id AND seen = true", conn))
                {
                    check.Parameters.AddWithValue("id", request.Sett_id);
                    check.Parameters.AddWithValue("users_id", result.id);

                    await using(var reader = await check.ExecuteReaderAsync())
                    {
                        if(await reader.ReadAsync())
                        {

                        } else {
                            await conn.CloseAsync();
                            return NotFound(new {error = 12});
                        }
                    }
                }
            }

                await using(var insert = new NpgsqlCommand("INSERT INTO wordstudy_flashcard (users_id, sett_id, front, back) VALUES (@users_id, @sett_id, @front, @back)", conn))
                {
                    insert.Parameters.AddWithValue("users_id", result.id);
                    insert.Parameters.AddWithValue("sett_id", request.Sett_id);
                    insert.Parameters.AddWithValue("front", request.Front);
                    insert.Parameters.AddWithValue("back", request.Back);

                    await insert.ExecuteNonQueryAsync();
                }
            await conn.CloseAsync();
            return Ok(new {status = 1});

        }
    }
}