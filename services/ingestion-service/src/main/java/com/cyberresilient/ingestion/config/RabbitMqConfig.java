package com.cyberresilient.ingestion.config;

import org.springframework.amqp.core.*;
import org.springframework.amqp.rabbit.config.RetryInterceptorBuilder;
import org.springframework.amqp.rabbit.config.SimpleRabbitListenerContainerFactory;
import org.springframework.amqp.rabbit.connection.ConnectionFactory;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.amqp.support.converter.Jackson2JsonMessageConverter;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.retry.interceptor.RetryOperationsInterceptor;

/**
 * Declares the Topic Exchange, main queue and Dead-Letter Queue.
 * All declarations are idempotent (durable=true).
 */
@Configuration
public class RabbitMqConfig {

    @Value("${cyber.rabbitmq.exchange}")
    private String exchange;

    @Value("${cyber.rabbitmq.queue}")
    private String queue;

    @Value("${cyber.rabbitmq.routing-key}")
    private String routingKey;

    @Value("${cyber.rabbitmq.dead-letter-exchange}")
    private String dlx;

    @Value("${cyber.rabbitmq.dead-letter-queue}")
    private String dlq;

    @Bean
    public TopicExchange cyberEventsExchange() {
        return ExchangeBuilder.topicExchange(exchange).durable(true).build();
    }

    @Bean
    public TopicExchange deadLetterExchange() {
        return ExchangeBuilder.topicExchange(dlx).durable(true).build();
    }

    @Bean
    public Queue cyberEventsQueue() {
        return QueueBuilder.durable(queue)
                .withArgument("x-dead-letter-exchange", dlx)
                .withArgument("x-message-ttl", 86_400_000) // 24 h
                .build();
    }

    @Bean
    public Queue deadLetterQueue() {
        return QueueBuilder.durable(dlq).build();
    }

    @Bean
    public Binding cyberEventsBinding(Queue cyberEventsQueue, TopicExchange cyberEventsExchange) {
        return BindingBuilder.bind(cyberEventsQueue).to(cyberEventsExchange).with(routingKey);
    }

    @Bean
    public Binding dlqBinding(Queue deadLetterQueue, TopicExchange deadLetterExchange) {
        return BindingBuilder.bind(deadLetterQueue).to(deadLetterExchange).with("#");
    }

    @Bean
    public Jackson2JsonMessageConverter messageConverter() {
        return new Jackson2JsonMessageConverter();
    }

    @Bean
    public RabbitTemplate rabbitTemplate(ConnectionFactory cf) {
        var template = new RabbitTemplate(cf);
        template.setMessageConverter(messageConverter());
        template.setMandatory(true);
        return template;
    }
}
