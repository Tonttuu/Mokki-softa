using MySqlConnector;
using System.Data.Common;

namespace Mokki_softa
{
    public partial class PalvelutPage : ContentPage
    {
        private DatabaseConnector dbConnector;

        public PalvelutPage()
        {
            InitializeComponent();
            var appSettings = ConfigurationProvider.GetAppSettings();
            dbConnector = new DatabaseConnector(appSettings);
            LoadPalvelutIntoPicker(dbConnector);
        }

        // ei toimi, varmaan kirjoitusvirhe mitä en huomaa
        private async Task<bool> UpdatePalveluToDatabase(int palveluId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string checkQuery = "SELECT COUNT(*) FROM palvelu WHERE palvelu_id = @palveluId";
                using var checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@palveluId", palveluId);
                long count = (long)await checkCmd.ExecuteScalarAsync();

                if (count > 0)
                {

                    // Update existing palvelu data
                    string updateQuery = "UPDATE palvelu SET alue_id = @alue_id, nimi = @nimi, kuvaus = @kuvaus, hinta = @hinta, alv = @alv WHERE palvelu_id = @palveluId";

                    using var updateCmd = new MySqlCommand(updateQuery, conn);
                    updateCmd.Parameters.AddWithValue("@alue_id", AlueEntry.Text);
                    updateCmd.Parameters.AddWithValue("@nimi", NimiEntry.Text);
                    updateCmd.Parameters.AddWithValue("@kuvaus", KuvausEntry.Text);
                    updateCmd.Parameters.AddWithValue("@hinta", HintaEntry.Text);
                    updateCmd.Parameters.AddWithValue("@alv", ALVEntry.Text);
                    

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
                Console.WriteLine($"Virhe palvelun tietojen päivityksessä: {ex.Message}");
                return false;
            }
        }

        private async void PaivitaPalvelu_Clicked(object sender, EventArgs e)
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
                await DisplayAlert("Virhe", "Tarkista, että alue id, hinta, sekä ALV ovat numeerisia. Alue_id max pituus 10, hinnan 8,2, sekä ALV:in 8,2.", "OK");
                return;
            }

            // Tarkistetaan, että kenttien pituus on oikea
            if (!AreFieldLengthsValid())
            {
                await DisplayAlert("Virhe", "Tarkista, että kenttien pituus on oikea.", "OK");
                return;
            }

            // Tarkistetaan, ettei kenttiin ole syötetty SQL-injektioita
            if (ContainsSQLInjection(AlueEntry.Text) || ContainsSQLInjection(NimiEntry.Text) ||
                ContainsSQLInjection(KuvausEntry.Text) || ContainsSQLInjection(HintaEntry.Text) ||
                ContainsSQLInjection(ALVEntry.Text))
            {
                await DisplayAlert("Virhe", "Tarkista, että kentissä ei ole kiellettyjä erikoismerkkejä.", "OK");
                return;
            }

            // Funktio tarkistaa, että kaikki kentät ovat täytettyjä
            bool AreAllFieldsFilled()
            {
                return !string.IsNullOrWhiteSpace(AlueEntry.Text) &&
                       !string.IsNullOrWhiteSpace(NimiEntry.Text) &&
                       !string.IsNullOrWhiteSpace(KuvausEntry.Text) &&
                       !string.IsNullOrWhiteSpace(HintaEntry.Text) &&
                       !string.IsNullOrWhiteSpace(ALVEntry.Text);
            }

            // Funktio tarkistaa, että alue id, hinta, sekä henkilömäärä ovat numeerisia
            bool AreNumericFieldsNumeric()
            {
                return int.TryParse(AlueEntry.Text, out _) && double.TryParse(HintaEntry.Text, out _) && double.TryParse(ALVEntry.Text, out _);
            }

