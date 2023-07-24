using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace DAL.Stored_Procedures
{

    // THIS CLASS CALLS sp_Find_PONumber STORED PROCEDURE.THIS PROCEDURE ACCEPTS PONUMBER @PONumber AND
    // RETURNS @ponumber_find.
    // PONUMBER IS PASSED TO Find_Repair_PONumber FUNCTION.STORED PROCEDURE RETURNS 0 OR 1 IN @ponumber_find
    // PARAMETER.IF 1,IT MEANS PONUMBER EXISTS IN RM DATABASE.
    // ELSE 0,IT MEANS PONUMBER DOES NOT EXISTS IN RM DATABASE.

    public class Find_PONumber
    {
        public int Find_Repair_PONumber(string ponumber)
        {
            string conn_str = ConfigurationManager.ConnectionStrings["Warehouse_DB"].ConnectionString;
            SqlConnection con = new SqlConnection(conn_str);
            con.Open();
            SqlCommand cmd = new SqlCommand("sp_Find_PONumber", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@PONumber", SqlDbType.VarChar).Value = ponumber;

            SqlParameter parmOUT = new SqlParameter("@ponumber_find", SqlDbType.Int);
            parmOUT.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(parmOUT);
            cmd.ExecuteNonQuery();

            int find = (int)cmd.Parameters["@ponumber_find"].Value;
            
            con.Close();

            return find;
        }
    }
}
