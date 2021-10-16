using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AlturaAirDropper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///    

    public partial class MainWindow : Window
    {

        IWebProxy proxy;

        public HttpClientHandler clientHandler = new HttpClientHandler();

        public HttpClient client = new HttpClient();
        public MainWindow()
        {
            proxy = WebRequest.DefaultWebProxy;
            //System.Diagnostics.Debug.WriteLine(WebRequest.DefaultWebProxy.GetProxy(new Uri("http://www.google.com")));

            //System.Diagnostics.Debug.WriteLine(proxy.GetProxy(new Uri("http://www.google.com")));
            clientHandler.Proxy = proxy;
            clientHandler.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

            InitializeComponent();

            txt_HolderToken.IsEnabled = (bool)chk_SpecificToken.IsChecked;


        }


        private async Task<SimpleItem> GetItem(string collection, string tokenId)
        {

            try
            {
                var url = "http://api.alturanft.com/api/external/item?collectionId=" + collection;
                url += "&tokenId=" + tokenId;
                var msg = await client.GetStringAsync(url);

                JObject result = JObject.Parse(msg);

                if (msg.Contains("error")) return null;

                return JsonConvert.DeserializeObject<SimpleItem>(result["item"].ToString());

            }
            catch (Exception e)
            {
                Log("http error");
                Log(e.Message);
                return null;
            }
        }


        private async Task<Dictionary<string,int>> GetHolders(string collection, string tokenId = "")
        {
            Dictionary<string, int> addresses = new Dictionary<string, int>();


            try
            {
                var url = "http://api.alturanft.com/api/external/item?collectionId=" + collection;
                if (tokenId != "") url += "&tokenId=" + tokenId;
                var msg = await client.GetStringAsync(url);

                JObject result = JObject.Parse(msg);

                if (msg.Contains("error")) return null;
                List<SimpleItem> simpleItems = new List<SimpleItem>();
                if (tokenId == "")
                    simpleItems = JsonConvert.DeserializeObject<List<SimpleItem>>(result["items"].ToString());
                else
                {

                    simpleItems.Add(JsonConvert.DeserializeObject<SimpleItem>(result["item"].ToString()));
                }


                if (simpleItems != null)
                {
                    if (simpleItems.Count > 0)
                    {
                        Log(simpleItems[0].itemCollectionName);
                    }
                    if (tokenId != "") Log(simpleItems[0].name);
                }

                foreach (SimpleItem item in simpleItems)
                {
                    foreach (Holder h in item.holders)
                    {
                        if (!addresses.ContainsKey(h.address))
                        {
                            if (h.address != "0xe29f0b490f0d89ca7acac1c7bed2e07ecad65201" && h.address != "0x000000000000000000000000000000000000dead")
                            {
                                addresses.Add(h.address, h.balance);
                            }
                        }
                        else
                        {

                            addresses[h.address] += h.balance;
                        }
                    }

                }

                foreach (KeyValuePair<string, int> addy in addresses)
                {
                    Log(addy.Value + " - " + addy.Key);
                }

                return addresses;

            }
            catch (Exception e)
            {
                Log("http error");
                Log(e.Message);
                return null;
            }

        }

        private bool TransferItem(string collection, string from, string to, int amount, int index, string apiKey)
        {
            try
            {
                var url = "https://app.alturanft.com/api/external/token/transfer?apiKey="+apiKey;

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "POST";

                httpRequest.ContentType = "application/json";

                var data = @"{
                ""collection"":""" + collection + @""",
                ""to"":""" + to + @""",
                ""from"":""" + from + @""",
                ""amounts"": [" + amount + @"],
                ""ids"": [" + index + @"]
                }";

                Console.WriteLine(data);

                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Console.WriteLine(result);
                }

                Console.WriteLine(httpResponse.StatusCode);
                Console.WriteLine(httpResponse);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Http Error:" + e.Message);

                return false;
            }

        }

        public void Log(string message)
        {
            LogBox.AppendText("\n[" + DateTime.Now.ToString("HH:mm") + "]" + message);
            LogScrollViewer.ScrollToEnd();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (chk_SpecificToken.IsChecked == true)
                await GetHolders(txt_holdCol.Text, txt_HolderToken.Text);
            else
                await GetHolders(txt_holdCol.Text);
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void chk_SpecificToken_Checked(object sender, RoutedEventArgs e)
        {
            txt_HolderToken.IsEnabled = (bool)chk_SpecificToken.IsChecked;
            
        }

        private void btn_ManAirdrop_Click(object sender, RoutedEventArgs e)
        {
            

            if (!CheckAirDropInfo()) return;

            if (txt_ManAirDropAddress.Text.Length != 42)
            {
                Log("Invalid Address String, should be 42 Characters such as: 0xDDA06A9D45D28a5aC74D5Cfbe66c53d3cdf804Cd");
                return;
            }
            Log("Loading Please wait...");
            ButtonSetState(false);
            ManAirDrop();
        }

        private void btn_AutoAirdrop_Click(object sender, RoutedEventArgs e)
        {
            

            if (!CheckAirDropInfo()) return;
            Log("Loading Please wait...");
            ButtonSetState(false);
            AutoAirDrop();
            
        }

        private async Task AutoAirDrop()
        {
            if (txt_holdCol.Text.Length != 42)
            {
                Log("Invalid Holder String, should be 42 Characters such as: 0x27970a7fa322bbfefe208dbca7f8130a964c2b12");
                ButtonSetState(true);
                return;
            }

            bool holderItem = (bool)chk_SpecificToken.IsChecked;

            Dictionary<string, int> currentHolders;

            if (holderItem)
            {
                if (int.Parse(txt_HolderToken.Text) < 1)
                {
                    Log("Invalid Item Token - Should be a number greater than 0");
                    ButtonSetState(true);
                    return;
                }
                currentHolders = await GetHolders(txt_holdCol.Text, txt_HolderToken.Text);
            }
            else
            {
                currentHolders = await GetHolders(txt_holdCol.Text);
            }

            if (currentHolders == null)
            {
                Log("Could not retrieve Holders of this collection for some reason");
                ButtonSetState(true);
                return;
            }
            if (currentHolders.Count == 0)
            {
                Log("There are no holders of this collection");
                ButtonSetState(true);
                return;
            }

            var airDropItem = await GetItem(txt_AirDropCollection.Text, txt_numAirDropToken.Text);

            if (airDropItem == null)
            {
                Log("Cannot retrieve AirDropItem");
                ButtonSetState(true);
                return;
            }

            string msg = $"Are you sure you wish to Airdrop {txt_AmountToAirdrop.Text} {airDropItem.name}(s)\nID: {txt_numAirDropToken.Text} from Collection: {txt_AirDropCollection.Text}\n\n" +
                $"This will be airdropped to {currentHolders.Count} holders of:\n" +
                $"Collection: {txt_holdCol.Text}\n";
            if (holderItem)
            {
                msg += $"Token: {txt_HolderToken.Text}\n";
            }
            msg += $"\nPlease ensure you have enough BNB in your Dev wallet, and the airdrop items are currently owned by that wallet. \n\n" +
            $"" +
            $"Once this process starts it cannot be stopped";

            MessageBoxResult result = MessageBox.Show(msg, "Are you sure you wish to AirDrop These tokens?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            switch (result)
            {
                case MessageBoxResult.Yes:

                    await TaskAirDrop(currentHolders.Keys.ToList(), txt_APIKey.Text, txt_AirDropCollection.Text, txt_AmountToAirdrop.Text, txt_numAirDropToken.Text);
                    
                    break;
                case MessageBoxResult.No:
                    Log("Cancelling auto airdrop");
                    break;
            }
            ButtonSetState(true);
        }

        private async Task ManAirDrop()
        {
            var airDropItem = await GetItem(txt_AirDropCollection.Text, txt_numAirDropToken.Text);

            if (airDropItem == null)
            {
                Log("Cannot retrieve AirDropItem");
                ButtonSetState(true);
                return;
            }

            string msg = $"Are you sure you wish to Airdrop {txt_AmountToAirdrop.Text} {airDropItem.name}(s)\nID: {txt_numAirDropToken.Text} from Collection: {txt_AirDropCollection.Text}\n\n" +
                $"This will be airdropped to {txt_ManAirDropAddress.Text}";
            msg += $"\nPlease ensure you have enough BNB in your Dev wallet, and the airdrop items are currently owned by that wallet. \n\n" +
            $"" +
            $"Once this process starts it cannot be stopped";

            MessageBoxResult result = MessageBox.Show(msg, "Are you sure you wish to AirDrop These tokens?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    var addy = new List<string>();
                    addy.Add(txt_ManAirDropAddress.Text);
                   await TaskAirDrop(addy, txt_APIKey.Text,txt_AirDropCollection.Text,txt_AmountToAirdrop.Text,txt_numAirDropToken.Text);                    
                    break;
                case MessageBoxResult.No:
                    Log("Cancelling auto airdrop");
                    break;
            }

            ButtonSetState(true);
        }

        private async Task TaskAirDrop(List<string> addresses, string apiKey, string collection, string amount, string token)//collection, amount, index
        {
            if (addresses == null)
            {
                Log("Addresses = null, cancelling");
                return;
            }
            if (addresses.Count == 0)
            {
                Log("No Addresses found to drop to, cancelling");
                return;
            }

            foreach(string addy in addresses)
            {
              
                Log("Transferring Item to: " + addy);
                await Task.Delay(10);
                if (!TransferItem(collection, "", addy, int.Parse(amount), int.Parse(token), apiKey))
                {
                    Log($"Error Transferring Item, Cancelling incase of major issues:\n" +
                        $"Collection:{collection}\n" +
                        $"Token: {token}\n" +
                        $"To Addy:{addy}\n" +
                        $"Amount: {amount}\n");
                }
                await Task.Delay(10);
            }
        }

        private bool CheckAirDropInfo()
        {
            if (txt_APIKey.Text == "")
            {
                Log("No API Key Provided..");
                return false;
            }

            if (txt_AirDropCollection.Text.Length != 42)
            {
                Log("Invalid Collection String, should be 42 Characters such as: 0x27970a7fa322bbfefe208dbca7f8130a964c2b12");
                return false;
            }

            if (int.Parse(txt_numAirDropToken.Text) < 1)
            {
                Log("Invalid Airdrop Token - Should be a number greater than 0");
                return false;
            }

            if (int.Parse(txt_numAirDropToken.Text) < 1)
            {
                Log("Invalid Airdrop Amount - Should be a number greater than 0");
                return false;
            }

            return true;
        }


        private void ButtonSetState(bool state)
        {
            btn_AutoAirdrop.IsEnabled = state;
            btn_ManAirdrop.IsEnabled = state;
            btn_CheckHolders.IsEnabled = state;
            chk_SpecificToken.IsEnabled = state;
        }


        private void Btn_Twitter1_Click(object sender, RoutedEventArgs e)
        {
            WebsiteOpen("https://twitter.com/theshillverse");
        }


        private void Btn_Altura_Click(object sender, RoutedEventArgs e)
        {
            WebsiteOpen("https://app.alturanft.com/collection/0x27970a7fa322bbfefe208dbca7f8130a964c2b12");
        }

        

        private void WebsiteOpen(string url)
        {
            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void Btn_Github1_Click(object sender, RoutedEventArgs e)
        {
            WebsiteOpen("https://github.com/sdoddler");
        }

        private void Btn_BSC_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText("0xDDA06A9D45D28a5aC74D5Cfbe66c53d3cdf804Cd");
        }


    }
}
