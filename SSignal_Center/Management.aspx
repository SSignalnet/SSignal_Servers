<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Management.aspx.vb" Inherits="SSignal_Center.Management" %>
<%@ Import Namespace="SSignal_Protocols" %>
<%@ Import Namespace="SSignal_GlobalCommonCode" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>讯宝网络管理中心</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />

    <link rel="stylesheet" href="../jquery/jquery.mobile-1.4.5.min.css" />
    <script src="../jquery/jquery-2.1.4.min.js"></script>
    <script>$(document).bind("mobileinit", function () { $.extend($.mobile, { defaultPageTransition: 'none', defaultDialogTransition: 'none' }); });</script>
    <script src="../jquery/jquery.mobile-1.4.5.min.js"></script>
    
    <style type="text/css">
        <!--
        .overlay {
            width: 100%;
            height: 100%;
            position: fixed;
            background: #000;
            opacity: 0.0;
            z-index: 10000;
        }
        a {text-decoration:none;}
        .ui-header .ui-title {margin-left:0;margin-right:0;}
        -->
    </style>

    <script>

        var ID_on, String_on, Server_on;
        var UserID, Credential, Passcode;
        var JumpToPage;

        $(document).on("pageshow", "#PG_1", function () { ListUsers(PG_1_RG, PG_1_ID, PG_1_OB, undefined, true); });
        $(document).on("pageshow", "#PG_1_RD", function () { RevisingDuty(); });
        $(document).on("pageshow", "#PG_2", function () { ListServers(PG_2_T, PG_2_OB, PG_2_HN, undefined, true); });
        $(document).on("pageshow", "#PG_2_VD", function () {
            $("#PG_2_VD_SD").text(Server_on);
            if (Server_on != Server_on_VD) {
                PG_2_VD_PN = 1;
                PG_2_VD_TP = 0;
                $("#PG_2_VD_DT").html("");
                $("#PG_2_VD_PN").text("");
                $("#PG_2_VD_PG").hide();
                Server_on_VD = Server_on;
            }
            ListVisitors(WD_MA_GN, RG_MA_GN, ET_MA_GN, undefined, true);
        });
        $(document).on("pageshow", "#PG_2_PS", function () { $("#PG_2_PS_SD").text(Server_on); });

        function PrepareToChangePage(Totalpages, PageName, PageID) {
            $("#PG_CP_TP").text(Totalpages);
            $("#PG_CP_PN").val("");
            $("#PG_CP_FT").text(PageName).attr("href", "#" + PageID);
        }

        function ChangePage() {
            var PageNumber = Number($("#PG_CP_PN").val());
            if (PageNumber < 1 || PageNumber > Number($("#PG_CP_TP").text())) {
                $("#PG_CP_PN").val("");
                return;
            }
            JumpToPage = PageNumber;
            history.back(-1);
        }

        function AdminLogin() {
            var Password = $("#PG_1_LG_PW").val();
            if (Password.length < 12) { return; }
            var popupid = "PG_1_LG_PP";
            if (needLogin(UserID, Credential) == true) { return; }
            var urlpart = "?C=AdminLogin&UserID=" + UserID + "&Credential=" + Credential + "&Password=" + encodeURIComponent(Password);
            RequestServer(urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    Passcode = $R.find("PASSCODE").text();
                    $("#PG_1_LG_PW").val("");
                    history.back(-1);
                    $("#PG_1_LG_BT").hide();
                    window.external.AdminLogin(Passcode);
                } else {
                    $R = $xmlDoc.find("INCORRECT");
                    if ($R.length > 0) {
                        showPopupInfo(popupid, "密码不正确。");
                    } else {
                        findReason($xmlDoc, popupid);
                    }
                }
            });
        }


        function showhide_User() {
            switch ($("#PG_1_RG").val()) {
                case "someone":
                    $("#PG_1_OB_DV").hide();
                    $("#PG_1_EM_DV").show();
                    break;
                default:
                    $("#PG_1_EM_DV").hide();
                    $("#PG_1_OB_DV").show();
                    $('#PG_1_ID').val("");
                    ListUsers($('#PG_1_RG').val(), "", $('#PG_1_OB').val());
            }
        }

        var PG_1_PN = 1, PG_1_TP = 0, PG_1_RG = "", PG_1_ID = "", PG_1_OB = "";
        function ListUsers(Range, ID, OrderBy, PageNumber, IsPageShow) {
            if (IsNullorEmpty(Passcode)) { return; }
            if (IsPageShow == true) {
                if (IsNullorEmpty(JumpToPage)) {
                    if ($("#PG_1_DT").html() != "") { return; }
                } else {
                    PageNumber = JumpToPage;
                    JumpToPage = undefined;
                }
            }
            if (needLogin(UserID, Credential) == true) { return; }
            if (PageNumber == undefined) {
                PageNumber = 1;
            } else if (PageNumber < 1) {
                return;
            } else if (PageNumber > PG_1_TP) {
                return;
            }
            var TimezoneOffset = -(new Date().getTimezoneOffset());
            var popupid = "PG_1_PP";
            var urlpart = "?C=ListUsers&UserID=" + UserID + "&Credential=" + Credential + "&Range=" + Range + "&ID=" + encodeURIComponent(ID) + "&OrderBy=" + OrderBy + "&PageNumber=" + PageNumber + "&Passcode=" + encodeURIComponent(Passcode) + "&TimezoneOffset=" + TimezoneOffset;
            RequestServer(urlpart, popupid, function (response) {
                if (response == null) {
                    PG_1_PN = 1;
                    PG_1_TP = 0;
                    PG_1_RG = "";
                    PG_1_ID = "";
                    PG_1_OB = "";
                    $("#PG_1_DT").html("");
                    $("#PG_1_PN").text("");
                    $("#PG_1_PG").hide();
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    var s = "";
                    $R.find("USER").each(function (i) {
                        var $R2 = $(this);
                        var ID = $R2.find("ID").text();
                        var ENGLISH;
                        var $R3 = $R2.find("ENGLISH");
                        if ($R3.length > 0) {
                            ENGLISH = $R3.text();
                        } else {
                            ENGLISH = "*";
                        }
                        var NATIVE;
                        $R3 = $R2.find("NATIVE");
                        if ($R3.length > 0) {
                            NATIVE = $R3.text();
                        } else {
                            NATIVE = "*";
                        }
                        var DISABLED = "";
                        var t;
                        $R3 = $R2.find("DISABLED");
                        if ($R3.length > 0) {
                            t = true;
                            DISABLED = '<p id="PG_1_DS_' + ID + '" style="color:red;">已停用</p>';
                        } else {
                            t = false;
                            DISABLED = '<p id="PG_1_DS_' + ID + '" style="color:red;display:none;">已停用</p>';
                        }
                        var PHONE;
                        $R3 = $R2.find("PHONE");
                        if ($R3.length > 0) {
                            PHONE = "<p>手机号：" + $R3.text() + "</p>";
                        } else {
                            PHONE = "";
                        }
                        var EMAIL;
                        $R3 = $R2.find("EMAIL");
                        if ($R3.length > 0) {
                            EMAIL = "<p>电子邮箱地址：" + $R3.text() + "</p>";
                        } else {
                            EMAIL = "";
                        }
                        var DUTY;
                        var d = "";
                        $R3 = $R2.find("DUTY");
                        if ($R3.length > 0) {
                            DUTY = $R3.text();
                            d = getDutyName(DUTY);
                            if (d != "") {
                                d = '<p id="PG_1_DU_' + ID + '">职能：' + d + '</p>';
                            } else {
                                d = '<p id="PG_1_DU_' + ID + '" style="display:none;"></p>';
                            }
                        } else {
                            DUTY = "";
                            d = '<p id="PG_1_DU_' + ID + '" style="display:none;"></p>';
                        }
                        var HOSTNAME;
                        $R3 = $R2.find("HOSTNAME");
                        if ($R3.length > 0) {
                            HOSTNAME = "<p>传送服务器：" + $R3.text() + "</p>";
                        } else {
                            HOSTNAME = "";
                        }
                        var dl = "";
                        var LOGINDATE_PC = $R2.find("LOGINDATE_PC");
                        if (LOGINDATE_PC.length > 0) {
                            dl = "<p>电脑登录：" + LOGINDATE_PC.text() + " [" + $R2.find("IP_PC").text() + "]</p>";
                        }
                        var LOGINDATE_MP = $R2.find("LOGINDATE_MP");
                        if (LOGINDATE_MP.length > 0) {
                            dl += "<p>手机登录：" + LOGINDATE_MP.text() + " [" + $R2.find("IP_MP").text() + "]</p>";
                        }
                        var ss = "";
                        var sss;
                        if (t == true) {
                            ss = '<a href="#PG_1_EB" id="PG_1_ED_' + ID + '" data-theme="a" data-rel="popup" data-position-to="window" onclick="setID(\'' + ID + '\')">启用</a>';
                            sss = '#';
                        } else {
                            ss = '<a href="#PG_1_DB" id="PG_1_ED_' + ID + '" data-theme="a" data-rel="popup" data-position-to="window" onclick="setID(\'' + ID + '\')">停用</a>';
                            sss = '#PG_1_RD';
                        }
                        s += '<li><a href="' + sss + '" id="PG_1_RD_' + ID + '" data-theme="a" onclick="setID(\'' + ID + '\', \'' + DUTY + '\')"><h2>' + NATIVE + ' /  ' + ENGLISH + '</h2>' + PHONE + EMAIL + d + HOSTNAME + dl + DISABLED + '</a>' + ss + '</li>';
                    });
                    $("#PG_1_DT").html(s).listview("refresh");
                    PG_1_PN = Number($R.find("PAGENUMBER").text());
                    PG_1_TP = Number($R.find("TOTALPAGES").text());
                    PG_1_RG = $R.find("RANGE").text();
                    PG_1_ID = $R.find("SEARCHID").text();
                    PG_1_OB = $R.find("ORDERBY").text();
                    if (IsNullorEmpty(s)) {
                        $("#PG_1_PG").hide();
                    } else {
                        $("#PG_1_PG").show();
                    }
                    $("#PG_1_PN").text(PG_1_TP);
                    $("#PG_1_RG").val(PG_1_RG).selectmenu("refresh");
                    if (PG_1_RG == "someone") {
                        $("#PG_1_ID").val(PG_1_ID);
                    } else {
                        $("#PG_1_OB").val(PG_1_OB).selectmenu("refresh");
                    }
                    $.mobile.silentScroll(0);
                } else {
                    $R = $xmlDoc.find("INCORRECT");
                    if ($R.length > 0) {
                        showPopupInfo(popupid, "请重新登录管理中心。");
                    } else {
                        findReason($xmlDoc, popupid);
                    }
                }
            });
        }

        function RevisingDuty() {
            var DUTY = String_on;
            $("#PG_1_RD_Z").prop("disabled", true).checkboxradio("refresh");
            if (DUTY.indexOf("z") >= 0) {
                $("#PG_1_RD_Z").prop("checked", true).checkboxradio("refresh");
                $("#PG_1_RD_F").prop("checked", false).checkboxradio("refresh");
                $("#PG_1_RD_F").prop("disabled", true).checkboxradio("refresh");
            } else {
                $("#PG_1_RD_Z").prop("checked", false).checkboxradio("refresh");
                if (DUTY.indexOf("f") >= 0) {
                    $("#PG_1_RD_F").prop("checked", true).checkboxradio("refresh");
                } else {
                    $("#PG_1_RD_F").prop("checked", false).checkboxradio("refresh");
                }
                $("#PG_1_RD_F").prop("disabled", false).checkboxradio("refresh");
            }
        }

        function ReviseDuty() {
            if (IsNullorEmpty(Passcode)) { return; }
            if (needLogin(UserID, Credential) == true) { return; }
            var ID = ID_on;
            var Duty = "";
            if ($("#PG_1_RD_Z").is(":checked")) {
                Duty += "z";
            } else {
                if ($("#PG_1_RD_F").is(":checked")) {
                    Duty += "f";
                }
            }
            if (String_on == Duty) { return; }
            var urlpart = "?C=ReviseDuty&UserID=" + UserID + "&Credential=" + Credential + "&Who=" + ID + "&Duty=" + Duty + "&Passcode=" + encodeURIComponent(Passcode);
            var popupid = "PG_1_RD_PP";
            RequestServer(urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    history.back(-1);
                    if (Duty == "") {
                        $("#PG_1_DU_" + ID).text("").hide();
                    } else {
                        var d = getDutyName(Duty);
                        $("#PG_1_DU_" + ID).text("职能：" + d).show();
                    }
                    $("#PG_1_RD_" + ID).attr("onclick", 'setID(\'' + ID + '\', \'' + Duty + '\')');
                } else {
                    $R = $xmlDoc.find("INCORRECT");
                    if ($R.length > 0) {
                        showPopupInfo(popupid, "请重新登录管理中心。");
                    } else {
                        findReason($xmlDoc, popupid);
                    }
                }
            });
        }

        function getDutyName(Duty) {
            var d;
            if (Duty.indexOf("z") >= 0) {
                d = "站长";
            } else if (Duty.indexOf("f") >= 0) {
                d = "副站长";
            } else {
                d = "";
            }
            return d;
        }

        function DisableEnableAccount(Disable_Enable) {
            if (IsNullorEmpty(Passcode)) { return; }
            if (needLogin(UserID, Credential) == true) { return; }
            if (Disable_Enable == "enable") {
                $("#PG_1_EB").popup("close");
            } else if (Disable_Enable == "disable") {
                $("#PG_1_DB").popup("close");
            } else {
                return;
            }
            var ID = ID_on;
            var urlpart = "?C=DisableEnableAccount&UserID=" + UserID + "&Credential=" + Credential + "&Who=" + ID + "&Disable_Enable=" + Disable_Enable + "&Passcode=" + encodeURIComponent(Passcode);
            var popupid = "PG_1_PP";
            RequestServer(urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    if (Disable_Enable == "disable") {
                        $("#PG_1_DS_" + ID).show();
                        $("#PG_1_ED_" + ID).attr("href", "#PG_1_EB").text("启用");
                        $("#PG_1_RD_" + ID).attr("href", "#");
                    } else {
                        $("#PG_1_DS_" + ID).hide();
                        $("#PG_1_ED_" + ID).attr("href", "#PG_1_DB").text("停用");
                        $("#PG_1_RD_" + ID).attr("href", "#PG_1_RD");
                    }
                } else {
                    $R = $xmlDoc.find("INCORRECT");
                    if ($R.length > 0) {
                        showPopupInfo(popupid, "请重新登录管理中心。");
                    } else {
                        $R = $xmlDoc.find("CANNOTDISABLE");
                        if ($R.length > 0) {
                            showPopupInfo(popupid, "不可以停用自己。");
                        } else {
                            findReason($xmlDoc, popupid);
                        }
                    }
                }
            });
        }


        var PG_2_PN = 1, PG_2_TP = 0, PG_2_T = "<%=服务器类别_常量集合.传送服务器 %>", PG_2_OB = "", PG_2_HN = "";
        function ListServers(Type, OrderBy, HostName, PageNumber, IsPageShow) {
            if (IsNullorEmpty(Passcode)) { return; }
            if (IsPageShow == true) {
                if (IsNullorEmpty(JumpToPage)) {
                    if ($("#PG_2_DT").html() != "") { return; }
                } else {
                    PageNumber = JumpToPage;
                    JumpToPage = undefined;
                }
            }
            if (needLogin(UserID, Credential) == true) { return; }
            if (PageNumber == undefined) {
                PageNumber = 1;
            } else if (PageNumber < 1) {
                return;
            } else if (PageNumber > PG_2_TP) {
                return;
            }
            var TimezoneOffset = -(new Date().getTimezoneOffset());
            var urlpart = "?C=ListServers&UserID=" + UserID + "&Credential=" + Credential + "&Type=" + Type + "&OrderBy=" + OrderBy + "&HostName=" + encodeURIComponent(HostName) + "&PageNumber=" + PageNumber + "&Passcode=" + encodeURIComponent(Passcode) + "&TimezoneOffset=" + TimezoneOffset;
            var popupid = "PG_2_PP";
            RequestServer(urlpart, popupid, function (response) {
                if (response == null) {
                    PG_2_PN = 1;
                    PG_2_TP = 0;
                    PG_2_T = "";
                    PG_2_OB = "";
                    PG_2_HN = "";
                    $("#PG_2_DT").html("");
                    $("#PG_2_PN").text("");
                    $("#PG_2_PG").hide();
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    var DOMAIN = $R.find("DOMAIN").text();
                    var s = "";
                    PG_2_T = $R.find("TYPE").text();
                    $R.find("SERVER").each(function (i) {
                        var $R2 = $(this);
                        var NAME = $R2.find("NAME").text();
                        var IP = $R2.find("IP").text();
                        var DATE = $R2.find("DATE").text();
                        var DISABLED = $R2.find("DISABLED");
                        if (DISABLED.length > 0) {
                            DISABLED = '<p id="PG_2_DS_' + NAME + '" style="color:red;">已停用</p>';
                        } else {
                            DISABLED = '<p id="PG_2_DS_' + NAME + '" style="color:red;display:none;">已停用</p>';
                        }
                        var sss = "";
                        var ERROR = $R2.find("ERROR1");
                        if (ERROR.length > 0) {
                            sss = "<hr><p style='white-space:pre-wrap;'>" + ERROR.text() + "</p>";
                            ERROR = $R2.find("ERROR2");
                            if (ERROR.length > 0) {
                                sss += "<p style='white-space:pre-wrap;'>" + ERROR.text() + "</p>";
                                ERROR = $R2.find("ERROR3");
                                if (ERROR.length > 0) {
                                    sss += "<p style='white-space:pre-wrap;'>" + ERROR.text() + "</p>";
                                }
                            }
                        }
                        var ss;
                        var STATISTICS = $R2.find("STATISTICS").text();
                        switch (PG_2_T) {
                            case "<%=服务器类别_常量集合.传送服务器 %>":
                            case "<%=服务器类别_常量集合.大聊天群服务器 %>":
                            case "<%=服务器类别_常量集合.超级大聊天群服务器 %>":
                                ss = "<p>用户数：" + STATISTICS + "</p>";
                                break;
                            case "<%=服务器类别_常量集合.小宇宙中心服务器 %>":
                            case "<%=服务器类别_常量集合.小宇宙写入服务器 %>":
                            case "<%=服务器类别_常量集合.小宇宙读取服务器 %>":
                                ss = "<p>访问量：" + STATISTICS + "</p>";
                                break;
                            case "<%=服务器类别_常量集合.视频通话服务器 %>":
                                ss = "<p>通话次数：" + STATISTICS + "</p>";
                                break;
                        }
                        s += '<li id="PG_2_S_' + NAME + '"><a href="#PG_2_MN_' + PG_2_T + '" data-theme="a" data-rel="popup" onclick="setServer(\'' + NAME + '\')"><h2><span id="PG_2_DM_' + NAME + '">' + NAME + '.' + DOMAIN + '</span> [<span id="PG_2_IP_' + NAME + '">' + IP + '</span>]</h2>' + ss + DISABLED + '<p>' + DATE + '</p>' + sss + '</a></li>';
                    });
                    $("#PG_2_DT").html(s).listview("refresh");
                    PG_2_PN = Number($R.find("PAGENUMBER").text());
                    PG_2_TP = Number($R.find("TOTALPAGES").text());
                    PG_2_OB = $R.find("ORDERBY").text();
                    PG_2_HN = $R.find("HOSTNAME").text();
                    if (IsNullorEmpty(s)) {
                        $("#PG_2_PG").hide();
                    } else {
                        $("#PG_2_PG").show();
                    }
                    $("#PG_2_PN").text(PG_2_TP);
                    $("#PG_2_T").val(PG_2_T).selectmenu("refresh");
                    $("#PG_2_OB").val(PG_2_OB).selectmenu("refresh");
                    $("#PG_2_HN").val(PG_2_HN);
                    $.mobile.silentScroll(0);
                } else {
                    $R = $xmlDoc.find("INCORRECT");
                    if ($R.length > 0) {
                        showPopupInfo(popupid, "请重新登录管理中心。");
                    } else {
                        findReason($xmlDoc, popupid);
                    }
                }
            });
        }

        function setServer(server) {
            Server_on = server;
        }

        function Menu_2_<%=服务器类别_常量集合.传送服务器 %>(name) {
            $("#PG_2_MN_<%=服务器类别_常量集合.传送服务器 %>").popup("close");
            switch (name) {
                case "revise":
                    window.setTimeout(function () {
                        $.mobile.changePage("#PG_2_RS");
                        $("#PG_2_RS_DM").text(Server_on);
                        $("#PG_2_RS_IP").val($("#PG_2_IP_" + Server_on).text());
                    }, 100);
                    break;
                case "account":
                    window.setTimeout(function () {
                        if ($("#PG_2_DS_" + Server_on).is(":hidden")) {
                            $("#PG_2_DB").popup("open");
                        } else {
                            $("#PG_2_EB").popup("open");
                        }
                    }, 100);
                    break;
                default:
                    window.setTimeout(function () {
                        GetServerInfo(name);
                    }, 100);
                    break;
            }
        }

        function Menu_2_<%=服务器类别_常量集合.大聊天群服务器 %>(name) {
            $("#PG_2_MN_<%=服务器类别_常量集合.大聊天群服务器 %>").popup("close");
            switch (name) {
                case "revise":
                    window.setTimeout(function () {
                        $.mobile.changePage("#PG_2_RS");
                        $("#PG_2_RS_DM").text(Server_on);
                        $("#PG_2_RS_IP").val($("#PG_2_IP_" + Server_on).text());
                    }, 100);
                    break;
                case "account":
                    window.setTimeout(function () {
                        if ($("#PG_2_DS_" + Server_on).is(":hidden")) {
                            $("#PG_2_DB").popup("open");
                        } else {
                            $("#PG_2_EB").popup("open");
                        }
                    }, 100);
                    break;
                default:
                    window.setTimeout(function () {
                        GetServerInfo(name);
                    }, 100);
                    break;
            }
        }

        function Menu_2_<%=服务器类别_常量集合.小宇宙中心服务器 %>(name) {
            $("#PG_2_MN_<%=服务器类别_常量集合.小宇宙中心服务器 %>").popup("close");
            switch (name) {
                case "revise":
                    window.setTimeout(function () {
                        $.mobile.changePage("#PG_2_RS");
                        $("#PG_2_RS_DM").text(Server_on);
                        $("#PG_2_RS_IP").val($("#PG_2_IP_" + Server_on).text());
                    }, 100);
                    break;
                case "account":
                    window.setTimeout(function () {
                        if ($("#PG_2_DS_" + Server_on).is(":hidden")) {
                            $("#PG_2_DB").popup("open");
                        } else {
                            $("#PG_2_EB").popup("open");
                        }
                    }, 100);
                    break;
                default:
                    window.setTimeout(function () {
                        GetServerInfo(name);
                    }, 100);
                    break;
            }
        }

        function GetServerInfo(InfoType) {
            if (IsNullorEmpty(Passcode)) { return; }
            if (needLogin(UserID, Credential) == true) { return; }
            var urlpart = "?C=GetServerInfo&UserID=" + UserID + "&Credential=" + Credential + "&HostName=" + Server_on + "&InfoType=" + InfoType + "&Passcode=" + encodeURIComponent(Passcode);
            var popupid = "PG_2_PP";
            RequestServer(urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    window.external.ServerInfo($R.text());
               } else {
                    $R = $xmlDoc.find("INCORRECT");
                    if ($R.length > 0) {
                        showPopupInfo(popupid, "请重新登录管理中心。");
                    } else {
                        findReason($xmlDoc, popupid);
                    }
                }
            });
        }

        var PG_2_VD_PN = 1, PG_2_VD_TP = 0, WD_MA_GN = "", RG_MA_GN = "", ET_MA_GN = "";
        function ListVisitors(WhichDay, Range, EndTime, PageNumber, IsPageShow) {
            if (IsNullorEmpty(Passcode)) { return; }
            if (IsPageShow == true) {
                if (IsNullorEmpty(JumpToPage)) {
                    if ($("#PG_2_VD_DT").html() != "") { return; }
                } else {
                    PageNumber = JumpToPage;
                    JumpToPage = undefined;
                }
            }
            if (needLogin(UserID, Credential) == true) { return; }
            var popupid = "PG_2_VD_PP";
            if (PageNumber == undefined) {
                PageNumber = 1;
            } else if (PageNumber < 1) {
                return;
            } else if (PageNumber > PG_2_VD_TP) {
                return;
            }
            var TimezoneOffset = -(new Date().getTimezoneOffset());
            var urlpart = "?C=ListVisitors&UserID=" + UserID + "&Credential=" + Credential + "&WhichDay=" + WhichDay + "&Range=" + Range + "&EndTime=" + EndTime + "&PageNumber=" + PageNumber + "&Passcode=" + encodeURIComponent(Passcode) + "&TimezoneOffset=" + TimezoneOffset + "&Domain=" + Server_on;
            RequestServer(urlpart, popupid, function (response) {
                if (response == null) {
                    PG_2_VD_PN = 1;
                    PG_2_VD_TP = 0;
                    WD_MA_GN = "";
                    RG_MA_GN = "";
                    ET_MA_GN = "";
                    $("#PG_2_VD_DT").html("");
                    $("#PG_2_VD_PN").text("");
                    $("#PG_2_VD_PG").hide();
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    var s = "";
                    $R.find("VISITOR").each(function (i) {
                        var $R2 = $(this);
                        var IP = $R2.find("IP").text();
                        var $R3 = $R2.find("COMEFROM");
                        var COMEFROM;
                        if ($R3.length > 0) {
                            COMEFROM = $R3.text();
                        }
                        var OS = $R2.find("OS").text();
                        var BROWSER = $R2.find("BROWSER").text();
                        var LANGUAGE = $R2.find("LANGUAGE").text();
                        var DATE = $R2.find("DATE").text();
                        var ss = '<a href="http://cn.bing.com/search?q=' + IP + '" target="_blank" data-theme="a">查询IP</a>';
                        if (IsNullorEmpty(COMEFROM)) {
                            s += '<li><a href="#" data-theme="a"><h2>' + IP + '</h2><p>' + OS + ', ' + BROWSER + ', ' + LANGUAGE + '</p><p>' + DATE + '（北京时间）</p></a>' + ss + '</li>';
                        } else {
                            s += '<li><a href="' + COMEFROM + '" target="_blank" data-theme="a"><h2>' + IP + '</h2><p>' + OS + ', ' + BROWSER + ', ' + LANGUAGE + '</p><p style="color:blue">来路：' + COMEFROM + '</p><p>' + DATE + '</p></a>' + ss + '</li>';
                        }
                    });
                    $("#PG_2_VD_DT").html(s).listview("refresh");
                    PG_2_VD_PN = Number($R.find("PAGENUMBER").text());
                    PG_2_VD_TP = Number($R.find("TOTALPAGES").text());
                    WD_MA_GN = $R.find("WHICHDAY").text();
                    RG_MA_GN = $R.find("RANGE").text();
                    ET_MA_GN = $R.find("ENDTIME").text();
                    if (IsNullorEmpty(s)) {
                        $("#PG_2_VD_PG").hide();
                    } else {
                        $("#PG_2_VD_PG").show();
                    }
                    $("#PG_2_VD_PN").text(PG_2_VD_PN);
                    $("#WD_MA_GN").val(WD_MA_GN).selectmenu("refresh");
                    $("#RG_MA_GN").val(RG_MA_GN).selectmenu("refresh");
                    $.mobile.silentScroll(0);
                } else {
                    $R = $xmlDoc.find("INCORRECT");
                    if ($R.length > 0) {
                        showPopupInfo(popupid, "请重新登录管理中心。");
                    } else {
                        findReason($xmlDoc, popupid);
                    }
                }
            });
        }

        function ReviseServer() {
            if (IsNullorEmpty(Passcode)) { return; }
            if (needLogin(UserID, Credential) == true) { return; }
            var popupid = "PG_2_RS_PP";
            var IP = $.trim($("#PG_2_RS_IP").val());
            if (IP.length < 3 || (IP.indexOf(".") <= 0 && IP.indexOf(":") < 0)) { return; }
            var urlpart = "?C=ReviseServer&UserID=" + UserID + "&Credential=" + Credential + "&Name=" + Server_on + "&IP=" + IP + "&Passcode=" + encodeURIComponent(Passcode);
            RequestServer(urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    if (IP != "0.0.0.0") {
                        $("#PG_2_IP_" + Server_on).text(IP);
                        $("#PG_2_RS_DM").text("");
                        $("#PG_2_RS_IP").val("");
                    } else {
                        $("#PG_2_S_" + Server_on).remove();
                    }
                    history.back(-1);
                } else {
                    $R = $xmlDoc.find("INCORRECT");
                    if ($R.length > 0) {
                        showPopupInfo(popupid, "请重新登录管理中心。");
                    } else {
                        findReason($xmlDoc, popupid);
                    }
                }
            });
        }

        function DisableEnableServer(Disable_Enable) {
            if (IsNullorEmpty(Passcode)) { return; }
            if (needLogin(UserID, Credential) == true) { return; }
            if (Disable_Enable == "enable") {
                $("#PG_2_EB").popup("close");
            } else if (Disable_Enable == "disable") {
                $("#PG_2_DB").popup("close");
            } else {
                return;
            }
            var urlpart = "?C=DisableEnableServer&UserID=" + UserID + "&Credential=" + Credential + "&Name=" + Server_on + "&Decision=" + Disable_Enable + "&Passcode=" + encodeURIComponent(Passcode);
            var popupid = "PG_2_PP";
            RequestServer(urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    if (Disable_Enable == "disable") {
                        $("#PG_2_DS_" + Server_on).show();
                    } else {
                        $("#PG_2_DS_" + Server_on).hide();
                    }
                } else {
                    $R = $xmlDoc.find("INCORRECT");
                    if ($R.length > 0) {
                        showPopupInfo(popupid, "请重新登录管理中心。");
                    } else {
                        findReason($xmlDoc, popupid);
                    }
                }
            });
        }
        

        function needLogin(UserID, Credential) {
            var CredentialLength = 40;
            if (IsNullorEmpty(UserID) || IsNullorEmpty(Credential) || Credential.length != CredentialLength) {
                return true;
            } else {
                return false;
            }
        }

        function RequestServer(urlpart, popupid, func) {
            if (window.XMLHttpRequest) {
                var xhr = new XMLHttpRequest();
                xhr.onreadystatechange = function () {
                    if (xhr.readyState == 1) {
                        $("body").append('<div class="overlay"></div>');
                        $.mobile.loading("show", {
                            text: "请稍等……",
                            textVisible: true,
                            theme: "b",
                            textonly: false,
                            html: ""
                        });
                    } else if (xhr.readyState == 4) {
                        $.mobile.loading("hide");
                        $("div.overlay").remove();
                        if (xhr.status == 200) {
                            func(xhr.responseXML);
                        } else {
                            showPopupInfo(popupid, "无法连接服务器。请重试。（错误代码 " + xhr.status + "）");
                        }
                    }
                };
                xhr.timeout = 15000;
                xhr.open("POST", "Default.aspx" + urlpart, true);
                xhr.send(null);
            } else {
                showPopupInfo(popupid, "你正在使用的浏览器太陈旧。");
            }
        }

        function findReason($xmlDoc, popupid) {
            var $R = $xmlDoc.find("INVALIDCREDENTIAL");
            if ($R.length > 0) {
                showPopupInfo(popupid, "请重新登录。");
            } else {
                $R = $xmlDoc.find("NOTAUTHORIZED");
                if ($R.length > 0) {
                    showPopupInfo(popupid, "你未获得进行此操作的权限。");
                } else {
                    $R = $xmlDoc.find("FAILED");
                    if ($R.length > 0) {
                        showPopupInfo(popupid, "由于服务器的某个原因，你的操作失败。");
                    } else {
                        $R = $xmlDoc.find("DATABASENOTREADY");
                        if ($R.length > 0) {
                            showPopupInfo(popupid, "数据库未运行。");
                        } else {
                            $R = $xmlDoc.find("ERROR");
                            if ($R.length > 0) {
                                showPopupInfo(popupid, $R.text());
                            } else {
                                $R = $xmlDoc.find("DISABLED");
                                if ($R.length > 0) {
                                    showPopupInfo(popupid, "服务器账号已停用");
                                } else {
                                    var t = $xmlDoc.text();
                                    if (IsNullorEmpty(t)) {
                                        t = $xmlDoc.find("*").eq(0).prop("tagName");
                                    }
                                    showPopupInfo(popupid, t);
                                }
                            }
                        }
                    }
                }
            }
        }

        function showPopupInfo(popupid, info) {
            if (!IsNullorEmpty(popupid)) {
                window.setTimeout(function () {
                    try {
                        $("#" + popupid + " p").text(info);
                        $("#" + popupid).popup("open");
                    } catch (e) { }
                }, 100);
            }
        }

        function setID(ID, String) {
            ID_on = ID;
            String_on = String;
        }

        function IsNullorEmpty(text) {
            if (text == undefined || text == null || text == "") {
                return true;
            } else {
                return false;
            }
        }

        function UserIDCredential(UserID2, Credential2) {
            UserID = UserID2;
            Credential = Credential2;
        }

    </script>

