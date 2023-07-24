using DAL.CRUD;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Reflection.Emit;

namespace RM_Warehouse.Pages
{
    // THIS CLASS IS FOR ADMIN WAREHOUSE - PAGE
    public class Admin_WarehouseModel : PageModel
    {
        [BindProperty]
        public string warehouse_name { get; set; }
        public static DataTable? dt_wh_all { get; set; }
        
        [BindProperty]
        [ValidateNever]
        public static List<Warehouse_Names_Only> warehouseList { get; set; }

        [BindProperty]
        public bool flag_new_warehouse { get; set; }
        
        [BindProperty]
        public string UserName { get; set; }
        public static DataTable? dt_my_warehouses { get; set; }

        public string Msg { get; set; }

        // ON PAGE LOAD ,THIS FUNCTION CALLS FillWarehouseList() TO POPULATE WAREHOUSE DROPDOWN.
        public IActionResult OnGet()
        {

            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));

            if (flag_username)
            {
                return RedirectToPage("Index");
            }
            FillWarehouseList();
            dt_my_warehouses = null;
            return Page();
        }
        // THIS FUNCTION CALLS Warehouse.FindMyWarewhouses(UserName) FUNCTION TO POPULATE dt_my_warehouses
        // DATATABLE 
        public IActionResult OnPostFindMyWarehouses()
        {
            if(UserName==null)
            {
                Msg = "Please Give AD UserName.";
                return Page();
            }
            
            Warehouse warehouse = new();
            dt_my_warehouses = warehouse.FindMyWarewhouses(UserName);

            if(dt_my_warehouses==null)
            {
                Msg = "User is not assigned to any Warehouse.";
            }

  //          flag_new_warehouse = true;
  //          FillWarehouseList();
            return Page();
        }
        // THIS FUNCTION POPULATES warehouseList DROPDOWN WITH ALL WAREHOUSES PRESENT IN DATABASE TABLE.

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
        // THIS FUNCTION CEHECKS USER INPUTS FOR UserName,warehouse_name AND INSERTS USER GRANT FOR 
        // WAREHOUSE IN DATABASE TABLE

        public IActionResult OnPostSubmitNewWarehouse()
        {
            if (UserName == null)
            {
                Msg = "Please Give AD UserName.";
                flag_new_warehouse = true;
                return Page();
            }
            if (warehouse_name == null)
            {
                Msg = "Please Select Warehouse Name";
                flag_new_warehouse = true;
                return Page();
            }

            Warehouse warehouse = new Warehouse();

            string insert_status = warehouse.GiveAccess(warehouse_name,UserName);

            if(insert_status !="OK")
            {
                Msg = insert_status;
                flag_new_warehouse = true;
                return Page();
            }
            // UPDATE DATATABLE

            dt_my_warehouses = warehouse.FindMyWarewhouses(UserName);
            return Page();
        }

        // THIS FUNCTION CANCELS flag_new_warehouse FORM 
        public IActionResult OnPostCancel()
        {
            return Page();
        }
        // THIS FUNCTION DELTES USER ACCESS FOR WAREHOUSE FROM DATABASE TABLE. 
        public IActionResult OnPostDelete(int id_1)
        {
            Warehouse warehouse = new Warehouse();
            warehouse.DeleteAccess(id_1);

            // UPDATE DATATABLE

            dt_my_warehouses = warehouse.FindMyWarewhouses(UserName);

            return Page();
        }

        // THIS FUNCTION OPENS flag_new_warehouse FORM TO GIVE USER ACCESS FOR WAREHOUSE.
        public IActionResult OnPostAdd_New_Record()
        {
            flag_new_warehouse = true;
            warehouse_name = null;
            return Page();
        }

    }
}
