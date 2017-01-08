using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    private MySqlConnection InitializeDB()
    {
        string server = "localhost";
        string database = "ahome";
        string uid = "ahome";
        string password = GetKey("sqlPass");
        string connectionString;
        connectionString = "SERVER=" + server + ";" + "DATABASE=" +
        database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

        return new MySqlConnection(connectionString);
    }

    protected void Login1_Authenticate(object sender, AuthenticateEventArgs e)
    {
        int userId = -1;
        if (Login1.Password.Length < 8 && Login1.Password.Length > 30)
        {
            Login1.FailureText = "Password must be between 8 & 30 characters";
            return;
        }
        using (MySqlConnection conn = InitializeDB())
        {
            using (MySqlCommand cmd = new MySqlCommand("validate_user"))
            {
                cmd.Connection = conn;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("Uname", Login1.UserName);
                cmd.Parameters.AddWithValue("Passw", Encrypt(Login1.Password));
                conn.Open();
                var usr = cmd.ExecuteScalar();
                if (usr != null)
                    Int32.TryParse(usr.ToString(), out userId);
                conn.Close();
            }
        }
        switch (userId)
        {
            case -1:
                Login1.FailureText = "Username and/or password is incorrect.";
                break;
            case -2:
                Login1.FailureText = "Account has not been activated.";
                break;
            default:
                FormsAuthentication.SetAuthCookie(Login1.UserName, true);
                e.Authenticated = true;
                Session["LoginUserName"] = Convert.ToString(Login1.UserName);
                break;
        }
    }
    private string Encrypt(string clearText)
    {
        string EncryptionKey = GetKey("encryptKey");
        byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                clearText = Convert.ToBase64String(ms.ToArray());
            }
        }
        return clearText;
    }
    private string GetKey(string key)
    {
        var customSetting = WebConfigurationManager.AppSettings[key];
        return customSetting;

    }
    private string Decrypt(string cipherText)
    {

        string EncryptionKey = GetKey("encryptKey");

        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                cipherText = Encoding.Unicode.GetString(ms.ToArray());
            }
        }
        return cipherText;
    }

    protected void Login1_LoggedIn(object sender, EventArgs e)
    {
        Response.Redirect("~/Default.aspx");
    }
}