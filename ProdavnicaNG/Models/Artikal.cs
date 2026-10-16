namespace ProdavnicaNG.Models;

internal record Artikal(int Id, int KategorijaId, string Naziv, decimal Cena, string? Mera)
{
    public override string ToString() =>
        string.IsNullOrWhiteSpace(Mera) ? $"{Naziv} - {Cena:N2}" : $"{Naziv} - {Cena:N2} / {Mera}";
}
