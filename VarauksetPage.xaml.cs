using MySqlConnector;

namespace Mokki_softa
{
    public partial class VarauksetPage : ContentPage
    {
        public VarauksetPage()
        {
            InitializeComponent();
            OnAppearing();
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);
            LoadVarausIntoPicker(dbConnector); // varaukset Pickeriin
             // PaivitaLista.Clicked += async (sender, e) => await HaeVaraukset();
             pickerVaraukset.SelectedIndexChanged += OnVarausSelectedIndexChanged;
        }
        
        // Uuden varauksen tietojen talletus
        private async Task<bool> SaveNewVarausData(Varaus varaus, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string query = "INSERT INTO varaus (varaus_id, mokki_id, asiakas_id, varattu_pvm, vahvistus_pvm, varattu_alkupvm, varattu_loppupvm) " +
                               "VALUES (@varaus_Id, @mokki_Id, @asiakas_Id, @varattu_pvm, @vahvistus_pvm, @varattu_alkupvm, @varattu_loppupvm)";

               
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@varaus_Id", varaus.VarausId);
                cmd.Parameters.AddWithValue("@mokki_Id", varaus.MokkiId);
                cmd.Parameters.AddWithValue("@asiakas_id", varaus.AsiakasId);
                cmd.Parameters.AddWithValue("@varattu_pvm", varaus.VarattuPvm);
                cmd.Parameters.AddWithValue("@vahvistus_pvm", varaus.VahvistusPvm);
                cmd.Parameters.AddWithValue("@varattu_alkupvm", varaus.VarattuAlkuPvm);
                cmd.Parameters.AddWithValue("@varattu_loppupvm", varaus.VarattuLoppuPvm);
                

                
                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            
            {
                Console.WriteLine($"Virhe uuden varauksen tallennuksessa: {ex.Message}");
                return false;
            }
        }
        

