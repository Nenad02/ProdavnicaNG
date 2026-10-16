using Microsoft.Data.Sqlite;
using ProdavnicaNG.Models;

namespace ProdavnicaNG.Data;

internal static class ArtikalRepo
{
    private const string Kolone = "idArtikla, idKategorije, naziv, cena, mera";

    public static List<Artikal> Svi() =>
        Baza.Citaj($"SELECT {Kolone} FROM artikli ORDER BY idArtikla", Mapiraj);

    public static List<Artikal> PoKategoriji(int kategorijaId) =>
        Baza.Citaj($"SELECT {Kolone} FROM artikli WHERE idKategorije = @k ORDER BY naziv", Mapiraj,
            new SqliteParameter("@k", kategorijaId));

    public static void Dodaj(int kategorijaId, string naziv, decimal cena, string? mera) =>
        Baza.Izvrsi(
            "INSERT INTO artikli (idKategorije, naziv, cena, mera) VALUES (@k, @naziv, @cena, @mera)",
            Parametri(kategorijaId, naziv, cena, mera));

    public static void Izmeni(int id, int kategorijaId, string naziv, decimal cena, string? mera) =>
        Baza.Izvrsi(
            "UPDATE artikli SET idKategorije = @k, naziv = @naziv, cena = @cena, mera = @mera WHERE idArtikla = @id",
            Parametri(kategorijaId, naziv, cena, mera).Append(new SqliteParameter("@id", id)).ToArray());

    public static void Obrisi(int id) =>
        Baza.Izvrsi("DELETE FROM artikli WHERE idArtikla = @id", new SqliteParameter("@id", id));

    private static Artikal Mapiraj(SqliteDataReader r) =>
        new(r.GetInt32(0), r.GetInt32(1), r.GetString(2), r.GetDecimal(3), r.IsDBNull(4) ? null : r.GetString(4));

    private static SqliteParameter[] Parametri(int kategorijaId, string naziv, decimal cena, string? mera) =>
    [
        new SqliteParameter("@k", kategorijaId),
        new SqliteParameter("@naziv", naziv),
        new SqliteParameter("@cena", cena),
        new SqliteParameter("@mera", (object?)mera ?? DBNull.Value),
    ];
}
