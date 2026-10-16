using System.Globalization;
using System.Text;
using ProdavnicaNG.Models;

namespace ProdavnicaNG.Logika;

/// <summary>Delegat za dogadjaj: neko je pokusao da unese alkohol u zabranjeno vreme.</summary>
internal delegate void ZabranjenPokusajHandler(DateTime vreme, Artikal artikal);

internal class ZabranaAlkohola
{
    public const string NazivKategorije = "Alkoholna pića";
    public static readonly TimeSpan PocetakZabrane = new(22, 0, 0);

    public event ZabranjenPokusajHandler? ZabranjenPokusaj;

    public static bool JeAlkohol(string nazivKategorije) =>
        Normalizuj(nazivKategorije) == Normalizuj(NazivKategorije);

    public static bool JeZabranjeno(string nazivKategorije, DateTime vreme) =>
        JeAlkohol(nazivKategorije) && vreme.TimeOfDay >= PocetakZabrane;

    /// <summary>Vraca true ako unos sme da prodje. Inace pokrece dogadjaj i vraca false.</summary>
    public bool Proveri(string nazivKategorije, Artikal artikal, DateTime vreme)
    {
        if (!JeZabranjeno(nazivKategorije, vreme)) return true;
        ZabranjenPokusaj?.Invoke(vreme, artikal);
        return false;
    }

    // "Alkoholna pića" i "alkoholna pica" tretiramo isto
    private static string Normalizuj(string tekst)
    {
        var razlozen = tekst.Trim().Normalize(NormalizationForm.FormD);
        var rezultat = new StringBuilder();
        foreach (var znak in razlozen)
            if (CharUnicodeInfo.GetUnicodeCategory(znak) != UnicodeCategory.NonSpacingMark)
                rezultat.Append(znak);
        return rezultat.ToString().ToLowerInvariant();
    }
}
