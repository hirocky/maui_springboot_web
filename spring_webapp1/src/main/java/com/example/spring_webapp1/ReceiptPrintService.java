package com.example.spring_webapp1;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;

import java.awt.*;
import java.awt.print.*;
import javax.print.*;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.Arrays;
import java.util.List;

/**
 * APD5 ドライバ経由で EPSON TM-T88V にレシートを印刷するサービス。
 * Java AWT PrinterJob (GDI) を使用し、MS Gothic で日本語を描画する。
 * テキストフォーマット記号は MauiApp1/WindowsEpsonReceiptPrintService と共通:
 *   [B] 太字, [C] 中央, [R] 右寄せ, [L] 大文字, [LR]左テキスト|右テキスト
 */
@Service
public class ReceiptPrintService {

    private static final Logger log = LoggerFactory.getLogger(ReceiptPrintService.class);
    private static final String PRINTER_KEYWORD = "TM-T88V";

    public void printOrder(OrderApiController.OrderRequest order) {
        PrintService printer = findPrinter(PRINTER_KEYWORD);
        if (printer == null) {
            List<String> available = Arrays.stream(PrintServiceLookup.lookupPrintServices(null, null))
                    .map(PrintService::getName).toList();
            log.warn("プリンター「{}」が見つかりません。利用可能: {}", PRINTER_KEYWORD, available);
            return;
        }
        try {
            String text = buildReceiptText(order);
            PrinterJob job = PrinterJob.getPrinterJob();
            job.setPrintService(printer);
            PageFormat pf = job.defaultPage();
            job.setPrintable(new ReceiptPrintable(text), pf);
            job.print();
            log.info("レシート印刷完了: #{}", order.orderNumber());
        } catch (PrinterException e) {
            log.error("レシート印刷エラー: {}", e.getMessage(), e);
        }
    }

    private static PrintService findPrinter(String keyword) {
        for (PrintService svc : PrintServiceLookup.lookupPrintServices(null, null)) {
            if (svc.getName().contains(keyword)) return svc;
        }
        return null;
    }

    private static String buildReceiptText(OrderApiController.OrderRequest order) {
        String div = "----------------------------------------";
        String now = LocalDateTime.now().format(DateTimeFormatter.ofPattern("yyyy/MM/dd HH:mm"));
        var sb = new StringBuilder();

        sb.append(div).append("\n");
        sb.append("[B][C][L]注文伝票").append("\n");
        sb.append(div).append("\n");
        sb.append("[LR]オーダー番号|#").append(order.orderNumber()).append("\n");
        sb.append("[R]").append(now).append("\n");
        sb.append(div).append("\n");

        for (var item : order.items()) {
            String total = "¥" + String.format("%,d", item.price() * item.qty());
            sb.append("[LR]").append(item.name()).append(" x").append(item.qty())
              .append("|").append(total).append("\n");
        }

        sb.append(div).append("\n");
        sb.append("[LR]小計|¥").append(String.format("%,d", order.subtotal())).append("\n");
        sb.append("[LR]消費税(10%)|¥").append(String.format("%,d", order.tax())).append("\n");
        sb.append("[B][LR]合計|¥").append(String.format("%,d", order.total())).append("\n");
        sb.append(div).append("\n");
        sb.append("\n\n");

        return sb.toString();
    }

    // -------------------------------------------------------------------------
    // Printable 実装 (C# の RenderReceipt に相当)
    // -------------------------------------------------------------------------

    private static class ReceiptPrintable implements Printable {

        private static final int FONT_SIZE_NORMAL = 9;
        private static final int FONT_SIZE_LARGE = 14;

        private final String text;

        ReceiptPrintable(String text) {
            this.text = text;
        }

