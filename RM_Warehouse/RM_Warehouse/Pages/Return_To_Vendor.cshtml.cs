using DAL.CRUD;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace RM_Warehouse.Pages
{
    // THIS CLASS IS FOR RETURN ITEMS -> RETURN TO VENDOR PAGE.

    public class Return_To_VendorModel : PageModel
    {
        [BindProperty]
        public bool flag_items { get; set; }
        [BindProperty]
        public static DataTable? dt_InBound_Details { get; set; }
        [BindProperty]
        [ValidateNever]
        public static List<Item_Codes_Description> itemList { get; set; }
        [BindProperty]
        public int item_id_search { get; set; }
        [BindProperty]
        public static DataTable? items { get; set; }
        [BindProperty]
        public static int Item_ID { get; set; }
        [BindProperty]
        public static DataTable? dt_locations { get; set; }
        
        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime? Expiry_Date { get; set; }
        [BindProperty]
        public int Qty_In_Hand { get; set; }
        [BindProperty]
        public int Location_Id { get; set; }
        [BindProperty]
        public int Return_Quantity { get; set; }
        [BindProperty]
        public int Item_ID_1 { get; set; }
        [BindProperty]
        public int Details_Id { get; set; }
        [BindProperty]
        public int Inv_Id { get; set; }
        [BindProperty]
        public List<int> AreChecked { get; set; }
        [BindProperty]
        public bool flag_locations { get; set; }

        [BindProperty]
        public string Msg_Item_Return_Form { get; set; }

        [BindProperty]
        public static string PONumber { get; set; }
        
        [BindProperty]
        public static long Order_ID { get; set; }
        [BindProperty]
        public static string Item_Code { get; set; }
        [BindProperty]
        public static string Item_Desc { get; set; }
        [BindProperty]
        public static int Ordered_Quantity { get; set; }
        [BindProperty]
        public static int Received_Quantity { get; set; }
        [BindProperty]
        public static decimal Cost { get; set; }
        [BindProperty]
        public static string Currency { get; set; }


        [BindProperty]
        public bool flag_item_return_form { get; set; }
        [BindProperty]
        public static long details_id { get; set; }
        public static DataSet nested_tables { get; set; }

        [BindProperty]
        public static DataTable? dt_orders { get; set; }
        
        [BindProperty]
        public bool flag_closed_order_items { get; set; }
        
        public IActionResult OnGet()
        {

            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));
            
            if (flag_username)
            {
                return RedirectToPage("Index");
            }
            Fill_ItemList();
       
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

        // THIS FUNCTION IS INVOKED FROM ORDER DETAILS GRID'ROW ReturnItemToVendor BUTTON.IT OPENS ITEM RETURN FORM
        // WITH FILLED VALUES.IT ALSO POPULATES LOCATIONS GRID WITH ALL LOCATIONS FOR SAME ITEM. 

        public IActionResult OnPostReturnItemToVendor(long id_1,long order_id_1,string item_code_1,string item_desc_1,int ordered_quantity_1,int received_quantity_1,decimal cost_1,string currency_1,string ponumber_1,int item_id_1)
        {
            details_id = id_1;
            Order_ID = order_id_1;
            Item_Code = item_code_1;
            Item_Desc = item_desc_1;
            Ordered_Quantity = ordered_quantity_1;
            Received_Quantity= received_quantity_1;
            Cost = cost_1;
            Currency = currency_1;
            PONumber = ponumber_1;
            Item_ID = item_id_1;

            Return_Items rt_items=new Return_Items();

            string warehouse = HttpContext.Session.GetString("warehouse");
   
            dt_locations = rt_items.Get_Available_Items_and_Locations(Order_ID, Item_ID, warehouse);

            flag_item_return_form = true;
            flag_locations = true;
            Msg_Item_Return_Form = string.Empty;

            return Page();
        }

        // THIS FUNCTION DOES SUBMIT FOR ITEM RETURN FORM.IT ACCEPTS INPUT LIKE RETURN_QUANTITY.IT PROCESSES CHECKED
        // ROWS ONLY.IT ALSO PROMPTS ERROR MESSAGES.

        public IActionResult OnPostReturnItems(IFormCollection form)
        {
            string user = HttpContext.Session.GetString("username");

            //          
            string temp_inv_id = form["Inv_Id"];
            string[] temp_inv_ids = temp_inv_id.Split(',');

            string temp_return_qty = form["Return_Quantity"];
            string[] temp_return_qtys = temp_return_qty.Split(',');

            string temp_qty_in_hand = form["Qty_In_Hand"];
            string[] temp_qty_in_hands = temp_qty_in_hand.Split(',');

            string temp_details_id = form["Details_Id"];
            string[] temp_details_ids = temp_details_id.Split(',');

            string temp_expiry_date = form["Expiry_Date"];
            string[] temp_expiry_dates = temp_expiry_date.Split(',');

            string temp_location_id = form["Location_Id"];
            string[] temp_location_ids = temp_location_id.Split(',');

            string temp_item_id = form["Item_ID_1"];
            string[] temp_item_ids = temp_item_id.Split(',');

            Return_Items rt_items=new Return_Items();

            // CHECKING RETURN QUANTITY AGAINST QUANTITY IN HAND  FOR EACH ROW

            int count_return_qty = 0;
    
            foreach (var item in AreChecked)
            {
                int index = Array.IndexOf(temp_inv_ids, item.ToString());

                int var_return_qty = Convert.ToInt32(temp_return_qtys[index]);
                int var_qty_in_hand = Convert.ToInt32(temp_qty_in_hands[index]);

                if (var_return_qty > var_qty_in_hand)
                {
                    Msg_Item_Return_Form = "Return Quantity can't be greater than Quantity In Hand.Please Give smaller value.";
                    flag_item_return_form = true;
                    flag_locations = true;
                    return Page();
                }
                if (var_return_qty == 0)
                {
                    Msg_Item_Return_Form = "Please give Return Quantity.";
                    flag_item_return_form = true;
                    flag_locations = true;
                    return Page();
                }
                count_return_qty += var_return_qty;
            }

            // CHECKING TOTAL RETURN QUANTITY AGAINST RECEIVED QUANTITY

            if (count_return_qty > Received_Quantity)
            {
                Msg_Item_Return_Form = "Total Return Quantity can't be greater than Received Quantity for this order.Please Give smaller value.";
                flag_item_return_form = true;
                flag_locations = true;
                return Page();
            }

            foreach (var item in AreChecked)
            {
                int index = Array.IndexOf(temp_inv_ids, item.ToString());

                int var_return_qty = Convert.ToInt32(temp_return_qtys[index]);

                long var_details_id = Convert.ToInt64(temp_details_ids[index]);
                int var_location_id = Convert.ToInt32(temp_location_ids[index]);
                int var_item_id = Convert.ToInt32(temp_item_ids[index]);

                DateTime? var_expiry_date;
                if (temp_expiry_dates[index] != "null")
                    var_expiry_date = Convert.ToDateTime(temp_expiry_dates[index]);
                else
                    var_expiry_date = null;

                rt_items.RemoveInventory(item, var_details_id, var_location_id, var_item_id, var_return_qty, var_expiry_date, user);
            }

            // RETURNING TO VENDOR
            
            string warehouse = HttpContext.Session.GetString("warehouse");
            rt_items.ReturnToVendor(Order_ID, DateTime.Now, PONumber, user, warehouse, Item_ID, Cost, Currency, count_return_qty,Details_Id);

            TempData["ConfirmationMessage"] = "Success:Item is Returned To Vendor";
            return Redirect("Return_To_Vendor");
        }

        // THIS FUNCTION SEARCHES CLOSED INBOUND ORDER DETAILS FOR SELECTED ITEM_CODE FROM DROPDOWN.IT POPULATES
        // DATATABLE dt_InBound_Details WITH SEARCHED RECORDS.

        public IActionResult OnPostSearch_Inbound_Order_Details()
        {
            if (item_id_search == 0)
            {
                return Page();
            }

            string warehouse = HttpContext.Session.GetString("warehouse");

            Return_Items return_Items = new Return_Items();
            dt_InBound_Details = return_Items.GetInboundOrderDetailsByItemID(item_id_search, warehouse);

            flag_items = true;
            return Page();
        }
    }
}
