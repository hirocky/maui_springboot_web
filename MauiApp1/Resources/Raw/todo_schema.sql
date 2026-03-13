-- TODOテーブル定義（DDL）サンプル（MySQL版）
-- 実際のアプリでは、事前に MySQL 上でこのテーブルを作成しておくことを想定している。
-- （アプリから自動作成してもよいが、本番環境ではマイグレーションツール等で管理するのが一般的）

CREATE TABLE IF NOT EXISTS todo_items (
    id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(200) NOT NULL,
    is_completed TINYINT(1) NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL
);

