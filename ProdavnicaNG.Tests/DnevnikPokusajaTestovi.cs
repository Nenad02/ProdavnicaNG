using ProdavnicaNG.Logika;
using ProdavnicaNG.Models;
using Xunit;

namespace ProdavnicaNG.Tests;

public class DnevnikPokusajaTestovi
{
    [Fact]
    public void Zapisi_DodajeRedPoPokusaju_UNovFajlIUNepostojeciFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"dnevnik_{Guid.NewGuid():N}");
        var putanja = Path.Combine(folder, "pokusaji.txt");
        try
        {
            var dnevnik = new DnevnikPokusaja(putanja);
            dnevnik.Zapisi(new DateTime(2025, 4, 5, 22, 10, 30), new Artikal(7, 3, "Vino Kremen", 900m, "kom"));
            dnevnik.Zapisi(new DateTime(2025, 4, 5, 23, 0, 0), new Artikal(8, 3, "Pivo", 150m, "kom"));

            var redovi = File.ReadAllLines(putanja);

            Assert.Equal(2, redovi.Length);
            Assert.Equal("2025-04-05 22:10:30\t7\tVino Kremen", redovi[0]);
            Assert.Contains("Pivo", redovi[1]);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
        }
    }
}
