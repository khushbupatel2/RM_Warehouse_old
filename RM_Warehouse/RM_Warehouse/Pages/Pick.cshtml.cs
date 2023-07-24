using DAL.CRUD;
using DAL.Stored_Procedures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace RM_Warehouse.Pages
{

    // THIS CLASS IS FOR ORDER MANAGEMENT -> OUTBOUND ORDER -> PICK PAGE

    public class PickModel : PageModel
    {
        [BindProperty]
        public static string item_desc { get; set; }
        [BindProperty]
        public int Qty_In_Hand { get; set; }
        public DataTable? dt_items { get; set; }

        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime? Expiry_Date { get; set; }
        [BindProperty]
        public int Item_Id { get; set; }
        [BindProperty]
        public int Location_Id { get; set; }
        [BindProperty]
        public int Picked_Quantity { get; set; }

        [BindProperty]
        public int Inv_Id { get; set; }
        [BindProperty]
        public int Details_Id { get; set; }
        [BindProperty]
        public List<int> AreChecked { get; set; }

        [BindProperty]
        public static List<Location_Codes> locationList { get; set; }
        [BindProperty]
        public int location_id_form { get; set; }
        [BindProperty]
        public bool flag_locations { get; set; }
        [BindProperty]
        public static int quantity_to_be_picked { get; set; }
        [BindProperty]
        public static int quantity_available { get; set; }
        [BindProperty]
        public static long item_id { get; set; }
        [BindProperty]
        public static string item_code { get; set; }
        [BindProperty]
        public bool flag_pick_form { get; set; }
        public DataSet nested_tables { get; set; }

        [BindProperty]
        public static long details_id { get; set; }
        [BindProperty]
        public bool flag_orders { get; set; }
        [BindProperty]

        public static DataTable? dt_orders { get; set; }
        [BindProperty]

        public static DataTable? dt_locations { get; set; }

        [BindProperty]
        public string Msg_Pick_Form { get; set; }

       [BindProperty]

        public DateTime Order_Date { get; set; }

        [BindProperty]
        public string PONumber { get; set; }

        [BindProperty]

        public static long Order_ID { get; set; }
        
        public IActionResult OnGet()
        {
            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));

            if (flag_username)
            {
                return RedirectToPage("Index");
            }
            Fill_Orders();
            return Page();
        }

        // THIS FUNCTION DOES RESET FOR PICK FORM.

        public void Reset_Pick_Form()
        {
            Order_ID = 0;
            details_id = 0;
            item_id = 0;
            item_code = string.Empty;
            quantity_to_be_picked = 0;
            quantity_available = 0;
            
            ModelState.Clear();

        }

        // THIS FUNCTION DOES CANCEL FOR PICK FORM.

        public IActionResult OnPostCancel()
        {
            flag_pick_form = false;
            flag_orders = true;

            Reset_Pick_Form();
            Fill_Orders();
            return Page();
        }

        // THIS FUNCTION POPULTES OUTBOUND ORDERS GRID WITH ORDER STAUS='OPEN'.
        // NESTED ITEMS DETAILS ARE WITH IS_RECEIVED=false FIELD.

        public void Fill_Orders()
        {
            Order_Outbound order = new Order_Outbound();
			string warehouse = HttpContext.Session.GetString("warehouse");
			dt_orders = order.GetOrders(warehouse);

            nested_tables = new DataSet();
            if (dt_orders == null)
            {
                flag_orders = true;
                return;
            }

            foreach (DataRow row in dt_orders.Rows)
            {
                long order_id = Convert.ToInt64(row["ORDER_ID"]);
                DataTable? temp = order.GetItems(order_id);

                if (temp != null)
                {
                    temp.TableName = row["ORDER_ID"].ToString();
                    nested_tables.Tables.Add(temp);
                }
            }
            flag_orders = true;

        }

        // THIS FUNCTION IS CALLED FROM ORDERS GRID'S ROW PICK BUTTON.IT OPENS PICK FORM WITH PASSED VALUES TO THIS
        // FUNCTION.ALSO,IT UPADTES ORDER DETAILS IF quantity_to_be_picked<=0,THEN MARK IT AS IS_RECEIVED=1.
        // ALSO,CHECKS ITEMS FOR THIS ORDER,IF ALL ITEMS ARE RECEIVED ,THEN MARK ORDER_STATUS='PICK' FOR THIS ORDER.
        // ALSO,SHOWS ALERT MESSAGE REGARDING "PICK ORDER".
        // THIS FUNCTION FILLS quantity_available FIELD FROM DATABASE.ALSO,POPULATES DATATABLE dt_locations WITH
        // ITEMS AND LOCATIONS.

        public IActionResult OnPostPickItem(long id_1, long order_id_1, string item_code_1,int item_id_1, int ordered_quantity_1, int picked_quantity_1,string item_desc_1)
        {
            flag_pick_form = true;

            quantity_to_be_picked = ordered_quantity_1-picked_quantity_1;
            Order_ID = order_id_1;
            details_id = id_1;
            item_code = item_code_1;
            item_id = item_id_1;
            item_desc = item_desc_1;

            Pick pick = new Pick();

            if(quantity_to_be_picked<=0)
            {
                pick.ReceivedItem(details_id);

				Order_Outbound order = new Order_Outbound();
				dt_items = order.GetItems(Order_ID);

				if (dt_items == null) // All items in order are picked,so order status is "PICK"
				{
					order.UpdatePickOrder(Order_ID);
                    TempData["ConfirmationMessage"] = string.Format("All Items for Order ID:{0} are Picked", Order_ID);
                }

				return Redirect("Pick");
            }

            
            string warehouse = HttpContext.Session.GetString("warehouse");
            quantity_available=pick.ItemsAvailable(item_id, warehouse);

            dt_locations = pick.Get_Available_Items_and_Locations(Order_ID, item_id, warehouse);

            flag_pick_form = true;
            flag_locations = true;
            Msg_Pick_Form = string.Empty;

            return Page();
        }

        // THIS FUNCTION IS CALLED FROM LOCATIONS GRID.IT IS PRESENT AT BOTTOM ROW OF THIS GRID.IT ACCEPTS IFormCollection
        // FOR THE FIELDS PRESENT IN THIS FORM GRID.

        public IActionResult OnPostPickOrder(IFormCollection form)
        {
            string user = HttpContext.Session.GetString("username");

  //          
            string temp_inv_id = form["Inv_Id"];
            string[] temp_inv_ids = temp_inv_id.Split(',');

            string temp_pick_qty = form["Picked_Quantity"];
            string[] temp_pick_qtys = temp_pick_qty.Split(',');

            string temp_qty_in_hand = form["Qty_In_Hand"];
            string[] temp_qty_in_hands = temp_qty_in_hand.Split(',');

            string temp_details_id = form["Details_Id"];
            string[] temp_details_ids = temp_details_id.Split(',');

            string temp_expiry_date = form["Expiry_Date"];
            string[] temp_expiry_dates = temp_expiry_date.Split(',');

            string temp_location_id = form["Location_Id"];
            string[] temp_location_ids = temp_location_id.Split(',');

            string temp_item_id = form["Item_Id"];
            string[] temp_item_ids = temp_item_id.Split(',');

            Pick pick = new Pick();

            // CHECKING PICKED QUANTITY AGAINST QUANTITY IN HAND FOR EACH CHECKED ROW

            int count_picked_qty = 0;
            foreach (var item in AreChecked)
            {
                int index = Array.IndexOf(temp_inv_ids, item.ToString());

                int var_pick_qty = Convert.ToInt32(temp_pick_qtys[index]);
                int var_qty_in_hand = Convert.ToInt32(temp_qty_in_hands[index]);

                if (var_pick_qty > var_qty_in_hand)
                {
                    Msg_Pick_Form = "Picked Quantity can't be greater than Quantity In Hand.Please Give smaller value.";
                    flag_pick_form = true;
                    flag_locations = true;
                    return Page();
                }
                count_picked_qty += var_pick_qty;
            }

            // CHECKING TOTAL PICKED QUANTITY AGAINST QUANTITY TO BE PICKED

            if (count_picked_qty > quantity_to_be_picked)
            {
                Msg_Pick_Form = "Total Picked Quantity can't be greater than Quantity to be Picked.Please Give smaller value.";
                flag_pick_form = true;
                flag_locations = true;
                return Page();
            }

            // UPADTING INVENTORY PRESENT IN DATABASE BY CALLING Pick.RemoveInventory FUNCTION FOR EACH CHECKED ROW.

            foreach (var item in AreChecked)
            {
                int index = Array.IndexOf(temp_inv_ids, item.ToString());

                int var_pick_qty = Convert.ToInt32(temp_pick_qtys[index]);
            
                long var_details_id = Convert.ToInt64(temp_details_ids[index]);
                int var_location_id = Convert.ToInt32(temp_location_ids[index]);
                int var_item_id = Convert.ToInt32(temp_item_ids[index]);

                DateTime? var_expiry_date;
                if (temp_expiry_dates[index]!="null")
                    var_expiry_date = Convert.ToDateTime(temp_expiry_dates[index]);
                else
                    var_expiry_date = null;

                pick.RemoveInventory(item, var_details_id, var_location_id, var_item_id, var_pick_qty, var_expiry_date, user);
            }
            Order_Outbound order = new Order_Outbound();
            dt_items = order.GetItems(Order_ID);

            if (dt_items == null) // All items in order are received,so order status is "PICK"
            {
                order.UpdatePickOrder(Order_ID);
                TempData["ConfirmationMessage"] = string.Format("All Items for Order ID:{0} are Picked", Order_ID);
            }

            return Redirect("Pick");
        }
    }
}
