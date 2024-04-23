using MySqlConnector;

namespace Mokki_softa
{
    public partial class AsiakasHallintaPage : ContentPage
    {
        public AsiakasHallintaPage()
        {
            InitializeComponent();
        }
        
/*
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
*/
        public async void OnAsiakasSubmitClicked(object sender, EventArgs e)
        {
            // Numeeristen kenttien tarkistus, sanitoidaan syöttöä
            if (!double.TryParse(entryPostiNro.Text, out double hinta) ||
                !int.TryParse(entryPuhNro.Text, out int henkilomaara)) //||
                //!int.TryParse(entryAlueId.Text, out int alueId))
            {
                await DisplayAlert("Virhe", "Puhelinnumeron ja postinumeron täytyy olla numeerisia.", "OK");
                return;
            }
/*
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
  */         
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            
            bool isSuccess = await SaveDataToDatabase(/*mokki,*/ dbConnector);
            if (isSuccess)
            {
                await DisplayAlert("Onnistui!", "Tiedot lisätty", "OK");
            }
            else
            {
                await DisplayAlert("Virhe", "Tietojen lisäys epäonnistui", "OK");
            }
        }
        
        private async Task<bool> SaveDataToDatabase(/*Mokki mokki, */DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string query = "INSERT INTO asiakas (postinro, etunimi, sukunimi, lahiosoite, email, puhelinnro) " +
                               "VALUES (@postinro, @etunimi, @sukunimi, @lahiosoite, @email, @puhelinnro)";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@postinro", entryPostiNro.Text);
                cmd.Parameters.AddWithValue("@etunimi", entryEtuNimi.Text);
                cmd.Parameters.AddWithValue("@sukunimi", entrySukuNimi.Text);
                cmd.Parameters.AddWithValue("@lahiosoite", entryLahisoite.Text);
                cmd.Parameters.AddWithValue("@email", entryEmail.Text);
                cmd.Parameters.AddWithValue("@puhelinnro", entryPuhNro.Text);

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
