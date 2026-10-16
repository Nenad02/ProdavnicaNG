using System.Globalization;
using Microsoft.Data.Sqlite;
using ProdavnicaNG.Data;
using ProdavnicaNG.Models;

namespace ProdavnicaNG.UI;

internal class UpravljanjeForma : Form
{
    // kategorije
    private readonly ListBox _listaKategorija = new() { Dock = DockStyle.Fill };
    private readonly TextBox _nazivKategorije = new() { Width = 200 };

    // artikli
    private readonly ComboBox _izborKategorije = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly ListBox _listaArtikala = new() { Dock = DockStyle.Fill };
    private readonly TextBox _nazivArtikla = new() { Width = 200 };
    private readonly TextBox _cenaArtikla = new() { Width = 200 };
    private readonly TextBox _meraArtikla = new() { Width = 200 };

    // statistika
    private readonly DateTimePicker _datum = new() { Format = DateTimePickerFormat.Long, Width = 280 };
    private readonly FlowLayoutPanel _statistika = new()
    {
        Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true
    };

    public UpravljanjeForma()
    {
        Text = "Administracija";
        ClientSize = new Size(680, 420);
        StartPosition = FormStartPosition.CenterParent;

        var tabovi = new TabControl { Dock = DockStyle.Fill };
        tabovi.TabPages.Add(NapraviTabKategorija());
        tabovi.TabPages.Add(NapraviTabArtikli());
        tabovi.TabPages.Add(NapraviTabStatistika());
        tabovi.SelectedIndexChanged += (_, _) => { OsveziKategorije(); OsveziStatistiku(); };
        Controls.Add(tabovi);

        _listaKategorija.SelectedIndexChanged += (_, _) =>
        {
            if (_listaKategorija.SelectedItem is Kategorija k) _nazivKategorije.Text = k.Naziv;
        };
        _izborKategorije.SelectedIndexChanged += (_, _) => OsveziArtikle();
        _listaArtikala.SelectedIndexChanged += (_, _) => PopuniPoljaArtikla();

        Load += (_, _) => { OsveziKategorije(); OsveziStatistiku(); };
    }

    // ---------- izgradnja prozora ----------

    private TabPage NapraviTabKategorija()
    {
        var desno = NovaKolona();
        desno.Controls.Add(new Label { Text = "Naziv kategorije:", AutoSize = true });
        desno.Controls.Add(_nazivKategorije);
        desno.Controls.Add(Dugme("Dodaj kategoriju", DodajKategoriju));
        desno.Controls.Add(Dugme("Izmeni kategoriju", IzmeniKategoriju));
        desno.Controls.Add(Dugme("Obriši kategoriju", ObrisiKategoriju));

        return NapraviTab("Ažuriranje kategorija", _listaKategorija, desno);
    }

    private TabPage NapraviTabArtikli()
    {
        var levo = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        levo.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        levo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        levo.Controls.Add(_izborKategorije, 0, 0);
        levo.Controls.Add(_listaArtikala, 0, 1);

        var desno = NovaKolona();
        desno.Controls.Add(new Label { Text = "Naziv artikla:", AutoSize = true });
        desno.Controls.Add(_nazivArtikla);
        desno.Controls.Add(new Label { Text = "Cena artikla:", AutoSize = true });
        desno.Controls.Add(_cenaArtikla);
        desno.Controls.Add(new Label { Text = "Jedinica mere artikla:", AutoSize = true });
        desno.Controls.Add(_meraArtikla);
        desno.Controls.Add(Dugme("Dodaj artikal", DodajArtikal));
        desno.Controls.Add(Dugme("Izmeni artikal", IzmeniArtikal));
        desno.Controls.Add(Dugme("Obriši artikal", ObrisiArtikal));

        return NapraviTab("Ažuriranje artikala", levo, desno);
    }

