using System.Globalization;

namespace ProdavnicaNG.Logika;

/// <summary>
/// Stanje unosa pored tastature: izabran artikal, pa "x", pa cifre.
/// Prikaz je npr. "Banane x 2". Bez "x" ili bez cifara kolicina je 1.
/// </summary>
internal class UnosKolicine
{
    private const int NajviseZnakova = 8;

    public string? Naziv { get; private set; }
    public bool Mnozenje { get; private set; }
    public string Kolicina { get; private set; } = "";

    public bool ImaArtikal => Naziv != null;

    public void IzaberiArtikal(string naziv)
    {
        Naziv = naziv;
        Mnozenje = false;
        Kolicina = "";
    }

    public void PritisniX()
    {
        if (Naziv != null) Mnozenje = true;
    }

    public void Cifra(char znak)
    {
        if (!Mnozenje || Kolicina.Length >= NajviseZnakova) return;

        if (znak == ',' || znak == '.')
        {
            if (Kolicina.Contains(',')) return;
            Kolicina += Kolicina.Length == 0 ? "0," : ",";
            return;
        }

        if (!char.IsDigit(znak)) return;
        if (Kolicina == "0") Kolicina = "";   // da ne nastane "05"
        Kolicina += znak;
    }

    /// <summary>Uneta kolicina; null ako tekst nije broj.</summary>
    public decimal? Vrednost
    {
        get
        {
            if (!Mnozenje || Kolicina.Length == 0) return 1m;
            return decimal.TryParse(Kolicina.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var v)
                ? v
                : null;
        }
    }

    public string Prikaz => Naziv == null ? "" : Mnozenje ? $"{Naziv} x {Kolicina}" : Naziv;

    public void Ponisti()
    {
        Naziv = null;
        Mnozenje = false;
        Kolicina = "";
    }
}
