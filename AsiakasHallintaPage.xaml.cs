using MySqlConnector;

namespace Mokki_softa
{
    public partial class AsiakasHallintaPage : ContentPage
    {
        public AsiakasHallintaPage()
        {
            InitializeComponent();
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);
            LoadCustomersIntoPicker(dbConnector); // Asiakkaat Pickeriin
        }
    

// Asiakkaan tietpojen poisto
public async void OnAsiakasDeleteClicked(object sender, EventArgs e)
{
    if (pickerAsiakkaat.SelectedItem == null)
    {
        await DisplayAlert("Virhe", "Valitse ensin asiakas poistettavaksi.", "OK");
        return;
    }
   //int asiakasId = (int)pickerAsiakkaat.SelectedItem; //vanha, toimii id:llä
    // Otetaan valitusta itemistä vain asiakkaan id
    string selectedItem = (string)pickerAsiakkaat.SelectedItem;
    int asiakasId = int.Parse(selectedItem.Split(':')[0]);

    var appSettings = ConfigurationProvider.GetAppSettings();
    var dbConnector = new DatabaseConnector(appSettings);

        bool isSuccess = await RemoveCustomerData(asiakasId, dbConnector);
        if (isSuccess)
        {
            
            await DisplayAlert("Onnistui!", "Asiakkaan tiedot poistettu!", "OK");
            await LoadCustomersIntoPicker(dbConnector);
            // Kenttien tyhjennys
             ClearFields();
        }
        else
        {
            await DisplayAlert("Virhe", "Asiakkaan tietojen poisto epäonnistui!", "OK");
        }

}
private void ClearFields()
{
    entryEtuNimi.Text = string.Empty;
    entrySukuNimi.Text = string.Empty;
    entryLahisoite.Text = string.Empty;
    entryPostiNro.Text = string.Empty;
    entryPuhNro.Text = string.Empty;
    entryEmail.Text = string.Empty;
}



// Asiakkaan tietojen poisto
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
  
// Asiakkaan tietojen lisääminen tai päivittäminen
private async Task<bool> SaveOrUpdateDataToDatabase(int asiakasId, DatabaseConnector dbConnector)
{
    try
    {
        using var conn = dbConnector.GetConnection();
        await conn.OpenAsync();
        // Tarkistaan, onko pickeristä valittu jo olemassa oleva asiakas.
        string checkQuery = "SELECT COUNT(*) FROM asiakas WHERE asiakas_id = @asiakasId";
        using var checkCmd = new MySqlCommand(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("@asiakasId", asiakasId);
        long count = (long)await checkCmd.ExecuteScalarAsync();

        if (count > 0)
        {
            // Päivitä olemassa olevan asiakkaan tiedot
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
            // Lisätään uusi asiakas tietokantaan
            string insertQuery = "INSERT INTO asiakas (postinro, etunimi, sukunimi, lahiosoite, email, puhelinnro) " +
                                "VALUES (@postinro, @etunimi, @sukunimi, @lahiosoite, @email, @puhelinnro)";

            using var insertCmd = new MySqlCommand(insertQuery, conn);
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



// Asiakkaan lisäys/tietojen päivitys nappi & tarkistukset
public async void OnAsiakasSubmitClicked(object sender, EventArgs e)
{
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

    if (pickerAsiakkaat.SelectedItem != null)
    {
        // Asiakas valittu Pickeristä, päivitä asiakkaan tiedot
      //  int asiakasId = (int)pickerAsiakkaat.SelectedItem; //toimii, vanha
    // Otetaan valitusta itemistä vain asiakkaan id
    string selectedItem = (string)pickerAsiakkaat.SelectedItem; // korjattu
    int asiakasId = int.Parse(selectedItem.Split(':')[0]);

        bool isSuccess = await SaveOrUpdateDataToDatabase(asiakasId, dbConnector);
        if (isSuccess)
        {
            await DisplayAlert("Onnistui!", "Tiedot päivitetty", "OK");
             ClearFields();
        }
        else
        {
            await DisplayAlert("Virhe", "Tietojen päivitys epäonnistui", "OK");
        }
    }
    else
    {
        // Uusi asiakas, lisää uudet tiedot tietokantaan
        bool isSuccess = await SaveNewCustomerData(dbConnector);
        if (isSuccess)
        {
            await DisplayAlert("Onnistui!", "Uusi asiakas lisätty", "OK");
           // await LoadCustomersIntoPicker(dbConnector);
            ClearFields();
        }
        else
        {
            await DisplayAlert("Virhe", "Uuden asiakkaan lisäys epäonnistui", "OK");
        }
    }
}

// Uuden asiakkaan tietojen talletus
private async Task<bool> SaveNewCustomerData(DatabaseConnector dbConnector)
{
    try
    {
        using var conn = dbConnector.GetConnection();
        await conn.OpenAsync();

        string insertQuery = "INSERT INTO asiakas (postinro, etunimi, sukunimi, lahiosoite, email, puhelinnro) " +
                            "VALUES (@postinro, @etunimi, @sukunimi, @lahiosoite, @email, @puhelinnro)";

        using var insertCmd = new MySqlCommand(insertQuery, conn);
        insertCmd.Parameters.AddWithValue("@postinro", entryPostiNro.Text);
        insertCmd.Parameters.AddWithValue("@etunimi", entryEtuNimi.Text);
        insertCmd.Parameters.AddWithValue("@sukunimi", entrySukuNimi.Text);
        insertCmd.Parameters.AddWithValue("@lahiosoite", entryLahisoite.Text);
        insertCmd.Parameters.AddWithValue("@email", entryEmail.Text);
        insertCmd.Parameters.AddWithValue("@puhelinnro", entryPuhNro.Text);

        int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Virhe uuden asiakkaan tallennuksessa: {ex.Message}");
        return false;
    }
}



// Asiakkaat pickerissä (ID)
/*
private async void OnAsiakasSelectedIndexChanged(object sender, EventArgs e)
{
    var appSettings = ConfigurationProvider.GetAppSettings();
    var dbConnector = new DatabaseConnector(appSettings);

    if (pickerAsiakkaat.SelectedItem != null)
    {
        int selectedCustomerId = (int)pickerAsiakkaat.SelectedItem;
        await LoadCustomerData(selectedCustomerId, dbConnector);
    }
}
*/// toimiva yllä_ 

// Muutettu OnAsiakasSelectedIndexChanged, jotta se käsittelee asiakkaan id:n, etunimen ja sukunimen
private async void OnAsiakasSelectedIndexChanged(object sender, EventArgs e)
{
    var appSettings = ConfigurationProvider.GetAppSettings();
    var dbConnector = new DatabaseConnector(appSettings);

    if (pickerAsiakkaat.SelectedItem != null)
    {
        // Otetaan valitusta itemistä asiakkaan id, etunimi ja sukunimi
        string selectedItem = (string)pickerAsiakkaat.SelectedItem;
        int asiakasId = int.Parse(selectedItem.Split(':')[0]);
        await LoadCustomerData(asiakasId, dbConnector);
    }
}


// Hae kaikki asiakkaat tietokannasta ja aseta ne Pickerin ItemsSourceksi
/* toimiva alla
private async Task LoadCustomersIntoPicker(DatabaseConnector dbConnector)
{
    try
    {
        using var conn = dbConnector.GetConnection();
        await conn.OpenAsync();

        string query = "SELECT asiakas_id FROM asiakas";

        using var cmd = new MySqlCommand(query, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        List<int> customers = new List<int>();
        while (reader.Read())
        {
            int customerId = reader.GetInt32("asiakas_id");
            customers.Add(customerId);
        }

        pickerAsiakkaat.ItemsSource = customers;
    }
    catch (Exception ex)
    {
        await DisplayAlert("Virhe", $"Virhe asiakkaiden lataamisessa Pickeriin: {ex.Message}", "OK");
    }
}
*/
//testaus, uuusi:
private async Task LoadCustomersIntoPicker(DatabaseConnector dbConnector)
{
    try
    {
        using var conn = dbConnector.GetConnection();
        await conn.OpenAsync();

        string query = "SELECT asiakas_id, etunimi, sukunimi FROM asiakas";

        using var cmd = new MySqlCommand(query, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        List<string> customerNames = new List<string>();
        while (reader.Read())
        {
            int customerId = reader.GetInt32("asiakas_id");
            string etunimi = reader.GetString("etunimi");
            string sukunimi = reader.GetString("sukunimi");
            // Lisätään asiakkaan id ja etu- ja sukunimi listaan
            customerNames.Add($"{customerId}: {etunimi} {sukunimi}");
        }

        pickerAsiakkaat.ItemsSource = customerNames;
    }
    catch (Exception ex)
    {
        await DisplayAlert("Virhe", $"Virhe asiakkaiden lataamisessa Pickeriin: {ex.Message}", "OK");
    }
}




// Näytä pickeristä valitun asiakkaan tiedot entry-kentissä
private async Task LoadCustomerData(int asiakasId, DatabaseConnector dbConnector)
{
    try
    {
        using var conn = dbConnector.GetConnection();
        await conn.OpenAsync();

        string query = "SELECT * FROM asiakas WHERE asiakas_id = @asiakasId";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@asiakasId", asiakasId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (reader.Read())
        {
            // Aseta asiakastiedot käyttöliittymään
            entryPostiNro.Text = reader.GetString("postinro");
            entryEtuNimi.Text = reader.GetString("etunimi");
            entrySukuNimi.Text = reader.GetString("sukunimi");
            entryLahisoite.Text = reader.GetString("lahiosoite");
            entryEmail.Text = reader.GetString("email");
            entryPuhNro.Text = reader.GetString("puhelinnro");

        }
    }
    catch (Exception ex)
    {
        await DisplayAlert("Virhe", $"Virhe asiakkaiden lataamisessa: {ex.Message}", "OK");
    }
}

// -----------------------------------------------------------------
  }
  
  }
  