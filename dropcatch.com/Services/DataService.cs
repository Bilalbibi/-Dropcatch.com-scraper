using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using dropcatch.com.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Packaging.Ionic.Zlib;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;

namespace dropcatch.com.Services
{
    public class DataService
    {
        public void ExportNewlyAddedBids(List<Bid> bids)
        {
            var date = DateTime.Now.ToString("yyyy_MM_dd_HH_mm");
            AppStates.NewBidsExportPath = $@"Dropped doamins from dropcatch.com\NewBids_{date}.xlsx";
            var row = 2;
            var uniqueUsers = new HashSet<string>();
            using (var excelPackage = new ExcelPackage(new FileInfo(AppStates.NewBidsExportPath)))
            {
                #region Create Excel and Write headers

                var sheet = excelPackage.Workbook.Worksheets.Add("New Seo User Bids");
                sheet.Protection.AllowSelectLockedCells = false;
                sheet.Row(1).Height = 20;
                sheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Row(1).Style.Font.Bold = true;
                sheet.Row(1).Style.Font.Size = 8;

                sheet.Cells[1, 1].Value = "Seo User";
                sheet.Cells[1, 2].Value = "Domain Name";
                sheet.Cells[1, 3].Value = "DA";
                sheet.Cells[1, 4].Value = "Links";
                sheet.Cells[1, 5].Value = "Registered Date";
                sheet.Cells[1, 6].Value = "Type";
                sheet.Cells[1, 7].Value = "Current price";
                sheet.Cells[1, 8].Value = "Auction Date";
                sheet.Cells[1, 9].Value = "Auction Link";
                sheet.Cells[1, 10].Value = "No. of Pages Indexed";
                sheet.Cells[1, 11].Value = "Index Status";

                var count = bids.Count(x => x.Domain.Da > 20);
                var range = sheet.Cells[$"A1:K{count + 1}"];
                var tab = sheet.Tables.Add(range, "bids");

                #endregion

                foreach (var bid in bids)
                {
                    if (!uniqueUsers.Contains(bid.UserName))
                    {
                        sheet.Cells[row, 1].Value = bid.UserName;
                        uniqueUsers.Add(bid.UserName);
                    }
                    else
                        sheet.Cells[row, 1].Value = "";

                    if (bid.Domain.Da > 20)
                    {
                        sheet.Cells[row, 2].Value = bid.Domain.DomainName;
                        sheet.Cells[row, 3].Value = bid.Domain.Da;
                        sheet.Cells[row, 4].Value = bid.Domain.Links;
                        sheet.Cells[row, 7].Value = "$" + bid.Amount;
                        sheet.Cells[row, 8].Value = bid.Domain.EndDate.ToString("dd-MMM-yyyy");
                        sheet.Cells[row, 9].Value = $"https://www.dropcatch.com/domain/{bid.Domain.DomainName}";
                        sheet.Cells[row, 10].Value = bid.Domain.GoogleResults;
                        sheet.Cells[row, 11].Value = bid.Domain.GoogleResults > 0 ? "Yes" : "No";
                        if (bid.Domain.RegisteredDate.ToString("MMM-dd-yyyy") == "Jan-01-0001")
                        {
                            sheet.Cells[row, 5].Value = "Recording date not yet available";
                            sheet.Cells[row, 6].Value = "N/A";
                        }
                        else
                        {
                            sheet.Cells[row, 5].Value = bid.Domain.RegisteredDate.ToString("dd-MMM-yyyy"); ;
                            var dateNow = DateTime.Now;
                            sheet.Cells[row, 6].Value =
                                ((dateNow.Year - bid.Domain.RegisteredDate.Year) * 12) + dateNow.Month -
                                bid.Domain.RegisteredDate.Month < 12
                                    ? "Expired"
                                    : "AGED";
                        }

                        row++;
                    }
                }

                sheet.Column(1).AutoFit();
                sheet.Column(2).AutoFit();
                sheet.Column(3).AutoFit();
                sheet.Column(4).AutoFit();
                sheet.Column(5).AutoFit();
                sheet.Column(6).AutoFit();
                sheet.Column(7).AutoFit();
                sheet.Column(8).AutoFit();
                sheet.Column(9).AutoFit();
                sheet.Column(10).AutoFit();
                sheet.Column(11).AutoFit();
                excelPackage.Save();
            }
        }

