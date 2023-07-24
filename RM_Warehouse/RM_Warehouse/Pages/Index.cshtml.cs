using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.DirectoryServices;
using System.Reflection.PortableExecutable;
using DirectoryEntry = System.DirectoryServices.DirectoryEntry;
using Microsoft.AspNetCore.Http;
using DAL.CRUD;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RM_Warehouse.Pages
{
    // THIS CLASS IS FOR LOGIN PAGE
    public class IndexModel : PageModel
    {
        // User Menu FLAGS starts here
        // THESE FLAGS ARE STATIC BOOLS WHICH STORES USER ACCESS FOR THE PAGES,FOR GRANTED PAGE ,IT HAS true,
        // FOR UNASSIGNED PAGE IT HAS false values.
        // THESE VALUES ARE USED IN _Layout.cshtml PAGE TO SHOW MENU OPTIONS FOR LOGIN USER.

        public static bool flag_Return_From_Garage { get; set; }

		public static bool flag_Return_To_Vendor { get; set; }

		public static bool flag_0_Inventory { get; set; }
        public static bool flag_Move_Inventory { get; set; }
        public static bool flag_PO_Invoice_Process { get; set; }
        public static bool flag_Notes { get; set; }
        public static bool flag_Submit_To_Accounts_Dept { get; set; }
        public static bool flag_Order_Entry_Inbound { get; set; }
        public static bool flag_Receive_Order { get; set; }
        public static bool flag_Put_Away { get; set; }
		public static bool flag_Order_Entry_Outbound { get; set; }
		public static bool flag_Pick { get; set; }
		public static bool flag_Complete_Order { get; set; }
		public static bool flag_Warehouse_Master { get; set; }
		public static bool flag_Location_Master { get; set; }
		public static bool flag_Item_Master { get; set; }
		public static bool flag_Vendor_Index { get; set; }
		public static bool flag_Put_Away_History { get; set; }
		public static bool flag_Pick_History { get; set; }
        public static bool flag_Admin_Warehouse { get; set; }
        public static bool flag_Admin_Pages { get; set; }

        // User Menu FLAGS ends here

        [BindProperty]
        public string warehouse_name { get; set; }
        public static DataTable? dt_wh_all { get; set; }
        [BindProperty]
        [ValidateNever]
        public static List<Warehouse_Names_Only> warehouseList { get; set; }

        private readonly ILogger<IndexModel> _logger;
        
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string Msg { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }


        public void OnGet()
        {
            FillWarehouseList();
        }

        // THIS FUNCTION CHECKS FOR Username,Password AND warehouse_name INPUTS.
        // IT CHECKS FOR USER AGAINST ACTIVE DIRECTORY AUTHENTICATION.
        // IT ALSO CHECKS FOR USER AGAINST WAREHOUSE AUTHENTICATION FROM OUR DATABASE.
        // IT FILLS SESSION OBJECT FOR FOLLOWING KEYS- warehouse,username,password(ENCRYPTED),
        // login_user_email,Is_Mechanic

        public IActionResult OnPostSubmit()
        {
            if (Username == null)
            {
                Msg = "Please enter Username";
                return Page();
            }
            else if (Password == null)
            {
                Msg = "Please enter Password";
                return Page();
            }
            else if(warehouse_name==null)
            {
                Msg = "Please Select Warehouse Name";
                return Page();
            }
            else {

                bool authorized = false;
                string domainAdnUserName = @"HEI\" + Username;
#pragma warning disable CA1416 // Validate platform compatibility
                DirectoryEntry entry = new("LDAP://local.hunterexpress.ca", domainAdnUserName, Password, AuthenticationTypes.None);

                try
                {
                    //Bind to the native AdsObject to force authentication.			
                    Object obj = entry.NativeObject;

                    DirectorySearcher search = new DirectorySearcher(entry);

                    search.Filter = "(SAMAccountName=" + Username + ")";
                    search.PropertiesToLoad.Add("cn");
                    search.PropertiesToLoad.Add("memberOf");
                    search.PropertiesToLoad.Add("mail");
                    SearchResult result = search.FindOne();

                    if (null == result)
                    {
                        Msg = "LOGIN FAILED.";
                        authorized = false;
                        return Page();
                    }
                    else
                    {
                        Warehouse warehouse = new Warehouse();
                        bool Is_Valid_Warehouse = warehouse.IsValidWarehouse(warehouse_name, Username);
                        if(!Is_Valid_Warehouse)
                        {
                            Msg = "You are not authorized for this Warehouse";
                            authorized = false;
                            return Page();
                        }
                        updateMenuFlags();

                        HttpContext.Session.SetString("warehouse", warehouse_name);
                        foreach (string GroupPath in result.Properties["memberOf"])
                        {
                            if (GroupPath.Contains("RM_MECHANIC")) //if (GroupPath.Contains("RM RECORD"))
                            {
                                HttpContext.Session.SetString("username", Username);
                                if (result.Properties["mail"][0] != null)
                                {
                                    HttpContext.Session.SetString("login_user_email", result.Properties["mail"][0].ToString());
                                }
                                
                                HttpContext.Session.SetString("password", EncryptDecrypt.Encrypt(Password));
                                HttpContext.Session.SetString("Is_Mechanic", "yes");
                                authorized = true;
                                Msg = "Success";
                                return RedirectToPage("Welcome");
                            }
                            if (GroupPath.Contains("RM RECORD")) //if (GroupPath.Contains("RM RECORD"))
                            {
                                HttpContext.Session.SetString("username", Username);
                                if (result.Properties["mail"][0] != null)
                                {
                                    HttpContext.Session.SetString("login_user_email", result.Properties["mail"][0].ToString());
                                }
                                HttpContext.Session.SetString("password", EncryptDecrypt.Encrypt(Password));
                                HttpContext.Session.SetString("Is_Mechanic", "no");
                                authorized = true;
                                Msg = "Success";
                                return RedirectToPage("Welcome");
                            }

                        }
                    }
                    if (!authorized)
                    {
                        Msg = "YOU ARE NOT AUTHORIZED TO USE THIS.";
                        return Page();
                    }
                }
                catch (Exception ex)
                {

                    Msg = ex.Message.ToUpper();

                }
            }
#pragma warning restore CA1416 // Validate platform compatibility
            return Page();
        }
        // THIS FUNCTION SHOWS ALL WAREHOUSE NAMES PRESENT IN OUR DATABASE
        public void FillWarehouseList()
        {

            Inhand_Inventory in_inv = new Inhand_Inventory();
            dt_wh_all = in_inv.GetAll_Warehouses();

            warehouseList = new List<Warehouse_Names_Only>();
            for (int i = 0; i < dt_wh_all.Rows.Count; i++)
            {
                Warehouse_Names_Only warehouse = new();
                warehouse.Warehouse = dt_wh_all.Rows[i]["Name"].ToString();
                warehouseList.Add(warehouse);
            }

        }

        // THIS FUNCTION CHECKS FOR USER ACCESS TO ALL PAGES IN OUR WEBSITE,ONE BY ONE.
        // ALSO,IT CHECKS PO INVOICE PROCESS TAB - "SUBMIT_TO_ACCOUNTS_DEPT" ACCESS FOR USER.

        public void updateMenuFlags()
        {
            User_Pages up = new User_Pages();

			flag_Return_From_Garage = up.IsValidPage("Return_From_Garage", Username);
			flag_Return_To_Vendor = up.IsValidPage("Return_To_Vendor", Username);
			flag_0_Inventory = up.IsValidPage("0_Inventory", Username);
            flag_Move_Inventory= up.IsValidPage("Move_Inventory", Username);
            flag_PO_Invoice_Process = up.IsValidPage("PO_INVOICE_PROCESS", Username);
            flag_Notes= up.IsValidPage("Notes", Username);
            flag_Submit_To_Accounts_Dept = up.IsValidPage("SUBMIT_TO_ACCOUNTS_DEPT", Username);
            flag_Order_Entry_Inbound = up.IsValidPage("Order_Entry_Inbound", Username);
            flag_Receive_Order = up.IsValidPage("Receive_Order", Username);
            flag_Put_Away = up.IsValidPage("Put_Away", Username);
            flag_Order_Entry_Outbound= up.IsValidPage("Order_Entry_Outbound", Username);
            flag_Pick = up.IsValidPage("Pick", Username);
            flag_Complete_Order = up.IsValidPage("Complete_Order", Username);
            flag_Warehouse_Master = up.IsValidPage("Warehouse_Master", Username);
            flag_Location_Master = up.IsValidPage("Location_Master", Username);
            flag_Item_Master = up.IsValidPage("Item_Master", Username);
            flag_Vendor_Index = up.IsValidPage("Vendor_Index", Username);
            flag_Put_Away_History = up.IsValidPage("Put_Away_History", Username);
            flag_Pick_History = up.IsValidPage("Pick_History", Username);
            flag_Admin_Warehouse = up.IsValidPage("Admin_Warehouse", Username);
            flag_Admin_Pages = up.IsValidPage("Admin_Pages", Username);


        }

    }
}