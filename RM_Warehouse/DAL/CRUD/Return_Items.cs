using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DAL.CRUD
{
    // THIS CLASS IS FOR DATABASE UPDATES.

    public class Return_Items
	{
        // RETURN FROM GARAGE PAGE.
        // THIS FUNCTION FETCHES OUTBOUND ORDER DEATILS WITH ORDER_STATUS='COMPLETE' FOR GIVEN ITEM_ID
        // AND WAREHOUSE.IT ALSO CHECKS FLAG ODO.IS_RETURN is null. 

        public DataTable? GetOutboundOrderDetailsByItemID(int itemID,string warehouse)
		{
			string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

			SqlConnection con = new SqlConnection(conn_str);
			con.Open();
			string sql_all_items = "select ODO.*,IM.[Item_Code],IM.[Item_Desc],OHO.[PONUMBER] from ORDER_DETAILS_OUTBOUND ODO " +
                                    "inner join Item_Master IM on ODO.ITEM_ID=IM.Item_ID " +
									"inner join ORDER_HEADER_OUTBOUND OHO on ODO.ORDER_ID=OHO.ORDER_ID "+
									"where ODO.ITEM_ID=" +itemID+" and OHO.ORDER_STATUS='COMPLETE' " +
									"and OHO.WAREHOUSE='"+warehouse+"' and ODO.IS_RETURN is null  order by ODO.PICKED_DATE desc";
			
			SqlDataAdapter da = new(sql_all_items, con);
			DataTable dt = new DataTable();
			da.Fill(dt);

			con.Close();

			if (dt.Rows.Count == 0)
				return null;
			return dt;
		}

        // RETURN FROM GARAGE
        // THIS FUNCTION UPDATES ORDER_DETAILS_OUTBOUND TABLE WITH RETURN_QUANTITY,IS_RETURN=1 FIELDS
        // FOR GIVEN DETAILS_ID.
        // IT INSERTS LOGS INTO RETURN_ORDER_HEADER AND RETURN_ORDER_DETAILS TABLES.
        // IT INSERTS NEW INBOUND ORDER FOR RETURN ITEM.
        // IT INSERTS ITEM INTO Inventory_Master table.

        public void UpdateReturnItemFromGarage(long order_id,long details_id,int return_quantity,string user,int location_id,int item_id,string warehouse,decimal unit_price,string currency,string rm_ponumber)
		{
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

			// UPDATE RETURN QUANTITY IN "ORDER_DETAILS_OUTBOUND" TABLE

			string sql_1 = "update ORDER_DETAILS_OUTBOUND set RETURN_QUANTITY=" + return_quantity + ",IS_RETURN=1 where ID=" + details_id;
			SqlCommand cmd_1=new SqlCommand(sql_1, con);
			cmd_1.ExecuteNonQuery();


            // INSERT INTO RETURN LOGS
            // 1.ORDER_HEADER_OUTBOUND INTO RETURN_ORDER_HEADER
            // 2.ORDER_DETAILS_OUTBOUND INTO RETURN_ORDER_DETAILS

            string sql_2 = "insert into RETURN_ORDER_HEADER(MASTER_ORDER_ID,MASTER_ORDER_TYPE,MASTER_COMMENTS,RETURN_DATE,RETURN_BY) OUTPUT INSERTED.RETURN_ORDER_ID " +
							" values ("+order_id+ ",'OUTBOUND','RETURN FROM GARAGE',GETDATE(),'"+user.ToUpper()+"')";
            SqlCommand cmd_2 = new SqlCommand(sql_2, con);
            int return_order_id=(int)cmd_2.ExecuteScalar();

            string sql_3 = "insert into RETURN_ORDER_DETAILS(MASTER_DETAILS_ID,RETURN_ORDER_ID,RETURN_QUANTITY)" +
				" values ("+details_id+","+return_order_id+","+return_quantity+")";
            SqlCommand cmd_3 = new SqlCommand(sql_3, con);
            cmd_3.ExecuteNonQuery();

            //INSERT NEW INBOUND ORDER FOR RETURN ITEM

            string rm_ponumber_1 = rm_ponumber + "-RT";

            string sql_order_id = @"insert into ORDER_HEADER_INBOUND([ORDER_DATE],[ORDER_STATUS],[ENTER_DATE],[PONUMBER],[ESTIMATED_ARRIVAL_DATE]
                                    ,[ENTER_BY],[WAREHOUSE],[ORDER_DESCRIPTION],[RETURN_RM_PONUMBER]) output INSERTED.ORDER_ID values(GETDATE(),'RETURN',GETDATE(),'RETURN',GETDATE(),'" + user.ToUpper() + "','" + warehouse.ToUpper() + "','RETURN FROM GARAGE','"+rm_ponumber_1+"')";
            SqlCommand sqlCommand = new SqlCommand(sql_order_id, con);
            long new_order_id = (long)sqlCommand.ExecuteScalar();


            string sql_new_item = @"insert into ORDER_DETAILS_INBOUND(ORDER_ID,ITEM_ID,ORDERED_QUANTITY,COST,CURRENCY,RETURN_QUANTITY,IS_RETURN) output INSERTED.ID 
                                    values(" + new_order_id + "," + item_id + ",0,"+unit_price+",'" + currency + "',"+return_quantity+",1)";
            SqlCommand cmd = new SqlCommand(sql_new_item, con);
            long new_details_id=(long)cmd.ExecuteScalar();

            // INSERT ITEM INTO Inventory_Master table

            string sql_4 = "insert into Inventory_Master([Location_ID],[Item_ID],[QTY_In_Hand],[Details_ID])" +
                " values (" + location_id + "," + item_id + "," + return_quantity + ","+new_details_id+")";
            SqlCommand cmd_4 = new SqlCommand(sql_4, con);
            cmd_4.ExecuteNonQuery();


            con.Close();
        }


        // RETURN TO VENDOR
        // THIS FUNCTION INSERTS LOGS INTO RETURN_ORDER_HEADER AND RETURN_ORDER_DETAILS TABLES FOR
        // RETURN INBOUND ORDER.
        // IT INSERTS NEW OUTBOUND ORDER WITH ORDER_STATUS='RETURN'.ALSO, PONUMBER IS APPENDED WITH "-RT". 
        // IT INSERTS NEW OUTBOUND DETAILS WITH RETURN_QUANTITY,IS_RETURN=1,ITEM_ID,UNIT_PRICE,CURRENCY FIELDS. 


        public void ReturnToVendor(long INBOUND_ORDER_ID,DateTime ORDER_DATE, string PONUMBER, string ENTER_BY, string WAREHOUSE,int ITEM_ID, decimal PRICE, string CURRENCY,int RETURN_QUANTITY,long DETAILS_ID)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            // INSERT INTO RETURN LOGS
            // 1.ORDER_HEADER_INBOUND INTO RETURN_ORDER_HEADER
            // 2.ORDER_DETAILS_INBOUND INTO RETURN_ORDER_DETAILS

            string sql_2 = "insert into RETURN_ORDER_HEADER(MASTER_ORDER_ID,MASTER_ORDER_TYPE,MASTER_COMMENTS,RETURN_DATE,RETURN_BY) OUTPUT INSERTED.RETURN_ORDER_ID " +
                            " values (" + INBOUND_ORDER_ID + ",'INBOUND','RETURN TO VENDOR',GETDATE(),'" + ENTER_BY.ToUpper() + "')";
            SqlCommand cmd_2 = new SqlCommand(sql_2, con);
            int return_order_id = (int)cmd_2.ExecuteScalar();

            string sql_3 = "insert into RETURN_ORDER_DETAILS(MASTER_DETAILS_ID,RETURN_ORDER_ID,RETURN_QUANTITY)" +
                " values (" + DETAILS_ID + "," + return_order_id + "," + RETURN_QUANTITY + ")";
            SqlCommand cmd_3 = new SqlCommand(sql_3, con);
            cmd_3.ExecuteNonQuery();


            // INSERTING NEW OUTBOUND ORDER
            
            string ponumber_1 = PONUMBER + "-RT";

            string sql_order_id = @"insert into ORDER_HEADER_OUTBOUND([ORDER_DATE],[ORDER_STATUS],[ENTER_DATE],[PONUMBER],[ENTER_BY],[WAREHOUSE]) output INSERTED.ORDER_ID values('" + ORDER_DATE.ToString("yyyy-MM-dd HH:mm:ss") + "','RETURN',GETDATE(),'" + ponumber_1 + "','" + ENTER_BY.ToUpper() + "','" + WAREHOUSE.ToUpper() + "');";
            SqlCommand sqlCommand = new SqlCommand(sql_order_id, con);
            long new_id = (long)sqlCommand.ExecuteScalar();


            // INSERTING NEW OUTBOUND DETAILS

            string sql_new_item = @"insert into ORDER_DETAILS_OUTBOUND(ORDER_ID,ITEM_ID,ORDERED_QUANTITY,UNIT_PRICE,CURRENCY,RETURN_QUANTITY,IS_RETURN) values(
                                    " + new_id + "," + ITEM_ID + ",0," + PRICE + ",'" + CURRENCY + "',"+RETURN_QUANTITY+",1)";
            SqlCommand cmd = new SqlCommand(sql_new_item, con);
            cmd.ExecuteNonQuery();
            
            con.Close();

        }

        // RETURN TO VENDOR.
        // THIS FUNCTION UPDATES Inventory_Master TABLE WITH QTY_In_Hand=QTY_In_Hand - items_returned FOR
        // GIVEN INV_ID.
        // IT UPDATES ORDER_DETAILS_INBOUND TABLE WITH RETURN_QUANTITY,IS_RETURN=1 FIELDS FOR
        // GIVEN DETAILS_ID.
         

        public void RemoveInventory(long inv_id, long details_id, int Location_ID, int Item_ID, int items_returned, DateTime? Expiry_Date, string user)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string expiry_date;
            if (Expiry_Date == null)
                expiry_date = "null";
            else
                expiry_date = "'" + Expiry_Date?.ToString("yyyy-M-d") + "'";

            // UPDATE Inventory_Master

            string sql_items_returned = "update Inventory_Master set QTY_In_Hand=QTY_In_Hand-" + items_returned + " where Inv_ID=" + inv_id;
            SqlCommand sqlCommand_1 = new SqlCommand(sql_items_returned, con);
            sqlCommand_1.ExecuteNonQuery();


            // UPDATE RETURN_QUANTITY,IS_RETURN=1 IN ORDER_DETAILS_INBOUND TABLE

            string sql_qty_returned = "update ORDER_DETAILS_INBOUND set RETURN_QUANTITY=" + items_returned + ",IS_RETURN=1 where ID=" + details_id;

            SqlCommand sqlCommand_2 = new SqlCommand(sql_qty_returned, con);
            sqlCommand_2.ExecuteNonQuery();

           
           con.Close();
        }

        // RETURN TO VENDOR.
        // THIS FUNCTION FETCHES LOCATIONS WITH INVENTORY FOR GIVEN ORDER_ID,ITEM_ID AND WAREHOUSE.
        // ALSO,QTY_IN_HAND > 0 CONDITION.

        public DataTable? Get_Available_Items_and_Locations(long order_id, long item_id, string warehouse)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_items_available = @"select IM.Inv_ID,ODI.ID,ODI.ORDER_ID,ODI.ORDERED_QUANTITY,LM.Location_ID,LM.Location_Code,LM.Warehouse_ID,IM.QTY_In_Hand,IM.Expiry_Date,
                                        ODI.RECEIVED_QUANTITY,ODI.RECEIVED_BY,ODI.RECEIVED_DATE,
                                        ITEM.Item_Code,ITEM.Item_Desc,ITEM.Item_ID    
                                        from Inventory_Master IM " +
                " inner join Location_Master LM on IM.Location_ID=LM.Location_ID " +
                " inner join Warehouse_Master WM on WM.Warehouse_ID=LM.Warehouse_ID " +
                " inner join Item_Master ITEM on ITEM.Item_ID=IM.Item_ID " +
                " inner join ORDER_DETAILS_INBOUND ODI on IM.Item_ID=ODI.ITEM_ID " +
                " where ODI.ORDER_ID=" + order_id + " and IM.Item_ID=" + item_id + " and WM.Name='" + warehouse + "' and IM.QTY_In_Hand>0 order by IM.Expiry_Date,Location_Code";
            DataTable dt = new DataTable();
            SqlDataAdapter da = new(sql_items_available, con);
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            else
                return dt;

        }

        // RETURN TO VENDOR.
        // THIS FUNCTION FETCHES ALL INBOUND DETAILS FOR GIVEN ITEM_ID AND WAREHOUSE.
        // ALSO,IT CHECKS FOR ORDER_STATUS='CLOSED' AND IS_RETURN is null.


        public DataTable? GetInboundOrderDetailsByItemID(int itemID, string warehouse)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_all_items = "select ODI.*,IM.[Item_Code],IM.[Item_Desc],OHI.[PONUMBER] from ORDER_DETAILS_INBOUND ODI " +
                                    "inner join Item_Master IM on ODI.ITEM_ID=IM.Item_ID " +
                                    "inner join ORDER_HEADER_INBOUND OHI on ODI.ORDER_ID=OHI.ORDER_ID " +
                                    "where ODI.ITEM_ID=" + itemID + " and OHI.ORDER_STATUS='CLOSED' " +
                                    "and OHI.WAREHOUSE='" + warehouse + "' and ODI.IS_RETURN is null  order by ODI.RECEIVED_DATE desc";

            SqlDataAdapter da = new(sql_all_items, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }
    }
}
