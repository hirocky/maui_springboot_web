package com.example.spring_webapp1.application.printing;

import com.example.spring_webapp1.domain.printing.IReceiptPrinter;
import com.example.spring_webapp1.domain.printing.ReceiptDocument;
import org.springframework.stereotype.Component;

import java.awt.print.PrinterException;

/**
 * レシート印刷ユースケース。
 * テキストを解析して {@link ReceiptDocument} を作り、{@link IReceiptPrinter} に委譲する。
 */
@Component
public class PrintReceiptUseCase {

    private final IReceiptPrinter printer;
    private final ReceiptTextParser parser;

    public PrintReceiptUseCase(IReceiptPrinter printer, ReceiptTextParser parser) {
        this.printer = printer;
        this.parser = parser;
    }

    public void execute(String text, String printerKeyword) throws PrinterException {
        execute(text, printerKeyword, null);
    }

    public void execute(String text, String printerKeyword, String logoPath) throws PrinterException {
        ReceiptDocument document = parser.parse(text, logoPath);
        printer.printAndCut(document, printerKeyword);
    }
}
