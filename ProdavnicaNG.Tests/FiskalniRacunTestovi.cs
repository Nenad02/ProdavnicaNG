using System.Globalization;
using ProdavnicaNG.Logika;
using ProdavnicaNG.Models;
using Xunit;

namespace ProdavnicaNG.Tests;

public class FiskalniRacunTestovi
{
    [Fact]
    public void Formatiraj_PrikazujeStavkeIUkupno()
    {
        var stavke = new List<StavkaRacuna>
        {
            new(1, "Banane", 130m, 2m, "kg"),
            new(2, "Ananas", 130m, 1m, "kg"),
            new(3, "Krompir beli", 60m, 0.95m, "kg"),
            new(4, "Krompir crveni", 58m, 3m, "kg"),
        };

        var tekst = FiskalniRacun.Formatiraj(stavke, CultureInfo.InvariantCulture);

        Assert.Contains("Banane\t260.00", tekst);
        Assert.Contains("Krompir beli\t57.00", tekst);
        Assert.Contains("Krompir crveni\t174.00", tekst);
        Assert.Contains("Ukupno", tekst);
        Assert.Contains("621.00", tekst);
    }

    [Fact]
    public void Formatiraj_PrazanRacun_PrikazujeNulaUkupno()
    {
        var tekst = FiskalniRacun.Formatiraj(Array.Empty<StavkaRacuna>(), CultureInfo.InvariantCulture);
        Assert.Contains("0.00", tekst);
    }

    [Fact]
    public void StavkaRacuna_ToString_PrikazujeNazivKolicinuIMeru()
    {
        var stavka = new StavkaRacuna(1, "Banane", 130m, 2m, "kg");
        Assert.Contains("Banane(", stavka.ToString());
        Assert.Contains("kg)", stavka.ToString());
    }

    [Fact]
    public void StavkaRacuna_Iznos_JeCenaPutaKolicina()
    {
        Assert.Equal(57m, new StavkaRacuna(3, "Krompir beli", 60m, 0.95m).Iznos);
    }
}
