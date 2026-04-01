package com.example.spring_webapp1.infrastructure.printing;

import com.example.spring_webapp1.domain.printing.IReceiptPrinter;
import com.example.spring_webapp1.domain.printing.ReceiptDocument;
import com.example.spring_webapp1.domain.printing.ReceiptLine;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;

import javax.print.PrintService;
import java.awt.*;
import java.awt.print.*;
import java.util.Arrays;
import java.util.Set;
import java.util.HashSet;

/**
 * Java AWT PrinterJob を使って EPSON TM レシートプリンターに出力する {@link IReceiptPrinter} 実装。
 * テキスト解析は Application 層の {@code ReceiptTextParser} が担い、
 * このクラスは {@link ReceiptDocument} の描画に専念する。
 */
@Component
public class EpsonAwtReceiptPrinter implements IReceiptPrinter {

    private static final Logger log = LoggerFactory.getLogger(EpsonAwtReceiptPrinter.class);

    private static final int FONT_SIZE_NORMAL = 9;
    private static final int FONT_SIZE_LARGE = 14;

    private final JavaPrinterDiscovery printerDiscovery;

    public EpsonAwtReceiptPrinter(JavaPrinterDiscovery printerDiscovery) {
        this.printerDiscovery = printerDiscovery;
    }

    @Override
    public void printAndCut(ReceiptDocument document, String printerKeyword) throws PrinterException {
        PrintService printer = printerDiscovery.findByKeyword(printerKeyword)
                .orElseThrow(() -> {
                    log.warn("プリンター「{}」が見つかりません。利用可能: {}",
                            printerKeyword, printerDiscovery.getInstalledPrinterNames());
                    return new PrinterException("プリンター「" + printerKeyword + "」が見つかりません");
                });

        PrinterJob job = PrinterJob.getPrinterJob();
        job.setPrintService(printer);
        PageFormat pf = job.defaultPage();
        job.setPrintable(new ReceiptPrintable(document), pf);
        job.print();
        log.info("レシート印刷完了");
    }

    // -------------------------------------------------------------------------
    // Printable 実装（描画ロジック）
    // -------------------------------------------------------------------------

    private static class ReceiptPrintable implements Printable {

        private final ReceiptDocument document;

        ReceiptPrintable(ReceiptDocument document) {
            this.document = document;
        }

        @Override
        public int print(Graphics graphics, PageFormat pageFormat, int pageIndex) {
            if (pageIndex > 0) return NO_SUCH_PAGE;

            Graphics2D g = (Graphics2D) graphics;
            g.setRenderingHint(RenderingHints.KEY_TEXT_ANTIALIASING, RenderingHints.VALUE_TEXT_ANTIALIAS_ON);
            g.setColor(Color.BLACK);

            String fontName = resolveFontName(g, "MS Gothic", "MS ゴシック", Font.MONOSPACED);
            Font fNormal   = new Font(fontName, Font.PLAIN, FONT_SIZE_NORMAL);
            Font fBold     = new Font(fontName, Font.BOLD,  FONT_SIZE_NORMAL);
            Font fNormalLg = new Font(fontName, Font.PLAIN, FONT_SIZE_LARGE);
            Font fBoldLg   = new Font(fontName, Font.BOLD,  FONT_SIZE_LARGE);

            float xOrigin   = (float) pageFormat.getImageableX();
            float yOrigin   = (float) pageFormat.getImageableY();
            float pageWidth = (float) pageFormat.getImageableWidth();

            g.setFont(fNormal);
            FontMetrics fmBase = g.getFontMetrics();
            float y = yOrigin + fmBase.getAscent();

            for (ReceiptLine line : document.lines()) {
                Font font = pickFont(line.bold(), line.large(), fNormal, fBold, fNormalLg, fBoldLg);
                g.setFont(font);
                FontMetrics fm = g.getFontMetrics();

                if (line.isLeftRight()) {
                    float xRight   = xOrigin + pageWidth - fm.stringWidth(line.rightText());
                    float minXRight = xOrigin + fm.stringWidth(line.leftText()) + 4;
                    g.drawString(line.leftText(), xOrigin, y);
                    g.drawString(line.rightText(), Math.max(minXRight, xRight), y);
                } else {
                    float x;
                    if (line.center()) {
                        x = xOrigin + (pageWidth - fm.stringWidth(line.draw())) / 2f;
                    } else if (line.right()) {
                        x = xOrigin + pageWidth - fm.stringWidth(line.draw());
                    } else {
                        x = xOrigin;
                    }
                    g.drawString(line.draw(), Math.max(xOrigin, x), y);
                }

                y += fm.getHeight();
            }

            return PAGE_EXISTS;
        }

        private static String resolveFontName(Graphics2D g, String... candidates) {
            Set<String> available = new HashSet<>(
                    Arrays.asList(GraphicsEnvironment.getLocalGraphicsEnvironment().getAvailableFontFamilyNames()));
            for (String c : candidates) {
                if (available.contains(c)) return c;
            }
            return candidates[candidates.length - 1];
        }

        private static Font pickFont(boolean bold, boolean large, Font fN, Font fB, Font fNL, Font fBL) {
            if (large) return bold ? fBL : fNL;
            return bold ? fB : fN;
        }
    }
}
