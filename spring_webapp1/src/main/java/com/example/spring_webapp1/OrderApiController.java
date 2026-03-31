package com.example.spring_webapp1;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;
import java.util.Map;

@RestController
@RequestMapping("/api/orders")
public class OrderApiController {

    private static final Logger log = LoggerFactory.getLogger(OrderApiController.class);

    private final ReceiptPrintService receiptPrintService;

    public OrderApiController(ReceiptPrintService receiptPrintService) {
        this.receiptPrintService = receiptPrintService;
    }

    public record OrderItem(String id, String name, int price, int qty) {}

    public record OrderRequest(
            String orderNumber,
            List<OrderItem> items,
            int subtotal,
            int tax,
            int total
    ) {}

    @PostMapping
    public ResponseEntity<Map<String, String>> placeOrder(@RequestBody OrderRequest order) {
        log.info("注文受付 #{} 合計¥{} {}品目", order.orderNumber(), order.total(), order.items().size());
        order.items().forEach(item ->
                log.info("  - {} x{} ¥{}", item.name(), item.qty(), item.price() * item.qty())
        );
        receiptPrintService.printOrder(order);
        return ResponseEntity.ok(Map.of("orderNumber", order.orderNumber(), "status", "received"));
    }
}
