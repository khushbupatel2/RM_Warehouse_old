using ClosedXML.Excel;
using DAL.CRUD;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace RM_Warehouse.Pages
{

    // THIS CLASS IS FOR REPORTS -> PICK HISTORY PAGE.
    public class Pick_HistoryModel : PageModel
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
        public static DataTable? dt_pick_logs { get; set; }
        public IActionResult OnGet()
        {
            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));

            if (flag_username)
            {
                return RedirectToPage("Index");
            }
            dt_pick_logs = null;

            DateTime dt = DateTime.Now.AddMonths(-1);
            string s2 = dt.ToString("yyyy-MM-ddTHH:mm:ss");
            From_Date = DateTime.Parse(s2);

            DateTime dt_1 = DateTime.Now;
            string s2_1 = dt_1.ToString("yyyy-MM-ddTHH:mm:ss");
            To_Date = DateTime.Parse(s2_1);

            return Page();
        }

        // THIS FUNCTION SHOWS ALL PICK LOGS.

        public IActionResult OnPostShow_All_Logs()
        {
            Pick pa = new Pick();
            string warehouse = HttpContext.Session.GetString("warehouse");

			dt_pick_logs = pa.All_Pick_Logs(warehouse);

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

        // THIS FUNCTION IS CALLED FROM SEARCH LOGS BUTTON.IT CHECKS USER INPUTS FOR Search_Criteria RADIO
        // BUTTONS AND Search_Value TEXTBOX INPUTS.THIS SHOWS RECORDS WITH DATE INTERVALS AND SEARCHED
        // INPUTS.

        public IActionResult OnPostSearch()
        {
            if (Search_Criteria == null)
            {
                Msg_Search = "Please select Serach Criteria.";
                dt_pick_logs = null;
                return Page();
            }
            if (Search_Value == null)
            {
                Msg_Search = "Please give Search Value";
                dt_pick_logs = null;
                return Page();
            }

			string warehouse = HttpContext.Session.GetString("warehouse");
			Pick pa = new Pick();
            dt_pick_logs = pa.Search(warehouse,Search_Criteria, Search_Value, From_Date, To_Date);

            return Page();
        }

        // THIS FUNCTION EXPORTS DATATABLE dt_pick_logs TO EXCEL FILE.THIS FILE IS DOWNLOADED TO CLIENT
        // BROWSER FROM SERVER.

        public IActionResult OnPostExportToExcel()
        {
            string file_name = "Pick_History_" + From_Date.ToShortDateString() + "_" + To_Date.ToShortDateString() + ".xlsx";
            using (XLWorkbook wb = new())
            {

                wb.Worksheets.Add(dt_pick_logs, "Report_1");
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file_name);
                }
            }
        }
    }
}
