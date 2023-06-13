using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//hello c###

//githut check23

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
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



            label2.Text = "습도 : " + data[0] + "%";
            label3.Text = "온도 : " + data[1] + "°C";
            //label4.Text = "체감온도 : " + data[2] + "'C";
            label4.Text = "체감온도 : " + data[2].Replace("\n", "").Replace("\n", "") + "°C".Replace("\n", "");


        }

        private void Form1_Load(object sender, EventArgs e)
        {
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
                // hello c#
                //woww


            }
            else
            {
                comport.Close();
                btnConnect.Text = "Connect";
            }
        }
    }
}

//변경 내용 작성 !!