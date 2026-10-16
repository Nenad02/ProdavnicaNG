namespace ProdavnicaNG.UI;

internal class PocetnaForma : Form
{
    public PocetnaForma()
    {
        Text = "Prodavnica NG";
        ClientSize = new Size(320, 140);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        var administracija = new Button { Text = "Administracija i statistika", Size = new Size(260, 44), Location = new Point(30, 20) };
        var prodaja = new Button { Text = "Prodaja/Naplata", Size = new Size(260, 44), Location = new Point(30, 76) };

        administracija.Click += (_, _) => Otvori(() => new UpravljanjeForma());
        prodaja.Click += (_, _) => Otvori(() => new ProdajaForma());

        Controls.AddRange(new Control[] { administracija, prodaja });
    }

    private void Otvori(Func<Form> napravi)
    {
        Hide();
        using var forma = napravi();
        forma.ShowDialog(this);
        Show();
    }
}
