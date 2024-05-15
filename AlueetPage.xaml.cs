using MySqlConnector;
using System.Data.Common;

namespace Mokki_softa
{
    public partial class AlueetPage : ContentPage
    {
        private DatabaseConnector dbConnector;
        public AlueetPage()
        {
            InitializeComponent();
            var appSettings = ConfigurationProvider.GetAppSettings();
            dbConnector = new DatabaseConnector(appSettings);
            LoadAlueetIntoPicker(dbConnector);
        }
        
        private async Task<bool> UpdateAlueToDatabase(int alueId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string checkQuery = "SELECT COUNT(*) FROM alue WHERE alue_id = @alueId";
                using var checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@alueId", alueId);
                long count = (long)await checkCmd.ExecuteScalarAsync();

                if (count > 0)
                {
                    // Update existing alue data
                    string updateQuery = "UPDATE alue SET nimi = @nimi WHERE alue_id = @alueId";

                    using var updateCmd = new MySqlCommand(updateQuery, conn);
                    updateCmd.Parameters.AddWithValue("@nimi", entryAlueNimi.Text);
                    updateCmd.Parameters.AddWithValue("@alueId", alueId);

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
                Console.WriteLine($"Virhe alueen tietojen päivityksessä: {ex.Message}");
                return false;
            }
        }

        private async void PaivitaAlue_Clicked(object sender, EventArgs e)
        {

            // Tarkistetaan, että kaikki kentät ovat täytettyjä
            if (!AreAllFieldsFilled())
            {
                await DisplayAlert("Virhe", "Tarkista, että kaikki kentät ovat täytettyjä.", "OK");
                return;
            }

            // Tarkistetaan, että kenttien pituus on oikea
            if (!AreFieldLengthsValid())
            {
                await DisplayAlert("Virhe", "Tarkista, että kenttien pituus on oikea.", "OK");
                return;
            }

            // Tarkistetaan, ettei kenttiin ole syötetty SQL-injektioita
            if (ContainsSQLInjection(entryAlueNimi.Text))
            {
                await DisplayAlert("Virhe", "Tarkista, että kentissä ei ole kiellettyjä erikoismerkkejä.", "OK");
                return;
            }

            // Funktio tarkistaa, että kaikki kentät ovat täytettyjä
            bool AreAllFieldsFilled()
            {
                return !string.IsNullOrWhiteSpace(entryAlueNimi.Text);
                       
            }

            // Funktio tarkistaa, että kenttien pituus on oikea (max)
            bool AreFieldLengthsValid()
            {
                return entryAlueNimi.Text.Length <= 40;
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

            if (pickerAlue.SelectedItem == null)
            {
                await DisplayAlert("Virhe", "Valitse ensin alue päivitettäväksi.", "OK");
                return;
            }

            // Otetaan valitusta itemistä vain alueen id
            string selectedItem = (string)pickerAlue.SelectedItem;
            int alueId = int.Parse(selectedItem.Split(':')[0]);

            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);


            bool isSuccess = await UpdateAlueToDatabase(alueId, dbConnector);
            if (isSuccess)
            {
                await DisplayAlert("Onnistui!", "Alueen tiedot päivitetty!", "OK");
                await LoadAlueetIntoPicker(dbConnector); // Päivitetään pickerin tiedot
                // Kenttien tyhjennys
                ClearAlueFields();
            }
            else
            {
                await DisplayAlert("Virhe", "Alueen tietojen päivitys epäonnistui.", "OK");
            }
        }

