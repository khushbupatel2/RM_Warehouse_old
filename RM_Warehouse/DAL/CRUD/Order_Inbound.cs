using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DAL.CRUD
{

    // THIS CLASS IS FOR UPDATING DATABSE.

    public class Order_Inbound
    {

        // THIS FUNCTION INSERTS NEW RECORD INTO ORDER_HEADER_INBOUND TABLE.ALL FIELDS ARE PASSED TO THIS FUNCTION.
        // THIS FUNCTION RETURNS NEWLY GENERATED ORDER_ID FOR INSERTED RECORD.


        public long InsertNewOrder(DateTime ORDER_DATE,string PONUMBER,DateTime ESTIMATED_ARRIVAL_DATE,long VENDOR_ID,string ENTER_BY,string WAREHOUSE,string ORDER_DESCRIPTION)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_order_id = @"insert into ORDER_HEADER_INBOUND([ORDER_DATE],[ORDER_STATUS],[ENTER_DATE],[PONUMBER],[ESTIMATED_ARRIVAL_DATE]
                                    ,[VENDOR_ID],[ENTER_BY],[WAREHOUSE],[ORDER_DESCRIPTION]) output INSERTED.ORDER_ID values('" + ORDER_DATE.ToString("yyyy-MM-dd HH:mm:ss") + "','CREATED',GETDATE(),'"+PONUMBER+"','" + ESTIMATED_ARRIVAL_DATE.ToString("yyyy-MM-dd HH:mm:ss")+"',"+VENDOR_ID+",'"+ENTER_BY.ToUpper()+"','"+WAREHOUSE.ToUpper()+"','"+ORDER_DESCRIPTION+"')";
            SqlCommand sqlCommand=new SqlCommand(sql_order_id, con);
            long new_id = (long)sqlCommand.ExecuteScalar();

            con.Close();

            return new_id;

        }

        // THIS FUNCTION INSERTS NEW ITEM DETAILS RECORD INTO ORDER_DETAILS_INBOUND TABLE.ALL FIELDS ARE PASSED 
        // TO THIS FUNCTION.

        public void InsertNewItem(long ORDER_ID,int ITEM_ID,int ORDERED_QUANTITY,decimal COST,string CURRENCY)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_new_item = @"insert into ORDER_DETAILS_INBOUND(ORDER_ID,ITEM_ID,ORDERED_QUANTITY,COST,CURRENCY) values(
                                    " + ORDER_ID + "," + ITEM_ID + "," + ORDERED_QUANTITY + ","+COST+",'"+CURRENCY+"')";
            SqlCommand cmd=new SqlCommand(sql_new_item, con);
            cmd.ExecuteNonQuery();

            con.Close();
        }

        // THIS FUNCTION FETCHES ALL DETAILS ITEMS FOR GIVEN ORDER_ID.IT ALSO RECEIVES FLAGS IS_RECEIVED AND 
        // flag_closed_order.WHEN IS_RECEIVED=false THEN SHOW ALL ITEMS DETAILS.
        // WHEN IS_RECEIVED=true THEN SHOW ONLY ITEM DETAILS WHERE RECEIVED_QUANTITY>QUANTITY_PLACED
        // WHEN flag_closed_order=true THEN SHOW ALL ITEMS DETAILS WITH IS_RETURN is null CONDITION.

        public DataTable? GetItems(long Order_ID,bool IS_RECEIVED=false,bool flag_closed_order=false)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            DataTable dt_items = new DataTable();
            string sql_items;

            if(IS_RECEIVED==false)
                sql_items = @"select OD.ID,OD.ORDER_ID,IM.[Item_ID],IM.[Item_Code],IM.[Item_Desc],OD.[ORDERED_QUANTITY],OD.[COST],OD.[CURRENCY],OD.[RECEIVED_QUANTITY],OD.[QUANTITY_PLACED],OD.[RECEIVED_BY],OD.[RECEIVED_DATE],OD.[EXPIRY_DATE]
                                from ORDER_DETAILS_INBOUND OD
                                inner join Item_Master IM on IM.Item_ID=OD.ITEM_ID
                                where OD.ORDER_ID=" + Order_ID +" and IS_RECEIVED='"+IS_RECEIVED+ "'";
            else
                // IF ALL QUANTITY IS PLACED THEN DON'T SHOW THIS RECORD.
                sql_items = @"select OD.ID,OD.ORDER_ID,IM.[Item_ID],IM.[Item_Code],IM.[Item_Desc],OD.[ORDERED_QUANTITY],OD.[COST],OD.[CURRENCY],OD.[RECEIVED_QUANTITY],OD.[QUANTITY_PLACED],OD.[RECEIVED_BY],OD.[RECEIVED_DATE],OD.[EXPIRY_DATE]
                                from ORDER_DETAILS_INBOUND OD
                                inner join Item_Master IM on IM.Item_ID=OD.ITEM_ID
                                where OD.ORDER_ID=" + Order_ID + " and IS_RECEIVED='" + IS_RECEIVED + "' and RECEIVED_QUANTITY>QUANTITY_PLACED";
            
            if(flag_closed_order==true)
                sql_items = @"select OD.ID,OD.ORDER_ID,IM.[Item_ID],IM.[Item_Code],IM.[Item_Desc],OD.[ORDERED_QUANTITY],OD.[COST],OD.[CURRENCY],OD.[RECEIVED_QUANTITY],OD.[QUANTITY_PLACED],OD.[RECEIVED_BY],OD.[RECEIVED_DATE],OD.[EXPIRY_DATE]
                                from ORDER_DETAILS_INBOUND OD
                                inner join Item_Master IM on IM.Item_ID=OD.ITEM_ID
                                where OD.ORDER_ID=" + Order_ID + " and IS_RETURN is null";


            SqlDataAdapter da_items=new SqlDataAdapter(sql_items, con);

            da_items.Fill(dt_items);

            con.Close();

            if (dt_items.Rows.Count == 0)
                return null;

            return dt_items;
        }

        // THIS FUNCTION DELETS AN ITEM DETAIL RECORD FROM ORDER_DETAILS_INBOUND TABLE FOR GIVEN ID

        public void DeleteItem(long id)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_delete_item = @"delete from ORDER_DETAILS_INBOUND where ID="+id;
            SqlCommand cmd = new SqlCommand(sql_delete_item, con);
            cmd.ExecuteNonQuery();

            con.Close();
        }

        // THIS FUNCTION FETCHES ALL INBOUND ORDERS WITH GIVEN ORDER_STATUS. 

        public DataTable? GetOrders(string WAREHOUSE,string ORDER_STATUS = "CREATED")
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            DataTable dt_items = new DataTable();
            string sql_items = @"select OH.[ORDER_ID],OH.[ORDER_DATE],OH.[ENTER_DATE],OH.[PONUMBER],OH.[ESTIMATED_ARRIVAL_DATE],VR.[Vendor_Name],OH.[ENTER_BY],OH.[ORDER_DESCRIPTION]
                                from ORDER_HEADER_INBOUND OH inner join Vendor VR on VR.Vendor_ID = OH.VENDOR_ID and OH.ORDER_STATUS='" + ORDER_STATUS+"' and OH.WAREHOUSE='"+WAREHOUSE+"';";
                                
            SqlDataAdapter da_items = new SqlDataAdapter(sql_items, con);

            da_items.Fill(dt_items);

            con.Close();

            if (dt_items.Rows.Count == 0)
                return null;
            
            return dt_items;
        }

        // THIS FUNCTION UPDATES AN ITEM DETAILS RECORD FROM ORDER_DETAILS_INBOUND TABLE WITH FIELDS ITEM_ID,
        // ORDERED_QUANTITY,COST,CURRENCY FOR GIVEN ID.

        public void UpdateItem(long id,int ITEM_ID, int ORDERED_QUANTITY,decimal COST,string CURRENCY)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_update_item = @"update ORDER_DETAILS_INBOUND set ITEM_ID="+ITEM_ID+",ORDERED_QUANTITY="+ORDERED_QUANTITY+",COST="+COST+",CURRENCY='"+CURRENCY+"'"+
                                    " where ID="+id;
            SqlCommand cmd = new SqlCommand(sql_update_item, con);
            cmd.ExecuteNonQuery();

            con.Close();
        }

       
        // THIS FUNCTION UPDATES AN INBOUND ORDER WITH FIELDS PASSED TO THIS FUNCTION.

        public void UpdateOrder(long ORDER_ID,DateTime ORDER_DATE, string PONUMBER, DateTime ESTIMATED_ARRIVAL_DATE, long VENDOR_ID, string ENTER_BY,string ORDER_DESCRIPTION)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_update_order = @"update ORDER_HEADER_INBOUND set ORDER_DATE='" + ORDER_DATE.ToString("yyyy-MM-dd HH:mm:ss") + "',PONUMBER='" + PONUMBER + "',ESTIMATED_ARRIVAL_DATE='" + ESTIMATED_ARRIVAL_DATE.ToString("yyyy-MM-dd HH:mm:ss") + "',VENDOR_ID=" + VENDOR_ID + ",ENTER_BY='" + ENTER_BY.ToUpper() + "',ORDER_DESCRIPTION='"+ORDER_DESCRIPTION+"' where ORDER_ID=" + ORDER_ID;
                                    
            SqlCommand sqlCommand = new SqlCommand(sql_update_order, con);
            sqlCommand.ExecuteNonQuery();

            con.Close();

        }

        // THIS FUNCTION FETCHES SINGLE RECORD FOR GIVEN ORDER_ID FROM ORDER_HEADER_INBOUND TABLE.

        public DataRow? GetOrderById(long ORDER_ID)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            DataTable dt_order = new DataTable();
            string sql_order_by_id = @"select * from ORDER_HEADER_INBOUND where ORDER_ID="+ORDER_ID;

            SqlDataAdapter da_order = new SqlDataAdapter(sql_order_by_id, con);

            da_order.Fill(dt_order);

            con.Close();

            if (dt_order.Rows.Count == 0)
                return null;

            return dt_order.Rows[0];
        }

        // THIS FUNCTION UPDATES AN ITEM DETAILS RECORD WITH RECEIVED_QUANTITY,RECEIVED_BY,RECEIVED_DATE,
        // EXPIRY_DATE FIELDS.IT ALSO SETS IS_RECEIVED=1 FOR GIVEN DETAILS RECORD.

        public void UpdateReceivedItem(long order_id,long id,int RECEIVED_QUANTITY, string RECEIVED_BY, DateTime RECEIVED_DATE, DateTime? EXPIRY_DATE)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string expiry_date;
            if (EXPIRY_DATE == null)
                expiry_date = "null";
            else
                expiry_date = "'" + EXPIRY_DATE?.ToString("yyyy-M-d") + "'";

            string sql_update_received_item = @"update ORDER_DETAILS_INBOUND set RECEIVED_QUANTITY=" + RECEIVED_QUANTITY + ",RECEIVED_BY='" + RECEIVED_BY.ToUpper() + "',RECEIVED_DATE='" + RECEIVED_DATE.ToString("yyyy-MM-dd HH:mm:ss") + "',EXPIRY_DATE=" +expiry_date  + ",IS_RECEIVED=1 "+
                                    " where ID=" + id+" and ORDER_ID="+order_id;
            SqlCommand cmd = new SqlCommand(sql_update_received_item, con);
            cmd.ExecuteNonQuery();

            con.Close();
        }

        // THIS FUNCTION UPDATES AN INBOUND ORDER FOR GIVEN ORDER_ID WITH GIVEN ORDER_STATUS.

        public void UpdateReceiveOrder(long order_id,string order_status="RECEIVED")
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_receive_order = "update ORDER_HEADER_INBOUND set ORDER_STATUS='"+order_status+"' where ORDER_ID=" + order_id;
            SqlCommand cmd = new SqlCommand(sql_receive_order, con);
            cmd.ExecuteNonQuery();

            con.Close();

        }
    }
}
