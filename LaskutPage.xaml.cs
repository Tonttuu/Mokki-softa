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
            string[] forbiddenChars = { "'", ";", "--", "/*", "*/" }; // Esimerkkej� kielletyist� merkeist�
            foreach (var forbiddenChar in forbiddenChars)
            {
                if (input.Contains(forbiddenChar))
                {
                    return true; // Palauta true, jos kielletty merkki l�ytyy
                }
            }
            return false; // Palauta false, jos kielletty� merkki� ei l�ytynyt
        }

        //  Lis�� lasku -nappi:
        private async void LisaaLasku_Clicked(object sender, EventArgs e)
        {
            // Tarkistetaan, ett� kent�t on t�ytetty
            if (!string.IsNullOrWhiteSpace(VarausEntry.Text) &&
                !string.IsNullOrWhiteSpace(SummaEntry.Text) &&
                !string.IsNullOrWhiteSpace(AlvEntry.Text) &&
                MaksunTilaPicker.SelectedIndex != -1)
            {
                // Tarkistetaan, ett� tiedot ovat numeereisia
                if (int.TryParse(VarausEntry.Text, out int varausID) &&
                   double.TryParse(SummaEntry.Text, out double summa) &&
                   double.TryParse(AlvEntry.Text, out double ALV))
                {
                    // Tarkistetaan, ett� ei ole k�ytetty SQL injektioita
                    if (!ContainsSQLInjection(VarausEntry.Text) ||
                        !ContainsSQLInjection(SummaEntry.Text) ||
                        !ContainsSQLInjection(AlvEntry.Text))
                    {
                        // Tiedon tallentaminen / p�ivitt�minen tietokantaan
                        var appSettings = ConfigurationProvider.GetAppSettings();
                        var dbConnector = new DatabaseConnector(appSettings);

                        if (LaskuPicker.SelectedItem != null)
                        {
                            // Otetaan valitusta itemist� vain laskun id
                            string selectedItem = (string)LaskuPicker.SelectedItem;
                            int asiakasId = int.Parse(selectedItem.Split(':')[0]);

                            bool isSuccess = await SaveOrUpdateDataToDatabase(asiakasId, dbConnector);
                            if (isSuccess)
                            {
                                await DisplayAlert("Onnistui!", "Tiedot p�ivitetty", "OK");
                                await LoadReceiptsIntoPicker(dbConnector); // P�ivitet��n pickerin data.
                            }
                            else
                            {
                                await DisplayAlert("Virhe", "Tietojen p�ivitys ep�onnistui", "OK");
                            }
                        }
                        else
                        {
                            // Uusi asiakas, lis�� uudet tiedot tietokantaan
                            bool isSuccess = await SaveNewDataToDatabase(dbConnector);
                            if (isSuccess)
                            {
                                await DisplayAlert("Onnistui!", "Uusi lasku lis�tty", "OK");
                                await LoadReceiptsIntoPicker(dbConnector); // P�ivitet��n pickerin data
                            }
                            else
                            {
                                await DisplayAlert("Virhe", "Uuden laskun lis�ys ep�onnistui!", "OK");
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Virhe", "�l� k�yt� sy�tteess� kiellettyj� merkkej�", "OK");
                    }

                    // Tiedon tallentaminen tietokantaan
                    try
                    {
                        // tarkista, ett� varausID on olemassa (ei v�ltt�m�tt� pakollinen?)
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
                await DisplayAlert("Tiedot puutteellisia", "T�yt� kaikki tiedot lis�t�ksesi laskun tiedot tieotokantaan", "OK");
            }
        }

        // Lis��/p�ivit� lasku funktio
        // Uuden laskun lis�ys toimii, olemassaolevan p�ivitys ei, vaikka sen pit�isi toimia
        private async Task<bool> SaveOrUpdateDataToDatabase(int laskuId, DatabaseConnector dbConnector)
        {
            try
            {
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();
                // Tarkistaan, onko pickerist� valittu jo olemassa oleva lasku.
                string checkQuery = "SELECT COUNT(*) FROM lasku WHERE lasku_id = @asiakasId";
                using var checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@laskuId", laskuId);
                long count = (long)await checkCmd.ExecuteScalarAsync();

                if (count > 0)
                {
                    // P�ivitet��n olemassa olevan laskun tiedot
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
                    // Lis�t��n uusi asiakas tietokantaan
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
                Console.WriteLine($"Virhe tietojen tallennuksessa tai p�ivityksess�: {ex.Message}");
                return false;
            }
        }

        // Uuden laskun lis�ys funktio
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


        //  P�ivit� lasku -nappi:
        // Ei toimi, vaikka koodi samanlaista kuin muilla sivuilla ja pit�isi toimia
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
                    if (int.TryParse(VarausEntry.Text, out int varausID) &&
                       double.TryParse(SummaEntry.Text, out double summa) &&
                       double.TryParse(AlvEntry.Text, out double ALV))
                    {
                        // Tarkistetaan, ett� ei ole k�ytetty SQL injektioita
                        if (!ContainsSQLInjection(VarausEntry.Text) ||
                            !ContainsSQLInjection(SummaEntry.Text) ||
                            !ContainsSQLInjection(AlvEntry.Text))
                        {
                            // Tiedon p�ivitt�minen tietokantaan
                            string selectedItem = (string)LaskuPicker.SelectedItem;
                            int laskuId = int.Parse(selectedItem.Split(':')[0]);

                            var appSettings = ConfigurationProvider.GetAppSettings();
                            var dbConnector = new DatabaseConnector(appSettings);

                            bool isSuccess = await UpdateLaskuToDatabase(laskuId, dbConnector);
                            if (isSuccess)
                            {
                                await DisplayAlert("Onnistui!", "Laskun tiedot p�ivitetty!", "OK");
                                await LoadReceiptsIntoPicker(dbConnector);
                            }
                            else
                            {
                                await DisplayAlert("Virhe", "Laskun tietojen p�ivitys ep�onnistui.", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Virhe", "�l� k�yt� sy�tteess� kiellettyj� merkkej�", "OK");
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

        // Laskun p�ivitys funktio (pit�isi toimia, koodi t�ysin vastaava kuin muilla sivuilla ja ne toimivat)
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
                    // P�ivitet��n laskun tiedot
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
                Console.WriteLine($"Virhe laskun tietojen p�ivityksess�: {ex.Message}");
                return false;
            }
        }

        // Tyhjenn� kent�t -nappi:
        private void TyhjennaKentat_Clicked(object sender, EventArgs e)
        {
            LaskuPicker.SelectedIndex = -1;
            VarausEntry.Text = string.Empty;
            SummaEntry.Text = string.Empty;
            AlvEntry.Text = string.Empty;
            MaksunTilaPicker.SelectedIndex = -1;
        }

        // P�ivit� lista -nappi:
        // T�t� en ehtinyt toteuttaa
        private async void PaivitaLista_Clicked(object sender, EventArgs e)
        {

        }

        //  Poista lasku -nappi:
        private async void PoistaLasku_Clicked(object sender, EventArgs e)
        {
            // Tarkistetaan, ett� pickerist� on valittu lasku
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
                    await DisplayAlert("Virhe", "Laskun tietojen poisto ep�onnistui.", "OK");
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
                    // Lis�t��n lasku id ja varausid ja summa listaan
                    receiptNames.Add($"{laskuId}: VarausID = {varausId} Summa = {summa} ");
                }

                LaskuPicker.ItemsSource = receiptNames;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", $"Virhe kuittien lataamisessa Pickeriin: {ex.Message}", "OK");
            }
        }

        // Valitun kuitin tietojen lataus k�ytt�liittym��n funktio
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

        // Haetaan kuitin tiedot, kun se valitaan pickerist�
        private async void LaskuPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var appSettings = ConfigurationProvider.GetAppSettings();
            var dbConnector = new DatabaseConnector(appSettings);

            if (LaskuPicker.SelectedItem != null)
            {
                // Otetaan valitusta itemist� laskun id
                string selectedItem = (string)LaskuPicker.SelectedItem;
                int laskuId = int.Parse(selectedItem.Split(':')[0]);
                await LoadReceiptData(laskuId, dbConnector);
            }
        }
    }
}
