using ProdavnicaNG.UI;

namespace ProdavnicaNG;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new PocetnaForma());
    }
}
