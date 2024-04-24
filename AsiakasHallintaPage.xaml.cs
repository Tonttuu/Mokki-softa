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
        // Asiakkaan tietojen lisääminen tietokantaan - funktio(t)
        public async void OnAsiakasSubmitClicked(object sender, EventArgs e)
        {
            // Numeeristen kenttien tarkistus, sanitoidaan syöttöä
            if (!double.TryParse(entryPostiNro.Text, out double hinta) ||
                !int.TryParse(entryPuhNro.Text, out int henkilomaara)) //||
            {
                await DisplayAlert("Virhe", "Puhelinnumeron ja postinumeron täytyy olla numeerisia.", "OK");
                return;
            }
     
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            bool isSuccess = await SaveDataToDatabase(dbConnector);
            if (isSuccess)
            {
                await DisplayAlert("Onnistui!", "Tiedot lisätty", "OK");
            }
            else
            {
                await DisplayAlert("Virhe", "Tietojen lisäys epäonnistui", "OK");
            }
        }
        
        private async Task<bool> SaveDataToDatabase(DatabaseConnector dbConnector)
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

*/


        // Asiakkaan poistaminen tietokannasta - funktio(t)
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
            await DisplayAlert("Onnistui!", "Asiakkaan tiedot poistettu!", "OK");
        }
        else
        {
            await DisplayAlert("Virhe", "Asiakkaan tietojen poisto epäonnistui!", "OK");
        }
    }
    else
    {
        await DisplayAlert("Virhe", "Asiakasta ei löydy tietokannasta", "OK");
    }
}

// Tarkistetaan, että onko poistettava asiakas tietokannassa.
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
        Console.WriteLine($"Virhe asiakkaan tarkistuksessa: {ex.Message}");
        return false;
    }
}

// jos asiakas tietokannassa => Poistetaan asiakkaan tiedot.
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
  




  // Asiakkaan tietojen lisääminen tai päivittäminen tietokantaan - funktio(t)
  
public async void OnAsiakasSubmitClicked(object sender, EventArgs e)
{


if (!int.TryParse(entryAsiakasID.Text, out int asiakasId))
{
    await DisplayAlert("Virhe", "Asiakas ID:n täytyy olla numeerinen.", "OK");
    return;
}


        // Tarkista, että kaikki kentät ovat täytettyjä
    if (string.IsNullOrWhiteSpace(entryPostiNro.Text) ||
        string.IsNullOrWhiteSpace(entryEtuNimi.Text) ||
        string.IsNullOrWhiteSpace(entrySukuNimi.Text) ||
        string.IsNullOrWhiteSpace(entryLahisoite.Text) ||
        string.IsNullOrWhiteSpace(entryEmail.Text) ||
        string.IsNullOrWhiteSpace(entryPuhNro.Text))
    {
        await DisplayAlert("Virhe", "Kaikki kentät ovat pakollisia.", "OK");
        return;
    }

    var appSettings = ConfigurationProvider.GetAppSettings();
    var dbConnector = new DatabaseConnector(appSettings);

    bool isSuccess = await SaveOrUpdateDataToDatabase(asiakasId, dbConnector);
    if (isSuccess)
    {
        await DisplayAlert("Onnistui!", "Tiedot päivitetty tai lisätty", "OK");
    }
    else
    {
        await DisplayAlert("Virhe", "Tietojen päivitys tai lisäys epäonnistui", "OK");
    }
}

private async Task<bool> SaveOrUpdateDataToDatabase(int asiakasId, DatabaseConnector dbConnector)
{
    try
    {
        using var conn = dbConnector.GetConnection();
        await conn.OpenAsync();

        // Tarkista ensin, onko asiakas jo olemassa
        string checkQuery = "SELECT COUNT(*) FROM asiakas WHERE asiakas_id = @asiakasId";
        using var checkCmd = new MySqlCommand(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("@asiakasId", asiakasId);
        long count = (long)await checkCmd.ExecuteScalarAsync();

        if (count > 0)
        {
            // Päivitä asiakkaan tiedot
            string updateQuery = "UPDATE asiakas SET postinro = @postinro, etunimi = @etunimi, sukunimi = @sukunimi, lahiosoite = @lahiosoite, email = @email, puhelinnro = @puhelinnro";
            updateQuery += " WHERE asiakas_id = @asiakasId";

            using var updateCmd = new MySqlCommand(updateQuery, conn);
            updateCmd.Parameters.AddWithValue("@postinro", entryPostiNro.Text);
            updateCmd.Parameters.AddWithValue("@etunimi", entryEtuNimi.Text);
            updateCmd.Parameters.AddWithValue("@sukunimi", entrySukuNimi.Text);
            updateCmd.Parameters.AddWithValue("@lahiosoite", entryLahisoite.Text);
            updateCmd.Parameters.AddWithValue("@email", entryEmail.Text);
            updateCmd.Parameters.AddWithValue("@puhelinnro", entryPuhNro.Text);
            updateCmd.Parameters.AddWithValue("@asiakasId", asiakasId);

            int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }
        else
        {
            // Lisää uusi asiakas
            string insertQuery = "INSERT INTO asiakas (postinro, etunimi, sukunimi, lahiosoite, email, puhelinnro) " +
                                "VALUES (@postinro, @etunimi, @sukunimi, @lahiosoite, @email, @puhelinnro)";

            using var insertCmd = new MySqlCommand(insertQuery, conn);
         //   insertCmd.Parameters.AddWithValue("@asiakasId", asiakasId);
            insertCmd.Parameters.AddWithValue("@postinro", entryPostiNro.Text);
            insertCmd.Parameters.AddWithValue("@etunimi", entryEtuNimi.Text);
            insertCmd.Parameters.AddWithValue("@sukunimi", entrySukuNimi.Text);
            insertCmd.Parameters.AddWithValue("@lahiosoite", entryLahisoite.Text);
            insertCmd.Parameters.AddWithValue("@email", entryEmail.Text);
            insertCmd.Parameters.AddWithValue("@puhelinnro", entryPuhNro.Text);

            int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Virhe tietojen tallennuksessa tai päivityksessä: {ex.Message}");
        return false;
    }
}










// -----------------------------------------------------------------
  }
  
  }
  