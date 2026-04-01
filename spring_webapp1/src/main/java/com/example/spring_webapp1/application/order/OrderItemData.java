package com.example.spring_webapp1.application.order;

/**
 * 注文明細を表すアプリケーション層 DTO。
 */
public record OrderItemData(String id, String name, int price, int qty) {}