        public async Task SendEmailAsync()
        {
            using (var mm = new MailMessage())
            {
                var sc = new SmtpClient();
                mm.From = new MailAddress("proupworker@gmail.com", "Lab41");
                //mm.To.Add(new MailAddress("alex@lab41.co"));
                mm.To.Add(new MailAddress("lpro2008@hotmail.fr"));
                mm.IsBodyHtml = true;
                mm.Subject = $"Dropcatch User Bids {DateTime.Now:MM-dd-yyy}";
                mm.BodyEncoding = Encoding.UTF8;
                mm.SubjectEncoding = Encoding.UTF8;
                mm.Attachments.Add(new Attachment(AppStates.NewBidsExportPath));
                var su = new NetworkCredential("proupworker@gmail.com", "bilelbilel23051984");
                sc.EnableSsl = true;
                sc.UseDefaultCredentials = true;
                sc.Host = "smtp.gmail.com";
                sc.Port = 587;
                sc.Credentials = su;
                await sc.SendMailAsync(mm);
            }
        }

        public async Task ExportAllDomains(bool scrapeTfAndSf, int links, int da)
        {
            var domains = AppStates.Domains.Values.OrderByDescending(o => o.Da).ToList();
            var date = DateTime.Now.ToString("yyyy_MM_dd");

            //var path = $@"Dropped doamins from dropcatch.com\Dropcatch.com_{date}.xlsx";
            var path = $@"C:\Users\Administrator\Dropbox\Auctions to Bid\Dropcatch Domains\Dropcatch.com_{date}.xlsx";
            AppStates.AllDomainsExportPath = path;
            var excelPkg = new ExcelPackage(new FileInfo(path));
            var sheet = excelPkg.Workbook.Worksheets.Add("All Domains");

            #region Write headers of sheet1 and 2

            sheet.Protection.IsProtected = false;
            sheet.Protection.AllowSelectLockedCells = false;
            sheet.Row(1).Height = 20;
            sheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(1).Style.Font.Size = 8;

            sheet.Cells[1, 1].Value = "URL";
            sheet.Cells[1, 2].Value = "Domain Name";
            sheet.Cells[1, 3].Value = "Username";
            sheet.Cells[1, 4].Value = "SEO User";
            sheet.Cells[1, 5].Value = "Current price";
            sheet.Cells[1, 6].Value = "End date";
            sheet.Cells[1, 7].Value = "DA";
            sheet.Cells[1, 8].Value = "# PA";
            sheet.Cells[1, 9].Value = "Links";
            sheet.Cells[1, 10].Value = "Equity";
            sheet.Cells[1, 11].Value = "SEO Domain?";


            var sheet2 = excelPkg.Workbook.Worksheets.Add("DomainsToCheck");

            sheet2.Protection.IsProtected = false;
            sheet2.Protection.AllowSelectLockedCells = false;
            sheet2.Row(1).Height = 20;
            sheet2.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet2.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sheet2.Row(1).Style.Font.Bold = true;
            sheet2.Row(1).Style.Font.Size = 8;
            sheet2.Row(1).Style.WrapText = true;


            sheet2.Cells[1, 1].Value = "Domain Name";
            sheet2.Cells[1, 2].Value = "End date";
            sheet2.Cells[1, 3].Value = "DA";
            sheet2.Cells[1, 4].Value = "PA";
            sheet2.Cells[1, 5].Value = "Links";
            sheet2.Cells[1, 6].Value = "Equity";
            sheet2.Cells[1, 7].Value = "TF";
            sheet2.Cells[1, 8].Value = "CF";
            sheet2.Cells[1, 9].Value = "TF/CF Ratio";
            sheet2.Cells[1, 10].Value = "Registered Date";
            sheet2.Cells[1, 11].Value = "Type";
            sheet2.Cells[1, 12].Value = "Last Checked Price";
            sheet2.Cells[1, 13].Value = "SEO Domain?";
            sheet2.Cells[1, 14].Value = "Did a SEO User Bid On This?";
            sheet2.Cells[1, 15].Value = "How Many SEO User's Bidded?";
            sheet2.Cells[1, 16].Value = "Ahrefs Rank";
            sheet2.Cells[1, 17].Value = "Ahrefs DR";
            sheet2.Cells[1, 18].Value = "Ahrefs External Backlinks";
            sheet2.Cells[1, 19].Value = "Ahrefs RD";
            sheet2.Cells[1, 20].Value = "To Review";
            sheet2.Cells[1, 21].Value = "Notes";
            sheet2.Cells[1, 22].Value = "URL";
            sheet2.Cells[1, 23].Value = "To Order";
            sheet2.Cells[1, 24].Value = "Max Price";

            var range = sheet2.Cells[$"A1:X{domains.Count + 1}"];
            var tab = sheet2.Tables.Add(range, "Domains");

            //tab.ShowHeader = true;
            tab.TableStyle = TableStyles.Medium2;
            sheet2.Cells.Style.Font.Size = 8;

            #endregion


            var row = 2;
            var row2 = 2;
            foreach (var domain in domains)
            {
                #region Write to sheet1 domains+bids
                foreach (var domainBid in domain.Bids)
                {


                    sheet.Cells[row, 1].Value = "https://www.dropcatch.com/domain/" + domain.DomainName;
                    sheet.Cells[row, 2].Value = domain.DomainName;
                    sheet.Cells[row, 3].Value = domainBid.UserName;
                    if (AppStates.Users.Contains(domainBid.UserName))
                        sheet.Cells[row, 4].Value = "Yes";

                    sheet.Cells[row, 5].Value = "$" + domainBid.Amount;
                    sheet.Cells[row, 6].Value = domain.EndDate;
                    sheet.Cells[row, 7].Value = domain.Da;
                    sheet.Cells[row, 8].Value = domain.Pa;
                    sheet.Cells[row, 9].Value = domain.Links;
                    sheet.Cells[row, 10].Value = domain.Equity;

                    if (domain.Da > 25)
                        sheet.Cells[row, 11].Value = "Yes";

                    if (AppStates.Users.Contains(domainBid.UserName))
                    {
                        for (int i = 1; i <= 11; i++)
                        {
                            sheet.Cells[row, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[row, i].Style.Fill.BackgroundColor.SetColor(Color.Red);
                        }
                    }
                    row++;
                }

                #endregion
                #region Write to sheet2 domain info

                sheet2.Cells[row2, 1].Value = domain.DomainName;
                sheet2.Cells[row2, 2].Value = domain.EndDate.ToString("dd-MMM-yyyy");
                sheet2.Cells[row2, 3].Value = domain.Da;
                sheet2.Cells[row2, 3].Style.Numberformat.Format = "0";
                sheet2.Cells[row2, 3].Value = Convert.ToDecimal(sheet2.Cells[row2, 3].Value);
                sheet2.Cells[row2, 4].Value = domain.Pa;
                sheet2.Cells[row2, 5].Value = domain.Links;
                sheet2.Cells[row2, 6].Value = domain.Equity;
                if (scrapeTfAndSf)
                {
                    sheet2.Cells[row2, 7].Value = domain.Tf;
                    sheet2.Cells[row2, 8].Value = domain.Cf;
                    sheet2.Cells[row2, 9].Value = domain.Tf / domain.Cf;
                }
                sheet2.Cells[row2, 12].Value = "$" + domain.Bids.Max(x => x.Amount);

                if (domain.Links > links && domain.Da > da)
                    sheet2.Cells[row2, 20].Value = "YES";

                if (domain.Da > 25)
                    sheet2.Cells[row2, 13].Value = "Yes";

                var competitorsBids = domain.Bids.Where(x => AppStates.Users.Contains(x.UserName)).ToList();
                if (competitorsBids.Count > 0)
                {
                    sheet2.Cells[row2, 14].Value = "Yes";
                    sheet2.Cells[row2, 15].Value = competitorsBids.Count;
                    sheet2.Cells[row2, 15].Style.Numberformat.Format = "0";
                    sheet2.Cells[row2, 15].Value = Convert.ToDecimal(sheet2.Cells[row2, 15].Value);
                }
                sheet2.Cells[row2, 22].Value = "https://www.dropcatch.com/domain/" + domain.DomainName;
                if (domain.RegisteredDate.ToString("MMM-dd-yyyy") == "Jan-01-0001")
                {
                    sheet2.Cells[row2, 10].Value = "Not Available";
                    sheet2.Cells[row2, 11].Value = "N/A";
                }
                else
                {
                    sheet2.Cells[row2, 10].Value = domain.RegisteredDate.ToString("MMM-dd-yyyy");
                    var dateNow = DateTime.Now;
                    var type = ((dateNow.Year - domain.RegisteredDate.Year) * 12) + dateNow.Month - domain.RegisteredDate.Month;
                    if (type < 12)
                        sheet2.Cells[row2, 11].Value = "Expired";
                    else
                        sheet2.Cells[row2, 11].Value = "AGED";
                }
                sheet2.Cells[row2, 16].Value = domain.AhrefsRank;
                sheet2.Cells[row2, 17].Value = domain.AhrefsDR;
                sheet2.Cells[row2, 18].Value = domain.AhrefsEB;
                sheet2.Cells[row2, 19].Value = domain.AhrefsRD;
                row2++;

                #endregion
            }

            #region Format cells and columns

            for (var i = 1; i <= 11; i++) sheet.Column(i).AutoFit();
            for (var i = 1; i <= 23; i++) sheet2.Column(i).AutoFit();

            sheet2.Cells["K2:K" + row2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet2.Cells["L2:L" + row2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet2.Cells["Q2:Q" + row2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet2.Cells["M2:M" + row2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet2.Cells["C1:C" + row2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            sheet2.Cells["J1:J" + row2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            sheet2.Cells["A1:X1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sheet2.Cells["A1:X1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


            foreach (var cell in sheet2.Cells["D2:F" + row2])
            {
                cell.Style.Numberformat.Format = "0";
                cell.Value = Convert.ToDecimal(cell.Value);
            }
            foreach (var cell in sheet2.Cells["P2:S" + row2])
            {
                cell.Style.Numberformat.Format = "0";
                cell.Value = Convert.ToDecimal(cell.Value);
            }
            sheet.Cells["A2:X" + row2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            for (int i = 3; i <= 10; i++) sheet.Column(i).Width = 8;

            sheet2.Column(16).Width = 10;
            sheet2.Column(17).Width = 10;
            sheet2.Column(18).Width = 10;
            sheet2.Column(19).Width = 10;
            sheet2.Column(20).Width = 50;

            sheet2.View.FreezePanes(2, 1);
            sheet2.View.FreezePanes(2, 2);

            #endregion

            do
            {
                try
                {
                    excelPkg.Save();
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    MessageBox.Show(@"Please close the excel file to complete the saving: " + path);
                }
            } while (true);

        }

        public List<Domain> ImportDomainsToBid(string path)
        {
            var excelPkg = new ExcelPackage(new FileInfo(path));
            var sheet = excelPkg.Workbook.Worksheets["DomainsToCheck"];

            var domainsToBid = new List<Domain>();
            for (var i = 2; i <= sheet.Dimension.End.Row; i++)
            {
                if (string.IsNullOrEmpty(sheet.Cells[i, 23].Value?.ToString())) continue;
                if (((string)sheet.Cells[i, 22].Value).ToLower() != "yes") continue;
                if (string.IsNullOrEmpty(sheet.Cells[i, 24].Value?.ToString())) continue;
                var maxBid = decimal.Parse(sheet.Cells[i, 24].Value.ToString());
                domainsToBid.Add(new Domain
                {
                    DomainName = sheet.Cells[i, 1].Value.ToString(),
                    MaxBid = maxBid
                });
            }

            return domainsToBid;
        }
    }
}
