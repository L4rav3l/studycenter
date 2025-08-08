using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;
using System;
using System.Threading.Tasks;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class DeleteFolderController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public DeleteFolderController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/wordstudy/folder/delete")]
        public async Task<IActionResult> DeleteFolder([FromBody] DeleteFolderRequest request)
        {
            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if (result == null)
            {
                return BadRequest(new { error = 8 });
            }

            await using var conn = await _connection.GetOpenConnectionAsync();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                await using (var check = new NpgsqlCommand("SELECT * FROM wordstudy_folder WHERE users_id = @users_id AND id = @id AND seen = true", conn, transaction))
                {
                    check.Parameters.AddWithValue("users_id", result.id);
                    check.Parameters.AddWithValue("id", request.Id);

                    var exists = await check.ExecuteScalarAsync();
                    if (exists == null)
                    {
                        await transaction.RollbackAsync();
                        await conn.CloseAsync();
                        return BadRequest(new { error = 9 });
                    }
                }

                await using (var sett = new NpgsqlCommand("SELECT id FROM wordstudy_sett WHERE folder_id = @folder_id AND users_id = @users_id AND seen = true", conn, transaction))
                {
                    sett.Parameters.AddWithValue("folder_id", request.Id);
                    sett.Parameters.AddWithValue("users_id", result.id);

                    await using var reader = await sett.ExecuteReaderAsync();

                    var settIds = new List<int>();

                    while (await reader.ReadAsync())
                    {
                        settIds.Add(reader.GetInt32(0));
                    }
                    await reader.CloseAsync();

                    foreach (var settId in settIds)
                    {
                        await using var updateFlashcard = new NpgsqlCommand("UPDATE wordstudy_flashcard SET seen = false WHERE users_id = @users_id AND sett_id = @sett_id AND seen = true", conn, transaction);
                        updateFlashcard.Parameters.AddWithValue("users_id", result.id);
                        updateFlashcard.Parameters.AddWithValue("sett_id", settId);
                        await updateFlashcard.ExecuteNonQueryAsync();

                        await using var updateSett = new NpgsqlCommand("UPDATE wordstudy_sett SET seen = false WHERE users_id = @users_id AND id = @id AND seen = true", conn, transaction);
                        updateSett.Parameters.AddWithValue("users_id", result.id);
                        updateSett.Parameters.AddWithValue("id", settId);
                        await updateSett.ExecuteNonQueryAsync();
                    }
                }

                await using (var updateFolder = new NpgsqlCommand("UPDATE wordstudy_folder SET seen = false WHERE users_id = @users_id AND id = @id AND seen = true", conn, transaction))
                {
                    updateFolder.Parameters.AddWithValue("users_id", result.id);
                    updateFolder.Parameters.AddWithValue("id", request.Id);
                    await updateFolder.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                await conn.CloseAsync();
                return Ok(new { status = 1 });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await conn.CloseAsync();
                return StatusCode(500, new { error = 0 });
            }
        }
    }
}
