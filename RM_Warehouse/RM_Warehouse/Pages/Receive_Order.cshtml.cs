using DAL.CRUD;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace RM_Warehouse.Pages
{

    // THIS CLASS IS FOR ORDER MANAGEMENT -> INBOUND ORDER -> RECEIVE ORDER

    public class Receive_OrderModel : PageModel
    {
        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public List<int> AreChecked { get; set; }

        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime? Expiry_Date { get; set; } 

        [BindProperty]
        public DateTime Received_Date { get; set; }

        [BindProperty]
        public int Received_Quantity { get; set; }
        [BindProperty]
        public string Msg_Received_Order_Form { get; set; }
        [BindProperty]
        public static long details_id { get; set; }

        [BindProperty]
        public bool flag_received_order_form { get; set; }
        [BindProperty]
        public bool flag_orders { get; set; }

        public static DataSet nested_tables { get; set; }

        [BindProperty]

        public static DataTable? dt_orders { get; set; }

        public DataTable? dt_items { get;set; }

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

        // THIS FUNCTION POPULTAES NESTED DATATABLES WITH INBOUND CREATED ORDERS.ALL DETAILS ITEMS ARE NOT RECIEVED.

        public void Fill_Orders()
        {
            Order_Inbound order = new Order_Inbound();
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

        // THIS FUNCTION DOES SUBMIT FOR RECEIVE ORDER GRID.IT ACCEPTS USER INPUTS LIKE RECEIVED_QUANTITY,
        // RECEIVED_DATE AND EXPIRY_DATE FOR DETAILS ITEMS FOR EACH ROW.
        // IT UPDATES DATABASE WITH RECEIVED ITEMS.IF ALL ITEMS FOR INBOUND ORDER ARE RECEIVED.THEN IT UPDATES 
        // ORDER_STATUS AS RECEIVED.ALSO,IT ALERTS WITH RECEIVED ORDER MESSAGE.  

        public IActionResult OnPostReceiveOrder(long order_id_1,IFormCollection form)
        {
            string user = HttpContext.Session.GetString("username");

            Order_Inbound order = new Order_Inbound();
            string temp_id = form["Id"];
            string[] temp_ids = temp_id.Split(',');

            string temp_rec_qty = form["Received_Quantity"];
            string[] temp_rec_qtys = temp_rec_qty.Split(',');

            string temp_rec_date = form["Received_Date"];
            string[] temp_rec_dates = temp_rec_date.Split(',');

    
            foreach (var item in AreChecked)
            {
                int index = Array.IndexOf(temp_ids, item.ToString());
                
                int var_rec_qty = Convert.ToInt32(temp_rec_qtys[index]);
                DateTime var_rec_date = Convert.ToDateTime(temp_rec_dates[index]);
                DateTime? var_expiry_date;
                if (string.IsNullOrEmpty(form["Expiry_Date"][index]))
                    var_expiry_date = null;
                else
                    var_expiry_date = Convert.ToDateTime(form["Expiry_Date"][index]);
                order.UpdateReceivedItem(order_id_1, item, var_rec_qty, user, var_rec_date, var_expiry_date);
               
            }

            dt_items = order.GetItems(order_id_1);

            if(dt_items==null) // All items in order are received,so order status is "RECEIVED"
            {
                order.UpdateReceiveOrder(order_id_1);
                TempData["ConfirmationMessage"] = string.Format("Order ID:{0} is Received",order_id_1);
            }
            
            return Redirect("Receive_Order");
        }
    }
}
