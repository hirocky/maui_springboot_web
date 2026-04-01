package com.example.spring_webapp1.domain.printing;

/**
 * フォーマット済みレシートの1行を表す値オブジェクト。
 * マーカー（[B][C][R][L][LR]）解析済みの状態を保持する。
 */
public record ReceiptLine(
        String draw,
        boolean bold,
        boolean large,
        boolean center,
        boolean right,
        boolean isLeftRight,
        String leftText,
        String rightText
) {}
