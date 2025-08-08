using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class GetItemsController : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public GetItemsController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpGet("api/graphs/itemlist")]
        public async Task<IActionResult> GetGraph([FromQuery] GetItemRequest request)
        {
            string token = Request.GetBearerToken();
            UsersData result = await _authentication.VerifyToken(token);

            if (result == null)
            {
                return Unauthorized(new { error = 8 });
            }

            var conn = await _connection.GetOpenConnectionAsync();
            var items = new List<Dictionary<string, object>>();

            await using (var graphs = new NpgsqlCommand("SELECT * FROM category WHERE main_parent = @main_parent AND users_id = @users_id AND seen = true", conn))
            {
                graphs.Parameters.AddWithValue("main_parent", request.Id);
                graphs.Parameters.AddWithValue("users_id", result.id);

                await using (var reader = await graphs.ExecuteReaderAsync())
                {

                    int parentColumnIndex = reader.GetOrdinal("parent");

                    while (await reader.ReadAsync())
                    {
                        var item = new Dictionary<string, object>
                        {
                            
                            ["PARENT"] = reader.IsDBNull(parentColumnIndex) ? (int?)null : reader.GetInt32(parentColumnIndex),
                            ["NAME"] = reader.GetString(reader.GetOrdinal("name")),
                            ["ID"] = reader.GetInt32(reader.GetOrdinal("id")),
                            ["TYPE"] = "CATEGORY"
                        };

                        items.Add(item);
                    }
                }
            }

            await using (var documents = new NpgsqlCommand("SELECT * FROM documents WHERE main_parent = @main_parent AND users_id = @users_id AND seen = true", conn))
            {
                documents.Parameters.AddWithValue("main_parent", request.Id);
                documents.Parameters.AddWithValue("users_id", result.id);

                await using (var reader = await documents.ExecuteReaderAsync())
                {

                    int parentColumnIndex = reader.GetOrdinal("parent");

                    while (await reader.ReadAsync())
                    {
                        var item = new Dictionary<string, object>
                        {
                            ["PARENT"] = reader.IsDBNull(parentColumnIndex) ? (int?)null : reader.GetInt32(parentColumnIndex),
                            ["TITLE"] = reader.GetString(reader.GetOrdinal("title")),
                            ["ID"] = reader.GetInt32(reader.GetOrdinal("id")),
                            ["TYPE"] = "DOCUMENT"
                        };

                        items.Add(item);
                    }
                }
            }

            await conn.CloseAsync();
            return Ok(new { items });
        }
    }
}
