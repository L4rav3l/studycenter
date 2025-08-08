using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{   
    [ApiController]
    public class SetParentSettController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public SetParentSettController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/wordstudy/sett/setparent")]
        public async Task<IActionResult> SetParentSett([FromBody] SetParentSettRequest request)
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
                await using(var check_sett = new NpgsqlCommand("SELECT * FROM wordstudy_sett WHERE users_id = @users_id AND id = @id AND seen = true", conn, transaction))
                {
                    check_sett.Parameters.AddWithValue("users_id", result.id);
                    check_sett.Parameters.AddWithValue("id", request.Sett_id);
                    
                    await using(var reader = await check_sett.ExecuteReaderAsync())
                    {
                        if(await reader.ReadAsync())
                        {

                        } else {
                            await conn.CloseAsync();
                            return Unauthorized(new {error = 9});
                        }
                    }
                }
                
                    if(request.Folder_id != null) {
                await using(var check_folder = new NpgsqlCommand("SELECT * FROM wordstudy_folder WHERE users_id = @users_id AND id = @id AND seen = true", conn, transaction))
                {
                    check_folder.Parameters.AddWithValue("users_id", result.id);
                    check_folder.Parameters.AddWithValue("id", request.Folder_id);
                    
                    await using(var reader = await check_folder.ExecuteReaderAsync())
                    {
                        if(await reader.ReadAsync())
                        {

                        } else {
                            await conn.CloseAsync();
                            return Unauthorized(new {error = 9});
                        }
                    }
                }
            }

                await using(var update = new NpgsqlCommand("UPDATE wordstudy_sett SET folder_id = @folder_id WHERE users_id = @users_id AND id = @id AND seen = true", conn, transaction))
                {
                    update.Parameters.AddWithValue("folder_id", (object)request.Folder_id ?? DBNull.Value);
                    update.Parameters.AddWithValue("users_id", result.id);
                    update.Parameters.AddWithValue("id", request.Sett_id);

                    await update.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                await conn.CloseAsync();
                return Ok(new {status = 1});
            }

            catch(Exception ex) {
                await transaction.RollbackAsync();
                await conn.CloseAsync();
                return StatusCode(500, new {error = 0});
            }
        }
    }
}