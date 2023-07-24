using DAL.CRUD;
using DocumentFormat.OpenXml.Office.PowerPoint.Y2021.M06.Main;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace RM_Warehouse.Pages
{
    // THIS CLASS IS FOR ADMIN PAGES - PAGE
    public class Admin_PagesModel : PageModel
    {
        [BindProperty]
        public bool Chk_Submit_to_Accounts_Dept { get; set; }
        [BindProperty]
        public string page_name { get; set; }
        public static DataTable? dt_pages_all { get; set; }

        [BindProperty]
        [ValidateNever]
        public static List<Page_Names_Only> pageList { get; set; }

        [BindProperty]
        public bool flag_new_page { get; set; }

        [BindProperty]
        public string UserName { get; set; }
        public static DataTable? dt_my_pages { get; set; }

        public string Msg { get; set; }


        public IActionResult OnGet()
        {

            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));

            if (flag_username)
            {
                return RedirectToPage("Index");
            }
           // THIS CODE CLEARS USER'S PAGES TO NULL ON PAGE LOAD

            dt_my_pages = null;
            return Page();
        }
        // BELOW FUNCTION ACCESSES USER'S GRANTED PAGES FROM DATABASE TABLE AND
        // POPULATE dt_my_pages DATATBLE WITH THESE RECORDS.

        public IActionResult OnPostFindMyPages()
        {
            if (UserName == null)
            {
                Msg = "Please Give AD UserName.";
                return Page();
            }

            User_Pages up = new User_Pages();
            dt_my_pages = up.FindMyPages(UserName);

            if (dt_my_pages == null)
            {
                Msg = "User is not assigned to any Page.";
            }
            FillPagesList();

            return Page();
        }
        // THIS FUNCTION FILLS PAGE LIST DROPDOWN WITH UNASSIGNED PAGES FOR THE USER
        public void FillPagesList()
        {
            User_Pages up = new User_Pages();
            dt_pages_all = up.GetAll_Unassigned_Pages(UserName);

            pageList = new List<Page_Names_Only>();

            if (dt_pages_all == null)
            {
                Page_Names_Only page = new();
                page.Page_Name = string.Empty;
                pageList.Add(page);
                return;
            }
                
            for (int i = 0; i < dt_pages_all.Rows.Count; i++)
            {
                Page_Names_Only page = new();
                page.Page_Name = dt_pages_all.Rows[i]["Page_Name"].ToString();
                pageList.Add(page);
            }

        }
        // THIS FUNCTION CHECKS USER INPUTS AND INSERTS PAGES,USER INTO DATABASE TABLE 
        public IActionResult OnPostSubmitNewPage()
        {
            if (UserName == null)
            {
                Msg = "Please Give AD UserName.";
                flag_new_page = true;
                return Page();
            }
            if (page_name == null)
            {
                Msg = "Please Select Page";
                flag_new_page = true;
                return Page();
            }

            User_Pages up = new User_Pages();

            string insert_status = up.GiveAccess(page_name, UserName);

            if (insert_status != "OK")
            {
                Msg = insert_status;
                flag_new_page = true;
                return Page();
            }
            
            // UPDATE DATATABLE

            dt_my_pages = up.FindMyPages(UserName);
            FillPagesList();
            return Page();
        }
        // THIS FUNCTION DOES CANCEL FUNCTIONALITY FOR flag_new_page - FORM
        public IActionResult OnPostCancel()
        {
            return Page();
        }
        // THIS FUNCTION DELETES GRANTED PAGE FOR USER FROM DATABASE TABLE 
        public IActionResult OnPostDelete(int id_1)
        {
            User_Pages up = new User_Pages();
            up.DeleteAccess(id_1);

            // UPDATE DATATABLE

            dt_my_pages = up.FindMyPages(UserName);

            FillPagesList();
            
            return Page();
        }
        // THIS FUNCTION OPENS flag_new_page FORM AND CALLS USER'S UNASSIGNED PAGES LIST DROPDOWN
        // FUNCTION
        public IActionResult OnPostAdd_New_Record()
        {
            flag_new_page = true;
            page_name = null;
            User_Pages up = new User_Pages();
            
            if (UserName == null)
            {
                Msg = "Please Give AD UserName.";
                flag_new_page = false;
                return Page();
            }
            FillPagesList();
            Chk_Submit_to_Accounts_Dept = up.FindAccountsAccess(UserName);
            return Page();
        }
        // THIS FUNCTION GIVE OR REMOVE "SUBMIT_TO_ACCOUNTS_DEPT" TAB ACCESS FROM PO INVOICE PROCESS PAGE.
        // THIS FUNCTION SETS OR GETS Chk_Submit_to_Accounts_Dept CHECKBOX VALUE
        // ON BASE OF THIS CHECKBOX VALUE,IT UPDATES DATABASE TABLE.
        public IActionResult OnPostGiveAccountsAccess()
        {
            if (UserName == null)
            {
                Msg = "Please Give AD UserName.";
                flag_new_page = true;
                return Page();
            }

            string insert_status;
            User_Pages up = new User_Pages();

            if (Chk_Submit_to_Accounts_Dept)
            {
                insert_status = up.GiveAccess("SUBMIT_TO_ACCOUNTS_DEPT", UserName);
                if (insert_status != "OK")
                {
                    Msg = insert_status;
                    flag_new_page = true;
                    return Page();
                }
            }
            else
            {
                up.DeleteAccess("SUBMIT_TO_ACCOUNTS_DEPT", UserName);
            }

            // UPDATE DATATABLE

            dt_my_pages = up.FindMyPages(UserName);
            FillPagesList();
            return Page();
        }
    }
}
