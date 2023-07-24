using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace DAL.CRUD
{
    // THIS CLASS IS FOR UPDATING DATABSE.
    public class Inhand_Inventory
    {

        // THIS FUNCTION IS TO FETCH LIST OF ALL WAREHOUSES FROM Warehouse_Master TABLE.IT SELECTS ONLY 2 FIELDS
        // Warehouse_ID AND Name. 

        public DataTable? GetAll_Warehouses()
        {

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_all_warehouse = "select Warehouse_ID,Name  from Warehouse_Master order by Name";
            SqlDataAdapter da = new(sql_all_warehouse, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }

        // THIS FUNCTION FETCHES ALL LOCATIONS PRESENT IN THIS WAREHOUSE (string warehouse).

        public DataTable? GetAll_Locations_for_Warehouse(string warehouse)
        {

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_all_locations_for_warehouse = "select LM.* from Location_Master LM" +
                                                        " inner join Warehouse_Master WM on LM.Warehouse_ID=WM.Warehouse_ID " +
                                                        " where WM.Name='"+warehouse +"' order by LM.Location_Code";
            SqlDataAdapter da = new(sql_all_locations_for_warehouse, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }

        // THIS FUNCTION FETCHES ALL ITEMS PRESENT AT THIS LOCATION (int location_ID).

        public DataTable? GetAll_Items_for_Location(int location_ID)
        {

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_all_locations_for_warehouse = @"select INM.Location_ID,INM.Item_ID,sum(INM.QTY_In_Hand) as QTY_In_Hand,Item_Code,Item_Desc,Location_Code,WA.Warehouse_ID,WA.Name,INM.Expiry_Date
                                                        from Inventory_Master INM inner join Item_Master IT on INM.Item_ID=IT.Item_ID inner join Location_Master LO 
                                                        on INM.Location_ID=LO.Location_ID inner join Warehouse_Master WA on WA.Warehouse_ID=LO.Warehouse_ID  
                                                        and INM.Location_ID="+location_ID+" and INM.QTY_In_Hand>0 "+
                                                        "group by INM.Item_ID,INM.Expiry_Date,Item_Code,Item_Desc,INM.Location_ID,Location_Code,WA.Warehouse_ID,WA.Name "+
                                                        "order by Item_Code,INM.Expiry_Date";
            SqlDataAdapter da = new(sql_all_locations_for_warehouse, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }

        // THIS FUNCTION INSERTS NEW LOCATION INTO Location_Master TABLE.ALL FIELDS ARE PASSED TO THIS FUNCTION.

        public int Insert_Location(string location_code,int warehouse_id,string Created_By,DateTime Created_Date)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_new_location = "insert into Location_Master(Location_Code,Warehouse_ID,Created_By,Created_Date) OUTPUT INSERTED.Location_ID values('" + location_code + "'," + warehouse_id + ",'" + Created_By + "','" + Created_Date.ToString("yyyy-M-dd HH:mm")+"')";
            SqlCommand cmd=new SqlCommand(sql_new_location, con);
            int new_location_id=(int)cmd.ExecuteScalar();

            con.Close();

            return new_location_id;
        }

        // THIS FUNCTION UPDATES A LOCATION RECORD IN Location_Master TABLE.ALL FIELDS ARE PASSED TO THIS FUNCTION.

        public void Update_Location(int location_id,string location_code, int warehouse_id, string Updated_By, DateTime Updated_Date)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_update_location = "update Location_Master set Location_Code='" + location_code + "',Warehouse_ID=" + warehouse_id + ",Updated_By='"+Updated_By+"',Updated_Date='" + Updated_Date.ToString("yyyy-M-dd HH:mm") + "' where Location_ID="+location_id;
            SqlCommand cmd = new SqlCommand(sql_update_location, con);
            cmd.ExecuteNonQuery();

            con.Close();
        }

        // THIS FUNCTION MOVES AN INVENTORY.IT ACCEPTS PREV_LOCATION_ID,NEXT_LOCATION_ID,ITEM_ID,QTY_MOVE,
        // EXPIRY_DATE.FIRST, IT FETCHES RECORDS FROM Inventory_Master.THEN SUBTRACTS QTY_MOVE FROM QTY_IN_HAND
        // IN PREV_LOCATION_ID RECORDS .
        // THEN INSERTS NEW INVENTORY(QTY_MOVE) AT NEXT_LOCATION_ID

        public void Move_Inventory(int prev_location_id, int next_location_id,int item_id, int qty_move, DateTime? expiry_date)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string str_expiry_date;
            if (expiry_date == null)
                str_expiry_date = "null";
            else
                str_expiry_date = "'" + expiry_date?.ToString("yyyy-M-d") + "'";

            string sql_same_items;
            if(expiry_date==null)
                sql_same_items = "select * from Inventory_Master where Location_ID=" + prev_location_id + " and Item_ID=" + item_id+ " and Expiry_Date is null";
            else
                sql_same_items = "select * from Inventory_Master where Location_ID=" + prev_location_id + " and Item_ID=" + item_id + " and Expiry_Date="+str_expiry_date;
            SqlCommand cmd = new SqlCommand(sql_same_items, con);
            SqlDataReader dr=cmd.ExecuteReader();

            // Subtracting Qty from Prev. Location 

            int qty_left = qty_move;

            while(dr.Read())
            {
                long inv_id = (long)dr["Inv_ID"];
                int qty = (int)dr["QTY_In_Hand"];
                if(qty>=qty_left)
                {
                    string update_item_1 = "Update Inventory_Master set QTY_In_Hand=QTY_In_Hand-" + qty_left + " where Inv_ID=" + inv_id;
                    SqlCommand cmd_1=new(update_item_1,con);  
                    cmd_1.ExecuteNonQuery();
                    break;
                }
                else
                {
                    qty_left -= qty;
                    string update_item_2= "Update Inventory_Master set QTY_In_Hand=0 where Inv_ID=" + inv_id;
                    SqlCommand cmd_2 = new(update_item_2, con);
                    cmd_2.ExecuteNonQuery();
                }
            }

            dr.Close();

            // Inserting into new location
                    

            string sql_insert_at_new_location = "insert into Inventory_Master([Location_ID],[Item_ID],[QTY_In_Hand],[Expiry_Date]) values(" + next_location_id + "," + item_id + "," + qty_move + "," + str_expiry_date + ")";
            SqlCommand cmd_insert=new(sql_insert_at_new_location,con);
            cmd_insert.ExecuteNonQuery();

            con.Close();
        }

        // THIS FUNCTION RETURNS DATATABLE dt WITH ALL ITEM RECORDS WHOSE TOTAL QTY_IN_HAND EQUALS 0.  
        // IT SEARCHES ALL ITEMS PRESENT IN CURRENT WAREHOUSE.

        public DataTable? Get_0_Inventory(string warehouse)
        {
            Warehouse wh = new Warehouse();
            int warehouse_id = wh.GetWarehouse_ID(warehouse);

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_0_inventory = @"select ISNULL(sum(QTY_In_Hand), 0) as Qty_In_Hand,IT.Item_ID,IT.Item_Code,IT.Item_Desc from Inventory_Master INM
right join Item_Master IT on INM.Item_ID = IT.Item_ID
left join Location_Master LM on LM.Location_ID = INM.Location_ID
left join Warehouse_Master WM on LM.Warehouse_ID = WM.Warehouse_ID

and WM.Name = '"+warehouse+"' group by IT.Item_ID,IT.Item_Code,IT.Item_Desc,IT.Warehouse_ID having ISNULL(sum(QTY_In_Hand), 0) = 0 "+
"and IT.Warehouse_ID = "+warehouse_id+" order by IT.Item_Code;";

            SqlDataAdapter da = new(sql_0_inventory, con);
            DataTable dt = new DataTable();
            
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }
    }
}
