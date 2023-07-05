using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PortForwardingLivebox
{
    public partial class Login : Form
    {

        private Ini INI = new Ini("param.ini");

        public Login()
        {
            InitializeComponent();

            currentTime();

            if (File.Exists("param.ini"))
            {
                textBox3.Text = INI.Read("Ip", "Identifiant");
                textBox2.Text = INI.Read("Login", "Identifiant");
                textBox1.Text = INI.Read("Pass", "Identifiant");
            }
        }

        public async Task currentTime()
        {
            label3.Text = DateTime.Today.ToString("dddd dd MMMM");

            while (true)
            {
                label2.Text = DateTime.Now.ToString("HH") + "h" + DateTime.Now.ToString("mm");
                await Task.Delay(1000);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1.url = "http://" + textBox3.Text + "/ws"; // URL
            Form1.login = textBox2.Text; // LOGIN
            Form1.pass = textBox1.Text; // PASSWORD

            HttpPOSTlogin();
        }


        public async Task HttpPOSTlogin()
        {
            if (Form1.logged) return;

            Form1.logged = true;

            var body = new
            {
                service = "sah.Device.Information",
                method = "createContext",
                parameters = new
                {
                    applicationName = "webui",
                    username = Form1.login,
                    password = Form1.pass
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            Form1.client.DefaultRequestHeaders.Clear();
            Form1.client.DefaultRequestHeaders.Add("Authorization", "X-Sah-Login");

            var response = await Form1.client.PostAsync(Form1.url, content);

            Form1 frm = new Form1();

            //SESSION ID
            HttpHeaders headers = response.Headers;
            IEnumerable<string> values;
            if (headers.TryGetValues("Set-Cookie", out values))
            {

                Form1.sessionID = values.First();

                Form1.sessionID = Form1.sessionID.Split(new string[] { "sessid=" }, StringSplitOptions.None).Last().Split(new string[] { ";" }, StringSplitOptions.None).First();

                frm.label1.Text = "SessionID = " + Form1.sessionID;
            }

            if (response.StatusCode.ToString() == "Unauthorized")
            {
                label4.Visible = true;
                label4.ForeColor = Color.Red;
                label4.Text = "Nom d'utilisateur ou mot de passe incorrect";
                Form1.logged = false;
                return;
            }
            else
            {
                Form1.logged = true;
                button4.Enabled = false;
            }

            //CONTEXT ID
            var res = await response.Content.ReadAsStringAsync();
            var json = (JObject)JsonConvert.DeserializeObject(res);

            Form1.contextID = json["data"]["contextID"].ToString();

            frm.label2.Text = "contextID = " + Form1.contextID;

            //Complete cookie
            Form1.completeCookie = "51a3cd15/accept-language=fr-FR,fr; UILang=fr; 51a3cd15/sessid=" + Form1.sessionID + "; sah/contextId=" + Form1.contextID + ";";

            frm.label3.Text = "cookie = " + Form1.completeCookie;

            frm.Show();
            this.Hide();
        }

        private void button4_MouseEnter(object sender, EventArgs e)
        {
            button4.ForeColor = Color.White;
        }

        private void button4_MouseLeave(object sender, EventArgs e)
        {
            button4.ForeColor = Color.Black;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (textBox1.Text.Length == 0)
            {
                label4.Visible = true;
                label4.ForeColor = Color.Red;
                label4.Text = "Veuillez insérer un mot de passe";
                return;
            }

            INI.Write("Ip", textBox3.Text, "Identifiant");
            INI.Write("Login", textBox2.Text, "Identifiant");
            INI.Write("Pass", textBox1.Text, "Identifiant");

            label4.Visible = true;
            label4.ForeColor = Color.FromArgb(241, 110, 0);
            label4.Text = "Les informations ont été sauvegardé";
        }
    }
}
