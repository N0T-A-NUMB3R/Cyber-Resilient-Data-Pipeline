package com.cyberresilient.ingestion.model;

import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface SensorLogRepository extends JpaRepository<SensorLog, Long> {
    Optional<SensorLog> findByEventId(String eventId);
    boolean existsByEventId(String eventId);
}
