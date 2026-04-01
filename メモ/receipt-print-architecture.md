# レシート印刷機能 クリーンアーキテクチャ設計

MauiApp1 と spring_webapp1 の共通アーキテクチャを示す。

---

## クラス図

```mermaid
classDiagram
    %% ==================== DOMAIN ====================
    namespace Domain {
        class ReceiptLine {
            +string Draw
            +bool Bold
            +bool Large
            +bool Center
            +bool Right
            +bool IsLeftRight
            +string LeftText
            +string RightText
        }
        class ReceiptDocument {
            +IReadOnlyList~ReceiptLine~ Lines
            +string? LogoPath
        }
        class IReceiptPrinter {
            <<interface>>
            +PrintAndCutAsync(doc, printerName) Task
        }
        class IPrinterDiscovery {
            <<interface>>
            +GetInstalledPrinterNames() List~string~
        }
    }

    %% ==================== APPLICATION ====================
    namespace Application {
        class ReceiptTextParser {
            +Parse(text, logoPath) ReceiptDocument
        }
        class PrintReceiptUseCase {
            -IReceiptPrinter printer
            +ExecuteAsync(text, printerName, logoPath) Task
        }
    }

    %% ==================== INFRASTRUCTURE ====================
    namespace Infrastructure {
        class WindowsEpsonReceiptPrinter {
            +PrintAndCutAsync(doc, printerName) Task
            -RenderDocument(hDC, doc)
        }
        class WindowsPrinterDiscovery {
            +GetInstalledPrinterNames() List~string~
        }
        class NullReceiptPrinter {
            +PrintAndCutAsync() throws PlatformNotSupportedException
        }
        class NullPrinterDiscovery {
            +GetInstalledPrinterNames() []
        }
    }

    %% ==================== PRESENTATION ====================
    namespace Presentation {
        class ReceiptPrintPage {
            -PrintReceiptUseCase useCase
            -IPrinterDiscovery discovery
            +OnSampleKaikatsuReceiptClicked()
            +OnReceiptPrintClicked()
        }
    }

    %% ==================== 依存関係 ====================
    PrintReceiptUseCase --> IReceiptPrinter : uses
    PrintReceiptUseCase --> ReceiptTextParser : uses
    ReceiptTextParser --> ReceiptDocument : creates
    ReceiptTextParser --> ReceiptLine : creates
    ReceiptDocument "1" *-- "n" ReceiptLine

    WindowsEpsonReceiptPrinter ..|> IReceiptPrinter
    NullReceiptPrinter ..|> IReceiptPrinter
    WindowsPrinterDiscovery ..|> IPrinterDiscovery
    NullPrinterDiscovery ..|> IPrinterDiscovery

    ReceiptPrintPage --> PrintReceiptUseCase
    ReceiptPrintPage --> IPrinterDiscovery
```

---

## シーケンス図（共通 印刷フロー）

```mermaid
sequenceDiagram
    participant UI as Presentation<br/>(ReceiptPrintPage / OrderApiController)
    participant UC as Application<br/>PrintReceiptUseCase
    participant Parser as Application<br/>ReceiptTextParser
    participant Domain as Domain<br/>ReceiptDocument / ReceiptLine
    participant Port as Domain<br/>IReceiptPrinter
    participant Infra as Infrastructure<br/>EpsonReceiptPrinter

    UI->>UC: ExecuteAsync(rawText, printerName, logoPath)
    UC->>Parser: Parse(rawText, logoPath)
    Parser->>Domain: new ReceiptLine per line (マーカー解析)
    Parser->>Domain: new ReceiptDocument(lines, logoPath)
    Domain-->>Parser: ReceiptDocument
    Parser-->>UC: ReceiptDocument
    UC->>Port: PrintAndCutAsync(document, printerName)
    Port->>Infra: (実装に委譲)
    Infra->>Infra: プリンター解決 (GDI / AWT)
    Infra->>Infra: ReceiptLine ごとに描画<br/>(Bold/Large/Center/Right/LR)
    Infra-->>UC: 完了
    UC-->>UI: 完了
```

