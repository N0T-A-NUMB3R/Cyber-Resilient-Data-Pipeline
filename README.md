# Cyber-Resilient Data Pipeline

Pipeline event-driven per l'ingestion e l'analisi di eventi di sicurezza in tempo reale.  
Architettura containerizzata, pronta per il cloud, costruita su Java, .NET 8 e RabbitMQ.
<img width="1407" height="768" alt="Gemini_Generated_Image_ntg6lwntg6lwntg6" src="https://github.com/user-attachments/assets/2c713181-3bf5-437a-be31-f461d7eadd30" />

---

## Prerequisiti

| Strumento | Versione minima |
|---|---|
| [Docker Desktop](https://www.docker.com/products/docker-desktop) | 4.x |
| Python (opzionale, per lo script di seed) | 3.9+ |

Non è necessario installare Java, .NET, MongoDB o RabbitMQ: Docker li scarica automaticamente.

---

## Avvio rapido

```bash
# 1. Copia e configura le variabili d'ambiente
cp .env.example .env

# 2. Avvia tutto
docker-compose up --build

# 3. (facoltativo) Genera eventi di test
python scripts/seed_events.py --count 50
```

---

## Architettura

```
Client / Sensore
    │
    ▼ HTTPS
YARP Gateway  :8080          ← entry point unico
    │
    ├─► Ingestion Service  :8081   (Java / Spring Boot 3)
    │       │ valida → pubblica su RabbitMQ → salva checkpoint su SQLite
    │       ▼
    │   RabbitMQ  :15672
    │       │
    │       ▼
    └─► Analysis Service  :8082   (.NET 8 + MassTransit)
            │ analizza → rileva minacce → salva su MongoDB
            ▼
        MongoDB  :27017
```


---

## Servizi e porte

| Servizio | URL locale |
|---|---|
| YARP Gateway (entry point) | http://localhost:8080 |
| Ingestion Service | http://localhost:8081 |
| Analysis Service | http://localhost:8082 |
| RabbitMQ Management UI | http://localhost:15672 |
| Grafana | http://localhost:3000 |
| Prometheus | http://localhost:9090 |
| MongoDB | mongodb://localhost:27017 |

---

## Struttura del progetto

```
CyberResilience/
├── gateway/                   YARP API Gateway (.NET 8)
├── services/
│   ├── ingestion-service/     Microservizio Java / Spring Boot
│   └── analysis-service/      Microservizio .NET 8
├── infrastructure/
│   ├── rabbitmq/              Configurazione broker e definizioni
│   ├── mongodb/               Script di inizializzazione
│   └── prometheus/            Configurazione scraping metriche
├── scripts/
│   └── seed_events.py         Generatore di eventi di test
├── docker-compose.yml
└── .env.example
```

---

## Inviare un evento manualmente

```bash
curl -X POST http://localhost:8080/api/v1/ingest \
  -H "Content-Type: application/json" \
  -d '{
    "eventId":   "evt-001",
    "sourceIp":  "192.168.1.10",
    "eventType": "BRUTE_FORCE",
    "timestamp": "2026-04-05T10:00:00Z",
    "payload":   "failed_logins=150; target=ssh; user=root"
  }'
```

Risposta attesa: `202 ACQUISITO`

---

## Script di seed

```bash
# 30 eventi (default), 30% anomali
python scripts/seed_events.py

# 100 eventi, 50% anomali, pausa 0.1s
python scripts/seed_events.py --count 100 --anomaly-ratio 0.5 --delay 0.1

# Contro un gateway remoto
python scripts/seed_events.py --url http://mio-server:8080/api/v1/ingest
```

Gli eventi anomali testano la validazione dell'API (IP non validi, campi mancanti, payload troppo lunghi, ecc.).

---

## Flusso dei dati

```
[ACQUISITO]  → evento ricevuto e validato dall'Ingestion Service
[QUEUED]     → evento pubblicato su RabbitMQ
[ANALYZED]   → evento processato e salvato su MongoDB dall'Analysis Service
[DUPLICATO]  → eventId già presente, scartato
[ERRORE]     → fallimento di pubblicazione, salvato su SQLite con status FAILED
```

---

## Tipi di minaccia riconosciuti

| Tipo evento | Livello minaccia |
|---|---|
| BRUTE_FORCE | CRITICAL |
| SQL_INJECTION | CRITICAL |
| PORT_SCAN | HIGH |
| UNAUTHORIZED_ACCESS | HIGH |
| XSS | MEDIUM |
| RECON | LOW |
| NORMAL_TRAFFIC, HEARTBEAT | NONE |

