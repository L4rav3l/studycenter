using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    public class GetMainCategory : ControllerBase
    {
        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public GetMainCategory(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpGet("api/graphs/main/get")]
        public async Task<IActionResult> GetMain()
        {
           string token = Request.GetBearerToken();
           UsersData result = await _authentication.VerifyToken(token);

           if(result == null)
           {
            return Unauthorized(new {error = 8});
           }

        var items = new List<Dictionary<string, object>>();
        var conn = await _connection.GetOpenConnectionAsync();

        await using(var data = new NpgsqlCommand("SELECT * FROM main_category WHERE users_id = @users_id AND seen = true", conn))
        {
            data.Parameters.AddWithValue("users_id", result.id);

            await using(var reader = await data.ExecuteReaderAsync())
            {
                while(await reader.ReadAsync())
                {
                    var item = new Dictionary<string, object>
                    {
                        ["NAME"] = reader.GetString(reader.GetOrdinal("name")),
                        ["ID"] = reader.GetInt32(reader.GetOrdinal("id"))
                    };

                    items.Add(item);
                }
            }
        }

        await conn.CloseAsync();
        return Ok(new {items});


        }
    }
}