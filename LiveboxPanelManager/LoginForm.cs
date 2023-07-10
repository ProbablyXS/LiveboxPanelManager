using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveboxPanelManager
{

    public partial class LoginForm : Form
    {
        public static readonly HttpClient client = new HttpClient();

        public static bool logged = false;

        public static string completeCookie;
        public static string sessionID;
        public static string contextID;

        public static string url;
        public static string login;
        public static string pass;


        private Ini INI = new Ini("param.ini");

        public LoginForm()
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
            url = "http://" + textBox3.Text + "/ws"; // URL
            login = textBox2.Text; // LOGIN
            pass = textBox1.Text; // PASSWORD

            HttpPOSTlogin();
        }


        public async Task HttpPOSTlogin()
        {
            if (logged) return;

            logged = true;

            var body = new
            {
                service = "sah.Device.Information",
                method = "createContext",
                parameters = new
                {
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
            }

            if (response.StatusCode.ToString() == "Unauthorized")
            {
                label4.Visible = true;
                label4.ForeColor = Color.Red;
                label4.Text = "Nom d'utilisateur ou mot de passe incorrect";
                logged = false;
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

            //Complete cookie
            completeCookie = "51a3cd15/accept-language=fr-FR,fr; UILang=fr; 51a3cd15/sessid=" + sessionID + "; sah/contextId=" + contextID + ";";

            AccueilForm frm = new AccueilForm();
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
            save();
        }

        private void save()
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

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                save();
            }
        }
    }
}
