using System.Globalization;
using Microsoft.Data.Sqlite;
using ProdavnicaNG.Models;

namespace ProdavnicaNG.Data;

internal static class RacunRepo
{
    /// <summary>Snima racun i sve njegove stavke u jednoj transakciji. Vraca id novog racuna.</summary>
    public static int Sacuvaj(DateTime datum, IReadOnlyList<StavkaRacuna> stavke)
    {
        if (stavke.Count == 0)
            throw new ArgumentException("Racun mora imati bar jednu stavku.", nameof(stavke));

        int idRacuna = 0;

        Baza.UTransakciji((veza, transakcija) =>
        {
            using (var racun = new SqliteCommand(
                "INSERT INTO racuni (datum, ukupno) VALUES (@datum, @ukupno); SELECT last_insert_rowid();",
                veza, transakcija))
            {
                racun.Parameters.AddWithValue("@datum", datum.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                racun.Parameters.AddWithValue("@ukupno", stavke.Sum(s => s.Iznos));
                idRacuna = Convert.ToInt32(racun.ExecuteScalar());
            }

            foreach (var s in stavke)
            {
                using var stavka = new SqliteCommand(
                    "INSERT INTO racun_artikal (idRacuna, idArtikla, kolicina, cena) VALUES (@r, @a, @kol, @cena)",
                    veza, transakcija);
                stavka.Parameters.AddWithValue("@r", idRacuna);
                stavka.Parameters.AddWithValue("@a", s.ArtikalId);
                stavka.Parameters.AddWithValue("@kol", s.Kolicina);
                stavka.Parameters.AddWithValue("@cena", s.Cena);
                stavka.ExecuteNonQuery();
            }
        });

        return idRacuna;
    }

    /// <summary>Svi zatvoreni racuni sa stavkama.</summary>
    public static List<Racun> Svi()
    {
        var stavkePoRacunu = Baza.Citaj(
                """
                SELECT ra.idRacuna, ra.idArtikla, a.naziv, ra.cena, ra.kolicina, a.mera
                FROM racun_artikal ra
                JOIN artikli a ON a.idArtikla = ra.idArtikla
                ORDER BY ra.idRacuna
                """,
                r => (RacunId: r.GetInt32(0),
                      Stavka: new StavkaRacuna(r.GetInt32(1), r.GetString(2), r.GetDecimal(3), r.GetDecimal(4),
                                               r.IsDBNull(5) ? null : r.GetString(5))))
            .ToLookup(x => x.RacunId, x => x.Stavka);

        return Baza.Citaj(
            "SELECT idRacuna, datum, ukupno FROM racuni ORDER BY idRacuna",
            r => new Racun(
                r.GetInt32(0),
                DateTime.ParseExact(r.GetString(1), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                r.GetDecimal(2),
                stavkePoRacunu[r.GetInt32(0)].ToList()));
    }
}
