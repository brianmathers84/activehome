using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Data;
using ActiveHomeScriptLib;
using MySql.Data.MySqlClient;
using System.Linq;

public partial class bedtime : System.Web.UI.Page
{
    private ActiveHome ah;
    private DataTable dtMessages = new DataTable();
    const string TessAddr = "C1";
    const string KirstyAddr = "C2";
    const string PoppyAddr = "D1";
    protected void Page_Load(object sender, EventArgs e)
    {
        string onoff = Request.QueryString["onoff"].ToString();
        string device = Request.QueryString["device"].ToString();
        switch (device)
        {
            case "kirsty": SendPlc(KirstyAddr, onoff);break;
            case "tess": SendPlc(TessAddr, onoff); break;
            case "poppy": SendPlc(PoppyAddr, onoff); break;
            case "bedtime": SendPlc(KirstyAddr, "on"); SendPlc(TessAddr, "on");
                SendPlc(KirstyAddr, "off"); SendPlc(TessAddr, "off");
                SendPlc(KirstyAddr, "on"); SendPlc(TessAddr, "on");
                SendPlc(KirstyAddr, "off"); SendPlc(TessAddr, "off");
                SendPlc(KirstyAddr, "on"); SendPlc(TessAddr, "on");
                SendPlc(KirstyAddr, "off"); SendPlc(TessAddr, "off"); break;
            case "winetime":
                SendPlc(PoppyAddr, "off"); SendPlc(PoppyAddr, "on");
                SendPlc(PoppyAddr, "off"); SendPlc(PoppyAddr, "on");
                SendPlc(PoppyAddr, "off"); SendPlc(PoppyAddr, "on"); break;
            default: break;
        }
    }
    private void SendPlc(string sAddr, string onoff)
    {
        if (ah == null)
            ah = new ActiveHomeClass();
        ah.SendAction("SENDPLC", sAddr + " " + onoff, null, null);
    }
    #region Database Methods
    private MySqlConnection InitializeDB()
    {
        string server = "localhost";
        string database = "ahome";
        string uid = "ahome";
        string password = "P0ps!cleP0l0";
        string connectionString;
        connectionString = "SERVER=" + server + ";" + "DATABASE=" +
        database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

        return new MySqlConnection(connectionString);
    }
    private void GetLogs()
    {
        DataTable dt;
        if (Session["MessageTable"] != null)
            dt = (DataTable)Session["MessageTable"];
        else
            dt = new DataTable();
        try
        {
            using (MySqlConnection conn = InitializeDB())
            {
                conn.Open();
                string query = "SELECT * FROM logs";

                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, conn);

                //Execute command
                MySqlDataReader reader = cmd.ExecuteReader();
                dt.Load(reader);
                if (!dt.Columns.Contains("message"))
                    dt.Columns.Add("message", typeof(String));
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["command"].ToString() != "")
                    {
                        dr["message"] = dr["timestamp"].ToString() + " " +
                            dr["command"].ToString() + " " + dr["address"].ToString() +
                            " " + dr["parms"].ToString();
                    }
                }
                conn.Close();
            }
        }
        catch (MySqlException ex)
        {
            AddMessage(ex.Message);
        }
        Session["MessageTable"] = dt;
    }
    private void DeleteDbLogs(bool bDeleteAll)
    {
        DataTable dt;
        string query = "";
        if (Session["MessageTable"] != null)
            dt = (DataTable)Session["MessageTable"];
        else
            dt = new DataTable();
        try
        {
            using (MySqlConnection conn = InitializeDB())
            {
                conn.Open(); // keep the latest log for each address and delete the rest
                if (bDeleteAll)
                    query = "truncate table logs;";
                else
                    query = "DELETE FROM logs where id NOT IN (select cid from (SELECT MAX(id) as cid FROM logs where timestamp >= DATE_ADD(CURDATE(), INTERVAL -4 DAY) GROUP BY address ) as c);";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    int success = cmd.ExecuteNonQuery();
                }
                conn.Close();
                dt.Clear();
            }
        }
        catch (MySqlException ex)
        {
            AddMessage(ex.Message);
        }
        Session["MessageTable"] = dt;
    }
    private void InsertDbLog(string command, string address, string parms)
    {
        try
        {
            using (MySqlConnection conn = InitializeDB())
            {
                conn.Open();
                string query = "INSERT INTO logs (command, address, parms) values ('" +
                   command + "','" + address + "','" + parms + "')";

                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, conn);
                int success = cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
        catch (MySqlException ex)
        {
            AddMessage(ex.Message);
        }
    }
    #endregion
    #region Messages
    private void InitMessages()
    {
        dtMessages = new DataTable();
        DataColumn dcp = new DataColumn("ID", typeof(Int32));
        DataColumn dc = new DataColumn("message");
        DataColumn dcc = new DataColumn("command");
        DataColumn dca = new DataColumn("address");
        DataColumn dct = new DataColumn("timestamp", typeof(DateTime));
        DataColumn dccalc = new DataColumn("parms");
        dtMessages.Columns.Add(dcp);
        dtMessages.Columns.Add(dc);
        dtMessages.Columns.Add(dca);
        dtMessages.Columns.Add(dcc);
        dtMessages.Columns.Add(dct);
        dtMessages.Columns.Add(dccalc);
        DataColumn[] prim = new DataColumn[1] { dcp };
        dtMessages.PrimaryKey = prim;

        dtMessages.Clear();
        DataRow dr = dtMessages.NewRow();
        dr["ID"] = 0;
        dr["message"] = "Initialised";
        dr["timestamp"] = DateTime.Now;
        dtMessages.Rows.Add(dr);
        DataView dv = dtMessages.DefaultView;
        dv.Sort = "ID desc";
        Session["MessageTable"] = dtMessages;
    }
    private void AddMessage(string message)
    {
        if (Session["MessageTable"] != null)
            dtMessages = (DataTable)Session["MessageTable"];
        else
            InitMessages();
        DataRow dr = dtMessages.NewRow();
        dr["ID"] = dtMessages.Rows.Count;
        dr["message"] = message;
        dr["timestamp"] = DateTime.Now;
        dtMessages.Rows.Add(dr);
        Session["MessageTable"] = dtMessages;
    }
    private void AddMessage(string command, string code, string message)
    {
        if (Session["MessageTable"] != null)
            dtMessages = (DataTable)Session["MessageTable"];
        else
            InitMessages();
        DataRow dr = dtMessages.NewRow();
        dr["ID"] = dtMessages.Rows.Count;
        dr["message"] = message;
        dr["command"] = command;
        dr["code"] = code;
        dr["timestamp"] = DateTime.Now;
        dtMessages.Rows.Add(dr);
        Session["MessageTable"] = dtMessages;
    }
    #endregion
}