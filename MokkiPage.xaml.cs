using MySqlConnector;
using System.Data.Common;

namespace Mokki_softa
{
    public partial class MokkiPage : ContentPage
    {
        private DatabaseConnector dbConnector;
        
        public MokkiPage()
        {
            InitializeComponent();
            var appSettings = ConfigurationProvider.GetAppSettings();
            dbConnector = new DatabaseConnector(appSettings);
            LoadMokitIntoPicker(dbConnector);
        }

        private async Task<bool> UpdateMokkiToDatabase(int mokkiId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string checkQuery = "SELECT COUNT(*) FROM mokki WHERE mokki_id = @mokkiId";
                using var checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@mokkiId", mokkiId);
                long count = (long)await checkCmd.ExecuteScalarAsync();

                if (count > 0)
                {

                    // Update existing mokki data
                    string updateQuery = "UPDATE mokki SET alue_id = @alue_id, postinro = @postinro, mokkinimi = @mokkinimi, katuosoite = @katuosoite, hinta = @hinta, kuvaus = @kuvaus, henkilomaara = @henkilomaara, varustelu = @varustelu";
                    updateQuery += " WHERE mokki_id = @mokkiId";

                    using var updateCmd = new MySqlCommand(updateQuery, conn);
                    updateCmd.Parameters.AddWithValue("@alue_Id", entryAlueId.Text);
                    updateCmd.Parameters.AddWithValue("@postinro", entryPostinro.Text);
                    updateCmd.Parameters.AddWithValue("@mokkinimi", entryMokkiNimi.Text);
                    updateCmd.Parameters.AddWithValue("@katuosoite", entryKatuosoite.Text);
                    updateCmd.Parameters.AddWithValue("@hinta", entryHinta.Text);
                    updateCmd.Parameters.AddWithValue("@kuvaus", entryKuvaus.Text);
                    updateCmd.Parameters.AddWithValue("@henkilomaara", entryHenkilomaara.Text);
                    updateCmd.Parameters.AddWithValue("@varustelu", entryVarustelu.Text);
                    updateCmd.Parameters.AddWithValue("@mokkiId", mokkiId);

                    int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                    return rowsAffected > 0;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Virhe mökin tietojen päivityksessä: {ex.Message}");
                return false;
            }
        }

