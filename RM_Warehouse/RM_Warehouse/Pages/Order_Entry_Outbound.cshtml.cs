using DAL.CRUD;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using System.Data;
using DAL.Stored_Procedures;

namespace RM_Warehouse.Pages
{

    // THIS CLASS IS USED FOR ORDER MANAGEMENT -> OUTBOUND ORDER -> ORDER ENTRY PAGE.
    public class Order_Entry_OutboundModel : PageModel
    {
        [BindProperty]
        public string Currency { get; set; }
        [BindProperty]
        public bool flag_no_order_open { get; set; }
        [BindProperty]
        public decimal item_price { get; set; }
        public DataSet nested_tables { get; set; }

        [BindProperty]
        public static long details_id { get; set; }
        [BindProperty]
        public bool flag_orders { get; set; }
        [BindProperty]

        public static DataTable? dt_orders { get; set; }
        [BindProperty]

        public static DataTable? dt_items { get; set; }

        [BindProperty]
        public bool flag_items { get; set; }

        [BindProperty]
        public int Ordered_Quantity { get; set; }

        [BindProperty]
        public string Msg_Order_Form { get; set; }
        [BindProperty]
        public string Msg_Item_Form { get; set; }

        [BindProperty]

        public DateTime Order_Date { get; set; }

        [BindProperty]
        public string PONumber { get; set; }

        [BindProperty]
        public bool flag_new_order_form { get; set; }
        [BindProperty]
        public bool flag_item_entry_form { get; set; }

        [BindProperty]

        public static long Order_ID { get; set; }
        
        [BindProperty]
        static DataTable? items { get; set; }

        [BindProperty]
        [ValidateNever]
        public static List<Item_Codes_Description_Qty_In_Hand> itemList { get; set; }

        [BindProperty]

        public int item_id { get; set; }
        public IActionResult OnGet()
        {
            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));

            if (flag_username)
            {
                return RedirectToPage("Index");
            }
            Fill_ItemList();
            Fill_Orders();