    private TabPage NapraviTabStatistika()
    {
        var raspored = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(8) };
        raspored.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        raspored.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        raspored.Controls.Add(_datum, 0, 0);
        raspored.Controls.Add(_statistika, 0, 1);

        _datum.ValueChanged += (_, _) => OsveziStatistiku();

        var tab = new TabPage("Pregled statistike");
        tab.Controls.Add(raspored);
        return tab;
    }

    private static TabPage NapraviTab(string naslov, Control levo, Control desno)
    {
        var raspored = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(8) };
        raspored.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        raspored.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
        raspored.Controls.Add(levo, 0, 0);
        raspored.Controls.Add(desno, 1, 0);

        var tab = new TabPage(naslov);
        tab.Controls.Add(raspored);
        return tab;
    }

    private static FlowLayoutPanel NovaKolona() =>
        new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(8, 0, 0, 0) };

    private static Button Dugme(string tekst, Action akcija)
    {
        var dugme = new Button { Text = tekst, Width = 200, Height = 30 };
        dugme.Click += (_, _) => akcija();
        return dugme;
    }

    // ---------- osvezavanje podataka ----------

    private void OsveziKategorije()
    {
        Pokusaj(() =>
        {
            int? izabrana = (_izborKategorije.SelectedItem as Kategorija)?.Id;
            var sve = KategorijaRepo.Sve();

            // Dve odvojene liste: ako dva kontrola dele isti DataSource, menjaju izbor jedan drugom.
            _listaKategorija.DataSource = new List<Kategorija>(sve);
            _izborKategorije.DataSource = new List<Kategorija>(sve);

            if (izabrana is int id)
                _izborKategorije.SelectedItem = sve.FirstOrDefault(k => k.Id == id);
        });
    }

    private void OsveziArtikle()
    {
        _listaArtikala.DataSource = _izborKategorije.SelectedItem is Kategorija k
            ? ArtikalRepo.PoKategoriji(k.Id)
            : new List<Artikal>();
    }

    private void PopuniPoljaArtikla()
    {
        if (_listaArtikala.SelectedItem is not Artikal a) return;
        _nazivArtikla.Text = a.Naziv;
        _cenaArtikla.Text = a.Cena.ToString("0.##", CultureInfo.CurrentCulture);
        _meraArtikla.Text = a.Mera ?? "";
    }

    private void OsveziStatistiku()
    {
        Pokusaj(() =>
        {
            _statistika.SuspendLayout();
            foreach (var kontrola in _statistika.Controls.Cast<Control>().ToList())
            {
                _statistika.Controls.Remove(kontrola);
                kontrola.Dispose();
            }

            foreach (var s in StatistikaRepo.ZaDatum(_datum.Value.Date))
                _statistika.Controls.Add(NapraviRedStatistike(s));

            _statistika.ResumeLayout();
        });
    }

    private static Control NapraviRedStatistike(StatistikaArtikla s)
    {
        var red = new TableLayoutPanel { Width = 600, Height = 30, ColumnCount = 3, RowCount = 1 };
        red.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
        red.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        red.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));

        red.Controls.Add(new Label { Text = s.Naziv, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        red.Controls.Add(new ProgressBar
        {
            Minimum = 0, Maximum = 100, Value = (int)Math.Round(s.Procenat), Dock = DockStyle.Fill
        }, 1, 0);
        red.Controls.Add(new Label
        {
            Text = s.ProdatoNaDan.ToString("0.##", CultureInfo.CurrentCulture),
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight
        }, 2, 0);
        return red;
    }

    // ---------- kategorije ----------

    private void DodajKategoriju()
    {
        if (!ImaTekst(_nazivKategorije, "Unesi naziv kategorije.")) return;
        PokusajIzmenu(() => { KategorijaRepo.Dodaj(_nazivKategorije.Text.Trim()); _nazivKategorije.Clear(); OsveziKategorije(); });
    }

    private void IzmeniKategoriju()
    {
        if (_listaKategorija.SelectedItem is not Kategorija k) { Poruka("Izaberi kategoriju iz liste."); return; }
        if (!ImaTekst(_nazivKategorije, "Unesi novi naziv.")) return;
        PokusajIzmenu(() => { KategorijaRepo.Izmeni(k.Id, _nazivKategorije.Text.Trim()); OsveziKategorije(); });
    }

    private void ObrisiKategoriju()
    {
        if (_listaKategorija.SelectedItem is not Kategorija k) { Poruka("Izaberi kategoriju iz liste."); return; }
        PokusajIzmenu(() => { KategorijaRepo.Obrisi(k.Id); _nazivKategorije.Clear(); OsveziKategorije(); });
    }

    // ---------- artikli ----------

    private void DodajArtikal()
    {
        if (!UcitajArtikal(out var k, out var naziv, out var cena, out var mera)) return;
        PokusajIzmenu(() => { ArtikalRepo.Dodaj(k.Id, naziv, cena, mera); OcistiPoljaArtikla(); OsveziArtikle(); });
    }

    private void IzmeniArtikal()
    {
        if (_listaArtikala.SelectedItem is not Artikal a) { Poruka("Izaberi artikal iz liste."); return; }
        if (!UcitajArtikal(out var k, out var naziv, out var cena, out var mera)) return;
        PokusajIzmenu(() => { ArtikalRepo.Izmeni(a.Id, k.Id, naziv, cena, mera); OcistiPoljaArtikla(); OsveziArtikle(); });
    }

    private void ObrisiArtikal()
    {
        if (_listaArtikala.SelectedItem is not Artikal a) { Poruka("Izaberi artikal iz liste."); return; }
        PokusajIzmenu(() => { ArtikalRepo.Obrisi(a.Id); OcistiPoljaArtikla(); OsveziArtikle(); });
    }

    private bool UcitajArtikal(out Kategorija kategorija, out string naziv, out decimal cena, out string? mera)
    {
        kategorija = null!; naziv = ""; cena = 0; mera = null;

        if (_izborKategorije.SelectedItem is not Kategorija k) { Poruka("Izaberi kategoriju."); return false; }
        if (!ImaTekst(_nazivArtikla, "Unesi naziv artikla.")) return false;
        if (!decimal.TryParse(_cenaArtikla.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out cena) || cena < 0)
        { Poruka("Cena mora biti broj (0 ili veći)."); return false; }

        kategorija = k;
        naziv = _nazivArtikla.Text.Trim();
        mera = string.IsNullOrWhiteSpace(_meraArtikla.Text) ? null : _meraArtikla.Text.Trim();
        return true;
    }

    private void OcistiPoljaArtikla()
    {
        _nazivArtikla.Clear();
        _cenaArtikla.Clear();
        _meraArtikla.Clear();
    }

    // ---------- pomocne ----------

    private static bool ImaTekst(TextBox polje, string poruka)
    {
        if (!string.IsNullOrWhiteSpace(polje.Text)) return true;
        Poruka(poruka);
        return false;
    }

    private static void Poruka(string tekst) =>
        MessageBox.Show(tekst, "Prodavnica NG", MessageBoxButtons.OK, MessageBoxIcon.Information);

    /// <summary>Izmena podataka: posle uspesnog upisa osvezi i XML kopiju.</summary>
    private static void PokusajIzmenu(Action akcija) =>
        Pokusaj(() => { akcija(); XmlArhiva.OsveziKopiju(); });

    private static void Pokusaj(Action akcija)
    {
        try { akcija(); }
        catch (SqliteException ex) when (ex.SqliteExtendedErrorCode == 787)
        {
            Poruka("Ne može da se obriše: kategorija ima artikle ili je artikal već prodat.");
        }
        catch (SqliteException ex)
        {
            MessageBox.Show("Greška u radu sa bazom:\n" + ex.Message, "Prodavnica NG", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
