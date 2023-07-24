using DAL.CRUD;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace RM_Warehouse.Pages
{
    public class Put_AwayModel : PageModel
    {

        // THIS CLASS IS FOR ORDER MANAGEMENT -> INBOUND ORDER -> PUT AWAY PAGE. 

        public static DataTable? dt_loc_for_item_id { get; set; }
        [BindProperty]
        public static string item_description { get; set; }
        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime? expiry_date { get; set; }

        [BindProperty]
        public int quantity_placed { get; set; }

        [BindProperty]
        public static long order_id { get; set; }
        
        [BindProperty]
        public static long details_id { get; set; }
        
        [BindProperty]
        public static int quantity_left { get; set; }
        [BindProperty]
        public static int item_id { get; set; }
        [BindProperty]
        public static string item_code { get; set; }   
        public string Msg_Put_Away_Form { get; set; }

        [BindProperty]
        public static List<Location_Codes> locationList { get; set; }

        [BindProperty]

        public int location_id_form { get; set; }

        [BindProperty]
        public bool flag_locations { get; set; }
        public static DataTable? dt_loc_all_for_wh { get; set; }
      
        public static DataTable? dt_wh_all { get; set; }
        [BindProperty]
        public bool flag_orders_put_away_form { get; set; }

        [BindProperty]
        public bool flag_orders { get; set; }

        public static DataSet nested_tables { get; set; }

        [BindProperty]

        public static DataTable? dt_orders { get; set; }

        public DataTable dt_items { get; set; }

        public IActionResult OnGet()
        {
            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));

            if (flag_username)
            {
                return RedirectToPage("Index");
            }
            Fill_Orders();
            Fill_Locations();

            return Page();
        }

        // THIS FUNCTION POPULATES NESTED DATATABLES WITH INBOUND RECEIVED ORDERS.ALL ITEMS ARE RECEIVED

        public void Fill_Orders()
        {
            Order_Inbound order = new Order_Inbound();
			string warehouse = HttpContext.Session.GetString("warehouse");
			dt_orders = order.GetOrders(warehouse,"RECEIVED");

            nested_tables = new DataSet();
			if (dt_orders == null)
            {
                flag_orders = true;
                return;
            }
				

			foreach (DataRow row in dt_orders.Rows)
            {
                long order_id = Convert.ToInt64(row["ORDER_ID"]);
                DataTable? temp = order.GetItems(order_id,true);

                if (temp != null)
                {
                    temp.TableName = row["ORDER_ID"].ToString();
                    nested_tables.Tables.Add(temp);
                }
            }
            flag_orders = true;
        }

        // THIS FUNCTION IS INVOKED FROM ORDERS GRID'S ROW 'PUT AWAY' BUTTON.IT OPENS PUT AWAY FORM WITH
        // FILLED VALUES.IT ALSO SHOWS PRESENT LOACTIONS FOR SAME ITEM.

        public IActionResult OnPostPutAway(long order_id_1,long deatils_id_1,string item_code_1,int item_id_1,string expiry_date_1,string item_desc_1)
        {
           
            order_id = order_id_1;
            details_id = deatils_id_1;
            item_code = item_code_1;
            item_id = item_id_1;
            item_description = item_desc_1;

           
            if (expiry_date_1 != null)
                expiry_date = Convert.ToDateTime(expiry_date_1);
           
            Put_Away pa = new Put_Away();

            quantity_left = pa.ItemsLeft(deatils_id_1);

            quantity_placed = quantity_left;

            flag_orders_put_away_form = true;
            flag_locations = true;
            Msg_Put_Away_Form = string.Empty;

            Fill_Locations_For_Item_ID(item_id);

            return Page();
        }

      // THIS FUNCTION POPULATES DROPDOWN LOCATIONS WITH ALL LOCATIONS PRESENT IN CURRENT LOGIN WAREHOUSE.

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

        // THIS FUNCTION DOES SUBMIT FOR PUT AWAY FORM.IT CHECKS USER INPUTS.ON SUCCESS,IT UPDATES DATATBASE WITH
        // SAVE INVENTORY.

        public IActionResult OnPostSubmit()
        {
            if(location_id_form==0)
            {
                Msg_Put_Away_Form = "Please select Location.";
                flag_orders_put_away_form = true;
                flag_locations = true;
                quantity_placed = quantity_left;
                return Page();
            }
            if(quantity_placed>quantity_left)
            {
                Msg_Put_Away_Form = "Quantity Placed cannot be greater than Quanity Left.";
                flag_orders_put_away_form = true;
                flag_locations = true;
                return Page();
            }
            if(quantity_placed<=0)
            {
                Msg_Put_Away_Form = "Quantity Placed cannot be <= 0.";
                flag_orders_put_away_form = true;
                flag_locations = true;
                return Page();
            }

            string user = HttpContext.Session.GetString("username");

            Put_Away pa = new Put_Away();
            
            pa.SaveInventory(details_id,location_id_form, item_id, quantity_placed, expiry_date,user);
            

            // IF ALL ITEMS ARE PLACED, THEN REMOVE IT FROM DISPLAY GRID
            
            Order_Inbound order=new Order_Inbound();

            dt_items = order.GetItems(order_id,true);

            if (dt_items == null) // All items in order are placed,so order status is "CLOSED"
            {
                order.UpdateReceiveOrder(order_id,"CLOSED");
                TempData["ConfirmationMessage"] = string.Format("All Items in Order ID:{0} are Placed ", order_id);
            }

            return Redirect("Put_Away");

        }

        // THIS FUNCTION DOES CANCEL FOR PUT AWAY FORM. 

        public IActionResult OnPostCancel()
        {
            flag_orders_put_away_form = false;
            flag_locations = false;
            return Redirect("Put_Away");
        }

        // THIS FUNCTION POPULATES DATATABLE dt_loc_for_item_id WITH ALL LOCATIONS FOR SAME ITEM PLACED.

        public void Fill_Locations_For_Item_ID(int item_id)
        {
            string warehouse = HttpContext.Session.GetString("warehouse");
            Put_Away put_Away = new Put_Away();
            
            dt_loc_for_item_id= put_Away.Get_Locations_For_Item_ID(item_id, warehouse);
        }
    }
}

