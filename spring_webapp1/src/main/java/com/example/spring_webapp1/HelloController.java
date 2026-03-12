package com.example.spring_webapp1;

import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class HelloController {

    @GetMapping("/hello")
    public String hello() {
        return "Hello from spring_webapp1だよー。がんばろうー！なかなかのノウハウやな。。";
    }

    @GetMapping("/")
    public String root() {
        return "spring_webapp1 is running. Try /hello";
    }
}
