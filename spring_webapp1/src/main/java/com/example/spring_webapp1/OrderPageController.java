package com.example.spring_webapp1;

import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.GetMapping;

@Controller
public class OrderPageController {

    @GetMapping("/order")
    public String orderPage() {
        // 静的な POS 画面にフォワードするだけ（サーバー側ロジックなし）
        return "forward:/order/index.html";
    }
}

