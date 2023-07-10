using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveboxPanelManager
{
    public partial class NatPatForm : Form
    {
        public static List<rulesList> rulesL = new List<rulesList>();

        public static string protocolTcpUdp; // 6 = TCP, 17 = UDP
        public string[] protocolId = { "6", "17", "6,17" };
        public string[] protocolName = { "TCP", "UDP", "TCP/UDP" };

        public class rulesList
        {
            public string Id { get; set; }
            public string Ip { get; set; }

            public rulesList(string id, string ip)
            {
                this.Id = id;
                this.Ip = ip;
            }
        }


        public NatPatForm()
        {
            InitializeComponent();

            comboBox1.SelectedIndex = 2; // TCP/UDP
        }

        public async Task HttpPOSTAddPortForwarding()
        {

            if (LoginForm.function_started) return;
            LoginForm.function_started = true;

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
            LoginForm.client.DefaultRequestHeaders.Clear();
            LoginForm.client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "X-Sah " + LoginForm.contextID);
            LoginForm.client.DefaultRequestHeaders.Add("Cookie", LoginForm.completeCookie);
            LoginForm.client.DefaultRequestHeaders.Add("X-Context", LoginForm.contextID);

            var response = await LoginForm.client.PostAsync(LoginForm.url, content);
            var res = await response.Content.ReadAsStringAsync();

            HttpPOSTRefreshPortForwardingList();
        }

        public async Task HttpPOSTDeletePortForwarding(int indexC)
        {
            var body = new
            {
                service = "Firewall",
                method = "deletePortForwarding",
                parameters = new
                {
                    id = rulesL[indexC].Id,
                    destinationIPAddress = rulesL[indexC].Ip,
                    origin = "webui",
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/x-sah-ws-4-call+json");
            LoginForm.client.DefaultRequestHeaders.Clear();
            LoginForm.client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "X-Sah " + LoginForm.contextID);
            LoginForm.client.DefaultRequestHeaders.Add("Cookie", LoginForm.completeCookie);
            LoginForm.client.DefaultRequestHeaders.Add("X-Context", LoginForm.contextID);

            var response = await LoginForm.client.PostAsync(LoginForm.url, content);
            var res = await response.Content.ReadAsStringAsync();

            HttpPOSTRefreshPortForwardingList();
        }

        public async Task HttpPOSTRefreshPortForwardingList()
        {
            rulesL.Clear();
            dataGridView1.Rows.Clear();

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
            LoginForm.client.DefaultRequestHeaders.Clear();
            LoginForm.client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "X-Sah " + LoginForm.contextID);
            LoginForm.client.DefaultRequestHeaders.Add("Cookie", LoginForm.completeCookie);
            LoginForm.client.DefaultRequestHeaders.Add("X-Context", LoginForm.contextID);

            var response = await LoginForm.client.PostAsync(LoginForm.url, content);
            var res = await response.Content.ReadAsStringAsync();

            var json = (JObject)JsonConvert.DeserializeObject(res);

            foreach (var i in json["status"])
            {

                var NAME = i.First()["Description"];
                var INTERNAL_PORT = i.First()["InternalPort"];
                var EXTERNAL_PORT = i.First()["ExternalPort"];
                var protocol = i.First()["Protocol"];
                var DESTINATIONIPADDRESS = i.First()["DestinationIPAddress"];

                if (protocol.ToString() == protocolId[0])
                {
                    protocol = protocolName[0];
                }
                else if (protocol.ToString() == protocolId[1])
                {
                    protocol = protocolName[1];
                }
                else if (protocol.ToString() == protocolId[2])
                {
                    protocol = protocolName[2];
                }

                rulesL.Add(new rulesList(
                    i.First()["Id"].ToString(),
                    i.First()["DestinationIPAddress"].ToString()));

                dataGridView1.Rows.Add(NAME.ToString(), INTERNAL_PORT.ToString(), EXTERNAL_PORT.ToString(), protocol.ToString(), DESTINATIONIPADDRESS.ToString());
            }

            LoginForm.function_started = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox3.TextLength == 0 || textBox7.TextLength == 0) 
            {
                MessageBox.Show("Insérer une valeur !");
                return;
            }
            HttpPOSTAddPortForwarding();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text.ToLower() == protocolName[0].ToLower())
            {
                protocolTcpUdp = protocolId[0].ToString();
            }
            else if (comboBox1.Text.ToLower() == protocolName[1].ToLower())
            {
                protocolTcpUdp = protocolId[1];
            }
            else if (comboBox1.Text.ToLower() == protocolName[2].ToLower())
            {
                protocolTcpUdp = protocolId[2];
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            HttpPOSTRefreshPortForwardingList();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var frm = Owner as AccueilForm;
            frm.Show();
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            button1.ForeColor = Color.Black;
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            button1.ForeColor = Color.FromArgb(241, 110, 0);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewImageColumn)
            {
                HttpPOSTDeletePortForwarding(senderGrid.Columns[e.RowIndex].Index);
            }
        }
    }
}
