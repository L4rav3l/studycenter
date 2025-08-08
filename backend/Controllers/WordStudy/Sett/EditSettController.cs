using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.Models;
using StudyCenter.System;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class EditSettController : ControllerBase
    {

        private readonly Postgresql _connection;
        private readonly Authentication _authentication;

        public EditSettController(Postgresql connection, Authentication authentication)
        {
            _connection = connection;
            _authentication = authentication;
        }

        [HttpPost("api/wordstudy/sett/edit")]
        public async Task<IActionResult> EditSett([FromBody] EditSettRequest request)
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

            await using(var update = new NpgsqlCommand("UPDATE wordstudy_sett SET name = @name, notification_date = @notification_date, end_date = @end_date WHERE users_id = @users_id AND id = @id AND seen = true", conn, transaction))
            {
                update.Parameters.AddWithValue("users_id", result.id);
                update.Parameters.AddWithValue("id", request.Id);
                update.Parameters.AddWithValue("name", request.Name);
                update.Parameters.AddWithValue("notification_date", (object?)request.Notification_date ?? DBNull.Value);
                update.Parameters.AddWithValue("end_date", (object?)request.End_date ?? DBNull.Value);

                await update.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                await conn.CloseAsync();
                return Ok(new {status = 1});
            }

            catch
            {
                await transaction.RollbackAsync();
                await conn.CloseAsync();
                return StatusCode(500, new {error = 0});
            }
        }


    }
}