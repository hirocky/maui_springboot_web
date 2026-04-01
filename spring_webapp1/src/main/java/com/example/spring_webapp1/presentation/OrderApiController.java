package com.example.spring_webapp1.presentation;

import com.example.spring_webapp1.application.order.OrderData;
import com.example.spring_webapp1.application.order.OrderItemData;
import com.example.spring_webapp1.application.order.PrintOrderReceiptUseCase;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

@RestController
@RequestMapping("/api/orders")
public class OrderApiController {

    private static final Logger log = LoggerFactory.getLogger(OrderApiController.class);

    private final PrintOrderReceiptUseCase printOrderReceiptUseCase;

    public OrderApiController(PrintOrderReceiptUseCase printOrderReceiptUseCase) {
        this.printOrderReceiptUseCase = printOrderReceiptUseCase;
    }

    // ---- HTTP リクエスト DTO ----

    public record OrderItem(String id, String name, int price, int qty) {}

    public record OrderRequest(
            String orderNumber,
            List<OrderItem> items,
            int subtotal,
            int tax,
            int total
    ) {}

    // ---- エンドポイント ----

    @PostMapping
    public ResponseEntity<Map<String, String>> placeOrder(@RequestBody OrderRequest request) {
        log.info("注文受付 #{} 合計¥{} {}品目",
                request.orderNumber(), request.total(), request.items().size());
        request.items().forEach(item ->
                log.info("  - {} x{} ¥{}", item.name(), item.qty(), item.price() * item.qty()));

        OrderData order = toOrderData(request);

        try {
            printOrderReceiptUseCase.execute(order);
        } catch (Exception e) {
            log.error("レシート印刷エラー: {}", e.getMessage(), e);
        }

        return ResponseEntity.ok(Map.of(
                "orderNumber", request.orderNumber(),
                "status", "received"
        ));
    }

    private static OrderData toOrderData(OrderRequest request) {
        List<OrderItemData> items = request.items().stream()
                .map(i -> new OrderItemData(i.id(), i.name(), i.price(), i.qty()))
                .collect(Collectors.toList());
        return new OrderData(request.orderNumber(), items, request.subtotal(), request.tax(), request.total());
    }
}
