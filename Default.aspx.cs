using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using ActiveHomeScriptLib;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Collections.Generic;

public partial class GridViewEditCell : System.Web.UI.Page
{
    /// <summary>
    /// There is a ButtonField column therefore first edit cell index is 1
    /// </summary>
    private const int _firstEditCellIndex = 1;
    private ActiveHome ah;
    private DataTable dtMessages = new DataTable();
    const string TessAddr = "C1";
    const string KirstyAddr = "C2";
    const string LRAddr = "D1";
    const int COLS = 16;
    const int ROWS = 16;
    #region Page Load

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.Page.User.Identity.IsAuthenticated)
        {
            FormsAuthentication.RedirectToLoginPage();
        }
        Label1.Text = "Welcome    " + Convert.ToString(Session["LoginUserName"]);
        if (!IsPostBack)
        {
            _sampleData = null;
            GridView1.DataSource = _sampleData;
            GridView1.DataBind();
            InitMessages();
            try
            {
                ah = new ActiveHomeClass();
                //ah.RecvAction += Ah_RecvAction;
                // get logs from the database instead of polling activehome
                //QueryPlc();
                GetLogs();
                QueryDBPlc();
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }
        }
    }

    #endregion
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

    #region PLC Methods
    private void QueryDBPlc()
    {
        if (Session["MessageTable"] != null)
            dtMessages = (DataTable)Session["MessageTable"];
        if (dtMessages == null || dtMessages.Rows.Count < 1)
            InitMessages(); // session timeout may reset main table

        if (dtMessages != null && dtMessages.Rows.Count > 1)
        {
            // get latest timestamp for each address excluding initialised message
            var command = dtMessages.AsEnumerable().Where(r => r.Field<String>("message") != "Initialised");
            var q = from n in command
                    group n by n.Field<String>("address") into g
                    select g.OrderByDescending(t => t.Field<DateTime>("timestamp")).FirstOrDefault();
            DataTable dtTmp = q.AsEnumerable().CopyToDataTable();
            DataView dv = dtTmp.DefaultView;
            string sAddr = "";
            foreach (DataRowView dr in dv)
            {
                if (dr["address"].ToString().Length > 0)
                {
                    sAddr = dr["address"].ToString();
                    string cmd = dr["parms"].ToString();
                    if (cmd.ToLower().Contains("on"))
                    {
                        SetColor(sAddr, GridView1, "green");
                        SetTitle(sAddr, cmd + " " + dr["timestamp"].ToString(), GridView1);
                    }
                    if (cmd.ToLower().Contains("off"))
                    {
                        SetColor(sAddr, GridView1, "red");
                        SetTitle(sAddr, cmd + " " + dr["timestamp"].ToString(), GridView1);
                    }
                    if (cmd.ToLower().Contains("devicestop"))
                    {
                        SetColor(sAddr, GridView1, "orange");
                        SetTitle(sAddr, cmd + " " + dr["timestamp"].ToString(), GridView1);
                    }
                    if (cmd.ToLower().Contains("hail") || cmd.ToLower().Contains("extended")
                        || cmd.ToLower().Contains("request") || cmd.ToLower().Contains("preset"))
                    {
                        SetColor(sAddr, GridView1, "yellow");
                        SetTitle(sAddr, cmd + " " + dr["timestamp"].ToString(), GridView1);
                    }
                }
            }
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
        MessageLabel.DataSource = dv;
        MessageLabel.DataBind();
        Session["MessageTable"] = dtMessages;
        QueryPlc();
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

        MessageLabel.Width = 300;
        double nheight = MessageLabel.Height.Value;
        MessageLabel.Height = new Unit(nheight + 20);
        RefreshDataSource();
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

        MessageLabel.Width = 300;
        double nheight = MessageLabel.Height.Value;
        MessageLabel.Height = new Unit(nheight + 20);
        RefreshDataSource();
    }
    #endregion
    #region Refresh Data
    private void RefreshDataSource()
    {
        if (Session["MessageTable"] != null)
            dtMessages = (DataTable)Session["MessageTable"];
        InitializeDB();
        QueryDBPlc();
        GetLogs();
        MessageLabel.DataSource = dtMessages;
        lblLastRefresh.Text = "Last Refreshed: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
        MessageLabel.DataBind();
    }
    #endregion
    #region GridView1

    protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Get the LinkButton control in the first cell
            LinkButton _singleClickButton = (LinkButton)e.Row.Cells[0].Controls[0];
            // Get the javascript which is assigned to this LinkButton
            string _jsSingle = ClientScript.GetPostBackClientHyperlink(_singleClickButton, "");

            // If the page contains validator controls then call 
            // Page_ClientValidate before allowing a cell to be edited
            if (Page.Validators.Count > 0)
                _jsSingle = _jsSingle.Insert(11, "if(Page_ClientValidate())");

            // Add events to each editable cell
            for (int columnIndex = _firstEditCellIndex; columnIndex < e.Row.Cells.Count; columnIndex++)
            {
                // Add the column index as the event argument parameter
                string js = _jsSingle.Insert(_jsSingle.Length - 2, columnIndex.ToString());
                // Add this javascript to the onclick Attribute of the cell
                e.Row.Cells[columnIndex].Attributes["onclick"] = js;
                // Add a cursor style to the cells
                e.Row.Cells[columnIndex].Attributes["style"] += "width:20px;height:20px;cursor:pointer;cursor:hand;";
                if (ViewState["colour" + e.Row.RowIndex + "_" + columnIndex] != null)
                {
                    Label l = (Label)e.Row.Cells[columnIndex].Controls[1];
                    var colour = ViewState["colour" + e.Row.RowIndex + "_" + columnIndex].ToString();
                    switch (colour)
                    {
                        case "red": l.CssClass = "tdoff"; break;
                        case "green": l.CssClass = "tdon"; break;
                        case "orange": l.CssClass = "tdstop"; break;
                        default: l.CssClass = "tdq"; break;
                    }
                }
            }
        }
    }

    protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        GridView _gridView = (GridView)sender;

        switch (e.CommandName)
        {
            case ("SingleClick"):
                // Get the row index
                int _rowIndex = int.Parse(e.CommandArgument.ToString());
                // Parse the event argument (added in RowDataBound) to get the selected column index
                int _columnIndex = int.Parse(Request.Form["__EVENTARGUMENT"]);
                // Set the Gridview selected index
                _gridView.SelectedIndex = _rowIndex;
                // Bind the Gridview
                _gridView.DataSource = _sampleData;
                _gridView.DataBind();

                SetPlc(_rowIndex, _columnIndex, GridView1);
                // Get the display control for the selected cell and make it invisible
                string sAddr = GetAddress(_columnIndex, _rowIndex);
                SetColor(sAddr, _gridView, "");
                break;
        }
    }
    #endregion
    #region Render Override

    // Register the dynamically created client scripts
    protected override void Render(HtmlTextWriter writer)
    {
        // The client events for GridView1 were created in GridView1_RowDataBound
        foreach (GridViewRow r in GridView1.Rows)
        {
            if (r.RowType == DataControlRowType.DataRow)
            {
                for (int columnIndex = _firstEditCellIndex; columnIndex < r.Cells.Count; columnIndex++)
                {
                    Page.ClientScript.RegisterForEventValidation(r.UniqueID + "$ctl00", columnIndex.ToString());
                }
            }
        }

        base.Render(writer);
    }

    #endregion
    #region Sample Data

    /// <summary>
    /// Property to manage data
    /// </summary>
    private DataTable _sampleData
    {
        get
        {
            DataTable dt = (DataTable)Session["TestData"];

            if (dt == null)
            {
                // Create a DataTable and save it to session
                dt = new DataTable();
                List<String> values = new List<String>();
                for (int iHouse = 0; iHouse < COLS; iHouse++)
                {
                    dt.Columns.Add(Convert.ToChar('A' + iHouse).ToString(), typeof(string));                  
                }
                for (int i = 1; i <= ROWS; i++)
                {
                    values = new List<string>();
                    for (int col = 0; col < COLS; col++)
                    {
                        values.Add((i).ToString());
                    }
                    dt.Rows.Add(values.ToArray());
                }
                _sampleData = dt;
            }

            return dt;
        }
        set
        {
            Session["TestData"] = value;
        }
    }
    #endregion
    #region Set Color / Title / etc
    private void SetColor(string sAddr, GridView grd, string colour)
    {
        TableCell c = FindCell(sAddr);
        if (c != null && c.Controls.Count > 1)
        {
            int row = GetHouseRow(sAddr);
            int col = GetHouseCol(sAddr);

            Label _displayControl = (Label)grd.Rows[row].Cells[col].Controls[1];
            if (_displayControl != null)
            {
                switch (colour)
                {
                    case "red":
                        _displayControl.CssClass = "tdoff";
                        ViewState["colour" + row + "_" + col] = "red"; break;
                    case "green":
                        _displayControl.CssClass = "tdon";
                        ViewState["colour" + row + "_" + col] = "green"; break;
                    case "orange":
                        _displayControl.CssClass = "tdstop";
                        ViewState["colour" + row + "_" + col] = "orange"; break;
                    default:
                        _displayControl.CssClass = "tdq";
                        _displayControl.Text = "?";
                        ViewState["colour" + row + "_" + col] = "yellow"; break;
                }
            }
        }
    }

    
    private void SetTitle(string sAddr, string sTitle, GridView grd)
    {
        int row = GetHouseRow(sAddr);
        int col = GetHouseCol(sAddr);
        TableCell c = FindCell(sAddr);
        if (c != null && c.Controls.Count > 1)
        {
            Label _displayControl = (Label)grd.Rows[row].Cells[col].Controls[1];
            if (_displayControl != null)
            {
                _displayControl.ToolTip = sAddr + " " + sTitle;
                ViewState["title" + row + "_" + col] = sAddr + " " + sTitle;
            }
        }
    }
    #endregion
    #region PLC commands
    private void SetPlc(int row, int col, GridView grd)
    {
        TableCell c = grd.Rows[row].Cells[col];
        if (c != null && c.Controls.Count > 1)
        {
            Label _displayControl = (Label)grd.Rows[row].Cells[col].Controls[1];
            if (_displayControl != null)
            {
                string colour = "green";
                if (ViewState["colour" + row + "_" + col] != null)
                    colour = ViewState["colour" + row + "_" + col].ToString();
                string sCode = Convert.ToChar(col + 64).ToString();
                sCode = sCode + (row + 1);
                if (colour == "green")
                {
                    SendPlc(sCode, "off");
                    SetTitle(sCode, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString(), GridView1);
                }
                else
                {
                    SendPlc(sCode, "on");
                    SetTitle(sCode, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString(), GridView1);
                }
            }
        }
    }
    private void SendPlc(string sAddr, string onoff)
    {
        if (ah == null)
            ah = new ActiveHomeClass();
        ah.SendAction("SENDPLC", sAddr + " " + onoff, null, null);
    }
    private void QueryPlc()
    {
        if (ah == null)
            ah = new ActiveHomeClass();
        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLS; col++)
            {
                string sCode = GetAddress(row, col);
                object oCode = ah.SendAction("queryplc", sCode + " on", null, null);
                if (oCode.ToString() == "1")
                {
                    if (sCode.ToUpper() == TessAddr)
                        btnL1.Checked = true;
                    if (sCode.ToUpper() == KirstyAddr)
                        btnL2.Checked = true;
                    if (sCode.ToUpper() == LRAddr)
                        btnL3.Checked = true;
                    SetColor(sCode , GridView1, "green");
                    SetTitle(sCode, "Queryplc " +
                        DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString(), GridView1);
                }
            }
        }
    }
    #endregion
    #region GetHouse row / col
    private int GetHouseRow(string address)
    {
        if (address != null && address.Length > 0)
        {
            string cl = address.Substring(1, address.Length - 1);
            return int.Parse(cl) - 1;
        }
        return -1;
    }
    private int GetHouseCol(string address)
    {
        if (address != null && address.Length > 1)
        {
            int hse = Convert.ToChar(address.ToUpper().Substring(0, 1)) - 65;
            return (hse + 1);
        }
        return -1;
    }
    private string GetAddress(int col, int row)
    {
        string sCode = Convert.ToChar(col + 65).ToString();
        sCode = sCode + (row + 1);
        return sCode;
    }
    private TableCell FindCell(string sAddr)
    {
        TableCell returnCell = new TableCell();
        if (GridView1 != null)
        {
            int row = GetHouseRow(sAddr);
            int col = GetHouseCol(sAddr);
            if (row <= ROWS && col <= COLS && row >= 0 && col >= 0)
                returnCell = (TableCell)GridView1.Rows[row].Cells[col];
        }
        return returnCell;     
    }
    #endregion
    #region Receive
    private void Ah_RecvAction(object bszAction, object bszAddress, object bszCommand,
        object bszParm1, object bszParm2, object bszParm3, object bszReserved)
    {
        String strMsg = "";
        string strAddr = "";
       
        strAddr = bszAddress.ToString().ToUpper();
        strMsg = strAddr +" "+ bszCommand.ToString().ToUpper();

        if (bszCommand.ToString().ToLower() == "on")
            SetColor(strAddr, GridView1, "red");
        if (bszCommand.ToString().ToLower() == "off")
            SetColor(strAddr, GridView1, "red");
        strMsg = strMsg +bszAction + " " + bszAddress + " " + 
            bszCommand.ToString() + " " + bszParm1.ToString();
        if (bszParm2 != null)
            strMsg =strMsg + " "+ bszParm2.ToString();
        if (bszParm3 != null)
            strMsg = strMsg + " " + bszParm3.ToString();
        if (bszReserved != null)
            strMsg = strMsg + " " + bszReserved.ToString();
        AddMessage(strMsg);
    }

    #endregion
    #region Events
    protected void MessageLabel_DataBinding(object sender, EventArgs e)
    {

    }

    protected void ClearLog_Click(object sender, EventArgs e)
    {
        DeleteDbLogs(false);
        InitMessages();
        QueryDBPlc();
    }

    protected void Unnamed2_Click(object sender, EventArgs e)
    {
    }

    protected void Timer1_Tick(object sender, EventArgs e)
    {
        RefreshDataSource();
    }

    protected void btnL1_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox c = (CheckBox)sender;
        if (c.Checked)
            SendPlc(TessAddr, "on");
        else
            SendPlc(TessAddr, "off");
    }

    protected void ResetLog_Click(object sender, EventArgs e)
    {
        DeleteDbLogs(true);
        InitMessages();
        QueryDBPlc();
    }

    protected void btnL2_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox c = (CheckBox)sender;
        if (c.Checked)
            SendPlc(KirstyAddr, "on");
        else
            SendPlc(KirstyAddr, "off");
    }

    protected void btnL3_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox c = (CheckBox)sender;
        if (c.Checked)
            SendPlc(LRAddr, "on");
        else
            SendPlc(LRAddr, "off");
    }
    private void WineTime()
    {
        SendPlc(LRAddr, "on");
        SendPlc(LRAddr, "off");
        SendPlc(LRAddr, "on");
        SendPlc(LRAddr, "off");
        SendPlc(LRAddr, "on");
        SendPlc(LRAddr, "off");
    }
    protected void btnWine_Click(object sender, EventArgs e)
    {
        WineTime();
    }
    #endregion
}
