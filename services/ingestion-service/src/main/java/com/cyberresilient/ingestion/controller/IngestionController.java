package com.cyberresilient.ingestion.controller;

import com.cyberresilient.ingestion.dto.CyberEventRequest;
import com.cyberresilient.ingestion.service.IngestionService;
import jakarta.validation.Valid;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.Map;

@RestController
@RequestMapping("/api/v1/ingest")
public class IngestionController {

    private final IngestionService servizio;

    public IngestionController(IngestionService servizio) {
        this.servizio = servizio;
    }

    @PostMapping
    public ResponseEntity<Map<String, String>> acquisisci(@Valid @RequestBody CyberEventRequest richiesta) {
        servizio.ingest(richiesta);
        return ResponseEntity
                .status(HttpStatus.ACCEPTED)
                .body(Map.of("stato", "ACQUISITO", "eventId", richiesta.eventId()));
    }

    @GetMapping("/ping")
    public ResponseEntity<Map<String, String>> ping() {
        return ResponseEntity.ok(Map.of("stato", "ok", "servizio", "ingestion-service"));
    }
}
