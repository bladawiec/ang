' Self-contained VB.NET snippet for Magic.

Imports System
Imports System.Globalization
Imports System.Net
Imports System.Xml.Linq

Dim eur As Decimal = GetNbpMidRate("EUR")
Console.WriteLine("EUR: " & eur)

Function GetNbpMidRate(currencyCode As String) As Decimal
    Dim code = currencyCode.Trim().ToUpperInvariant()
    Dim url = "https://api.nbp.pl/api/exchangerates/rates/A/" & Uri.EscapeDataString(code) & "/?format=xml"

    Using client As New WebClient()
        client.Headers.Add(HttpRequestHeader.Accept, "application/xml")
        Dim xml = client.DownloadString(url)
        Dim doc = XDocument.Parse(xml)
        Dim midText = doc.Root.Element("Rates").Element("Rate").Element("Mid").Value

        Return Decimal.Parse(midText, CultureInfo.InvariantCulture)
    End Using
End Function
