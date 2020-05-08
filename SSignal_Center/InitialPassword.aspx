<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InitialPassword.aspx.vb" Inherits="SSignal_Center.InitialPassword" %>
<%@ Import Namespace="SSignal_Protocols" %>
<%@ Import Namespace="SSignal_GlobalCommonCode" %>
<%@ Import Namespace="SSignal_CenterCode" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1"/>
    <title>设置邮箱初始密码</title>

    <script>
        function SavePassword() {
            var Password = document.getElementById("password").value;
            if (Password.length < 12) {
                alert("密码至少要有12个字符。")
                return;
            }
            var urlpart = "?C=InitialEmailBoxPassword&Password=" + encodeURIComponent(Password)
            RequestServer(urlpart, function (response) {
                if (response == null) {
                    return;
                }
                alert(response);
            });
        }

        function RequestServer(urlpart, func) {
            if (window.XMLHttpRequest) {
                var xhr = new XMLHttpRequest();
                xhr.onreadystatechange = function () {
                    if (xhr.readyState == 4) {
                        if (xhr.status == 200) {
                            func(xhr.responseText);
                        } else {
                            alert("从服务器获取数据失败。请重试。（原因代码" + xhr.status + "）");
                        }
                    }
                };
                xhr.timeout = 15000;
                xhr.open("POST", "Default.aspx" + urlpart, true);
                xhr.send(null);
            } else {
                alert("此浏览器太老旧了，请使用最新的浏览器。");
            }
        }
    </script>
</head>
<body>
    <p><%=EmailAddress()%></p>
    <input name="password" id="password" type="password" value="" maxlength="<%=最大值_常量集合.密码长度%>"/>
    <button type="button" onclick="SavePassword()">确定</button>
    <p><%=Language()%></p>
</body>
</html>
