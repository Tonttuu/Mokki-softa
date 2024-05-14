using Microsoft.Maui.Graphics.Text;
using MySqlConnector;
using static System.Net.Mime.MediaTypeNames;

namespace Mokki_softa
{
    // Ti: 4h
    // Ke: 
    // To: 
    public partial class LaskutPage : ContentPage
    {
        public LaskutPage()
        {
            InitializeComponent();
            // pickerLasku p‰ivitys aina sivun avatessa
        }

        //  Lis‰‰ lasku:
        //  nappia painetaan -> tarkistus -> mahdollinen ilmoitus -> tietokantaan lis‰‰minen -> ilmoitus
        private async void LisaaLasku_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LaskuEntry.Text) &&
                !string.IsNullOrWhiteSpace(VarausEntry.Text) &&
                !string.IsNullOrWhiteSpace(SummaEntry.Text) &&
                !string.IsNullOrWhiteSpace(AlvEntry.Text) &&
                MaksunTilaPicker.SelectedIndex != -1)
            {
                if (int.TryParse(LaskuEntry.Text, out int LaskuID) &&
                   int.TryParse(VarausEntry.Text, out int VarausID) &&
                   int.TryParse(SummaEntry.Text, out int Summa) &&
                   int.TryParse(AlvEntry.Text, out int ALV))
                {
                    // Tiedon tallentaminen tietokantaan
                    try
                    {

                        await DisplayAlert("Onnistui", "Tiedon tallentaminen tietokantaan onnistui", "OK");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Tiedon tallentaminen tietokantaan ep‰onnistui: {ex.Message}");
                    }
                }
                else
                {
                    await DisplayAlert("Tiedot v‰‰ri‰", "Syˆt‰ tieto numeroina", "OK");
                }

            }
            else
            {
                await DisplayAlert("Tiedot puutteellisia", "T‰yt‰ kaikke tiedot lis‰t‰ksesi laskun tiedot tieotokantaan", "OK");
            }
        }

        //  P‰ivit‰ lasku:
        //  nappia painetaan -> tarkistus -> mahdollinen ilmoitus -> tietokannan p‰ivitt‰minen -> ilmoitus
        private async void PaivitaLasku_Clicked(object sender, EventArgs e)
        {
            if (pickerLasku.SelectedIndex != -1)
            {
                if (!string.IsNullOrWhiteSpace(LaskuEntry.Text) &&
                !string.IsNullOrWhiteSpace(VarausEntry.Text) &&
                !string.IsNullOrWhiteSpace(SummaEntry.Text) &&
                !string.IsNullOrWhiteSpace(AlvEntry.Text) &&
                MaksunTilaPicker.SelectedIndex != -1)
                {
                    if (int.TryParse(LaskuEntry.Text, out int LaskuID) &&
                       int.TryParse(VarausEntry.Text, out int VarausID) &&
                       int.TryParse(SummaEntry.Text, out int Summa) &&
                       int.TryParse(AlvEntry.Text, out int ALV))
                    {
                        // Tiedon p‰ivitt‰minen tietokantaan
                        try
                        {

                            await DisplayAlert("Onnistui", "Tiedon tallentaminen tietokantaan onnistui", "OK");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Tiedon p‰ivitt‰minen tietokantaan ep‰onnistui: {ex.Message}");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Tiedot v‰‰ri‰", "Syˆt‰ tieto numeroina", "OK");
                    }

                }
                else
                {
                    await DisplayAlert("Tiedot puutteellisia", "T‰yt‰ kaikke tiedot lis‰t‰ksesi laskun tiedot tieotokantaan", "OK");
                }
            }
            else
            {
                await DisplayAlert("Virhe", "Valitse lasku, jota haluat muokata", "OK");
            }
        }

        // Tyhjenn‰ kent‰t:
        //  nappia painetaan -> tyhjennet‰‰n kent‰t
        private void TyhjennaKentat_Clicked(object sender, EventArgs e)
        {
                pickerLasku.SelectedIndex = -1;
                LaskuEntry.Text = string.Empty;
                VarausEntry.Text = string.Empty;
                SummaEntry.Text = string.Empty;
                AlvEntry.Text = string.Empty;
                MaksunTilaPicker.SelectedIndex = -1;
        }
        
        // P‰ivit‰ lista:
        //  nappia painetaan -> tarkistus -> mahdollinen ilmoitus -> mahdollinen listan p‰ivitys ->  ilmoitus
        private async void PaivitaLista_Clicked(object sender, EventArgs e)
        {

        }

        //  Poista lasku:
        //  nappia painetaan -> poistetaan rivi tietokannasta -> ilmoitus
        private async void PoistaLasku_Clicked(object sender, EventArgs e)
        {
            if (pickerLasku.SelectedIndex != -1)
            {
                int.TryParse(LaskuEntry.Text, out int LaskuID);
                if (pickerLasku.SelectedIndex != LaskuID - 1)
                {
                        // Tiedon p‰ivitt‰minen tietokantaan
                        try
                        {

                            await DisplayAlert("Onnistui", "Tiedon poistaminen tietokannasta onnistui", "OK");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Tiedon poistaminen tietokannasta ep‰onnistui: {ex.Message}");
                        }

                }
                else
                {
                    await DisplayAlert("Virhe", "Yrit‰t poistaa v‰‰r‰‰ laskua", "OK");
                }
            }
            else
            {
                await DisplayAlert("Virhe", "Valitse lasku, jonka haluat poistaa", "OK");
            }
        }
    }
}
