package com.cyberresilient.ingestion.model;

import jakarta.persistence.*;
import java.time.Instant;

/**
 * Persists the ingestion checkpoint for each received event.
 * Stored in SQLite to track state and allow deduplication.
 */
@Entity
@Table(name = "sensor_log", indexes = {
    @Index(name = "idx_event_id", columnList = "event_id", unique = true),
    @Index(name = "idx_ingested_at", columnList = "ingested_at")
})
public class SensorLog {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(name = "event_id", nullable = false, unique = true, length = 64)
    private String eventId;

    @Column(name = "source_ip", nullable = false, length = 45)
    private String sourceIp;

    @Column(name = "event_type", nullable = false, length = 128)
    private String eventType;

    @Column(name = "event_timestamp", nullable = false)
    private Instant eventTimestamp;

    @Column(name = "ingested_at", nullable = false)
    private Instant ingestedAt;

    @Enumerated(EnumType.STRING)
    @Column(name = "status", nullable = false, length = 16)
    private IngestionStatus status;

    public enum IngestionStatus { QUEUED, FAILED }

    // --- constructors ---

    protected SensorLog() {}

    public SensorLog(String eventId, String sourceIp, String eventType,
                     Instant eventTimestamp, IngestionStatus status) {
        this.eventId        = eventId;
        this.sourceIp       = sourceIp;
        this.eventType      = eventType;
        this.eventTimestamp = eventTimestamp;
        this.ingestedAt     = Instant.now();
        this.status         = status;
    }

    // --- getters ---

    public Long getId()                  { return id; }
    public String getEventId()           { return eventId; }
    public String getSourceIp()          { return sourceIp; }
    public String getEventType()         { return eventType; }
    public Instant getEventTimestamp()   { return eventTimestamp; }
    public Instant getIngestedAt()       { return ingestedAt; }
    public IngestionStatus getStatus()   { return status; }
    public void setStatus(IngestionStatus s) { this.status = s; }
}
