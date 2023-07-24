using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace DAL.CRUD
{

    // THIS CLASS IS FOR UPDATING DATABSE.

    public class Order_Outbound
    {

        // THIS FUNCTION INSERTS NEW RECORD INTO ORDER_HEADER_OUTBOUND TABLE.ALL FIELDS ARE PASSED TO THIS FUNCTION.
        // THIS FUNCTION RETURNS NEWLY GENERATED ORDER_ID FOR INSERTED RECORD.

        public long InsertNewOrder(DateTime ORDER_DATE, string PONUMBER, string ENTER_BY,string WAREHOUSE)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_order_id = @"insert into ORDER_HEADER_OUTBOUND([ORDER_DATE],[ORDER_STATUS],[ENTER_DATE],[PONUMBER],[ENTER_BY],[WAREHOUSE]) output INSERTED.ORDER_ID values('" + ORDER_DATE.ToString("yyyy-MM-dd HH:mm:ss") + "','OPEN',GETDATE(),'" + PONUMBER + "','" + ENTER_BY.ToUpper() + "','"+WAREHOUSE.ToUpper()+"');";
            SqlCommand sqlCommand = new SqlCommand(sql_order_id, con);
            long new_id = (long)sqlCommand.ExecuteScalar();

            con.Close();

            return new_id;

        }

        // THIS FUNCTION INSERTS NEW ITEM DETAILS RECORD INTO ORDER_DETAILS_OUTBOUND TABLE.ALL FIELDS ARE PASSED 
        // TO THIS FUNCTION.

        public void InsertNewItem(long ORDER_ID, int ITEM_ID, int ORDERED_QUANTITY,decimal PRICE,string CURRENCY)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_new_item = @"insert into ORDER_DETAILS_OUTBOUND(ORDER_ID,ITEM_ID,ORDERED_QUANTITY,UNIT_PRICE,CURRENCY) values(
                                    " + ORDER_ID + "," + ITEM_ID + "," + ORDERED_QUANTITY + ","+PRICE+",'"+CURRENCY+"')";
            SqlCommand cmd = new SqlCommand(sql_new_item, con);
            cmd.ExecuteNonQuery();

            con.Close();
        }

        // THIS FUNCTION FETCHES ALL DETAILS ITEMS FOR GIVEN ORDER_ID.IT ALSO RECEIVES FLAGS IS_RECEIVED AND 
        // Show_Picked.WHEN IS_RECEIVED=false THEN SHOW ALL ITEMS DETAILS.
        // WHEN IS_RECEIVED=true THEN SHOW ONLY ITEM DETAILS WHERE  ORDERED_QUANTITY>PICKED_QUANTITY
        // WHEN Show_Picked=true THEN SHOW ALL ITEMS DETAILS.


        public DataTable? GetItems(long Order_ID, bool IS_RECEIVED = false,bool Show_Picked=false)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            DataTable dt_items = new DataTable();
            string sql_items;

            if (IS_RECEIVED == false)
                sql_items = @"select OD.UNIT_PRICE,OD.CURRENCY,OD.IS_RECEIVED,OD.ID,OD.ORDER_ID,IM.[Item_ID],IM.[Item_Code],IM.[Item_Desc],OD.[ORDERED_QUANTITY],OD.[PICKED_QUANTITY],OD.[PICKED_BY],OD.[PICKED_DATE]
                                from ORDER_DETAILS_OUTBOUND OD
                                inner join Item_Master IM on IM.Item_ID=OD.ITEM_ID
                                where OD.ORDER_ID=" + Order_ID + " and IS_RECEIVED='" + IS_RECEIVED + "'";
            else
                // IF ALL QUANTITY IS PICKED THEN DON'T SHOW THIS RECORD.
                sql_items = @"select OD.UNIT_PRICE,OD.CURRENCY,OD.IS_RECEIVED,OD.ID,OD.ORDER_ID,IM.[Item_ID],IM.[Item_Code],IM.[Item_Desc],OD.[ORDERED_QUANTITY],OD.[PICKED_QUANTITY],OD.[PICKED_BY],OD.[PICKED_DATE]
                                from ORDER_DETAILS_OUTBOUND OD
                                inner join Item_Master IM on IM.Item_ID=OD.ITEM_ID
                                where OD.ORDER_ID=" + Order_ID + " and IS_RECEIVED='" + IS_RECEIVED + "' and ORDERED_QUANTITY>PICKED_QUANTITY";
            
            // SHOW PICKED RECORDS ALSO
            
            if(Show_Picked==true)
            {
                sql_items = @"select OD.UNIT_PRICE,OD.CURRENCY,OD.IS_RECEIVED,OD.ID,OD.ORDER_ID,IM.[Item_ID],IM.[Item_Code],IM.[Item_Desc],OD.[ORDERED_QUANTITY],OD.[PICKED_QUANTITY],OD.[PICKED_BY],OD.[PICKED_DATE]
                                from ORDER_DETAILS_OUTBOUND OD
                                inner join Item_Master IM on IM.Item_ID=OD.ITEM_ID
                                where OD.ORDER_ID=" + Order_ID + " and IS_RECEIVED='" + IS_RECEIVED + "'";
            }
            
            SqlDataAdapter da_items = new SqlDataAdapter(sql_items, con);

            da_items.Fill(dt_items);

            con.Close();

            if (dt_items.Rows.Count == 0)
                return null;

            return dt_items;
        }

        // THIS FUNCTION DELETS AN ITEM DETAIL RECORD FROM ORDER_DETAILS_OUTBOUND TABLE FOR GIVEN ID

        public void DeleteItem(long id)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_delete_item = @"delete from ORDER_DETAILS_OUTBOUND where ID=" + id;
            SqlCommand cmd = new SqlCommand(sql_delete_item, con);
            cmd.ExecuteNonQuery();

            con.Close();
        }

        // THIS FUNCTION FETCHES ALL OUTBOUND ORDERS WITH GIVEN ORDER_STATUS.

        public DataTable? GetOrders(string WAREHOUSE,string ORDER_STATUS = "OPEN")
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            DataTable dt_items = new DataTable();
            string sql_items = @"select OH.[ORDER_STATUS],OH.[ORDER_ID],OH.[ORDER_DATE],OH.[ENTER_DATE],OH.[PONUMBER],OH.[ENTER_BY]
                                from ORDER_HEADER_OUTBOUND OH where OH.ORDER_STATUS='" + ORDER_STATUS + "' and WAREHOUSE='"+WAREHOUSE+"';";

            SqlDataAdapter da_items = new SqlDataAdapter(sql_items, con);

            da_items.Fill(dt_items);

            con.Close();

            if (dt_items.Rows.Count == 0)
                return null;

            return dt_items;
        }

        // THIS FUNCTION UPDATES AN ITEM DETAILS RECORD FROM ORDER_DETAILS_OUTBOUND TABLE WITH FIELDS ITEM_ID,
        // ORDERED_QUANTITY,PRICE,CURRENCY FOR GIVEN ID.

        public void UpdateItem(long id, int ITEM_ID, int ORDERED_QUANTITY,decimal PRICE,string CURRENCY)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_update_item = @"update ORDER_DETAILS_OUTBOUND set ITEM_ID=" + ITEM_ID + ",ORDERED_QUANTITY=" + ORDERED_QUANTITY + ",UNIT_PRICE="+PRICE+",CURRENCY='"+CURRENCY+"' where ID=" + id;
            SqlCommand cmd = new SqlCommand(sql_update_item, con);
            cmd.ExecuteNonQuery();

            con.Close();
        }

        // THIS FUNCTION UPDATES AN OUTBOUND ORDER WITH FIELDS PASSED TO THIS FUNCTION.

        public void UpdateOrder(long ORDER_ID, DateTime ORDER_DATE, string PONUMBER, string ENTER_BY)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_update_order = @"update ORDER_HEADER_OUTBOUND set ORDER_DATE='" + ORDER_DATE.ToString("yyyy-MM-dd HH:mm:ss") + "',PONUMBER='" + PONUMBER + "',ENTER_BY='" + ENTER_BY.ToUpper() + "' where ORDER_ID=" + ORDER_ID;

            SqlCommand sqlCommand = new SqlCommand(sql_update_order, con);
            sqlCommand.ExecuteNonQuery();

            con.Close();

            
        }

        // THIS FUNCTION FETCHES SINGLE RECORD FOR GIVEN ORDER_ID FROM ORDER_HEADER_OUTBOUND TABLE.

        public DataRow? GetOrderById(long ORDER_ID)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            DataTable dt_order = new DataTable();
            string sql_order_by_id = @"select * from ORDER_HEADER_OUTBOUND where ORDER_ID=" + ORDER_ID;

            SqlDataAdapter da_order = new SqlDataAdapter(sql_order_by_id, con);

            da_order.Fill(dt_order);

            con.Close();

            if (dt_order.Rows.Count == 0)
                return null;

            return dt_order.Rows[0];
        }

        // THIS FUNCTION UPDATES ORDER_STATUS WITH GIVEN order_status FOR GIVEN ORDER_ID order_id.

        public void UpdatePickOrder(long order_id, string order_status = "PICK")
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_pick_order = "update ORDER_HEADER_OUTBOUND set ORDER_STATUS='" + order_status + "' where ORDER_ID=" + order_id;
            SqlCommand cmd = new SqlCommand(sql_pick_order, con);
            cmd.ExecuteNonQuery();

            con.Close();

        }

        // THIS FUNCTION UPDATES AN OUTBOUND ORDER WITH ORDER_STATUS='COMPLETE'.IT UPDATES FIELDS COMPLETED_BY,
        // COMPLETED_DATE FOR GIVEN OUTBOUND ORDER.

        public void UpdateCompleteOrder(long order_id, string completed_by,string order_status = "COMPLETE")
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_complete_order = "update ORDER_HEADER_OUTBOUND set ORDER_STATUS='" + order_status + "',COMPLETED_BY='"+completed_by.ToUpper()+"',COMPLETED_DATE=GETDATE() where ORDER_ID=" + order_id;
            SqlCommand cmd = new SqlCommand(sql_complete_order, con);
            cmd.ExecuteNonQuery();

            con.Close();

        }

        // THIS FUNCTION FETCHES ITEM_ID,ITEM_CODE,ITEM_DESC,QTY_IN_HAND FOR ALL ITEMS PRESENT IN DATABASE FOR
        // GIVEN WAREHOUSE.

        public DataTable? ItemsAvailable_ItemCode_Description(string warehouse)
        {
            Warehouse wh = new Warehouse();
            int warehouse_id = wh.GetWarehouse_ID(warehouse);

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_items_available = @"select ITM.Item_ID,Item_Code,ITM.Item_Desc,(ISNULL(sum(QTY_In_Hand),0)-(select ISNULL(sum(ORDERED_QUANTITY),0)
from ORDER_DETAILS_OUTBOUND ODO inner join ORDER_HEADER_OUTBOUND OHO on ODO.ORDER_ID=OHO.ORDER_ID and OHO.ORDER_STATUS='OPEN' 
and ODO.ITEM_ID=ITM.Item_ID))as QUANTITY_AVAILABLE from Item_Master ITM left join Inventory_Master IM on ITM.Item_ID=IM.Item_ID 
 
left join Location_Master LM on IM.Location_ID=LM.Location_ID left join Warehouse_Master WM on WM.Warehouse_ID=LM.Warehouse_ID 
and WM.Name='"+warehouse+"' where ITM.Warehouse_ID="+warehouse_id+" group by Item_Code,Item_Desc,ITM.Item_ID order by Item_Code";

            SqlDataAdapter da = new SqlDataAdapter(sql_items_available,con);
            DataTable dt = new DataTable();

            da.Fill(dt);

            if (dt.Rows.Count == 0)
                return null;
            else
                return dt;
        }

    }
}