</head>
<body>

    <div data-role="page" id="PG_1">
        <div data-role="header" data-theme="b" data-position="fixed" data-tap-toggle="false">
            <h1>讯宝网络管理中心</h1>
        </div>
        <div role="main" class="ui-content">
            <a class="ui-btn ui-corner-all" href="#PG_1_LG" id="PG_1_LG_BT" style="color:red">登录管理中心</a>
            <div class="ui-grid-b">
                <div class="ui-block-a"><a class="ui-btn ui-corner-all" href="#" onclick="ListUsers(PG_1_RG, PG_1_ID, PG_1_OB, PG_1_PN - 1)">上一页</a></div>
                <div class="ui-block-b"><a class="ui-btn ui-corner-all" href="#" onclick="ListUsers(PG_1_RG, PG_1_ID, PG_1_OB, PG_1_PN + 1)">下一页</a></div>
                <div class="ui-block-c"><a class="ui-btn ui-corner-all" href="#" onclick="ListUsers($('#PG_1_RG').val(), $.trim($('#PG_1_ID').val()), $('#PG_1_OB').val())">查找</a></div>
            </div>
            <ul id="PG_1_DT" data-role="listview" data-inset="true" data-split-theme="b" data-split-icon="edit"></ul>
            <div class="ui-grid-b" id="PG_1_PG" style="display:none;">
                <div class="ui-block-a"><a class="ui-btn ui-corner-all" href="#" onclick="ListUsers(PG_1_RG, PG_1_ID, PG_1_OB, PG_1_PN - 1)">上一页</a></div>
                <div class="ui-block-b"><a class="ui-btn ui-corner-all" href="#" onclick="ListUsers(PG_1_RG, PG_1_ID, PG_1_OB, PG_1_PN + 1)">下一页</a></div>
                <div class="ui-block-c"><a class="ui-btn ui-corner-all" href="#PG_CP" onclick="PrepareToChangePage(PG_1_TP, '用户列表', 'PG_1')" id="PG_1_PN"></a></div>
            </div>
            <select id="PG_1_RG" data-native-menu="false" onchange="showhide_User()">
                <option selected="selected" value="all">全部</option>
                <option value="hasduty">管理人员</option>
                <option value="disabled">停用的</option>
                <option value="someone">某人</option>
            </select>
            <div class="ui-grid-a" id="PG_1_EM_DV" style="display:none;">
                <div class="ui-block-a" style="width:90%;"><input id="PG_1_ID" placeholder="用户名/手机号/电子邮箱地址" data-type="search" maxlength="<%=最大值_常量集合.讯宝和电子邮箱地址长度%>" style="color:red;" /></div>
                <div class="ui-block-b" style="width:10%;"><a href="#" data-role="button" data-icon="search" data-iconpos="notext" onclick="ListUsers($('#PG_1_RG').val(), $.trim($('#PG_1_ID').val()), $('#PG_1_OB').val())">查找</a></div>
            </div>
            <div id="PG_1_OB_DV">
                <select id="PG_1_OB" data-native-menu="false" onchange="ListUsers($('#PG_1_RG').val(), $.trim($('#PG_1_ID').val()), $('#PG_1_OB').val())">
                    <option value="bh_j" selected="selected">注册时间 从近到远</option>
                    <option value="bh_s">注册时间 从远到近</option>
                </select>
            </div>
        </div>
        <div data-role="footer" data-theme="b" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar">
                <ul>
                    <li><a href="#PG_1" data-icon="user" class="ui-btn-active ui-state-persist">用户</a></li>
                    <li><a href="#PG_2" data-icon="cloud">服务器</a></li>
                </ul>
            </div>
            <h4 style="display:none;"></h4>
        </div>
        <div data-role="popup" id="PG_1_EB" data-theme="a" data-overlay-theme="b" class="ui-content" style="max-width:340px; padding-bottom:2em;">
            <h3>启用此账号吗？</h3>
            <a href="#" class="ui-shadow ui-btn ui-corner-all ui-btn-b ui-btn-inline ui-mini" onclick="DisableEnableAccount('enable')">是</a>
            <a href="#" data-rel="back" class="ui-shadow ui-btn ui-corner-all ui-btn-inline ui-mini">否</a>
        </div>
        <div data-role="popup" id="PG_1_DB" data-theme="a" data-overlay-theme="b" class="ui-content" style="max-width:340px; padding-bottom:2em;">
            <h3>停用此账号吗？</h3>
            <a href="#" class="ui-shadow ui-btn ui-corner-all ui-btn-b ui-btn-inline ui-mini" onclick="DisableEnableAccount('disable')">是</a>
            <a href="#" data-rel="back" class="ui-shadow ui-btn ui-corner-all ui-btn-inline ui-mini">否</a>
        </div>
        <div class="ui-content" id="PG_1_PP" data-role="popup" data-theme="a" data-position-to="window">
            <p></p>
        </div>
    </div>
    
    <div data-role="page" id="PG_1_LG">
        <div data-role="header" data-theme="b" data-position="fixed" data-tap-toggle="false">
            <h1>登录管理面板</h1>
        </div>
        <div role="main" class="ui-content">
            <p>请输入登录密码</p>
            <input id="PG_1_LG_PW" type="password" data-theme="a" maxlength="<%=最大值_常量集合.密码长度%>" />
            <div class="ui-grid-a">
                <div class="ui-block-a"><a href="#" class="ui-btn ui-corner-all ui-btn-b" onclick="AdminLogin()">确定</a></div>
                <div class="ui-block-b"><a href="#" data-rel="back" class="ui-btn ui-corner-all">后退</a></div>
            </div>
        </div>
        <div class="ui-content" id="PG_1_LG_PP" data-role="popup" data-theme="a" data-position-to="window">
            <p></p>
        </div>
    </div>
    
    <div data-role="page" id="PG_1_RD">
        <div data-role="header" data-theme="b" data-position="fixed" data-tap-toggle="false">
            <h1>修改职能</h1>
        </div>
        <div role="main" class="ui-content">
            <fieldset data-role="controlgroup">
                <input name="PG_1_RD_Z" id="PG_1_RD_Z" type="checkbox" />
                <label for="PG_1_RD_Z">站长</label>
                <input name="PG_1_RD_F" id="PG_1_RD_F" type="checkbox" />
                <label for="PG_1_RD_F">副站长</label>
            </fieldset>
            <div class="ui-grid-a">
                <div class="ui-block-a"><a href="#" class="ui-btn ui-corner-all ui-btn-b" onclick="ReviseDuty()">确定</a></div>
                <div class="ui-block-b"><a href="#" data-rel="back" class="ui-btn ui-corner-all">后退</a></div>
            </div>
        </div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar">
                <ul>
                    <li><a href="#PG_1" class="ui-btn-active ui-state-persist">用户列表</a></li>
                </ul>
            </div>
            <h4 style="display:none;"></h4>
        </div>
        <div class="ui-content" id="PG_1_RD_PP" data-role="popup" data-theme="a" data-position-to="window">
            <p></p>
        </div>
    </div>
    
    <div data-role="page" id="PG_2">
        <div data-role="header" data-theme="b" data-position="fixed" data-tap-toggle="false">
            <h1>讯宝网络管理中心</h1>
        </div>
        <div role="main" class="ui-content">
            <select id="PG_2_T" data-native-menu="false" onchange="ListServers($('#PG_2_T').val(), $('#PG_2_OB').val(), $.trim($('#PG_2_HN').val()))">
                <option value="<%=服务器类别_常量集合.传送服务器%>" selected="selected">传送服务器</option>
                <option value="<%=服务器类别_常量集合.大聊天群服务器%>">大聊天群服务器</option>
                <option value="<%=服务器类别_常量集合.小宇宙中心服务器%>">小宇宙中心服务器</option>
                <option value="<%=服务器类别_常量集合.小宇宙写入服务器%>">小宇宙写入服务器</option>
                <option value="<%=服务器类别_常量集合.小宇宙读取服务器%>">小宇宙读取服务器</option>
                <option value="<%=服务器类别_常量集合.视频通话服务器%>">视频通话服务器</option>
            </select>
            <div class="ui-grid-b">
                <div class="ui-block-a"><a class="ui-btn ui-corner-all" href="#" onclick="ListServers(PG_2_T, PG_2_OB, PG_2_HN, PG_2_PN - 1)">上一页</a></div>
                <div class="ui-block-b"><a class="ui-btn ui-corner-all" href="#" onclick="ListServers(PG_2_T, PG_2_OB, PG_2_HN, PG_2_PN + 1)">下一页</a></div>
                <div class="ui-block-c"><a class="ui-btn ui-corner-all" href="#" onclick="ListServers($('#PG_2_T').val(), $('#PG_2_OB').val(), $.trim($('#PG_2_HN').val()))">查找</a></div>
            </div>
            <ul id="PG_2_DT" data-role="listview" data-inset="true" data-split-theme="b" data-split-icon="edit"></ul>
            <div class="ui-grid-b" id="PG_2_PG" style="display:none;">
                <div class="ui-block-a"><a class="ui-btn ui-corner-all" href="#" onclick="ListServers(PG_2_T, PG_2_OB, PG_2_HN, PG_2_PN - 1)">上一页</a></div>
                <div class="ui-block-b"><a class="ui-btn ui-corner-all" href="#" onclick="ListServers(PG_2_T, PG_2_OB, PG_2_HN, PG_2_PN + 1)">下一页</a></div>
                <div class="ui-block-c"><a class="ui-btn ui-corner-all" href="#PG_CP" onclick="PrepareToChangePage(PG_2_TP, '服务器列表', 'PG_2')" id="PG_2_PN"></a></div>
            </div>
            <div class="ui-grid-a" id="PG_2_HN_DV">
                <div class="ui-block-a" style="width:90%;"><input id="PG_2_HN" placeholder="主机名" data-type="search" maxlength="<%=最大值_常量集合.主机名字符数%>" style="color:red;" /></div>
                <div class="ui-block-b" style="width:10%;"><a href="#" data-role="button" data-icon="search" data-iconpos="notext" onclick="ListServers($('#PG_2_T').val(), $('#PG_2_OB').val(), $.trim($('#PG_2_HN').val()))">查找</a></div>
            </div>
            <select id="PG_2_OB" data-native-menu="false" onchange="ListServers($('#PG_2_T').val(), $('#PG_2_OB').val(), $.trim($('#PG_2_HN').val()))">
                <option value="time_j" selected="selected">按开设时间排序 从近到远</option>
                <option value="time_s">按开设时间排序 从远到近</option>
                <option value="name_s">按主机名排序 升序</option>
                <option value="name_j">按主机名排序 降序</option>
            </select>
        </div>
        <div data-role="footer" data-theme="b" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar">
                <ul>
                    <li><a href="#PG_1" data-icon="user">用户</a></li>
                    <li><a href="#PG_2" data-icon="cloud" class="ui-btn-active ui-state-persist">服务器</a></li>
                </ul>
            </div>
            <h4 style="display:none;"></h4>
        </div>
        <div id="PG_2_MN_<%=服务器类别_常量集合.传送服务器 %>" data-role="popup" data-theme="b">
            <ul style="min-width:160px;" data-role="listview" data-inset="true">
                <li><a href="#" onclick="Menu_2_<%=服务器类别_常量集合.传送服务器 %>('statistics')">统计</a></li>
                <li><a href="#" onclick="Menu_2_<%=服务器类别_常量集合.传送服务器 %>('users')">在线用户</a></li>
                <li><a href="#" onclick="Menu_2_<%=服务器类别_常量集合.传送服务器 %>('chatgroups')">聊天群</a></li>
                <li><a href="#" onclick="Menu_2_<%=服务器类别_常量集合.传送服务器 %>('ssnumber')">讯宝数量</a></li>
                <li><a href="#" onclick="Menu_2_<%=服务器类别_常量集合.传送服务器 %>('revise')">修改</a></li>
                <li><a href="#" onclick="Menu_2_<%=服务器类别_常量集合.传送服务器 %>('account')">停用/启用</a></li>
            </ul>
        </div>
        <div id="PG_2_MN_<%=服务器类别_常量集合.大聊天群服务器 %>" data-role="popup" data-theme="b">
            <ul style="min-width:160px;" data-role="listview" data-inset="true">
                <li><a href="#" onclick="Menu_2_<%=服务器类别_常量集合.大聊天群服务器 %>('chatgroups')">聊天群</a></li>
                <li><a href="#" onclick="Menu_2_<%=服务器类别_常量集合.大聊天群服务器 %>('ssnumber')">讯宝数量</a></li>
                <li><a href="#" onclick="Menu_2_<%=服务器类别_常量集合.大聊天群服务器 %>('revise')">修改</a></li>
                <li><a href="#" onclick="Menu_2_<%=服务器类别_常量集合.大聊天群服务器 %>('account')">停用/启用</a></li>
            </ul>
        </div>
        <div id="PG_2_MN_<%=服务器类别_常量集合.小宇宙中心服务器 %>" data-role="popup" data-theme="b">
            <ul style="min-width:160px;" data-role="listview" data-inset="true">
                <li><a href="#" onclick="Menu_2_<%=服务器类别_常量集合.小宇宙中心服务器 %>('visitors')">访问者</a></li>
                <li><a href="#" onclick="Menu_2_<%=服务器类别_常量集合.小宇宙中心服务器 %>('goods')">商品</a></li>
                <li><a href="#" onclick="Menu_2_<%=服务器类别_常量集合.小宇宙中心服务器 %>('meterorains')">流星语</a></li>
                <li><a href="#" onclick="Menu_2_<%=服务器类别_常量集合.小宇宙中心服务器 %>('revise')">修改</a></li>
                <li><a href="#" onclick="Menu_2_<%=服务器类别_常量集合.小宇宙中心服务器 %>('account')">停用/启用</a></li>
            </ul>
        </div>
        <div data-role="popup" id="PG_2_EB" data-theme="a" data-overlay-theme="b" class="ui-content" style="max-width:340px; padding-bottom:2em;">
            <h3>启用此账号吗？</h3>
            <a href="#" class="ui-shadow ui-btn ui-corner-all ui-btn-b ui-btn-inline ui-mini" onclick="DisableEnableServer('enable')">是</a>
            <a href="#" data-rel="back" class="ui-shadow ui-btn ui-corner-all ui-btn-inline ui-mini">否</a>
        </div>
        <div data-role="popup" id="PG_2_DB" data-theme="a" data-overlay-theme="b" class="ui-content" style="max-width:340px; padding-bottom:2em;">
            <h3>停用此账号吗？</h3>
            <a href="#" class="ui-shadow ui-btn ui-corner-all ui-btn-b ui-btn-inline ui-mini" onclick="DisableEnableServer('disable')">是</a>
            <a href="#" data-rel="back" class="ui-shadow ui-btn ui-corner-all ui-btn-inline ui-mini">否</a>
        </div>
        <div class="ui-content" id="PG_2_PP" data-role="popup" data-theme="a" data-position-to="window">
            <p></p>
        </div>
    </div>

    <div data-role="page" id="PG_2_VD">
        <div data-role="header" data-theme="b" data-position="fixed" data-tap-toggle="false">
            <h1>访客详情 - <span id="PG_2_VD_SD"></span></h1>
        </div>
        <div role="main" class="ui-content">
            <a href="#" data-rel="back" class="ui-btn ui-corner-all">后退</a>
            <select id="WD_MA_GN" data-native-menu="false">
                <option selected="selected" value="today">今天</option>
                <option value="yesterday">昨天</option>
                <option value="1beforeyest">前天</option>
                <option value="2beforeyest">大前天</option>
            </select>
            <select id="RG_MA_GN" data-native-menu="false">
                <option selected="selected" value="all">全部访客</option>
                <option value="bylink">从其它网页跳转至本站的访客</option>
            </select>
            <div class="ui-grid-b">
                <div class="ui-block-a"><a class="ui-btn ui-corner-all" href="#" onclick="ListVisitors(WD_MA_GN, RG_MA_GN, ET_MA_GN, PG_2_VD_PN - 1)">上一页</a></div>
                <div class="ui-block-b"><a class="ui-btn ui-corner-all" href="#" onclick="ListVisitors(WD_MA_GN, RG_MA_GN, ET_MA_GN, PG_2_VD_PN + 1)">下一页</a></div>
                <div class="ui-block-c"><a class="ui-btn ui-corner-all" href="#" onclick="ListVisitors($('#WD_MA_GN').val(), $('#RG_MA_GN').val(), ET_MA_GN)">查找</a></div>
            </div>
            <ul id="PG_2_VD_DT" data-role="listview" data-inset="true" data-split-theme="b" data-split-icon="search"></ul>
            <div class="ui-grid-b" id="PG_2_VD_PG" style="display:none;">
                <div class="ui-block-a"><a class="ui-btn ui-corner-all" href="#" onclick="ListVisitors(WD_MA_GN, RG_MA_GN, ET_MA_GN, PG_2_VD_PN - 1)">上一页</a></div>
                <div class="ui-block-b"><a class="ui-btn ui-corner-all" href="#" onclick="ListVisitors(WD_MA_GN, RG_MA_GN, ET_MA_GN, PG_2_VD_PN + 1)">下一页</a></div>
                <div class="ui-block-c"><a class="ui-btn ui-corner-all" href="#PG_CP" onclick="PrepareToChangePage(PG_2_VD_TP, '访客详情', 'PG_2_VD')" id="PG_2_VD_PN"></a></div>
            </div>
        </div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar">
                <ul>
                    <li><a href="#PG_2" class="ui-btn-active ui-state-persist">服务器列表</a></li>
                </ul>
            </div>
            <h4 style="display:none;"></h4>
        </div>
        <div class="ui-content" id="PG_2_VD_PP" data-role="popup" data-theme="a" data-position-to="window">
            <p></p>
        </div>
    </div>

    <div data-role="page" id="PG_2_RS">
        <div data-role="header" data-theme="b" data-position="fixed" data-tap-toggle="false">
            <h1>修改服务器账号</h1>
        </div>
        <div role="main" class="ui-content">
            <p>主机名：<span id="PG_2_RS_DM"></span></p>
            <label for="PG_2_RS_IP">IP（为0.0.0.0时，删除该服务器账号）</label>
            <input id="PG_2_RS_IP" data-type="text" maxlength="34" value="" />
            <div class="ui-grid-a">
                <div class="ui-block-a"><a href="#" class="ui-btn ui-corner-all ui-btn-b" onclick="ReviseServer()">确定</a></div>
                <div class="ui-block-b"><a href="#" data-rel="back" class="ui-btn ui-corner-all">后退</a></div>
            </div>
        </div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar">
                <ul>
                    <li><a href="#PG_2" class="ui-btn-active ui-state-persist">服务器列表</a></li>
                </ul>
            </div>
            <h4 style="display:none;"></h4>
        </div>
        <div class="ui-content" id="PG_2_RS_PP" data-role="popup" data-theme="a" data-position-to="window">
            <p></p>
        </div>
    </div>

    <div data-role="page" id="PG_CP">
        <div data-role="header" data-theme="b" data-position="fixed" data-tap-toggle="false">
            <h1>跳转至</h1>
        </div>
        <div role="main" class="ui-content">
            <p>请输入介于1和<span id="PG_CP_TP"></span>之间的数字：</p>
            <input id="PG_CP_PN" type="text" value="" data-theme="a" maxlength="10" />
            <div class="ui-grid-a">
                <div class="ui-block-a"><a href="#" class="ui-btn ui-corner-all ui-btn-b" onclick="ChangePage()">确定</a></div>
                <div class="ui-block-b"><a href="#" data-rel="back" class="ui-btn ui-corner-all">取消</a></div>
            </div>
        </div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar">
                <ul>
                    <li><a id="PG_CP_FT" href="#" class="ui-btn-active ui-state-persist"></a></li>
                </ul>
            </div>
            <h4 style="display:none;"></h4>
        </div>
    </div>
    
</body>
</html>
