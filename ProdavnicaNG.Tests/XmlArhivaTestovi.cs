using ProdavnicaNG.Data;
using ProdavnicaNG.Models;
using Xunit;

namespace ProdavnicaNG.Tests;

public class XmlArhivaTestovi
{
    private static string NovaPutanja() => Path.Combine(Path.GetTempPath(), $"arhiva_{Guid.NewGuid():N}.xml");

    [Fact]
    public void SnimiPaUcitaj_VracaIstePodatke()
    {
        var putanja = NovaPutanja();
        try
        {
            var stavke = new List<StavkaRacuna>
            {
                new(1, "Banane", 130m, 2m, "kg"),
                new(2, "Krompir beli", 60m, 0.95m, null),
            };
            var arhiva = new Arhiva(
                new List<Kategorija> { new(1, "Pijaca"), new(2, "Alkoholna pića") },
                new List<Artikal> { new(1, 1, "Banane", 130m, "kg"), new(2, 1, "Krompir beli", 60m, null) },
                new List<Racun> { new(1, new DateTime(2025, 4, 5), 317m, stavke) });

            XmlArhiva.Snimi(arhiva, putanja);
            var ucitano = XmlArhiva.Ucitaj(putanja);

            Assert.True(arhiva.Kategorije.SequenceEqual(ucitano.Kategorije));
            Assert.True(arhiva.Artikli.SequenceEqual(ucitano.Artikli));
            var racun = Assert.Single(ucitano.Racuni);
            Assert.Equal(1, racun.Id);
            Assert.Equal(new DateTime(2025, 4, 5), racun.Datum);
            Assert.Equal(317m, racun.Ukupno);
            Assert.True(stavke.SequenceEqual(racun.Stavke));
        }
        finally { File.Delete(putanja); }
    }

    [Fact]
    public void SnimiPaUcitaj_PraznaArhiva_VracaPrazneListe()
    {
        var putanja = NovaPutanja();
        try
        {
            XmlArhiva.Snimi(new Arhiva(new List<Kategorija>(), new List<Artikal>(), new List<Racun>()), putanja);
            var ucitano = XmlArhiva.Ucitaj(putanja);

            Assert.Empty(ucitano.Kategorije);
            Assert.Empty(ucitano.Artikli);
            Assert.Empty(ucitano.Racuni);
        }
        finally { File.Delete(putanja); }
    }

    [Fact]
    public void Snimi_NapraviFolderAkoNePostoji()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"xml_{Guid.NewGuid():N}");
        var putanja = Path.Combine(folder, "arhiva.xml");
        try
        {
            XmlArhiva.Snimi(new Arhiva(new List<Kategorija>(), new List<Artikal>(), new List<Racun>()), putanja);
            Assert.True(File.Exists(putanja));
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
    }

    [Fact]
    public void Ucitaj_NepostojeciFajl_BacaFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => XmlArhiva.Ucitaj(NovaPutanja()));
    }

    [Fact]
    public void Ucitaj_NedostajeAtribut_BacaInvalidDataException()
    {
        var putanja = NovaPutanja();
        try
        {
            File.WriteAllText(putanja, "<prodavnica><kategorije><kategorija id=\"1\"/></kategorije></prodavnica>");
            Assert.Throws<InvalidDataException>(() => XmlArhiva.Ucitaj(putanja));
        }
        finally { File.Delete(putanja); }
    }
}
