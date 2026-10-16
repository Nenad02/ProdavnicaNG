using ProdavnicaNG.Logika;
using ProdavnicaNG.Models;
using Xunit;

namespace ProdavnicaNG.Tests;

public class KorpaTestovi
{
    private static Artikal Banane => new(1, 1, "Banane", 130m, "kg");
    private static Artikal Ananas => new(2, 1, "Ananas", 130m, "kg");
    private static Artikal KrompirBeli => new(3, 1, "Krompir beli", 60m, "kg");
    private static Artikal KrompirCrveni => new(4, 1, "Krompir crveni", 58m, "kg");

    [Fact]
    public void NovaKorpa_JePrazna()
    {
        var korpa = new Korpa();
        Assert.True(korpa.Prazna);
        Assert.Equal(0m, korpa.Ukupno);
    }

    [Fact]
    public void Dodaj_NoviArtikal_DodajeStavkuIRacunaUkupno()
    {
        var korpa = new Korpa();
        korpa.Dodaj(Banane, 2m);

        Assert.Single(korpa.Stavke);
        Assert.Equal(260m, korpa.Ukupno);
        Assert.False(korpa.Prazna);
    }

    [Fact]
    public void Dodaj_IstiArtikalDvaPuta_SabiraKolicinu()
    {
        var korpa = new Korpa();
        korpa.Dodaj(Banane, 2m);
        korpa.Dodaj(Banane, 1m);

        Assert.Single(korpa.Stavke);
        Assert.Equal(3m, korpa.Stavke[0].Kolicina);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Dodaj_NulaIliManje_BacaIzuzetak(int kolicina)
    {
        var korpa = new Korpa();
        Assert.Throws<ArgumentOutOfRangeException>(() => korpa.Dodaj(Banane, kolicina));
        Assert.True(korpa.Prazna);
    }

    [Fact]
    public void Ukupno_PrimerSaSlikeIzZadatka_Iznosi621()
    {
        var korpa = new Korpa();
        korpa.Dodaj(Banane, 2m);
        korpa.Dodaj(Ananas, 1m);
        korpa.Dodaj(KrompirBeli, 0.95m);
        korpa.Dodaj(KrompirCrveni, 3m);

        Assert.Equal(621m, korpa.Ukupno);
    }

    [Fact]
    public void Ukloni_BrisePoIndeksu()
    {
        var korpa = new Korpa();
        korpa.Dodaj(Banane, 1m);
        korpa.Dodaj(Ananas, 1m);

        korpa.Ukloni(0);

        Assert.Single(korpa.Stavke);
        Assert.Equal("Ananas", korpa.Stavke[0].Naziv);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void Ukloni_NepostojeciIndeks_BacaIzuzetak(int indeks)
    {
        var korpa = new Korpa();
        korpa.Dodaj(Banane, 1m);
        Assert.Throws<ArgumentOutOfRangeException>(() => korpa.Ukloni(indeks));
    }

    [Fact]
    public void Isprazni_BriseSveStavke()
    {
        var korpa = new Korpa();
        korpa.Dodaj(Banane, 1m);
        korpa.Isprazni();

        Assert.True(korpa.Prazna);
        Assert.Equal(0m, korpa.Ukupno);
    }
}
