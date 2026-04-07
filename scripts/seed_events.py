#!/usr/bin/env python3
"""
Genera eventi di sicurezza verso il gateway YARP.
Uso: python seed_events.py [--url URL] [--count N] [--delay SEC]
"""

import argparse
import json
import random
import sys
import time
import uuid
from datetime import datetime, timezone, timedelta
from urllib.request import urlopen, Request
from urllib.error import HTTPError, URLError

GATEWAY_URL = "http://localhost:8080/api/v1/ingest"

TIPI_EVENTO = [
    "BRUTE_FORCE",
    "PORT_SCAN",
    "UNAUTHORIZED_ACCESS",
    "SQL_INJECTION",
    "XSS",
    "RECON",
    "NORMAL_TRAFFIC",
    "HEARTBEAT",
]

INDIRIZZI_IP = [
    "192.168.1.{}".format(i) for i in range(1, 30)
] + [
    "10.0.0.{}".format(i) for i in range(1, 20)
] + [
    "203.0.113.{}".format(i) for i in range(1, 15)  # IP esterni (TEST-NET-3)
]

PAYLOAD_TEMPLATE = {
    "BRUTE_FORCE":         "failed_logins={attempts}; target=ssh; user={user}",
    "PORT_SCAN":           "scanned_ports={ports}; protocol=TCP; stealth=true",
    "UNAUTHORIZED_ACCESS": "resource={resource}; method=GET; status=403",
    "SQL_INJECTION":       "query=\" OR '1'='1'; input_field={field}",
    "XSS":                 "payload=<script>alert(1)</script>; field={field}",
    "RECON":               "user_agent=nmap/7.94; probes={n}",
    "NORMAL_TRAFFIC":      "bytes_in={b_in}; bytes_out={b_out}; proto=HTTPS",
    "HEARTBEAT":           "sensor_id={sid}; status=alive",
}


def genera_payload(tipo: str) -> str:
    t = PAYLOAD_TEMPLATE.get(tipo, "raw_data={}".format(uuid.uuid4().hex[:16]))
    return t.format(
        attempts=random.randint(5, 500),
        user=random.choice(["root", "admin", "ubuntu", "pi"]),
        ports=random.randint(100, 65000),
        resource=random.choice(["/admin", "/etc/passwd", "/wp-login.php"]),
        field=random.choice(["username", "email", "search"]),
        n=random.randint(10, 1000),
        b_in=random.randint(100, 100_000),
        b_out=random.randint(100, 50_000),
        sid="SID-{:04d}".format(random.randint(1, 50)),
    )


def genera_evento(anomalo: bool = False) -> dict:
    tipo = random.choice(TIPI_EVENTO)
    base_time = datetime.now(timezone.utc) - timedelta(seconds=random.randint(0, 3600))

    evento = {
        "eventId":   str(uuid.uuid4()),
        "sourceIp":  random.choice(INDIRIZZI_IP),
        "eventType": tipo,
        "timestamp": base_time.isoformat(),
        "payload":   genera_payload(tipo),
    }

    if anomalo:
        scelta = random.randint(0, 4)
        if scelta == 0:
            del evento["eventId"]                  # campo obbligatorio mancante
        elif scelta == 1:
            evento["sourceIp"] = "NON_UN_IP_VALIDO" # IP non valido
        elif scelta == 2:
            evento["eventType"] = "A" * 200        # stringa troppo lunga
        elif scelta == 3:
            evento["timestamp"] = "non-una-data"   # timestamp non valido
        elif scelta == 4:
            evento["payload"] = "X" * 5000         # payload oltre il limite

    return evento


def invia(url: str, evento: dict) -> tuple[int, str]:
    body = json.dumps(evento).encode("utf-8")
    req = Request(
        url,
        data=body,
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    try:
        with urlopen(req, timeout=5) as resp:
            return resp.status, resp.read().decode()
    except HTTPError as e:
        return e.code, e.read().decode()
    except URLError as e:
        return 0, str(e.reason)


def main():
    parser = argparse.ArgumentParser(description="Seed eventi cyber verso il gateway")
    parser.add_argument("--url",   default=GATEWAY_URL, help="URL endpoint ingest")
    parser.add_argument("--count", type=int, default=30, help="Numero totale di eventi")
    parser.add_argument("--delay", type=float, default=0.3, help="Pausa tra richieste (sec)")
    parser.add_argument("--anomaly-ratio", type=float, default=0.3,
                        help="Frazione di eventi anomali (0.0 - 1.0)")
    args = parser.parse_args()

    ok = errori_validazione = errori_rete = 0

    print(f"Invio {args.count} eventi verso {args.url}")
    print(f"Ratio anomalie: {args.anomaly_ratio:.0%}\n")

    for i in range(1, args.count + 1):
        anomalo = random.random() < args.anomaly_ratio
        evento = genera_evento(anomalo=anomalo)
        stato, risposta = invia(args.url, evento)

        etichetta = "ANOMALO" if anomalo else "VALIDO "
        tipo = evento.get("eventType", "???")[:20]

        if stato == 202:
            ok += 1
            print(f"[{i:03d}] {etichetta} | {tipo:<20} | {stato} ACQUISITO")
        elif stato in (400, 422):
            errori_validazione += 1
            print(f"[{i:03d}] {etichetta} | {tipo:<20} | {stato} VALIDAZIONE FALLITA")
        elif stato == 0:
            errori_rete += 1
            print(f"[{i:03d}] {etichetta} | {tipo:<20} | ERRORE RETE: {risposta}")
        else:
            print(f"[{i:03d}] {etichetta} | {tipo:<20} | {stato} {risposta[:60]}")

        time.sleep(args.delay)

    print(f"\n--- Riepilogo ---")
    print(f"  Acquisiti:           {ok}")
    print(f"  Errori validazione:  {errori_validazione}")
    print(f"  Errori rete:         {errori_rete}")
    print(f"  Totale:              {args.count}")

    if errori_rete > 0:
        print("\nATTENZIONE: errori di rete. Assicurati che i container siano avviati.")
        sys.exit(1)


if __name__ == "__main__":
    main()
