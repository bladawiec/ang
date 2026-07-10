# Kursy walut NBP

Prosty modul Pythona do pobierania kursow walut z publicznego API NBP.

## Uzycie

```python
from nbp_rates import fetch_nbp_rate

rate = fetch_nbp_rate("EUR")
print(rate.mid)

rate_c = fetch_nbp_rate("USD", table="C", rate_date="2026-07-09")
print(rate_c.bid, rate_c.ask)
```

## Co zwraca funkcja

`fetch_nbp_rate` zwraca obiekt `NbpRate` z polami:

- `currency`
- `code`
- `table`
- `effective_date`
- `mid` dla tabel A i B
- `bid` oraz `ask` dla tabeli C

Modul korzysta tylko z biblioteki standardowej Pythona.
