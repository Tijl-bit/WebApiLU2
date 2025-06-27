using API.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace API.Repositories
{
    public class Object2DRepository : IObject2DRepository


    {
        private readonly string _connectionString;

        public Object2DRepository(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            }

            _connectionString = connectionString;
        }



        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<Object2D>> GetByEnvironmentIdAsync(Guid environmentId)
        {
            using var connection = CreateConnection();
            var sql = "SELECT * FROM Object2D WHERE EnvironmentId = @EnvironmentId";
            return await connection.QueryAsync<Object2D>(sql, new { EnvironmentId = environmentId });
        }



        public async Task<Object2D> GetByIdAsync(Guid id) // Gebruik Guid
        {
            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<Object2D>("SELECT * FROM Object2D WHERE Id = @Id", new { Id = id });
        }

        public async Task<Guid> InsertAsync(Object2D object2D) // Return type Guid
        {
            using var connection = CreateConnection();
            object2D.Id = Guid.NewGuid(); // Genereer een nieuwe Guid voor het object

            var sql = "INSERT INTO Object2D (Id, EnvironmentId, PrefabId, PositionX, PositionY, ScaleX, ScaleY, RotationZ, SortingLayer) " +
                      "VALUES (@Id, @EnvironmentId, @PrefabId, @PositionX, @PositionY, @ScaleX, @ScaleY, @RotationZ, @SortingLayer);";

            await connection.ExecuteAsync(sql, object2D);
            return object2D.Id; // Retourneer de nieuw gegenereerde Guid
        }
        public async Task<bool> UpdateAsync(Object2D object2D)
        {
            using var connection = CreateConnection();
            var sql = "UPDATE Object2D SET EnvironmentId = @EnvironmentId, PrefabId = @PrefabId, PositionX = @PositionX, " +
                      "PositionY = @PositionY, ScaleX = @ScaleX, ScaleY = @ScaleY, RotationZ = @RotationZ, SortingLayer = @SortingLayer " +
                      "WHERE Id = @Id";
            return await connection.ExecuteAsync(sql, object2D) > 0;
        }

        public async Task<bool> DeleteAsync(Guid id) // Gebruik Guid
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync("DELETE FROM Object2D WHERE Id = @Id", new { Id = id }) > 0;
        }
       


    }
}