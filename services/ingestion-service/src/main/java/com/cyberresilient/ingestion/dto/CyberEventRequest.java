package com.cyberresilient.ingestion.dto;

import jakarta.validation.constraints.*;
import java.time.Instant;

/**
 * Inbound payload from perimeter sensors (firewalls, IDS, OSINT tools).
 * All fields are validated before any IO operation is performed.
 */
public record CyberEventRequest(

        @NotBlank(message = "eventId must not be blank")
        @Size(max = 64)
        String eventId,

        @NotBlank(message = "sourceIp must not be blank")
        @Pattern(
            regexp = "^(([0-9]{1,3}\\.){3}[0-9]{1,3}|([0-9a-fA-F]{1,4}:){1,7}[0-9a-fA-F]{1,4})$",
            message = "sourceIp must be a valid IPv4 or IPv6 address"
        )
        String sourceIp,

        @NotBlank(message = "eventType must not be blank")
        @Size(max = 128)
        String eventType,

        @NotNull(message = "timestamp must not be null")
        Instant timestamp,

        @Size(max = 4096)
        String payload
) {}
