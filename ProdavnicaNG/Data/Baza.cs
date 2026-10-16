using Microsoft.Data.Sqlite;

namespace ProdavnicaNG.Data;

/// <summary>
/// Jedino mesto gde se otvara konekcija. Baza je jedan fajl (prodavnica.db) pored
/// programa; tabele se same naprave pri prvom pokretanju.
/// </summary>
internal static class Baza
{
    // PRODAVNICA_DB omogucava testovima da koriste sopstveni fajl baze
    private static readonly string Veza =
        "Data Source=" + (Environment.GetEnvironmentVariable("PRODAVNICA_DB")
                          ?? Path.Combine(AppContext.BaseDirectory, "prodavnica.db"));

    static Baza()
    {
        Izvrsi("""
            CREATE TABLE IF NOT EXISTS kategorije (
                idKategorije INTEGER PRIMARY KEY AUTOINCREMENT,
                naziv        TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS artikli (
                idArtikla    INTEGER PRIMARY KEY AUTOINCREMENT,
                idKategorije INTEGER NOT NULL REFERENCES kategorije(idKategorije),
                naziv        TEXT NOT NULL,
                cena         NUMERIC NOT NULL CHECK (cena >= 0),
                mera         TEXT
            );
            CREATE TABLE IF NOT EXISTS racuni (
                idRacuna INTEGER PRIMARY KEY AUTOINCREMENT,
                datum    TEXT NOT NULL,          -- yyyy-MM-dd
                ukupno   NUMERIC NOT NULL
            );
            CREATE TABLE IF NOT EXISTS racun_artikal (
                idRacuna  INTEGER NOT NULL REFERENCES racuni(idRacuna),
                idArtikla INTEGER NOT NULL REFERENCES artikli(idArtikla),
                kolicina  NUMERIC NOT NULL CHECK (kolicina > 0),
                cena      NUMERIC NOT NULL
            );
            """);
    }

    private static SqliteConnection Otvori()
    {
        var veza = new SqliteConnection(Veza);
        veza.Open();
        // SQLite ne proverava strane kljuceve dok se ne ukljuci
        using var pragma = new SqliteCommand("PRAGMA foreign_keys = ON;", veza);
        pragma.ExecuteNonQuery();
        return veza;
    }

    public static int Izvrsi(string sql, params SqliteParameter[] parametri)
    {
        using var veza = Otvori();
        using var komanda = new SqliteCommand(sql, veza);
        komanda.Parameters.AddRange(parametri);
        return komanda.ExecuteNonQuery();
    }

    /// <summary>Vise upisa kao jedna celina: ako nesto pukne, nista se ne upise.</summary>
    public static void UTransakciji(Action<SqliteConnection, SqliteTransaction> posao)
    {
        using var veza = Otvori();
        using var transakcija = veza.BeginTransaction();
        posao(veza, transakcija);
        transakcija.Commit();
    }

    public static List<T> Citaj<T>(string sql, Func<SqliteDataReader, T> uObjekat, params SqliteParameter[] parametri)
    {
        using var veza = Otvori();
        using var komanda = new SqliteCommand(sql, veza);
        komanda.Parameters.AddRange(parametri);

        var rezultat = new List<T>();
        using var citac = komanda.ExecuteReader();
        while (citac.Read())
            rezultat.Add(uObjekat(citac));
        return rezultat;
    }
}
