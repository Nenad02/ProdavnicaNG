namespace ProdavnicaNG.Models;

/// <summary>Prodaja jednog artikla: na izabrani dan i ukupno od kada postoji prodavnica.</summary>
internal record StatistikaArtikla(string Naziv, decimal ProdatoNaDan, decimal ProdatoUkupno)
{
    /// <summary>Koliki deo ukupne prodaje artikla je prodat tog dana (0-100), za ProgressBar.</summary>
    public decimal Procenat => ProdatoUkupno <= 0 ? 0 : Math.Min(100m, ProdatoNaDan / ProdatoUkupno * 100m);
}
