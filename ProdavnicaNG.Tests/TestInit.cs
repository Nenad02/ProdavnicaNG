using System.Runtime.CompilerServices;

namespace ProdavnicaNG.Tests;

internal static class TestInit
{
    // Testovi koriste sopstveni fajl baze, nikad pravu bazu programa.
    [ModuleInitializer]
    internal static void Podesi() =>
        Environment.SetEnvironmentVariable(
            "PRODAVNICA_DB",
            Path.Combine(Path.GetTempPath(), $"prodavnica_test_{Guid.NewGuid():N}.db"));
}
