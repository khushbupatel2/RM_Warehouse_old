using DAL.CRUD;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Emit;

namespace RM_Warehouse.Pages
{
    // THIS CLASS IS FOR WAREHOUSE MANAGEMENT -> WAREHOUSE MASTER PAGE.
    public class Warehouse_MasterModel : PageModel
    {
        [BindProperty]
        public string po_abbrivation { get; set; }
        [BindProperty]
        public bool flag_entry_form { get; set; }
        [BindProperty]
        public string Label { get; set; }
        public string Msg { get; set; }
        public static DataTable? dt_wh_all { get; set; }

        [BindProperty]

        public int warehouse_id { get; set; }

        [BindProperty]

        public string name { get; set; }

        [BindProperty]

        public string address1 { get; set; }

        [BindProperty]

        public string address2 { get; set; }

        [BindProperty]

        public string city { get; set; }

        [BindProperty]

        public string state_province { get; set; }
        [BindProperty]

        public string postal_code { get; set; }
        [BindProperty]

        public string country { get; set; }

        [BindProperty]

        public string email { get; set; }
        [BindProperty]

        public string fax { get; set; }

        [BindProperty]

        public string phone { get; set; }

        [BindProperty]

        public bool is_active { get; set; }
        [BindProperty]

        public int default_receiving_location_id { get; set; }

        public IActionResult OnGet()
        {

            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));

            if (flag_username)
            {
                return RedirectToPage("Index");
            }

            Label = "Create New Warehouse";
            Fill_Warehouse();

            return Page();
        }


        // THIS FUNCTION POPULATES DATATABLE dt_wh_all WITH ALL WAREHOUSES PRESENT IN DATABASE.

        public void Fill_Warehouse()
        {
            Warehouse wh = new Warehouse();
            dt_wh_all = wh.GetAll();
        }

        // THIS FUNCTION DOES SUBMIT FOR WAREHOUSE ENTRY FORM.IT INSERT/UPDATE WAREHOUSE RECORD DEPENDING ON LABEL
        // VALUE.if (Label == "Create New Warehouse"),THEN INSERT.ELSE UPDATE WAREHOUSE RECORD.

        public IActionResult OnPostSubmit()
        {
            if (!Check_Input())
                return Page();

            Warehouse wh = new Warehouse();

            if (Label == "Create New Warehouse")
            {
                wh.CreateRecord(name, address1, address2, city, state_province, postal_code, country, email, fax, is_active, default_receiving_location_id, phone,po_abbrivation);
            }
            else
            {
                wh.UpdateRecord(warehouse_id, name, address1, address2, city, state_province, postal_code, country, email, fax, is_active, default_receiving_location_id, phone, po_abbrivation);
            }


            // reseting the form

            Reset_Form();

            Fill_Warehouse();

            flag_entry_form = false;

            return Page();
        }

        // THIS FUNCTION CHECKS USER INPUTS FOR WAREHOUSE ENTRY FORM.IT SHOWS ERROR MESSAGES FOR WRONG INPUTS.
        // IT RETURNS true FOR SUCCESS,ELSE RETURNS false.

        public bool Check_Input()
        {
            flag_entry_form = true;
            if (string.IsNullOrEmpty(name))
            {
                Msg = "Please enter Warehouse Name.";
                return false;
            }
            if (string.IsNullOrEmpty(address1))
            {
                Msg = "Please enter Address1.";
                return false;
            }
            if (string.IsNullOrEmpty(city))
            {
                Msg = "Please enter City.";
                return false;
            }
            if (string.IsNullOrEmpty(state_province))
            {
                Msg = "Please enter State Province.";
                return false;
            }
            if (string.IsNullOrEmpty(postal_code))
            {
                Msg = "Please enter Postal Code.";
                return false;
            }
            if (string.IsNullOrEmpty(country))
            {
                Msg = "Please enter Country.";
                return false;
            }
            if (string.IsNullOrEmpty(phone))
            {
                Msg = "Please enter Phone.";
                return false;
            }
            
            if (string.IsNullOrEmpty(email))
            {
                Msg = "Please enter Email.";
                return false;
            }

            if (string.IsNullOrEmpty(po_abbrivation))
            {
                Msg = "Please enter PO Abbrivation.";
                return false;
            }

            return true;
        }

        // THIS FUNCTION IS INVOKED FROM WAREHOUSES GRID'S ROW Edit BUTTON.IT FILLS WAREHOUSE ENTRY FORM WITH
        // VALUES FOUND IN DATABASE.


        public IActionResult OnPostEdit(int id)
        {
            Warehouse wh = new Warehouse();

            DataRow dr_by_warehouse_id = wh.Get_By_Warehouse_ID(id);

            warehouse_id = id;
            name = dr_by_warehouse_id["Name"].ToString();
            address1 = dr_by_warehouse_id["Address1"].ToString();
            address2 = dr_by_warehouse_id["Address2"].ToString();
            city = dr_by_warehouse_id["City"].ToString();
            state_province = dr_by_warehouse_id["State_Province"].ToString();
            postal_code = dr_by_warehouse_id["Postal_Code"].ToString();
            country = dr_by_warehouse_id["Country"].ToString();
            email = dr_by_warehouse_id["Email"].ToString();
            phone = dr_by_warehouse_id["Phone"].ToString();
            fax = dr_by_warehouse_id["Fax"].ToString();
            is_active = Convert.ToBoolean(dr_by_warehouse_id["Is_Active"]);
            if (dr_by_warehouse_id["Default_Receiving_Location_Id"] != DBNull.Value)
                default_receiving_location_id = Convert.ToInt32(dr_by_warehouse_id["Default_Receiving_Location_Id"]);
            else
                default_receiving_location_id = 0;
            po_abbrivation = dr_by_warehouse_id["PO_ABBRIVATION"].ToString();
            Label = "Update Warehouse";

            flag_entry_form = true;

            return Page();
        }

        // THIS FUNCTION OPENS WAREHOUSE ENTRY FORM WITH RESET VALUES FOR NEW RECORD.

        public IActionResult OnPostAdd_Warehouse()
        {
            Reset_Form();
            flag_entry_form = true;
            return Page();
        }

        // THIS FUNCTION DOES RESET FOR WAREHOUSE ENTRY FORM.

        public void Reset_Form()
        {
            name = string.Empty;
            address1 = string.Empty;
            address2 = string.Empty;
            city = string.Empty;
            state_province = string.Empty;
            postal_code = string.Empty;
            country = string.Empty;
            email = string.Empty;
            phone = string.Empty;
            fax = string.Empty;
            is_active = false;
            default_receiving_location_id = 0;
            po_abbrivation = string.Empty;
            ModelState.Clear();
            Label = "Create New Warehouse";
        }

        // THIS FUNCTION DOES CANCEL FOR WAREHOUSE ENTRY FORM.


        public IActionResult OnPostCancel()
        {
            Reset_Form();
            flag_entry_form = false;
            return Page();
        }
    }
}
