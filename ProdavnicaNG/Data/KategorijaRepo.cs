using Microsoft.Data.Sqlite;
using ProdavnicaNG.Models;

namespace ProdavnicaNG.Data;

internal static class KategorijaRepo
{
    public static List<Kategorija> Sve() =>
        Baza.Citaj("SELECT idKategorije, naziv FROM kategorije ORDER BY naziv",
            r => new Kategorija(r.GetInt32(0), r.GetString(1)));

    public static void Dodaj(string naziv) =>
        Baza.Izvrsi("INSERT INTO kategorije (naziv) VALUES (@naziv)",
            new SqliteParameter("@naziv", naziv));

    public static void Izmeni(int id, string naziv) =>
        Baza.Izvrsi("UPDATE kategorije SET naziv = @naziv WHERE idKategorije = @id",
            new SqliteParameter("@naziv", naziv), new SqliteParameter("@id", id));

    public static void Obrisi(int id) =>
        Baza.Izvrsi("DELETE FROM kategorije WHERE idKategorije = @id",
            new SqliteParameter("@id", id));
}