         public async void OnVarausSubmitClicked(object sender, EventArgs e)
        {
            
            // Numeeristen kenttien tarkistus, sanitoidaan syöttöä
            if (!int.TryParse(AsiakasIdEntry.Text, out int asiakasId) ||
                !int.TryParse(entryMokkiId.Text, out int mokkiId))
            {
                await DisplayAlert("Virhe", "Tarkista, että kaikki kentät ovat täytettyjä. Lisäksi AsiakasID:n ja Mökki ID:n täytyy olla numeerisia.", "OK");
                return;
            }

                    // Tarkistetaan, että DatePicker-ohjaimet on valittu
            if (varausDatePicker.Date == null || vahvistusDatePicker.Date == null || alkuDatePicker.Date == null || loppuDatePicker.Date == null)
            {
                await DisplayAlert("Virhe", "Valitse päivämäärät kaikkiin kenttiin.", "OK");
                return;
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

            // Tarkistetaan, ettei kenttiin ole syötetty SQL-injektioita
            if (ContainsSQLInjection(AsiakasIdEntry.Text) || ContainsSQLInjection(AsiakasIdEntry.Text) ||
                ContainsSQLInjection(entryMokkiId.Text) || ContainsSQLInjection(entryMokkiId.Text))
            {
                await DisplayAlert("Virhe", "Tarkista, että kentissä ei ole kiellettyjä erikoismerkkejä.", "OK");
                return;
            }

            // Funktio tarkistaa, että kenttien pituus on oikea (max)
            bool AreFieldLengthsValid()
            {
                return AsiakasIdEntry.Text.Length <= 10 && 
                    entryMokkiId.Text.Length <= 10;
            }

            var varaus = new Varaus
            {
                MokkiId = mokkiId,
                AsiakasId = asiakasId,
                VarattuPvm = varausDatePicker.Date,
                VahvistusPvm = vahvistusDatePicker.Date,
                VarattuAlkuPvm = alkuDatePicker.Date,
                VarattuLoppuPvm = loppuDatePicker.Date

                /*
                VarattuPvm = DateTime.Parse(entryVarausPvm.Text),
                VahvistusPvm = DateTime.Parse(entryVahvistusPvm.Text),
                VarattuAlkuPvm = DateTime.Parse(entryVarausAlku.Text),
                VarattuLoppuPvm = DateTime.Parse(entryVarausPäättyy.Text)*/
            };
           
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            
            bool isSuccess = await SaveNewVarausData(varaus, dbConnector);
            if (isSuccess)
            {
                await DisplayAlert("Onnistui!", "Varauksen tiedot lisätty", "OK");
                 await LoadVarausIntoPicker(dbConnector); // Päivitetään pickerin data
                 ClearFields(); // Tyhjennetään käyttöliittymän kentät
            }
            else
            {
                await DisplayAlert("Virhe", "Tietojen lisäys epäonnistui", "OK");
            }

        }

        // TÄTÄ PITÄISI SAADA KUTSUTTUA JONNEKIN

        private async Task<bool> UpdateVarausToDatabase(int varausId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string checkQuery = "SELECT COUNT(*) FROM varaus WHERE varaus_id = @varausId";
                using var checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@varausId", varausId);
                long count = (long)await checkCmd.ExecuteScalarAsync();

                if (count > 0)
                {

                    // Update existing varaus data
                    string updateQuery = "UPDATE varaus SET asiakas_id = @asiakas_id, mokki_id = @mokki_id, varattu_pvm = @varattu_pvm, vahvistus_pvm = @vahvistus_pvm, varattu_alkupvm = @varattu_alkupvm, varattu_loppupvm = @varattu_loppupvm";
                    updateQuery += " WHERE varaus_id = @varausId";

                    using var updateCmd = new MySqlCommand(updateQuery, conn);
                    updateCmd.Parameters.AddWithValue("@asiakas_Id", AsiakasIdEntry.Text);
                    updateCmd.Parameters.AddWithValue("@mokki_id", entryMokkiId.Text);
                    updateCmd.Parameters.AddWithValue("@varattu_pvm", varausDatePicker.Date);
                    updateCmd.Parameters.AddWithValue("@vahvistus_pvm", vahvistusDatePicker.Date);
                    updateCmd.Parameters.AddWithValue("@varattu_alkupvm", alkuDatePicker.Date);
                    updateCmd.Parameters.AddWithValue("@varattu_loppupvm", loppuDatePicker.Date);
                    updateCmd.Parameters.AddWithValue("@varausId", varausId);

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
                Console.WriteLine($"Virhe varauksen tietojen päivityksessä: {ex.Message}");
                return false;
            }
        }
        

        // varauksen tietojen poisto KESKEN
        private async Task<bool> RemoveVarausData(int varausId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string deleteQuery = "DELETE FROM varaus WHERE varaus_id = @varausId";
                using var deleteCmd = new MySqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@varausId", varausId);
                await deleteCmd.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Virhe varauksen tietojen poistamisessa: {ex.Message}");
                return false;
            }
        }

        public async void TyhjennaVaraus_Clicked(object sender, EventArgs e)
        {
            ClearFields();

        }

        // Varauksen tietojen poisto
        public async void OnVarausDeleteClicked(object sender, EventArgs e)
        {
            if (pickerVaraukset.SelectedItem == null)
            {
                await DisplayAlert("Virhe", "Valitse ensin varaus poistettavaksi.", "OK");
                return;
            }

            // Otetaan valitusta itemistä vain varauksen id
            string selectedItem = (string)pickerVaraukset.SelectedItem;
            int varausId = int.Parse(selectedItem.Split(':')[0]);

            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

                bool isSuccess = await RemoveVarausData(varausId, dbConnector);
                if (isSuccess)
                {
                    await DisplayAlert("Onnistui!", "Varauksen tiedot poistettu!", "OK");
                    await LoadVarausIntoPicker(dbConnector);
                    // Kenttien tyhjennys
                    ClearFields();
                }
                else
                {
                    await DisplayAlert("Virhe", "Varauksen tietojen poisto epäonnistui.", "OK");
                }

        } 

