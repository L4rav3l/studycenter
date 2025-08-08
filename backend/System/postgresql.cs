using Npgsql;
using DotNetEnv;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace StudyCenter.System {
    public class Postgresql {

        private readonly string _connectionString;
        
        public Postgresql() {
            Env.Load();

            _connectionString = Environment.GetEnvironmentVariable("PSQL_CONNECTION");
        }

        public async Task<NpgsqlConnection> GetOpenConnectionAsync()
        {
            var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            return conn;
        }



    }
}