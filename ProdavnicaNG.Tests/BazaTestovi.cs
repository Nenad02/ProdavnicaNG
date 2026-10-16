using Microsoft.Data.Sqlite;
using ProdavnicaNG.Data;
using ProdavnicaNG.Models;
using Xunit;

namespace ProdavnicaNG.Tests;

/// <summary>Testovi sa pravom (SQLite) bazom. Svi su u jednoj klasi da se ne izvrsavaju paralelno.</summary>
public class BazaTestovi
{
    public BazaTestovi()
    {
        // xUnit pravi novu instancu klase za svaki test, pa svaki test krece od prazne baze
        Baza.Izvrsi("DELETE FROM racun_artikal");
        Baza.Izvrsi("DELETE FROM racuni");
        Baza.Izvrsi("DELETE FROM artikli");
        Baza.Izvrsi("DELETE FROM kategorije");
    }

    private static Kategorija NovaKategorija(string naziv)
    {
        KategorijaRepo.Dodaj(naziv);
        return KategorijaRepo.Sve().Single(k => k.Naziv == naziv);
    }

    private static Artikal NoviArtikal(Kategorija k, string naziv, decimal cena, string? mera = "kg")
    {
        ArtikalRepo.Dodaj(k.Id, naziv, cena, mera);
        return ArtikalRepo.PoKategoriji(k.Id).Single(a => a.Naziv == naziv);
    }

    private static List<StavkaRacuna> Stavke(Artikal a, decimal kolicina) =>
        new() { new StavkaRacuna(a.Id, a.Naziv, a.Cena, kolicina, a.Mera) };

    // ---------- kategorije ----------

    [Fact]
    public void Kategorija_Dodaj_PojavljujeSeUListi()
    {
        KategorijaRepo.Dodaj("Pijaca");
        Assert.Equal("Pijaca", Assert.Single(KategorijaRepo.Sve()).Naziv);
    }

    [Fact]
    public void Kategorija_Sve_VracaSortiranoPoNazivu()
    {
        KategorijaRepo.Dodaj("Tehnika");
        KategorijaRepo.Dodaj("Kozmetika");
        Assert.Equal(new[] { "Kozmetika", "Tehnika" }, KategorijaRepo.Sve().Select(k => k.Naziv));
    }

    [Fact]
    public void Kategorija_Izmeni_MenjaNaziv()
    {
        var k = NovaKategorija("Pijaca");
        KategorijaRepo.Izmeni(k.Id, "Pijaca i voće");
        Assert.Equal("Pijaca i voće", Assert.Single(KategorijaRepo.Sve()).Naziv);
    }

    [Fact]
    public void Kategorija_Obrisi_Uklanja()
    {
        var k = NovaKategorija("Pijaca");
        KategorijaRepo.Obrisi(k.Id);
        Assert.Empty(KategorijaRepo.Sve());
    }

    [Fact]
    public void Kategorija_ObrisiSaArtiklima_BacaIzuzetakStranogKljuca()
    {
        var k = NovaKategorija("Pijaca");
        NoviArtikal(k, "Banane", 130m);

        var greska = Assert.Throws<SqliteException>(() => KategorijaRepo.Obrisi(k.Id));

        Assert.Equal(787, greska.SqliteExtendedErrorCode);
        Assert.Single(KategorijaRepo.Sve());
    }

    // ---------- artikli ----------

    [Fact]
    public void Artikal_Dodaj_VracaSvePoljaPoKategoriji()
    {
        var k = NovaKategorija("Pijaca");
        var a = NoviArtikal(k, "Banane", 130.5m, "kg");

        Assert.Equal(k.Id, a.KategorijaId);
        Assert.Equal(130.5m, a.Cena);
        Assert.Equal("kg", a.Mera);
    }

    [Fact]
    public void Artikal_BezMere_VracaNull()
    {
        var k = NovaKategorija("Pijaca");
        Assert.Null(NoviArtikal(k, "Banane", 130m, null).Mera);
    }

