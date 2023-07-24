using System.Configuration;
using System.Data.SqlClient;

namespace DAL.Sequence
{
    // THIS CLASS FETCHES NEXT VALUE FROM GetNumber DATABASE SEQUENCE.
    // IT HAS ONLY NextVal() FUNCTION WHICH RETURNS THIS VALUE FROM SEQUENCE. 

    public class GeneratePONumber
    {
        public long NextVal()
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;

            SqlConnection con = new SqlConnection(conn_str);
            con.Open();

            string sql_nextval = "select next value for GetNumber;";

            SqlCommand cmd = new SqlCommand(sql_nextval, con);
            long next_val= (long)cmd.ExecuteScalar();

            con.Close();

            return next_val;
        }
    }
}
