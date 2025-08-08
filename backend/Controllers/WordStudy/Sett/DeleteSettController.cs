using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class DeleteSettController : ControllerBase
    {

        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public DeleteSettController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/wordstudy/sett/delete")]
        public async Task<IActionResult> DeleteSett([FromBody] DeleteSettRequest request)
        {
            
            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if(result == null)
            {
                return Unauthorized(new {error = 8});
            }

            var conn = await _connection.GetOpenConnectionAsync();
            var transaction = await conn.BeginTransactionAsync();

            try 
            {
            await using(var check = new NpgsqlCommand("SELECT * FROM wordstudy_sett WHERE users_id = @users_id AND id = @id AND seen = true", conn, transaction))
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

            await using(var update = new NpgsqlCommand("UPDATE wordstudy_sett SET seen = false WHERE users_id = @users_id AND id = @id", conn, transaction))
            {
                update.Parameters.AddWithValue("users_id", result.id);
                update.Parameters.AddWithValue("id", request.Id);

                await update.ExecuteNonQueryAsync();
                }

            await using(var update_2 = new NpgsqlCommand("UPDATE wordstudy_flashcard SET seen = false WHERE users_id = @users_id AND sett_id = @id", conn, transaction))
            {
                update_2.Parameters.AddWithValue("users_id", result.id);
                update_2.Parameters.AddWithValue("id", request.Id);

                await update_2.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                await conn.CloseAsync();
                return Ok(new { status = 1 });
            }

            catch 
            {
                await transaction.RollbackAsync();
                await conn.CloseAsync();
                return StatusCode(500, new { error = 0 });
            }
        }
    }
}