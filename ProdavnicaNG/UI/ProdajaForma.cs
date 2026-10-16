using Microsoft.Data.Sqlite;
using ProdavnicaNG.Data;
using ProdavnicaNG.Logika;
using ProdavnicaNG.Models;

namespace ProdavnicaNG.UI;

internal class ProdajaForma : Form
{
    private static readonly Color BojaKategorije = Color.RosyBrown;
    private static readonly Color BojaIzabraneKategorije = Color.Red;
    private static readonly Color BojaArtikla = Color.Orange;
    private static readonly Color BojaIzabranogArtikla = Color.DarkRed;
    private static readonly Font MalaSlova = new("Segoe UI", 8f);

    // logika (bez prozora)
    private readonly Korpa _korpa = new();
    private readonly UnosKolicine _unos = new();
    private readonly ZabranaAlkohola _zabrana = new();
    private readonly DnevnikPokusaja _dnevnik = new(Putanje.DnevnikAlkohola);

    private Kategorija? _kategorija;
    private Artikal? _artikal;

    // kontrole
    private readonly FlowLayoutPanel _panelKategorija = new() { Dock = DockStyle.Fill, WrapContents = false, AutoScroll = true };
    private readonly FlowLayoutPanel _panelArtikala = new() { Dock = DockStyle.Fill, AutoScroll = true };
    private readonly TabControl _tabovi = new() { Dock = DockStyle.Fill };
    private readonly ListBox _listaStavki = new() { Dock = DockStyle.Fill };
    private readonly Label _ukupno = new()
    {
        Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 11f, FontStyle.Bold)
    };
    private readonly TextBox _prikazUnosa = new() { ReadOnly = true, Dock = DockStyle.Fill };
    private readonly TextBox _fiskalni = new()
    {
        Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 9f)
    };
    private readonly List<Button> _tasteriCifara = new();
    private Button _tasterX = null!;

    public ProdajaForma()
    {
        Text = "Prodaja/Naplata";
        ClientSize = new Size(840, 580);
        MinimumSize = new Size(760, 540);
        StartPosition = FormStartPosition.CenterParent;

        var glavni = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(6) };
        glavni.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        glavni.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        glavni.Controls.Add(NapraviLevuStranu(), 0, 0);
        glavni.Controls.Add(NapraviDesnuStranu(), 1, 0);
        Controls.Add(glavni);

        _zabrana.ZabranjenPokusaj += NaZabranjenPokusaj;
        Load += (_, _) => { UcitajKategorije(); OsveziUnos(); OsveziStavke(); };
    }

    // ---------- izgradnja prozora ----------

    private Control NapraviLevuStranu()
    {
        var levo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        levo.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
        levo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        levo.Controls.Add(_panelKategorija, 0, 0);
        levo.Controls.Add(_panelArtikala, 0, 1);
        return levo;
    }

    private Control NapraviDesnuStranu()
    {
        var nova = new Button { Text = "Nova transakcija", Width = 160, Height = 36 };
        nova.Click += (_, _) => NovaTransakcija();

        _tabovi.TabPages.Add(NapraviTabUnos());
        _tabovi.TabPages.Add(NapraviTabFiskalni());

        var desno = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        desno.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        desno.RowStyles.Add(new RowStyle(SizeType.Absolute, 250));
        desno.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        desno.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        desno.Controls.Add(nova, 0, 0);
        desno.Controls.Add(_tabovi, 0, 1);
        desno.Controls.Add(_prikazUnosa, 0, 2);
        desno.Controls.Add(NapraviTastaturu(), 0, 3);
        return desno;
    }

    private TabPage NapraviTabUnos()
    {
        var obrisi = new Button { Text = "Obriši", Width = 80, Height = 40, Anchor = AnchorStyles.Top };
        obrisi.Click += (_, _) => ObrisiStavku();
        var dalje = new Button { Text = "Dalje =>", Width = 80, Height = 30, Anchor = AnchorStyles.None };
        dalje.Click += (_, _) => Dalje();

        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(4) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        t.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        t.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        t.Controls.Add(_listaStavki, 0, 0);
        t.Controls.Add(obrisi, 1, 0);
        t.Controls.Add(_ukupno, 0, 1);
        t.Controls.Add(dalje, 1, 1);

        var tab = new TabPage("Unos/Obrada");
        tab.Controls.Add(t);
        return tab;
    }

    private TabPage NapraviTabFiskalni()
    {
        var sacuvaj = new Button { Text = "Sačuvaj račun", Width = 80, Height = 46, Anchor = AnchorStyles.Top };
        sacuvaj.Click += (_, _) => SacuvajRacun();

        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(4) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        t.Controls.Add(_fiskalni, 0, 0);
        t.Controls.Add(sacuvaj, 1, 0);

        var tab = new TabPage("Fiskalni račun");
        tab.Controls.Add(t);
        return tab;
    }

    private Control NapraviTastaturu()
    {
        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 5 };
        for (int i = 0; i < 4; i++) t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        for (int i = 0; i < 5; i++) t.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

        _tasterX = Taster("x", () => { _unos.PritisniX(); OsveziUnos(); });
        t.Controls.Add(_tasterX, 0, 0);

        string[][] cifre = { new[] { "7", "8", "9" }, new[] { "4", "5", "6" }, new[] { "1", "2", "3" } };
        for (int red = 0; red < 3; red++)
        {
            for (int kolona = 0; kolona < 3; kolona++)
            {
                char znak = cifre[red][kolona][0];
                var taster = Taster(znak.ToString(), () => PritisnutaCifra(znak));
                _tasteriCifara.Add(taster);
                t.Controls.Add(taster, kolona, red + 1);
            }
        }

        var nula = Taster("0", () => PritisnutaCifra('0'));
        _tasteriCifara.Add(nula);
        t.Controls.Add(nula, 0, 4);
        t.SetColumnSpan(nula, 2);

        var zarez = Taster(",", () => PritisnutaCifra(','));
        _tasteriCifara.Add(zarez);
        t.Controls.Add(zarez, 2, 4);

        var unesi = Taster("Unesi", Unesi);
        t.Controls.Add(unesi, 3, 1);
        t.SetRowSpan(unesi, 2);

        var ponisti = Taster("Poništi", PonistiUnos);
        t.Controls.Add(ponisti, 3, 3);
        t.SetRowSpan(ponisti, 2);

        return t;
    }

    private static Button Taster(string tekst, Action akcija)
    {
        var taster = new Button { Text = tekst, Dock = DockStyle.Fill, Margin = new Padding(2) };
        taster.Click += (_, _) => akcija();
        return taster;
    }

    // ---------- kategorije i artikli (dinamicka dugmad) ----------

    private void UcitajKategorije()
    {
        Pokusaj(() =>
        {
            Isprazni(_panelKategorija);
            foreach (var k in KategorijaRepo.Sve())
            {
                var dugme = new Button
                {
                    Text = k.Naziv, Tag = k, Size = new Size(84, 62), Font = MalaSlova,
                    BackColor = BojaKategorije, UseVisualStyleBackColor = false
                };
                dugme.Click += (_, _) => IzaberiKategoriju(k);
                _panelKategorija.Controls.Add(dugme);
            }
        });
    }

    private void IzaberiKategoriju(Kategorija k)
    {
        _kategorija = k;
        foreach (var dugme in _panelKategorija.Controls.OfType<Button>())
            dugme.BackColor = (dugme.Tag as Kategorija) == k ? BojaIzabraneKategorije : BojaKategorije;

        Pokusaj(() =>
        {
            Isprazni(_panelArtikala);
            foreach (var a in ArtikalRepo.PoKategoriji(k.Id))
            {
                var dugme = new Button
                {
                    Text = a.Naziv, Tag = a, Size = new Size(88, 78),
                    BackColor = BojaArtikla, UseVisualStyleBackColor = false
                };
                dugme.Click += (_, _) => IzaberiArtikal(a);
                _panelArtikala.Controls.Add(dugme);
            }
        });

        PonistiUnos();
    }

    private void IzaberiArtikal(Artikal a)
    {
        _artikal = a;
        _unos.IzaberiArtikal(a.Naziv);
        ObojiArtikle();
        OsveziUnos();
    }

    private void ObojiArtikle()
    {
        foreach (var dugme in _panelArtikala.Controls.OfType<Button>())
        {
            bool izabran = _artikal != null && (dugme.Tag as Artikal) == _artikal;
            dugme.BackColor = izabran ? BojaIzabranogArtikla : BojaArtikla;
            dugme.ForeColor = izabran ? Color.White : SystemColors.ControlText;
        }
    }

    private static void Isprazni(FlowLayoutPanel panel)
    {
        foreach (var kontrola in panel.Controls.Cast<Control>().ToList())
        {
            panel.Controls.Remove(kontrola);
            kontrola.Dispose();
        }
    }

    // ---------- unos kolicine ----------

    private void PritisnutaCifra(char znak)
    {
        _unos.Cifra(znak);
        OsveziUnos();
    }

    private void OsveziUnos()
    {
        _prikazUnosa.Text = _unos.Prikaz;
        _tasterX.Enabled = _unos.ImaArtikal;
        foreach (var taster in _tasteriCifara)
            taster.Enabled = _unos.Mnozenje;
    }

    private void Unesi()
    {
        if (_artikal is null || _kategorija is null) { Poruka("Izaberi artikal."); return; }

        // Alkohol posle 22h: dogadjaj snima pokusaj u datoteku i prikazuje poruku, a unos se odustaje.
        if (!_zabrana.Proveri(_kategorija.Naziv, _artikal, DateTime.Now))
        {
            PonistiUnos();
            return;
        }

        if (_unos.Vrednost is not decimal kolicina || kolicina <= 0)
        {
            Poruka("Količina mora biti veća od nule.");
            return;
        }

        _korpa.Dodaj(_artikal, kolicina);
        OsveziStavke();
        PonistiUnos();
    }

    private void PonistiUnos()
    {
        _artikal = null;
        _unos.Ponisti();
        ObojiArtikle();
        OsveziUnos();
    }

    // ---------- racun ----------

    private void OsveziStavke()
    {
        _listaStavki.Items.Clear();
        _listaStavki.Items.AddRange(_korpa.Stavke.Cast<object>().ToArray());
        _ukupno.Text = $"{_korpa.Ukupno:N2} rsd";
    }

    private void ObrisiStavku()
    {
        if (_listaStavki.SelectedIndex < 0) { Poruka("Izaberi stavku za brisanje."); return; }
        _korpa.Ukloni(_listaStavki.SelectedIndex);
        OsveziStavke();
    }

    private void Dalje()
    {
        if (_korpa.Prazna) { Poruka("Račun je prazan."); return; }
        _fiskalni.Text = FiskalniRacun.Formatiraj(_korpa.Stavke);
        _tabovi.SelectedIndex = 1;
    }

    private void SacuvajRacun()
    {
        if (_korpa.Prazna) { Poruka("Račun je prazan."); return; }

        Pokusaj(() =>
        {
            RacunRepo.Sacuvaj(DateTime.Now, _korpa.Stavke);
            XmlArhiva.OsveziKopiju();
            Poruka("Račun je sačuvan.");
            NovaTransakcija();
        });
    }

    private void NovaTransakcija()
    {
        _korpa.Isprazni();
        OsveziStavke();
        _fiskalni.Clear();
        _tabovi.SelectedIndex = 0;
        PonistiUnos();
    }

    // ---------- dogadjaj za alkohol ----------

    private void NaZabranjenPokusaj(DateTime vreme, Artikal artikal)
    {
        try
        {
            _dnevnik.Zapisi(vreme, artikal);
            Poruka("Prodaja alkoholnih pića nije dozvoljena posle 22h. Pokušaj je zabeležen.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Poruka("Prodaja alkohola nije dozvoljena posle 22h, ali pokušaj nije mogao da se zabeleži:\n" + ex.Message);
        }
    }

    // ---------- pomocne ----------

    private static void Poruka(string tekst) =>
        MessageBox.Show(tekst, "Prodavnica NG", MessageBoxButtons.OK, MessageBoxIcon.Information);

    private static void Pokusaj(Action akcija)
    {
        try { akcija(); }
        catch (SqliteException ex)
        {
            MessageBox.Show("Greška u radu sa bazom:\n" + ex.Message, "Prodavnica NG", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
