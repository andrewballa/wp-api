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
        var cats = "564", tags = "805";

        if (0 !== cats.length || 0 !== tags.length) {
            var queries = "categories=" + cats + "&tags=" + tags;
            $.ajax({
                method: "get",
                url: "/customapi/wordpress",
                contentType: "application/json",
                data: queries
            }).done(function(response) {
                console.log(response);
            });
        }
    })
</script>
