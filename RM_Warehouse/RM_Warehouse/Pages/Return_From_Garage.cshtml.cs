using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using DAL.CRUD;

namespace RM_Warehouse.Pages
{

    // THIS CLASS IS FOR RETURN ITEMS -> RETURN FROM GARAGE PAGE.


    public class Return_From_GarageModel : PageModel
    {
        [BindProperty]
        public static string rm_ponumber { get; set; }
        [BindProperty]
        public bool flag_items { get; set; }
        
        [BindProperty]
        public static DataTable? locations { get; set; }
        [BindProperty]
        [ValidateNever]
        public static List<Location_Codes> locationList { get; set; }

        [BindProperty]
        public int location_id { get; set; }
        [BindProperty]
        public static int Picked_Quantity { get; set; }
        [BindProperty]
        public int Return_Quantity { get; set; }
        [BindProperty]
        public static string item_code { get; set; }
        [BindProperty]
        public static string Currency { get; set; }
        [BindProperty]
        public static decimal item_price { get; set; }
        [BindProperty]
        public static int item_id { get; set; }
        [BindProperty]
        public static long details_id { get; set; }
        [BindProperty]
        public static long Order_ID { get; set; }
        [BindProperty]
        public static int Ordered_Quantity { get; set; }
        [BindProperty]
        public string Msg_Item_Return_Form { get; set; }
        [BindProperty]
        public bool flag_item_return_form { get; set; }
        [BindProperty]
        [ValidateNever]
        public static List<Item_Codes_Description> itemList { get; set; }
        [BindProperty]
        public int item_id_search { get; set; }
        [BindProperty]
        public static DataTable? items { get; set; }

        [BindProperty]
        public static DataTable? dt_OutBound_Details { get; set; }

        public IActionResult OnGet()
        {

            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));

            if (flag_username)
            {
                return RedirectToPage("Index");
            }
            Fill_ItemList();
            Fill_LocationsList();
           
            return Page();
        }
      
        // THIS FUNCTION POPULATES DROPDOWN ITEM_CODES WITH ALL ITEMS PRESENT IN DATABASE.

        public void Fill_ItemList()
        {
            string warehouse = HttpContext.Session.GetString("warehouse");

            Item item = new Item();
            items = item.GetAll(warehouse);

            itemList = new List<Item_Codes_Description>();
            if (items == null)
                return;
            for (int i = 0; i < items.Rows.Count; i++)
            {
                Item_Codes_Description item_1 = new();
                item_1.Item_ID = Convert.ToInt32(items.Rows[i]["Item_ID"]);
                item_1.Item_Code_Description = items.Rows[i]["Item_Code"].ToString() + "-" + items.Rows[i]["Item_Desc"].ToString();
                itemList.Add(item_1);
            }

            
        }

        // THIS FUNCTION POPULATES DROPDOWN LOCATION_CODES WITH ALL LOCATIONS PRESENT IN CURRENT LOGIN WAREHOUSE.

        public void Fill_LocationsList()
        {
            Inhand_Inventory inhand_Inventory = new Inhand_Inventory();
            string warehouse_name = HttpContext.Session.GetString("warehouse");

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

        // THIS FUNCTION SEARCHES AND SHOWS OUTBOUND ORDER DETAILS WITH SELECTED ITEM_CODE FROM DROPDOWN.

        public IActionResult OnPostSearch_Oubound_Order_Details()
        {
            if (item_id_search == 0)
            {
                return Page();
            }

            string warehouse = HttpContext.Session.GetString("warehouse");

            Return_Items return_Items = new Return_Items();
            dt_OutBound_Details=return_Items.GetOutboundOrderDetailsByItemID(item_id_search,warehouse);

            flag_items = true;
            return Page();
        }

        // THIS FUNCTION IS INVOKED FROM ORDER DETAILS GRID'S ROW ReturnItem BUTTON.IT OPENS ITEM RETURN FORM WITH
        // FILLED VALUES.

        public IActionResult OnPostReturnItem(long id_1, long order_id_1, int item_id_1,string item_code_1, int ordered_quantity_1,int picked_quantity_1, decimal price_1, string currency_1,string ponumber_1)
        {
            flag_item_return_form = true;

            Ordered_Quantity = ordered_quantity_1;
            Picked_Quantity = picked_quantity_1;
            Order_ID = order_id_1;
            details_id = id_1;
            item_id = item_id_1;
            item_code = item_code_1;
            item_price = price_1;
            Currency = currency_1;
            rm_ponumber = ponumber_1;

            flag_item_return_form = true;
            flag_items = true;
            return Page();
        }

        // THIS FUNCTION DOES SUBMIT FOR ITEM RETURN FORM.IT CHECKS USER INPUTS.IT UPDATES DATABASE.IT ALERTS MESSAGE
        // REGARDING ITEM RETURNED SUCCESSFULLY.

        public IActionResult OnPostSubmitReturnItem()
        {
            Return_Items rt_item = new Return_Items();

            string user = HttpContext.Session.GetString("username");
            string warehouse = HttpContext.Session.GetString("warehouse");

            if (Return_Quantity==0)
            {
                Msg_Item_Return_Form = "Please Give Return Quantity";
                flag_item_return_form = true;
                flag_items = true;
                return Page();
            }

            if (Return_Quantity > Picked_Quantity)
            {
                Msg_Item_Return_Form = "Return Quantity can't be greater than Picked Quantity";
                flag_item_return_form = true;
                flag_items = true;
                return Page();
            }

            if (location_id == 0)
            {
                Msg_Item_Return_Form = "Please select Location Code.";
                flag_item_return_form = true;
                flag_items = true;
                return Page();
            }

            
            rt_item.UpdateReturnItemFromGarage(Order_ID,details_id,Return_Quantity,user,location_id,item_id,warehouse,item_price,Currency,rm_ponumber);

            TempData["ConfirmationMessage"] = string.Format("Item is Returned Successfully!");

            Return_Items return_Items = new Return_Items();
            dt_OutBound_Details = return_Items.GetOutboundOrderDetailsByItemID(item_id_search, warehouse);

            flag_items = true;

            return Page();
            
        }

        // THIS FUNCTION DOES CANACEL FOR ITEM RETURN FORM.

        public IActionResult OnPostCancelReturnItem()
        {
            flag_items = true;
            return Page();
        }

    }
}
