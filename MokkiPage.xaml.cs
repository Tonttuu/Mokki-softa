using MySqlConnector;

namespace Mokki_softa
{
    public partial class MokkiPage : ContentPage
    {
        public MokkiPage()
        {
            InitializeComponent();
        }

        private async void OnDatabaseClicked(object sender, EventArgs e)
        {
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            try
            {
                var conn = dbConnector.GetConnection();
                conn.Open();
                await DisplayAlert("Onnistui", "Tietokanta yhteys aukesi", "OK");
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Tietokanta yhteys epäonnistui", ex.Message, "OK");
            }
        }

        private async void OnMokkiSubmitClicked(object sender, EventArgs e)
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
           
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            
            bool isSuccess = await SaveDataToDatabase(mokki, dbConnector);
            if (isSuccess)
            {
                await DisplayAlert("Onnistui!", "Tiedot lisätty", "OK");
            }
            else
            {
                await DisplayAlert("Virhe", "Tietojen lisäys epäonnistui", "OK");
            }
        }
        private async Task<bool> SaveDataToDatabase(Mokki mokki, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string query = "INSERT INTO mokki (alue_id, postinro, mokkinimi, katuosoite, hinta, kuvaus, henkilomaara, varustelu) " +
                               "VALUES (@alue_Id, @postinro, @mokkinimi, @katuosoite, @hinta, @kuvaus, @henkilomaara, @varustelu)";

               
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@alue_Id", mokki.AlueId);
                cmd.Parameters.AddWithValue("@postinro", mokki.Postinro);
                cmd.Parameters.AddWithValue("@mokkinimi", mokki.Mokkinimi);
                cmd.Parameters.AddWithValue("@katuosoite", mokki.Katuosoite);
                cmd.Parameters.AddWithValue("@hinta", mokki.Hinta);
                cmd.Parameters.AddWithValue("@kuvaus", mokki.Kuvaus);
                cmd.Parameters.AddWithValue("@henkilomaara", mokki.Henkilomaara);
                cmd.Parameters.AddWithValue("@varustelu", mokki.Varustelu);

                
                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            
            {
                Console.WriteLine($"Virhe tietojen lisäyksessä tietokantaan: {ex.Message}");
                return false;
            }
        }
    }
}
