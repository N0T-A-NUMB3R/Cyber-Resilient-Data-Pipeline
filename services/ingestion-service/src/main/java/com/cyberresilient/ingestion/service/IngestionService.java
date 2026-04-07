package com.cyberresilient.ingestion.service;

import com.cyberresilient.ingestion.dto.CyberEventRequest;
import com.cyberresilient.ingestion.messaging.CyberEventPublisher;
import com.cyberresilient.ingestion.model.SensorLog;
import com.cyberresilient.ingestion.model.SensorLog.IngestionStatus;
import com.cyberresilient.ingestion.model.SensorLogRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
public class IngestionService {

    private static final Logger log = LoggerFactory.getLogger(IngestionService.class);

    private final SensorLogRepository repo;
    private final CyberEventPublisher publisher;

    public IngestionService(SensorLogRepository repo, CyberEventPublisher publisher) {
        this.repo = repo;
        this.publisher = publisher;
    }

    @Transactional
    public void ingest(CyberEventRequest evento) {
        if (repo.existsByEventId(evento.eventId())) {
            log.warn("[DUPLICATO] eventId={}", evento.eventId());
            return;
        }

        var registro = new SensorLog(
                evento.eventId(), evento.sourceIp(),
                evento.eventType(), evento.timestamp(),
                IngestionStatus.QUEUED);

        try {
            publisher.publish(evento);
            repo.save(registro);
            log.info("[ACQUISITO] eventId={} tipo={}", evento.eventId(), evento.eventType());
        } catch (Exception ex) {
            registro.setStatus(IngestionStatus.FAILED);
            repo.save(registro);
            log.error("[ERRORE_CODA] eventId={} errore={}", evento.eventId(), ex.getMessage());
            throw ex;
        }
    }
}
