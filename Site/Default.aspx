<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="/Scripts/jquery-3.1.1.js" type="text/javascript"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Literal ID="output" runat="server"></asp:Literal>
        </div>
    </form>
</body>
</html>

<script>
    $(document).ready(function () {
        var queries = "categories=564,680&tags=36,672";
        $.ajax({
            method: "get",
            url: "/customapi/wordpress",
            contentType: "application/json",
            data: queries
        }).done(function (response) {
            console.log(response);
        });
    })
</script>
