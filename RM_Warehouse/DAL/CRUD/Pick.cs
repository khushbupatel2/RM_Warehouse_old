using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace DAL.CRUD
{

    // THIS CLASS IS FOR DATABASE UPDATES.

    public class Pick
    {

        // THIS FUNCTION RETURNS QUANTITY_AVAILABLE FOR GIVEN ITEM_ID FOR GIVEN WAREOUSE.

        public int ItemsAvailable(long item_id,string warehouse)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_items_available = @"select sum(QTY_In_Hand) as QUANTITY_AVAILABLE from Inventory_Master IM " +
                " inner join Location_Master LM on IM.Location_ID=LM.Location_ID " +
                " inner join Warehouse_Master WM on WM.Warehouse_ID=LM.Warehouse_ID " +
                " where IM.Item_ID=" + item_id+ " and WM.Name='"+warehouse+"'";

            SqlCommand sqlCommand = new SqlCommand(sql_items_available, con);
            SqlDataReader dr = sqlCommand.ExecuteReader();

            //         con.Close();

            if (dr.Read() && dr["QUANTITY_AVAILABLE"] != DBNull.Value)
            {
                int qty_available = Convert.ToInt32(dr["QUANTITY_AVAILABLE"]);
                if (qty_available <= 0)
                    return 0;
                return qty_available;
            }
            return 0;
        }

        // THIS FUNCTION FETCHES LOCATIONS WITH QUANTITY IN HAND FOR GIVEN ITEM_ID FOR GIVEN WAREHOUSE.

        public DataTable? Get_Available_Items_and_Locations(long order_id,long item_id, string warehouse)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_items_available = @"select IM.Inv_ID,ODO.ID,ODO.ORDER_ID,ODO.ORDERED_QUANTITY,LM.Location_ID,LM.Location_Code,LM.Warehouse_ID,IM.QTY_In_Hand,IM.Expiry_Date,
                                        ODO.PICKED_QUANTITY,ODO.PICKED_BY,ODO.PICKED_DATE,
                                        ITEM.Item_Code,ITEM.Item_Desc,ITEM.Item_ID    
                                        from Inventory_Master IM " +
                " inner join Location_Master LM on IM.Location_ID=LM.Location_ID " +
                " inner join Warehouse_Master WM on WM.Warehouse_ID=LM.Warehouse_ID " +
                " inner join Item_Master ITEM on ITEM.Item_ID=IM.Item_ID " +
                " inner join ORDER_DETAILS_OUTBOUND ODO on IM.Item_ID=ODO.ITEM_ID " +
                " where ODO.ORDER_ID="+order_id+" and IM.Item_ID=" + item_id + " and WM.Name='" + warehouse + "' and IM.QTY_In_Hand>0 order by IM.Expiry_Date,Location_Code";
            DataTable dt = new DataTable();
            SqlDataAdapter da = new(sql_items_available, con);
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            else
                return dt;

        }

        // THIS FUNCTION UPDATES Inventory_Master TABLE WITH QTY_In_Hand=QTY_In_Hand - items_picked 
        // IT UPDATES ORDER_DETAILS_OUTBOUND TABLE WITH PICKED_QUANTITY=PICKED_QUANTITY + items_picked,
        // PICKED_BY, PICKED_DATE FIELDS.
        // IF ALL QUANTITY IS PICKED,THEN SET IS_RECEIVED=true in ORDER_DETAILS_OUTBOUND TABLE
        // INSERT INTO Pick_Logs TABLE.

        public void RemoveInventory(long inv_id,long details_id, int Location_ID, int Item_ID, int items_picked, DateTime? Expiry_Date, string user)
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

            string sql_items_picked = "update Inventory_Master set QTY_In_Hand=QTY_In_Hand-" + items_picked + " where Inv_ID=" + inv_id;
            SqlCommand sqlCommand_1 = new SqlCommand(sql_items_picked, con);
            sqlCommand_1.ExecuteNonQuery();


            // UPDATE QUANTITY_PICKED IN ORDER_DETAILS_OUTBOUND TABLE

            string sql_qty_picked = "update ORDER_DETAILS_OUTBOUND set PICKED_QUANTITY=PICKED_QUANTITY+" + items_picked + ",PICKED_BY='"+user.ToUpper()+"',PICKED_DATE='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"' where ID=" + details_id;

            SqlCommand sqlCommand_2 = new SqlCommand(sql_qty_picked, con);
            sqlCommand_2.ExecuteNonQuery();

            // IF ALL QUANTITY IS PICKED,THEN SET IS_RECEIVED=true in ORDER_DETAILS_OUTBOUND TABLE

            string sql_check_picked = "select PICKED_QUANTITY,ORDERED_QUANTITY from ORDER_DETAILS_OUTBOUND where ID=" + details_id;

            SqlCommand sqlCommand_4 = new SqlCommand(sql_check_picked, con);
            SqlDataReader dr = sqlCommand_4.ExecuteReader();

            dr.Read();
            int qty_ordered = Convert.ToInt32(dr["ORDERED_QUANTITY"]);
            int qty_picked= Convert.ToInt32(dr["PICKED_QUANTITY"]);
            dr.Close();

            if(qty_picked>=qty_ordered)
            {
                string sql_is_received = "update ORDER_DETAILS_OUTBOUND set IS_RECEIVED=1 where ID=" + details_id;

                SqlCommand sqlCommand_5 = new SqlCommand(sql_is_received, con);
                sqlCommand_5.ExecuteNonQuery();
            }


            // INSERT INTO Pick_Logs TABLE

            string sql_pick_logs = "insert into Pick_Logs([Location_ID],[Item_ID],[Quantity_Picked],[Expiry_Date],[Picked_By],[Record_Enter_Date],[Details_ID]) values(" + Location_ID + "," + Item_ID + "," + items_picked + "," + expiry_date + ",'" + user.ToUpper() + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',"+details_id+")";

            SqlCommand sqlCommand_3 = new SqlCommand(sql_pick_logs, con);
            sqlCommand_3.ExecuteNonQuery();

            con.Close();
        }

        // THIS FUNCTION FETCHES ALL PICK LOGS FOR GIVEN WAREHOUSE.

        public DataTable? All_Pick_Logs(string warehouse)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_all_put_away_logs = @"select PL.Details_ID,LM.Location_Code,IM.Item_Code,IM.Item_Desc,PL.Quantity_Picked,PL.Picked_By,PL.Expiry_Date,PL.Record_Enter_Date
                                            from Pick_Logs PL inner join Location_Master LM on
                                            PL.Location_ID=LM.Location_ID inner join Item_Master IM on
                                            PL.Item_ID=IM.Item_ID inner join ORDER_DETAILS_OUTBOUND ODO on
                                            PL.Details_ID=ODO.ID inner join ORDER_HEADER_OUTBOUND OHO on
                                            ODO.ORDER_ID=OHO.ORDER_ID and OHO.WAREHOUSE='" + warehouse+"'" +
                                            " order by PL.Record_Enter_Date desc;";
            SqlDataAdapter da = new SqlDataAdapter(sql_all_put_away_logs, con);

            DataTable dt_all_put_away_logs = new DataTable();

            da.Fill(dt_all_put_away_logs);

            //     con.Close();

            if (dt_all_put_away_logs.Rows.Count == 0)
                return null;
            else
                return dt_all_put_away_logs;

        }

        // THIS FUNCTION SEARCHES PICK LOGS RECORDS WITH 'SEARCH CRITERIA','SEARCH VALUE','FROM DATE',
        // 'TO DATE' FIELDS.
        // SEARCH CRITERIA HAS 3 VALUES 'LOCATION CODE','ITEM CODE','CREATED BY'.
        // IT FORMS SQL QUERY BASED ON THIS SEARCH CRITERIA.
        // IT FETCHES RECORDS WITH SQL QUERY.

        public DataTable? Search(string warehouse,string Search_Criteria, string Search_Value, DateTime From_Date, DateTime To_Date)
        {
            string search_query = @"select PL.Details_ID,LM.Location_Code,IM.Item_Code,IM.Item_Desc,PL.Quantity_Picked,PL.Picked_By,PL.Expiry_Date,PL.Record_Enter_Date
                                            from Pick_Logs PL inner join Location_Master LM on
                                            PL.Location_ID=LM.Location_ID inner join Item_Master IM on
                                            PL.Item_ID=IM.Item_ID inner join ORDER_DETAILS_OUTBOUND ODO on
                                            PL.Details_ID=ODO.ID inner join ORDER_HEADER_OUTBOUND OHO on
                                            ODO.ORDER_ID=OHO.ORDER_ID and OHO.WAREHOUSE='" + warehouse+"'"
                                            + " and PL.Record_Enter_Date between '"
											+ From_Date.ToString("yyyy-MM-dd HH:mm:ss") + "' and '"
                                            + To_Date.ToString("yyyy-MM-dd HH:mm:ss") + "'";

            switch (Search_Criteria)
            {
                case "Location Code":
                    search_query += " and LM.Location_Code='" + Search_Value + "'";
                    break;
                case "Item Code":
                    search_query += "and IM.Item_Code='" + Search_Value + "'";
                    break;
                case "Created By":
                    search_query += " and PL.Picked_By='" + Search_Value.ToUpper() + "'";
                    break;

            }

            search_query += " order by PL.Record_Enter_Date desc;";

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            SqlDataAdapter adapter = new(search_query, con);
            DataTable DT = new DataTable();

            adapter.Fill(DT);

            if (DT.Rows.Count == 0)
                return null;
            return DT;
        }

        // THIS FUNCTION SETS IS_RECEIVED=1 FOR GIVEN DETAILS ITEM IN ORDER_DETAILS_OUTBOUND TABLE.

        public void ReceivedItem(long details_id)
        {
			string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

			SqlConnection con = new SqlConnection(conn_str);
			con.Open();

            string sql_received_item = "update ORDER_DETAILS_OUTBOUND set IS_RECEIVED=1 where ID=" + details_id;
            SqlCommand cmd = new SqlCommand(sql_received_item, con);

            cmd.ExecuteNonQuery();

            con.Close();

		}

    }
}
