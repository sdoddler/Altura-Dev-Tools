using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AlturaImageChanger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

            txt_itemToken.IsEnabled = (bool)chk_SpecificToken.IsChecked;


        }



        public void Log(string message)
        {
            LogBox.AppendText("\n[" + DateTime.Now.ToString("HH:mm") + "]" + message);
            LogScrollViewer.ScrollToEnd();
        }


        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void chk_SpecificToken_Checked(object sender, RoutedEventArgs e)
        {
            txt_itemToken.IsEnabled = (bool)chk_SpecificToken.IsChecked;

        }


        private async Task TriggerChangeItem()
        {


            if (!CheckImageChangeInfo()) return;
            Log("Loading Please wait...");
            ButtonSetState(false);


            List<SimpleItem> items = new List<SimpleItem>();
            if ((bool)chk_SpecificToken.IsChecked)
                items = await GetItems(txt_holdCol.Text,txt_itemToken.Text);
            else
                items = await GetItems(txt_holdCol.Text);


            if (items != null)
            {
                if (items.Count > 0) {
                    var msg = $"You are about to change images on {items.Count} from {items[0].itemCollectionName}, are you sure you wish to do this?";
                MessageBoxResult result = MessageBox.Show(msg, "Are you sure you wish to change these images?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:

                            await TaskChangeImages(items, txt_APIKey.Text, int.Parse(txt_imageIndex.Text), txt_holdCol.Text);
                            ButtonSetState(true);
                            break;
                        case MessageBoxResult.No:
                            Log("Cancelling auto airdrop");
                            ButtonSetState(true);
                            break;
                    } }
            }

        }

        private async Task TaskChangeImages(List<SimpleItem> items, string apiKey, int imageIndex, string collection)
        {
            foreach (SimpleItem item in items)
            {
                Log($"Updating Item: {item.name} to image index {imageIndex}");
               await Task.Delay(10);
                if (!UpdateImage(apiKey, item.tokenId, collection, imageIndex))
                {
                    Log($"Received an error updating {item.name} at index {item.tokenId} cancelling batch");
                }
            }
        }

        private bool UpdateImage(string apiKey, int tokenId, string collection, int imageIndex)
        {
            try
            {
                var url = "https://app.alturanft.com/api/external/item/image?apiKey="+apiKey;

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "POST";

                httpRequest.ContentType = "application/json";

                var data = @"{
  ""tokenId"":" + tokenId + @",
  ""collectionId"":""" + collection + @""",
  ""imageIndex"": " + imageIndex + "}";


                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    //  Console.WriteLine(result);
                }

                Console.WriteLine(httpResponse.StatusCode);
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("Http Error:" + e.Message);
                // Console.WriteLine(e.Data);
                return false;
            }

        }

        private async Task<List<SimpleItem>> GetItems(string collection, string tokenId = "")
        {
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

                return simpleItems;
            }
            catch (Exception e)
            {
                Log("http error");
                Log(e.Message);
                return null;
            }
        }


                private bool CheckImageChangeInfo()
        {
            if (txt_APIKey.Text == "")
            {
                Log("No API Key Provided..");
                return false;
            }


            if (int.Parse(txt_imageIndex.Text) < 1)
            {
                Log("Invalid Image Index - Should be a number greater than 0");
                return false;
            }

            if (txt_holdCol.Text.Length != 42)
            {
                Log("Invalid Holder String, should be 42 Characters such as: 0x27970a7fa322bbfefe208dbca7f8130a964c2b12");

                return false;
            }

            if (int.Parse(txt_itemToken.Text) < 1)
            {
                Log("Invalid Item Index - Should be a number greater than 0");
                return false;
            }

            return true;
        }


        private void ButtonSetState(bool state)
        {
            btn_AutoAirdrop.IsEnabled = state;
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

        private void btn_ChangeImage(object sender, RoutedEventArgs e)
        {
            TriggerChangeItem();
        }
    }
}
