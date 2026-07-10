using System;
using System.Globalization;
using System.Net;
using System.Xml.Linq;

public class NbpRate
{
    public string Currency { get; set; }
    public string Code { get; set; }
    public string Table { get; set; }
    public string EffectiveDate { get; set; }
    public decimal? Mid { get; set; }
    public decimal? Bid { get; set; }
    public decimal? Ask { get; set; }
}

public static class NbpRates
{
    private const string ApiBaseUrl = "https://api.nbp.pl/api/exchangerates/rates";

    public static decimal GetMidRate(string currencyCode, string rateDate = null)
    {
        var rate = GetRate(currencyCode, "A", rateDate);
        if (!rate.Mid.HasValue)
        {
            throw new InvalidOperationException("NBP response does not contain a mid rate.");
        }

        return rate.Mid.Value;
    }

    public static NbpRate GetRate(string currencyCode, string table = "A", string rateDate = null)
    {
        var code = NormalizeCurrencyCode(currencyCode);
        var nbpTable = NormalizeTable(table);
        var url = BuildUrl(code, nbpTable, rateDate);
        var xml = DownloadString(url, code, nbpTable, rateDate);
        return ParseRate(xml);
    }

    public static string BuildUrl(string currencyCode, string table = "A", string rateDate = null)
    {
        var code = NormalizeCurrencyCode(currencyCode);
        var nbpTable = NormalizeTable(table);
        var datePart = String.IsNullOrWhiteSpace(rateDate) ? "" : "/" + rateDate.Trim();

        return ApiBaseUrl
            + "/"
            + Uri.EscapeDataString(nbpTable)
            + "/"
            + Uri.EscapeDataString(code)
            + datePart
            + "/?format=xml";
    }

    private static string DownloadString(string url, string code, string table, string rateDate)
    {
        try
        {
            using (var client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.Accept, "application/xml");
                return client.DownloadString(url);
            }
        }
        catch (WebException ex)
        {
            var status = ex.Response as HttpWebResponse;
            if (status != null && status.StatusCode == HttpStatusCode.NotFound)
            {
                var suffix = String.IsNullOrWhiteSpace(rateDate) ? "." : " on " + rateDate.Trim() + ".";
                throw new InvalidOperationException(
                    "NBP has no rate for currency " + code + " in table " + table + suffix,
                    ex);
            }

            throw new InvalidOperationException("Could not download NBP exchange rate.", ex);
        }
    }

    private static NbpRate ParseRate(string xml)
    {
        var root = XDocument.Parse(xml).Root;
        if (root == null)
        {
            throw new InvalidOperationException("NBP response is empty.");
        }

        var rateNode = root.Element("Rates") == null ? null : root.Element("Rates").Element("Rate");
        if (rateNode == null)
        {
            throw new InvalidOperationException("NBP response does not contain exchange rates.");
        }

        return new NbpRate
        {
            Currency = ReadText(root, "Currency"),
            Code = ReadText(root, "Code"),
            Table = ReadText(root, "Table"),
            EffectiveDate = ReadText(rateNode, "EffectiveDate"),
            Mid = ReadDecimal(rateNode, "Mid"),
            Bid = ReadDecimal(rateNode, "Bid"),
            Ask = ReadDecimal(rateNode, "Ask")
        };
    }

    private static string NormalizeCurrencyCode(string currencyCode)
    {
        if (String.IsNullOrWhiteSpace(currencyCode))
        {
            throw new ArgumentException("Currency code is required.", "currencyCode");
        }

        var code = currencyCode.Trim().ToUpperInvariant();
        if (code.Length != 3)
        {
            throw new ArgumentException("Currency code must have 3 letters.", "currencyCode");
        }

        return code;
    }

    private static string NormalizeTable(string table)
    {
        if (String.IsNullOrWhiteSpace(table))
        {
            return "A";
        }

        var nbpTable = table.Trim().ToUpperInvariant();
        if (nbpTable != "A" && nbpTable != "B" && nbpTable != "C")
        {
            throw new ArgumentException("NBP table must be A, B, or C.", "table");
        }

        return nbpTable;
    }

    private static string ReadText(XElement element, string name)
    {
        var child = element.Element(name);
        return child == null ? null : child.Value;
    }

    private static decimal? ReadDecimal(XElement element, string name)
    {
        var child = element.Element(name);
        if (child == null || String.IsNullOrWhiteSpace(child.Value))
        {
            return null;
        }

        return Decimal.Parse(child.Value, CultureInfo.InvariantCulture);
    }
}
