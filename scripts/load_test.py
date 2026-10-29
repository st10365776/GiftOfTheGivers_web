#!/usr/bin/env python3
"""Run a small concurrent HTTP load test using only the Python standard library."""

import argparse
import concurrent.futures
import math
import statistics
import threading
import time
import urllib.error
import urllib.request


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--url", default="http://127.0.0.1:5118/")
    parser.add_argument("--users", type=int, default=5)
    parser.add_argument("--requests-per-user", type=int, default=20)
    args = parser.parse_args()
    if args.users < 1 or args.requests_per_user < 1:
        parser.error("--users and --requests-per-user must be positive")

    start_barrier = threading.Barrier(args.users)
    results = []
    lock = threading.Lock()

    def virtual_user():
        local_results = []
        start_barrier.wait()
        for _ in range(args.requests_per_user):
            request_start = time.perf_counter()
            status = 0
            response_bytes = 0
            error = None
            try:
                with urllib.request.urlopen(args.url, timeout=15) as response:
                    status = response.status
                    response_bytes = len(response.read())
            except Exception as exc:
                error = f"{type(exc).__name__}: {exc}"
            local_results.append(
                (time.perf_counter() - request_start, status, response_bytes, error)
            )
        with lock:
            results.extend(local_results)

    test_start = time.perf_counter()
    with concurrent.futures.ThreadPoolExecutor(max_workers=args.users) as pool:
        list(pool.map(lambda _: virtual_user(), range(args.users)))
    elapsed = time.perf_counter() - test_start

    latencies = sorted(result[0] for result in results)
    failures = [result for result in results if result[3] or result[1] != 200]
    request_count = len(results)
    p50 = latencies[math.ceil(0.50 * request_count) - 1] * 1000
    p95 = latencies[math.ceil(0.95 * request_count) - 1] * 1000

    print(f"Target: {args.url}")
    print(
        f"Virtual users: {args.users}; requests/user: {args.requests_per_user}; "
        f"total: {request_count}"
    )
    print(
        f"Duration: {elapsed:.3f}s; throughput: "
        f"{request_count / elapsed:.2f} requests/s"
    )
    success_count = sum(not result[3] and result[1] == 200 for result in results)
    print(f"HTTP 200: {success_count}/{request_count}; failures: {len(failures)}")
    print(
        f"Latency ms: min={latencies[0] * 1000:.2f}; "
        f"mean={statistics.mean(latencies) * 1000:.2f}; "
        f"p50={p50:.2f}; p95={p95:.2f}; max={latencies[-1] * 1000:.2f}"
    )
    print(f"Response bytes (first): {results[0][2]}")
    for failure in failures[:5]:
        print(f"Failure: status={failure[1]}, error={failure[3]}")

    return 1 if failures else 0


if __name__ == "__main__":
    raise SystemExit(main())
