-- =============================================================================
-- 習慣（ルーチン）機能用テーブル定義（MySQL）
-- =============================================================================
-- 毎日やりたいことの達成状況を記録するためのスキーマ。
-- 事前に MySQL 上で実行するか、マイグレーションツールで管理する想定。
-- =============================================================================

-- カテゴリ（健康、学習、家事など）
CREATE TABLE IF NOT EXISTS categories (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    sort_order INT NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL
);

-- 習慣（習慣名、目標頻度、色設定、カテゴリ）
-- category_id=0 は「未分類」として扱い、FK は張らない（アプリ側で整合性を取る）。
CREATE TABLE IF NOT EXISTS habits (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    target_frequency_per_week INT NOT NULL DEFAULT 7 COMMENT '週あたりの目標回数。7=毎日',
    color_hex VARCHAR(20) NOT NULL DEFAULT '#6200EE',
    category_id INT NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL
);

-- チェックイン：どの日付に、どの習慣を完了したかの記録
CREATE TABLE IF NOT EXISTS check_ins (
    id INT AUTO_INCREMENT PRIMARY KEY,
    habit_id INT NOT NULL,
    check_in_date DATE NOT NULL,
    created_at DATETIME NOT NULL,
    UNIQUE KEY uq_habit_date (habit_id, check_in_date),
    CONSTRAINT fk_check_ins_habit FOREIGN KEY (habit_id) REFERENCES habits(id) ON DELETE CASCADE
);

-- 初期カテゴリ（任意）
-- INSERT INTO categories (name, sort_order, created_at) VALUES
-- ('健康', 1, NOW()),
-- ('学習', 2, NOW()),
-- ('家事', 3, NOW());
