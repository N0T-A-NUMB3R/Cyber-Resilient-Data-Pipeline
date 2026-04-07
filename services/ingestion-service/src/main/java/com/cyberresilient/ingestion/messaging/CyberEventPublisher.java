package com.cyberresilient.ingestion.messaging;

import com.cyberresilient.ingestion.dto.CyberEventRequest;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

/**
 * Publishes validated cyber events to the RabbitMQ Topic Exchange.
 * Retry with exponential back-off is configured in application.yml.
 *
 * Telemetry checkpoint: [QUEUED] — event published to broker.
 */
@Component
public class CyberEventPublisher {

    private static final Logger log = LoggerFactory.getLogger(CyberEventPublisher.class);

    private final RabbitTemplate rabbitTemplate;

    @Value("${cyber.rabbitmq.exchange}")
    private String exchange;

    @Value("${cyber.rabbitmq.routing-key}")
    private String routingKey;

    public CyberEventPublisher(RabbitTemplate rabbitTemplate) {
        this.rabbitTemplate = rabbitTemplate;
    }

    /**
     * Publishes the event payload to the broker.
     * Throws {@link org.springframework.amqp.AmqpException} on failure —
     * caller is responsible for updating the SensorLog status to FAILED.
     */
    public void publish(CyberEventRequest event) {
        log.info("[QUEUED] eventId={} sourceIp={} type={} routing={}",
                event.eventId(), event.sourceIp(), event.eventType(), routingKey);

        rabbitTemplate.convertAndSend(exchange, routingKey, event);
    }
}