            flag_no_order_open = true;
            return Page();
        }

        // THIS FUNCTION POPULTAES DROPDOWN ITEM_CODE_WITH_DESCRIPTION_QTY_IN_HAND WITH ALL ITEMS
        // WITH QUANTITY IN HAND ,PRESENT IN DATABASE.

        public void Fill_ItemList()
        {
            Order_Outbound order_Outbound = new Order_Outbound();
            string warehouse = HttpContext.Session.GetString("warehouse");
            
            items = order_Outbound.ItemsAvailable_ItemCode_Description(warehouse);

            itemList = new List<Item_Codes_Description_Qty_In_Hand>();
            if (items == null)
                return;
            for (int i = 0; i < items.Rows.Count; i++)
            {
                Item_Codes_Description_Qty_In_Hand item_1 = new();
                item_1.Item_ID = Convert.ToInt32(items.Rows[i]["Item_ID"]);
                item_1.Item_Code_Description_Qty_In_Hand = items.Rows[i]["Item_Code"].ToString()+"-"+ items.Rows[i]["Item_Desc"].ToString()+" ("+ items.Rows[i]["QUANTITY_AVAILABLE"].ToString()+")";
                itemList.Add(item_1);
            }

        }

        // THIS FUNCTION IS CALLED FROM Add_Order BUTTON.IT SHOWS ORDER ENTRY FORM WITH RESET VALUES. 

        public IActionResult OnPostAdd_Order()
        {
            Reset_Order_Form();
            flag_new_order_form = true;
            flag_no_order_open = false;
            return Page();
        }

        // THIS FUNCTION IS CALLED FROM Add_Item BUTTON.IT SHOWS ITEM ENTRY FORM WITH RESET VALUES. 

        public IActionResult OnPostAdd_Item()
        {
            Reset_Item_Form();
            Order_Outbound order = new Order_Outbound();
            DataRow? row = order.GetOrderById(Order_ID);

            Fill_Order_Form(row);

            flag_new_order_form = true;
            flag_item_entry_form = true;
            return Page();
        }

        // THIS FUNCTION DOES RESET FOR ORDER ENTRY FORM.

        public void Reset_Order_Form()
        {
            Order_ID = 0;
            Order_Date = DateTime.Now;
            PONumber = string.Empty;
            ModelState.Clear();

        }

        // THIS FUNCTION DOES SUBMIT FOR ORDER ENTRY FORM.IT INSERT/UPDATE AN ORDER DEPENDING ON BOOL flag_updated
        // IF true,THEN UPDATE,ELSE INSERT AN OUTBOUND ORDER.

        public IActionResult OnPostSubmitOrder(bool flag_updated)
        {
            if (!Check_Order_Input())
            {
                flag_new_order_form = true;
                return Page();
            }
            string user = HttpContext.Session.GetString("username");

            Order_Outbound order = new Order_Outbound();
            if (flag_updated)
            {
                order.UpdateOrder(Order_ID, Order_Date, PONumber, user);
                return Redirect("Order_Entry_Outbound");
            }
            else
            {
				string warehouse = HttpContext.Session.GetString("warehouse");
				Order_ID = order.InsertNewOrder(Order_Date, PONumber, user,warehouse);
                flag_new_order_form = true;
                flag_item_entry_form = true;
            }

            return Page();
        }

        // THIS FUNCTION DOES CANACEL FOR ORDER ENTRY FORM.

        public IActionResult OnPostCancelOrder()
        {
            flag_new_order_form = false;
            flag_orders = true;
            flag_no_order_open = true;

            Reset_Order_Form();
            Fill_Orders();
            return Page();
        }

        // THIS FUNCTION DOES SUBMIT FOR ITEM ENTRY FORM.IT INSERT/UPDATE AN ITEM DEPENDING ON BOOL flag_updated
        // IF true,THEN UPDATE,ELSE INSERT AN ITEM FOR THIS ORDER.


        public IActionResult OnPostSubmitItem(bool flag_updated)
        {
            if (!Check_Item_Input())
            {
                //           flag_new_order_form = true;
                flag_item_entry_form = true;
                flag_new_order_form = true;
                flag_no_order_open = false;
                return Page();
            }

            Order_Outbound order = new Order_Outbound();

            if (flag_updated)    // UPDATED ITEM
                order.UpdateItem(details_id, item_id, Ordered_Quantity,item_price, Currency);
            else                // NEW ITEM
                order.InsertNewItem(Order_ID, item_id, Ordered_Quantity,item_price,Currency);

 
            DataRow? row = order.GetOrderById(Order_ID);
            Fill_Order_Form(row);

            flag_new_order_form = true;

            dt_items = order.GetItems(Order_ID);

            flag_items = true;
            flag_no_order_open = false;

            Reset_Item_Form();

            Fill_ItemList();
            
            return Page();
        }

        // THIS FUNCTION DOES CANCEL FOR ITEM ENTRY FORM.

        public IActionResult OnPostCancelItem()
        {
            flag_item_entry_form = false;
            flag_new_order_form = true;
            flag_no_order_open = false;
            Reset_Item_Form();
            Order_Outbound order = new Order_Outbound();
            dt_items = order.GetItems(Order_ID);
            flag_items = true;
            return Page();
        }

        // THIS FUNCTION DELETES AN ITEM FOR THIS ORDER.IT ALSO,UPDATES DATATBLE dt_items WITH ITEMS PRESENT IN
        // THIS ORDER.

        public IActionResult OnPostDeleteItem(long id, long order_id_1)
        {
            Order_Outbound order = new Order_Outbound();
            order.DeleteItem(id);

            Order_ID = order_id_1;

            dt_items = order.GetItems(Order_ID);

            DataRow? row = order.GetOrderById(Order_ID);

            Fill_Order_Form(row);

            Fill_ItemList();
            flag_items = true;
            flag_new_order_form = true;
            flag_no_order_open = false;

            return Page();
        }

        // THIS FUNCTION DOES RESET FOR ITEM ENTRY FORM.

        public void Reset_Item_Form()
        {
            item_id = 0;
            details_id = 0;
            Ordered_Quantity = 0;
            item_price = 0;
            Currency = "";
            ModelState.Clear();

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

        // THIS FUNCTION CHECKS USER INPUTS FOR ORDER ENTRY FORM.RETURNS true ON SUCCESS,false ON FAILURE.


        public bool Check_Order_Input()
        {
            if (string.IsNullOrEmpty(PONumber))
            {
                Msg_Order_Form = "Please Give Repair PONumber.";
                return false;
            }
            Find_PONumber find = new Find_PONumber();
            int find_ponumber = find.Find_Repair_PONumber(PONumber);

            if (find_ponumber == 0)
            {
                Msg_Order_Form = "Repair PONumber dosen't exists.Please Give valid Repair PONumber";
                return false; 
            }
            return true;
        }

        // THIS FUNCTION CHECKS USER INPUTS FOR ITEM ENTRY FORM.RETURNS true ON SUCCESS,false ON FAILURE.


        public bool Check_Item_Input()
        {
            if (item_id == 0)
            {
                Msg_Item_Form = "Please Select Item Code.";
                return false;
            }
            if (Ordered_Quantity <= 0)
            {
                Msg_Item_Form = "Please Give Item Quantity";
                return false;
            }
            if(item_price==0)
            {
                Msg_Item_Form = "Please Give Item Price";
                return false;
            }
            if (Currency == "0")
            {
                Msg_Item_Form = "Please Give Item Currency";
                return false;
            }
            return true;
        }

        // THIS FUNCTION IS CALLED FROM NESTED ITEMS GRID OR FROM ITEMS GRID.IT OPENS ITEM ENTRY FORM WITH VALUES
        // PASSED IN THIS FUNCTION.

        public IActionResult OnPostEditItem(long id_1, long order_id_1, int item_id_1, int ordered_quantity_1,decimal price_1,string currency_1)
        {
            flag_item_entry_form = true;

            Ordered_Quantity = ordered_quantity_1;
            Order_ID = order_id_1;
            details_id = id_1;
            item_id = item_id_1;
            item_price=price_1;
            if (currency_1 != null)
                Currency = currency_1;
            else
                Currency = "0";
           
            Order_Outbound order = new Order_Outbound();
            DataRow? row = order.GetOrderById(Order_ID);

            Fill_Order_Form(row);
            Fill_ItemList();

            flag_new_order_form = true;
            flag_no_order_open = false;
            return Page();
        }

        // THIS FUNCTION IS CALLED FROM ORDERS GRID'S ROW EDIT BUTTON.IT OPENS ORDER ENTRY FORM WITH VALUES PASSED.

        public IActionResult OnPostEditOrder(long order_id_1, string order_date, string ponumber)
        {
            flag_new_order_form = true;
            flag_item_entry_form = true;
            flag_no_order_open = false;

            Order_ID = order_id_1;

            // Changing dates to ISO standard

            DateTime dt = DateTime.Parse(order_date);
            string s2 = dt.ToString("yyyy-MM-ddTHH:mm:ss");
            Order_Date = DateTime.Parse(s2);

            PONumber = ponumber;

            return Page();
        }

        // THIS FUNCTION FILLS ORDER ENTRY FORM WITH PASSED DATAROW VALUES.

        public void Fill_Order_Form(DataRow? row)
        {
            if (row != null)
            {
                Order_ID = Convert.ToInt32(row["ORDER_ID"]);
                Order_Date = Convert.ToDateTime(row["ORDER_DATE"]);
                PONumber = row["PONUMBER"].ToString();
            }
        }

        // THIS FUNCTION IS CALLED ON DROPDOWN ITEMS OnSelectChanged EVENT.IT UPADTES ITEM_PRICE AND CURRENCY 
        // FOR SELECTED ITEM.

        public IActionResult OnPostOnSelectChanged()
        {
   
            flag_item_entry_form = true;
            flag_new_order_form = true;
            flag_no_order_open = false;
            
            Item item = new Item();
            DataRow row = item.Get_By_Item_ID(item_id);

            if (row["Price"] != DBNull.Value)
            {
                decimal priceData = ((Convert.ToDecimal(row["Price"]) * 20) / 100) + Convert.ToDecimal(row["Price"]);
                var data = priceData.ToString("0.00");
                item_price = Convert.ToDecimal(data);
            }
            else
                item_price = 0;

            if (row["Currency"] != DBNull.Value)
                Currency = row["Currency"].ToString();
            else
                Currency = "0";
            
            return Page();
        }
    }
}
