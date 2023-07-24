using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace DAL.CRUD
{

    // THIS CLASS IS FOR DATABASE UPDATES.

    public class Vendor
    {
        // THIS FUNCTION FETCHES SINGLE RECORD FROM Vendor TABLE FOR GIVEN VENDOR_ID.

        public DataRow Get_By_Vendor_ID(long vendor_id)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_by_vendor_id = "select * from Vendor where Vendor_ID=" + vendor_id;
            SqlDataAdapter da = new(sql_by_vendor_id, con);
            DataTable dt_by_vendor_id = new DataTable();
            da.Fill(dt_by_vendor_id);

            con.Close();

            return dt_by_vendor_id.Rows[0];

        }

        // THIS FUNCTION INSERTS A VENDOR RECORD INTO Vendor TABLE.ALL FIELDS ARE PASSED TO THIS FUNCTION.

        public void CreateRecord(string Vendor_Name, string Street_Address, string City, string Prov_State,string Postal_Code,string Phone,string Fax,string Email,bool Is_Active,string Contact_Person,string Contact_Phone,string Mode_Of_Payment,string Notes)
        {

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string insert_query = "insert into Vendor(Vendor_Name,Street_Address,City,Prov_State,Postal_Code,Phone," +
                                        "Fax,Email,Is_Active,Contact_Person,Contact_Phone,Mode_Of_Payment,Notes) " +
                    " values('" + Vendor_Name.ToUpper().Replace("'", "''") + "','" + Street_Address.ToUpper().Replace("'", "''") + "','"
                    + City.ToUpper().Replace("'", "''") + "','" + Prov_State.ToUpper().Replace("'", "''") + "','" + Postal_Code + "','" + Phone + "','" +
                    Fax + "','" + Email + "','" + Is_Active + "','" + Contact_Person.ToUpper() + "','" + Contact_Phone + "','" +
                    Mode_Of_Payment + "','"+Notes+"');";
            SqlCommand cmd = new(insert_query, con);
            cmd.ExecuteNonQuery();
            con.Close();
        }

        // THIS FUNCTION UPDATES A VENDOR RECORD FROM Vendor TABLE.ALL FIELDS ARE PASSED TO THIS FUNCTION.

        public void UpdateRecord(long Vendor_ID,string Vendor_Name, string Street_Address, string City, string Prov_State, string Postal_Code, string Phone, string Fax, string Email, bool Is_Active, string Contact_Person, string Contact_Phone, string Mode_Of_Payment,string Notes)
        {

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string update_query = "update Vendor set Vendor_Name='" + Vendor_Name.ToUpper().Replace("'", "''") +
                    "',Street_Address='" + Street_Address.ToUpper().Replace("'", "''") + "',City='" + City.ToUpper().Replace("'", "''") +
                    "',Prov_State='" + Prov_State.ToUpper().Replace("'", "''") + "',Postal_Code='" + Postal_Code + "',Phone='" + Phone +
                    "',Fax='" + Fax + "',Email='" + Email + "',Is_Active='" + Is_Active + "',Contact_Person='" + Contact_Person.ToUpper() +
                    "',Contact_Phone='" + Contact_Phone + "',Mode_Of_Payment='" + Mode_Of_Payment + "',Notes='"+Notes+"' where Vendor_ID=" + Vendor_ID + ";";



            SqlCommand cmd = new(update_query, con);
            cmd.ExecuteNonQuery();
            con.Close();
        }

        // THIS FUNCTION FETCHES ALL VENDOR RECORDS PRESENT IN Vendor TABLE.

        public DataTable? GetAll()
        {

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_all_items = "select * from Vendor order by Vendor_Name ";
            SqlDataAdapter da = new(sql_all_items, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }

        // THIS FUNCTION FETCHES ALL VENDOR RECORDS PRESENT IN Vendor TABLE WITH CONDITION Is_Active=1.

        public DataTable? GetAllActive()
        {

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_all_items = "select * from Vendor where Is_Active=1 order by Vendor_Name ";
            SqlDataAdapter da = new(sql_all_items, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }

        // THIS FUNCTION SEARCHES VENDOR RECORDS WITH 'SEARCH CRITERIA','SEARCH VALUE' FIELDS.
        // SEARCH CRITERIA HAS 3 VALUES 'Vendor Name','City','Province'.
        // IT FORMS SQL QUERY BASED ON THIS SEARCH CRITERIA.
        // IT FETCHES RECORDS WITH SQL QUERY.

        public DataTable? Search(string Search_Criteria,string Search_Value)
        {
            string search_query = null;

            switch (Search_Criteria)
            {
                case "Vendor Name":
                    search_query = "select * from Vendor where Vendor_Name='" + Search_Value.ToUpper().Replace("'", "''") + "';";
                    break;
                case "City":
                    search_query = "select * from Vendor where City='" + Search_Value.ToUpper().Replace("'", "''") + "';";
                    break;
                case "Province":
                    search_query = "select * from Vendor where Prov_State='" + Search_Value.ToUpper().Replace("'", "''") + "';";
                    break;

            }

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
