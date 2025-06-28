using API.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace API.Repositories
{
    public class Environment2DRepository : IEnvironment2DRepository
    {
        private readonly string _connectionString;

        public Environment2DRepository(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            }
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<Environment2D>> GetAllAsync(string id)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<Environment2D>("SELECT * FROM Environment2D WHERE OwnerUserId = @Id", new { Id = id });
        }

        public async Task<Environment2D> GetByIdAsync(Guid id)
        {
            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<Environment2D>("SELECT * FROM Environment2D WHERE Id = @Id", new { Id = id });
        }

        
        public async Task<bool> ExistsWithNameAsync(string ownerUserId, string name)
        {
            using var connection = CreateConnection();
            var sql = "SELECT COUNT(1) FROM Environment2D WHERE OwnerUserId = @OwnerUserId AND Name = @Name";
            int count = await connection.ExecuteScalarAsync<int>(sql, new { OwnerUserId = ownerUserId, Name = name });
            return count > 0;
        }

        public async Task<Guid> InsertAsync(Environment2D environment)
        {
            if (environment.OwnerUserId == null)
            {
                throw new ArgumentException("OwnerUserId cannot be null");
            }

            
            bool exists = await ExistsWithNameAsync(environment.OwnerUserId, environment.Name);
            if (exists)
            {
                throw new InvalidOperationException("An environment with this name already exists for this user.");
            }

            using var connection = CreateConnection();
            environment.Id = Guid.NewGuid();
            var sql = "INSERT INTO Environment2D (Id, Name, OwnerUserId, MaxLength, MaxHeight) " +
                      "VALUES (@Id, @Name, @OwnerUserId, @MaxLength, @MaxHeight);";
            await connection.ExecuteAsync(sql, environment);
            return environment.Id;
        }

        public async Task<bool> UpdateAsync(Environment2D environment)
        {
            using var connection = CreateConnection();
            var sql = "UPDATE Environment2D SET Name = @Name, OwnerUserId = @OwnerUserId, MaxLength = @MaxLength, MaxHeight = @MaxHeight WHERE Id = @Id";
            return await connection.ExecuteAsync(sql, environment) > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync("DELETE FROM Environment2D WHERE Id = @Id", new { Id = id }) > 0;
        }
    }
}
