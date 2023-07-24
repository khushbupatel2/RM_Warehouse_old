using DAL.CRUD;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.DirectoryServices;
using System.Net.Mail;
using static ClosedXML.Excel.XLPredefinedFormat;

namespace RM_Warehouse.Pages
{

    // THIS CLASS IS FOR PO INVOICE PROCESS PAGE.

    public class PO_INVOICE_PROCESSModel : PageModel
    {
        [BindProperty]
        public string Rejection_Uploaded_By { get; set; }
        [BindProperty]
        public string Rejection_PONumber { get; set; }
        [BindProperty]
        public string Rejection_Vendor_Name { get; set; }
        [BindProperty]
        public string Rejection_Invoice_Filename { get; set; }
        [BindProperty]
        public long Rejection_Order_ID { get; set; }
        [BindProperty]
        public string Msg_Rejection_Errors { get; set; }
        [BindProperty]
        public string Rejection_Reason { get; set; }
        [BindProperty]
        public bool flag_rejection_reason_form { get; set; }

        public static DataTable? items_table { get; set; }
        [BindProperty]
        public long Upload_Order_ID { get; set; }
        public string File_Empty_Msg { get; set; }
        [BindProperty]
        public IFormFile UploadFiles { get; set; }

        public bool flag_upload { get; set; }
        public DataTable? dt_items { get; set; }
        public string Msg_Invoice_Errors { get; set; }
        [BindProperty]
        public int Id { get; set; }
        [BindProperty]
        public List<int> AreChecked { get; set; }
        [BindProperty]
        public string Currency { get; set; }
        [BindProperty]
        public decimal Cost { get; set; }

        [BindProperty]
        public static DataTable? dt_orders { get; set; }

        [BindProperty]
        public static DataTable? dt_orders_tab2 { get; set; }

        [BindProperty]
        public static DataTable? dt_orders_tab3 { get; set; }
        [BindProperty]
        public static DataTable? dt_orders_tab4 { get; set; }
        public static DataSet nested_tables { get; set; }
        public static DataSet nested_tables_tab2 { get; set; }
        public static DataSet nested_tables_tab3 { get; set; }
        public static DataSet nested_tables_tab4 { get; set; }
        [BindProperty]
        public bool flag_orders { get; set; }

        [BindProperty]
        public bool flag_orders_tab2 { get; set; }

        [BindProperty]
        public bool flag_orders_tab3 { get; set; }

        [BindProperty]
        public bool flag_orders_tab4 { get; set; }

        public readonly IConfiguration _configuration;
        public PO_INVOICE_PROCESSModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // BELOW OnGet() FUNCTION POPULATES ALL TABS WITH ORDERS.

        public IActionResult OnGet()
        {
            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));

