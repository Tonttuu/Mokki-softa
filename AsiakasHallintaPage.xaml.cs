using MySqlConnector;

namespace Mokki_softa
{
    public partial class AsiakasHallintaPage : ContentPage
    {
        public AsiakasHallintaPage()
        {
            InitializeComponent();
        }
        

        // asiakkaan tietojen lisääminen tietokantaan
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


        // Asiakkaan poistaminen tietokannasta
        public async void OnAsiakasDeleteClicked(object sender, EventArgs e)
{
    if (!int.TryParse(entryAsiakasID.Text, out int asiakasId))
    {
        await DisplayAlert("Virhe", "Asiakas ID:n täytyy olla numeerinen.", "OK");
        return;
    }

    var appSettings = ConfigurationProvider.GetAppSettings();
    var dbConnector = new DatabaseConnector(appSettings);

    bool isCustomerExists = await CheckIfCustomerExists(asiakasId, dbConnector);
    if (isCustomerExists)
    {
        bool isSuccess = await RemoveCustomerData(asiakasId, dbConnector);
        if (isSuccess)
        {
            await DisplayAlert("Onnistui!", "Asiakkaan tiedot poistettu", "OK");
        }
        else
        {
            await DisplayAlert("Virhe", "Asiakkaan tietojen poisto epäonnistui", "OK");
        }
    }
    else
    {
        await DisplayAlert("Virhe", "Asiakasta ei löydy", "OK");
    }
}

private async Task<bool> CheckIfCustomerExists(int asiakasId, DatabaseConnector dbConnector)
{
    try
    {
        using var conn = dbConnector.GetConnection();
        await conn.OpenAsync();

        string query = "SELECT COUNT(*) FROM asiakas WHERE asiakas_id = @asiakasId";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@asiakasId", asiakasId);

        long count = (long)await cmd.ExecuteScalarAsync();
        return count > 0;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Virhe asiakkaan olemassaolon tarkistuksessa: {ex.Message}");
        return false;
    }
}

private async Task<bool> RemoveCustomerData(int asiakasId, DatabaseConnector dbConnector)
{
    try
    {
        using var conn = dbConnector.GetConnection();
        await conn.OpenAsync();

        string deleteQuery = "DELETE FROM asiakas WHERE asiakas_id = @asiakasId";
        using var deleteCmd = new MySqlCommand(deleteQuery, conn);
        deleteCmd.Parameters.AddWithValue("@asiakasId", asiakasId);
        await deleteCmd.ExecuteNonQueryAsync();

        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Virhe asiakkaan tietojen poistamisessa: {ex.Message}");
        return false;
    }
}
        
  }
      
    
}
