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
            // Sivun avautuessa päivitetään pickeri
            LoadReceiptsIntoPicker(dbConnector);
            
            // samoin listan päivitys (jos toteutetaan)
        }

        //  Lisää lasku:
        //  nappia painetaan -> tarkistus -> mahdollinen ilmoitus -> tietokantaan lisääminen -> ilmoitus
        private async void LisaaLasku_Clicked(object sender, EventArgs e)
        {
            // Tarkistetaan, että kentät eivät ole tyhjät
            if (!string.IsNullOrWhiteSpace(VarausEntry.Text) &&
                !string.IsNullOrWhiteSpace(SummaEntry.Text) &&
                !string.IsNullOrWhiteSpace(AlvEntry.Text) &&
                MaksunTilaPicker.SelectedIndex != -1)
            {
                // Tarkistetaan, että tiedot ovat numeereisia
                if (int.TryParse(VarausEntry.Text, out int VarausID) &&
                   int.TryParse(SummaEntry.Text, out int Summa) &&
                   double.TryParse(AlvEntry.Text, out double ALV))
                {
                    // Tiedon tallentaminen tietokantaan
                    try
                    {
                        // tarkista, että varausID on olemassa ja laskuID luodaan se, mikä seuraavana
                        await DisplayAlert("Onnistui", "Tiedon tallentaminen tietokantaan onnistui", "OK");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Tiedon tallentaminen tietokantaan epäonnistui: {ex.Message}");
                    }
                }
                else
                {
                    await DisplayAlert("Tiedot vääriä", "Syötä tieto numeroina", "OK");
                }

            }
            else
            {
                await DisplayAlert("Tiedot puutteellisia", "Täytä kaikke tiedot lisätäksesi laskun tiedot tieotokantaan", "OK");
            }
        }

        //  Päivitä lasku:
        //  nappia painetaan -> tarkistus -> mahdollinen ilmoitus -> tietokannan päivittäminen -> ilmoitus
        private async void PaivitaLasku_Clicked(object sender, EventArgs e)
        {
            // Tarkistetaan, että pickeristä on valittu lasku
            if (LaskuPicker.SelectedIndex != -1)
            {
                // Tarkistetaan, että kentät eivät ole tyhjät
                if (!string.IsNullOrWhiteSpace(VarausEntry.Text) &&
                !string.IsNullOrWhiteSpace(SummaEntry.Text) &&
                !string.IsNullOrWhiteSpace(AlvEntry.Text) &&
                MaksunTilaPicker.SelectedIndex != -1)
                {
                    // Tarkistetaan, että tiedot ovat numeerisia
                    if (int.TryParse(VarausEntry.Text, out int VarausID) &&
                       int.TryParse(SummaEntry.Text, out int Summa) &&
                       int.TryParse(AlvEntry.Text, out int ALV))
                    {
                        // Tiedon päivittäminen tietokantaan
                        try
                        {
                            // tarkista, että varausID ja laskuID ovat olemassa, ei voi luoda uutta
                            await DisplayAlert("Onnistui", "Tiedon tallentaminen tietokantaan onnistui", "OK");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Tiedon päivittäminen tietokantaan epäonnistui: {ex.Message}");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Tiedot vääriä", "Syötä tieto numeroina", "OK");
                    }

                }
                else
                {
                    await DisplayAlert("Tiedot puutteellisia", "Täytä kaikke tiedot lisätäksesi laskun tiedot tieotokantaan", "OK");
                }
            }
            else
            {
                await DisplayAlert("Virhe", "Valitse lasku, jota haluat muokata", "OK");
            }
        }

        // Tyhjennä kentät:
        //  nappia painetaan -> tyhjennetään kentät
        private void TyhjennaKentat_Clicked(object sender, EventArgs e)
        {
                LaskuPicker.SelectedIndex = -1;
                VarausEntry.Text = string.Empty;
                SummaEntry.Text = string.Empty;
                AlvEntry.Text = string.Empty;
                MaksunTilaPicker.SelectedIndex = -1;
        }
        
        // Päivitä lista:
        //  nappia painetaan -> tarkistus -> mahdollinen ilmoitus -> mahdollinen listan päivitys ->  ilmoitus
        private async void PaivitaLista_Clicked(object sender, EventArgs e)
        {

        }

        //  Poista lasku:
        //  nappia painetaan -> poistetaan rivi tietokannasta -> ilmoitus
        private async void PoistaLasku_Clicked(object sender, EventArgs e)
        {
            // Tarkistetaan, että pickeristä on valittu lasku
            if (LaskuPicker.SelectedIndex != -1)
            {
                // Tiedon päivittäminen tietokantaan
                try
                {

                    await DisplayAlert("Onnistui", "Tiedon poistaminen tietokannasta onnistui", "OK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Tiedon poistaminen tietokannasta epäonnistui: {ex.Message}");
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
                    // Lisätään lasku id ja varausid ja summa listaan
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
                    // Asetetaan laskutiedot käyttöliittymään
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

        // Pickerissä valittaessa kuitti, haetaan sen tiedot
        private async void LaskuPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            if (LaskuPicker.SelectedItem != null)
            {
                // Otetaan valitusta itemistä asiakkaan id, etunimi ja sukunimi
                string selectedItem = (string)LaskuPicker.SelectedItem;
                int laskuId = int.Parse(selectedItem.Split(':')[0]);
                await LoadReceiptData(laskuId, dbConnector);
            }
        }
    }
}
