namespace ProdavnicaNG.Models;

internal record Racun(int Id, DateTime Datum, decimal Ukupno, IReadOnlyList<StavkaRacuna> Stavke);
