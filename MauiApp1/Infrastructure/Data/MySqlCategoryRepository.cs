using MauiApp1.Domain.Entities;
using MauiApp1.Domain.Repositories;
using MySqlConnector;

namespace MauiApp1.Infrastructure.Data;

/// <summary>
/// カテゴリの MySQL リポジトリ実装（インフラストラクチャ層）。
///
/// DB 接続・SQL 発行をここに閉じ込め、ドメインの ICategoryRepository を満たす。
/// 接続文字列は Todo と同様 appdb を想定（本番では設定から取得すること）。
/// </summary>
public class MySqlCategoryRepository : ICategoryRepository
{
    private readonly string _connectionString =
        "Server=localhost;Port=3306;Database=appdb;User Id=appuser;Password=apppass;SslMode=None;";

    private MySqlConnection CreateConnection() => new MySqlConnection(_connectionString);

    public async Task<IReadOnlyList<Category>> GetAllAsync()
    {
        const string sql = """
            SELECT id, name, sort_order
            FROM categories
            ORDER BY sort_order ASC, id ASC;
            """;
        var list = new List<Category>();
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Category
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                SortOrder = reader.GetInt32("sort_order")
            });
        }
        return list;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        const string sql = "SELECT id, name, sort_order FROM categories WHERE id = @id;";
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;
        return new Category
        {
            Id = reader.GetInt32("id"),
            Name = reader.GetString("name"),
            SortOrder = reader.GetInt32("sort_order")
        };
    }

    public async Task<Category> AddAsync(Category category)
    {
        const string sql = """
            INSERT INTO categories (name, sort_order, created_at)
            VALUES (@name, @sort_order, NOW());
            SELECT LAST_INSERT_ID();
            """;
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@name", category.Name);
        cmd.Parameters.AddWithValue("@sort_order", category.SortOrder);
        var idObj = await cmd.ExecuteScalarAsync();
        category.Id = Convert.ToInt32(idObj);
        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        const string sql = """
            UPDATE categories SET name = @name, sort_order = @sort_order WHERE id = @id;
            """;
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@name", category.Name);
        cmd.Parameters.AddWithValue("@sort_order", category.SortOrder);
        cmd.Parameters.AddWithValue("@id", category.Id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM categories WHERE id = @id;";
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }
}
