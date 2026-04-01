package com.example.spring_webapp1.infrastructure.printing;

import com.example.spring_webapp1.domain.printing.IPrinterDiscovery;
import org.springframework.stereotype.Component;

import javax.print.PrintService;
import javax.print.PrintServiceLookup;
import java.util.Arrays;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;

/**
 * Java AWT PrintServiceLookup を使ってインストール済みプリンターを列挙する実装。
 */
@Component
public class JavaPrinterDiscovery implements IPrinterDiscovery {

    @Override
    public List<String> getInstalledPrinterNames() {
        return Arrays.stream(PrintServiceLookup.lookupPrintServices(null, null))
                .map(PrintService::getName)
                .sorted()
                .collect(Collectors.toList());
    }

    /**
     * キーワードを含む最初のプリンターを返す。見つからない場合は空。
     */
    public Optional<PrintService> findByKeyword(String keyword) {
        return Arrays.stream(PrintServiceLookup.lookupPrintServices(null, null))
                .filter(svc -> svc.getName().contains(keyword))
                .findFirst();
    }
}