    [Fact]
    public void Artikal_PoKategoriji_VracaSamoArtikleIzTeKategorije()
    {
        var pijaca = NovaKategorija("Pijaca");
        var hemija = NovaKategorija("Kućna hemija");
        NoviArtikal(pijaca, "Banane", 130m);
        NoviArtikal(hemija, "Domestos", 189m, "kom");

        Assert.Equal("Banane", Assert.Single(ArtikalRepo.PoKategoriji(pijaca.Id)).Naziv);
    }

    [Fact]
    public void Artikal_Svi_VracaArtikleIzSvihKategorija()
    {
        var pijaca = NovaKategorija("Pijaca");
        var hemija = NovaKategorija("Kućna hemija");
        NoviArtikal(pijaca, "Banane", 130m);
        NoviArtikal(hemija, "Domestos", 189m, "kom");

        Assert.Equal(2, ArtikalRepo.Svi().Count);
    }

    [Fact]
    public void Artikal_Izmeni_MenjaSvaPolja()
    {
        var pijaca = NovaKategorija("Pijaca");
        var hemija = NovaKategorija("Kućna hemija");
        var a = NoviArtikal(pijaca, "Banane", 130m);

        ArtikalRepo.Izmeni(a.Id, hemija.Id, "Domestos", 189m, "kom");

        var izmenjen = Assert.Single(ArtikalRepo.Svi());
        Assert.Equal(new Artikal(a.Id, hemija.Id, "Domestos", 189m, "kom"), izmenjen);
    }

    [Fact]
    public void Artikal_Obrisi_Uklanja()
    {
        var k = NovaKategorija("Pijaca");
        var a = NoviArtikal(k, "Banane", 130m);

        ArtikalRepo.Obrisi(a.Id);

        Assert.Empty(ArtikalRepo.Svi());
    }

    [Fact]
    public void Artikal_NepostojecaKategorija_BacaIzuzetak()
    {
        Assert.Throws<SqliteException>(() => ArtikalRepo.Dodaj(999, "Banane", 130m, "kg"));
    }

    // ---------- racuni ----------

    [Fact]
    public void Racun_Sacuvaj_UpisujeRacunIStavke()
    {
        var a = NoviArtikal(NovaKategorija("Pijaca"), "Banane", 130m);
        var stavke = Stavke(a, 2m);

        int id = RacunRepo.Sacuvaj(new DateTime(2025, 4, 5), stavke);

        var racun = Assert.Single(RacunRepo.Svi());
        Assert.Equal(id, racun.Id);
        Assert.Equal(new DateTime(2025, 4, 5), racun.Datum);
        Assert.Equal(260m, racun.Ukupno);
        Assert.True(stavke.SequenceEqual(racun.Stavke));
    }

    [Fact]
    public void Racun_SacuvajPrazanRacun_BacaIzuzetak()
    {
        Assert.Throws<ArgumentException>(() => RacunRepo.Sacuvaj(DateTime.Today, new List<StavkaRacuna>()));
    }

    [Fact]
    public void Racun_NepostojeciArtikal_PonistavaCeoUpis()
    {
        var losa = new List<StavkaRacuna> { new(999, "Nepostojeci", 10m, 1m) };

        Assert.Throws<SqliteException>(() => RacunRepo.Sacuvaj(DateTime.Today, losa));

        Assert.Empty(RacunRepo.Svi());   // racun nije ostao bez stavki
    }

    [Fact]
    public void Racun_PromenaCeneArtikla_NeMenjaStariRacun()
    {
        var k = NovaKategorija("Pijaca");
        var a = NoviArtikal(k, "Banane", 130m);
        RacunRepo.Sacuvaj(new DateTime(2025, 4, 5), Stavke(a, 1m));

        ArtikalRepo.Izmeni(a.Id, k.Id, "Banane", 500m, "kg");

        Assert.Equal(130m, RacunRepo.Svi().Single().Stavke.Single().Cena);
    }

    [Fact]
    public void Artikal_ObrisiVecProdat_BacaIzuzetak()
    {
        var a = NoviArtikal(NovaKategorija("Pijaca"), "Banane", 130m);
        RacunRepo.Sacuvaj(new DateTime(2025, 4, 5), Stavke(a, 1m));

        Assert.Throws<SqliteException>(() => ArtikalRepo.Obrisi(a.Id));
    }

