using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DAL.CRUD
{

    // THIS CLASS IS FOR UPDATING DATABSE.
    public class Notes
    {
        // THIS FUNCTION INSERTS NEW RECORD INTO Notes TABLE.ALL FIELDS ARE PASSED TO THIS FUNCTION.

        public void AddNote(string note_desc,string note_by)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_insert = "Insert into Notes(Note_Desc,Note_By,Note_date) values ('" + note_desc + "','" + note_by + "',GETDATE());";
            SqlCommand cmd=new SqlCommand(sql_insert, con);

            cmd.ExecuteNonQuery();

            con.Close();

        }

        // THIS FUNCTION UPDATES Notes TABLE WITH Completed_Comments,Is_Completed,Completed_By,Completed_Date FIELDS.

        public void CompleteNote(int id,string comments,string completed_by)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_complete = "Update Notes set Completed_Comments='"+comments+ "',Is_Completed=1,Completed_By='"+completed_by+ "',Completed_Date=GETDATE() where ID=" + id;
            SqlCommand cmd = new SqlCommand(sql_complete, con);

            cmd.ExecuteNonQuery();

            con.Close();
        }

        // THIS FUNCTION FETCHES ALL NOTES RECORDS WITH GIVEN IS_COMPLETE FLAG FROM Notes TABLE.

        public DataTable? GetNotes(bool is_complete=false)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            string sql_all_items = "select * from Notes where Is_Completed='"+is_complete+"' order by Note_date desc";
            SqlDataAdapter da = new(sql_all_items, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count == 0)
                return null;
            return dt;
        }

        // THIS FUNCTION UPADTES A NOTE RECORD WITH NOTE_DESC FIELD FOR GIVEN ID.

        public void UpdateNote(int id,string note_desc)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_update = "Update Notes set Note_Desc='" + note_desc + "' where ID="+id;
            SqlCommand cmd = new SqlCommand(sql_update, con);

            cmd.ExecuteNonQuery();

            con.Close();

        }
    }
}
