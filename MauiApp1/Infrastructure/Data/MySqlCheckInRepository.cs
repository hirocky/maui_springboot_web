using MauiApp1.Domain.Entities;
using MauiApp1.Domain.Repositories;
using MySqlConnector;

namespace MauiApp1.Infrastructure.Data;

/// <summary>
/// チェックイン（達成記録）の MySQL リポジトリ実装（インフラストラクチャ層）。
///
/// check_ins テーブルへの追加・削除・照会を担当。
/// 今日タスク画面での「今日チェック済みか」と、進捗レポートでの期間内達成日一覧を提供する。
/// </summary>
public class MySqlCheckInRepository : ICheckInRepository
{
    private readonly string _connectionString =
        "Server=localhost;Port=3306;Database=appdb;User Id=appuser;Password=apppass;SslMode=None;AllowPublicKeyRetrieval=True;";

    private MySqlConnection CreateConnection() => new MySqlConnection(_connectionString);

    public async Task<bool> ExistsAsync(int habitId, DateTime date)
    {
        var dateOnly = date.Date;
        const string sql = "SELECT 1 FROM check_ins WHERE habit_id = @habit_id AND check_in_date = @d LIMIT 1;";
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@habit_id", habitId);
        cmd.Parameters.AddWithValue("@d", dateOnly);
        var obj = await cmd.ExecuteScalarAsync();
        return obj != null;
    }

    public async Task<CheckIn> AddAsync(CheckIn checkIn)
    {
        const string sql = """
            INSERT INTO check_ins (habit_id, check_in_date, created_at)
            VALUES (@habit_id, @check_in_date, @created_at);
            SELECT LAST_INSERT_ID();
            """;
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@habit_id", checkIn.HabitId);
        cmd.Parameters.AddWithValue("@check_in_date", checkIn.CheckInDate.Date);
        cmd.Parameters.AddWithValue("@created_at", checkIn.CreatedAt);
        var idObj = await cmd.ExecuteScalarAsync();
        checkIn.Id = Convert.ToInt32(idObj);
        return checkIn;
    }

    public async Task DeleteAsync(int habitId, DateTime date)
    {
        var dateOnly = date.Date;
        const string sql = "DELETE FROM check_ins WHERE habit_id = @habit_id AND check_in_date = @d;";
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@habit_id", habitId);
        cmd.Parameters.AddWithValue("@d", dateOnly);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<DateTime>> GetCheckInDatesAsync(int habitId, DateTime from, DateTime to)
    {
        const string sql = """
            SELECT check_in_date FROM check_ins
            WHERE habit_id = @habit_id AND check_in_date >= @from AND check_in_date <= @to
            ORDER BY check_in_date;
            """;
        var list = new List<DateTime>();
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@habit_id", habitId);
        cmd.Parameters.AddWithValue("@from", from.Date);
        cmd.Parameters.AddWithValue("@to", to.Date);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(reader.GetDateTime("check_in_date").Date);
        }
        return list;
    }

    public async Task<IReadOnlySet<int>> GetCheckedInHabitIdsForDateAsync(DateTime date)
    {
        var dateOnly = date.Date;
        const string sql = "SELECT habit_id FROM check_ins WHERE check_in_date = @d;";
        var set = new HashSet<int>();
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@d", dateOnly);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            set.Add(reader.GetInt32("habit_id"));
        }
        return set;
    }
}
