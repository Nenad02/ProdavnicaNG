using ProdavnicaNG.Models;
using Xunit;

namespace ProdavnicaNG.Tests;

public class StatistikaArtiklaTestovi
{
    [Fact]
    public void Procenat_JeDeoDnevneProdajeUUkupnoj()
    {
        Assert.Equal(25m, new StatistikaArtikla("Crni luk", 2m, 8m).Procenat);
    }

    [Fact]
    public void Procenat_BezUkupneProdaje_JeNula()
    {
        Assert.Equal(0m, new StatistikaArtikla("Crni luk", 0m, 0m).Procenat);
    }

    [Fact]
    public void Procenat_NikadNijeVeciOdSto()
    {
        Assert.Equal(100m, new StatistikaArtikla("Crni luk", 5m, 3m).Procenat);
    }

    [Fact]
    public void Procenat_ArtikalProdatSamoTogDana_JeSto()
    {
        Assert.Equal(100m, new StatistikaArtikla("Vino", 2m, 2m).Procenat);
    }
}
