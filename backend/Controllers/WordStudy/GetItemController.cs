using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class GetItemController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public GetItemController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpGet("api/wordstudy/itemlist")]
        public async Task<IActionResult> GetItem()
        {

            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if(result == null)
            {
                return Unauthorized(new {error = 8});
            }

            var conn = await _connection.GetOpenConnectionAsync();
            var transaction = await conn.BeginTransactionAsync();
            var items = new List<Dictionary<string, object>>();

            try
            {

                await using(var folder = new NpgsqlCommand("SELECT * FROM wordstudy_folder WHERE users_id = @users_id AND seen = true", conn, transaction))
                {
                    folder.Parameters.AddWithValue("users_id", result.id);
                    
                    await using(var reader = await folder.ExecuteReaderAsync())
                    {

                    while(await reader.ReadAsync())
                    {
                        var item = new Dictionary<string, object>
                        {
                            ["NAME"] = reader.GetString(reader.GetOrdinal("name")),
                            ["ID"] = reader.GetInt32(reader.GetOrdinal("id")),
                            ["DATE"] = reader.GetDateTime(reader.GetOrdinal("created")),
                            ["TYPE"] = "FOLDER"
                        };

                        items.Add(item);
                        }
                    }
                }

                await using(var folder = new NpgsqlCommand("SELECT * FROM wordstudy_sett WHERE users_id = @users_id AND seen = true", conn, transaction))
                {
                    folder.Parameters.AddWithValue("users_id", result.id);
                    
                    await using(var reader = await folder.ExecuteReaderAsync())
                    {

                    while(await reader.ReadAsync())
                    {
                        int folderIdOrdinal = reader.GetOrdinal("folder_id");
                        int? folderId = reader.IsDBNull(folderIdOrdinal) ? (int?)null : reader.GetInt32(folderIdOrdinal);

                        var item = new Dictionary<string, object>
                        {
                            ["NAME"] = reader.GetString(reader.GetOrdinal("name")),
                            ["ID"] = reader.GetInt32(reader.GetOrdinal("id")),
                            ["FOLDER_ID"] = folderId,
                            ["DATE"] = reader.GetDateTime(reader.GetOrdinal("created")),
                            ["TYPE"] = "SETT"
                        };

                        items.Add(item);
                        }
                    }
                }

                await transaction.CommitAsync();
                await conn.CloseAsync();
                return Ok(new {items});
            }

            catch(Exception ex)
            {  
                await transaction.RollbackAsync();
                await conn.CloseAsync();
                return StatusCode(500, new {error = 0});
            }

        }
    }
}