using MySqlConnector;

namespace Mokki_softa
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnDatabaseClicked(object sender, EventArgs e)
        {
            DatabaseConnector dbc = new DatabaseConnector();
            try
            {
                var conn = dbc._getConnection();
                conn.Open();
                await DisplayAlert("Onnistui", "Tietokanta yhteys aukesi", "OK");
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Tietokanta yhteys epäonnistui", ex.Message, "OK");
            }
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            // Numeeristen kenttien tarkistus, sanitoidaan syöttöä
            if (!double.TryParse(entryHinta.Text, out double hinta) ||
                !int.TryParse(entryHenkilomaara.Text, out int henkilomaara) ||
                !int.TryParse(entryAlueId.Text, out int alueId))
            {
                await DisplayAlert("Virhe", "Hinnan, Henkilömäärän, sekä AlueIDn täytyy olla numeerisia.", "OK");
                return;
            }

            var mokki = new Mokki
            {
                Mokkinimi = entryMokkiNimi.Text,
                Katuosoite = entryKatuosoite.Text,
                Hinta = hinta,
                Kuvaus = entryKuvaus.Text,
                Henkilomaara = henkilomaara,
                Varustelu = entryVarustelu.Text,
                AlueId = alueId,
                Postinro = entryPostinro.Text
            };

            bool isSuccess = await SaveDataToDatabase(mokki);
            if (isSuccess)
            {
                await DisplayAlert("Onnistui!", "Tiedot lisätty", "OK");
            }
            else
            {
                await DisplayAlert("Virhe", "Tietojen lisäys epäonnistui", "OK");
            }
        }

        private async Task<bool> SaveDataToDatabase(Mokki mokki)
        {
            //WIP
            return await Task.FromResult(true);
        }
    }

}
