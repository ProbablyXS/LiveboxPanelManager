using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PortForwardingLivebox
{
    public partial class Form1 : Form
    {
        private static readonly HttpClient client = new HttpClient();
        public static List<rulesList> rulesL = new List<rulesList>();

        public static bool logged = false;

        public static string completeCookie;
        public static string sessionID;
        public static string contextID;

        public static string url;
        public static string login;
        public static string pass;

        public static string protocolTcpUdp; // 6 = TCP, 17 = UDP

        public class PROTOCOL // 6 = TCP // 17 = UDP
        {
            public static string TCP_Name { get; } = "TCP";
            public static string TCP_Id { get; } = "6";
            public static string UDP_Name { get; } = "UDP";
            public static string UDP_Id { get; } = "17";
            public static string TCPUDP_Name { get; } = "TCP/UDP";
            public static string TCPUDP_Id { get; } = "6,17";
        }

        public class rulesList
        {
            public string Id { get; set; }
            public string Ip { get; set; }
        }

        public Form1()
        {
            InitializeComponent();

            comboBox1.SelectedIndex = 2; // TCP/UDP
        }

        public async Task HttpPOSTlogin()
        {
            if (logged) return;

            var body = new
            {
                service = "sah.Device.Information",
                method = "createContext",
                parameters = new { 
                    applicationName = "webui", 
                    username = login, 
                    password = pass
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", "X-Sah-Login");

            var response = await client.PostAsync(url, content);

            //SESSION ID
            HttpHeaders headers = response.Headers;
            IEnumerable<string> values;
            if (headers.TryGetValues("Set-Cookie", out values))
            {

                sessionID = values.First();

                sessionID = sessionID.Split(new string[] { "sessid=" }, StringSplitOptions.None).Last().Split(new string[] { ";" }, StringSplitOptions.None).First();

                label1.Text = "SessionID = " + sessionID;
            }

            if (response.StatusCode.ToString() == "Unauthorized")
            {
                MessageBox.Show("Login or password incorect");
                return;
            } 
            else
            {
                logged = true;
                button4.Enabled = false;
            }

            //CONTEXT ID
            var res = await response.Content.ReadAsStringAsync();
            var json = (JObject)JsonConvert.DeserializeObject(res);
            
            contextID = json["data"]["contextID"].ToString();

            label2.Text = "contextID = " + contextID;

            //Complete cookie
            completeCookie = "51a3cd15/accept-language=fr-FR,fr; UILang=fr; 51a3cd15/sessid=" + sessionID + "; sah/contextId=" + contextID + ";";

            label3.Text = "cookie = " + completeCookie;

            HttpPOSTRefreshPortForwardingList();
        }

        public async Task HttpPOSTAddPortForwarding()
        {
            var body = new
            {
                service = "Firewall",
                method = "setPortForwarding",
                parameters = new
                {
                    id = textBox3.Text,
                    internalPort = numericUpDown1.Text,
                    externalPort = numericUpDown2.Text,
                    destinationIPAddress = textBox7.Text,
                    enable = "true",
                    persistent = "true",
                    protocol = protocolTcpUdp,
                    description = textBox3.Text,
                    sourceInterface = "data",
                    origin = "webui",
                    destinationMACAddress = "",
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/x-sah-ws-4-call+json");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "X-Sah " + contextID);
            client.DefaultRequestHeaders.Add("Cookie", completeCookie);
            client.DefaultRequestHeaders.Add("X-Context", contextID);

            var response = await client.PostAsync(url, content);
            var res = await response.Content.ReadAsStringAsync();

            MessageBox.Show(res + " PORT ADDED");

            HttpPOSTRefreshPortForwardingList();
        }

        public async Task HttpPOSTDeletePortForwarding()
        {
            var body = new
            {
                service = "Firewall",
                method = "deletePortForwarding",
                parameters = new
                {
                    id = rulesL[comboBox1.SelectedIndex].Id,
                    destinationIPAddress = rulesL[comboBox1.SelectedIndex].Ip,
                    origin = "webui",
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/x-sah-ws-4-call+json");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "X-Sah " + contextID);
            client.DefaultRequestHeaders.Add("Cookie", completeCookie);
            client.DefaultRequestHeaders.Add("X-Context", contextID);

            var response = await client.PostAsync(url, content);
            var res = await response.Content.ReadAsStringAsync();

            MessageBox.Show(res + " PORT DELETED");

            HttpPOSTRefreshPortForwardingList();
        }

        public async Task HttpPOSTRefreshPortForwardingList()
        {
            listBox1.Items.Clear();
            rulesL.Clear();

            var body = new
            {
                service = "Firewall",
                method = "getPortForwarding",
                parameters = new
                {
                    origin = "webui"
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/x-sah-ws-4-call+json");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "X-Sah " + contextID);
            client.DefaultRequestHeaders.Add("Cookie", completeCookie);
            client.DefaultRequestHeaders.Add("X-Context", contextID);

            var response = await client.PostAsync(url, content);
            var res = await response.Content.ReadAsStringAsync();

            var json = (JObject)JsonConvert.DeserializeObject(res);

            foreach (var i in json["status"])
            {

                var NAME = i.First()["Description"];
                var INTERNAL_PORT = i.First()["InternalPort"];
                var EXTERNAL_PORT = i.First()["ExternalPort"];
                var protocol = i.First()["Protocol"];
                var DESTINATIONIPADDRESS = i.First()["DestinationIPAddress"];

                if (protocol.ToString() == PROTOCOL.TCP_Id)
                {
                    protocol = PROTOCOL.TCP_Name;
                }
                else if (protocol.ToString() == PROTOCOL.UDP_Name)
                {
                    protocol = PROTOCOL.UDP_Id;
                }
                else if (protocol.ToString() == PROTOCOL.TCPUDP_Name)
                {
                    protocol = PROTOCOL.TCPUDP_Id;
                }

                rulesL.Add(new rulesList 
                { 
                    Id = i.First()["Id"].ToString(), 
                    Ip = i.First()["DestinationIPAddress"].ToString() 
                });

                listBox1.Items.Add("Name: " + NAME + 
                                   " Port interne: " + INTERNAL_PORT +
                                   " Port externe: " + EXTERNAL_PORT +
                                   " Protocol: " + protocol +
                                   " Équipement: " + DESTINATIONIPADDRESS);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HttpPOSTAddPortForwarding();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text.ToLower() == PROTOCOL.TCP_Name.ToLower())
            {
                protocolTcpUdp = PROTOCOL.TCP_Id;
            }
            else if (comboBox1.Text.ToLower() == PROTOCOL.UDP_Name.ToLower())
            {
                protocolTcpUdp = PROTOCOL.UDP_Id;
            }
            else if (comboBox1.Text.ToLower() == PROTOCOL.TCPUDP_Name.ToLower())
            {
                protocolTcpUdp = PROTOCOL.TCPUDP_Id;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HttpPOSTRefreshPortForwardingList();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            HttpPOSTDeletePortForwarding();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            url = "http://" + textBox8.Text + "/ws"; // URL
            login = textBox2.Text; // LOGIN
            pass = textBox1.Text; // PASSWORD

            HttpPOSTlogin();
        }
    }
}
