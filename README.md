# Kursy walut NBP dla Magic

Gotowe helpery i snippety do pobierania kursow walut z publicznego API NBP.

Najwazniejsze pliki:

- `NbpRates.cs` - helper C# do wklejenia lub dodania do projektu Magic.
- `snippets/magic-csharp.cs` - krotki przyklad uzycia w snippecie C#.
- `snippets/magic-vbnet.vb` - przyklad dla VB.NET.
- `nbp_rates.py` - pierwotna wersja Python, zostawiona jako referencja.

## C# w Magic

Dodaj `NbpRates.cs` do projektu albo wklej klase do miejsca dostepnego dla snippetu.
Potem w snippecie C# mozesz uzyc:

```csharp
var eur = NbpRates.GetMidRate("EUR");
Console.WriteLine(eur);
```

Kurs z konkretnej daty:

```csharp
var eur = NbpRates.GetMidRate("EUR", "2026-07-09");
Console.WriteLine(eur);
```

Tabela C, czyli kurs kupna i sprzedazy:

```csharp
var usd = NbpRates.GetRate("USD", "C");
Console.WriteLine(usd.Bid + " / " + usd.Ask);
```

## Python

```python
from nbp_rates import fetch_nbp_rate

rate = fetch_nbp_rate("EUR")
print(rate.mid)
```

Kod C# i VB.NET korzysta tylko ze standardowych bibliotek .NET.
