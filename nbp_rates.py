from __future__ import annotations

import json
from dataclasses import dataclass
from datetime import date
from typing import Any, Literal
from urllib.error import HTTPError, URLError
from urllib.parse import quote
from urllib.request import Request, urlopen


NBP_API_BASE_URL = "https://api.nbp.pl/api/exchangerates/rates"
NbpTable = Literal["A", "B", "C"]


@dataclass(frozen=True)
class NbpRate:
    currency: str
    code: str
    table: str
    effective_date: str
    mid: float | None = None
    bid: float | None = None
    ask: float | None = None


def fetch_nbp_rate(
    currency_code: str,
    *,
    table: NbpTable = "A",
    rate_date: date | str | None = None,
    timeout: float = 10.0,
) -> NbpRate:
    """Fetch an exchange rate from the public NBP API.

    Args:
        currency_code: ISO 4217 currency code, for example "EUR", "USD" or "CHF".
        table: NBP table name. Tables A and B return `mid`; table C returns `bid`
            and `ask`.
        rate_date: Optional date in YYYY-MM-DD format. If omitted, NBP returns the
            latest available rate.
        timeout: Request timeout in seconds.

    Raises:
        ValueError: When the input is invalid or NBP has no rate for the query.
        RuntimeError: When the API cannot be reached.
    """
    code = currency_code.strip().upper()
    nbp_table = table.strip().upper()

    if len(code) != 3 or not code.isalpha():
        raise ValueError("currency_code must be a 3-letter ISO 4217 code.")

    if nbp_table not in {"A", "B", "C"}:
        raise ValueError("table must be one of: A, B, C.")

    date_part = _format_rate_date(rate_date)
    path_parts = [quote(nbp_table), quote(code)]
    if date_part is not None:
        path_parts.append(date_part)

    url = f"{NBP_API_BASE_URL}/{'/'.join(path_parts)}/?format=json"
    request = Request(url, headers={"Accept": "application/json"})

    try:
        with urlopen(request, timeout=timeout) as response:
            payload = json.loads(response.read().decode("utf-8"))
    except HTTPError as exc:
        if exc.code == 404:
            raise ValueError(
                f"NBP has no rate for currency {code} in table {nbp_table}"
                + (f" on {date_part}." if date_part else ".")
            ) from exc
        raise RuntimeError(f"NBP API returned HTTP {exc.code}.") from exc
    except URLError as exc:
        raise RuntimeError(f"Could not reach NBP API: {exc.reason}") from exc

    return _parse_nbp_rate(payload)


def _format_rate_date(rate_date: date | str | None) -> str | None:
    if rate_date is None:
        return None
    if isinstance(rate_date, date):
        return rate_date.isoformat()
    if isinstance(rate_date, str):
        return rate_date
    raise ValueError("rate_date must be a date, YYYY-MM-DD string, or None.")


def _parse_nbp_rate(payload: dict[str, Any]) -> NbpRate:
    rates = payload.get("rates")
    if not rates:
        raise ValueError("NBP API response did not contain exchange rates.")

    first_rate = rates[0]
    return NbpRate(
        currency=payload["currency"],
        code=payload["code"],
        table=payload["table"],
        effective_date=first_rate["effectiveDate"],
        mid=first_rate.get("mid"),
        bid=first_rate.get("bid"),
        ask=first_rate.get("ask"),
    )


if __name__ == "__main__":
    rate = fetch_nbp_rate("EUR")
    print(f"{rate.code} {rate.effective_date}: {rate.mid}")
