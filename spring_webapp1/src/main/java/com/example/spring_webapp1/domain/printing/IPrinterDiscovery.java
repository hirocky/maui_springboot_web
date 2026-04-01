package com.example.spring_webapp1.domain.printing;

import java.util.List;

/**
 * インストール済みプリンターを列挙するポート。
 */
public interface IPrinterDiscovery {
    List<String> getInstalledPrinterNames();
}
