using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        string Conn = "Server=localhost;Database=humidata;Uid=root;Pwd=0000;";
        SerialPort comport = new SerialPort();
        private delegate void SetTextDelegate(string getString);
        private MySqlConnection conn;
        private int dataCount = 0;

        public Form1()
        {
            InitializeComponent();
            comport.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            conn = new MySqlConnection(Conn);
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string rxd = comport.ReadTo("\n");
            this.BeginInvoke(new SetTextDelegate(SerialReceived), new object[] { rxd });
        }

        // 아두이노로부터 데이터를 받는 부분
        private void SerialReceived(string inString)
        {
            textBox1.AppendText(inString + "\r\n");
            string[] data = inString.Split(',');

            if (data.Length >= 4)
            {
                string humi = data[0];
                string temp = data[1];
                string hic = data[2];
                string soil = data[3];

                label2.Text = " " + humi + "%";
                label3.Text = " " + temp + "°C";
                label4.Text = " " + soil + "%";

                string date = DateTime.Now.ToString();

                // 토글 체크 켜지면 1, 꺼지면 0 아두이노로 신호 보내기
                if (fanToggle.Checked)
                {
                    comport.Write("1");
                }
                else
                {
                    comport.Write("0");
                }

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

                // 그래프에 데이터 추가
                chart1.Series[0].Points.AddXY(date, double.Parse(humi));
                chart1.Series[1].Points.AddXY(date, double.Parse(temp));
                chart1.Series[2].Points.AddXY(date, double.Parse(soil));

                dataCount++;
            }
            else
            {
                // 데이터가 올바르게 파싱되지 않았을 경우 에러 처리 등을 수행할 수 있습니다.
                // 예를 들어, 데이터 포맷이 맞지 않아 데이터를 무시하거나, 에러 로그를 출력할 수 있습니다.
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label9.Text = DateTime.Now.ToString();

            cmbComport.Items.Clear();
            var portName = System.IO.Ports.SerialPort.GetPortNames();
            cmbComport.Items.AddRange(portName);
            cmbComport.SelectedIndex = cmbComport.Items.Count - 1;

            cmbBoardlate.Items.Clear();
            cmbBoardlate.Items.Add("9600");
            cmbBoardlate.Items.Add("115200");
            cmbBoardlate.SelectedIndex = 0;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (btnConnect.Text == "Connect")
            {
                comport.PortName = cmbComport.Text;
                comport.BaudRate = Convert.ToInt16(cmbBoardlate.Text);
                comport.DataBits = 8;
                comport.Parity = Parity.None;
                comport.StopBits = StopBits.One;
                comport.Handshake = Handshake.None;
                comport.Open();
                comport.DiscardInBuffer();
                btnConnect.Text = "Close";
            }
            else
            {
                comport.Close();
                btnConnect.Text = "Connect";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form Programinfo = new Autoset();
            Programinfo.ShowDialog();
        }
    }
}
