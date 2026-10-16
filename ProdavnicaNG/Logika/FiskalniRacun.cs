using System.Text;
using ProdavnicaNG.Models;

namespace ProdavnicaNG.Logika;

internal static class FiskalniRacun
{
    /// <summary>Tekst racuna za TextBox: naziv i iznos svake stavke, pa ukupno.</summary>
    public static string Formatiraj(IEnumerable<StavkaRacuna> stavke, IFormatProvider? kultura = null)
    {
        kultura ??= System.Globalization.CultureInfo.CurrentCulture;

        var tekst = new StringBuilder();
        decimal ukupno = 0;
        foreach (var s in stavke)
        {
            tekst.AppendLine($"{s.Naziv}\t{s.Iznos.ToString("N2", kultura)}");
            ukupno += s.Iznos;
        }
        tekst.AppendLine(new string('-', 28));
        tekst.AppendLine("Ukupno");
        tekst.AppendLine(ukupno.ToString("N2", kultura));
        return tekst.ToString();
    }
}
