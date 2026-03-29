using MauiApp1.Domain.Entities;
using MauiApp1.Domain.Repositories;
using MySqlConnector;

namespace MauiApp1.Infrastructure.Data;

/// <summary>
/// 習慣（Habit）の MySQL リポジトリ実装（インフラストラクチャ層）。
///
/// habits テーブルへの CRUD を担当。カテゴリは JOIN せず ID のみ保持するシンプル実装。
/// カテゴリ名が必要な場合はアプリケーション層で Category を別途取得して組み立てる。
/// </summary>
public class MySqlHabitRepository : IHabitRepository
{
    private readonly string _connectionString =
        "Server=localhost;Port=3306;Database=appdb;User Id=appuser;Password=apppass;SslMode=None;AllowPublicKeyRetrieval=True;";

    private MySqlConnection CreateConnection() => new MySqlConnection(_connectionString);

    public async Task<IReadOnlyList<Habit>> GetAllAsync()
    {
        const string sql = """
            SELECT id, name, target_frequency_per_week, color_hex, category_id, created_at
            FROM habits
            ORDER BY category_id ASC, created_at ASC;
            """;
        var list = new List<Habit>();
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapHabit(reader));
        }
        return list;
    }

    public async Task<Habit?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT id, name, target_frequency_per_week, color_hex, category_id, created_at
            FROM habits WHERE id = @id;
            """;
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;
        return MapHabit(reader);
    }

    public async Task<Habit> AddAsync(Habit habit)
    {
        const string sql = """
            INSERT INTO habits (name, target_frequency_per_week, color_hex, category_id, created_at)
            VALUES (@name, @target_frequency_per_week, @color_hex, @category_id, @created_at);
            SELECT LAST_INSERT_ID();
            """;
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@name", habit.Name);
        cmd.Parameters.AddWithValue("@target_frequency_per_week", habit.TargetFrequencyPerWeek);
        cmd.Parameters.AddWithValue("@color_hex", habit.ColorHex);
        cmd.Parameters.AddWithValue("@category_id", habit.CategoryId);
        cmd.Parameters.AddWithValue("@created_at", habit.CreatedAt);
        var idObj = await cmd.ExecuteScalarAsync();
        habit.Id = Convert.ToInt32(idObj);
        return habit;
    }

    public async Task UpdateAsync(Habit habit)
    {
        const string sql = """
            UPDATE habits
            SET name = @name, target_frequency_per_week = @tf, color_hex = @color_hex, category_id = @category_id
            WHERE id = @id;
            """;
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@name", habit.Name);
        cmd.Parameters.AddWithValue("@tf", habit.TargetFrequencyPerWeek);
        cmd.Parameters.AddWithValue("@color_hex", habit.ColorHex);
        cmd.Parameters.AddWithValue("@category_id", habit.CategoryId);
        cmd.Parameters.AddWithValue("@id", habit.Id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM habits WHERE id = @id;";
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static Habit MapHabit(MySqlDataReader reader)
    {
        return new Habit
        {
            Id = reader.GetInt32("id"),
            Name = reader.GetString("name"),
            TargetFrequencyPerWeek = reader.GetInt32("target_frequency_per_week"),
            ColorHex = reader.GetString("color_hex"),
            CategoryId = reader.GetInt32("category_id"),
            CreatedAt = reader.GetDateTime("created_at")
        };
    }
}