            // Funktio tarkistaa, että kenttien pituus on oikea (max)
            bool AreFieldLengthsValid()
            {
                return AlueEntry.Text.Length <= 10 &&
                       NimiEntry.Text.Length <= 40 &&
                       KuvausEntry.Text.Length <= 255 &&
                       HintaEntry.Text.Length <= 10 &&
                       ALVEntry.Text.Length <= 10;
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

            if (pickerPalvelu.SelectedItem == null)
            {
                await DisplayAlert("Virhe", "Valitse ensin mökki päivitettäväksi.", "OK");
                return;
            }

            // Otetaan valitusta itemistä vain palvelun id
            string selectedItem = (string)pickerPalvelu.SelectedItem;
            int palveluId = int.Parse(selectedItem.Split(':')[0]);

            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);


            bool isSuccess = await UpdatePalveluToDatabase(palveluId, dbConnector);
            if (isSuccess)
            {
                await DisplayAlert("Onnistui!", "Palvelun tiedot päivitetty!", "OK");
                await LoadPalvelutIntoPicker(dbConnector); // Päivitetään pickerin tiedot
                ClearPalveluFields();
            }
            else
            {
                await DisplayAlert("Virhe", "Palvelun tietojen päivitys epäonnistui.", "OK");
            }
        }

        private async void OnPalveluSubmitClicked(object sender, EventArgs e)
        {

            // Funktio tarkistaa, että kaikki kentät ovat täytettyjä
            bool AreAllFieldsFilled()
            {
                return !string.IsNullOrWhiteSpace(AlueEntry.Text) &&
                       !string.IsNullOrWhiteSpace(NimiEntry.Text) &&
                       !string.IsNullOrWhiteSpace(KuvausEntry.Text) &&
                       !string.IsNullOrWhiteSpace(HintaEntry.Text) &&
                       !string.IsNullOrWhiteSpace(ALVEntry.Text);
            }

            // Funktio tarkistaa, että alue id, hinta, sekä henkilömäärä ovat numeerisia
            bool AreNumericFieldsNumeric()
            {
                return int.TryParse(AlueEntry.Text, out _) && double.TryParse(HintaEntry.Text, out _) && double.TryParse(ALVEntry.Text, out _);
            }

            // Funktio tarkistaa, että kenttien pituus on oikea (max)
            bool AreFieldLengthsValid()
            {
                return AlueEntry.Text.Length <= 10 &&
                       NimiEntry.Text.Length <= 40 &&
                       KuvausEntry.Text.Length <= 255 &&
                       HintaEntry.Text.Length <= 10 &&
                       ALVEntry.Text.Length <= 10;
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
                ContainsSQLInjection(AlueEntry.Text) || ContainsSQLInjection(NimiEntry.Text) ||
                ContainsSQLInjection(KuvausEntry.Text) || ContainsSQLInjection(HintaEntry.Text) ||
                ContainsSQLInjection(ALVEntry.Text))
            {
                // Display an error message if any of the checks fail
                await DisplayAlert("Virhe", "Tarkista, että kaikki tiedot ovat oikein ja kentissä ei ole kiellettyjä erikoismerkkejä.", "OK");
                return; // Return to exit the method without proceeding
            }
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            // Uusi palvelu, lisää uudet tiedot tietokantaan
            bool isSuccess = await SaveNewPalveluToDatabase(dbConnector);
            if (isSuccess)
            {
                await DisplayAlert("Onnistui!", "Uusi palvelu lisätty", "OK");
                await LoadPalvelutIntoPicker(dbConnector); // Päivitetään pickerin tiedot
                ClearPalveluFields();
            }
            else
            {
                await DisplayAlert("Virhe", "Uuden palvelun lisäys epäonnistui!", "OK");
            }
        }
        public async void OnPalveluDeleteClicked(object sender, EventArgs e)
        {
            if (pickerPalvelu.SelectedItem == null)
            {
                await DisplayAlert("Virhe", "Valitse ensin palvelu poistettavaksi.", "OK");
                return;
            }
            // Otetaan valitusta itemistä vain palvelun id
            string selectedItem = (string)pickerPalvelu.SelectedItem;
            int palveluId = int.Parse(selectedItem.Split(':')[0]);

            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            bool isSuccess = await RemovePalveluData(palveluId, dbConnector);
            if (isSuccess)
            {
                await DisplayAlert("Onnistui!", "Palvelun tiedot poistettu!", "OK");
                await LoadPalvelutIntoPicker(dbConnector);
                // Kenttien tyhjennys
                ClearPalveluFields();
            }
            else
            {
                await DisplayAlert("Virhe", "Palvelun tietojen poisto epäonnistui.", "OK");
            }

        }
        // Palvelun tietojen poisto
        private async Task<bool> RemovePalveluData(int palveluId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string deleteQuery = "DELETE FROM palvelu WHERE palvelu_id = @palveluId";
                using var deleteCmd = new MySqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@palveluId", palveluId);
                await deleteCmd.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Virhe palvelun tietojen poistamisessa: {ex.Message}");
                return false;
            }
        }
        private void TyhjennaPalvelu_Clicked(object sender, EventArgs e)
        {
            ClearPalveluFields();
        }
        private void ClearPalveluFields()
        {
            AlueEntry.Text = string.Empty;
            NimiEntry.Text = string.Empty;
            KuvausEntry.Text = string.Empty;
            HintaEntry.Text = string.Empty;
            ALVEntry.Text = string.Empty;
        }

        // ei toimi, varmaan kirjoitusvirhe mitä en huomaa
        private async Task<bool> SaveNewPalveluToDatabase(DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string insertQuery = "INSERT INTO palvelu (alue_id, nimi, kuvaus, hinta, alv) VALUES (@alue_id, @nimi, @kuvaus, @hinta, @alv)";


                using var insertCmd = new MySqlCommand(insertQuery, conn);
                insertCmd.Parameters.AddWithValue("@alue_id", AlueEntry.Text);
                insertCmd.Parameters.AddWithValue("@nimi", NimiEntry.Text);
                insertCmd.Parameters.AddWithValue("@kuvaus", KuvausEntry.Text);
                insertCmd.Parameters.AddWithValue("@hinta", HintaEntry.Text);
                insertCmd.Parameters.AddWithValue("@alv", ALVEntry.Text);
                
                int rowsAffected = await insertCmd.ExecuteNonQueryAsync();


                return rowsAffected > 0;
            }
            catch (Exception ex)

            {
                Console.WriteLine($"Virhe tietojen lisäyksessä tietokantaan: {ex.Message}");
                return false;
            }
        }


        private async Task LoadPalvelutIntoPicker(DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string query = "SELECT palvelu_id, nimi FROM palvelu";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                List<string> palveluNames = new List<string>();
                while (reader.Read())
                {
                    int palveluId = reader.GetInt32("palvelu_id");
                    string nimi = reader.GetString("nimi");
                    

                    palveluNames.Add($"{palveluId}: {nimi}");
                }

                pickerPalvelu.ItemsSource = palveluNames;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", $"Virhe palveluiden lataamisessa Pickeriin: {ex.Message}", "OK");
            }
        }

        private async void PaivitaLista_Clicked(object sender, EventArgs e)
        {
            await LoadPalvelutIntoPicker(dbConnector);
        }


        private async void OnPalveluSelectedIndexChanged(object sender, EventArgs e)
        {
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            if (pickerPalvelu.SelectedItem != null)
            {
                string selectedItem = (string)pickerPalvelu.SelectedItem;
                int palveluId = int.Parse(selectedItem.Split(':')[0]);
                await LoadPalveluData(palveluId, dbConnector);
            }
        }

        // Näytä pickeristä valitun palvelun tiedot entry-kentissä
        private async Task LoadPalveluData(int palveluId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string query = "SELECT * FROM palvelu WHERE palvelu_id = @palveluId";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@palveluId", palveluId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (reader.Read())
                {

                    AlueEntry.Text = reader["alue_id"].ToString();
                    NimiEntry.Text = reader["nimi"].ToString();
                    KuvausEntry.Text = reader["kuvaus"].ToString();
                    HintaEntry.Text = reader["hinta"].ToString();
                    ALVEntry.Text = reader["alv"].ToString();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", $"Virhe alueiden lataamisessa: {ex.Message}", "OK");
            }
        }
    }
}
