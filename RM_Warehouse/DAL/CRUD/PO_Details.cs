using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace DAL.CRUD
{

    // THIS CLASS IS FOR DATABASE UPDATES.

    public class PO_Details
    {

        // THIS FUNCTION FETCHES ALL INBOUND ORDERS WITHOUT INVOICE UPLOADED FOR GIVEN WAREHOUSE.
        // ORDER_STATUS IS "'CLOSED','RECEIVED'".

        public DataTable? GetOrdersWithoutInvoiceUpload(string WAREHOUSE, string ORDER_STATUS = "'CLOSED','RECEIVED'")
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            DataTable dt_items = new DataTable();
            string sql_items = @"select OH.[ORDER_ID],OH.[ORDER_DATE],OH.[ENTER_DATE],OH.[PONUMBER],OH.[ESTIMATED_ARRIVAL_DATE],VR.[Vendor_Name],OH.[ENTER_BY],OH.[ORDER_DESCRIPTION]
                                from ORDER_HEADER_INBOUND OH inner join Vendor VR on VR.Vendor_ID = OH.VENDOR_ID and OH.ORDER_STATUS IN (" + ORDER_STATUS + ")  and OH.WAREHOUSE='" + WAREHOUSE + "' and INVOICE_FILENAME is null;";

            SqlDataAdapter da_items = new SqlDataAdapter(sql_items, con);

            da_items.Fill(dt_items);

            con.Close();

            if (dt_items.Rows.Count == 0)
                return null;

            return dt_items;
        }


        // THIS FUNCTION FETCHES ALL RECEIVED ORDER DETAILS WITH ITEM COST=0.

        public DataTable? GetRemainingItems(long Order_ID, bool IS_RECEIVED = true)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            DataTable dt_items = new DataTable();
            string sql_items;

            sql_items = @"select OD.*,IM.[Item_Code],IM.[Item_Desc]
                                from ORDER_DETAILS_INBOUND OD
                                inner join Item_Master IM on IM.Item_ID=OD.ITEM_ID
                                where OD.ORDER_ID=" + Order_ID + " and IS_RECEIVED='" + IS_RECEIVED + "' and OD.COST=0;";

            SqlDataAdapter da_items = new SqlDataAdapter(sql_items, con);

            da_items.Fill(dt_items);

            con.Close();

            if (dt_items.Rows.Count == 0)
                return null;

            return dt_items;
        }

        // THIS FUNCTION UPDATES ORDER_DETAILS_INBOUND TABLE WITH COST,CURRENCY FIELDS FOR GIVEN ID.

        public void UpdateCostOfItem(long id, decimal COST, string CURRENCY)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_update_item = @"update ORDER_DETAILS_INBOUND set COST=" + COST + ",CURRENCY='" + CURRENCY + "'" +
                                    " where ID=" + id;
            SqlCommand cmd = new SqlCommand(sql_update_item, con);
            cmd.ExecuteNonQuery();

            con.Close();
        }

        // THIS FUNCTION UPDATES ORDER_HEADER_INBOUND TABLE WITH INVOICE_FILENAME,INVOICE_UPLOADED_BY 
        // FIELDS FOR GIVEN ORDER_ID.

        public void SavePO(long order_id, string filename,string user)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_update_item = @"update ORDER_HEADER_INBOUND set INVOICE_FILENAME='" + filename + "',INVOICE_UPLOADED_BY='" +user.ToUpper()+"' where ORDER_ID=" + order_id;
            SqlCommand cmd = new SqlCommand(sql_update_item, con);
            cmd.ExecuteNonQuery();

            con.Close();
        }

        // THIS FUNCTION FETCHES INBOUND ORDERS WITH INVOICE UPLOADED FOR GIVEN WAREHOUSE.
        // IT ALSO ACCEPTS FLAG IS_ACCOUNTS_EMAIL_SENT.
        // IF true,RETURNS RECORDS WITH ACCOUNTS EMAIL SENT.
        // ELSE,RETURNS RECORDS WITHOUT ACCOUNTS EMAIL SENT.

        public DataTable? GetOrdersWithInvoiceUpload(string WAREHOUSE,bool IS_ACCOUNTS_EMAIL_SENT=false, string ORDER_STATUS = "'CLOSED','RECEIVED'")
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            DataTable dt_items = new DataTable();
            string sql_items = @"select OH.[INVOICE_UPLOADED_BY],OH.[INVOICE_FILENAME],OH.[ORDER_ID],OH.[ORDER_DATE],OH.[ENTER_DATE],OH.[PONUMBER],OH.[ESTIMATED_ARRIVAL_DATE],VR.[Vendor_Name],OH.[ENTER_BY],OH.[ORDER_DESCRIPTION]
                                from ORDER_HEADER_INBOUND OH inner join Vendor VR on VR.Vendor_ID = OH.VENDOR_ID and OH.ORDER_STATUS IN (" + ORDER_STATUS + ") and OH.WAREHOUSE='" + WAREHOUSE + "' and INVOICE_FILENAME is not null and ISNULL(OH.IS_ACCOUNTS_EMAIL_SENT, 0) = '"+IS_ACCOUNTS_EMAIL_SENT+"';";

            SqlDataAdapter da_items = new SqlDataAdapter(sql_items, con);

            da_items.Fill(dt_items);

            con.Close();

            if (dt_items.Rows.Count == 0)
                return null;

            return dt_items;
        }

        // THIS FUNCTION FETCHES INBOUND DETAILS ITEMS FOR GIVEN ORDER_ID.IT ALSO HAS FLAG IS_RECEIVED.
        // IF true,RETURNS RECORDS WITH RECEIVED ITEMS.
        // ELSE,RETURNS RECORDS WITHOUT RECEIVED ITEMS.

        public DataTable? GetItems(long Order_ID, bool IS_RECEIVED = true)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            DataTable dt_items = new DataTable();
            string sql_items;

            sql_items = @"select OD.*,IM.[Item_Code],IM.[Item_Desc],(OD.RECEIVED_QUANTITY-ISNULL(OD.RETURN_QUANTITY,0)) as BALANCE_QUANTITY
                                from ORDER_DETAILS_INBOUND OD
                                inner join Item_Master IM on IM.Item_ID=OD.ITEM_ID
                                where OD.ORDER_ID=" + Order_ID + " and IS_RECEIVED='" + IS_RECEIVED + "'";

            SqlDataAdapter da_items = new SqlDataAdapter(sql_items, con);

            da_items.Fill(dt_items);

            con.Close();

            if (dt_items.Rows.Count == 0)
                return null;

            return dt_items;
        }

        // THIS FUNCTION UPDATES ORDER_HEADER_INBOUND TABLE WITH IS_ACCOUNTS_EMAIL_SENT=1 FOR GIVEN 
        // INBOUND ORDER_ID.

        public void UpdateEmailSend(long Order_ID)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_update_email_send = "UPDATE ORDER_HEADER_INBOUND set IS_ACCOUNTS_EMAIL_SENT=1 where ORDER_ID=" + Order_ID;
            SqlCommand cmd = new SqlCommand(sql_update_email_send, con);
            cmd.ExecuteNonQuery();

            con.Close();
        }

        // THIS FUNCTION UPDATES ORDER_HEADER_INBOUND TABLE WITH REJECTION_REASON,INVOICE_FILENAME,
        // REJECTION_INVOICE_FILENAME,IS_ACCOUNTS_EMAIL_SENT FIELDS FOR GIVEN ORDER_ID,REJECTION_REASON 
        // AND INVOICE_FILENAME.

        public void UpdateRejectionReason(long order_id,string rejection_reason,string invoice_filename)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_update_rejection_reason = "UPDATE ORDER_HEADER_INBOUND set REJECTION_REASON='"+rejection_reason+ "',INVOICE_FILENAME=null,REJECTION_INVOICE_FILENAME='"+invoice_filename+ "',IS_ACCOUNTS_EMAIL_SENT=0 where ORDER_ID=" + order_id;
            SqlCommand cmd = new SqlCommand(sql_update_rejection_reason, con);
            cmd.ExecuteNonQuery();

            con.Close();
        }
    }
}
