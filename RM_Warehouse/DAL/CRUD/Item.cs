using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DAL.CRUD
{

    // THIS CLASS IS FOR UPDATING DATABSE.
    public class Item
    {

        // THIS FUNCTION FETCHES ONE ITEM RECORD FROM Item_Master TABLE FOR GIVEN ITEM_ID (int item_id). 

        public DataRow Get_By_Item_ID(int item_id)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
   
            string sql_by_item_id = "select * from Item_Master where Item_ID=" + item_id;
            SqlDataAdapter da = new(sql_by_item_id, con);
            DataTable dt_by_item_id = new DataTable();
            da.Fill(dt_by_item_id);

            con.Close();

            return dt_by_item_id.Rows[0];

        }

        // THIS FUNCTION INSERTS A NEW ITEM RECORD INTO Item_Master TABLE.ALL FIELDS ARE PASSED TO THIS FUNCTION.
        public void CreateRecord(string Item_Code,string Item_Desc,decimal price,string Currency, string Created_By,DateTime Created_Date,string warehouse)
        {
            Warehouse wh = new Warehouse();
            int warehouse_id = wh.GetWarehouse_ID(warehouse);


            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_insert = @"insert into Item_Master(Item_Code,Item_Desc,Price,Currency,Created_By,Created_Date,Warehouse_ID) values ('"
            + Item_Code + "','" + Item_Desc + "','" + price + "','" + Currency + "','" + Created_By + "','" + Created_Date.ToString("yyyy-MM-dd HH:mm:ss") + "',"+warehouse_id+");";

            SqlCommand cmd = new(sql_insert, con);
            cmd.ExecuteNonQuery();
            con.Close();
        }

        // THIS FUNCTION UPDATES AN ITEM RECORD INTO Item_Master TABLE.ALL FIELDS ARE PASSED TO THIS FUNCTION.

        public void UpdateRecord(int Item_ID,string Item_Code,string Item_Desc, string Updated_By, DateTime Updated_Date,string warehouse)
        {
            Warehouse wh = new Warehouse();
            int warehouse_id = wh.GetWarehouse_ID(warehouse);

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_update = @"update Item_Master set Item_Code='" + Item_Code + "',Item_Desc='" + Item_Desc + "',Updated_By='" + Updated_By + "',Updated_Date='" + Updated_Date.ToString("yyyy-MM-dd HH:mm:ss") + "',Warehouse_ID="+warehouse_id+" where Item_ID=" + Item_ID;
            SqlCommand cmd = new(sql_update, con);
            cmd.ExecuteNonQuery();
            con.Close();
        }

        // THIS FUNCTION FETCHES ALL ITEMS PRESENT IN Item_Master TABLE.

        public DataTable? GetAll(string warehouse)
        {
            Warehouse wh = new Warehouse();
            int warehouse_id = wh.GetWarehouse_ID(warehouse);

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_all_items = "select * from Item_Master where Warehouse_ID="+warehouse_id+ " order by Item_ID desc";
            SqlDataAdapter da = new(sql_all_items, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }
        public DataTable? GetAllItems(string warehouse)
        {
            Warehouse wh = new Warehouse();
            int warehouse_id = wh.GetWarehouse_ID(warehouse);

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_all_items = "select top 50 * from Item_Master where Warehouse_ID=" + warehouse_id + " order by Item_ID desc";
            SqlDataAdapter da = new(sql_all_items, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }
        public DataTable? GetAllCount(string warehouse)
        {
            Warehouse wh = new Warehouse();
            int warehouse_id = wh.GetWarehouse_ID(warehouse);

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_all_items = "select count(1) from Item_Master where Warehouse_ID=" + warehouse_id + " ";
            SqlDataAdapter da = new(sql_all_items, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }

        // THIS FUNCTION FETCHES ALL LOCATIONS WITH INVENTORY FIELDS FOR GIVEN ITEM_ID PRESENT IN THIS WAREHOUSE.

        public DataTable? Get_Available_Items_and_Locations(long item_id, string warehouse)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_items_available = @"select ODI.COST,ODI.CURRENCY,LM.Location_ID,LM.Location_Code,LM.Warehouse_ID,sum(IM.QTY_In_Hand) as QTY_In_Hand,IM.Expiry_Date
       from Inventory_Master IM 
                 left join ORDER_DETAILS_INBOUND ODI on ODI.ID=IM.Details_ID
                 inner join Location_Master LM on IM.Location_ID=LM.Location_ID 
                 inner join Warehouse_Master WM on WM.Warehouse_ID=LM.Warehouse_ID
				 inner join Item_Master ITEM on ITEM.Item_ID=IM.Item_ID 
				 where IM.Item_ID=" + item_id+" and WM.Name='"+warehouse+"' " +
                 "group by IM.Expiry_Date,Location_Code,LM.Location_ID,LM.Warehouse_ID,ODI.COST,ODI.CURRENCY " +
                 "order by IM.Expiry_Date,Location_Code";
            DataTable dt = new DataTable();
            SqlDataAdapter da = new(sql_items_available, con);
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            else
                return dt;

        }

        // THIS FUNCTION UPDATES Item_Master TABLE WITH PRICE,CURRENCY,UPDATED_BY AND UPDATED_DATE FIELDS.
        // THEN IT INSERTS OLD PRICE AND OLD CURRENCY INTO Item_Price_History TABLE FOR HISTORY PURPOSE..

        public void UpdatePriceOfItem(long item_id,decimal old_price,string old_currency,decimal new_price,string new_currency,string updated_by)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            // UPDATING PRICE IN Item_Master TABLE 

            string sql_update = @"update Item_Master set Price=" + new_price + ",Currency='" + new_currency + "',Updated_By='" + updated_by.ToUpper() + "',Updated_Date=GETDATE() where Item_ID=" + item_id;
            SqlCommand cmd = new(sql_update, con);
            cmd.ExecuteNonQuery();

            // INSERTING NEW RECORD IN Item_Price_History TABLE

            if (old_price != 0 && old_currency != null)
            {
                string sql_insert = @"insert into Item_Price_History(Item_ID,Price,Currency,Date_Updated,Updated_By) values(" + item_id + "," + old_price + ",'" + old_currency + "',GETDATE(),'" + updated_by.ToUpper() + "')";
                SqlCommand cmd_1 = new(sql_insert, con);
                cmd_1.ExecuteNonQuery();
            }
            con.Close();
        }

        // THIS FUNCTION FETCHES ALL RECORDS FROM Item_Price_History TABLE FOR GIVEN ITEM_ID (int item_id).

        public DataTable? GetPriceHistoryByItemID(int item_id)
        {

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_all_items = "select * from Item_Price_History where Item_ID="+item_id +" order by Date_Updated desc";
            SqlDataAdapter da = new(sql_all_items, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }

        // THIS FUNCTION UPDATES Item_Master TABLE WITH Image_Filename FIELD FOR GIVEN ITEM_ID AND FILENAME.

        public void UpdateImageFileName(int item_id, string fileName)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            // UPDATING Image_Filename IN Item_Master TABLE FOR GIVEN ITEM_ID.

            string sql_update = @"update Item_Master set Image_Filename ='" + fileName + "' where Item_ID=" + item_id;
            SqlCommand cmd = new(sql_update, con);
            cmd.ExecuteNonQuery();

            con.Close();
        }

        public DataTable? GetAllItem(string warehouse,string searchtext)
        {
            Warehouse wh = new Warehouse();
            int warehouse_id = wh.GetWarehouse_ID(warehouse);

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_all_items = "exec Usp_FindItemFromTable '"+ warehouse_id + "','"+ searchtext + "'";
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
