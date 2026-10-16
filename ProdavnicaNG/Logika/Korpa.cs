using ProdavnicaNG.Models;

namespace ProdavnicaNG.Logika;

/// <summary>Racun koji se trenutno sastavlja. Ne zna nista o prozorima ni o bazi.</summary>
internal class Korpa
{
    private readonly List<StavkaRacuna> _stavke = new();

    public IReadOnlyList<StavkaRacuna> Stavke => _stavke;
    public decimal Ukupno => _stavke.Sum(s => s.Iznos);
    public bool Prazna => _stavke.Count == 0;

    public void Dodaj(Artikal artikal, decimal kolicina)
    {
        if (kolicina <= 0)
            throw new ArgumentOutOfRangeException(nameof(kolicina), "Kolicina mora biti veca od nule.");

        int i = _stavke.FindIndex(s => s.ArtikalId == artikal.Id);
        if (i >= 0)
            _stavke[i] = _stavke[i] with { Kolicina = _stavke[i].Kolicina + kolicina };
        else
            _stavke.Add(new StavkaRacuna(artikal.Id, artikal.Naziv, artikal.Cena, kolicina, artikal.Mera));
    }

    public void Ukloni(int indeks)
    {
        if (indeks < 0 || indeks >= _stavke.Count)
            throw new ArgumentOutOfRangeException(nameof(indeks));
        _stavke.RemoveAt(indeks);
    }

    public void Isprazni() => _stavke.Clear();
}
