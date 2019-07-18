using System;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;

namespace SmsSender
{
    public partial class smsSenderForm : Form
    {
        public smsSenderForm()
        {
            InitializeComponent();
        }

        private void Send_Click(object sender, EventArgs e)
        {
            ExcelReader excelReader = new ExcelReader();
            excelReader.ReadFromExcelAsync();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            int x = 0;
            var client = new WebClient();
            while (x <= 100)
            {
                var content = client.DownloadString("http://example.com");
                MessageBox.Show(content);
                x++;
            }
            stopwatch.Stop();
            MessageBox.Show(stopwatch.Elapsed.ToString());
        }
    }
}
