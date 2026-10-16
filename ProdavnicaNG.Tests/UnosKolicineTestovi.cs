using ProdavnicaNG.Logika;
using Xunit;

namespace ProdavnicaNG.Tests;

public class UnosKolicineTestovi
{
    private static UnosKolicine SaArtiklom()
    {
        var unos = new UnosKolicine();
        unos.IzaberiArtikal("Banane");
        return unos;
    }

    [Fact]
    public void BezArtikla_PrikazJePrazan()
    {
        var unos = new UnosKolicine();
        Assert.Equal("", unos.Prikaz);
        Assert.False(unos.ImaArtikal);
    }

    [Fact]
    public void SamoArtikal_KolicinaJeJedan()
    {
        var unos = SaArtiklom();
        Assert.Equal("Banane", unos.Prikaz);
        Assert.Equal(1m, unos.Vrednost);
    }

    [Fact]
    public void XPaCifra_PrikazujeIKolicinu()
    {
        var unos = SaArtiklom();
        unos.PritisniX();
        unos.Cifra('2');

        Assert.Equal("Banane x 2", unos.Prikaz);
        Assert.Equal(2m, unos.Vrednost);
    }

    [Fact]
    public void XBezCifara_KolicinaJeJedan()
    {
        var unos = SaArtiklom();
        unos.PritisniX();
        Assert.Equal(1m, unos.Vrednost);
    }

    [Fact]
    public void CifraBezX_SeIgnorise()
    {
        var unos = SaArtiklom();
        unos.Cifra('5');
        Assert.Equal("", unos.Kolicina);
    }

    [Fact]
    public void PritisniX_BezArtikla_NemaEfekta()
    {
        var unos = new UnosKolicine();
        unos.PritisniX();
        Assert.False(unos.Mnozenje);
    }

    [Fact]
    public void Decimalna_Kolicina_SeParsiraSaZarezom()
    {
        var unos = SaArtiklom();
        unos.PritisniX();
        foreach (var znak in "0,95") unos.Cifra(znak);

        Assert.Equal("0,95", unos.Kolicina);
        Assert.Equal(0.95m, unos.Vrednost);
    }

    [Fact]
    public void ZarezNaPocetku_DajeNulaZarez()
    {
        var unos = SaArtiklom();
        unos.PritisniX();
        unos.Cifra(',');
        Assert.Equal("0,", unos.Kolicina);
    }

    [Fact]
    public void DrugiZarez_SeIgnorise()
    {
        var unos = SaArtiklom();
        unos.PritisniX();
        foreach (var znak in "1,5,") unos.Cifra(znak);
        Assert.Equal("1,5", unos.Kolicina);
    }

    [Fact]
    public void VodecaNula_SeZamenjujeCifrom()
    {
        var unos = SaArtiklom();
        unos.PritisniX();
        unos.Cifra('0');
        unos.Cifra('7');
        Assert.Equal("7", unos.Kolicina);
    }

    [Fact]
    public void Slovo_SeIgnorise()
    {
        var unos = SaArtiklom();
        unos.PritisniX();
        unos.Cifra('a');
        Assert.Equal("", unos.Kolicina);
    }

    [Fact]
    public void Kolicina_JeOgranicenaNaOsamZnakova()
    {
        var unos = SaArtiklom();
        unos.PritisniX();
        foreach (var znak in "1234567890") unos.Cifra(znak);
        Assert.Equal(8, unos.Kolicina.Length);
    }

    [Fact]
    public void NoviIzborArtikla_PonistavaKolicinu()
    {
        var unos = SaArtiklom();
        unos.PritisniX();
        unos.Cifra('3');

        unos.IzaberiArtikal("Ananas");

        Assert.Equal("Ananas", unos.Prikaz);
        Assert.False(unos.Mnozenje);
        Assert.Equal("", unos.Kolicina);
    }

    [Fact]
    public void Ponisti_VracaUPocetnoStanje()
    {
        var unos = SaArtiklom();
        unos.PritisniX();
        unos.Cifra('3');

        unos.Ponisti();

        Assert.Null(unos.Naziv);
        Assert.Equal("", unos.Prikaz);
        Assert.False(unos.Mnozenje);
    }
}
