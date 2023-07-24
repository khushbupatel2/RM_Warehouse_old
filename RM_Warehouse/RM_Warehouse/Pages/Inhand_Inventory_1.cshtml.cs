using DAL.CRUD;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace RM_Warehouse.Pages
{
    public class Inhand_Inventory_1Model : PageModel
    {

        [BindProperty]
        public bool Is_Default_Location { get; set; }
        [BindProperty]
        public static string location_code_1 { get; set; }

        [BindProperty]
        public static int location_id { get; set; }


        //[BindProperty]
        //public static List<Warehouse_Names> warehouseList { get; set; }

        //[BindProperty, Range(1, int.MaxValue, ErrorMessage = "Please select Warehouse Name. ")]

        //public int warehouse_id_form { get; set; }
        //[BindProperty]
        //public static int warehouse_id { get; set; }
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

            //     FillWarehouseList_form();
            OnGetShow_Locations();


			return Page();
        }
        
        public void OnGetShow_Locations()
        {
            Inhand_Inventory in_inv = new Inhand_Inventory();
            warehouse_name = HttpContext.Session.GetString("warehouse");

			dt_loc_all_for_wh = in_inv.GetAll_Locations_for_Warehouse(warehouse_name);
           
            flag_locations = true;
//            return Page();
        }

        public IActionResult OnGetShow_Items(int location_id,string location_code)
        {
            Inhand_Inventory in_inv = new Inhand_Inventory();
            dt_items_all_for_location = in_inv.GetAll_Items_for_Location(location_id);
            location_code_1 = location_code;
            flag_locations = true;
            flag_items = true;
            return Page();
        }

        public IActionResult OnPostEdit_Location(int location_id_1,string location_code_1)
        {
            flag_location_form = true;
            flag_locations = true;

            location_id_edit=location_id_1;
            location_code = location_code_1;
 //           warehouse_id_form = warehouse_id_1;

            return Page();
        }
        public IActionResult OnPostAdd_Location()
        {
            Reset_Form();
            flag_location_form = true;
            flag_locations=true;
//            warehouse_id_form = warehouse_id_1;
            return Page();
        }

        public void Reset_Form()
        {
            location_code = string.Empty;
  //          warehouse_id_form = 0;
            location_id_edit = 0; 


        }

        public IActionResult OnPostCancel()
        {
            Reset_Form();
            flag_location_form = false;
            flag_locations = true;
            return Page();
        }

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
                in_inv.Update_Location(location_id_edit, location_code, current_warehouse_id, current_user.ToUpper(), DateTime.Now);
            else
            {
                location_id_edit=in_inv.Insert_Location(location_code, current_warehouse_id, current_user.ToUpper(), DateTime.Now);
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
   //         flag_locations = false;

            return Page();
        }
        public bool Check_Input()
        {
            flag_location_form = true;
            flag_locations = true;
            if (string.IsNullOrEmpty(location_code))
            {
                Msg = "Please enter Location Code.";
                return false;
            }
            //if (warehouse_id_form==0)
            //{
            //    Msg = "Please select Warehouse.";
            //    return false;
            //}

            return true;
        }

        //public void FillWarehouseList_form()
        //{
            
        //    Inhand_Inventory in_inv = new Inhand_Inventory();
        //    dt_wh_all = in_inv.GetAll_Warehouses();

        //    // creating warehouse list and populating it with datatable dt.

        //    warehouseList = new List<Warehouse_Names>();
        //    for (int i = 0; i < dt_wh_all.Rows.Count; i++)
        //    {
        //        Warehouse_Names warehouse = new();
        //        warehouse.Warehouse_ID = Convert.ToInt32(dt_wh_all.Rows[i]["Warehouse_ID"]);
        //        warehouse.Name = dt_wh_all.Rows[i]["Name"].ToString();
        //        warehouseList.Add(warehouse);
        //    }

        //}

        

    }
}
