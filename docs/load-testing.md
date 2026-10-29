# Small-scale load test

This test sends concurrent HTTP requests to the running web application. It uses
Python's standard library; no load-testing package is required. The script
reports its results directly in the terminal.

## Test configuration

| Setting | Value |
|---|---|
| Application environment | Development |
| Target | `http://127.0.0.1:5118/` (home page) |
| HTTP method | GET |
| Concurrent virtual users | 5 |
| Requests per user | 20 |
| Total requests | 100 |
| Start pattern | All five users start together; each sends requests sequentially |
| Per-request timeout | 15 seconds |
| Success criteria | All requests return HTTP 200 with no request errors |

## Run the test

From the repository root, start the app in one terminal:

```sh
ASPNETCORE_ENVIRONMENT=Development dotnet run --no-launch-profile --urls http://127.0.0.1:5118
```

In another terminal, run the load test:

```sh
python3 scripts/load_test.py
```

Defaults are five concurrent users making 20 homepage requests each (100 total).
To test a different URL or load, pass options such as:

```sh
python3 scripts/load_test.py --url http://127.0.0.1:5118/ --users 5 --requests-per-user 20
```

The command prints the target, request count, duration, throughput, HTTP 200
count, failures, latency summary (minimum, mean, p50, p95, maximum), and response
size. The test exits unsuccessfully if any request fails or returns a status
other than HTTP 200.

## Recorded output

The following is the output captured from a local run against the homepage on
2026-10-29. Timing and throughput can vary between runs and machines.

```text
Target: http://127.0.0.1:5118/
Virtual users: 5; requests/user: 20; total: 100
Duration: 0.218s; throughput: 458.49 requests/s
HTTP 200: 100/100; failures: 0
Latency ms: min=0.74; mean=10.59; p50=1.17; p95=5.25; max=184.37
Response bytes (first): 16653
```

This is a small localhost test, not a production-capacity benchmark. It does
not include external network latency or represent real user behavior.
