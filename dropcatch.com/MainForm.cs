using dropcatch.com.Models;
using dropcatch.com.Services;
using MetroFramework.Forms;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace dropcatch.com
{
    public partial class MainForm : MetroForm
    {
        public bool LogToUi = true;
        public bool LogToFile = true;
        private readonly string _path = Application.StartupPath;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly DropcatchService _dropcatchService = new DropcatchService();
        private readonly DataService _dataService = new DataService();
        private readonly AutoBidService _autoBidService = new AutoBidService();
        private readonly GoogleService _googleService = new GoogleService();
        public static HttpCaller HttpCaller = new HttpCaller();

        public MainForm()
        {
            InitializeComponent();
            Reporter.OnLog += OnLog;
            Reporter.OnError += OnError;
            Reporter.OnProgress += OnProgress;
        }

        private void OnError(object sender, string e)
        {
            ErrorLog(e);
        }

        private void OnProgress(object sender, (int nbr, int max, string s) e)
        {
            SetProgress(e.nbr * 100 / e.max);
            Display($"{e.s} {e.nbr} / {e.max}");
        }
        private void OnLog(object sender, string e)
        {
            NormalLog(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists("Dropped doamins from dropcatch.com"))
                Directory.CreateDirectory("Dropped doamins from dropcatch.com");
            if (!Directory.Exists("Dropped domains from ukdroplidt.com and domainlore.uk"))
                Directory.CreateDirectory("Dropped domains from ukdroplidt.com and domainlore.uk");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ServicePointManager.DefaultConnectionLimit = 65000;
            Directory.CreateDirectory("data");
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Utility.CreateDb();
            Utility.LoadConfig();
            inputI.Text = _path + @"\input.xlsx";
            Utility.InitCntrl(this);
            _googleService.Init();
            SeoService.Init();
        }
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), @"Unhandled Thread Exception");
        }
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show((e.ExceptionObject as Exception)?.ToString(), @"Unhandled UI Exception");
        }
        #region UIFunctions
        public delegate void WriteToLogD(string s, Color c);
        public void WriteToLog(string s, Color c)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new WriteToLogD(WriteToLog), s, c);
                    return;
                }
                if (LogToUi)
                {
                    if (DebugTt.Lines.Length > 5000)
                    {
                        DebugTt.Text = "";
                    }
                    DebugTt.SelectionStart = DebugTt.Text.Length;
                    DebugTt.SelectionColor = c;
                    DebugTt.AppendText(DateTime.Now.ToString(Utility.SimpleDateFormat) + " : " + s + Environment.NewLine);
                }
                Console.WriteLine(DateTime.Now.ToString(Utility.SimpleDateFormat) + @" : " + s);
                if (LogToFile)
                {
                    File.AppendAllText(_path + "/data/log.txt", DateTime.Now.ToString(Utility.SimpleDateFormat) + @" : " + s + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public void NormalLog(string s)
        {
            WriteToLog(s, Color.Black);
        }
        public void ErrorLog(string s)
        {
            WriteToLog(s, Color.Red);
        }
        public void SuccessLog(string s)
        {
            WriteToLog(s, Color.Green);
        }
        public void CommandLog(string s)
        {
            WriteToLog(s, Color.Blue);
        }

        public delegate void SetProgressD(int x);
        public void SetProgress(int x)
        {
            if (InvokeRequired)
            {
                Invoke(new SetProgressD(SetProgress), x);
                return;
            }
            if ((x <= 100))
            {
                //ProgressB.Value = x;
            }
        }
        public delegate void DisplayD(string s);
        public void Display(string s)
        {
            if (InvokeRequired)
            {
                Invoke(new DisplayD(Display), s);
                return;
            }
            displayT.Text = s;
        }

        #endregion
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _googleService._driver?.Quit();
            SeoService._driver?.Quit();
            Utility.Config = new Dictionary<string, string>();
            Utility.SaveCntrl(this);
            Utility.SaveConfig();
        }
        private void loadInputB_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog { Filter = @"TXT|*.TXT", InitialDirectory = _path };
            if (o.ShowDialog() == DialogResult.OK)
            {
                inputI.Text = o.FileName;
            }
        }
        private void openInputB_Click_1(object sender, EventArgs e)
        {
            try
            {
                Process.Start(inputI.Text);
            }
            catch (Exception ex)
            {
                ErrorLog(ex.ToString());
            }
        }
        private void openOutputB_Click_1(object sender, EventArgs e)
        {
            try
            {
                //Process.Start(outputI.Text);
            }
            catch (Exception ex)
            {
                ErrorLog(ex.ToString());
            }
        }
        private void loadOutputB_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = @"xlsx file|*.xlsx",
                Title = @"Select the output location"
            };
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
                //outputI.Text = saveFileDialog1.FileName;
            }
        }

        async Task WaitTillTheChosenDate()
        {
            do
            {
                var todayDate = DateTime.Now.ToString("MM/dd/yyyy");
                var requiredDate = DateTime.Parse(DateTimePicker.Text).ToString("MM/dd/yyyy");
                if (todayDate == requiredDate)
                    break;
                await Task.Delay(1000 * 60);
            } while (true);
        }

        private async void startB_Click_1(object sender, EventArgs e)
        {
            //await OtherServicesWork();
            //return;
            if (inputI.Text == "")
            {
                Display("Please select the file which contains the users you want to check");
                return;
            }
            await WaitTillTheChosenDate();
            await MainWork();
        }

        private async Task MainWork()
        {

            _ = DropCatchWorkTask();
            _ = OtherServicesWork();
        }

        private async Task DropCatchWorkTask()
        {
            do
            {
                Invoke(new Action(() => NormalLog("new task has been launched on dropcatch.com")));
                var d1 = DateTime.Now.AddDays(3);
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                await DropCatchWork(_cancellationTokenSource.Token);
                var d2 = DateTime.Now;
                var delay = d1 - d2;
                Invoke(new Action(() => Display("Next run will be in 3 days, you can set the domains you want to bid on and their max bid amount")));
                Invoke(new Action(() => NormalLog("Next run will be in 3 days, you can set the domains you want to bid on and their max bid amount")));
                await Task.Delay(delay);
            } while (true);
        }

        private async Task OtherServicesWork()
        {
            await Task.Delay(3000);
            List<string> services = new List<string> { "dropcatch.com.Websites.Dropcatcher"/*, "dropcatch.com.Websites.caught_co_uk", "dropcatch.com.Websites.pfukcatching", "dropcatch.com.Websites.domainjunky", "dropcatch.com.Websites.yay", "dropcatch.com.Websites.Ukbackorder", "dropcatch.com.Websites.Dropped", /*"dropcatch.com.Websites.app_youdot",*//*"dropcatch.com.Websites.Autobackorder", "dropcatch.com.Websites.Dbcatch", "dropcatch.com.Websites.catchdrop",*/ /*"dropcatch.com.Websites.Domaintree", "dropcatch.com.Websites.Catchtiger", "dropcatch.com.Websites.Wizedrop"*/ };
            //List<string> services = new List<string> { /*"dropcatch.com.Websites.Dropcatcher", "dropcatch.com.Websites.caught_co_uk", "dropcatch.com.Websites.pfukcatching", "dropcatch.com.Websites.domainjunky",*/ "dropcatch.com.Websites.yay", "dropcatch.com.Websites.Ukbackorder", "dropcatch.com.Websites.Dropped", /*"dropcatch.com.Websites.app_youdot",*/"dropcatch.com.Websites.Autobackorder", "dropcatch.com.Websites.Dbcatch", "dropcatch.com.Websites.catchdrop", "dropcatch.com.Websites.Domaintree", "dropcatch.com.Websites.Catchtiger", "dropcatch.com.Websites.Wizedrop" };
            do
            {
                var d2 = DateTime.Now.AddDays(1);
                _ = Invoke(new Action(() =>
                      NormalLog(
                          "new scraping task has been launched on Ukdroplist.com and domainlore.uk and upload the dropped domaimns from them to the websites service")));
                var droppedDomains = new List<DroppedDoamins>();
                var ukDropList = await Task.Run(UkdroplistsService.GetDroppedDomainsFromUkdroplists);
                var jsonUkDropList = JsonConvert.SerializeObject(ukDropList, Formatting.Indented);
                File.WriteAllText($@"Dropped domains from ukdroplidt.com and domainlore.uk\UKDropLists.com {DateTime.Now:dd_MM_yyy hh_mm}.txt", jsonUkDropList);

                var domainLoreList = await Task.Run(DomainloreService.GetDroppedDomainsFromDomainlore);
                var jsonDomainloreList = JsonConvert.SerializeObject(domainLoreList, Formatting.Indented);
                File.WriteAllText($@"Dropped domains from ukdroplidt.com and domainlore.uk\DomainLore.uk {DateTime.Now:dd_MM_yyy hh_mm}.txt", jsonDomainloreList);
                if (ukDropList != null)
                {
                    droppedDomains.AddRange(ukDropList);
                }
                droppedDomains.AddRange(domainLoreList);
                //var droppeDomains =
                //    JsonConvert.DeserializeObject<List<DroppedDoamins>>(
                //        File.ReadAllText("UKDropLists.com 20_06_2021 05_09.txt"));//DomainLore.uk 17_06_2021 03_51.txt

                //droppeDomains.AddRange(JsonConvert.DeserializeObject<List<DroppedDoamins>>(
                //    File.ReadAllText("DomainLore.uk 17_06_2021 03_51.txt")));
                foreach (var domain in droppedDomains)
                {
                    var jsonObj = new
                    {
                        source = domain.Source,
                        domain = domain.Name,
                        note = "",
                        type = domain.Type,
                        priority = domain.Priority,
                        catchDay = DateTime.Now.Day,
                        catchMonth = DateTime.Now.Month,
                        catchYear = DateTime.Now.Year,
                    };
                    var json = JsonConvert.SerializeObject(jsonObj);
                    var posDoamins = await HttpCaller.PostJson("http://3.10.171.215:9090/api/ui/domains/add", json);
                }
                foreach (var domain in droppedDomains)
                {
                    foreach (var service in services)
                    {
                        var type = Type.GetType(service);
                        var serviceCalss = (IWebsite)Activator.CreateInstance(type);
                        if (domain.Name.EndsWith(".co.uk") && domain.Da > 40)
                        {
                            await serviceCalss.Bid(domain.Name);
                        }
                    }
                }
                var d1 = DateTime.Now;
                var delay = d2 - d1;
                Invoke(new Action(() => NormalLog($"Required domains added to http://3.10.171.215:7070/domains server and added to the dropped domains serviced too, next run will be {(DateTime.Now + delay):dd/MM/yyy hh:mm}")));
                await Task.Delay(delay);
            } while (true);
        }

        private async Task DropCatchWork(CancellationToken cancellationToken)
        {
            AppStates.Users = File.ReadAllLines(inputI.Text).ToList();
            List<Domain> domains;
            try
            {
                domains = await _dropcatchService.GetDroppedDomains();
                domains = await SeoService.AhrefData(domains);
                //return;
            }
            catch (Exception e)
            {
                ErrorLog($"Failed to get dropped domains {e.Message}");
                return;
            }
            try
            {
                await _googleService.PopulateGoogleResult(domains);
            }
            catch (Exception e)
            {
                ErrorLog($"Failed to get index from google {e.Message}");
            }
            _googleService.Dispose();
            var json = JsonConvert.SerializeObject(domains);
            File.WriteAllText("json.txt", json);
            domains = JsonConvert.DeserializeObject<List<Domain>>(File.ReadAllText("json.txt"));

            foreach (var domain in domains.Where(domain => !AppStates.Domains.ContainsKey(domain.DomainName)))
                AppStates.Domains.Add(domain.DomainName, domain);

            try
            {
                _ = ScanForDomainsThatEndToday(cancellationToken);
                _ = ContinuesScanForNewBids(cancellationToken);
            }
            catch (TaskCanceledException)
            {
                ErrorLog("Task canceled");
            }
            catch (Exception e)
            {
                ErrorLog($"Exception : {e.ToString()}");
            }
        }
        /// <summary>
        /// run each 24 hours
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ScanForDomainsThatEndToday(CancellationToken cancellationToken)
        {
            do
            {
                var y = AppStates.Domains.Values.FirstOrDefault(x => x.EndDate.Date == DateTime.Now.Date);
                var h = y?.EndDate.Hour ?? 19;
                var now = DateTime.Now;
                var endAt = new DateTime(now.Year, now.Month, now.Day, h, 0, 0);
                var midnight = new DateTime(now.Year, now.Month, now.Day, 0, 0, 1).AddDays(1);
                if (now > endAt)
                {
                    var waitTillMidnight = (int)(midnight - now).TotalMilliseconds;
                    await Task.Delay(waitTillMidnight, cancellationToken);
                }
                now = DateTime.Now;
                y = AppStates.Domains.Values.FirstOrDefault(x => x.EndDate.Date == DateTime.Now.Date);
                h = y?.EndDate.Hour ?? 19;
                endAt = new DateTime(now.Year, now.Month, now.Day, h, 0, 0).AddMinutes(-1);
                var waitTill7 = (int)(endAt - now).TotalMilliseconds;
                await Task.Delay(waitTill7, cancellationToken);
                var domainsThatEndToday = AppStates.Domains.Values.Where(x => x.EndDate.Date == DateTime.Now.Date).ToList();

                await BidOnEndedTodayDomain();

                await _dropcatchService.GetDomainBidsForAll(domainsThatEndToday);
                AppStates.EndedDateDomains.AddRange(domainsThatEndToday);
                DisplayEndedDomains();
                DisplayMaxCompetitorsBids();
            } while (true);
        }

        /// <summary>
        /// Check which domains will end today and bid on them on last min
        /// </summary>
        /// <returns></returns>
        async Task BidOnEndedTodayDomain()
        {
            try
            {
                AppStates.DomainsToBid = _dataService.ImportDomainsToBid(AppStates.AllDomainsExportPath).ToDictionary(x => x.DomainName, x => x);
            }
            catch (Exception e)
            {
                ErrorLog($"Failed to parse the excel file : {e.Message}");
                return;
            }
            var domainsToBidToday = AppStates.Domains.Values.Where(x => x.EndDate.Date == DateTime.Now.Date && AppStates.DomainsToBid.ContainsKey(x.DomainName)).ToList();
            if (domainsToBidToday.Count == 0)
            {
                Reporter.Log($"No domains to bid today");
                return;
            }
            Reporter.Log($"We will bid on {domainsToBidToday.Count} domains now");
            try
            {
                await _autoBidService.Login();
            }
            catch (Exception e)
            {
                ErrorLog($"Error on login to dropcatch {e.Message}");
                return;
            }
            for (var i = 0; i < domainsToBidToday.Count; i++)
            {
                var domain = domainsToBidToday[i];
                Reporter.Progress((i + 1), domainsToBidToday.Count, "Bidding on domain ");
                try
                {
                    await _autoBidService.Bid(domain);
                }
                catch (Exception e)
                {
                    ErrorLog($"Error bidding on {domain.DomainName} : {e.Message}");
                }
            }
            Reporter.Log($"completed the bid on {domainsToBidToday.Count} domains");
        }


        private void DisplayEndedDomains()
        {
            EndBidDomainsGrid.Rows.Clear();
            foreach (var endedDateDomain in AppStates.EndedDateDomains)
            {
                var maxBid = endedDateDomain.Bids.OrderByDescending(x => x.Amount).FirstOrDefault();
                if (maxBid == null) continue;
                EndBidDomainsGrid.Rows.Add(endedDateDomain.DomainName, maxBid.UserName, maxBid.Amount);
            }
        }
        private void DisplayMaxCompetitorsBids()
        {
            maxCompetitorsBidsOnEndedDomainsGrid.Rows.Clear();
            foreach (var endedDateDomain in AppStates.EndedDateDomains)
                foreach (var bid in endedDateDomain.Bids.Where(x => AppStates.Users.Contains(x.UserName) && x.IsHighestBid))
                    maxCompetitorsBidsOnEndedDomainsGrid.Rows.Add(bid.UserName, endedDateDomain.DomainName, bid.Amount);

            maxCompetitorsBidsOnEndedDomainsGrid.Sort(maxCompetitorsBidsOnEndedDomainsGrid.Columns[0], ListSortDirection.Descending);
        }

        /// <summary>
        /// Run each 2 hours
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ContinuesScanForNewBids(CancellationToken cancellationToken)
        {
            AppStates.ExportedFirstScan = false;
            do
            {
                await _dropcatchService.GetDomainBidsForAll(AppStates.Domains.Values.ToList());
                var newlyAddedBids = new List<Bid>();
                foreach (var domain in AppStates.Domains)
                {
                    foreach (var bid in domain.Value.Bids.Where(x => x.IsNew && AppStates.Users.Contains(x.UserName) && x.IsHighestBid))
                    {
                        newlyAddedBids.Add(bid);
                    }
                }
                Reporter.Log($"Newly added bids {newlyAddedBids.Count}");
                newlyAddedBids = newlyAddedBids.OrderBy(x => x.UserName).ToList();
                if (newlyAddedBids.Count > 0)
                {
                    _dataService.ExportNewlyAddedBids(newlyAddedBids);
                    await _dataService.SendEmailAsync();
                    Reporter.Log($"Report sent for {newlyAddedBids.Count} new bids");

                }

                if (!AppStates.ExportedFirstScan)
                {
                    if (ScrapeTFAndSF.Checked)
                    {
                        Display("scraping TF and CF data");
                        await SeoService.PopulateTfAndCfData();
                        Display("scraping TF and CF done");
                    }
                    await _dataService.ExportAllDomains(ScrapeTFAndSF.Checked, int.Parse(LinksI.Text), int.Parse(DAI.Text));
                    AppStates.ExportedFirstScan = true;
                    Reporter.Log($"Main Report exported (once each 3 days)");
                }
                Reporter.Log($"Scan for new bids completed, Next run will be at {DateTime.Now.AddHours(2):G}");
                await Task.Delay(1000 * 60 * 60 * 2, cancellationToken);
            } while (true);
        }

        private async void AutoBd_Click(object sender, EventArgs e)
        {
            //await _autoBidService.Login();
            //await _autoBidService.Test();

            AppStates.DomainsToBid = _dataService.ImportDomainsToBid(@"Dropped doamins\ Dropcatch.com_2020_11_05.xlsx").ToDictionary(x => x.DomainName, x => x);
            foreach (var item in AppStates.DomainsToBid)
            {
                Console.WriteLine(item.Value.MaxBid);
            }
        }
    }
}
