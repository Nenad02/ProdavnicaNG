namespace ProdavnicaNG.Models;

/// <summary>Jedna stavka na racunu. Cena se pamti u trenutku prodaje, pa kasnija promena cene artikla ne menja stare racune.</summary>
internal record StavkaRacuna(int ArtikalId, string Naziv, decimal Cena, decimal Kolicina, string? Mera = null)
{
    public decimal Iznos => Cena * Kolicina;

    // ListBox prikazuje ovaj tekst, npr. "Banane(2 kg)"
    public override string ToString() =>
        string.IsNullOrWhiteSpace(Mera) ? $"{Naziv}({Kolicina:0.###})" : $"{Naziv}({Kolicina:0.###} {Mera})";
}
