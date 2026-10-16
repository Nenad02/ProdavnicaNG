namespace ProdavnicaNG;

internal static class Putanje
{
    public static string PodaciFolder => Path.Combine(AppContext.BaseDirectory, "podaci");
    public static string XmlFajl => Path.Combine(PodaciFolder, "prodavnica.xml");
    public static string DnevnikAlkohola => Path.Combine(PodaciFolder, "pokusaji_alkohola.txt");
}
