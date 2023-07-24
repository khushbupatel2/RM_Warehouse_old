using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DAL.CRUD;
using System.Data;
using ClosedXML.Excel;

namespace RM_Warehouse.Pages
{

    // THIS CLASS IS FOR REPORTS -> PUT AWAY HISTORY PAGE.
    public class Put_Away_HistoryModel : PageModel
    {
        [BindProperty]
        public DateTime From_Date { get; set; }
        [BindProperty]
        public DateTime To_Date { get; set; }
        [BindProperty]
        public string Search_Value { get; set; }
        public string Msg_Search { get; set; }

        [BindProperty]
        public string Search_Criteria { get; set; }
        [BindProperty]
        public static DataTable? dt_put_away_logs { get; set; }
        public IActionResult OnGet()
        {
            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));

            if (flag_username)
            {
                return RedirectToPage("Index");
            }
            dt_put_away_logs = null;

            DateTime dt = DateTime.Now.AddMonths(-1);
            string s2 = dt.ToString("yyyy-MM-ddTHH:mm:ss");
            From_Date = DateTime.Parse(s2);

            DateTime dt_1 = DateTime.Now;
            string s2_1 = dt_1.ToString("yyyy-MM-ddTHH:mm:ss");
            To_Date = DateTime.Parse(s2_1);

            return Page();
        }

        // THIS FUNCTION POPULATES DATATABLE dt_put_away_logs WITH ALL PUT AWAY LOGS PRESENT IN DATABASE. 

        public IActionResult OnPostShow_All_Logs()
        {
            Put_Away pa = new Put_Away();

			string warehouse = HttpContext.Session.GetString("warehouse");
			dt_put_away_logs = pa.All_Put_Away_Logs(warehouse);

     //       int count = dt_put_away_logs.Rows.Count;

            Search_Criteria = null;
            Search_Value = null;


            DateTime dt = DateTime.Now.AddMonths(-1);
            string s2 = dt.ToString("yyyy-MM-ddTHH:mm:ss");
            From_Date = DateTime.Parse(s2);

            DateTime dt_1 = DateTime.Now;
            string s2_1 = dt_1.ToString("yyyy-MM-ddTHH:mm:ss");
            To_Date = DateTime.Parse(s2_1);

            ModelState.Clear();

            return Page();

        }

        // THIS FUNCTION DOES SEARCH INTO PUT AWAY LOGS WITH INPUTS LIKE Search_Criteria,Search_Value,From_Date,
        // To_Date.IT ALSO CHECKS USER INPUTS.IT POPULATES DATATABLE dt_put_away_logs WITH SEARCHED RECORDS.

        public IActionResult OnPostSearch()
        {
            if (Search_Criteria == null)
            {
                Msg_Search = "Please select Serach Criteria.";
                dt_put_away_logs = null;
                return Page();
            }
            if (Search_Value == null)
            {
                Msg_Search = "Please give Search Value";
                dt_put_away_logs = null;
                return Page();
            }

            Put_Away pa=new Put_Away();
			string warehouse = HttpContext.Session.GetString("warehouse");
			dt_put_away_logs = pa.Search(warehouse,Search_Criteria, Search_Value,From_Date,To_Date);

            return Page();
        }

        // THIS FUNCTION DOES EXPORT TO EXCEL FILE FOR DATATABLE dt_put_away_logs.

        public IActionResult OnPostExportToExcel()
        {
            string file_name = "Put_Away_History_" + From_Date.ToShortDateString() + "_" + To_Date.ToShortDateString() + ".xlsx";
            using (XLWorkbook wb = new())
            {
                
                wb.Worksheets.Add(dt_put_away_logs, "Report_1");
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",file_name);
                }
            }
        }
    }
}