    // ---------- statistika ----------

    [Fact]
    public void Statistika_ZaDatum_VracaKolicinuTogDanaIUkupnu()
    {
        var a = NoviArtikal(NovaKategorija("Pijaca"), "Banane", 130m);
        RacunRepo.Sacuvaj(new DateTime(2025, 4, 5), Stavke(a, 2m));
        RacunRepo.Sacuvaj(new DateTime(2025, 4, 6), Stavke(a, 3m));

        var s = Assert.Single(StatistikaRepo.ZaDatum(new DateTime(2025, 4, 5)));

        Assert.Equal("Banane", s.Naziv);
        Assert.Equal(2m, s.ProdatoNaDan);
        Assert.Equal(5m, s.ProdatoUkupno);
        Assert.Equal(40m, s.Procenat);
    }

    [Fact]
    public void Statistika_ZaDatum_SabiraViseRacunaIstogDana()
    {
        var a = NoviArtikal(NovaKategorija("Pijaca"), "Banane", 130m);
        RacunRepo.Sacuvaj(new DateTime(2025, 4, 5), Stavke(a, 1.5m));
        RacunRepo.Sacuvaj(new DateTime(2025, 4, 5), Stavke(a, 0.5m));

        Assert.Equal(2m, StatistikaRepo.ZaDatum(new DateTime(2025, 4, 5)).Single().ProdatoNaDan);
    }

    [Fact]
    public void Statistika_DatumBezProdaje_JePrazna()
    {
        var a = NoviArtikal(NovaKategorija("Pijaca"), "Banane", 130m);
        RacunRepo.Sacuvaj(new DateTime(2025, 4, 5), Stavke(a, 2m));

        Assert.Empty(StatistikaRepo.ZaDatum(new DateTime(2025, 4, 7)));
    }

    [Fact]
    public void Statistika_Vise_Artikala_SortiraPoDnevnojProdajiOpadajuce()
    {
        var k = NovaKategorija("Pijaca");
        var luk = NoviArtikal(k, "Crni luk", 80m);
        var krompir = NoviArtikal(k, "Krompir crveni", 58m);
        var datum = new DateTime(2025, 4, 5);
        RacunRepo.Sacuvaj(datum, Stavke(krompir, 6m));
        RacunRepo.Sacuvaj(datum, Stavke(luk, 7m));

        var rezultat = StatistikaRepo.ZaDatum(datum);

        Assert.Equal(new[] { "Crni luk", "Krompir crveni" }, rezultat.Select(s => s.Naziv));
    }

    // ---------- XML iz baze ----------

    [Fact]
    public void Xml_SnimiIzBazePaUcitaj_VracaIstePodatkeKaoBaza()
    {
        var a = NoviArtikal(NovaKategorija("Pijaca"), "Banane", 130m);
        RacunRepo.Sacuvaj(new DateTime(2025, 4, 5), Stavke(a, 2m));
        var putanja = Path.Combine(Path.GetTempPath(), $"iz_baze_{Guid.NewGuid():N}.xml");
        try
        {
            XmlArhiva.SnimiIzBaze(putanja);
            var ucitano = XmlArhiva.Ucitaj(putanja);

            Assert.True(KategorijaRepo.Sve().SequenceEqual(ucitano.Kategorije));
            Assert.True(ArtikalRepo.Svi().SequenceEqual(ucitano.Artikli));
            var racun = Assert.Single(ucitano.Racuni);
            Assert.Equal(260m, racun.Ukupno);
            Assert.True(Stavke(a, 2m).SequenceEqual(racun.Stavke));
        }
        finally { File.Delete(putanja); }
    }

    [Fact]
    public void Xml_OsveziKopiju_PraviFajlNaPodrazumevanojPutanji()
    {
        NovaKategorija("Pijaca");

        XmlArhiva.OsveziKopiju();

        Assert.True(File.Exists(Putanje.XmlFajl));
    }
}
