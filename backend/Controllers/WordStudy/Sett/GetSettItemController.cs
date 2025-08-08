using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class GetSettItemController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public GetSettItemController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpGet("api/wordstudy/sett/get")]
        public async Task<IActionResult> GetSett([FromQuery] GetSettRequest request)
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

                await using(var sett = new NpgsqlCommand("SELECT * FROM wordstudy_flashcard WHERE users_id = @users_id AND sett_id = @sett_id AND seen = true", conn, transaction))
                {
                    sett.Parameters.AddWithValue("users_id", result.id);
                    sett.Parameters.AddWithValue("sett_id", request.Id);
                    
                    await using(var reader = await sett.ExecuteReaderAsync())
                    {

                    while(await reader.ReadAsync())
                    {
                        var item = new Dictionary<string, object>
                        {
                            ["ID"] = reader.GetInt32(reader.GetOrdinal("id")),
                            ["FRONT"] = reader.GetString(reader.GetOrdinal("front")),
                            ["BACK"] = reader.GetString(reader.GetOrdinal("back")),
                            ["TYPE"] = "FLASHCARD"
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
                Console.WriteLine(ex);
                await transaction.RollbackAsync();
                await conn.CloseAsync();
                return StatusCode(500, new {error = 0});
            }

        }
    }
}