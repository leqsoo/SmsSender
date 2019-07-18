using Inetlab.SMPP;
using Inetlab.SMPP.Common;
using Inetlab.SMPP.Logging;
using Inetlab.SMPP.PDU;
using Microsoft.Office.Interop.Excel;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SmsSender
{
    class ExcelReader
    {
        [Obsolete]
        public async Task ReadFromExcelAsync()
        {
            Application xlApp;
            Workbook xlWorkBook;
            Worksheet xlWorkSheet;
            Range range;
            string filePath = ConfigurationManager.AppSettings.Get("Path");

            string phoneNumber;
            string smsText;
            int rowNumber;
            int columnNumber;
            int rowCount;
            int columnCount;

            xlApp = new Application();
            xlWorkBook = xlApp.Workbooks.Open(filePath, 0, true, 5, "", "", true, XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Worksheet)xlWorkBook.Worksheets.get_Item(1);

            range = xlWorkSheet.UsedRange;
            rowCount = range.Rows.Count;
            columnCount = range.Columns.Count;
            //var webClient = new WebClient();

            for (rowNumber = 2; rowNumber <= rowCount; rowNumber++)
            {
                for (columnNumber = 1; columnNumber <= columnCount; columnNumber++)
                {
                    phoneNumber = Convert.ToString((range.Cells[rowNumber, columnNumber] as Range).Value2 ?? "");
                    smsText = Convert.ToString((range.Cells[rowNumber, ++columnNumber] as Range).Value2 ?? "");
                    await SendSms(phoneNumber, smsText);
                    //webClient.DownloadString($"http://10.10.50.5:16000/pls/sms/phttp2sms.Process?src=20100&dst={phoneNumber}&txt={smsText}");
                    //MessageBox.Show(phoneNumber + smsText);
                }
            }

            xlWorkBook.Close(true, null, null);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorkSheet);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);
        }

        [Obsolete]
        public static async Task SendSms(string phoneNumber, string smsText)
        {

            string filePath = ConfigurationManager.AppSettings.Get("SMPPLogPath");
            LogManager.SetLoggerFactory(name => new FileLogger(filePath, LogLevel.All));
            using (SmppClient client = new SmppClient())
            {
                try
                {
                    if (await client.Connect(new DnsEndPoint("smpp.server", 7777, AddressFamily.InterNetwork)))
                    {
                        BindResp bindResp = await client.Bind("username", "password");

                        if (bindResp.Header.Status == CommandStatus.ESME_ROK)
                        {
                            var submitResp = await client.Submit(
                                SMS.ForSubmit()
                                    .From("short code")
                                    .To(phoneNumber)
                                    .Coding(DataCodings.UCS2)
                                    .Text(smsText));

                            if (submitResp.All(x => x.Header.Status == CommandStatus.ESME_ROK))
                            {
                                client.Logger.Info("Message has been sent.");
                            }
                        }

                        await client.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    client.Logger.Error("Failed send message", ex);
                }
            }
        }
    }
}
