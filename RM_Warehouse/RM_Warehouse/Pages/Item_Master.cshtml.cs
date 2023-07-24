using DAL.CRUD;
using DocumentFormat.OpenXml.VariantTypes;
//using DocumentFormat.OpenXml.Spreadsheet;
using Irony.Parsing.Construction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Data;
using System.Reflection;

namespace RM_Warehouse.Pages
{
    // THIS CLASS IS FOR ITEM MASTER PAGE.
    public class Item_MasterModel : PageModel
    {
        [BindProperty]
        public static int item_id_upload { get; set; }  
        public string File_Empty_Msg { get; set; }
        [BindProperty]
        public IFormFile UploadFiles { get; set; }

        [BindProperty]
        public bool flag_upload { get; set; }
        [BindProperty]
        public bool flag_price_history { get; set; }
        [BindProperty]
        public string item_desc_history { get; set; }
        [BindProperty]
        public string item_code_history { get; set; }
        public static DataTable? dt_price_history { get; set; }
        [BindProperty]
        public string item_currency_old { get; set; }
        [BindProperty]
        public decimal item_price_old { get; set; }
        [BindProperty]
        public string item_currency_new { get; set; }
        [BindProperty]
        public decimal item_price_new { get; set; }
        [BindProperty]
        public string item_desc_price { get; set; }
        [BindProperty]
        public string item_code_price { get; set; }
        [BindProperty]
        public int item_id_price { get; set; }
        public string Msg_Update_Price { get; set; }
        [BindProperty]
        public bool flag_update_price_form { get; set; }
        [BindProperty]
        public bool flag_search { get; set; }
        [BindProperty]
        public static DataTable? items { get; set; }
        [BindProperty]
        [ValidateNever]
        public static List<Item_Codes_Description> itemList { get; set; }
        [BindProperty]
        public int item_id_search { get; set; }
        [BindProperty]
        public bool flag_locations { get; set; }
        [BindProperty]
        public string item_code_locations { get; set; }
        [BindProperty]
        public string item_desc_locations { get; set; }
        [BindProperty]
        public static DataTable? dt_locations { get; set; }
        [BindProperty]
        public bool flag_entry_form { get; set; }
        [BindProperty]
        public string Label { get; set; }
        public string Msg { get; set; }

        public static DataTable? dt_items_count { get; set; }
        public static DataTable? dt_items_all { get; set; }
        
        public static int cnt { get; set; }

        [BindProperty]

        public int item_id { get; set; }

        [BindProperty]

        public string item_code { get; set; }

        [BindProperty]

        public string item_desc { get; set; }
        [BindProperty]

        public string Currency { get; set; }
        
        [BindProperty]

        public decimal price { get; set; }

        [BindProperty]

        public string created_by { get; set; }

        [BindProperty]

        public DateTime created_date { get; set; }

        [BindProperty]

        public string? updated_by { get; set; }
        [BindProperty]

        public DateTime? updated_date { get; set; }

        public readonly IConfiguration _configuration;
        public Item_MasterModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult OnGet()
        {

            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));

            if (flag_username)
            {
                return RedirectToPage("Index");
            }

            Label = "Create New Item";
            Fill_Items();
            //Fill_ItemList();

