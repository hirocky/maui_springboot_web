package com.example.spring_webapp1.domain.printing;

import java.awt.print.PrinterException;

/**
 * レシートプリンターへの出力を抽象化するポート（依存性逆転の原則）。
 * Infrastructure 層に具体実装を持つ。
 */
public interface IReceiptPrinter {
    void printAndCut(ReceiptDocument document, String printerKeyword) throws PrinterException;
}