---

## シーケンス図（spring_webapp1 固有 注文→レシート）

```mermaid
sequenceDiagram
    participant HTTP as HTTP Client (POS画面)
    participant Ctrl as OrderApiController
    participant OrderUC as PrintOrderReceiptUseCase
    participant PrintUC as PrintReceiptUseCase
    participant Parser as ReceiptTextParser
    participant Printer as EpsonAwtReceiptPrinter

    HTTP->>Ctrl: POST /api/orders (OrderRequest JSON)
    Ctrl->>Ctrl: toOrderData(request) → OrderData
    Ctrl->>OrderUC: execute(OrderData)
    OrderUC->>OrderUC: buildReceiptText(order) → rawText
    OrderUC->>PrintUC: execute(rawText, "TM-T88V")
    PrintUC->>Parser: parse(rawText)
    Parser-->>PrintUC: ReceiptDocument
    PrintUC->>Printer: printAndCut(document, "TM-T88V")
    Printer->>Printer: JavaPrinterDiscovery.findByKeyword()
    Printer->>Printer: PrinterJob.print() → AWT描画
    Printer-->>HTTP: 印刷完了
    Ctrl-->>HTTP: {"status":"received"}
```

---

## ファイル構成

### MauiApp1

```
MauiApp1/
├── Domain/Printing/
│   ├── ReceiptLine.cs          ← 値オブジェクト（マーカー解析済み1行）
│   ├── ReceiptDocument.cs      ← 値オブジェクト（レシート全体）
│   ├── IReceiptPrinter.cs      ← ポート（印刷）
│   └── IPrinterDiscovery.cs    ← ポート（プリンター列挙）
├── Application/Printing/
│   ├── ReceiptTextParser.cs    ← マーカー付きテキスト → ReceiptDocument
│   └── PrintReceiptUseCase.cs  ← ユースケース
├── Infrastructure/Platform/
│   ├── WindowsEpsonReceiptPrinter.cs  ← IReceiptPrinter 実装 (GDI)
│   ├── WindowsPrinterDiscovery.cs     ← IPrinterDiscovery 実装 (WinSpool)
│   ├── NullReceiptPrinter.cs          ← 非Windows用スタブ
│   └── NullPrinterDiscovery.cs        ← 非Windows用スタブ
└── Presentation/Pages/Receipt/
    └── ReceiptPrintPage.xaml.cs       ← PrintReceiptUseCase + IPrinterDiscovery を注入
```

### spring_webapp1

```
com.example.spring_webapp1/
├── domain/printing/
│   ├── ReceiptLine.java         ← 値オブジェクト（マーカー解析済み1行）
│   ├── ReceiptDocument.java     ← 値オブジェクト（レシート全体）
│   ├── IReceiptPrinter.java     ← ポート（印刷）
│   └── IPrinterDiscovery.java   ← ポート（プリンター列挙）
├── application/printing/
│   ├── ReceiptTextParser.java   ← マーカー付きテキスト → ReceiptDocument
│   └── PrintReceiptUseCase.java ← ユースケース
├── application/order/
│   ├── OrderData.java           ← アプリケーション層 DTO
│   ├── OrderItemData.java       ← アプリケーション層 DTO
│   └── PrintOrderReceiptUseCase.java ← 注文→レシートテキスト変換＋印刷
├── infrastructure/printing/
│   ├── EpsonAwtReceiptPrinter.java  ← IReceiptPrinter 実装 (Java AWT)
│   └── JavaPrinterDiscovery.java    ← IPrinterDiscovery 実装 (PrintServiceLookup)
└── presentation/
    ├── OrderApiController.java  ← HTTP → OrderData → PrintOrderReceiptUseCase
    └── OrderPageController.java ← /order → static HTML
```
