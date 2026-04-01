package com.example.spring_webapp1.application.printing;

import com.example.spring_webapp1.domain.printing.ReceiptDocument;
import com.example.spring_webapp1.domain.printing.ReceiptLine;
import org.springframework.stereotype.Component;

import java.util.Arrays;
import java.util.List;
import java.util.stream.Collectors;

/**
 * レシートテキスト（マーカー付き文字列）を {@link ReceiptDocument} に変換するパーサー。
 * <p>対応マーカー: [B] 太字, [L] 大字, [C] 中央, [R] 右寄せ, [LR]左テキスト|右テキスト</p>
 */
@Component
public class ReceiptTextParser {

    public ReceiptDocument parse(String text) {
        return parse(text, null);
    }

    public ReceiptDocument parse(String text, String logoPath) {
        List<ReceiptLine> lines = Arrays.stream(
                text.replace("\r\n", "\n").replace("\r", "\n").split("\n", -1)
        ).map(this::parseLine).collect(Collectors.toList());
        return new ReceiptDocument(lines, logoPath);
    }

    private ReceiptLine parseLine(String raw) {
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

        return new ReceiptLine(s, bold, large, center, right, isLR, leftText, rightText);
    }
}
