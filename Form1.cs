using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        //string Conn = "Server=localhost;Database=humidata;Uid=root;Pwd=0000;";
        string Conn2 = "Server=localhost;Database=mushroom;Uid=root;Pwd=0000;";
        SerialPort comport = new SerialPort();
        private delegate void SetTextDelegate(string getString);
        private MySqlConnection conn;
        private int dataCount = 0;

        public Form1()
        {
            InitializeComponent();
            comport.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            //conn = new MySqlConnection(Conn);
        }

        private void LoadhumidataFromMySQL()
        {
            string Conn = "Server=localhost;Database=humidata;Uid=root;Pwd=0000;";

            int lastNum = 0; // The 'num' value of the last row we fetched

            while (true)
            {
                using (MySqlConnection conn = new MySqlConnection(Conn))
                {
                    conn.Open();

                    //리스트뷰에 데이터 추가 
                    string sql = "SELECT * FROM temphumi WHERE num > @lastNum ORDER BY num DESC LIMIT 20";
                    MySqlCommand command = new MySqlCommand(sql, conn);
                    command.Parameters.AddWithValue("@lastNum", lastNum);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int num = reader.IsDBNull(reader.GetOrdinal("num")) ? 0 : reader.GetInt32("num");
                            double humi = reader.IsDBNull(reader.GetOrdinal("humi")) ? 0.0 : reader.GetDouble("humi");
                            double temp = reader.IsDBNull(reader.GetOrdinal("temp")) ? 0.0 : reader.GetDouble("temp");
                            string date = reader.IsDBNull(reader.GetOrdinal("date")) ? string.Empty : reader.GetString("date");
                            int soil = reader.IsDBNull(reader.GetOrdinal("soil")) ? 0 : reader.GetInt32("soil");

                            Console.WriteLine($"num: {num}, humi: {humi}, temp: {temp}, date: {date}, soil: {soil}");

                            lastNum = num; // 마지막으로 읽은 num을 lastnum에 저장. 다음쿼리에서 이값보다 큰행만 가져오기 위해 

                            // 업데이트 
                            this.Invoke((MethodInvoker)delegate
                            {
                                label3.Text = temp.ToString() + "°C";
                                label2.Text = humi.ToString() + "%";
                                label4.Text = soil.ToString() + "%"; // soil is now a double

                                // 리스트뷰의 마지막 항목 제거(리스트뷰 항복 20개 유지하기 위해서)
                                ListViewItem item = new ListViewItem(new string[] { num.ToString(), humi.ToString(), temp.ToString(), soil.ToString(),date });
                                listView1.Items.Insert(0, item); // Add the item to the top of the list

                                // 리스트뷰 항목수가 20개 초과하는 동안 루프 실행 
                                while (listView1.Items.Count > 20)
                                {
                                    listView1.Items.RemoveAt(listView1.Items.Count - 1); // Remove the last item
                                }
                                // Add data to chart
                                chart1.Series[0].Points.AddXY(date, Convert.ToDouble(humi));
                                chart1.Series[1].Points.AddXY(date, Convert.ToDouble(temp));
                                chart1.Series[2].Points.AddXY(date, Convert.ToDouble(soil));
                                dataCount++;

                                // Keep only the most recent 20 points
                                while (chart1.Series[0].Points.Count > 20)
                                {
                                    chart1.Series[0].Points.RemoveAt(0);
                                }
                                while (chart1.Series[1].Points.Count > 20)
                                {
                                    chart1.Series[1].Points.RemoveAt(0);
                                }
                                while (chart1.Series[2].Points.Count > 20)
                                {
                                    chart1.Series[2].Points.RemoveAt(0);
                                }
                            });


                        }
                    }
                }

                Thread.Sleep(2000); // Delay for 2 seconds
            }
        }

        private void LoadMushroomDataFromMySQL()
        {
            try
            {
                using (MySqlConnection conn2 = new MySqlConnection(Conn2))
                {
                    conn2.Open();

                    string query = "SELECT small, medium, large FROM pyogo ORDER BY date DESC LIMIT 1";
                    MySqlCommand command = new MySqlCommand(query, conn2);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int smallCount = Convert.ToInt32(reader["small"]);
                            int mediumCount = Convert.ToInt32(reader["medium"]);
                            int largeCount = Convert.ToInt32(reader["large"]);

                            // Do something with the retrieved data
                            // For example, update labels with the values
                            label15.Text = smallCount.ToString();
                            label16.Text = mediumCount.ToString();
                            label17.Text = largeCount.ToString();

                            

                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error retrieving pyogo data from MySQL: " + ex.Message);
            }
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string rxd = comport.ReadTo("\n");
            this.BeginInvoke(new SetTextDelegate(SerialReceived), new object[] { rxd });
        }

        // 아두이노로부터 데이터를 받는 부분
        private void SerialReceived(string inString)
        {
        

                // 토글 체크 켜지면 1, 꺼지면 0 아두이노로 신호 보내기
                if (fanToggle.Checked)
                {
                    comport.Write("3");
                }
                else if (watertoggle.Checked)
                {
                    comport.Write("1");
                }
                else if (!watertoggle.Checked)  // check if watertoggle is unchecked (off)
                {
                    comport.Write("2");
                }
                else
                {
                    comport.Write("4");
                }


                conn.Open();


                conn.Close();

                
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label9.Text = DateTime.Now.ToString();
            LoadMushroomDataFromMySQL();

            cmbComport.Items.Clear();
            var portName = System.IO.Ports.SerialPort.GetPortNames();
            cmbComport.Items.AddRange(portName);
            cmbComport.SelectedIndex = cmbComport.Items.Count - 1;

            cmbBoardlate.Items.Clear();
            cmbBoardlate.Items.Add("9600");
            cmbBoardlate.Items.Add("115200");
            cmbBoardlate.SelectedIndex = 0;

            //mysql humidata 가져오기위한것 
            Thread thread = new Thread(new ThreadStart(LoadhumidataFromMySQL));
            thread.Start();

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

        private void button2_Click(object sender, EventArgs e)
        {
            Form2 videoform = new Form2();
            videoform.StartPosition = FormStartPosition.Manual;
            videoform.Location = new Point(220, 100);
            videoform.BringToFront();
            videoform.ShowDialog();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form Programinfo = new DataSet();
            Programinfo.ShowDialog();
        }


    }
}