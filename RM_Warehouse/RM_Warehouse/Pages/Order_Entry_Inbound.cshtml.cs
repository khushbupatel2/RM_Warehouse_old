using DAL.CRUD;
using DAL.Sequence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace RM_Warehouse.Pages
{

    // THIS CLASS IS USED FOR ORDER MANAGEMENT -> INBOUND ORDER -> ORDER ENTRY PAGE.

public class Order_Entry_InboundModel : PageModel
{
    [BindProperty]
    public string Order_Description { get; set; }

    [BindProperty]
    public bool flag_no_order_open { get; set; }


    [BindProperty]
    public decimal Cost { get; set; }

    [BindProperty]
    public string Currency { get; set; }

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
    public DateTime Estimated_Arrival_Date { get; set; }

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
    public static DataTable? vendors { get; set; }
    [BindProperty]
    public static DataTable? items { get; set; }

    [BindProperty]
    [ValidateNever]
    public static List<Vendor_Names> vendorList { get; set; }

    [BindProperty]

    public int vendor_id { get; set; }

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
        Fill_VendorList();
        Fill_ItemList();
        Fill_Orders();
        flag_no_order_open = true;
        return Page();
    }
    
    // THIS FUNCTION POPULATES DROPDOWN VENDORS WITH ALL ACTIVE VENDORS.    

    public void Fill_VendorList()
    {
        Vendor vendor = new Vendor();
        vendors = vendor.GetAllActive();

        vendorList = new List<Vendor_Names>();
            if (vendors == null)
                return;
        for (int i = 0; i < vendors.Rows.Count; i++)
        {
            Vendor_Names vendor_1 = new();
            vendor_1.Vendor_ID = Convert.ToInt32(vendors.Rows[i]["Vendor_ID"]);
            vendor_1.Vendor_Name = vendors.Rows[i]["Vendor_Name"].ToString();
            vendorList.Add(vendor_1);
        }

    }

    // THIS FUNCTION POPULTAES DROPDOWN ITEM_CODE_WITH_DESCRIPTION WITH ALL ITEMS PRESENT IN DATABASE.

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
            item_1.Item_Code_Description = items.Rows[i]["Item_Code"].ToString()+"-"+ items.Rows[i]["Item_Desc"].ToString();
            itemList.Add(item_1);
        }
                
    }

    // THIS FUNCTION IS CALLED FROM Add_Order BUTTON.IT OPENS NEW ORDER FORM WITH RESET VALUES.

    public IActionResult OnPostAdd_Order()
    {
        Reset_Order_Form();
        flag_new_order_form = true;
        flag_no_order_open = false;
        return Page();
    }

    // THIS FUNCTION IS CALLED FROM Add_Item BUTTON.IT OPENS ITEM ENTRY FORM WITH RESET VALUES.

    public IActionResult OnPostAdd_Item()
    {
        Reset_Item_Form();
        Order_Inbound order = new Order_Inbound();
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
        Estimated_Arrival_Date = DateTime.Now.AddDays(1);
        PONumber = string.Empty;
        vendor_id = 0;

        ModelState.Clear();
            
    }

    // THIS FUNCTION DOES SUBMIT FOR ORDER ENTRY FORM.IT INSERTS OR UPDATES ORDER DEPENDING ON BOOL flag_updated.
    // IF true,DOES UPDATE.ELSE DOES INSERT.

    public IActionResult OnPostSubmitOrder(bool flag_updated)
    {
        if (!Check_Order_Input())
        {
            flag_new_order_form = true;
            return Page();
        }
        string user = HttpContext.Session.GetString("username");

        if (string.IsNullOrEmpty(Order_Description))
            Order_Description = string.Empty;

        Order_Inbound order = new Order_Inbound();
        if (flag_updated)
        {
            order.UpdateOrder(Order_ID, Order_Date, PONumber, Estimated_Arrival_Date, vendor_id, user,Order_Description);
            return Redirect("Order_Entry_Inbound");
        }
        else
        {
			string warehouse = HttpContext.Session.GetString("warehouse");
			Order_ID = order.InsertNewOrder(Order_Date, PONumber, Estimated_Arrival_Date, vendor_id, user,warehouse,Order_Description);
            flag_new_order_form = true;
            flag_item_entry_form = true;
        }

//            flag_new_order_form = true;
//            flag_item_entry_form = true;
        return Page();
    }

    // THIS FUNCTION DOES CANCEL FOR ORDER ENTRY FORM.IT HIDES ORDER ENTRY FORM AND SHOWS ADD NEW ORDER BUTTON
    // AND ORDERS GRID.

    public IActionResult OnPostCancelOrder()
    {
        flag_new_order_form = false;
        flag_no_order_open = true;
        flag_orders = true;

        Reset_Order_Form();
        Fill_Orders();
        return Page();
    }

    // THIS FUNCTION DOES SUBMIT FOR ITEM ENTRY FORM.IT INSERT OR UPDATE ITEM DEPENDING ON BOOL flag_updated.
    // IF true,DOES UPDATE.ELSE DOES INSERT.IT ALSO SHOWS ALL ITEMS FOR THIS ORDER.

    public IActionResult OnPostSubmitItem(bool flag_updated)
    {
        if (!Check_Item_Input())
        {
            flag_item_entry_form = true;
            flag_new_order_form = true;
            flag_no_order_open = false;
            return Page();
        }

        Order_Inbound order = new Order_Inbound();

        if (flag_updated)    // UPDATED ITEM
            order.UpdateItem(details_id, item_id, Ordered_Quantity,Cost,Currency);
        else                // NEW ITEM
            order.InsertNewItem(Order_ID, item_id, Ordered_Quantity, Cost, Currency);

            
        DataRow? row = order.GetOrderById(Order_ID);
        Fill_Order_Form(row);

        flag_new_order_form = true;

        dt_items = order.GetItems(Order_ID);

        flag_items = true;
        flag_no_order_open = false;

        Reset_Item_Form();
        return Page();
    }


    // THIS FUNCTION DOES CANCEL FOR ITEM ENTRY FORM.IT HIDES THIS FORM AND SHOWS ORDER ENTRY FORM AND ALL ITEMS
    // PRESENT IN THIS ORDER.

    public IActionResult OnPostCancelItem()
    {
        flag_item_entry_form = false;
        flag_new_order_form=true;
        flag_no_order_open = false;
        Reset_Item_Form();
        Order_Inbound order = new Order_Inbound();
        dt_items = order.GetItems(Order_ID);
        flag_items = true;
        return Page();
    }

    // THIS FUNCTION DELETS AN ITEM FROM THIS ORDER.IT SHOWS ORDER ENTRY FORM AND ALL ITEMS
    // PRESENT IN THIS ORDER.


    public IActionResult OnPostDeleteItem(long id,long order_id_1)
    {
        Order_Inbound order= new Order_Inbound();
        order.DeleteItem(id);

        Order_ID = order_id_1;
        
        dt_items = order.GetItems(Order_ID);
        
        DataRow? row = order.GetOrderById(Order_ID);
       
        Fill_Order_Form(row);

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
        Cost = 0;
        Currency = "0";
        ModelState.Clear();

    }

    // THIS FUNCTION POPULTES INBOUND ORDERS GRID WITH ORDER STAUS='CREATED'.
    // NESTED ITEMS DETAILS ARE WITH IS_RECEIVED=false FIELD.

    public void Fill_Orders()
    {
        Order_Inbound order = new Order_Inbound();
        string warehouse = HttpContext.Session.GetString("warehouse");

		dt_orders = order.GetOrders(warehouse);
            
        nested_tables=new DataSet();
        
        if (dt_orders == null)
        {
                flag_orders = true;
                return;
        }

        foreach(DataRow row in dt_orders.Rows)
        {
            long order_id = Convert.ToInt64(row["ORDER_ID"]);
            DataTable? temp= order.GetItems(order_id);

            if (temp != null)
            {
                temp.TableName = row["ORDER_ID"].ToString();
                nested_tables.Tables.Add(temp);
            }
        }
            flag_orders = true;
        }


    // THIS FUNCTION CHECKS USER INPUTS FOR ORDER ENTRY FORM  

    public bool Check_Order_Input()
    {
        if (string.IsNullOrEmpty(PONumber))
        {
            Msg_Order_Form = "Please Give PONumber.";
            return false;
        }
        if (vendor_id==0)
        {
            Msg_Order_Form = "Please Select Vendor Name.";
            return false;
        }
        
        return true;
    }


    // THIS FUNCTION CHECKS FOR USER IPUTS FOR ITEM ENTRY FORM.


    public bool Check_Item_Input()
    {
        if (item_id == 0)
        {
            Msg_Item_Form = "Please Select Item Code.";
            return false;
        }
        if(Ordered_Quantity<=0)
        {
            Msg_Item_Form = "Please Give Item Quantity";
            return false;
        }
        
        if (Currency == "0")
        {
            Msg_Item_Form = "Please Select Currency.";
            return false;
        }
        return true;
    }

    // THIS FUNCTION EDITS AN ITEM PRESENT IN ORDER.IT SHOWS ITEM ENTRY FORM WITH FILLED VALUES.


    public IActionResult OnPostEditItem(long id_1,long order_id_1,int item_id_1,int ordered_quantity_1,decimal cost_1,string currency_1)
    {
        flag_item_entry_form = true;
            
        Ordered_Quantity = ordered_quantity_1;
        Order_ID = order_id_1;
        details_id = id_1;
        Cost= cost_1;
        Currency= currency_1;
        item_id = item_id_1;
                
        Order_Inbound order = new Order_Inbound();
        DataRow? row = order.GetOrderById(Order_ID);
        
        Fill_Order_Form(row);
        
        flag_new_order_form = true;
        flag_no_order_open = false;
        return Page();
    }

    // THIS FUNCTION EDITS AN ORDER.IT SHOWS ORDER ENTRY FORM WITH FILLED VALUES.


    public IActionResult OnPostEditOrder(long order_id_1, string order_date, string ponumber,string estimated_arrival_date,string vendor_name,string order_description)
    {
        flag_new_order_form = true;
        flag_item_entry_form = true;
        flag_no_order_open = false;

        Order_ID= order_id_1;

        // Changing dates to ISO standard
        
        DateTime dt = DateTime.Parse(order_date);
        string s2 = dt.ToString("yyyy-MM-ddTHH:mm:ss");
        Order_Date = DateTime.Parse(s2);
    
        PONumber =ponumber;

        DateTime dt_1 = DateTime.Parse(estimated_arrival_date);
        string s2_1 = dt_1.ToString("yyyy-MM-ddTHH:mm:ss");
        Estimated_Arrival_Date = DateTime.Parse(s2_1);
    
        foreach (Vendor_Names vendor in vendorList)
        {
            if (vendor.Vendor_Name == vendor_name)
            {
                vendor_id = (int)vendor.Vendor_ID;
                break;
            }
        }

        Order_Description = order_description;
        
        return Page();
    }

    // THIS FUNCTION FILLS ORDER ENTRY FORM WITH PASSED DATAROW VALUES. 


    public void Fill_Order_Form(DataRow? row)
    {
        if (row != null)
        {
            Order_ID= Convert.ToInt32(row["ORDER_ID"]);
            Order_Date = Convert.ToDateTime(row["ORDER_DATE"]);
            PONumber = row["PONUMBER"].ToString();
            Estimated_Arrival_Date = Convert.ToDateTime(row["ESTIMATED_ARRIVAL_DATE"]);
            vendor_id = Convert.ToInt32(row["VENDOR_ID"]);
        }
    }

    // THIS FUNCTION GENERATES NEW PONUMBER WITH "WMS-" CONCATES WITH WAREHOUSE ABBREVATION AND CONCATES WITH
    // NEXT_VAL FORM DATABASE SEQUENCE.EXAMPLE "WMS-BRM-12".


    public IActionResult OnPostGeneratePONumber()
    {
        GeneratePONumber generatePONumber = new GeneratePONumber();
        long next_val = generatePONumber.NextVal();
        Warehouse wh = new Warehouse();
        string warehouse = HttpContext.Session.GetString("warehouse");
        string po_abbrivation = wh.GetWarehouse_PO_Abbrivation(warehouse);

        PONumber = "WMS-" + po_abbrivation + "-" + next_val.ToString();
        flag_new_order_form = true;
        
        return Page();
    }

    }
}
