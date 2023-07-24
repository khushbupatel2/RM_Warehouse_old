using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DAL.CRUD
{

    // THIS CLASS IS FOR DATABASE UPDATES.
    public class Put_Away
    {

        // THIS FUNCTION FETCHES ALL LOCATIONS WITH INVENTORY FOR GIVEN ITEM_ID AND WAREHOUSE.

        public DataTable? Get_Locations_For_Item_ID(int ITEM_ID,string WAREHOUSE)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql = @"select IM.Item_ID,IM.Item_Code,IM.Item_Desc,LM.Location_ID,LM.Location_Code,INV.QTY_In_Hand,INV.Expiry_Date from Item_Master IM inner join Inventory_Master INV on IM.Item_ID=INV.Item_ID
							 inner join Location_Master LM on INV.Location_ID=LM.Location_ID
							 inner join Warehouse_Master WM on LM.Warehouse_ID=WM.Warehouse_ID
							 and IM.Item_ID="+ITEM_ID+" and WM.Name='"+WAREHOUSE+ "' and INV.QTY_In_Hand>0 order by LM.Location_Code";

            SqlDataAdapter da = new(sql, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }

        // THIS FUNCTION RETURNS RECEIVED_QUANTITY-QUANTITY_PLACED as QUANTITY_LEFT FROM ORDER_DETAILS_INBOUND
        // TABLE FOR GIVEN DETAILS_ID.

        public int ItemsLeft(long details_id)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_items_left = "select RECEIVED_QUANTITY-QUANTITY_PLACED as QUANTITY_LEFT from ORDER_DETAILS_INBOUND where id=" + details_id;

            SqlCommand sqlCommand = new SqlCommand(sql_items_left, con);
            SqlDataReader dr= sqlCommand.ExecuteReader();

   //         con.Close();

            if(dr.Read())
            {
                int qty_left = Convert.ToInt32(dr["QUANTITY_LEFT"]);
                if (qty_left <= 0)
                    return 0;
                return qty_left;
            }
            return 0;
        }

        // THIS FUNCTION INSERTS AN RECORD INTO Inventory_Master TABLE.
        // IT UPDATES ORDER_DETAILS_INBOUND TABLE WITH QUANTITY_PLACED=QUANTITY_PLACED+items_placed
        // FOR GIVEN DETAILS_ID.
        // FINALLY IT INSERTS AN RECORD INTO Put_Away_Logs TABLE FOR HISTORY PURPOSE.

        public void SaveInventory(long details_id,int Location_ID,int Item_ID,int items_placed,DateTime? Expiry_Date,string user)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string expiry_date;
            if (Expiry_Date == null)
                expiry_date = "null";
            else
                expiry_date = "'" + Expiry_Date?.ToString("yyyy-M-d") + "'";
            
            // INSERT INTO Inventory_Master

            string sql_items_placed = "Insert into Inventory_Master(Location_ID,Item_ID,QTY_In_Hand,Expiry_Date,Details_ID) values(" + Location_ID + "," + Item_ID + "," + items_placed + ","+expiry_date+","+details_id+");";
                SqlCommand sqlCommand_1 = new SqlCommand(sql_items_placed, con);
                sqlCommand_1.ExecuteNonQuery();
           

            // UPDATE QUANTITY_PLACED IN ORDER_DETAILS_INBOUND TABLE

            string sql_qty_placed = "update ORDER_DETAILS_INBOUND set QUANTITY_PLACED=QUANTITY_PLACED+" +items_placed +" where ID="+details_id;

            SqlCommand sqlCommand_2 = new SqlCommand(sql_qty_placed, con);
            sqlCommand_2.ExecuteNonQuery();

            // INSERT INTO Put_Away_Logs TABLE

            string sql_put_away_logs = "insert into Put_Away_Logs([Location_ID],[Item_ID],[Quantity_Placed],[Expiry_Date],[Placed_By],[Record_Enter_Date],[Details_ID]) values(" + Location_ID+","+ Item_ID+","+ items_placed+","+expiry_date+",'"+user.ToUpper()+"','"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',"+details_id+")";

            SqlCommand sqlCommand_3 = new SqlCommand(sql_put_away_logs, con);
            sqlCommand_3.ExecuteNonQuery();

            con.Close();
        }

        // THIS FUNCTION FETCHES ALL PUT AWAY LOGS FOR GIVEN WAREHOUSE FROM DATABASE. 

        public DataTable? All_Put_Away_Logs(string warehouse)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_all_put_away_logs = @"select PAL.Details_ID,LM.Location_Code,IM.Item_Code,IM.Item_Desc,PAL.Quantity_Placed,PAL.Placed_By,PAL.Expiry_Date,PAL.Record_Enter_Date
                                            from Put_Away_Logs PAL inner join Location_Master LM on
                                            PAL.Location_ID=LM.Location_ID inner join Item_Master IM on
                                            PAL.Item_ID=IM.Item_ID inner join ORDER_DETAILS_INBOUND OD on
                                            PAL.Details_ID=OD.ID inner join ORDER_HEADER_INBOUND OH on
                                            OD.ORDER_ID=OH.ORDER_ID and OH.WAREHOUSE='" + warehouse+"'" +
                                            " order by PAL.Record_Enter_Date desc;";
            SqlDataAdapter da = new SqlDataAdapter(sql_all_put_away_logs, con);

            DataTable dt_all_put_away_logs = new DataTable();

            da.Fill(dt_all_put_away_logs);

       //     con.Close();

            if (dt_all_put_away_logs.Rows.Count == 0)
                return null;
            else
                return dt_all_put_away_logs;

        }

        // THIS FUNCTION SEARCHES PUT AWAY LOGS RECORDS WITH 'SEARCH CRITERIA','SEARCH VALUE','FROM DATE',
        // 'TO DATE' FIELDS.
        // SEARCH CRITERIA HAS 3 VALUES 'LOCATION CODE','ITEM CODE','CREATED BY'.
        // IT FORMS SQL QUERY BASED ON THIS SEARCH CRITERIA.
        // IT FETCHES RECORDS WITH SQL QUERY.

        public DataTable? Search(string warehouse,string Search_Criteria, string Search_Value,DateTime From_Date,DateTime To_Date)
        {
            string search_query = @"select PAL.Details_ID,LM.Location_Code,IM.Item_Code,IM.Item_Desc,PAL.Quantity_Placed,PAL.Placed_By,PAL.Expiry_Date,PAL.Record_Enter_Date
                                            from Put_Away_Logs PAL inner join Location_Master LM on
                                            PAL.Location_ID=LM.Location_ID inner join Item_Master IM on
                                            PAL.Item_ID=IM.Item_ID inner join ORDER_DETAILS_INBOUND OD on
                                            PAL.Details_ID=OD.ID inner join ORDER_HEADER_INBOUND OH on
                                            OD.ORDER_ID=OH.ORDER_ID and OH.WAREHOUSE='" + warehouse+"'"  
                                            + " and PAL.Record_Enter_Date between '" + From_Date.ToString("yyyy-MM-dd HH:mm:ss") +"' and '"
                                            +To_Date.ToString("yyyy-MM-dd HH:mm:ss") + "'";

            switch (Search_Criteria)
            {
                case "Location Code":
                    search_query += " and LM.Location_Code='" + Search_Value+ "'";
                    break;
                case "Item Code":
                    search_query += "and IM.Item_Code='" + Search_Value + "'";
                    break;
                case "Created By":
                    search_query += " and PAL.Placed_By='" + Search_Value.ToUpper() + "'";
                    break;

            }

            search_query += " order by PAL.Record_Enter_Date desc;";

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
    }
}
