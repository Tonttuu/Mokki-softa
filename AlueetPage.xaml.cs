using MySqlConnector;

namespace Mokki_softa
{
    public partial class AlueetPage : ContentPage
    {
        public AlueetPage()
        {
            InitializeComponent();
        }
        private async void OnSaveAlueClicked(object sender, EventArgs e)
        {
            var nimi = entryAlueNimi.Text;

            if (string.IsNullOrEmpty(nimi))
            {
                await DisplayAlert("Virhe", "Syötä alueen nimi.", "OK");
                return;
            }

            try
            {
                var appSettings = ConfigurationProvider.GetAppSettings();
                var dbConnector = new DatabaseConnector(appSettings);
                using var conn = dbConnector.GetConnection();
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("INSERT INTO alue (nimi) VALUES (@nimi)", conn);
                cmd.Parameters.AddWithValue("@nimi", nimi);

                var result = await cmd.ExecuteNonQueryAsync();
                if (result > 0)
                    await DisplayAlert("Onnistui!", "Alue tallennettu onnistuneesti.", "OK");
                else
                    await DisplayAlert("Virhe!", "Alueen tallennus epäonnistui.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Tietokantavirhe", $"Virhe: {ex.Message}", "OK");
            }
        }
    }
}
