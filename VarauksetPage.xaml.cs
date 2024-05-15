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
                await DisplayAlert("Virhe", "Varaus ID:n, AsiakasID:n sekä Mökki ID:n täytyy olla numeerisia.", "OK");
                return;
            }

                    // Tarkistetaan, että DatePicker-ohjaimet on valittu
            if (varausDatePicker.Date == null || vahvistusDatePicker.Date == null || alkuDatePicker.Date == null || loppuDatePicker.Date == null)
            {
                await DisplayAlert("Virhe", "Valitse päivämäärät kaikkiin kenttiin.", "OK");
                return;
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
            }
            else
            {
                await DisplayAlert("Virhe", "Tietojen lisäys epäonnistui", "OK");
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

        // Varauksen tietojen poisto KESKEN TEE TÄMÄ EKA!!!!
        public async void OnVarausDeleteClicked(object sender, EventArgs e)
        {
              /*      if (pickerVaraukset.SelectedItem == null)
            {
                await DisplayAlert("Virhe", "Valitse ensin varaus poistettavaksi.", "OK");
                return;
            }

            // Otetaan valitusta itemistä vain varauksen id (KESKEN)
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
                } */

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
            // Pickerissä -1, katso S harjoitustyö
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
                    reservation.Add($"Varaus {varaus_Id}: Asiakas ID: {asiakas_Id} Mökki ID: {mokki_Id}");
                }

                pickerVaraukset.ItemsSource = reservation;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", $"Virhe asiakkaiden lataamisessa Pickeriin: {ex.Message}", "OK");
            }
        } 
      
    }
   
}
