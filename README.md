# pep

`pep` は、以下の構成で動作確認や検証を行うためのワークスペースです。

- `MauiApp1`: .NET MAUI アプリ本体
- `MauiApp1.UnitTests`: `MauiApp1` 向けの単体テスト
- `spring_webapp1`: Spring Boot アプリ
- `docker-compose.yml`: MySQL コンテナ起動設定

## 前提環境

- .NET SDK 10 系
- .NET MAUI ワークロード
- Java 21（Spring Boot 用）
- Docker Desktop（MySQL 用）

## セットアップ

### 1. MySQL を起動

```powershell
docker compose up -d
```

`docker-compose.yml` では、`mysql:8.4` を利用し、ローカルの `mysql-data` をデータ永続化先として使用します。

### 2. MAUI アプリをビルドして実行

`dotnet build` はビルドのみ行い、アプリは起動しません。  
Windows で実行する場合は `dotnet run` を使います。

```powershell
# ビルド（起動はしない）
dotnet build .\MauiApp1\MauiApp1.csproj -f net10.0-windows10.0.19041.0

# 実行（必要ならビルドも行って起動）
dotnet run --project .\MauiApp1\MauiApp1.csproj -f net10.0-windows10.0.19041.0
```

### 3. 配布用 exe の作成（Windows / publish）

`dotnet publish` で `MauiApp1.exe` を含むフォルダ一式を出力します。出力先は `artifacts/`（`.gitignore` 済み）です。

**スクリプト（推奨）** — `pep` をカレントにして実行します。

```powershell
# フレームワーク依存（各 PC に .NET 10 デスクトップランタイムが必要）
.\scripts\publish-maui-windows.ps1

# ランタイム同梱（フォルダは大きくなるが、端末に .NET を入れなくてよい）
.\scripts\publish-maui-windows.ps1 -SelfContained
```

成果物の例: `artifacts\MauiApp1-windows\MauiApp1.exe`。**exe 単体ではなく、このフォルダごと**（ZIP やコピー）で配布してください。

**起動**: 上記の publish で作られた `MauiApp1.exe` を、エクスプローラーでダブルクリックするだけで MAUI アプリが起動します（同じフォルダ内の DLL などはそのまま置いた状態にしてください）。

**コマンドで直接行う場合**

```powershell
dotnet publish .\MauiApp1\MauiApp1.csproj `
  -c Release `
  -f net10.0-windows10.0.19041.0 `
  -o .\artifacts\MauiApp1-windows
```

### 4. 単体テストを実行

```powershell
dotnet test .\MauiApp1.UnitTests\MauiApp1.UnitTests.csproj
```

### 5. Spring Boot アプリを起動

```powershell
cd .\spring_webapp1
.\gradlew.bat bootRun
```

### 6. Cursor Terminal で文字化けする場合

`spring_webapp1` のログが文字化けする場合は、`bootRun` 前に文字コードを UTF-8 に揃えてから起動します。

```powershell
# 実行中の bootRun があれば Ctrl + C で停止
chcp 65001
[Console]::InputEncoding  = [System.Text.UTF8Encoding]::new($false)
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$env:JAVA_TOOL_OPTIONS = "-Dfile.encoding=UTF-8 -Dsun.stdout.encoding=UTF-8 -Dsun.stderr.encoding=UTF-8"
cd .\spring_webapp1
.\gradlew.bat bootRun
```

## 開発時メモ

- カスタマーディスプレイ（DM-D30）の出力方針と図: [`メモ/カスタマーディスプレイ出力方式.md`](メモ/カスタマーディスプレイ出力方式.md)
- レシート印刷（TM-T88V）の設計メモ: [`メモ/レシートプリンター出力方式.md`](メモ/レシートプリンター出力方式.md)
- ローカル専用ディレクトリ（`.claude`、`.vscode` など）は `.gitignore` で除外しています。
- 生成物（`bin`、`obj`、`build`、`target` など）も除外しています。
- 秘密情報は `.env` などに置き、Git に含めない運用を推奨します。
