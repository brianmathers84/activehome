<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="GridViewEditCell" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>ActiveHome Status / Trigger</title>  
    <link href="Stylesheet.css" rel="stylesheet" type="text/css" />  
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
        <asp:Label ID="Label1" runat="server"></asp:Label><br />
        <asp:CheckBox ID="btnL1" autopostback="true" runat="server" OnClick="btnL1_Click" text="Tess Light" OnCheckedChanged="btnL1_CheckedChanged" BorderStyle="Solid" BorderWidth="1px" Font-Size="Small" Width="97px" />
                <asp:CheckBox ID="btnL2" runat="server" autopostback="true" BorderStyle="Solid" BorderWidth="1px" Font-Size="Small" OnCheckedChanged="btnL2_CheckedChanged" OnClick="btnL2_Click" text="Kirsty Light" Width="97px" />
                <asp:CheckBox ID="btnL3" runat="server" autopostback="true" BorderStyle="Solid" BorderWidth="1px" Font-Size="Small" OnCheckedChanged="btnL3_CheckedChanged" OnClick="btnL3_Click" text="Poppy Light" Width="97px" />
                <br />
                <asp:Button ID="btnL4" CssClass="button" runat="server" autopostback="true" BorderStyle="Solid" BorderWidth="1px" Font-Size="Small" OnClick="btnWine_Click" text="Wine Time" Width="97px" />
                <asp:Button ID="btnBedtime" CssClass="button" runat="server" autopostback="true" BorderStyle="Solid" BorderWidth="1px" Font-Size="Small" OnClick="btnBedtime_Click" text="Bedtime" Width="97px" />
        <asp:GridView ID="GridView1" runat="server" BackColor="White" BorderColor="#e9ecef" AutoGenerateColumns="False" 
            BorderStyle="None" BorderWidth="1px" CellPadding="4" HorizontalAlignment="stretch" ForeColor="Black" GridLines="Vertical"
            OnRowDataBound="GridView1_RowDataBound" OnRowCommand="GridView1_RowCommand" ShowFooter="True">
            <Columns>                
                <asp:ButtonField Text="SingleClick" CommandName="SingleClick" Visible="False"/>
                <asp:TemplateField HeaderText="A">
                    <ItemTemplate>
                        <asp:Label ID="ALabel" runat="server" Text='<%# Eval("A") %>'></asp:Label>                        
                    </ItemTemplate>               
                </asp:TemplateField>
                <asp:TemplateField HeaderText="B">
                    <ItemTemplate>
                        <asp:Label ID="BLabel" runat="server" Text='<%# Eval("B") %>'></asp:Label>                        
                    </ItemTemplate>               
                </asp:TemplateField>
                <asp:TemplateField HeaderText="C">
                    <ItemTemplate>
                        <asp:Label ID="CLabel" runat="server" Text='<%# Eval("C") %>'></asp:Label>                        
                    </ItemTemplate>               
                </asp:TemplateField>
                <asp:TemplateField HeaderText="D">
                    <ItemTemplate>
                        <asp:Label ID="DLabel" runat="server" Text='<%# Eval("D") %>'></asp:Label>                        
                    </ItemTemplate>               
                </asp:TemplateField>
                <asp:TemplateField HeaderText="E">
                    <ItemTemplate>
                        <asp:Label ID="ELabel" runat="server" Text='<%# Eval("E") %>'></asp:Label>                        
                    </ItemTemplate>               
                </asp:TemplateField>
                <asp:TemplateField HeaderText="F">
                    <ItemTemplate>
                        <asp:Label ID="FLabel" runat="server" Text='<%# Eval("F") %>'></asp:Label>                        
                    </ItemTemplate>               
                </asp:TemplateField>
                <asp:TemplateField HeaderText="G">
                    <ItemTemplate>
                        <asp:Label ID="GLabel" runat="server" Text='<%# Eval("G") %>'></asp:Label>                        
                    </ItemTemplate>               
                </asp:TemplateField>
                <asp:TemplateField HeaderText="H">
                    <ItemTemplate>
                        <asp:Label ID="HLabel" runat="server" Text='<%# Eval("H") %>'></asp:Label>                        
                    </ItemTemplate>               
                </asp:TemplateField>
                <asp:TemplateField HeaderText="I">
                    <ItemTemplate>
                        <asp:Label ID="ILabel" runat="server" Text='<%# Eval("I") %>'></asp:Label>                        
                    </ItemTemplate>               
                </asp:TemplateField>
                <asp:TemplateField HeaderText="J">
                    <ItemTemplate>
                        <asp:Label ID="JLabel" runat="server" Text='<%# Eval("J") %>'></asp:Label>                        
                    </ItemTemplate>               
                </asp:TemplateField>
                <asp:TemplateField HeaderText="K">
                    <ItemTemplate>
                        <asp:Label ID="KLabel" runat="server" Text='<%# Eval("K") %>'></asp:Label>                        
                    </ItemTemplate>               
                </asp:TemplateField>
                <asp:TemplateField HeaderText="L">
                    <ItemTemplate>
                        <asp:Label ID="LLabel" runat="server" Text='<%# Eval("L") %>'></asp:Label>                        
                    </ItemTemplate>               
                </asp:TemplateField>
                <asp:TemplateField HeaderText="M">
                    <ItemTemplate>
                        <asp:Label ID="MLabel" runat="server" Text='<%# Eval("M") %>'></asp:Label>                        
                    </ItemTemplate>               
                </asp:TemplateField>
                <asp:TemplateField HeaderText="N">
                    <ItemTemplate>
                        <asp:Label ID="NLabel" runat="server" Text='<%# Eval("N") %>'></asp:Label>                        
                    </ItemTemplate>               
                </asp:TemplateField>
                <asp:TemplateField HeaderText="O" >
                    <ItemTemplate>
                        <asp:Label ID="OLabel" runat="server" Text='<%# Eval("O") %>'></asp:Label>                        
                    </ItemTemplate>               
                </asp:TemplateField>
                <asp:TemplateField HeaderText="P" >
                    <ItemTemplate>
                        <asp:Label ID="PLabel" runat="server" Text='<%# Eval("P") %>'></asp:Label>                        
                    </ItemTemplate>               
                </asp:TemplateField>
            </Columns>
            <HeaderStyle CssClass="headerStyle" ForeColor="Blue" />
            <RowStyle CssClass="rowStyle" />
            <AlternatingRowStyle CssClass="alternatingRowStyle" />
            <FooterStyle CssClass="footerStyle" />
            <PagerStyle CssClass="pagerStyle" ForeColor="White" />
        </asp:GridView>
        <asp:Button ID="btnClearLog" runat="server" Text="Clear Log" OnClick="ClearLog_Click" />
        <asp:Button ID="btnResetLogs" runat="server" Text="Reset Log" OnClick="ResetLog_Click" />
        <asp:Button ID="btnRefresh" runat="server" Text="Hidden" OnClick="Unnamed2_Click" Visible="false" /><br />
                 <asp:Timer ID="Timer1" runat="server" OnTick="Timer1_Tick" Interval="5000" />
                <asp:Label ID="lblLastRefresh" runat="server" Text="Last Refreshed:"></asp:Label><br />
                <asp:GridView AutoGenerateColumns="False" Width="300px" ID="MessageLabel" runat="server" AutoPostBack="True" OnDataBinding="MessageLabel_DataBinding">
                    <Columns>
                        <asp:BoundField ItemStyle-Width="40px" DataField="ID" HeaderText="ID" />
                        <asp:BoundField ItemStyle-Width="250px" DataField="message" HeaderText="Message" />
                    </Columns>
                </asp:GridView>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
