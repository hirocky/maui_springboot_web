using MauiApp1.Domain.Entities;
using MauiApp1.Domain.Repositories;
using MySqlConnector;

namespace MauiApp1.Infrastructure.Data;

/// <summary>
/// MySQL を利用した TODO リポジトリの実装。
///
/// - レイヤード構成では「インフラストラクチャ層」に属する。
/// - DB 接続や SQL 発行といった、外部リソースへの具体的なアクセス方法をここに閉じ込める。
/// - 上位レイヤー（アプリケーションサービスや ViewModel）は ITodoRepository のみを参照し、
///   このクラスや MySqlConnector の存在を意識しないようにするのがポイント。
/// </summary>
public class MySqlTodoRepository : ITodoRepository
{
    private readonly string _connectionString;

    public MySqlTodoRepository()
    {
        // 本来は connection string は設定ファイルやセキュアストレージから取得する。
        // サンプルなので固定値としているが、実際には appsettings.json や
        // 環境変数などから取得するのが望ましい。
        //
        // 要件:
        //   - ホスト: localhost
        //   - ユーザー: appuser
        //   - パスワード: apppass
        //   - スキーマ（DB 名）: appdb
        //
        _connectionString =
            "Server=localhost;Port=3306;Database=appdb;User Id=appuser;Password=apppass;SslMode=None;AllowPublicKeyRetrieval=True;";
    }

    /// <summary>
    /// MySQL への接続を生成するヘルパー。
    /// using スコープごとに都度生成し、使い終わったら破棄する。
    /// （コネクションプールは MySqlConnector 側で管理される）
    /// </summary>
    private MySqlConnection CreateConnection()
        => new MySqlConnection(_connectionString);

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync()
    {
        const string sql = """
            SELECT
                id,
                title,
                is_completed,
                created_at
            FROM todo_items;
            """;

        var result = new List<TodoItem>();

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var item = new TodoItem
            {
                Id = reader.GetInt32("id"),
                Title = reader.GetString("title"),
                IsCompleted = reader.GetBoolean("is_completed"),
                CreatedAt = reader.GetDateTime("created_at")
            };

            result.Add(item);
        }

        return result;
    }

    public async Task<TodoItem> AddAsync(TodoItem item)
    {
        const string sql = """
            INSERT INTO todo_items (title, is_completed, created_at)
            VALUES (@title, @is_completed, @created_at);
            SELECT LAST_INSERT_ID();
            """;

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@title", item.Title);
        command.Parameters.AddWithValue("@is_completed", item.IsCompleted);
        command.Parameters.AddWithValue("@created_at", item.CreatedAt);

        var idObj = await command.ExecuteScalarAsync();
        item.Id = Convert.ToInt32(idObj);

        return item;
    }

    public async Task UpdateAsync(TodoItem item)
    {
        const string sql = """
            UPDATE todo_items
            SET
                title = @title,
                is_completed = @is_completed,
                created_at = @created_at
            WHERE id = @id;
            """;

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@title", item.Title);
        command.Parameters.AddWithValue("@is_completed", item.IsCompleted);
        command.Parameters.AddWithValue("@created_at", item.CreatedAt);
        command.Parameters.AddWithValue("@id", item.Id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = """
            DELETE FROM todo_items
            WHERE id = @id;
            """;

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        await command.ExecuteNonQueryAsync();
    }
}

