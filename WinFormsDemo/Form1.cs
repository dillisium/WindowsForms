using Microsoft.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace WinFormsDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(main);
            thread.Start();
        }
        private void main()
        {
            string mac = GetMacAddress();
            string ip = GetExternalIp();
            string user = Environment.UserName;
            textBox1.Invoke((MethodInvoker)delegate
            {
                textBox1.Text = $"MAC Address: {mac}\nExternal IP: {ip}\nUser Name: {user}";
            });

            /* Connection string который использовала я:
            

            string connectionString = "Server=DESKTOP-84FAGOC\\SQLEXPRESS;Database=test;User Id=TestDev;Password=9163Dev;TrustServerCertificate=True;";

            */

            string connectionString = "Server=127.0.0.1,3304;Database=test;User Id=TestDev;Password=9163Dev;TrustServerCertificate=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string queryCheck = "SELECT * FROM TestTable WHERE MacAddress = @mac AND Username = @user";
                using (SqlCommand cmdCheck = new SqlCommand(queryCheck, connection))
                {
                    cmdCheck.Parameters.AddWithValue("@mac", mac);
                    cmdCheck.Parameters.AddWithValue("@user", user);

                    using (SqlDataReader reader = cmdCheck.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            MessageBox.Show($"База данных уже содержит запись со значениями: \nMAC Address: {mac}\nExternal IP: {ip}\nUser Name: {user}\nЗапись остановлена.");
                            return;
                        }

                    }

                    string queryInsert = "INSERT INTO TestTable (MacAddress, IpAddress, Username) VALUES (@mac, @ip, @user)";
                    using (SqlCommand cmdInsert = new SqlCommand(queryInsert, connection))
                    {
                        cmdInsert.Parameters.AddWithValue("@mac", mac);
                        cmdInsert.Parameters.AddWithValue("@ip", ip);
                        cmdInsert.Parameters.AddWithValue("@user", user);
                        cmdInsert.ExecuteNonQuery();
                    }
                    MessageBox.Show("Данные внесены в базу данных");

                }
            }
        }

        private string GetMacAddress()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault() ?? "No MAC Address";

        }
        private string GetExternalIp()
        {
            using HttpClient client = new HttpClient();
            return client.GetStringAsync("http://icanhazip.com").Result.Trim();
        }
    }
}