        public async void PaivitaVaraus_Clicked(object sender, EventArgs e)
        {
             // Otetaan valitusta itemistä vain varauksen id
            string selectedItem = (string)pickerVaraukset.SelectedItem;
            int varausId = int.Parse(selectedItem.Split(':')[0]); //Virhe tässä JOS vaihtaa varauksen alkupäivämäärää!!!!

            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

           
            bool isSuccess = await UpdateVarausToDatabase(varausId, dbConnector);
            if (isSuccess)
            {
                await DisplayAlert("Onnistui!", "Varauksen tiedot päivitetty!", "OK");
                await LoadVarausIntoPicker(dbConnector); // Päivitetään pickerin tiedot
            }
            else
            {
                await DisplayAlert("Virhe", "Varauksen tietojen päivitys epäonnistui.", "OK");
            }
        }

         public async void PaivitaLista_Clicked(object sender, EventArgs e)
        {

        }

        // tyhjentää kentät
        private void ClearFields()
        {
            entryMokkiId.Text = string.Empty;
            AsiakasIdEntry.Text = string.Empty;
            varausDatePicker.Date = DateTime.Today;
            vahvistusDatePicker.Date = DateTime.Today;
            alkuDatePicker.Date = DateTime.Today;
            loppuDatePicker.Date = DateTime.Today;
            pickerVaraukset.SelectedIndex = -1;
        }

        
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Asetetaan DatePickerien minimi- ja maksimipäivämäärät
            varausDatePicker.MaximumDate = DateTime.Now;
            varausDatePicker.MinimumDate = DateTime.Now;
            vahvistusDatePicker.MinimumDate = DateTime.Today;
            vahvistusDatePicker.MaximumDate = DateTime.Today.AddDays(7);
            alkuDatePicker.MinimumDate = DateTime.Today;
            alkuDatePicker.MaximumDate = DateTime.Today.AddYears(1);
            loppuDatePicker.MinimumDate = DateTime.Today;
            loppuDatePicker.MaximumDate = DateTime.Today.AddYears(2);

        }

            // Hakee kaikki varaukset tietokannasta ja asettaa ne Pickerin ItemsSourceksi (KESKEN)
        private async Task LoadVarausIntoPicker(DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string query = "SELECT varaus_id, asiakas_id, mokki_id FROM varaus";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                List<string> reservation = new List<string>();
                while (reader.Read())
                {
                    int varaus_Id = reader.GetInt32("varaus_id");
                    int asiakas_Id = reader.GetInt32("asiakas_id");
                    int mokki_Id = reader.GetInt32("mokki_id");
                    // Lisätään varauksen, asiakkaan ja mökin id:t listaan
                    reservation.Add($"{varaus_Id}: Asiakas ID {asiakas_Id} Mökki ID {mokki_Id}");
                }

                pickerVaraukset.ItemsSource = reservation;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", $"Virhe asiakkaiden lataamisessa Pickeriin: {ex.Message}", "OK");
            }
        }

        // EI TOiMi, kesken

         private async void OnVarausSelectedIndexChanged(object sender, EventArgs e)
        {
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            if (pickerVaraukset.SelectedItem != null)
            {
                // Otetaan valitusta itemistä asiakkaan id, etunimi ja sukunimi
                string selectedItem = (string)pickerVaraukset.SelectedItem;
                int varausId = int.Parse(selectedItem.Split(':')[0]);
                await LoadVarausData(varausId, dbConnector);
            }
        } 
        
        // Näytä pickeristä valitun varauksen tiedot entry-kentissä
        private async Task LoadVarausData(int varausId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string query = "SELECT * FROM varaus WHERE varaus_id = @varausId";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@varausId", varausId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (reader.Read())
                {
                    // Asetetaan asiakastiedot käyttöliittymään
                    AsiakasIdEntry.Text = reader["asiakas_id"].ToString();
                    entryMokkiId.Text = reader["mokki_id"].ToString();
                    varausDatePicker.Date = reader.GetDateTime("varattu_pvm");
                    vahvistusDatePicker.Date = reader.GetDateTime("vahvistus_pvm");
                    alkuDatePicker.Date = reader.GetDateTime("varattu_alkupvm");
                    loppuDatePicker.Date = reader.GetDateTime("varattu_loppupvm");

                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", $"Virhe asiakkaiden lataamisessa: {ex.Message}", "OK");
            }
        }
      
    }
   
}
