using Microsoft.Maui.Graphics.Text;
using MySqlConnector;
using static System.Net.Mime.MediaTypeNames;

namespace Mokki_softa
{
    // Ti: 4h
    // Ke: 1.75h
    // To: 
    public partial class LaskutPage : ContentPage
    {
        public LaskutPage()
        {
            InitializeComponent();
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);
            // Sivun avautuessa p�ivitet��n pickeri
            LoadReceiptsIntoPicker(dbConnector);
            
            // samoin listan p�ivitys (jos toteutetaan)
        }

        //  Lis�� lasku:
        //  nappia painetaan -> tarkistus -> mahdollinen ilmoitus -> tietokantaan lis��minen -> ilmoitus
        private async void LisaaLasku_Clicked(object sender, EventArgs e)
        {
            // Tarkistetaan, ett� kent�t eiv�t ole tyhj�t
            if (!string.IsNullOrWhiteSpace(VarausEntry.Text) &&
                !string.IsNullOrWhiteSpace(SummaEntry.Text) &&
                !string.IsNullOrWhiteSpace(AlvEntry.Text) &&
                MaksunTilaPicker.SelectedIndex != -1)
            {
                // Tarkistetaan, ett� tiedot ovat numeereisia
                if (int.TryParse(VarausEntry.Text, out int VarausID) &&
                   int.TryParse(SummaEntry.Text, out int Summa) &&
                   double.TryParse(AlvEntry.Text, out double ALV))
                {
                    // Tiedon tallentaminen tietokantaan
                    try
                    {
                        // tarkista, ett� varausID on olemassa ja laskuID luodaan se, mik� seuraavana
                        await DisplayAlert("Onnistui", "Tiedon tallentaminen tietokantaan onnistui", "OK");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Tiedon tallentaminen tietokantaan ep�onnistui: {ex.Message}");
                    }
                }
                else
                {
                    await DisplayAlert("Tiedot v��ri�", "Sy�t� tieto numeroina", "OK");
                }

            }
            else
            {
                await DisplayAlert("Tiedot puutteellisia", "T�yt� kaikke tiedot lis�t�ksesi laskun tiedot tieotokantaan", "OK");
            }
        }

        //  P�ivit� lasku:
        //  nappia painetaan -> tarkistus -> mahdollinen ilmoitus -> tietokannan p�ivitt�minen -> ilmoitus
        private async void PaivitaLasku_Clicked(object sender, EventArgs e)
        {
            // Tarkistetaan, ett� pickerist� on valittu lasku
            if (LaskuPicker.SelectedIndex != -1)
            {
                // Tarkistetaan, ett� kent�t eiv�t ole tyhj�t
                if (!string.IsNullOrWhiteSpace(VarausEntry.Text) &&
                !string.IsNullOrWhiteSpace(SummaEntry.Text) &&
                !string.IsNullOrWhiteSpace(AlvEntry.Text) &&
                MaksunTilaPicker.SelectedIndex != -1)
                {
                    // Tarkistetaan, ett� tiedot ovat numeerisia
                    if (int.TryParse(VarausEntry.Text, out int VarausID) &&
                       int.TryParse(SummaEntry.Text, out int Summa) &&
                       int.TryParse(AlvEntry.Text, out int ALV))
                    {
                        // Tiedon p�ivitt�minen tietokantaan
                        try
                        {
                            // tarkista, ett� varausID ja laskuID ovat olemassa, ei voi luoda uutta
                            await DisplayAlert("Onnistui", "Tiedon tallentaminen tietokantaan onnistui", "OK");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Tiedon p�ivitt�minen tietokantaan ep�onnistui: {ex.Message}");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Tiedot v��ri�", "Sy�t� tieto numeroina", "OK");
                    }

                }
                else
                {
                    await DisplayAlert("Tiedot puutteellisia", "T�yt� kaikke tiedot lis�t�ksesi laskun tiedot tieotokantaan", "OK");
                }
            }
            else
            {
                await DisplayAlert("Virhe", "Valitse lasku, jota haluat muokata", "OK");
            }
        }

        // Tyhjenn� kent�t:
        //  nappia painetaan -> tyhjennet��n kent�t
        private void TyhjennaKentat_Clicked(object sender, EventArgs e)
        {
                LaskuPicker.SelectedIndex = -1;
                VarausEntry.Text = string.Empty;
                SummaEntry.Text = string.Empty;
                AlvEntry.Text = string.Empty;
                MaksunTilaPicker.SelectedIndex = -1;
        }
        
        // P�ivit� lista:
        //  nappia painetaan -> tarkistus -> mahdollinen ilmoitus -> mahdollinen listan p�ivitys ->  ilmoitus
        private async void PaivitaLista_Clicked(object sender, EventArgs e)
        {

        }

        //  Poista lasku:
        //  nappia painetaan -> poistetaan rivi tietokannasta -> ilmoitus
        private async void PoistaLasku_Clicked(object sender, EventArgs e)
        {
            // Tarkistetaan, ett� pickerist� on valittu lasku
            if (LaskuPicker.SelectedIndex != -1)
            {
                // Tiedon p�ivitt�minen tietokantaan
                try
                {

                    await DisplayAlert("Onnistui", "Tiedon poistaminen tietokannasta onnistui", "OK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Tiedon poistaminen tietokannasta ep�onnistui: {ex.Message}");
                }
            }
            else
            {
                await DisplayAlert("Virhe", "Valitse lasku, jonka haluat poistaa", "OK");
            }
        }

        // Ladataan tiedot pickeriin
        private async Task LoadReceiptsIntoPicker(DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string query = "SELECT lasku_id, varaus_id, summa FROM lasku";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                List<string> receiptNames = new List<string>();
                while (reader.Read())
                {
                    int laskuId = reader.GetInt32("lasku_id");
                    int varausId = reader.GetInt32("varaus_id");
                    int summa = reader.GetInt32("summa");
                    // Lis�t��n lasku id ja varausid ja summa listaan
                    receiptNames.Add($"{laskuId}: {varausId} {summa}");
                }

                LaskuPicker.ItemsSource = receiptNames;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", $"Virhe kuittien lataamisessa Pickeriin: {ex.Message}", "OK");
            }
        }

        // Ladataan valitun kuitin tiedot entry kenttiin ja pickeriin
        private async Task LoadReceiptData(int laskuId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string query = "SELECT * FROM lasku WHERE lasku_id = @laskuId";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@laskuId", laskuId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (reader.Read())
                {
                    // Asetetaan laskutiedot k�ytt�liittym��n
                    VarausEntry.Text = reader.GetString("varaus_id");
                    SummaEntry.Text = reader.GetString("summa");
                    AlvEntry.Text = reader.GetString("alv");
                    MaksunTilaPicker.SelectedIndex = int.Parse(reader.GetString("maksettu"));

                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", $"Virhe kuittien lataamisessa: {ex.Message}", "OK");
            }
        }

        // Pickeriss� valittaessa kuitti, haetaan sen tiedot
        private async void LaskuPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            if (LaskuPicker.SelectedItem != null)
            {
                // Otetaan valitusta itemist� asiakkaan id, etunimi ja sukunimi
                string selectedItem = (string)LaskuPicker.SelectedItem;
                int laskuId = int.Parse(selectedItem.Split(':')[0]);
                await LoadReceiptData(laskuId, dbConnector);
            }
        }
    }
}
