package com.example.spring_webapp1.application.order;

import com.example.spring_webapp1.application.printing.PrintReceiptUseCase;
import org.springframework.stereotype.Component;

import java.awt.print.PrinterException;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;

/**
 * 注文レシート印刷ユースケース。
 * 注文データをレシートテキストに変換して {@link PrintReceiptUseCase} に委譲する。
 */
@Component
public class PrintOrderReceiptUseCase {
    private final PrintReceiptUseCase printReceiptUseCase;

    public PrintOrderReceiptUseCase(PrintReceiptUseCase printReceiptUseCase) {
        this.printReceiptUseCase = printReceiptUseCase;
    }

    public void execute(OrderData order) throws PrinterException {
        String text = buildReceiptText(order);
        printReceiptUseCase.execute(text);
    }

    private static String buildReceiptText(OrderData order) {
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
}
