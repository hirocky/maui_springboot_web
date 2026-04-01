package com.example.spring_webapp1.domain.printing;

import java.util.List;

/**
 * 印字対象のレシート全体を表す値オブジェクト。
 */
public record ReceiptDocument(
        List<ReceiptLine> lines,
        String logoPath
) {
    public ReceiptDocument(List<ReceiptLine> lines) {
        this(lines, null);
    }
}