        private async void PaivitaMokki_Clicked(object sender, EventArgs e)
        {

            // Tarkistetaan, että kaikki kentät ovat täytettyjä
            if (!AreAllFieldsFilled())
            {
                await DisplayAlert("Virhe", "Tarkista, että kaikki kentät ovat täytettyjä.", "OK");
                return;
            }

            // Tarkistetaan, että alue id, hinta, sekä henkilömäärä ovat numeerisia
            if (!AreNumericFieldsNumeric())
            {
                await DisplayAlert("Virhe", "Tarkista, että alue id, hinta, sekä henkilömäärä ovat numeerisia. Alue_id max pituus 10, postinumeron 5, sekä henkilömäärän 11.", "OK");
                return;
            }

            // Tarkistetaan, että kenttien pituus on oikea
            if (!AreFieldLengthsValid())
            {
                await DisplayAlert("Virhe", "Tarkista, että kenttien pituus on oikea.", "OK");
                return;
            }

            // Tarkistetaan, ettei kenttiin ole syötetty SQL-injektioita
            if (ContainsSQLInjection(entryAlueId.Text) || ContainsSQLInjection(entryPostinro.Text) ||
                ContainsSQLInjection(entryMokkiNimi.Text) || ContainsSQLInjection(entryKatuosoite.Text) ||
                ContainsSQLInjection(entryHinta.Text) || ContainsSQLInjection(entryKuvaus.Text) || 
                ContainsSQLInjection(entryHenkilomaara.Text) || ContainsSQLInjection(entryVarustelu.Text))
            {
                await DisplayAlert("Virhe", "Tarkista, että kentissä ei ole kiellettyjä erikoismerkkejä.", "OK");
                return;
            }

            // Funktio tarkistaa, että kaikki kentät ovat täytettyjä
            bool AreAllFieldsFilled()
            {
                return !string.IsNullOrWhiteSpace(entryAlueId.Text) &&
                       !string.IsNullOrWhiteSpace(entryPostinro.Text) &&
                       !string.IsNullOrWhiteSpace(entryMokkiNimi.Text) &&
                       !string.IsNullOrWhiteSpace(entryKatuosoite.Text) &&
                       !string.IsNullOrWhiteSpace(entryHinta.Text) &&
                       !string.IsNullOrWhiteSpace(entryKuvaus.Text) &&
                       !string.IsNullOrWhiteSpace(entryHenkilomaara.Text) &&
                       !string.IsNullOrWhiteSpace(entryVarustelu.Text);
            }

            // Funktio tarkistaa, että alue id, hinta, sekä henkilömäärä ovat numeerisia
            bool AreNumericFieldsNumeric()
            {
                return int.TryParse(entryAlueId.Text, out _) && int.TryParse(entryPostinro.Text, out _) && double.TryParse(entryHinta.Text, out _);
            }

            // Funktio tarkistaa, että kenttien pituus on oikea (max)
            bool AreFieldLengthsValid()
            {
                return entryAlueId.Text.Length <= 10 &&
                       entryPostinro.Text.Length == 5 &&
                       entryMokkiNimi.Text.Length <= 45 &&
                       entryKatuosoite.Text.Length <= 45 &&
                       entryHinta.Text.Length <= 10 &&
                       entryKuvaus.Text.Length <= 150 &&
                       entryHenkilomaara.Text.Length <= 11 &&
                       entryVarustelu.Text.Length <= 100 ;
            }

            // Funktio tarkistaa, sisältääkö syöte kiellettyjä SQL-injektiomerkkejä
            bool ContainsSQLInjection(string input)
            {
                string[] forbiddenChars = { "'", ";", "--", "/*", "*/" }; // Esimerkkejä kielletyistä merkeistä
                foreach (var forbiddenChar in forbiddenChars)
                {
                    if (input.Contains(forbiddenChar))
                    {
                        return true; // Palauta true, jos kielletty merkki löytyy
                    }
                }
                return false; // Palauta false, jos kiellettyä merkkiä ei löytynyt
            }

            if (pickerMokit.SelectedItem == null)
            {
                await DisplayAlert("Virhe", "Valitse ensin mökki päivitettäväksi.", "OK");
                return;
            }

            // Otetaan valitusta itemistä vain mökin id
            string selectedItem = (string)pickerMokit.SelectedItem;
            int mokkiId = int.Parse(selectedItem.Split(':')[0]);

            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

           
            bool isSuccess = await UpdateMokkiToDatabase(mokkiId, dbConnector);
            if (isSuccess)
            {
                await DisplayAlert("Onnistui!", "Mökin tiedot päivitetty!", "OK");
                await LoadMokitIntoPicker(dbConnector); // Päivitetään pickerin tiedot
            }
            else
            {
                await DisplayAlert("Virhe", "Mökin tietojen päivitys epäonnistui.", "OK");
            }
        }

