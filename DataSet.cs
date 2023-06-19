using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp1
{
    public partial class DataSet : Form
    {
        string Conn = "Server=localhost;Database=humidata;Uid=root;Pwd=0000;";
        SerialPort comport = new SerialPort();
        private delegate void SetTextDelegate(string getString);
        private MySqlConnection conn;
        private int dataCount = 0;

        public DataSet()
        {
            InitializeComponent();

            comport.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            conn = new MySqlConnection(Conn);

            toolStripComboBox1.Text = "월 선택";
            toolStripComboBox2.Text = "일 선택";

            // 1월부터 12월까지 콤보박스에 추가
            for (int month = 1; month <= 12; month++)
            {
                toolStripComboBox1.Items.Add(month.ToString() + "월");
            }

            for (int day = 1; day <= 31; day++)
            {
                toolStripComboBox2.Items.Add(day.ToString() + "일");
            }

            toolStripComboBox1.SelectedIndexChanged += toolStripComboBox_SelectedIndexChanged;
            toolStripComboBox2.SelectedIndexChanged += toolStripComboBox_SelectedIndexChanged;
        }

        private void toolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            if (toolStripComboBox1.SelectedItem == null || toolStripComboBox2.SelectedItem == null)
            {
                return;
            }

            string month = toolStripComboBox1.SelectedItem.ToString().Split('월')[0];
            string day = toolStripComboBox2.SelectedItem.ToString().Split('일')[0];

            string query = $"SELECT * FROM temphumi WHERE MONTH(date) = {month} AND DAY(date) = {day}";

            using (MySqlConnection connection = new MySqlConnection(Conn))
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        listView1.Items.Clear();
                        while (reader.Read())
                        {
                            ListViewItem item = new ListViewItem(reader["num"].ToString());
                            item.SubItems.Add(reader["humi"].ToString());
                            item.SubItems.Add(reader["temp"].ToString());
                            item.SubItems.Add(reader["date"].ToString());
                            item.SubItems.Add(reader["soil"].ToString());
                            listView1.Items.Add(item);
                        }
                    }
                }
            }
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string rxd = comport.ReadTo("\n");
            this.BeginInvoke(new SetTextDelegate(SerialReceived), new object[] { rxd });
        }

        private void SerialReceived(string inString)
        {
            string[] data = inString.Split(',');
            string humi = data[0];
            string temp = data[1];
            string hic = data[2];
            string soil = data[3];

            string date = DateTime.Now.ToString();


            conn.Open();
            MySqlCommand msc = new MySqlCommand("INSERT INTO temphumi(humi, temp, hic, date, soil) VALUES(" + humi + ", " + temp + ", " + hic + ", '" + date + "', " + soil + ");", conn);
            msc.ExecuteNonQuery();

            // Retrieve the inserted 'num' value
            msc = new MySqlCommand("SELECT LAST_INSERT_ID();", conn);
            int num = Convert.ToInt32(msc.ExecuteScalar());

            conn.Close();

            // 리스트뷰에 데이터 추가
            ListViewItem lvi = new ListViewItem();

            lvi.Text = num.ToString();
            lvi.SubItems.Add(humi);
            lvi.SubItems.Add(temp);
            lvi.SubItems.Add(date);
            lvi.SubItems.Add(soil);

            listView1.Items.Add(lvi);
        }
    }
}