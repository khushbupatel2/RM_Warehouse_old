using DAL.CRUD;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace RM_Warehouse.Pages
{
    // THIS CLASS IS FOR COMPLETE ORDER PAGE  
    public class Complete_OrderModel : PageModel
    {
        public DataSet nested_tables { get; set; }

        [BindProperty]
        public bool flag_orders { get; set; }
        [BindProperty]

        public static DataTable? dt_orders { get; set; }
        
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
        public static List<Item_Codes_Description> itemList { get; set; }

        [BindProperty]

        public int item_id { get; set; }
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

        // THIS FUNCTION POPULATES DATATABLES WITH PICKED OUTBOUND ORDERS RECORDS.
        public void Fill_Orders()
        {
            Order_Outbound order = new Order_Outbound();
			string warehouse = HttpContext.Session.GetString("warehouse");
			dt_orders = order.GetOrders(warehouse,"PICK");

            nested_tables = new DataSet();
            if (dt_orders == null)
            {
                flag_orders = true;
                return;
            }

            foreach (DataRow row in dt_orders.Rows)
            {
                long order_id = Convert.ToInt64(row["ORDER_ID"]);
                DataTable? temp = order.GetItems(order_id,true,true);

                if (temp != null)
                {
                    temp.TableName = row["ORDER_ID"].ToString();
                    nested_tables.Tables.Add(temp);
                }
            }
            flag_orders = true;

        }
        // THIS FUNCTION COMPLETES AN ORDER AND SHOWS ALERT MESSAGE FOR COMPLETE ORDER.
        public IActionResult OnPostCompleteOrder(long order_id_1)
        {
            Order_Outbound order = new Order_Outbound();
            string user=HttpContext.Session.GetString("username");
            
            order.UpdateCompleteOrder(order_id_1, user);
            TempData["ConfirmationMessage"] = string.Format("Order ID:{0} is Completed", order_id_1);

            return Redirect("Complete_Order");
        }
    }
}
