using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PortForwardingLivebox
{
    public partial class AccueilForm : Form
    {
        public static bool WANstatus = false;
        public static bool IPTVStatus = false;
        public static bool VOIPStatus = false;
        public static bool WIFIStatus = false;

        public static bool _Loading = true;
        public bool Loading
        {
            get { return _Loading; }
            set { _Loading = value; LoadingState(); }
        }

        protected virtual async Task LoadingState()
        {

            if (!Loading)
            {
                pictureBox2.Enabled = false;
                pictureBox2.Visible = false;

            }
        }

        public AccueilForm()
        {
            InitializeComponent();

            HttpPOSTGetAllInfo();
        }

        public async Task HttpPOSTGetAllInfo()
        {
            //GET WAN
            var body = new
            {
                service = "NMC",
                method = "getWANStatus",
                parameters = new { }
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/x-sah-ws-4-call+json");
            LoginForm.client.DefaultRequestHeaders.Clear();
            LoginForm.client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "X-Sah " + LoginForm.contextID);
            LoginForm.client.DefaultRequestHeaders.Add("Cookie", LoginForm.completeCookie);
            LoginForm.client.DefaultRequestHeaders.Add("X-Context", LoginForm.contextID);

            var response = await LoginForm.client.PostAsync(LoginForm.url, content);
            var res = await response.Content.ReadAsStringAsync();
            var json = (JObject)JsonConvert.DeserializeObject(res);

            if (json["data"]["WanState"].ToString() == "up")
            {
                WANstatus = true;
                button2.BackColor = Color.LimeGreen;
            }
            else
            {
                button2.BackColor = Color.Red;
            }

            //GET IPTV
            body = new
            {
                service = "NMC.OrangeTV",
                method = "getIPTVStatus",
                parameters = new { }
            };

            content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/x-sah-ws-4-call+json");
            LoginForm.client.DefaultRequestHeaders.Clear();
            LoginForm.client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "X-Sah " + LoginForm.contextID);
            LoginForm.client.DefaultRequestHeaders.Add("Cookie", LoginForm.completeCookie);
            LoginForm.client.DefaultRequestHeaders.Add("X-Context", LoginForm.contextID);

            response = await LoginForm.client.PostAsync(LoginForm.url, content);
            res = await response.Content.ReadAsStringAsync();
            json = (JObject)JsonConvert.DeserializeObject(res);

            if (json["data"]["IPTVStatus"].ToString() == "Available")
            {
                IPTVStatus = true;
                button3.BackColor = Color.LimeGreen;
            }
            else
            {
                button3.BackColor = Color.Red;
            }

            //GET VOIP
            var bodyWParam = new
            {
                service = "NeMo.Intf.voip",
                method = "getFirstParameter",
                parameters = new
                {
                    name = "IPAddress",
                    traverse = "down"
                }

            };

            content = new StringContent(JsonConvert.SerializeObject(bodyWParam), Encoding.UTF8, "application/x-sah-ws-4-call+json");
            LoginForm.client.DefaultRequestHeaders.Clear();
            LoginForm.client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "X-Sah " + LoginForm.contextID);
            LoginForm.client.DefaultRequestHeaders.Add("Cookie", LoginForm.completeCookie);
            LoginForm.client.DefaultRequestHeaders.Add("X-Context", LoginForm.contextID);

            response = await LoginForm.client.PostAsync(LoginForm.url, content);
            res = await response.Content.ReadAsStringAsync();
            json = (JObject)JsonConvert.DeserializeObject(res);

            if (json["status"].HasValues == true)
            {
                VOIPStatus = true;
                button4.BackColor = Color.LimeGreen;
            }
            else
            {
                button4.BackColor = Color.Red;
            }

            //GET WIFI
            body = new
            {
                service = "NMC.Wifi",
                method = "get",
                parameters = new { }
            };

            content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/x-sah-ws-4-call+json");
            LoginForm.client.DefaultRequestHeaders.Clear();
            LoginForm.client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "X-Sah " + LoginForm.contextID);
            LoginForm.client.DefaultRequestHeaders.Add("Cookie", LoginForm.completeCookie);
            LoginForm.client.DefaultRequestHeaders.Add("X-Context", LoginForm.contextID);

            response = await LoginForm.client.PostAsync(LoginForm.url, content);
            res = await response.Content.ReadAsStringAsync();
            json = (JObject)JsonConvert.DeserializeObject(res);

            if ((bool)json["status"]["Enable"] == true)
            {
                WIFIStatus = true;
                button8.BackColor = Color.LimeGreen;
            }
            else
            {
                button8.BackColor = Color.Red;
            }


            //STOP LOADING
            Loading = false;

        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            if (btn.Name == button1.Name)
            {
                label2.Text = "Mes équipements connectés";
            }
            else if (btn.Name == button2.Name)
            {
                if (WANstatus) { label2.Text = "Internet disponible"; }
                else { label2.Text = "Internet indisponible"; }
            }
            else if (btn.Name == button3.Name)
            {
                if (IPTVStatus) { label2.Text = "Télévision disponible"; }
                else { label2.Text = "Télévision indisponible"; }
            }
            else if (btn.Name == button4.Name)
            {
                if (VOIPStatus) { label2.Text = "Téléphone disponible"; }
                else { label2.Text = "Téléphone indisponible"; }
            }
            else if (btn.Name == button8.Name)
            {
                if (WIFIStatus) { label2.Text = "Wi-Fi activé"; }
                else { label2.Text = "Wi-Fi désactivé"; }
            }
            else if (btn.Name == button13.Name)
            {
                label2.Text = "Réseau";
            }
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            label2.Text = "";
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (Loading) return;

            NatPatForm frm = new NatPatForm();

            frm.label1.Text = "SessionID = " + LoginForm.sessionID;
            frm.label2.Text = "contextID = " + LoginForm.contextID;
            frm.label3.Text = "cookie = " + LoginForm.completeCookie;

            frm.Show();
            this.Hide();
        }

        private void AccueilForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
