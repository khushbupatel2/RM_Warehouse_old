using DAL.CRUD;
using DAL.Sequence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Numerics;
using static ClosedXML.Excel.XLPredefinedFormat;

namespace RM_Warehouse.Pages
{

    // THIS CLASS IS FOR LOCATION MASTER PAGE.

    public class Location_MasterModel : PageModel
    {
        [BindProperty]
        public static DataTable? locations { get; set; }

        [BindProperty]
        [ValidateNever]
        public static List<Location_Codes> locationList { get; set; }

        [BindProperty]

        public int location_id { get; set; }
        [BindProperty]
        public bool Is_Default_Location { get; set; }
        [BindProperty]
        public static string location_code_1 { get; set; }

        
        [BindProperty]
        public static string warehouse_name { get; set; }
        [BindProperty]
        public string location_code { get; set; }
        public string Msg { get; set; }
        [BindProperty]
        public static int location_id_edit { get; set; }

        [BindProperty]
        public bool flag_location_form { get; set; }
        [BindProperty]
        public bool flag_locations { get; set; }
        
        [BindProperty]
        public bool flag_items { get; set; }
        public static DataTable? dt_wh_all { get; set; }
        public static DataTable? dt_loc_all_for_wh { get; set; }
        public static DataTable? dt_items_all_for_location { get; set; }
        public static DataTable? dt_items_all { get; set; }
        public IActionResult OnGet()
        {
            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));

            if (flag_username)
            {
                return RedirectToPage("Index");
            }

        
            OnGetShow_Locations();
            Fill_LocationsList();

            return Page();
        }

        // THIS FUNCTION POPULATES DATATABLE dt_loc_all_for_wh WITH ALL LOCATIONS PRESENT IN LOGIN WAREHOUSE.

        public void OnGetShow_Locations()
        {
            Inhand_Inventory in_inv = new Inhand_Inventory();
            warehouse_name = HttpContext.Session.GetString("warehouse");

			dt_loc_all_for_wh = in_inv.GetAll_Locations_for_Warehouse(warehouse_name);
           
            flag_locations = true;

        }

        // THIS FUNCTION POPULATES DATATABLE dt_items_all_for_location WITH ALL ITEMS PRESENT AT THIS LOACTION.


        public IActionResult OnGetShow_Items(int location_id,string location_code)
        {
            Inhand_Inventory in_inv = new Inhand_Inventory();
            dt_items_all_for_location = in_inv.GetAll_Items_for_Location(location_id);
            location_code_1 = location_code;
            flag_locations = true;
            flag_items = true;
            return Page();
        }

        // THIS FUNCTION OPENS LOCATION ENTRY FORM WITH CURRENT LOCATION VALUES.

        public IActionResult OnPostEdit_Location(int location_id_1,string location_code_1)
        {
            flag_location_form = true;
            flag_locations = true;

            location_id_edit=location_id_1;
            location_code = location_code_1;


            return Page();
        }

        // THIS FUNCTION RESETS LOCATION ENTERY FORM VALUES AND OPENS THIS FORM.

        public IActionResult OnPostAdd_Location()
        {
            Reset_Form();
            flag_location_form = true;
            flag_locations=true;

            return Page();
        }

        // THIS FUNCTION RESETS LOCATION ENTRY FORM FIELDS WITH EMPTY VALUES.

        public void Reset_Form()
        {
            location_code = string.Empty;

            location_id_edit = 0; 


        }

        // THIS FUNCTION DOES CANCEL ACTION FOR LOCATION ENTRY FORM BY RESETTING FIELD VALUES AND HIDING
        // THIS FORM.


        public IActionResult OnPostCancel()
        {
            Reset_Form();
            flag_location_form = false;
            flag_locations = true;
            return Page();
        }

        // THIS FUNCTION DOES SUBMIT FOR LOCATION ENTRY FORM.
        // THIS DOES BOTH INSERT NEW RECORD OR UPDATE CURRENT RECORD
        // BASED ON VALUE OF location_id_edit VARIABLE.
        // IF location_id_edit==0, THEN INSERT NEW RECORD
        // ELSE,UPDATE CURRENT RECORD.


        public IActionResult OnPostSubmit()
        {
            if (!Check_Input())
                return Page();

            string current_user = HttpContext.Session.GetString("username");
			string current_warehouse = HttpContext.Session.GetString("warehouse");
            
            Warehouse wh=new Warehouse();
            int current_warehouse_id = wh.GetWarehouse_ID(current_warehouse);

			Inhand_Inventory in_inv = new Inhand_Inventory();

            if (location_id_edit != 0)
                in_inv.Update_Location(location_id_edit, location_code, current_warehouse_id, current_user.ToUpper(), System.DateTime.Now);
            else
            {
                location_id_edit=in_inv.Insert_Location(location_code, current_warehouse_id, current_user.ToUpper(), System.DateTime.Now);
            }
            // If Set As Default Location CHECBOX is CHECKED

            if(Is_Default_Location)
            {
                wh.Change_Default_Location(current_warehouse, location_id_edit);
            }

            OnGetShow_Locations();

            // reseting the form

            Reset_Form();

            flag_location_form = false;


            return Page();
        }

        // THIS FUNCTION CHECKS USER INPUT FROM LOCATION ENTRY FORM
        // 1.location_code VARIABLE IS CHECKED FOR NULL OR EMPTY VALUE.
        // IF true,THEN RETURN false.
        // ELSE,RETURN true.

        public bool Check_Input()
        {
            flag_location_form = true;
            flag_locations = true;
            if (string.IsNullOrEmpty(location_code))
            {
                Msg = "Please enter Location Code.";
                return false;
            }

            return true;
        }

        // THIS FUNCTION POPULATES SEARCHABLE DROPDOWN FOR LOCATION CODES.

        public void Fill_LocationsList()
        {
            Inhand_Inventory inhand_Inventory=new Inhand_Inventory();
            warehouse_name = HttpContext.Session.GetString("warehouse");

            locations = inhand_Inventory.GetAll_Locations_for_Warehouse(warehouse_name);
            locationList = new List<Location_Codes>();
            if (locations == null)
                return;
            for (int i = 0; i < locations.Rows.Count; i++)
            {
                Location_Codes location_1 = new();
                location_1.Location_ID = Convert.ToInt32(locations.Rows[i]["Location_ID"]);
                location_1.Location_Code = locations.Rows[i]["Location_Code"].ToString();
                locationList.Add(location_1);
            }

            
        }

        // THIS FUNCTION POPULTAES DATATABLE dt_items_all_for_location WITH ALL ITEMS PRESENT AT THIS 
        // LOCATION.

        public IActionResult OnPostSearch_Location()
        {
            Inhand_Inventory in_inv = new Inhand_Inventory();
            dt_items_all_for_location = in_inv.GetAll_Items_for_Location(location_id);
            location_code_1 = location_code;

            flag_items = true;
            return Page();
        }

    }
}
