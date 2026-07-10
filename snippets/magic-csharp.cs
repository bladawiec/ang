// Add NbpRates.cs to your Magic project first, then use this snippet.

var eur = NbpRates.GetMidRate("EUR");
Console.WriteLine("EUR: " + eur);

var eurFromDate = NbpRates.GetMidRate("EUR", "2026-07-09");
Console.WriteLine("EUR 2026-07-09: " + eurFromDate);

var usdTableC = NbpRates.GetRate("USD", "C");
Console.WriteLine("USD bid/ask: " + usdTableC.Bid + " / " + usdTableC.Ask);
