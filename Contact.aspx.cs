using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.Emit;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;


public partial class Contact : System.Web.UI.Page
{
    SqlConnection sqlCon = new SqlConnection(@" Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Arman\source\repos\AddressBook\AddressBook\App_Data\ContactDatabase.mdf;Integrated Security=True");
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            btnDelete.Enabled = false;
            FillGridView();
        }
    }

    protected void btnClear_Click(object sender, EventArgs e)
    {
        Clear();
    }
    public void Clear()
    {
        hfContactId.Value = "";
        txtFullName.Text = txtMobile.Text = txtEmail.Text = txtAddress.Text = "";
        lblSuccessMessage.Text = lblErrorMessage.Text = "";
        btnSave.Text = "Save";
        btnDelete.Enabled = false;
    }

    protected bool checkName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;
        if (!char.IsLetter(name[0]) && name[0] != '_')
            return false;
        for (int ix = 1; ix < name.Length; ++ix)
            if (!char.IsLetterOrDigit(name[ix]) && name[ix] != '_' && name[ix] != ' ')
                return false;
        return true;
    }
    protected bool checkMobile(string mobile)
    {
        return Regex.Match(mobile, @"^(\+[0-9]{11})$").Success;
    }
    protected bool checkEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (sqlCon.State == System.Data.ConnectionState.Closed)
            sqlCon.Open();
        SqlCommand sqlCmd = new SqlCommand("ContactCrateOrUpdate", sqlCon);
        bool isValidName = checkName(txtFullName.Text.Trim());
        bool isValidMobile = checkMobile(txtMobile.Text.Trim());
        bool isValidEmail = checkEmail(txtEmail.Text.Trim());
        if (isValidName && isValidEmail && isValidMobile)
        {
            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("@ContactId", (hfContactId.Value == "" ? 0 : Convert.ToInt32(hfContactId.Value)));
            sqlCmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
            sqlCmd.Parameters.AddWithValue("@Mobile", txtMobile.Text.Trim());
            sqlCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
            sqlCmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
            sqlCmd.ExecuteNonQuery();
            sqlCon.Close();
            String contactId = hfContactId.Value;
            Clear();
            if (contactId == "")
                lblSuccessMessage.Text = "Saved Successfuly";
            else
                lblSuccessMessage.Text = "Update Successfuly";
            FillGridView();
        }
        else
        {
            string strNameMessage = "";
            string strMobileMessage = "";
            string strEmailMessage = "";
            if (!isValidName)
            {
                strNameMessage = "FullName,";
            }
            if (!isValidMobile)
            {
                strMobileMessage = "Mobile,";
            }
            if (!isValidEmail)
            {
                strEmailMessage = "Email,";
            }
            lblErrorMessage.Text =  ("Syntax  error in  the " + strNameMessage + strMobileMessage + strEmailMessage);

        }

        
    }
    void FillGridView()
    {
        if (sqlCon.State == System.Data.ConnectionState.Closed)
            sqlCon.Open();
        SqlDataAdapter sqlDa = new SqlDataAdapter("ContactViewAll", sqlCon);
        sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
        DataTable dtbl = new DataTable();
        sqlDa.Fill(dtbl);
        sqlCon.Close();
        gvContact.DataSource = dtbl;
        gvContact.DataBind();
    }


    protected void btnDelete_Click(object sender, EventArgs e)
    {
        if (sqlCon.State == System.Data.ConnectionState.Closed)
            sqlCon.Open();
        SqlCommand sqlCmd = new SqlCommand("ContactDeleteById", sqlCon);
        sqlCmd.CommandType = CommandType.StoredProcedure;
        sqlCmd.Parameters.AddWithValue("@ContactId", Convert.ToInt32(hfContactId.Value));
        sqlCmd.ExecuteNonQuery();
        sqlCon.Close();
        Clear();
        FillGridView();
        lblSuccessMessage.Text = "Deleted Successfuly";

    }

    protected void lnk_OnClick(object sender, EventArgs e)
    {
        int contactId = Convert.ToInt32((sender as LinkButton).CommandArgument);
        if (sqlCon.State == System.Data.ConnectionState.Closed)
            sqlCon.Open();
        SqlDataAdapter sqlDa = new SqlDataAdapter("ContactViewById", sqlCon);
        sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
        DataTable dtbl = new DataTable();
        sqlDa.SelectCommand.Parameters.AddWithValue("@ContactId", contactId);
        sqlDa.Fill(dtbl);
        sqlCon.Close();
        hfContactId.Value = contactId.ToString();
        txtFullName.Text = dtbl.Rows[0]["FullName"].ToString();
        txtMobile.Text = dtbl.Rows[0]["Mobile"].ToString();
        txtEmail.Text = dtbl.Rows[0]["Email"].ToString();
        txtAddress.Text = dtbl.Rows[0]["Address"].ToString();
        btnSave.Text = "Update";
        btnDelete.Enabled = true;
    }
}