        @Override
        public int print(Graphics graphics, PageFormat pageFormat, int pageIndex) {
            if (pageIndex > 0) return NO_SUCH_PAGE;

            Graphics2D g = (Graphics2D) graphics;
            g.setRenderingHint(RenderingHints.KEY_TEXT_ANTIALIASING, RenderingHints.VALUE_TEXT_ANTIALIAS_ON);
            g.setColor(Color.BLACK);

            String fontName = resolveFontName(g, "MS Gothic", "MS ゴシック", Font.MONOSPACED);
            Font fNormal     = new Font(fontName, Font.PLAIN, FONT_SIZE_NORMAL);
            Font fBold       = new Font(fontName, Font.BOLD,  FONT_SIZE_NORMAL);
            Font fNormalLg   = new Font(fontName, Font.PLAIN, FONT_SIZE_LARGE);
            Font fBoldLg     = new Font(fontName, Font.BOLD,  FONT_SIZE_LARGE);

            float xOrigin = (float) pageFormat.getImageableX();
            float yOrigin = (float) pageFormat.getImageableY();
            float pageWidth = (float) pageFormat.getImageableWidth();

            g.setFont(fNormal);
            FontMetrics fmBase = g.getFontMetrics();
            float y = yOrigin + fmBase.getAscent();

            for (String rawLine : text.replace("\r\n", "\n").replace("\r", "\n").split("\n", -1)) {
                Line line = Line.parse(rawLine);
                Font font = pickFont(line.bold, line.large, fNormal, fBold, fNormalLg, fBoldLg);
                g.setFont(font);
                FontMetrics fm = g.getFontMetrics();

                if (line.isLR) {
                    float xRight = xOrigin + pageWidth - fm.stringWidth(line.rightText);
                    float minXRight = xOrigin + fm.stringWidth(line.leftText) + 4;
                    g.drawString(line.leftText, xOrigin, y);
                    g.drawString(line.rightText, Math.max(minXRight, xRight), y);
                } else {
                    float x;
                    if (line.center) {
                        x = xOrigin + (pageWidth - fm.stringWidth(line.draw)) / 2f;
                    } else if (line.right) {
                        x = xOrigin + pageWidth - fm.stringWidth(line.draw);
                    } else {
                        x = xOrigin;
                    }
                    g.drawString(line.draw, Math.max(xOrigin, x), y);
                }

                y += fm.getHeight();
            }

            return PAGE_EXISTS;
        }

        private static String resolveFontName(Graphics2D g, String... candidates) {
            java.util.Set<String> available = new java.util.HashSet<>(
                    Arrays.asList(GraphicsEnvironment.getLocalGraphicsEnvironment().getAvailableFontFamilyNames()));
            for (String c : candidates) {
                if (available.contains(c)) return c;
            }
            return candidates[candidates.length - 1]; // fallback (last = logical name like MONOSPACED)
        }

        private static Font pickFont(boolean bold, boolean large, Font fN, Font fB, Font fNL, Font fBL) {
            if (large) return bold ? fBL : fNL;
            return bold ? fB : fN;
        }
    }

    // -------------------------------------------------------------------------
    // テキスト行パーサ (MauiApp1 の ParseReceiptLine に相当)
    // -------------------------------------------------------------------------

    private record Line(String draw, boolean bold, boolean large,
                        boolean center, boolean right,
                        boolean isLR, String leftText, String rightText) {

        static Line parse(String raw) {
            String s = raw;
            boolean bold = false, large = false, center = false, right = false, isLR = false;
            String leftText = "", rightText = "";

            if (s.startsWith("[B]")) { bold = true; s = s.substring(3); }
            if (s.startsWith("[L]")) { large = true; s = s.substring(3); }

            if (s.startsWith("[LR]")) {
                isLR = true;
                String rest = s.substring(4);
                int sep = rest.indexOf('|');
                leftText  = sep >= 0 ? rest.substring(0, sep).trim() : rest;
                rightText = sep >= 0 ? rest.substring(sep + 1).trim() : "";
                s = "";
            } else if (s.startsWith("[C]")) {
                center = true; s = s.substring(3);
                if (s.startsWith("[L]")) { large = true; s = s.substring(3); }
            } else if (s.startsWith("[R]")) {
                right = true; s = s.substring(3);
                if (s.startsWith("[L]")) { large = true; s = s.substring(3); }
            }

            return new Line(s, bold, large, center, right, isLR, leftText, rightText);
        }
    }
}
