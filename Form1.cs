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
        public Form1()
        {
            InitializeComponent();
            comport.DataReceived += new SerialDataReceivedEventHandler(DataRecieved);

        }
        private void DataRecieved(object sender, SerialDataReceivedEventArgs e)
        {
            string rxd = comport.ReadTo("\n");
            this.BeginInvoke(new SetTextDelegate(SerialReceived), new object[] { rxd });
        }

        private void SerialReceived(string inString)
        {
            textBox1.AppendText(inString + "\r\n");
            string[] data = inString.Split(',');

            if (data.Length >= 3) // 적어도 3개의 요소를 가져왔는지 확인
            {
                string humi = data[0];
                string temp = data[1];
                string hic = data[2];

                label2.Text = " " + humi + "%";
                label3.Text = " " + temp + "°C";
                label4.Text = " " + hic + "%"; //한줄 밑으로 내려가는 오류 잡기 

                string date = DateTime.Now.ToString();

                if(fanToggle.Checked)
                {
                    comport.Write("1");
                }
                else
                {
                    comport.Write("0");
                }

                using (MySqlConnection conn = new MySqlConnection(Conn))
                {
                    conn.Open();
                    MySqlCommand msc = new MySqlCommand("INSERT INTO temphumi(humi, temp, hic, date) VALUES(" + humi + ", " + temp + ", " + hic + ", '" + date + "');", conn);
                    msc.ExecuteNonQuery();
                }
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