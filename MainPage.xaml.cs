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

/*
        private async Task<bool> SaveDataToDatabase(Mokki mokki)
        {
            //WIP
            return await Task.FromResult(true);
        }
    }
*/


    private async Task<bool> SaveDataToDatabase(Mokki mokki)
{
    try
    {
        using (var connection = new DatabaseConnector()._getConnection())
        {
            await connection.OpenAsync();

            string query = "INSERT INTO mokki (mokkinimi, katuosoite, hinta, kuvaus, henkilomaara, varustelu, alue_id ,postinro) " +
                           "VALUES (@mokkinimi, @katuosoite, @hinta, @kuvaus, @henkilomaara, @varustelu, @alue_id, @postinro)";
            
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@mokkinimi", mokki.Mokkinimi);
                command.Parameters.AddWithValue("@katuosoite", mokki.Katuosoite);
                command.Parameters.AddWithValue("@hinta", mokki.Hinta);
                command.Parameters.AddWithValue("@kuvaus", mokki.Kuvaus);
                command.Parameters.AddWithValue("@henkilomaara", mokki.Henkilomaara);
                command.Parameters.AddWithValue("@varustelu", mokki.Varustelu);
                command.Parameters.AddWithValue("@alue_id", mokki.AlueId);
                command.Parameters.AddWithValue("@postinro", mokki.Postinro);
                
                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Virhe tallennettaessa tietoja tietokantaan: " + ex.Message);
        return false;
    }
    }
}




}