            if (flag_username)
            {
                return RedirectToPage("Index");
            }
            Fill_Orders();
            Fill_Orders_tab2();
            Fill_Orders_tab3();
            Fill_Orders_tab4();
            flag_orders = true;
            flag_orders_tab2 = true;
            flag_orders_tab3 = true;
            flag_orders_tab4 = true;
            return Page();
        }

        // THIS FUNCTION POPULATES TAB 1 WITH INBOUND ORDERS WITHOUT INVOICE UPLOADED.


        public void Fill_Orders()
        {
            PO_Details po = new PO_Details();
            string warehouse = HttpContext.Session.GetString("warehouse");
            dt_orders = po.GetOrdersWithoutInvoiceUpload(warehouse);

            nested_tables = new DataSet();
            if (dt_orders == null)
            {
                flag_orders = true;
                return;
            }
            foreach (DataRow row in dt_orders.Rows)
            {
                long order_id = Convert.ToInt64(row["ORDER_ID"]);
                DataTable? temp = po.GetItems(order_id);

                if (temp != null)
                {
                    temp.TableName = row["ORDER_ID"].ToString();
                    nested_tables.Tables.Add(temp);
                }
            }
  //          flag_orders = true;
        }

        // BELOW FUNCTION IS INVOKED FROM UPLOAD INVOICE BUTTON IN ORDERS GRID'S ROW.IT SHOWS UPLOAD INVOICE FORM.
        // THIS IS PRESENT IN TAB 1.

        public IActionResult OnPostUpload_Invoice(long order_id_1)
        {
            flag_upload = true;
            flag_orders = false;

            PO_Details po = new PO_Details();
           
            items_table = po.GetItems(order_id_1);

            Upload_Order_ID = order_id_1;
            return Page();
        }

        // THIS FUNCTION DOES CANCEL FOR INVOICE UPLOAD FORM IN TAB 1.
        public IActionResult OnPostCancel()
        {
            flag_upload = false;
            return Redirect("PO_INVOICE_PROCESS");
        }

        // THIS FUNCTION DOES SUBMIT FOR INVOICE UPLOAD FORM.IT CHECKS USER INPUTS FOR FILE UPLOADED,COST
        // AND CURRENCY.THEN IT PERFORMS DATABASE UPDATES.
        // IF ALL ITEMS FOR ORDER ARE PO DONE.THEN IT ALERTS MESSAGE TO USER REGARDING PO UPLOADED SUCCESSFULLY.


        public async Task<IActionResult> OnPostUploadAsync(IFormCollection form)
        {
            string user = HttpContext.Session.GetString("username");

            string temp_id = form["Id"];
            string[] temp_ids = temp_id.Split(',');

            PO_Details po = new PO_Details();

            if (UploadFiles == null)
            {
                File_Empty_Msg = "Please select File.";
                flag_upload = true;

                return Page();
            }
            if (!CheckFileType(UploadFiles.FileName))
            {
                File_Empty_Msg = "Not valid type.Please select .pdf file.";
                flag_upload = true;

                return Page();
            }

            // PO COST CURRENCY

            foreach (DataRow row in items_table.Rows)
            {
                int index = Array.IndexOf(temp_ids, row["ID"].ToString());

                decimal var_cost = Convert.ToDecimal(form["Cost"][index]);
                string var_currency = form["Currency"][index].ToString();

                if (var_cost == 0)
                {
                    Msg_Invoice_Errors = "Please Give Cost of Item.";
                    flag_upload = true;
                    return Page();
                }
                if (var_currency == "0")
                {
                    Msg_Invoice_Errors = "Please Select Currency of Item.";
                    flag_upload = true;
                    return Page();
                }

            }

            // UPDATE SQL TABLES

            foreach (DataRow row in items_table.Rows)
            {
                int index = Array.IndexOf(temp_ids, row["ID"].ToString());

                decimal var_cost = Convert.ToDecimal(form["Cost"][index]);
                string var_currency = form["Currency"][index].ToString();

                po.UpdateCostOfItem(Convert.ToInt64(row["ID"]), var_cost, var_currency);
            }


            dt_items = po.GetRemainingItems(Upload_Order_ID);

            if (dt_items == null) // PO done for All items in order,so order is Invoice Uploaded.
            {
                var guid = Guid.NewGuid().ToString();
                var file_name = guid + "__" + UploadFiles.FileName;
                string folder_path = _configuration.GetValue<string>("Upload_Folder_Path_Fixed");
                var file_path_upload = Path.Combine(folder_path, file_name);
                using (var fs = new FileStream(file_path_upload, FileMode.Create))
                {
                    await UploadFiles.CopyToAsync(fs);

                    po.SavePO(Upload_Order_ID, file_name,user);

                    TempData["ConfirmationMessage"] = "Invoice for Order_ID:" + Upload_Order_ID + "\\n" + " File: " + UploadFiles.FileName + " Uploaded Successfully.";

                    ModelState.Clear();
                    flag_upload = false;
                }
              
                return Redirect("PO_INVOICE_PROCESS");
            }
            flag_upload = true;
            
            return Page();
        }

        // THIS FUNCTION CHECKS EXTENSION OF FILE UPLOADED.ONLY .pdf FILES ARE VALID.

        bool CheckFileType(string file_name)
        {
            string ext = Path.GetExtension(file_name);
            switch (ext.ToLower())
            {
                case ".pdf":
                    return true;
                default:
                    return false;
            }
        }

        // THIS FUNCTION POPULATES TAB 2 WITH INBOUND ORDERS WITH INVOICE UPLOADED.
        public void Fill_Orders_tab2()
        {
            PO_Details po = new PO_Details();
            string warehouse = HttpContext.Session.GetString("warehouse");
            dt_orders_tab2 = po.GetOrdersWithInvoiceUpload(warehouse);

            nested_tables_tab2 = new DataSet();
            if (dt_orders_tab2 == null)
            {
                flag_orders_tab2 = true;
                return;
            }
            foreach (DataRow row in dt_orders_tab2.Rows)
            {
                long order_id = Convert.ToInt64(row["ORDER_ID"]);
                DataTable? temp = po.GetItems(order_id);

                if (temp != null)
                {
                    temp.TableName = row["ORDER_ID"].ToString();
                    nested_tables_tab2.Tables.Add(temp);
                }
            }
            //          flag_orders = true;
        }

        // THIS FUNCTION POPULATES TAB 3 WITH INBOUND ORDERS WITH INVOICE UPLOADED.

        public void Fill_Orders_tab3()
        {
            PO_Details po = new PO_Details();
            string warehouse = HttpContext.Session.GetString("warehouse");
            dt_orders_tab3 = po.GetOrdersWithInvoiceUpload(warehouse);

            nested_tables_tab3 = new DataSet();
            if (dt_orders_tab3 == null)
            {
                flag_orders_tab3 = true;
                return;
            }
            foreach (DataRow row in dt_orders_tab3.Rows)
            {
                long order_id = Convert.ToInt64(row["ORDER_ID"]);
                DataTable? temp = po.GetItems(order_id);

                if (temp != null)
                {
                    temp.TableName = row["ORDER_ID"].ToString();
                    nested_tables_tab3.Tables.Add(temp);
                }
            }
            //          flag_orders = true;
        }

        // THIS FUNCTION IS INVOKED FROM ORDERS GRID'S ROW BUTTON SEND INVOICE TO ACCT DEPT.IT IS PRESENT IN TAB 3.
        // IT SENDS INVOICE EMAIL TO ACCOUNTS DEPT.ALSO,IT ALERTS USER WITH EMAIL SENT MESSAGE.


        public IActionResult OnPostSend_Invoice_Email(long order_id_1,string po_number_1,string vendor_name_1,string file_name_1)
        {
            SendEmail(order_id_1, po_number_1, vendor_name_1, file_name_1);
            PO_Details po = new PO_Details();
            po.UpdateEmailSend(order_id_1);
            
            TempData["ConfirmationMessage"] = "For Order ID:"+order_id_1+"\\nEmail is sent to Accounts Dept.";

            return Redirect("PO_INVOICE_PROCESS");
        }

        // THIS FUNCTION DOES ALL EMAIL RELATED CODING FOR 'SEND INVOICE TO ACCT DEPT' BUTTON.
        

        public void SendEmail(long order_id, string po_number, string vendor_name,string file_name)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            // fetching record from database

            string To_Email = _configuration.GetValue<string>("ToAccountsEmailAddress");
            string From_Email = HttpContext.Session.GetString("login_user_email");

            //send email

            SmtpClient cl = new SmtpClient();
            cl.Host = _configuration.GetValue<string>("smtpHost");
            cl.Port = _configuration.GetValue<int>("smtpPort");
            cl.EnableSsl = true;
            cl.UseDefaultCredentials = false;
            MailMessage msg = new MailMessage();
            msg = new MailMessage();
            msg.From = new MailAddress(From_Email);
            msg.To.Add(To_Email);
            
            msg.Subject = "Invoice Of PoNumber : " + po_number ;
            msg.IsBodyHtml = true;
            string _msgbody = "THIS IS AUTOMATED EMAIL. DO NO REPLY TO EMAIL.";
            _msgbody += "<h4>HUNTER EXPRESS</h4>";
            _msgbody += "<div>Hi,<br> Please find attached invoice.<div><br> PO Number: " + po_number 
                + "<br>Vendor Name: " + vendor_name + "</div> <div> Please process for payment. <br> Thank you</div>"; ;

                   
            msg.Body = _msgbody;

            // ATTACHMENT INVOICE PDF

            string folder_path = _configuration.GetValue<string>("Upload_Folder_Path_Fixed");
            string file_path_upload = Path.Combine(folder_path, file_name);

            Attachment att;
            att = new Attachment(file_path_upload);
            msg.Attachments.Add(att);
            
            cl.Send(msg);
            msg.Dispose();
            cl.Dispose();

        }

        // THIS FUNCTION POPULATES TAB 4 WITH INBOUND ORDERS WITH ACCOUNTS EMAIL SENT FLAG=true.

        public void Fill_Orders_tab4()
        {
            PO_Details po = new PO_Details();
            string warehouse = HttpContext.Session.GetString("warehouse");
            dt_orders_tab4 = po.GetOrdersWithInvoiceUpload(warehouse,true);

            nested_tables_tab4 = new DataSet();
            if (dt_orders_tab4 == null)
            {
                flag_orders_tab4 = true;
                return;
            }
            foreach (DataRow row in dt_orders_tab4.Rows)
            {
                long order_id = Convert.ToInt64(row["ORDER_ID"]);
                DataTable? temp = po.GetItems(order_id);

                if (temp != null)
                {
                    temp.TableName = row["ORDER_ID"].ToString();
                    nested_tables_tab4.Tables.Add(temp);
                }
            }
            //          flag_orders = true;
        }

        // THIS FUNCTION RETURNS EMAIL ADDRESS OF created_by USER.THIS HAS ACTIVE DIRECTORY CODING.

        private string isADUser(string created_by)
        {
            string userName = HttpContext.Session.GetString("username");
            string passWord = EncryptDecrypt.Decrypt(HttpContext.Session.GetString("password"));

            List<string> allowedTerminal = new List<string>();
            //bool authorized = false;
            string domainAdnUserName = @"HEI\" + userName;
            DirectoryEntry entry = new DirectoryEntry("LDAP://local.hunterexpress.ca", domainAdnUserName, passWord, AuthenticationTypes.None);
            try
            {
                //Bind to the native AdsObject to force authentication.			
                Object obj = entry.NativeObject;

                DirectorySearcher search = new DirectorySearcher(entry);

                search.Filter = "(SAMAccountName=" + created_by + ")";
                //               search.Filter = "(SAMAccountName=" + "adhawan" + ")";
                search.PropertiesToLoad.Add("cn");
                search.PropertiesToLoad.Add("memberOf");
                search.PropertiesToLoad.Add("mail");
                SearchResult result = search.FindOne();

                if (null == result)
                {
                    //           authorized = false;
                    return null;
                }
                else
                {
                    foreach (string GroupPath in result.Properties["memberOf"])
                    {

                        if (result.Properties["mail"][0] != null)
                        {
                            return result.Properties["mail"][0].ToString();
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // THIS FUNCTION DOES ALL EMAIL RELATED CODING FOR 'SAVE REJECTION REASON' BUTTON IN REJECTION REASON FORM.

        public void Send_Rejection_Email(long order_id,string po_number,string vendor_name,string rejection_reason,string file_name,string uploaded_by)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            
            string To_Email = isADUser(uploaded_by);
            string From_Email = HttpContext.Session.GetString("login_user_email");

            //_configuration.GetValue<string>("FromAccountsEmailAddress");

            //send email

            SmtpClient cl = new SmtpClient();
            cl.Host = _configuration.GetValue<string>("smtpHost");
            cl.Port = _configuration.GetValue<int>("smtpPort");
            cl.EnableSsl = true;
            cl.UseDefaultCredentials = false;
            MailMessage msg = new MailMessage();
            msg = new MailMessage();
            msg.From = new MailAddress(From_Email);
            msg.To.Add(To_Email);

            msg.Subject = "Rejection : Invoice Of PoNumber : " + po_number;
            msg.IsBodyHtml = true;
            string _msgbody = "THIS IS AUTOMATED EMAIL. DO NO REPLY TO EMAIL.";
            _msgbody += "<h4>HUNTER EXPRESS</h4>";
            _msgbody += "<div>Hi," +
                "<br/>Your Invoice is Rejected " +
                "<br/>Order ID: " + order_id +
                "<br/>Rejection Reason: " +rejection_reason+
                "<br/>Please find attached invoice." +
                "<br/>PO Number: " + po_number+
                "<br/>Vendor Name: " + vendor_name + 
                "<br> Thank you</div>"; ;


            msg.Body = _msgbody;

            // ATTACHMENT INVOICE PDF

            string folder_path = _configuration.GetValue<string>("Upload_Folder_Path_Fixed");
            string file_path_upload = Path.Combine(folder_path, file_name);

            Attachment att;
            att = new Attachment(file_path_upload);
            msg.Attachments.Add(att);

            cl.Send(msg);
            msg.Dispose();
            cl.Dispose();

        }

        // THIS FUNCTION IS INVOKED FROM 'REJECTION REASON' BUTTON IN ORDERS GRID'S ROW.IT OPENS REJECTION REASON
        // FORM.IT IS PRESENT IN TAB 3.

        public IActionResult OnPostRejection_Reason(long order_id_1, string po_number_1, string vendor_name_1, string file_name_1,string invoice_uploaded_by_1)
        {
            flag_rejection_reason_form = true;
            Rejection_Order_ID= order_id_1;
            Rejection_Invoice_Filename = file_name_1;
            Rejection_PONumber = po_number_1;
            Rejection_Vendor_Name = vendor_name_1;
            Rejection_Uploaded_By = invoice_uploaded_by_1;

            //TempData["ConfirmationMessage"] = "Email is sent to Accounts Dept.";

            return Page();
        }

        // THIS FUNCTION DOES SUBMIT FOR REJECTION REASON FORM.IT CHECKS USER INPUTS.IT SENDS REJECTION
        // EMAIL TO UPLOAD BY (USER).IT ALSO UPDATES DATABASE.

        public IActionResult OnPostSubmitRejection(bool flag_updated)
        {
            if (Rejection_Reason==null)
            {
                flag_rejection_reason_form = true;
                Msg_Rejection_Errors = "Please Give Rejection Reason";
                return Page();
            }

            string user = HttpContext.Session.GetString("username");

            Send_Rejection_Email(Rejection_Order_ID,Rejection_PONumber,Rejection_Vendor_Name,Rejection_Reason,Rejection_Invoice_Filename, Rejection_Uploaded_By);

            PO_Details po = new PO_Details();
            po.UpdateRejectionReason(Rejection_Order_ID, Rejection_Reason, Rejection_Invoice_Filename);

            TempData["ConfirmationMessage"] = "For Order ID:"+Rejection_Order_ID+"\\nRejection  Reason Email is sent to USER:"+ Rejection_Uploaded_By;

            return Redirect("PO_INVOICE_PROCESS");
        }

        // THIS FUNCTION DOES CANCEL FOR REJECTION REASON FORM.

        public IActionResult OnPostCancelRejection()
        {
            flag_rejection_reason_form = false;

            return Redirect("PO_INVOICE_PROCESS");
        }
    }
}
