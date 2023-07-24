using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using DAL.CRUD;

namespace RM_Warehouse.Pages
{

    // THIS CLASS IS FOR VENDOR MASTER PAGE.

    public class Vendor_IndexModel : PageModel
    {
        [BindProperty]
        public string Notes { get; set; }
        [BindProperty]
        public bool flag_vendors { get; set; }
        [BindProperty]
        public bool flag_vendor_form { get; set; }

        [BindProperty]
        public bool flag_search_form { get; set; }

        [BindProperty]
        public static DataTable DT { get; set; }
        [BindProperty]
        public long Vendor_ID { get; set; }
        [BindProperty]
        public string Vendor_Name { get; set; }
        [BindProperty]
        public string Street_Address { get; set; }
        [BindProperty]
        public string City { get; set; }
        [BindProperty]
        public string Prov_State { get; set; }
        [BindProperty]
        public string Postal_Code { get; set; }
        [BindProperty]
        public string Phone { get; set; }
        [BindProperty]
        public string Fax { get; set; }

        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public bool Is_Active { get; set; }
        [BindProperty]
        public string Label { get; set; }
        public string Msg { get; set; }

        public string Msg_Search { get; set; }
        [BindProperty]
        public string Search_Criteria { get; set; }
        [BindProperty]
        public string Search_Value { get; set; }

        [BindProperty]
        public string Contact_Person { get; set; }

        [BindProperty]
        public string Contact_Phone { get; set; }

        [BindProperty]
        public string Mode_Of_Payment { get; set; }

       
        // BELOW FUNCTION POPULTAES DATATBLE DT WITH ALL VENDORS PRESENT IN DATABASE.
        
        public IActionResult OnGet()
        {

            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));

            if (flag_username)
            {
                return RedirectToPage("Index");
            }

            Vendor vendor = new Vendor();
            DT = vendor.GetAll();

            flag_vendors = true;
            flag_search_form = true;
            Label = "Create New Vendor";
            
            return Page();


        }

        // THIS FUNCTION CHECKS USER INPUTS FOR VENDOR ENTRY FORM.

        public bool Check_Input()
        {
            if (Vendor_Name == null)
            {
                Msg = "Please enter Vendor Name.";
                return false;
            }
            if(Street_Address==null)
            {
                Msg = "Please enter Street Address.";
                return false;
            }
            if(City==null)
            {
                Msg = "Please enter City.";
                return false;
            }
            if(Prov_State==null)
            {
                Msg = "Please enter Province State.";
                return false;
            }
            if(Postal_Code==null)
            {
                Msg = "Please enter Postal Code.";
                return false;
            }
            if(Phone==null)
            {
                Msg = "Please enter Phone No.";
                return false;
            }
            if(Fax==null)
            {
                Msg = "Please enter Fax No.";
                return false;
            }
            if (Email == null)
            {
                Msg = "Please enter Email Address.";
                return false;
            }
            // contact info.
            if (Contact_Person == null)
            {
                Msg = "Please enter Contact Person.";
                return false;
            }
            if (Contact_Phone == null)
            {
                Msg = "Please enter Contact Phone.";
                return false;
            }
            if (Mode_Of_Payment == null)
            {
                Msg = "Please select Mode Of Payment.";
                return false;
            }
            
            return true;
        }

        // THIS FUNCTION DOES SUBMIT FOR VENDOR ENTRY FORM.IT INSERT/UPDATE VENDOR RECORD.DEPENDING ON VALUE OF
        // LABEL.if (Label == "Create New Vendor"),THEN INSERT NEW VENDOR RECORD,ELSE UPADTE VENDOR RECORD. 

        public IActionResult OnPostSubmit()
        {
            if (!Check_Input())
            {
                flag_vendor_form = true;
                return Page();
            }        
            Vendor vendor= new Vendor();
            if (Label == "Create New Vendor")
            {
                vendor.CreateRecord(Vendor_Name,Street_Address,City,Prov_State,Postal_Code,Phone,Fax,Email,Is_Active,Contact_Person,Contact_Phone,Mode_Of_Payment,Notes);
            }
            else
            {
                vendor.UpdateRecord(Vendor_ID,Vendor_Name, Street_Address, City, Prov_State, Postal_Code, Phone, Fax, Email, Is_Active, Contact_Person, Contact_Phone, Mode_Of_Payment, Notes);
            }
        
            DT = vendor.GetAll();

            //OnGetPage();

            // reseting the form

            Reset_Form();
            flag_vendors = true;
            flag_search_form = true;
            return Page(); 
        }

        // THIS FUNCTION IS INVOKED FROM VENDORS GRID'ROW Edit BUTTON.IT OPENS VENDOR ENTRY FORM FILLED WITH VALUES
        // FROM DATABASE FOR THAT VENDOR RECORD.

        public IActionResult OnPostEdit(long id)
        {
            Vendor vendor = new Vendor();
            DataRow dr = vendor.Get_By_Vendor_ID(id);

            Vendor_ID = id;
            Vendor_Name = dr[1].ToString();
            Street_Address = dr[2].ToString();
            City = dr[3].ToString();
            Prov_State = dr[4].ToString();
            Postal_Code = dr[5].ToString();
            Phone = dr[6].ToString();
            Fax = dr[7].ToString();
            Email = dr[8].ToString();
            Is_Active = Convert.ToBoolean(dr[9]);
            Contact_Person = dr[10].ToString();
            Contact_Phone = dr[11].ToString();
            Mode_Of_Payment = dr[12].ToString();
            Notes= dr[13].ToString();

            Label = "Update Vendor";
            flag_vendor_form = true;
            return Page();
        }

        // THIS FUNCTION SEARCHES VENDORS DATABASE TABLE WITH SEARCHED INPUTS.IT ALSO CHECKS USER INPUTS AND SHOWS 
        // ERROR MESSAGES.IT POPULATES DATATABLE DT WITH SEARCHED VENDOR RECORDS.

        public IActionResult OnPostSearch()
        {
            if(Search_Criteria==null)
            {
                Msg_Search = "Please select serach criteria.";
                DT = null;
                flag_search_form = true;
                flag_vendors = true;
                return Page();
            }
            if(Search_Value==null)
            {
                Msg_Search = "Please give search value";
                DT = null;
                flag_search_form = true;
                flag_vendors = true;
                return Page();
            }

            Vendor vendor = new Vendor();
            DT= vendor.Search(Search_Criteria, Search_Value);

            flag_search_form = true;
            flag_vendors = true;

            return Page();

        }

        // THIS FUNCTION OPENS VENDOR ENTRY FORM WITH RESET VALUES FOR NEW VENDOR RECORD.

        public IActionResult OnPostAdd_Vendor()
        {
            Reset_Form();
            flag_vendor_form = true;
            flag_vendors = false;
            flag_search_form = false;
            return Page();
        }

        // THIS FUNCTION DOES RESET FOR VENDOR ENTRY FORM.

        public void Reset_Form()
        {
            Vendor_Name = null;
            Street_Address = null;
            City = null;
            Prov_State = null;
            Postal_Code = null;
            Phone = null;
            Fax = null;
            Email = null;
            Is_Active = false;
            Contact_Person = null;
            Contact_Phone = null;
            Mode_Of_Payment = null;
            Notes = null;

            ModelState.Clear();
            Label = "Create New Vendor";
        }

        // THIS FUNCTION DOES CANCEL FOR VENDOR ENTRY FORM.

        public IActionResult OnPostCancel()
        {
            Reset_Form();
            flag_vendor_form = false;
            flag_vendors = true;
            flag_search_form=true;
            return Page();
        }

    }
}
