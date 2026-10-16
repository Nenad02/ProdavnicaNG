using ProdavnicaNG.Logika;
using ProdavnicaNG.Models;
using Xunit;

namespace ProdavnicaNG.Tests;

public class ZabranaAlkoholaTestovi
{
    private static DateTime UVreme(int sat, int minut) => new(2025, 4, 5, sat, minut, 0);

    [Theory]
    [InlineData("Alkoholna pića", 22, 0, true)]
    [InlineData("Alkoholna pića", 23, 59, true)]
    [InlineData("Alkoholna pića", 21, 59, false)]
    [InlineData("Alkoholna pića", 12, 0, false)]
    [InlineData("alkoholna pica", 22, 30, true)]   // bez dijakritika i malim slovima
    [InlineData("  Alkoholna pića ", 22, 30, true)]
    [InlineData("Pijaca", 23, 0, false)]
    [InlineData("Kućna hemija", 23, 0, false)]
    public void JeZabranjeno_ZavisiOdKategorijeIVremena(string kategorija, int sat, int minut, bool ocekivano)
    {
        Assert.Equal(ocekivano, ZabranaAlkohola.JeZabranjeno(kategorija, UVreme(sat, minut)));
    }

    [Theory]
    [InlineData("Alkoholna pića", true)]
    [InlineData("alkoholna pica", true)]
    [InlineData("Pijaca", false)]
    public void JeAlkohol_PrepoznajeKategoriju(string kategorija, bool ocekivano)
    {
        Assert.Equal(ocekivano, ZabranaAlkohola.JeAlkohol(kategorija));
    }

    [Fact]
    public void Proveri_Zabranjeno_PokrecaDogadjajIVracaFalse()
    {
        var zabrana = new ZabranaAlkohola();
        var vino = new Artikal(7, 3, "Vino Kremen 0.75 L", 900m, "kom");
        DateTime? primljenoVreme = null;
        Artikal? primljenArtikal = null;
        zabrana.ZabranjenPokusaj += (vreme, artikal) => { primljenoVreme = vreme; primljenArtikal = artikal; };

        bool dozvoljeno = zabrana.Proveri("Alkoholna pića", vino, UVreme(22, 15));

        Assert.False(dozvoljeno);
        Assert.Equal(UVreme(22, 15), primljenoVreme);
        Assert.Equal(vino, primljenArtikal);
    }

    [Fact]
    public void Proveri_Dozvoljeno_NePokrecaDogadjaj()
    {
        var zabrana = new ZabranaAlkohola();
        int pokretanja = 0;
        zabrana.ZabranjenPokusaj += (_, _) => pokretanja++;

        bool dozvoljeno = zabrana.Proveri("Alkoholna pića", new Artikal(7, 3, "Vino", 900m, "kom"), UVreme(15, 0));

        Assert.True(dozvoljeno);
        Assert.Equal(0, pokretanja);
    }

    [Fact]
    public void Proveri_BezPretplatnika_NeBacaIzuzetak()
    {
        var zabrana = new ZabranaAlkohola();
        bool dozvoljeno = zabrana.Proveri("Alkoholna pića", new Artikal(7, 3, "Vino", 900m, "kom"), UVreme(23, 0));
        Assert.False(dozvoljeno);
    }
}
