using Newtonsoft.Json;
using System.Diagnostics;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {

        private static HttpClient sharedClient = new () { BaseAddress = new Uri("http://worldtimeapi.org/api/timezone/Europe/London") };
        private static readonly object Locker = new object();
        private static int requestCount = 0;
        private static Form1 context;
        private static Stopwatch watch = Stopwatch.StartNew();
        private static int availableRequestsCount = 0;

        public Form1()
        {
            InitializeComponent();

            context = this;
            for (int i = 0; i < 10; i++)
            {
                new Thread(MakeRequests).Start();
            }

            new System.Threading.Timer(state => {
                availableRequestsCount += 2;
            }, null, 0, 1000);
        }

        static void MakeRequests()
        {
            while (true)
            {
                
                lock (Locker)
                {
                    while (!(requestCount <= availableRequestsCount)) { }
                    requestCount++;
                }

                try
                {
                    var rest = sharedClient.GetAsync(sharedClient.BaseAddress).Result;
                    string json = rest.Content.ReadAsStringAsync().Result;
                    dynamic deserialized = JsonConvert.DeserializeObject(json);
                    context.Invoke(() =>
                    {
                        context.label1.Text = deserialized.datetime;
                        context.label2.Text = $"Requests count {requestCount} elapsed time {watch.Elapsed}";
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
