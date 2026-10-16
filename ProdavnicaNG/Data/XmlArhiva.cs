using System.Globalization;
using System.Xml.Linq;
using ProdavnicaNG.Models;

namespace ProdavnicaNG.Data;

internal record Arhiva(
    IReadOnlyList<Kategorija> Kategorije,
    IReadOnlyList<Artikal> Artikli,
    IReadOnlyList<Racun> Racuni);

/// <summary>Dodatna XML kopija kategorija, artikala i zatvorenih racuna (pored baze).</summary>
internal static class XmlArhiva
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static void SnimiIzBaze(string? putanja = null) =>
        Snimi(new Arhiva(KategorijaRepo.Sve(), ArtikalRepo.Svi(), RacunRepo.Svi()), putanja ?? Putanje.XmlFajl);

    /// <summary>Osvezi XML kopiju. Ako fajl trenutno nije dostupan, rad programa se ne prekida.</summary>
    public static void OsveziKopiju()
    {
        try { SnimiIzBaze(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }

    public static void Snimi(Arhiva arhiva, string putanja)
    {
        var dokument = new XDocument(
            new XElement("prodavnica",
                new XElement("kategorije",
                    arhiva.Kategorije.Select(k => new XElement("kategorija",
                        new XAttribute("id", k.Id),
                        new XAttribute("naziv", k.Naziv)))),
                new XElement("artikli",
                    arhiva.Artikli.Select(a => new XElement("artikal",
                        new XAttribute("id", a.Id),
                        new XAttribute("kategorija", a.KategorijaId),
                        new XAttribute("naziv", a.Naziv),
                        new XAttribute("cena", a.Cena.ToString(Inv)),
                        new XAttribute("mera", a.Mera ?? "")))),
                new XElement("racuni",
                    arhiva.Racuni.Select(r => new XElement("racun",
                        new XAttribute("id", r.Id),
                        new XAttribute("datum", r.Datum.ToString("yyyy-MM-dd", Inv)),
                        new XAttribute("ukupno", r.Ukupno.ToString(Inv)),
                        r.Stavke.Select(s => new XElement("stavka",
                            new XAttribute("artikal", s.ArtikalId),
                            new XAttribute("naziv", s.Naziv),
                            new XAttribute("cena", s.Cena.ToString(Inv)),
                            new XAttribute("kolicina", s.Kolicina.ToString(Inv)),
                            new XAttribute("mera", s.Mera ?? ""))))))));

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(putanja))!);
        dokument.Save(putanja);
    }

    public static Arhiva Ucitaj(string putanja)
    {
        var koren = XDocument.Load(putanja).Root
                    ?? throw new InvalidDataException("XML nema korenski element.");

        var kategorije = Elementi(koren, "kategorije", "kategorija")
            .Select(e => new Kategorija(Ceo(e, "id"), Tekst(e, "naziv")))
            .ToList();

        var artikli = Elementi(koren, "artikli", "artikal")
            .Select(e => new Artikal(Ceo(e, "id"), Ceo(e, "kategorija"), Tekst(e, "naziv"), Broj(e, "cena"), Mera(e)))
            .ToList();

        var racuni = Elementi(koren, "racuni", "racun")
            .Select(e => new Racun(
                Ceo(e, "id"),
                DateTime.ParseExact(Tekst(e, "datum"), "yyyy-MM-dd", Inv),
                Broj(e, "ukupno"),
                e.Elements("stavka")
                    .Select(s => new StavkaRacuna(Ceo(s, "artikal"), Tekst(s, "naziv"), Broj(s, "cena"), Broj(s, "kolicina"), Mera(s)))
                    .ToList()))
            .ToList();

        return new Arhiva(kategorije, artikli, racuni);
    }

    private static IEnumerable<XElement> Elementi(XElement koren, string grupa, string element) =>
        koren.Element(grupa)?.Elements(element) ?? Enumerable.Empty<XElement>();

    private static string Tekst(XElement e, string ime) =>
        (string?)e.Attribute(ime) ?? throw new InvalidDataException($"Nedostaje atribut '{ime}'.");

    private static int Ceo(XElement e, string ime) => int.Parse(Tekst(e, ime), Inv);

    private static decimal Broj(XElement e, string ime) => decimal.Parse(Tekst(e, ime), Inv);

    private static string? Mera(XElement e)
    {
        var mera = (string?)e.Attribute("mera");
        return string.IsNullOrEmpty(mera) ? null : mera;
    }
}