            return Page();
        }

        // THIS FUNCTION POPULATES DATATABLE dt_items_all  WITH ALL ITEMS PRESENT IN DATABASE 

        public void Fill_Items()
        {

            string warehouse = HttpContext.Session.GetString("warehouse");

            Item item = new Item();
            dt_items_all = item.GetAllItems(warehouse);
            dt_items_count = item.GetAllCount(warehouse);
            cnt = (int)dt_items_count.Rows[0][0];
        }

        // THIS FUNCTION DOES INSERT OR UPDATE ITEM RECORD.

        public IActionResult OnPostSubmit()
        {
            if (!Check_Input())
                return Page();

            string user = HttpContext.Session.GetString("username");
            string warehouse = HttpContext.Session.GetString("warehouse");

            Item item = new Item();

            if (Label == "Create New Item")
            {
                item.CreateRecord(item_code, item_desc,price, Currency, user.ToUpper(), DateTime.Now,warehouse);
            }
            else
            {
                item.UpdateRecord(item_id, item_code, item_desc, user.ToUpper(), DateTime.Now,warehouse);
            }


            // reseting the form

            Reset_Form();

            Fill_Items();

            flag_entry_form = false;

            return Page();
        }

        // THIS FUNCTION CHECKS FOR USER INPUTS- ITEM_CODE AND ITEM_DESC.
        // RETURNS false FOR EMPTY INPUTS OR true FOR FILLED INPUTS.  

        public bool Check_Input()
        {
            flag_entry_form = true;
            
            if (string.IsNullOrEmpty(item_code))
            {
                Msg = "Please enter Item Code.";
                return false;
            }
            if (string.IsNullOrEmpty(item_desc))
            {
                Msg = "Please enter Item Description.";
                return false;
            }
            
            return true;
        }

        // THIS FUNCTION FILLS VALUES IN ITEM ENTRY FORM (flag_entry_form) AND SHOWS THIS FORM TO USER. 

        public IActionResult OnPostEdit(int item_id_1)
        {
            Item item = new Item();

            DataRow dr_by_item_id = item.Get_By_Item_ID(item_id_1);

            item_id = item_id_1;
            item_code = dr_by_item_id["Item_Code"].ToString();
            item_desc = dr_by_item_id["Item_Desc"].ToString();
            created_by = dr_by_item_id["Created_By"].ToString();
            created_date = Convert.ToDateTime(dr_by_item_id["Created_Date"]);
            updated_by = dr_by_item_id["Updated_By"].ToString();
            if (dr_by_item_id["Updated_Date"] != DBNull.Value)
                updated_date = Convert.ToDateTime(dr_by_item_id["Updated_Date"]);
            else
                updated_date = null;


            Label = "Update Item";

            flag_entry_form = true;

            return Page();
        }

        //THIS FUNCTION RESETS ALL INPUTS AND SHOWS ITEM ENTRY FORM WITH EMPTY VALUES.

        public IActionResult OnPostAdd_Item()
        {
            Reset_Form();
            flag_entry_form = true;
            return Page();
        }

        // THIS FUNCTION RESETS ITEM ENTRY FORM INPUTS TO EMPTY VALUES.
        public void Reset_Form()
        {
            item_code = string.Empty;
            item_desc = string.Empty;
            
            ModelState.Clear();
            Label = "Create New Item";
        }

        // THIS FUNCTION RESETS ITEM ENTRY FORM INPUTS AND HIDES THIS FORM.

        public IActionResult OnPostCancel()
        {
            Reset_Form();
            flag_entry_form = false;
            return Page();
        }

        // THIS FUNCTION POPULATES DATATABLE dt_locations WITH QTY_IN_HAND,LOCATIONS,EXPIRY DATE,COST AND
        // CURRENCY FIELDS.
        // THIS FUNCTION IS CALLED FROM "SHOW LOACTIONS" BUTTON IN ITEMS GRID'S ROWS.

        public IActionResult OnPostShow_Locations(int item_id_1,string item_code_1,string item_desc_1)
        {
            item_code_locations = item_code_1;
            item_desc_locations= item_desc_1;
                        
            string warehouse = HttpContext.Session.GetString("warehouse");
            Item item = new Item();
            dt_locations = item.Get_Available_Items_and_Locations(item_id_1, warehouse);
            flag_locations = true;
            flag_search = false;

            return Page();
        }

        // THIS FUNCTION POPULATES DROPDOWN - ITEM CODES WITH DECSRIPTION WITH ALL ITEMS PRESENT IN DATABASE.

        //public void Fill_ItemList()
        //{

        //    string warehouse = HttpContext.Session.GetString("warehouse");

        //    Item item = new Item();
        //    items = item.GetAll(warehouse);

        //    itemList = new List<Item_Codes_Description>();
        //    if (items == null)
        //        return;
        //    for (int i = 0; i < items.Rows.Count; i++)
        //    {
        //        Item_Codes_Description item_1 = new();
        //        item_1.Item_ID = Convert.ToInt32(items.Rows[i]["Item_ID"]);
        //        item_1.Item_Code_Description = items.Rows[i]["Item_Code"].ToString() + "-" + items.Rows[i]["Item_Desc"].ToString();
        //        itemList.Add(item_1);
        //    }

        //}

        // THIS FUNCTION IS CALLED FROM "SEARCH LOCATIONS" - BUTTON.IT ALSO POPULATES DATATABLE dt_locations FOR
        // ITEM CODES DROPDOWN'S OPTION VALUE item_id_search.
        // IT ALSO CLEARS DATATABLE dt_items_all and INSERTS SEARCHED RECORD INTO THIS DATATABLE.

        public IActionResult OnPostSearch_Locations()
        {
            if(item_id_search==0)
            {
                return Page();
            }
                        
            string warehouse = HttpContext.Session.GetString("warehouse");


            Item item = new Item();
            dt_locations = item.Get_Available_Items_and_Locations(item_id_search, warehouse);

            DataRow item_by_id = item.Get_By_Item_ID(item_id_search);

            item_code_locations = item_by_id["Item_Code"].ToString();
            item_desc_locations=  item_by_id["Item_Desc"].ToString();


			dt_items_all.Clear();
            dt_items_all.ImportRow(item_by_id);

            flag_locations = true;
//            flag_search = true;
            return Page();
        }

        // THIS FUNCTION OPENS UPDATE ITEM PRICE FORM AND SETS FORM FEILDS WITH ITEM GRID'S THIS ROW VALUES.

        public IActionResult OnPostUpdate_Price(int item_id_1, string item_code_1, string item_desc_1,decimal old_price_1,string old_currency_1)
        {
            item_id_price = item_id_1;
            item_code_price = item_code_1;
            item_desc_price = item_desc_1;
            item_price_old=old_price_1;
            item_currency_old=old_currency_1;

            flag_update_price_form = true;          
            return Page();
        }

        // THIS FUNCTION CHECKS USER INPUTS AND SUBMITS NEW ITEM PRICE INTO DATABASE.THEN REDIRETS TO SAME PAGE.

        public IActionResult OnPostSubmit_New_Price()
        {
            if(item_price_new==0)
            {
                Msg_Update_Price = "Please Give New Price";
                flag_update_price_form = true;
                return Page();
            }

            if (item_currency_new == "0")
            {
                Msg_Update_Price = "Please Give New Currency";
                flag_update_price_form = true;
                return Page();
            }

            string user = HttpContext.Session.GetString("username");

            Item item = new Item();
            item.UpdatePriceOfItem(item_id_price, item_price_old, item_currency_old, item_price_new, item_currency_new, user);
            
            return Redirect("Item_Master");
        }

        // THIS FUNCTION DOES CANCEL FOR UPDATE ITEM PRICE FORM
        public IActionResult OnPostCancel_New_Price()
        {
         //   Reset_Form();
            flag_update_price_form = false;
            return Page();
        }

        // THIS FUNCTION POPULATES AND SHOWS DATATABLE dt_price_history FOR THIS PARTICULAR ITEM.

        public IActionResult OnPostPrice_History(int item_id_1, string item_code_1, string item_desc_1)
        {
            item_code_history = item_code_1;
            item_desc_history = item_desc_1;

            Item item = new Item();
            dt_price_history = item.GetPriceHistoryByItemID(item_id_1);

            flag_price_history = true;
            return Page();
        }

        // THIS FUNCTION APPENDS UNIQUE GUID TO FILENAME.THEN IT SAVES THIS FILE TO PHYSICAL FOLDER.IT ALSO
        // UPDATES Item_Master TABLE WITH Image_Filename FIELD.
        // ON SUCCESS,IT ALERTS USER WITH CONFIRMATION MESSAGE. 

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (UploadFiles == null)
            {
                File_Empty_Msg = "Please select File.";
                flag_upload = true;

                return Page();
            }
            if (!CheckFileType(UploadFiles.FileName))
            {
                File_Empty_Msg = "Not valid type.Please select image or pdf file.";
                flag_upload = true;

                return Page();
            }
            var guid = Guid.NewGuid().ToString();
            var file_name = guid + "__" + UploadFiles.FileName;
            string folder_path = _configuration.GetValue<string>("Upload_Folder_Item_Pics");
            var file_path_upload = Path.Combine(folder_path, file_name);
            using (var fs = new FileStream(file_path_upload, FileMode.Create))
            {
                await UploadFiles.CopyToAsync(fs);
                
                TempData["ConfirmationMessage"] = "File :" + UploadFiles.FileName + " uploaded successfully....!";

                // Saving to Item_Master Table.
                
                Item item = new Item();
                item.UpdateImageFileName(item_id_upload, file_name);   

                ModelState.Clear();
            
            }
            return Redirect("Item_Master");
        }

        // THIS FUNCTION CHECKS FILE EXTENSIONS FOR IMAGE/* OR PDF.IT RETURNS true ON SUCCESS ,ELSE RETURNS false.

        bool CheckFileType(string file_name)
        {
            string ext = Path.GetExtension(file_name);
            switch (ext.ToLower())
            {
                case ".gif":
                    return true;
                case ".jpg":
                    return true;
                case ".jpeg":
                    return true;
                case ".png":
                    return true;
                case ".pdf":
                    return true;
                default:
                    return false;
            }
        }

        // THIS FUNCTION OPENS FILE UPLOAD FORM.

        public IActionResult OnPostUpload_Image(int item_id_1, string item_code_1, string item_desc_1)
        {

            item_id_upload = item_id_1;

            flag_upload = true;
            return Page();
        }

        public async Task<JsonResult> OnGetItemList(string searchTerm)
        {
            Item item = new Item();
            string warehouse = HttpContext.Session.GetString("warehouse");

            List<Item_Codes_Description> ListLocations = new List<Item_Codes_Description>();
            items = item.GetAllItem(warehouse,searchTerm);
            if (items != null)
            {
                ListLocations = ConvertDataTable<Item_Codes_Description>(items);

            }
           
            
            var SearchList = (from e in ListLocations
                              select new
                              {
                                  id = e.Item_ID,
                                  text = e.Item_Code_Description,
                              }).Distinct().ToList();
            return new JsonResult(SearchList);
        }
        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }
    }
}
