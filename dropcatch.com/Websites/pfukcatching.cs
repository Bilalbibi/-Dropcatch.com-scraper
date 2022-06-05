using System.Threading.Tasks;
using dropcatch.com.Models;

namespace dropcatch.com.Websites
{
    public class pfukcatching : IWebsite
    {
        public HttpCallerDroppedDoaminsAdder HttpCallerDroppedDoaminsAdder = new HttpCallerDroppedDoaminsAdder();
        public MainForm mainForm = new MainForm();
        public async Task Bid(string domain)
        {
            var login = await HttpCallerDroppedDoaminsAdder.GetHtml("https://www.pfukcatching.co.uk/db/login_check.php?username=alexlab41&password=Snr9s*7rH^6B");
            if (login.error != null)
            {
                return;
            }

            var backOrder = await HttpCallerDroppedDoaminsAdder.GetHtml($"https://www.pfukcatching.co.uk/db/showTable.php?mode=save_new&id=[object%20HTMLInputElement]&domain={domain}&dropDate=&dropping=1&caught=0&caughtAt=1974-01-01");//somethingjewish.co.uk
            if (backOrder.error != null)
            {
                mainForm.ErrorLog($"backorder {domain} to pfukcatching.co.uk => {backOrder.error}");
                return;
            }
            if (backOrder.html.Contains("is already booked"))
            {
                mainForm.NormalLog($"{domain} is already booked on pfukcatching.co.uk");
                return;

            }
            mainForm.SuccessLog($"{domain} backordered successfully to pfukcatching.co.uk");
        }
    }
}
