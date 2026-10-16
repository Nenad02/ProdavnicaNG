using System.Globalization;
using Microsoft.Data.Sqlite;
using ProdavnicaNG.Models;

namespace ProdavnicaNG.Data;

internal static class StatistikaRepo
{
    /// <summary>Artikli prodati na zadati dan: kolicina tog dana i kolicina od kada postoji prodavnica.</summary>
    public static List<StatistikaArtikla> ZaDatum(DateTime datum) =>
        Baza.Citaj(
            """
            SELECT a.naziv,
                   SUM(CASE WHEN r.datum = @datum THEN ra.kolicina ELSE 0 END) AS naDan,
                   SUM(ra.kolicina) AS ukupno
            FROM racun_artikal ra
            JOIN racuni r  ON r.idRacuna = ra.idRacuna
            JOIN artikli a ON a.idArtikla = ra.idArtikla
            GROUP BY a.idArtikla, a.naziv
            HAVING SUM(CASE WHEN r.datum = @datum THEN ra.kolicina ELSE 0 END) > 0
            ORDER BY naDan DESC, a.naziv
            """,
            r => new StatistikaArtikla(r.GetString(0), r.GetDecimal(1), r.GetDecimal(2)),
            new SqliteParameter("@datum", datum.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
}
