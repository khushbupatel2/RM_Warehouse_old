using DAL.CRUD;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;

namespace RM_Warehouse.Pages
{

    // THIS CLASS IS FOR MOVE INVENTORY PAGE.

    public class Move_InventoryModel : PageModel
    {
        [BindProperty]
        public static int prev_location_id { get; set; }
        [BindProperty]
        public static string item_code { get; set; }
        [BindProperty]
        public bool flag_move_inventory_form { get; set; }
        [BindProperty]
        public int quantity_placed { get; set; }
        [BindProperty]
        public static List<Location_Codes> locationList { get; set; }
        [BindProperty]
        public int location_id_form { get; set; }
        [BindProperty]
        public static int quantity_present { get; set; }
        [BindProperty]
        public static string item_description { get; set; }
        [BindProperty]
        [DataType(DataType.Date)]
        public static DateTime? expiry_date { get; set; }
        [BindProperty]
        public static int item_id { get; set; }
        public string Msg_Move_Inventory_Form { get; set; }
        [BindProperty]
        static DataTable? locations { get; set; }

        [BindProperty]

        public int location_id { get; set; }
      
        [BindProperty]
        public static string location_code_1 { get; set; }

        [BindProperty]
        public static string warehouse_name { get; set; }
        [BindProperty]
        public string location_code { get; set; }
        public string Msg { get; set; }
        [BindProperty]
        public bool flag_search { get; set; }

        [BindProperty]
        public bool flag_locations { get; set; }

        [BindProperty]
        public bool flag_items { get; set; }
        
        public static DataTable? dt_loc_all_for_wh { get; set; }
        public static DataTable? dt_items_all_for_location { get; set; }
        
        public IActionResult OnGet()
        {
            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));

            if (flag_username)
            {
                return RedirectToPage("Index");
            }

            
            OnGetShow_Locations();
            Fill_Locations();
            flag_search = true;
            return Page();
        }

        // THIS FUNCTION POPULATES DATATABLE dt_loc_all_for_wh WITH ALL LOCATIONS PRESENT IN LOGIN WAREHOUSE.

        public void OnGetShow_Locations()
        {
            Inhand_Inventory in_inv = new Inhand_Inventory();
            warehouse_name = HttpContext.Session.GetString("warehouse");

            dt_loc_all_for_wh = in_inv.GetAll_Locations_for_Warehouse(warehouse_name);

            flag_locations = true;
            flag_search = true;
        
        }

        // THIS FUNCTION IS CALLED FROM LOCATION MASTER GRID ON HYPERLINK LOCATION_CODE.
        // THIS SHOWS ALL ITEMS PRESENT AT CURRENT LOCATION WITH ITEM_CODE,ITEM_DESC,QTY_IN_HAND AND EXPIRY DATE.

        public IActionResult OnGetShow_Items(int location_id, string location_code)
        {
            Inhand_Inventory in_inv = new Inhand_Inventory();
            dt_items_all_for_location = in_inv.GetAll_Items_for_Location(location_id);
            prev_location_id = location_id;
            location_code_1 = location_code;
            flag_locations = true;
            flag_items = true;
            flag_search = true;
            return Page();
        }

        // THIS FUNCTION SHOWS ALL ITEMS PRESENT AT SEARCHED LOCATION WITH ITEM_CODE,ITEM_DESC,QTY_IN_HAND
        // AND EXPIRY DATE.

        public IActionResult OnPostSearch_Location()
        {
            Inhand_Inventory in_inv = new Inhand_Inventory();
            dt_items_all_for_location = in_inv.GetAll_Items_for_Location(location_id);
            location_code_1 = location_code;
            prev_location_id=location_id;
            //       flag_locations = true;
            flag_items = true;
            flag_search = true;
            return Page();
        }

        // THIS FUNCTION IS CALLED FROM ITEMS AT LOCATION GRID ON MOVE INVENTORY ICON.
        // THIS OPENS MOVE INVENTORY FORM WITH ALL FILLED VALUES.

        public IActionResult OnPostMove(int item_id_1,string item_code_1,string item_desc_1,int qty_in_hand_1,DateTime? expiry_date_1)
        {
            flag_move_inventory_form = true;
            item_id = item_id_1;
            item_code = item_code_1;
            item_description = item_desc_1;
            quantity_present = qty_in_hand_1;

            expiry_date=expiry_date_1;
            
            return Page();
        }

        // THIS FUNCTION POPULTAES DROPDOWN LOCATION_CODES WITH ALL LOCATIONS PRESENT IN CURRENT WAREHOUSE.


        public void Fill_Locations()
        {
            string warehouse = HttpContext.Session.GetString("warehouse");
            Inhand_Inventory in_inv = new Inhand_Inventory();
            dt_loc_all_for_wh = in_inv.GetAll_Locations_for_Warehouse(warehouse);

            // creating location list and populating it with datatable dt.

            locationList = new List<Location_Codes>();
            for (int i = 0; i < dt_loc_all_for_wh.Rows.Count; i++)
            {
                Location_Codes location = new();
                location.Location_ID = Convert.ToInt32(dt_loc_all_for_wh.Rows[i]["Location_ID"]);
                location.Location_Code = dt_loc_all_for_wh.Rows[i]["Location_Code"].ToString();
                locationList.Add(location);
            }

        
        }

        // THIS FUNCTION DOES SUBMIT ACTION OF MOVE INVENTORY FORM.
        // THIS CHECKS USER INPUTS LIKE LOCATION_CODE AND QUANTITY_PLACED.
        // AFTER SUBMIT COMPLETE,IT RE-DIRECTS TO SAME PAGE.

        public IActionResult OnPostSubmitMove()
        {
            if (location_id_form == 0)
            {
                Msg_Move_Inventory_Form = "Please select Location.";
                flag_move_inventory_form = true;
        //        flag_locations = true;
                quantity_placed = quantity_present;
                return Page();
            }
            if (quantity_placed > quantity_present)
            {
                Msg_Move_Inventory_Form = "Quantity Placed cannot be greater than Quanity Present.";
                flag_move_inventory_form = true;
          //      flag_locations = true;
                return Page();
            }
            if (quantity_placed <= 0)
            {
                Msg_Move_Inventory_Form = "Quantity Placed cannot be <= 0.";
                flag_move_inventory_form = true;
            //    flag_locations = true;
                return Page();
            }

            string user = HttpContext.Session.GetString("username");

            Inhand_Inventory inv_in = new Inhand_Inventory();

            inv_in.Move_Inventory(prev_location_id,location_id_form, item_id, quantity_placed, expiry_date);

            return Redirect("Move_Inventory");

        }

        // THIS FUNCTION DOES CANCEL ACTION OF MOVE INVENTORY FORM.IT HIDES THIS FORM AND SHOWS LOCATIONS GRID AND
        // SEARCH FORM.IT ALSO, REDIRECTS TO SAME PAGE.

        public IActionResult OnPostCancelMove()
        {
            flag_move_inventory_form = false;
            flag_locations = true;
            flag_search = true;
            return Redirect("Move_Inventory");
        }
    }
}
