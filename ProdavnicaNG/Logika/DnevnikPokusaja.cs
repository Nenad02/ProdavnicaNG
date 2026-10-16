using System.Globalization;
using ProdavnicaNG.Models;

namespace ProdavnicaNG.Logika;

/// <summary>Upisuje zabranjene pokusaje (vreme, id i naziv artikla) u posebnu tekstualnu datoteku.</summary>
internal class DnevnikPokusaja
{
    private readonly string _putanja;

    public DnevnikPokusaja(string putanja) => _putanja = putanja;

    public void Zapisi(DateTime vreme, Artikal artikal)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_putanja))!);
        var vremeTekst = vreme.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        File.AppendAllText(_putanja, $"{vremeTekst}\t{artikal.Id}\t{artikal.Naziv}{Environment.NewLine}");
    }
}
