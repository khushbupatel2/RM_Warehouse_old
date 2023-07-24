using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DAL.CRUD
{

    // THIS CLASS IS FOR DATABASE UPDATES.
    public class Warehouse
    {
        // THIS FUNCTION FETCHES SINGLE RECORD FROM Warehouse_Master TABLE FOR GIVEN WAREHOUSE_ID.

        public DataRow Get_By_Warehouse_ID(int warehouse_id)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_by_warehouse_id = "select * from Warehouse_Master where Warehouse_ID=" + warehouse_id;
            SqlDataAdapter da = new(sql_by_warehouse_id, con);
            DataTable dt_by_warehouse_id = new DataTable();
            da.Fill(dt_by_warehouse_id);

            con.Close();

            return dt_by_warehouse_id.Rows[0];

        }

        // THIS FUNCTION INSERTS A WAREHOUSE RECORD INTO Warehouse_Master TABLE.ALL FIELDS ARE PASSED TO
        // THIS FUNCTION.

        public void CreateRecord(string Name, string Address1, string Address2, string City, string State_Province,
            string Postal_Code, string Country, string Email, string Fax, bool Is_Active, int? Default_Receiving_Location_Id, string Phone,string PO_Abbrivation)
        {

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_insert = @"insert into Warehouse_Master(Name,Address1,Address2,City,State_Province
                                ,Postal_Code,Country,Email,Fax,Is_Active,Default_Receiving_Location_Id,Phone,PO_ABBRIVATION) values ('" + Name.ToUpper() + "','" +
                                Address1 + "','" + Address2 + "','" + City + "','" + State_Province + "','" + Postal_Code + "','" + Country + "','" + Email + "','" +
                                Fax + "','" + Is_Active + "',"+Default_Receiving_Location_Id+",'" + Phone + "','"+PO_Abbrivation.ToUpper()+"')";
            SqlCommand cmd = new(sql_insert, con);
            cmd.ExecuteNonQuery();
            con.Close();
        }

        // THIS FUNCTION FETCHES ALL WAREHOUSE RECORDS PRESENT IN Warehouse_Master TABLE.

        public DataTable? GetAll()
        {

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_all_warehouse = "select * from Warehouse_Master order by Name";
            SqlDataAdapter da = new(sql_all_warehouse, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }

        // THIS FUNCTION UPDATES A WAREHOUSE RECORD FROM Warehouse_Master TABLE.ALL FIELDS ARE PASSED TO
        // THIS FUNCTION.

        public void UpdateRecord(int Warehouse_ID, string Name, string Address1, string Address2, string City, string State_Province,
            string Postal_Code, string Country, string Email, string Fax, bool Is_Active, int? Default_Receiving_Location_Id, string Phone,string PO_Abbrivation)
        {

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_update = @"update Warehouse_Master set Name='" + Name.ToUpper() + "',Address1='" + Address1 + "',Address2='" + Address2 + "',City='" + City + "',State_Province='" + State_Province +
                                "',Postal_Code='" + Postal_Code + "',Country='" + Country + "',Email='" + Email + "',Fax='" + Fax + "',Is_Active='" + Is_Active + "',Default_Receiving_Location_Id="+Default_Receiving_Location_Id+",Phone='" + Phone +
                                "',PO_ABBRIVATION='"+PO_Abbrivation.ToUpper()+"' where Warehouse_ID=" + Warehouse_ID;
            SqlCommand cmd = new(sql_update, con);
            cmd.ExecuteNonQuery();
            con.Close();
        }

        // THIS FUNCTION SEARCHES User_Access_Warehouse TABLE FOR GIVEN USER.
        // IT RETURNS RECORDS WITH GRANTED WAREHOUSE ACCESS FOR USER.

        public DataTable? FindMyWarewhouses(string user)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_my_warehouses = "select * from User_Access_Warehouse where User1='" + user + "' order by Warehouse";
            SqlDataAdapter da = new(sql_my_warehouses, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }

        // THIS FUNCTION INSERTS NEW RECORD INTO User_Access_Warehouse TABLE WITH WAREHOUSE AND USER1
        // FIELDS.THIS MEANS USER HAS GRANTED ACCESS FOR THIS WAREHOUSE. 

        public string GiveAccess(string warehouse, string user)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_give_warehouse_access = "insert into User_Access_Warehouse(Warehouse,User1) values('" + warehouse + "','" + user.ToUpper() + "')";
            SqlCommand cmd_insert = new SqlCommand(sql_give_warehouse_access, con);

            try
            {
                cmd_insert.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                return "Record Already Exists";
            }

            return "OK";
        }

        // THIS FUNCTION DELETES A RECORD FROM User_Access_Warehouse TABLE WITH GIVEN ID.
        // IT MEANS USER ACCESS FOR THIS WAREHOUSE IS REVOKED.

        public void DeleteAccess(int id)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_delete_access = "delete from User_Access_Warehouse where ID=" + id;
            SqlCommand cmd_delete = new SqlCommand(sql_delete_access, con);
            cmd_delete.ExecuteNonQuery();

            con.Close();
        }

        // THIS FUNCTION CHECKS GIVEN USER HAS ACEESS FOR GIVEN WAREHOUSE OR NOT.
        // IT RETURNS true,WHEN USER HAS ACCESS FOR GIVEN WAREHOUSE.
        // ELSE false,THEN USER HAS NO ACCESS FOR GIVEN WAREHOUSE.

        public bool IsValidWarehouse(string warehouse, string user)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_find_access = "select count(1) from User_Access_Warehouse where Warehouse='" + warehouse + "' and User1='" + user + "'";
            SqlCommand cmd_delete = new SqlCommand(sql_find_access, con);
            int count = (int)cmd_delete.ExecuteScalar();

            con.Close();

            if (count == 0)
                return false;
            else
                return true;
        }

        // THIS FUNCTION RETURNS WAREHOUSE_ID FOR GIVEN WAREHOUSE NAME FROM Warehouse_Master TABLE.

        public int GetWarehouse_ID(string warehouse)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_warehouse_id = "select Warehouse_ID from Warehouse_Master where Name='" + warehouse + "';";

            SqlCommand cmd = new SqlCommand(sql_warehouse_id, con);

            SqlDataReader dr = cmd.ExecuteReader();
            int warehouse_id = 0;
     
            if (dr.Read())
            {
                warehouse_id = (int)dr["Warehouse_ID"];
            }
            dr.Close();
            con.Close();

            return warehouse_id;

        }

        // THIS FUNCTION UPDATES Warehouse_Master TABLE WITH Default_Receiving_Location_Id FIELD.
        // NEW LOACTION AND WAREHOUSE ARE PASSED TO THIS FUNCTION.

        public void Change_Default_Location(string warehouse,int new_default_location)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_change_default_location = "update Warehouse_Master set Default_Receiving_Location_Id="+new_default_location+" where Name='"+warehouse+"';";

            SqlCommand cmd = new SqlCommand(sql_change_default_location, con);

            cmd.ExecuteNonQuery();

            con.Close();
            
        }

        // THIS FUNCTION RETURNS PO_ABBRIVATION VALUE FROM Warehouse_Master TABLE FOR GIVEN WAREHOUSE.

        public string GetWarehouse_PO_Abbrivation(string warehouse)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_warehouse_po_abbrivation = "select PO_ABBRIVATION from Warehouse_Master where Name='" + warehouse + "';";

            SqlCommand cmd = new SqlCommand(sql_warehouse_po_abbrivation, con);

            SqlDataReader dr = cmd.ExecuteReader();

            string warehouse_po_abbrivation=string.Empty;

            if (dr.Read())
            {
                warehouse_po_abbrivation = dr["PO_ABBRIVATION"].ToString();
            }
            dr.Close();
            con.Close();

            return warehouse_po_abbrivation;

        }
    }
}
