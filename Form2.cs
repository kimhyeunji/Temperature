using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {

        private string videoUrl = "http://192.168.0.46:8081/capture"; // ESP32 카메라의 IP 주소
        private bool isStreaming = false;
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!isStreaming)
            {
                isStreaming = true;
                StartStreaming();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (isStreaming)
            {
                isStreaming = false;
                StopStreaming();
            }

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
        }
        private void StartStreaming()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (isStreaming)
                    {
                        // 영상을 가져옵니다.
                        byte[] videoData = await GetVideoDataAsync();

                        if (videoData != null)
                        {
                            // PictureBox에 영상을 표시합니다.
                            using (MemoryStream stream = new MemoryStream(videoData))
                            {
                                Image image = Image.FromStream(stream);
                                pictureBox1.Invoke((MethodInvoker)(() => pictureBox1.Image = image));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("영상 스트리밍 중 오류가 발생했습니다. 에러: " + ex.Message);
                }
            });
        }

        private void StopStreaming()
        {
            pictureBox1.Image = null;
        }

        private async Task<byte[]> GetVideoDataAsync()
        {
            try
            {
                // ESP32 카메라로부터 영상 데이터를 가져옵니다.
                using (WebClient client = new WebClient())
                {
                    return await client.DownloadDataTaskAsync(videoUrl);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("영상 데이터를 가져오는 중 오류가 발생했습니다. 에러: " + ex.Message);
                return null;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
