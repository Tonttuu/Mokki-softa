using Microsoft.Maui.Graphics.Text;
using MySqlConnector;
using System.Data.Common;
using static System.Net.Mime.MediaTypeNames;

namespace Mokki_softa
{
    // Ti: 4h
    // Ke: 4h
    // To: 5h
    public partial class LaskutPage : ContentPage
    {
        public LaskutPage()
        {
            InitializeComponent();
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);
            // Sivun avautuessa kutstuaan funktiota
            LoadReceiptsIntoPicker(dbConnector);
        }

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

        //  Lisää lasku -nappi:
        private async void LisaaLasku_Clicked(object sender, EventArgs e)
        {
            // Tarkistetaan, että kentät on täytetty
            if (!string.IsNullOrWhiteSpace(VarausEntry.Text) &&
                !string.IsNullOrWhiteSpace(SummaEntry.Text) &&
                !string.IsNullOrWhiteSpace(AlvEntry.Text) &&
                MaksunTilaPicker.SelectedIndex != -1)
            {
                // Tarkistetaan, että tiedot ovat numeereisia
                if (int.TryParse(VarausEntry.Text, out int varausID) &&
                   double.TryParse(SummaEntry.Text, out double summa) &&
                   double.TryParse(AlvEntry.Text, out double ALV))
                {
                    // Tarkistetaan, että ei ole käytetty SQL injektioita
                    if (!ContainsSQLInjection(VarausEntry.Text) ||
                        !ContainsSQLInjection(SummaEntry.Text) ||
                        !ContainsSQLInjection(AlvEntry.Text))
                    {
                        // Tiedon tallentaminen / päivittäminen tietokantaan
                        var appSettings = ConfigurationProvider.GetAppSettings();
                        var dbConnector = new DatabaseConnector(appSettings);

                        if (LaskuPicker.SelectedItem != null)
                        {
                            // Otetaan valitusta itemistä vain laskun id
                            string selectedItem = (string)LaskuPicker.SelectedItem;
                            int asiakasId = int.Parse(selectedItem.Split(':')[0]);

                            bool isSuccess = await SaveOrUpdateDataToDatabase(asiakasId, dbConnector);
                            if (isSuccess)
                            {
                                await DisplayAlert("Onnistui!", "Tiedot päivitetty", "OK");
                                await LoadReceiptsIntoPicker(dbConnector); // Päivitetään pickerin data.
                            }
                            else
                            {
                                await DisplayAlert("Virhe", "Tietojen päivitys epäonnistui", "OK");
                            }
                        }
                        else
                        {
                            // Uusi asiakas, lisää uudet tiedot tietokantaan
                            bool isSuccess = await SaveNewDataToDatabase(dbConnector);
                            if (isSuccess)
                            {
                                await DisplayAlert("Onnistui!", "Uusi lasku lisätty", "OK");
                                await LoadReceiptsIntoPicker(dbConnector); // Päivitetään pickerin data
                            }
                            else
                            {
                                await DisplayAlert("Virhe", "Uuden laskun lisäys epäonnistui!", "OK");
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Virhe", "Älä käytä syötteessä kiellettyjä merkkejä", "OK");
                    }

                    // Tiedon tallentaminen tietokantaan
                    try
                    {
                        // tarkista, että varausID on olemassa (ei välttämättä pakollinen?)
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
                await DisplayAlert("Tiedot puutteellisia", "Täytä kaikki tiedot lisätäksesi laskun tiedot tieotokantaan", "OK");
            }
        }

        // Lisää/päivitä lasku funktio
        // Uuden laskun lisäys toimii, olemassaolevan päivitys ei, vaikka sen pitäisi toimia
        private async Task<bool> SaveOrUpdateDataToDatabase(int laskuId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();
                // Tarkistaan, onko pickeristä valittu jo olemassa oleva lasku.
                string checkQuery = "SELECT COUNT(*) FROM lasku WHERE lasku_id = @asiakasId";
                using var checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@laskuId", laskuId);
                long count = (long)await checkCmd.ExecuteScalarAsync();

                if (count > 0)
                {
                    // Päivitetään olemassa olevan laskun tiedot
                    string updateQuery = "UPDATE lasku SET varaus_id = @varaus_id, summa = @summa, alv = @alv, maksettu = @maksettu";
                    updateQuery += " WHERE lasku_id = @lasku_id";

                    int varausId = int.Parse(VarausEntry.Text);
                    double summa = double.Parse(SummaEntry.Text);
                    double alv = double.Parse(AlvEntry.Text);
                    int maksettu = MaksunTilaPicker.SelectedIndex;

                    using var updateCmd = new MySqlCommand(updateQuery, conn);
                    updateCmd.Parameters.AddWithValue("@varaus_id", varausId);
                    updateCmd.Parameters.AddWithValue("@summa", summa);
                    updateCmd.Parameters.AddWithValue("@alv", alv);
                    updateCmd.Parameters.AddWithValue("@maksettu", maksettu);

                    int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                    return rowsAffected > 0;
                }
                else
                {
                    // Lisätään uusi asiakas tietokantaan
                    string insertQuery = "INSERT INTO lasku (varaus_id, summa, alv, maksettu) " +
                                        "VALUES (@varaus_id, @summa, @alv, @maksettu)";
                    using var insertCmd = new MySqlCommand(insertQuery, conn);

                    int varausId = int.Parse(VarausEntry.Text);
                    double summa = double.Parse(SummaEntry.Text);
                    double alv = double.Parse(AlvEntry.Text);
                    int maksettu = MaksunTilaPicker.SelectedIndex;

                    insertCmd.Parameters.AddWithValue("@varaus_id", varausId);
                    insertCmd.Parameters.AddWithValue("@summa", summa);
                    insertCmd.Parameters.AddWithValue("@alv", alv);
                    insertCmd.Parameters.AddWithValue("@maksettu", maksettu);

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

        // Uuden laskun lisäys funktio
        private async Task<bool> SaveNewDataToDatabase(DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string insertQuery = "INSERT INTO lasku (varaus_id, summa, alv, maksettu) " +
                                    "VALUES (@varaus_id, @summa, @alv, @maksettu)";

                int varausId = int.Parse(VarausEntry.Text);
                double summa = double.Parse(SummaEntry.Text);
                double alv = double.Parse(AlvEntry.Text);
                int maksettu = MaksunTilaPicker.SelectedIndex;

                using var insertCmd = new MySqlCommand(insertQuery, conn);
                insertCmd.Parameters.AddWithValue("@varaus_id", varausId);
                insertCmd.Parameters.AddWithValue("@summa", summa);
                insertCmd.Parameters.AddWithValue("@alv", alv);
                insertCmd.Parameters.AddWithValue("@maksettu", maksettu);

                int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Virhe uuden laskun tallennuksessa: {ex.Message}");
                return false;
            }
        }


        //  Päivitä lasku -nappi:
        // Ei toimi, vaikka koodi samanlaista kuin muilla sivuilla ja pitäisi toimia
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
                    if (int.TryParse(VarausEntry.Text, out int varausID) &&
                       double.TryParse(SummaEntry.Text, out double summa) &&
                       double.TryParse(AlvEntry.Text, out double ALV))
                    {
                        // Tarkistetaan, että ei ole käytetty SQL injektioita
                        if (!ContainsSQLInjection(VarausEntry.Text) ||
                            !ContainsSQLInjection(SummaEntry.Text) ||
                            !ContainsSQLInjection(AlvEntry.Text))
                        {
                            // Tiedon päivittäminen tietokantaan
                            string selectedItem = (string)LaskuPicker.SelectedItem;
                            int laskuId = int.Parse(selectedItem.Split(':')[0]);

                            var appSettings = ConfigurationProvider.GetAppSettings();
                            var dbConnector = new DatabaseConnector(appSettings);

                            bool isSuccess = await UpdateLaskuToDatabase(laskuId, dbConnector);
                            if (isSuccess)
                            {
                                await DisplayAlert("Onnistui!", "Laskun tiedot päivitetty!", "OK");
                                await LoadReceiptsIntoPicker(dbConnector);
                            }
                            else
                            {
                                await DisplayAlert("Virhe", "Laskun tietojen päivitys epäonnistui.", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Virhe", "Älä käytä syötteessä kiellettyjä merkkejä", "OK");
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

        // Laskun päivitys funktio (pitäisi toimia, koodi täysin vastaava kuin muilla sivuilla ja ne toimivat)
        private async Task<bool> UpdateLaskuToDatabase(int laskuId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string checkQuery = "SELECT COUNT(*) FROM lasku WHERE lasku_id = @laskuId";
                using var checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@laskuId", laskuId);
                long count = (long)await checkCmd.ExecuteScalarAsync();

                if (count > 0)
                {
                    // Päivitetään laskun tiedot
                    string updateQuery = "UPDATE lasku SET lasku_id = @lasku_id, varaus_id = @varaus_id, summa = @summa, alv = @alv, maksettu = @maksettu";
                    updateQuery += " WHERE lasku_id = @laskuId";

                    using var updateCmd = new MySqlCommand(updateQuery, conn);

                    int varausId = int.Parse(VarausEntry.Text);
                    double summa = double.Parse(SummaEntry.Text);
                    double alv = double.Parse(AlvEntry.Text);
                    int maksettu = MaksunTilaPicker.SelectedIndex;

                    updateCmd.Parameters.AddWithValue("@lasku_id", laskuId);
                    updateCmd.Parameters.AddWithValue("@varaus_Id", varausId);
                    updateCmd.Parameters.AddWithValue("@summa", summa);
                    updateCmd.Parameters.AddWithValue("@alv", alv);
                    updateCmd.Parameters.AddWithValue("@maksettu", maksettu);

                    int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                    //return true;
                    return rowsAffected > 0;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Virhe laskun tietojen päivityksessä: {ex.Message}");
                return false;
            }
        }

        // Tyhjennä kentät -nappi:
        private void TyhjennaKentat_Clicked(object sender, EventArgs e)
        {
            LaskuPicker.SelectedIndex = -1;
            VarausEntry.Text = string.Empty;
            SummaEntry.Text = string.Empty;
            AlvEntry.Text = string.Empty;
            MaksunTilaPicker.SelectedIndex = -1;
        }

        // Päivitä lista -nappi:
        // Tätä en ehtinyt toteuttaa
        private async void PaivitaLista_Clicked(object sender, EventArgs e)
        {

        }

        //  Poista lasku -nappi:
        private async void PoistaLasku_Clicked(object sender, EventArgs e)
        {
            // Tarkistetaan, että pickeristä on valittu lasku
            if (LaskuPicker.SelectedIndex != -1)
            {
                // Tiedon poistaminen tietokannasta
                string selectedItem = (string)LaskuPicker.SelectedItem;
                int laskuId = int.Parse(selectedItem.Split(':')[0]);

                var appSettings = ConfigurationProvider.GetAppSettings();
                var dbConnector = new DatabaseConnector(appSettings);

                bool isSuccess = await RemoveReceiptData(laskuId, dbConnector);
                if (isSuccess)
                {
                    await DisplayAlert("Onnistui!", "Laskun tiedot poistettu!", "OK");
                    await LoadReceiptsIntoPicker(dbConnector);
                }
                else
                {
                    await DisplayAlert("Virhe", "Laskun tietojen poisto epäonnistui.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Virhe", "Valitse lasku, jonka haluat poistaa", "OK");
            }
        }

        // Kuitin tietojen poisto funktio
        private async Task<bool> RemoveReceiptData(int laskuId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                string deleteQuery = "DELETE FROM lasku WHERE lasku_id = @laskuId";
                using var deleteCmd = new MySqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@laskuId", laskuId);
                await deleteCmd.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Virhe laskun poistamisessa: {ex.Message}");
                return false;
            }
        }

        // Pickeriin tietojen lataus funktio
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
                    double summa = reader.GetDouble("summa");
                    // Lisätään lasku id ja varausid ja summa listaan
                    receiptNames.Add($"{laskuId}: VarausID = {varausId} Summa = {summa} ");
                }

                LaskuPicker.ItemsSource = receiptNames;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", $"Virhe kuittien lataamisessa Pickeriin: {ex.Message}", "OK");
            }
        }

        // Valitun kuitin tietojen lataus käyttöliittymään funktio
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
                    int varausID = reader.GetInt32("varaus_id");
                    VarausEntry.Text = varausID.ToString();

                    double summa = reader.GetDouble("summa");
                    SummaEntry.Text = summa.ToString();

                    double alv = reader.GetDouble("alv");
                    AlvEntry.Text = alv.ToString();

                    int maksunTila = reader.GetInt32("maksettu");
                    MaksunTilaPicker.SelectedIndex = maksunTila;

                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", $"Virhe kuittien lataamisessa: {ex.Message}", "OK");
            }
        }

        // Haetaan kuitin tiedot, kun se valitaan pickeristä
        private async void LaskuPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            if (LaskuPicker.SelectedItem != null)
            {
                // Otetaan valitusta itemistä laskun id
                string selectedItem = (string)LaskuPicker.SelectedItem;
                int laskuId = int.Parse(selectedItem.Split(':')[0]);
                await LoadReceiptData(laskuId, dbConnector);
            }
        }
    }
}