        private async void OnMokkiSubmitClicked(object sender, EventArgs e)
        {

            // Funktio tarkistaa, että kaikki kentät ovat täytettyjä
            bool AreAllFieldsFilled()
            {
                return !string.IsNullOrWhiteSpace(entryAlueId.Text) &&
                       !string.IsNullOrWhiteSpace(entryPostinro.Text) &&
                       !string.IsNullOrWhiteSpace(entryMokkiNimi.Text) &&
                       !string.IsNullOrWhiteSpace(entryKatuosoite.Text) &&
                       !string.IsNullOrWhiteSpace(entryHinta.Text) &&
                       !string.IsNullOrWhiteSpace(entryKuvaus.Text) &&
                       !string.IsNullOrWhiteSpace(entryHenkilomaara.Text) &&
                       !string.IsNullOrWhiteSpace(entryVarustelu.Text);
            }

            // Funktio tarkistaa, että alue id, hinta, sekä henkilömäärä ovat numeerisia
            bool AreNumericFieldsNumeric()
            {
                return int.TryParse(entryAlueId.Text, out _) && int.TryParse(entryPostinro.Text, out _) && double.TryParse(entryHinta.Text, out _);
            }

            // Funktio tarkistaa, että kenttien pituus on oikea (max)
            bool AreFieldLengthsValid()
            {
                return entryAlueId.Text.Length <= 10 &&
                       entryPostinro.Text.Length == 5 &&
                       entryMokkiNimi.Text.Length <= 45 &&
                       entryKatuosoite.Text.Length <= 45 &&
                       entryHinta.Text.Length <= 10 &&
                       entryKuvaus.Text.Length <= 150 &&
                       entryHenkilomaara.Text.Length <= 11 &&
                       entryVarustelu.Text.Length <= 100 ;
            }

            // Funktio tarkistaa, sisältääkö syöte kiellettyjä SQL-injektiomerkkejä
            bool ContainsSQLInjection(string input)
            {
                string[] forbiddenChars = { "'", ";", "--", "/*", "*/" }; // Esimerkkejä kielletyistä merkeistä
                foreach (var forbiddenChar in forbiddenChars)
                {
                    if (input.Contains(forbiddenChar))
                    {
                        return true; // Palauta true, jos kielletty merkki löytyy
                    }
                }
                return false; // Palauta false, jos kiellettyä merkkiä ei löytynyt
            }

            if (!AreAllFieldsFilled() || !AreNumericFieldsNumeric() || !AreFieldLengthsValid() || 
                ContainsSQLInjection(entryAlueId.Text) || ContainsSQLInjection(entryPostinro.Text) || 
                ContainsSQLInjection(entryMokkiNimi.Text) || ContainsSQLInjection(entryKatuosoite.Text) || 
                ContainsSQLInjection(entryHinta.Text) || ContainsSQLInjection(entryKuvaus.Text) || 
                ContainsSQLInjection(entryHenkilomaara.Text) || ContainsSQLInjection(entryVarustelu.Text))
            {
                // Display an error message if any of the checks fail
                await DisplayAlert("Virhe", "Tarkista, että kaikki tiedot ovat oikein ja kentissä ei ole kiellettyjä erikoismerkkejä.", "OK");
                return; // Return to exit the method without proceeding
            }
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

                // Uusi mökki, lisää uudet tiedot tietokantaan
                bool isSuccess = await SaveNewMokkiToDatabase(dbConnector);
                if (isSuccess)
                {
                    await DisplayAlert("Onnistui!", "Uusi mökki lisätty", "OK");
                }
                else
                {
                    await DisplayAlert("Virhe", "Uuden mökin lisäys epäonnistui!", "OK");
                }
        }
        public async void OnMokkiDeleteClicked(object sender, EventArgs e)
        {
            if (pickerMokit.SelectedItem == null)
            {
                await DisplayAlert("Virhe", "Valitse ensin mökki poistettavaksi.", "OK");
                return;
            }
            // Otetaan valitusta itemistä vain mökin id
            string selectedItem = (string)pickerMokit.SelectedItem;
            int mokkiId = int.Parse(selectedItem.Split(':')[0]);

            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            bool isSuccess = await RemoveMokkiData(mokkiId, dbConnector);
            if (isSuccess)
            {
                await DisplayAlert("Onnistui!", "Mökin tiedot poistettu!", "OK");
                await LoadMokitIntoPicker(dbConnector);
                // Kenttien tyhjennys
                ClearMokkiFields();
            }
            else
            {
                await DisplayAlert("Virhe", "Mökin tietojen poisto epäonnistui.", "OK");
            }

        } 
        // Mökin tietojen poisto
        private async Task<bool> RemoveMokkiData(int mokkiId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string deleteQuery = "DELETE FROM mokki WHERE mokki_id = @mokkiId";
                using var deleteCmd = new MySqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@mokkiId", mokkiId);
                await deleteCmd.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Virhe mökin tietojen poistamisessa: {ex.Message}");
                return false;
            }
        }
        private void TyhjennaMokki_Clicked(object sender, EventArgs e)
        {
            ClearMokkiFields();
        }
        private void ClearMokkiFields()
        {
            entryAlueId.Text = string.Empty;
            entryPostinro.Text = string.Empty;
            entryMokkiNimi.Text = string.Empty;
            entryKatuosoite.Text = string.Empty;
            entryHinta.Text = string.Empty;
            entryKuvaus.Text = string.Empty;
            entryHenkilomaara.Text = string.Empty;
            entryVarustelu.Text = string.Empty;


        }
        private async Task<bool> SaveNewMokkiToDatabase(DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string insertQuery = "INSERT INTO mokki (alue_id, postinro, mokkinimi, katuosoite, hinta, kuvaus, henkilomaara, varustelu) " +
                               "VALUES (@alue_Id, @postinro, @mokkinimi, @katuosoite, @hinta, @kuvaus, @henkilomaara, @varustelu)";

               
                using var insertCmd = new MySqlCommand(insertQuery, conn);
                insertCmd.Parameters.AddWithValue("@alue_Id", entryAlueId.Text);
                insertCmd.Parameters.AddWithValue("@postinro", entryPostinro.Text);
                insertCmd.Parameters.AddWithValue("@mokkinimi", entryMokkiNimi.Text);
                insertCmd.Parameters.AddWithValue("@katuosoite", entryKatuosoite.Text);
                insertCmd.Parameters.AddWithValue("@hinta", entryHinta.Text);
                insertCmd.Parameters.AddWithValue("@kuvaus", entryKuvaus.Text);
                insertCmd.Parameters.AddWithValue("@henkilomaara", entryHenkilomaara.Text);
                insertCmd.Parameters.AddWithValue("@varustelu", entryVarustelu.Text);

                
                int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            
            {
                Console.WriteLine($"Virhe tietojen lisäyksessä tietokantaan: {ex.Message}");
                return false;
            }
        }

        
        private async Task LoadMokitIntoPicker(DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string query = "SELECT mokki_id, mokkinimi, alue_id FROM Mokki";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                List<string> mokkiNames = new List<string>();
                while (reader.Read())
                {
                    int MokkiId = reader.GetInt32("mokki_id");
                    string Mokkinimi = reader.GetString("Mokkinimi");
                    int AlueId = reader.GetInt32("alue_id");
                    
                    mokkiNames.Add($"{MokkiId}: {Mokkinimi} {AlueId}");
                }

                pickerMokit.ItemsSource = mokkiNames;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", $"Virhe mökkien lataamisessa Pickeriin: {ex.Message}", "OK");
            }
        }

        private async void PaivitaLista_Clicked(object sender, EventArgs e)
        {
            await LoadMokitIntoPicker(dbConnector);
        }


        private async void OnMokkiSelectedIndexChanged(object sender, EventArgs e)
        {
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            if (pickerMokit.SelectedItem != null)
            {
                string selectedItem = (string)pickerMokit.SelectedItem;
                int mokkiId = int.Parse(selectedItem.Split(':')[0]);
                await LoadMokkiData(mokkiId, dbConnector);
            }
        }

        // Näytä pickeristä valitun mökin tiedot entry-kentissä
        private async Task LoadMokkiData(int mokkiId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string query = "SELECT * FROM mokki WHERE mokki_id = @mokkiId";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@mokkiId", mokkiId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (reader.Read())
                {

                    entryAlueId.Text = reader["alue_id"].ToString();
                    entryPostinro.Text = reader["postinro"].ToString();
                    entryMokkiNimi.Text = reader["mokkinimi"].ToString();
                    entryKatuosoite.Text = reader["katuosoite"].ToString();
                    entryHinta.Text = reader["hinta"].ToString();
                    entryKuvaus.Text = reader["kuvaus"].ToString();
                    entryHenkilomaara.Text = reader["henkilomaara"].ToString();
                    entryVarustelu.Text = reader["varustelu"].ToString();

                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", $"Virhe mökkien lataamisessa: {ex.Message}", "OK");
            }
        }
    }
}
