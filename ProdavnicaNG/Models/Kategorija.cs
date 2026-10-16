namespace ProdavnicaNG.Models;

internal record Kategorija(int Id, string Naziv)
{
    // ListBox i ComboBox prikazuju ono sto vrati ToString()
    public override string ToString() => Naziv;
}
