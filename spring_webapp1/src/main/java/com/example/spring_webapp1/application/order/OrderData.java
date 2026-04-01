package com.example.spring_webapp1.application.order;

import java.util.List;

/**
 * 注文を表すアプリケーション層 DTO。Presentation 層の HTTP DTO とは分離する。
 */
public record OrderData(
        String orderNumber,
        List<OrderItemData> items,
        int subtotal,
        int tax,
        int total
) {}
