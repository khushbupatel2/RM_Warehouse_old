using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DAL.CRUD
{
    // THIS CLASS IS FOR DATABASE UPDATES.

    public class User_Pages
	{
        //THIS FUNCTION SEARCHES TABLE "User_Access_Pages" AND RETURNS DATATABLE WITH USER'S GRANTED PAGES.
        
        public DataTable? FindMyPages(string user)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_my_pages = "select * from User_Access_Pages where User1='" + user + "' order by Page1";
            SqlDataAdapter da = new(sql_my_pages, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }

        // THIS FUNCTION GIVES ACCESS FOR GIVEN PAGE TO USER.ALSO,THIS CHECKS FOR UNIQUE CONSTRAINT IN TABLE,
        // IF RECORD ALREADY EXISTS ,IT RETURNS STRING  "Record Already Exists".OTHERWISE STRING "OK".

        public string GiveAccess(string page, string user)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_give_page_access = "insert into User_Access_Pages(Page1,User1) values('" + page + "','" + user.ToUpper() + "')";
            SqlCommand cmd_insert = new SqlCommand(sql_give_page_access, con);

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

        // THIS FUNCTION DELETES ACCESS FOR PAGE FROM USER.IT HAS "ID" TO DELETE PARTICULAR RECORD FROM
        // TABLE.

        public void DeleteAccess(int id)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_delete_access = "delete from User_Access_Pages where ID=" + id;
            SqlCommand cmd_delete = new SqlCommand(sql_delete_access, con);
            cmd_delete.ExecuteNonQuery();

            con.Close();
        }

        //THIS FUNCTION IS CALLED FROM INDEX.CSHTML.CS PAGE,IT CHECKS WHETHER USER HAS PERMISSION FOR 
        // PARTICULAR PAGE OR NOT.IT RETURNS true AND false.

        public bool IsValidPage(string page, string user)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_find_access = "select count(1) from User_Access_Pages where Page1='" + page + "' and User1='" + user + "'";
            SqlCommand cmd_delete = new SqlCommand(sql_find_access, con);
            int count = (int)cmd_delete.ExecuteScalar();

            con.Close();

            if (count == 0)
                return false;
            else
                return true;
        }

        // THIS FUNCTION RETURNS DATATABLE WITH ALL UNASSIGNED PAGES FOR THE PARTICULAR USER.

        public DataTable? GetAll_Unassigned_Pages(string user)
        {

            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_all_pages = "select * from Pages where Page_Name not in (select Page1 from User_Access_Pages where User1='"+user+"') order by Page_Name;";
            SqlDataAdapter da = new(sql_all_pages, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }

        // THIS FUNCTION DELETES ACCESS FOR PARTICULAR PAGE FROM PARTICULAR USER.

        public void DeleteAccess(string page_name,string user)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_delete_access = "delete from User_Access_Pages where Page1='" + page_name+ "' and User1='"+user+"';";
            SqlCommand cmd_delete = new SqlCommand(sql_delete_access, con);
            cmd_delete.ExecuteNonQuery();

            con.Close();
        }

        // THIS FUNCTION CHECKS WHETHER USER HAS ACCESS FOR PO INVOICE PAGE - TAB (SUBMIT TO ACCOUNTS DEPT).
        // IT RETURNS true OR false

        public bool FindAccountsAccess(string user)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_my_pages = "select count(1) from User_Access_Pages where User1='" + user + "' and Page1='SUBMIT_TO_ACCOUNTS_DEPT';";
            SqlCommand cmd = new(sql_my_pages, con);
            int count = (int)cmd.ExecuteScalar();

            con.Close();

            if (count == 0)
                return false;
            return true;
        }
    }
}