        private async void OnSaveAlueClicked(object sender, EventArgs e)
        {

            // Funktio tarkistaa, että kaikki kentät ovat täytettyjä
            bool AreAllFieldsFilled()
            {
                return !string.IsNullOrWhiteSpace(entryAlueNimi.Text);
            }

            // Funktio tarkistaa, että kenttien pituus on oikea (max)
            bool AreFieldLengthsValid()
            {
                return entryAlueNimi.Text.Length <= 40;
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

            if (!AreAllFieldsFilled() || !AreFieldLengthsValid() || ContainsSQLInjection(entryAlueNimi.Text))
            {
                // Display an error message if any of the checks fail
                await DisplayAlert("Virhe", "Tarkista, että kaikki tiedot ovat oikein ja kentissä ei ole kiellettyjä erikoismerkkejä.", "OK");
                return; // Return to exit the method without proceeding
            }
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            
            bool isSuccess = await SaveAlueToDatabase(dbConnector);
            if (isSuccess)
            {
                await DisplayAlert("Onnistui!", "Uusi alue lisätty", "OK");
                await LoadAlueetIntoPicker(dbConnector);
                // Kenttien tyhjennys
                ClearAlueFields();
            }
            else
            {
                await DisplayAlert("Virhe", "Uuden alueen lisäys epäonnistui!", "OK");
            }
        }
        public async void PoistaAlue_Clicked(object sender, EventArgs e)
        {
            if (pickerAlue.SelectedItem == null)
            {
                await DisplayAlert("Virhe", "Valitse ensin alue poistettavaksi.", "OK");
                return;
            }
            // Otetaan valitusta itemistä vain alue
            string selectedItem = (string)pickerAlue.SelectedItem;
            int alueId = int.Parse(selectedItem.Split(':')[0]);

            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            bool isSuccess = await RemoveAlueData(alueId, dbConnector);
            if (isSuccess)
            {
                await DisplayAlert("Onnistui!", "Alueen tiedot poistettu!", "OK");
                await LoadAlueetIntoPicker(dbConnector);
                // Kenttien tyhjennys
                ClearAlueFields();
            }
            else
            {
                await DisplayAlert("Virhe", "Alueen tietojen poisto epäonnistui.", "OK");
            }

        }
        // Alueen tietojen poisto
        private async Task<bool> RemoveAlueData(int alueId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string deleteQuery = "DELETE FROM alue WHERE alue_id = @alueId";
                using var deleteCmd = new MySqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@mokkiId", alueId);
                await deleteCmd.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Virhe alueen tietojen poistamisessa: {ex.Message}");
                return false;
            }
        }
        private void TyhjennaAlue_Clicked(object sender, EventArgs e)
        {
            ClearAlueFields();
        }
        private void ClearAlueFields()
        {
            entryAlueNimi.Text = string.Empty;
            
        }
        private async Task<bool> SaveAlueToDatabase(DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string insertQuery = "INSERT INTO alue (nimi) VALUES (@nimi)";


                using var insertCmd = new MySqlCommand(insertQuery, conn);
                insertCmd.Parameters.AddWithValue("@nimi", entryAlueNimi.Text);

                int rowsAffected = await insertCmd.ExecuteNonQueryAsync();


                return rowsAffected > 0;
            }
            catch (Exception ex)

            {
                Console.WriteLine($"Virhe tietojen lisäyksessä tietokantaan: {ex.Message}");
                return false;
            }
        }


        private async Task LoadAlueetIntoPicker(DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string query = "SELECT alue_id, nimi FROM alue";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                List<string> alueNames = new List<string>();
                while (reader.Read())
                {
                    int alueId = reader.GetInt32("alue_id");
                    string nimi = reader.GetString("nimi");
                    

                    alueNames.Add($"{alueId}: {nimi}");
                }

                pickerAlue.ItemsSource = alueNames;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", $"Virhe alueiden lataamisessa Pickeriin: {ex.Message}", "OK");
            }
        }

        private async void PaivitaLista_Clicked(object sender, EventArgs e)
        {
            await LoadAlueetIntoPicker(dbConnector);
        }


        private async void OnAlueSelectedIndexChanged(object sender, EventArgs e)
        {
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            if (pickerAlue.SelectedItem != null)
            {
                
                string selectedItem = (string)pickerAlue.SelectedItem;
                int alueId = int.Parse(selectedItem.Split(':')[0]);
                await LoadAlueData(alueId, dbConnector);
            }
        }

        // Näytä pickeristä valitun alueen tiedot entry-kentissä
        private async Task LoadAlueData(int alueId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string query = "SELECT * FROM alue WHERE alue_id = @alueId";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@alueId", alueId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (reader.Read())
                {
                    entryAlueNimi.Text = reader["nimi"].ToString();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", $"Virhe alueiden lataamisessa: {ex.Message}", "OK");
            }
        }
    }
}
