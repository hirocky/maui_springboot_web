package com.example.spring_webapp1.presentation;

import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.GetMapping;

@Controller
public class OrderPageController {

    @GetMapping("/order")
    public String orderPage() {
        return "forward:/order/index.html";
    }
}
