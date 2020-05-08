<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Default.aspx.vb" Inherits="SSignal_TinyUniverse.MyTinyUniverse" %>
<%@ Import Namespace="SSignal_Protocols" %>
<%@ Import Namespace="SSignal_GlobalCommonCode"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>我的小宇宙</title>
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
        .MeteorRain {
            margin-top:20px;
            cursor: pointer;
        }
        .Title {
            font-weight: bold;
        }
        .Comment {
            margin-top: 10px;
            cursor: pointer;
        }
        .Note {
            font-size:small;
        }
        .Reply {
            margin-top: 10px;
        }
        .ReplyTo {
            margin-top: 10px;
            margin-left: 50px;
            padding: 10px 5px;
            border: 1px solid lightgray;
            border-radius: 5px 5px;
        }
        .Paragraph {
            padding:10px 5px;
            margin-top:10px;
            margin-bottom:0px;
            border:1px solid lightgray;
        }
        .Price {
            color:red;
        }
        -->
    </style>

    <script>

        var EnglishSSAddress, ReadCredential, WriteCredential, EnglishUsername;
        var JumpToPage, EditWhat = 0, IsGoodsEditor = false;
        var CurrentMeteorRainID, CurrentCommentID, CurrentReplyID, CurrentGoodsID;
        <%=OtherJS()%>

        $(document).on("pageshow", "#PG_0", function () { window.external.RequestReadCredential(Domain_Read); });
        $(document).on("pageshow", "#PG_1", function () { ListMeteorRains(undefined, true); });
        $(document).on("pageshow", "#PG_2", function () { ListGoods(undefined, true); });
        $(document).on("pageshow", "#PG_10", function () { window.external.RequestWriteCredential(Domain_Write); });
        $(document).on("pageshow", "#PG_11", function () { window.external.GetTags("PG_11"); });
        $(document).on("pageshow", "#PG_12", function () { window.external.GetTags("PG_12"); });

        function PrepareToChangePage(Totalpages) {
            $("#PG_CP_TP").text(Totalpages);
            $("#PG_CP_PN").val("");
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


        var PG_1_PN = 1, PG_1_TP = 0;
        function ListMeteorRains(PageNumber, IsPageShow) {
            if (IsPageShow == true) {
                if (IsNullorEmpty(JumpToPage)) {
                    if ($("#PG_1_DT").html() != "") { return; }
                } else {
                    PageNumber = JumpToPage;
                    JumpToPage = undefined;
                }
            }
            if (PageNumber == undefined) {
                PageNumber = 1;
            } else if (PageNumber < 1) {
                return;
            } else if (PageNumber > PG_1_TP) {
                return;
            }
            var TimezoneOffset = -(new Date().getTimezoneOffset());
            var popupid = "PG_1_PP";
            var urlpart = "?C=ListMeteorRains&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&EnglishUsername=" + encodeURIComponent(EnglishUsername) + "&PageNumber=" + PageNumber + "&TimezoneOffset=" + TimezoneOffset;
            RequestServer(Domain_Read, urlpart, popupid, function (response) {
                if (response == null) {
                    PG_1_PN = 1;
                    PG_1_TP = 0;
                    $("#PG_1_DT").html("");
                    $("#PG_1_PN").text("");
                    $("#PG_1_PG").hide();
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    var s = "";
                    $R.find("METEORRAIN").each(function (i) {
                        var $R2 = $(this);
                        var ID = $R2.find("ID").text();
                        var TYPE = $R2.find("TYPE").text();
                        var STYLE = $R2.find("STYLE").text();
                        var TITLE = $R2.find("TITLE").text();
                        var DATE = $R2.find("DATE").text();
                        var STICKY;
                        var $R3 = $R2.find("STICKY");
                        if ($R3.length > 0) {
                            STICKY = "<a href='#PG_1_CS' data-theme='a' data-rel='popup' data-position-to='window' onclick='setMeteorRainID(\"" + ID + "\")'>取消置顶</a> ";
                        } else {
                            STICKY = "<a href='#PG_1_SK' data-theme='a' data-rel='popup' data-position-to='window' onclick='setMeteorRainID(\"" + ID + "\")'>置顶</a> ";
                        }
                        var tag;
                        $R3 = $R2.find("TAG");
                        if ($R3.length > 0) {
                            tag = $R3.text();
                        }
                        var PERMISSION = "<a id='P" + ID +"' href='#PG_12' onclick='setMeteorRainID(\"" + ID + "\")'>" + GetPermissionText(Number($R2.find("PERMISSION").text()), tag) + "</a> ";
                        switch (Number(TYPE)) {
                            case <%=流星语类型_常量集合.图文%>:
                                var COMMENTS = $R2.find("COMMENTS").text();
                                var LIKES = $R2.find("LIKES").text();
                                s += "<div class='MeteorRain' id='" + ID + "' onclick='GetMeteorRain(\"" + ID + "\")'>";
                                switch (Number(STYLE)) {
                                    case <%=流星语列表项样式_常量集合.一幅小图片 %>:
                                        var src = "../media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&EnglishUsername=" + encodeURIComponent(EnglishUsername) + "&FileName=" + ID + "_1_pre.jpg";
                                        s += "<table><tr><td valign='top'><img src='" + src + "' style='cursor: pointer;' /></td><td valign='top' style='padding-left:10px;'><span class='Title'>" + TITLE + "</span></td><tr></table>";
                                        break;
                                    case <%=流星语列表项样式_常量集合.三幅小图片 %>:
                                        s += "<span class='Title'>" + TITLE + "</span><br>";
                                        var src = "../media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&EnglishUsername=" + encodeURIComponent(EnglishUsername) + "&FileName=" + ID + "_";
                                        s += "<table style='width:100%;margin-top:10px;'><tr><td valign='top'><img style='width:100%;height:auto;cursor: pointer;' src='" + src + "1_pre.jpg' /></td><td valign='top' style='padding-left:10px;'><img style='width:100%;height:auto;cursor: pointer;' src='" + src + "2_pre.jpg' /></td><td valign='top' style='padding-left:10px;'><img style='width:100%;height:auto;cursor: pointer;' src='" + src + "3_pre.jpg' /></td><tr></table>";
                                        break;
                                    case <%=流星语列表项样式_常量集合.一幅大图片 %>:
                                        var src = "../media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&EnglishUsername=" + encodeURIComponent(EnglishUsername) + "&FileName=" + ID + "_1_pre.jpg";
                                        s += "<span class='Title'>" + TITLE + "</span><br><img style='width:100%;height:auto;margin-top:10px;cursor: pointer;' src='" + src + "' /><br>";
                                        break;
                                    default:
                                        s += "<span class='Title'>" + TITLE + "</span><br>";
                                }
                                s += "</div><div id='M" +ID + "'><span class='Note'>评论 " + COMMENTS + " 点赞 " + LIKES + " <a href='#PG_1_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setMeteorRainID(\"" + ID + "\")'>删除</a> " + STICKY + PERMISSION + "<br>" + DATE + "</span></div>";
                                break;
                            case <%=流星语类型_常量集合.视频%>:
                                var COMMENTS = $R2.find("COMMENTS").text();
                                var LIKES = $R2.find("LIKES").text();
                                s += "<div class='MeteorRain' id='" + ID + "' onclick='GetMeteorRain(\"" + ID + "\")'>";
                                switch (Number(STYLE)) {
                                    case <%=流星语列表项样式_常量集合.一幅小图片 %>:
                                        var src = "../media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&EnglishUsername=" + encodeURIComponent(EnglishUsername) + "&FileName=" + ID + ".jpg";
                                        s += "<table><tr><td valign='top'><img src='" + src + "' style='cursor: pointer;' /></td><td valign='top' style='padding-left:10px;'><span class='Title'>" + TITLE + "</span></td><tr></table>";
                                        break;
                                   case <%=流星语列表项样式_常量集合.一幅大图片 %>:
                                        var src = "../media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&EnglishUsername=" + encodeURIComponent(EnglishUsername) + "&FileName=" + ID + ".jpg";
                                        s += "<span class='Title'>" + TITLE + "</span><br><img style='width:100%;height:auto;margin-top:10px;cursor: pointer;' src='" + src + "' /><br>";
                                        break;
                                }
                                s += "</div><div id='M" + ID + "'><span class='Note'>评论 " + COMMENTS + " 点赞 " + LIKES + " <a href='#PG_1_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setMeteorRainID(\"" + ID + "\")'>删除</a> " + STICKY + PERMISSION + "<br>" + DATE + "</span></div>";
                                break;
                            case <%=流星语类型_常量集合.转发%>:
                                var DOMAIN = $R2.find("DOMAIN");
                                var FID = $R2.find("FID");

                                break;
                        }
                    });
                    $("#PG_1_DT").html(s);
                    PG_1_PN = Number($R.find("PAGENUMBER").text());
                    PG_1_TP = Number($R.find("TOTALPAGES").text());
                    if (IsNullorEmpty(s)) {
                        $("#PG_1_PG").hide();
                    } else {
                        $("#PG_1_PG").show();
                    }
                    $("#PG_1_PN").text(PG_1_TP);
                    $.mobile.silentScroll(0);
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function GetMeteorRain(ID) {
            var TimezoneOffset = -(new Date().getTimezoneOffset());
            var popupid = "PG_1_PP";
            var urlpart = "?C=GetMeteorRain&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&MeteorRainID=" + ID + "&TimezoneOffset=" + TimezoneOffset;
            RequestServer(Domain_Read, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    var TYPE = Number($R.find("TYPE").text());
                    if (TYPE != <%=流星语类型_常量集合.转发%>) {
                        $.mobile.changePage("#PG_1_VM");
                        CurrentMeteorRainID = ID;
                        CommentsEarlierThan = "0";
                        var ENGLISH = $R.find("ENGLISH2").text();
                        var AUTHOR;
                        var $R3 = $R.find("NATIVE2");
                        if ($R3.length > 0) {
                            AUTHOR = $R3.text() + " / " + ENGLISH;
                        } else {
                            AUTHOR = ENGLISH;
                        }
                        $("#PG_1_VM_TT").html($R.find("TITLE").text())
                        var COMMENTS = $R.find("COMMENTS").text();
                        var LIKES = $R.find("LIKES2").text();
                        var DATE = $R.find("DATE2").text();
                        $("#PG_1_VM_NT").html("评论 " + COMMENTS + " 点赞 " + LIKES + "<br>" + AUTHOR + " " + DATE);
                        var s = "";
                        switch (TYPE) {
                            case <%=流星语类型_常量集合.图文%>:
                                var j = 0;
                                var src = "../media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&EnglishUsername=" + encodeURIComponent(EnglishUsername) + "&FileName=" + ID + "_";
                                var BODY = $R.find("BODY2");
                                BODY.children().each(function (i) {
                                    var $R2 = $(this);
                                    switch ($R2[0].tagName) {
                                        case "P":
                                            s += "<p>" + $R2.text() + "</p>";
                                            break;
                                        case "IMG":
                                            j += 1;
                                            s += "<img style='width:100%;height:auto;' src='" + src + j + "." + $R2.text() + "' />";
                                            break
                                    }
                                });
                                break;
                            case <%=流星语类型_常量集合.视频%>:
                                var src = "../media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&EnglishUsername=" + encodeURIComponent(EnglishUsername) + "&FileName=" + ID + ".jpg";
                                s = "<img style='width:100%;height:auto;cursor:pointer;' src='" + src + "' onclick='PlayVideo(\"" + ID + "\")' />";
                                break;
                        }
                        $("#PG_1_VM_BD").html(s);
                        var CommentArea = document.getElementById("PG_1_VM_CM");
                        CommentArea.innerHTML = "";
                        $R.find("COMMENT").each(function (i) {
                            var $R2 = $(this);
                            ID = $R2.find("ID").text();
                            var ENGLISH = $R2.find("ENGLISH").text();
                            var AUTHOR;
                            if (IsNullorEmpty(ENGLISH)) {
                                AUTHOR = "本文作者";
                            } else if (ENGLISH == EnglishSSAddress) {
                                AUTHOR = "我";
                            } else {
                                var $R3 = $R2.find("NATIVE");
                                if ($R3.length > 0) {
                                    AUTHOR = $R3.text() + " / " + ENGLISH;
                                } else {
                                    AUTHOR = ENGLISH;
                                }
                            }
                            var BODY = $R2.find("BODY").text();
                            var REPLIES = $R2.find("REPLIES").text();
                            var LIKES = $R2.find("LIKES").text();
                            var DATE = $R2.find("DATE").text();
                            var s;
                            if (!IsNullorEmpty(ENGLISH)) {
                                if (ENGLISH != EnglishSSAddress) {
                                    s = " <a href='#' id='PG_1_VC_LK' onclick='LikeComment(\"" + ID + "\")'>点赞</a> ";
                                } else {
                                    s = " ";
                                }
                            } else {
                                if ((EnglishUsername + "<%=讯宝地址标识 %>" + EnglishDomain) != EnglishSSAddress) {
                                    s = " <a href='#' id='PG_1_VC_LK' onclick='LikeComment(\"" + ID + "\")'>点赞</a> ";
                                } else {
                                    s = " ";
                                }
                            }
                            var div = document.createElement("div");
                            div.id = "C" + ID;
                            div.innerHTML = "<div class='Comment' onclick='GetComment(\"" + ID + "\")'><span class='Note'>" + AUTHOR + "</span><br><span>" + BODY + "</span></div><span class='Note'>回复 " + REPLIES + " 点赞 " + LIKES + s + DATE + " <a href='#PG_1_VM_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setCommentID(\"" + ID + "\")'>删除</a></span>";
                            CommentArea.appendChild(div)
                        });
                        CommentsEarlierThan = ID;
                    } else {

                    }
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function LikeMeteorRain() {
            if ($("#PG_1_VM_LK").text() == "已点赞") { return; }
            if (IsNullorEmpty(WriteCredential)) {
                EditWhat = 0;
                $.mobile.changePage("#PG_10");
                return;
            }
            var popupid = "PG_1_VM_PP";
            var urlpart = "?C=PostComment&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(WriteCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&Domain_Read=" + encodeURIComponent(Domain_Read);
            RequestServer(Domain_Write, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    $("#PG_1_VM_LK").text("已点赞");
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function PostComment() {
            if (IsNullorEmpty(WriteCredential)) {
                EditWhat = 0;
                $.mobile.changePage("#PG_10");
                return;
            }
            var text = $("#PG_1_VM_TX").val();
            if (IsNullorEmpty(text)) { return; }
            var TimezoneOffset = -(new Date().getTimezoneOffset());
            var popupid = "PG_1_VM_PP";
            var urlpart = "?C=PostComment&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(WriteCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&Text=" + encodeURIComponent(text) + "&TimezoneOffset=" + TimezoneOffset + "&Domain_Read=" + encodeURIComponent(Domain_Read);
            RequestServer(Domain_Write, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    $("#PG_1_VM_TX").val("");
                    var ID = $R.find("ID").text();
                    var DATE = $R.find("DATE").text();
                   var div = document.createElement("div");
                    div.id = "C" + ID;
                    div.innerHTML = "<div class='Comment' onclick='GetComment(\"" + ID + "\")'><span class='Note'>我</span><br><span>" + text + "</span></div><span class='Note'>回复 0 点赞 0 " + DATE + " <a href='#PG_1_VM_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setCommentID(\"" + ID + "\")'>删除</a></span>";
                    var CommentArea = document.getElementById("PG_1_VM_CM");
                    var first = CommentArea.firstElementChild;
                    CommentArea.insertBefore(div, first);
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function DeleteComment() {
            $("#PG_1_VM_DL").popup("close");
            if (IsNullorEmpty(WriteCredential)) {
                EditWhat = 0;
                $.mobile.changePage("#PG_10");
                return;
            }
            var popupid = "PG_1_VM_PP";
            var urlpart = "?C=DeleteComment&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(WriteCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&CommentID=" + CurrentCommentID + "&Domain_Read=" + encodeURIComponent(Domain_Read);
            RequestServer(Domain_Write, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    $("#C" + CurrentCommentID).remove();
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        var CommentsEarlierThan = "0";
        var RepliesEarlierThan = "0";
        function MoreComments() {
            var TimezoneOffset = -(new Date().getTimezoneOffset());
            var popupid = "PG_1_VM_PP";
            var urlpart = "?C=MoreComments&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&EarlierThan=" + CommentsEarlierThan + "&TimezoneOffset=" + TimezoneOffset;
            RequestServer(Domain_Read, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    var CommentArea = document.getElementById("PG_1_VM_CM");
                    var ID;
                    $R.find("COMMENT").each(function (i) {
                        var $R2 = $(this);
                        ID = $R2.find("ID").text();
                        var ENGLISH = $R2.find("ENGLISH").text();
                        var AUTHOR;
                        if (IsNullorEmpty(ENGLISH)) {
                            AUTHOR = "本文作者";
                        } else if (ENGLISH == EnglishSSAddress) {
                            AUTHOR = "我";
                        } else {
                            var $R3 = $R2.find("NATIVE");
                            if ($R3.length > 0) {
                                AUTHOR = $R3.text() + " / " + ENGLISH;
                            } else {
                                AUTHOR = ENGLISH;
                            }
                        }
                        var BODY = $R2.find("BODY").text();
                        var REPLIES = $R2.find("REPLIES").text();
                        var LIKES = $R2.find("LIKES").text();
                        var DATE = $R2.find("DATE").text();
                        var s;
                        if (!IsNullorEmpty(ENGLISH)) {
                            if (ENGLISH != EnglishSSAddress) {
                                s = " <a href='#' id='PG_1_VC_LK' onclick='LikeComment(\"" + ID + "\")'>点赞</a> ";
                            } else {
                                s = " ";
                            }
                        } else {
                            if ((EnglishUsername + "<%=讯宝地址标识 %>" + EnglishDomain) != EnglishSSAddress) {
                                s = " <a href='#' id='PG_1_VC_LK' onclick='LikeComment(\"" + ID + "\")'>点赞</a> ";
                            } else {
                                s = " ";
                            }
                        }
                        var div = document.createElement("div");
                        div.id = "C" + ID;
                        div.innerHTML = "<div class='Comment' onclick='GetComment(\"" + ID + "\")'><span class='Note'>" + AUTHOR + "</span><br><span>" + BODY + "</span></div><span class='Note'>回复 " + REPLIES + " 点赞 " + LIKES + s + DATE + " <a href='#PG_1_VM_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setCommentID(\"" + ID + "\")'>删除</a></span>";
                        CommentArea.appendChild(div)
                    });
                    CommentsEarlierThan = ID;
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function GetComment(ID) {
            var TimezoneOffset = -(new Date().getTimezoneOffset());
            var popupid = "PG_1_VM_PP";
            var urlpart = "?C=MoreReplies&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&CommentID=" + ID + "&EarlierThan=0&TimezoneOffset=" + TimezoneOffset;
            RequestServer(Domain_Read, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    $.mobile.changePage("#PG_1_VC");
                    CurrentCommentID = ID;
                    var $R2 = $R.find("COMMENT");
                    var ENGLISH = $R2.find("ENGLISH").text();
                    var AUTHOR;
                    if (IsNullorEmpty(ENGLISH)) {
                        AUTHOR = "本文作者";
                    } else if (ENGLISH == EnglishSSAddress) {
                        AUTHOR = "我";
                    } else {
                        var $R3 = $R2.find("NATIVE");
                        if ($R3.length > 0) {
                            AUTHOR = $R3.text() + " / " + ENGLISH;
                        } else {
                            AUTHOR = ENGLISH;
                        }
                    }
                    var BODY = $R2.find("BODY").text();
                    var REPLIES = $R2.find("REPLIES").text();
                    var LIKES = $R2.find("LIKES").text();
                    var DATE = $R2.find("DATE").text();
                    var s = "<span class='Note'>" + AUTHOR + "</span><br><span>" + BODY + "</span><br><span class='Note'>回复 " + REPLIES + " 点赞 " + LIKES + " " + DATE + " <a href='#PG_1_RP' onclick='setReplyID()'>回复</a></span>";
                    $("#PG_1_VC_DT").html(s);
                    var ReplyArea = document.getElementById("PG_1_VC_RP");
                    ReplyArea.innerHTML = "";
                    $R.find("REPLY").each(function (i) {
                        var $R2 = $(this);
                        ID = $R2.find("ID").text();
                        var ENGLISH = $R2.find("ENGLISH").text();
                        var AUTHOR;
                        if (IsNullorEmpty(ENGLISH)) {
                            AUTHOR = "本文作者";
                        } else if (ENGLISH == EnglishSSAddress) {
                            AUTHOR = "我";
                        } else {
                            var $R3 = $R2.find("NATIVE");
                            if ($R3.length > 0) {
                                AUTHOR = $R3.text() + " / " + ENGLISH;
                            } else {
                                AUTHOR = ENGLISH;
                            }
                        }
                        var BODY = $R2.find("BODY").text();
                        var LIKES = $R2.find("LIKES").text();
                        var DATE = $R2.find("DATE").text();
                        var $R4 = $R2.find("TOENGLISH");
                        var s;
                        if ($R4.length > 0) {
                            var TOENGLISH = $R4.text()
                            var TOWHOM;
                            if (IsNullorEmpty(TOENGLISH)) {
                                TOWHOM = "本文作者";
                            } else if (TOENGLISH == EnglishSSAddress) {
                                TOWHOM = "我";
                            } else {
                                var $R3 = $R2.find("TONATIVE");
                                if ($R3.length > 0) {
                                    TOWHOM = $R3.text() + " / " + TOENGLISH;
                                } else {
                                    TOWHOM = TOENGLISH;
                                }
                            }
                            var TOBODY = $R2.find("TOBODY").text();
                            s = "<div class='ReplyTo'><span class='Note'>" + TOWHOM + "</span><br><span>" + TOBODY + "</span></div>";
                        } else {
                            s = "";
                        }
                        var ss;
                        if (!IsNullorEmpty(ENGLISH)) {
                            if (ENGLISH != EnglishSSAddress) {
                                ss = " <a href='#' id='PG_1_RP_LK' onclick='LikeReply(\"" + ID + "\")'>点赞</a> ";
                            } else {
                                ss = " ";
                            }
                        } else {
                            if ((EnglishUsername + "<%=讯宝地址标识 %>" + EnglishDomain) != EnglishSSAddress) {
                                ss = " <a href='#' id='PG_1_RP_LK' onclick='LikeReply(\"" + ID + "\")'>点赞</a> ";
                            } else {
                                ss = " ";
                            }
                        }
                        var div = document.createElement("div");
                        div.id = "R" + ID;
                        div.className = "Reply";
                        div.innerHTML = "<span class='Note'>" + AUTHOR + "</span><br><span>" + BODY + "</span><br><span class='Note'>点赞 " + LIKES + ss + DATE + " <a href='#PG_1_RP' onclick='setReplyID(\"" + ID + "\")'>回复</a> <a href='#PG_1_VC_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setReplyID(\"" + ID + "\")'>删除</a></span>" + s;
                        ReplyArea.appendChild(div)
                    });
                    RepliesEarlierThan = ID;
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function LikeComment(ID) {
            if (IsNullorEmpty(WriteCredential)) {
                EditWhat = 0;
                $.mobile.changePage("#PG_10");
                return;
            }
            if ($("#PG_1_VC_LK").text() == "已点赞") { return; }
            var popupid = "PG_1_VM_PP";
            var urlpart = "?C=PostReply&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(WriteCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&CommentID=" + ID + "&Domain_Read=" + encodeURIComponent(Domain_Read);
            RequestServer(Domain_Write, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    $("#PG_1_VC_LK").text("已点赞");
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function MoreReplies() {
            var TimezoneOffset = -(new Date().getTimezoneOffset());
            var popupid = "PG_1_VC_PP";
            var urlpart = "?C=MoreReplies&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&CommentID=" + CurrentCommentID + "&EarlierThan=" + RepliesEarlierThan + "&TimezoneOffset=" + TimezoneOffset;
            RequestServer(Domain_Read, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    var ReplyArea = document.getElementById("PG_1_VC_RP");
                    var ID;
                    $R.find("REPLY").each(function (i) {
                        var $R2 = $(this);
                        ID = $R2.find("ID").text();
                        var ENGLISH = $R2.find("ENGLISH").text();
                        var AUTHOR;
                        if (IsNullorEmpty(ENGLISH)) {
                            AUTHOR = "本文作者";
                        } else if (ENGLISH == EnglishSSAddress) {
                            AUTHOR = "我";
                        } else {
                            var $R3 = $R2.find("NATIVE");
                            if ($R3.length > 0) {
                                AUTHOR = $R3.text() + " / " + ENGLISH;
                            } else {
                                AUTHOR = ENGLISH;
                            }
                        }
                        var BODY = $R2.find("BODY").text();
                        var LIKES = $R2.find("LIKES").text();
                        var DATE = $R2.find("DATE").text();
                        var $R4 = $R2.find("TOENGLISH");
                        var s;
                        if ($R4.length > 0) {
                            var TOENGLISH = $R4.text()
                            var TOWHOM;
                            if (IsNullorEmpty(TOENGLISH)) {
                                TOWHOM = "本文作者";
                            } else if (TOENGLISH == EnglishSSAddress) {
                                TOWHOM = "我";
                            } else {
                                var $R3 = $R2.find("TONATIVE");
                                if ($R3.length > 0) {
                                    TOWHOM = $R3.text() + " / " + TOENGLISH;
                                } else {
                                    TOWHOM = TOENGLISH;
                                }
                            }
                            var TOBODY = $R2.find("TOBODY").text();
                            s = "<div class='ReplyTo'><span class='Note'>" + TOWHOM + "</span><br><span>" + TOBODY + "</span></div>";
                        } else {
                            s = "";
                        }
                        var ss;
                        if (!IsNullorEmpty(ENGLISH)) {
                            if (ENGLISH != EnglishSSAddress) {
                                ss = " <a href='#' id='PG_1_RP_LK' onclick='LikeReply(\"" + ID + "\")'>点赞</a> ";
                            } else {
                                ss = " ";
                            }
                        } else {
                            if ((EnglishUsername + "<%=讯宝地址标识 %>" + EnglishDomain) != EnglishSSAddress) {
                                ss = " <a href='#' id='PG_1_RP_LK' onclick='LikeReply(\"" + ID + "\")'>点赞</a> ";
                            } else {
                                ss = " ";
                            }
                        }
                        var div = document.createElement("div");
                        div.id = "R" + ID;
                        div.className = "Reply";
                        div.innerHTML = "<span class='Note'>" + AUTHOR + "</span><br><span>" + BODY + "</span><br><span class='Note'>点赞 " + LIKES + ss + DATE + " <a href='#PG_1_RP' onclick='setReplyID(\"" + ID + "\")'>回复</a> <a href='#PG_1_VC_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setReplyID(\"" + ID + "\")'>删除</a></span>" + s;
                        ReplyArea.appendChild(div)
                    });
                    RepliesEarlierThan = ID;
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function PostReply() {
            if (IsNullorEmpty(WriteCredential)) {
                EditWhat = 0;
                $.mobile.changePage("#PG_10");
                return;
            }
            var text = $("#PG_1_RP_TX").val();
            if (IsNullorEmpty(text)) { return; }
            var TimezoneOffset = -(new Date().getTimezoneOffset());
            var popupid = "PG_1_RP_PP";
            var s;
            if (!IsNullorEmpty(CurrentReplyID)) {
                s = "&ReplyID=" + CurrentReplyID;
            } else {
                s = "";
            }
            var urlpart = "?C=PostReply&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(WriteCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&CommentID=" + CurrentCommentID + s + "&Text=" + encodeURIComponent(text) + "&TimezoneOffset=" + TimezoneOffset + "&Domain_Read=" + encodeURIComponent(Domain_Read);
            RequestServer(Domain_Write, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    $("#PG_1_RP_TX").val("");
                    history.back(-1);
                    var ID = $R.find("ID").text();
                    var DATE = $R.find("DATE").text();
                    var $R4 = $R.find("TOENGLISH");
                    var s;
                    if ($R4.length > 0) {
                        var TOENGLISH = $R4.text()
                        var TOWHOM;
                        if (IsNullorEmpty(TOENGLISH)) {
                            TOWHOM = "本文作者";
                        } else if (TOENGLISH == EnglishSSAddress) {
                            TOWHOM = "我";
                        } else {
                            var $R3 = $R2.find("TONATIVE");
                            if ($R3.length > 0) {
                                TOWHOM = $R3.text() + " / " + TOENGLISH;
                            } else {
                                TOWHOM = TOENGLISH;
                            }
                        }
                        var TOBODY = $R.find("TOBODY").text();
                        s = "<div class='ReplyTo'><span class='Note'>" + TOWHOM + "</span><br><span>" + TOBODY + "</span></div>";
                    } else {
                        s = "";
                    }
                    var div = document.createElement("div");
                    div.id = "R" + ID;
                    div.innerHTML = "<span class='Note'>我</span><br><span>" + text + "</span><br><span class='Note'>点赞 0 " + DATE + " <a href='#PG_1_RP' onclick='setReplyID(\"" + ID + "\")'>回复</a> <a href='#PG_1_VC_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setReplyID(\"" + ID + "\")'>删除</a></span>" + s;
                    var ReplyArea = document.getElementById("PG_1_VC_RP");
                    var first = ReplyArea.firstElementChild;
                    ReplyArea.insertBefore(div, first);
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function DeleteReply() {
            $("#PG_1_VC_DL").popup("close");
            if (IsNullorEmpty(WriteCredential)) {
                EditWhat = 0;
                $.mobile.changePage("#PG_10");
                return;
            }
            var popupid = "PG_1_VC_PP";
            var urlpart = "?C=DeleteReply&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(WriteCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&CommentID=" + CurrentCommentID + "&ReplyID=" + CurrentReplyID + "&Domain_Read=" + encodeURIComponent(Domain_Read);
            RequestServer(Domain_Write, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    $("#R" + CurrentReplyID).remove();
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function LikeReply(ID) {
            if (IsNullorEmpty(WriteCredential)) {
                EditWhat = 0;
                $.mobile.changePage("#PG_10");
                return;
            }
            if ($("#PG_1_RP_LK").text() == "已点赞") { return; }
            var popupid = "PG_1_VC_PP";
            var urlpart = "?C=PostReply&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(WriteCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&CommentID=" + CurrentCommentID + "&ReplyID=" + ID + "&Domain_Read=" + encodeURIComponent(Domain_Read);
            RequestServer(Domain_Write, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    $("#PG_1_RP_LK").text("已点赞");
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function GetPermissionText(value, tag) {
            switch (value) {
                case <%=流星语访问权限_常量集合.任何人%>:
                    return "任何人可访问";
                case <%=流星语访问权限_常量集合.全部讯友%>:
                    return "全部讯友可访问";
                case <%=流星语访问权限_常量集合.某标签讯友%>:
                    return "标签为 " + tag + " 的讯友可访问";
                case <%=流星语访问权限_常量集合.只有我%>:
                    return "只有我可访问";
                default:
                    return "";
            }
        }

        function setMeteorRainID(ID) {
            CurrentMeteorRainID = ID;
        }

        function setCommentID(ID) {
            CurrentCommentID = ID;
        }

        function setReplyID(ID) {
            CurrentReplyID = ID;
        }

        function DeleteMeteorRain() {
            $("#PG_1_DL").popup("close");
            if (IsNullorEmpty(WriteCredential)) {
                EditWhat = 0;
                $.mobile.changePage("#PG_10");
                return;
            }
            var popupid = "PG_1_PP";
            var urlpart = "?C=DeleteMeteorRain&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(WriteCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&Domain_Read=" + encodeURIComponent(Domain_Read);
            RequestServer(Domain_Write, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    $("#" + CurrentMeteorRainID).remove();
                    $("#M" + CurrentMeteorRainID).remove();
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function Sticky() {
            $("#PG_1_SK").popup("close");
            if (IsNullorEmpty(WriteCredential)) {
                EditWhat = 0;
                $.mobile.changePage("#PG_10");
                return;
            }
            var popupid = "PG_1_PP";
            var urlpart = "?C=Sticky&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(WriteCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&Domain_Read=" + encodeURIComponent(Domain_Read);
            RequestServer(Domain_Write, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    $("#" + CurrentMeteorRainID).remove();
                    $("#M" + CurrentMeteorRainID).remove();
                    if (PG_1_PN == 1) {
                        showPopupInfo(popupid, "已置顶。请刷新列表查看。");
                    } else {
                        showPopupInfo(popupid, "已置顶。请到第1页查看。");
                    }
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function CancelSticky() {
            $("#PG_1_CS").popup("close");
            if (IsNullorEmpty(WriteCredential)) {
                EditWhat = 0;
                $.mobile.changePage("#PG_10");
                return;
            }
            var popupid = "PG_1_PP";
            var urlpart = "?C=CancelSticky&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(WriteCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&Domain_Read=" + encodeURIComponent(Domain_Read);
            RequestServer(Domain_Write, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    $("#" + CurrentMeteorRainID).remove();
                    $("#M" + CurrentMeteorRainID).remove();
                    showPopupInfo(popupid, "已取消置顶。请刷新列表查看。");
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function PlayVideo(ID) {
            var url = "https://" + Domain_Read + "/media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&EnglishUsername=" + encodeURIComponent(EnglishUsername) + "&FileName=" + ID + ".mp4";
            window.external.PlayVideo(url);
        }


        var PG_2_PN = 1, PG_2_TP = 0;
        function ListGoods(PageNumber, IsPageShow) {
            if (IsPageShow == true) {
                if (IsNullorEmpty(JumpToPage)) {
                    if ($("#PG_2_DT").html() != "") { return; }
                } else {
                    PageNumber = JumpToPage;
                    JumpToPage = undefined;
                }
            }
            if (PageNumber == undefined) {
                PageNumber = 1;
            } else if (PageNumber < 1) {
                return;
            } else if (PageNumber > PG_2_TP) {
                return;
            }
            var popupid = "PG_2_PP";
            var urlpart = "?C=ListGoods&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&PageNumber=" + PageNumber;
            RequestServer(Domain_Read, urlpart, popupid, function (response) {
                if (response == null) {
                    PG_2_PN = 1;
                    PG_2_TP = 0;
                    $("#PG_2_DT").html("");
                    $("#PG_2_PN").text("");
                    $("#PG_2_PG").hide();
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    var s = "";
                    $R.find("GOODS").each(function (i) {
                        var $R2 = $(this);
                        var ID = $R2.find("ID").text();
                        var STYLE = $R2.find("STYLE").text();
                        var TITLE = $R2.find("TITLE").text();
                        var PRICE = $R2.find("PRICE").text();
                        var CURRENCY = $R2.find("CURRENCY").text();
                        s += "<div class='MeteorRain' id='" + ID + "' onclick='ViewGoods(\"" + ID + "\")'>";
                        switch (Number(STYLE)) {
                            case <%=流星语列表项样式_常量集合.一幅小图片 %>:
                                var src = "../media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&FileName=" + ID + "_1_pre.jpg";
                                s += "<table><tr><td valign='top'><img src='" + src + "' style='cursor: pointer;' /></td><td valign='top' style='padding-left:10px;'><span class='Title'>" + TITLE + "</span><br><span class='Price'>" + PRICE + " " + CURRENCY + "</span></td><tr></table>";
                                break;
                            case <%=流星语列表项样式_常量集合.三幅小图片 %>:
                                s += "<span class='Title'>" + TITLE + "</span><br><span class='Price'>" + PRICE + " " + CURRENCY + "</span><br>";
                                var src = "../media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&FileName=" + ID + "_";
                                s += "<table style='width:100%;margin-top:10px;'><tr><td valign='top'><img style='width:100%;height:auto;cursor: pointer;' src='" + src + "1_pre.jpg' /></td><td valign='top' style='padding-left:10px;'><img style='width:100%;height:auto;cursor: pointer;' src='" + src + "2_pre.jpg' /></td><td valign='top' style='padding-left:10px;'><img style='width:100%;height:auto;cursor: pointer;' src='" + src + "3_pre.jpg' /></td><tr></table>";
                                break;
                            case <%=流星语列表项样式_常量集合.一幅大图片 %>:
                                var src = "../media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&FileName=" + ID + "_1_pre.jpg";
                                s += "<span class='Title'>" + TITLE + "</span><br><span class='Price'>" + PRICE + " " + CURRENCY + "</span><br><img style='width:100%;height:auto;margin-top:10px;cursor: pointer;' src='" + src + "' /><br>";
                                break;
                            default:
                                s += "<span class='Title'>" + TITLE + "</span><br><span class='Price'>" + PRICE + " " + CURRENCY + "</span><br>";
                        }
                        s += "</div>";
                        if (IsGoodsEditor == true) {
                            s += "<div id='G" + ID + "'><span class='Note'><a href='#PG_2_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setGoodsID(\"" + ID + "\")'>删除</a> <a href='#' onclick='MoveToFront(\"" + ID + "\")'>移到最前</a></span></div>";
                        }
                    });
                    $("#PG_2_DT").html(s);
                    PG_2_PN = Number($R.find("PAGENUMBER").text());
                    PG_2_TP = Number($R.find("TOTALPAGES").text());
                    if (IsNullorEmpty(s)) {
                        $("#PG_2_PG").hide();
                    } else {
                        $("#PG_2_PG").show();
                    }
                    $("#PG_2_PN").text(PG_2_TP);
                    $.mobile.silentScroll(0);
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function ViewGoods(ID) {
            var popupid = "PG_2_PP";
            var urlpart = "?C=ViewGoods&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&GoodsID=" + ID;
            RequestServer(Domain_Read, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    $.mobile.changePage("#PG_2_VG");
                    $("#PG_2_VG_TT").html($R.find("TITLE").text())
                    $("#PG_2_VG_PR").html($R.find("PRICE").text())
                    $("#PG_2_VG_CR").html($R.find("CURRENCY").text())
                    var BODY = $R.find("BODY");
                    var s = "";
                    var j = 0;
                    var src = "../media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&FileName=" + ID + "_";
                    BODY.children().each(function (i) {
                        var $R2 = $(this);
                        switch ($R2[0].tagName) {
                            case "P":
                                s += "<p>" + $R2.text() + "</p>";
                                break;
                            case "IMG":
                                j += 1;
                                s += "<img style='width:100%;height:auto;' src='" + src + j + "." + $R2.text() + "' />";
                                break;
                            case "BUY":
                                $("#PG_2_VG_BY").prop("href", $R2.text());
                                break;
                        }
                    });
                    $("#PG_2_VG_BD").html(s);
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function setGoodsID(ID) {
            CurrentGoodsID = ID;
        }
        
        function DeleteGoods() {
            $("#PG_2_DL").popup("close");
            if (IsNullorEmpty(WriteCredential)) {
                EditWhat = 0;
                $.mobile.changePage("#PG_10");
                return;
            }
            var popupid = "PG_2_PP";
            var urlpart = "?C=DeleteGoods&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(WriteCredential) + "&GoodsID=" + CurrentGoodsID + "&Domain_Read=" + encodeURIComponent(Domain_Read);
            RequestServer(Domain_Write, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    $("#" + CurrentGoodsID).remove();
                    $("#G" + CurrentGoodsID).remove();
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function MoveToFront(ID) {
            if (IsNullorEmpty(WriteCredential)) {
                EditWhat = 0;
                $.mobile.changePage("#PG_10");
                return;
            }
            var popupid = "PG_2_PP";
            var urlpart = "?C=MoveToFront&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(WriteCredential) + "&GoodsID=" + ID + "&Domain_Read=" + encodeURIComponent(Domain_Read);
            RequestServer(Domain_Write, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    $("#" + ID).remove();
                    $("#G" + ID).remove();
                    if (PG_2_PN == 1) {
                        showPopupInfo(popupid, "已移至最前。请刷新列表查看。");
                    } else {
                        showPopupInfo(popupid, "已移至最前。请到第1页查看。");
                    }
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }



        function OpenEditor(Code) {
            EditWhat = Code;
            if (!IsNullorEmpty(WriteCredential)) {
                $.mobile.changePage("#PG_11");
                OpenEditor2();
            } else {
                $.mobile.changePage("#PG_10");
            }
        }

        function OpenEditor2() {
            if (EditWhat == 1) {
                $("#PG_11_TP_DV").show();
                $("#PG_11_PM_DV").show();
                $("#PG_11_GD").hide();
            } else {
                $("#PG_11_TP_DV").hide();
                $("#PG_11_PM_DV").hide();
                $("#PG_11_GD").show();
                if (EditWhat == 3) {

                }
            }
        }
        
        var id_Paragraph = 0;
        var id_SelectedParagraph;

        function ChooseType() {
            switch ($("#PG_11_TP").val()) {
                case "<%=流星语类型_常量集合.图文 %>":
                    $("#PG_11_VD").hide();
                    $("#PG_11_TX").show();
                    break;
                case "<%=流星语类型_常量集合.视频 %>":
                    $("#PG_11_TX").hide();
                    $("#PG_11_VD").show();
                    break;
            }
        }

        function ChoosePermission() {
            if ($("#PG_11_PM").val() == "<%=流星语访问权限_常量集合.某标签讯友 %>") {
                $("#PG_11_TG_DV").show();
            } else {
                $("#PG_11_TG_DV").hide();
            }
        }

        function ChoosePermission2() {
            if ($("#PG_12_PM").val() == "<%=流星语访问权限_常量集合.某标签讯友 %>") {
                $("#PG_12_TG_DV").show();
            } else {
                $("#PG_12_TG_DV").hide();
            }
        }

        function InsertText1(id) {
            id_SelectedParagraph = id;
            $.mobile.changePage("#PG_11_IT");
        }

        function InsertText2(Text) {
            var Text = $.trim($("#PG_11_IT_TX").val());
            if (!IsNullorEmpty(Text)) {
                $("#PG_11_IT_TX").val("");
                var p = document.createElement("p");
                id_Paragraph += 1;
                p.id = id_Paragraph;
                p.className = "Paragraph";
                p.innerHTML = Text;
                var div = document.createElement("div");
                div.id = "Ac" + id_Paragraph;
                div.innerHTML = "<a href='#' onclick='ReviseText1(\"" + id_Paragraph + "\")'>修改</a>  <a href='#' onclick='InsertText1(\"" + id_Paragraph + "\")'>插入文字</a>  <a href='#' onclick='SelectImage(\"" + id_Paragraph + "\")'>插入图片</a>  <a href='#' onclick='MoveUp(\"" + id_Paragraph + "\")'>上移</a>  <a href='#' onclick='MoveDown(\"" + id_Paragraph + "\")'>下移</a>  <a href='#' onclick='DeleteParagraph(\"" + id_Paragraph + "\")'>删除</a>";
                var EditArea = document.getElementById("PG_11_TX_ED");
                if (IsNullorEmpty(id_SelectedParagraph)) {
                    EditArea.appendChild(p);
                    EditArea.appendChild(div);
                } else {
                    var SelectedParagraph = document.getElementById(id_SelectedParagraph);
                    EditArea.insertBefore(p, SelectedParagraph);
                    EditArea.insertBefore(div, SelectedParagraph);
                }
            }
            history.back(-1);
        }

        function ReviseText1(id) {
            id_SelectedParagraph = id;
            $.mobile.changePage("#PG_11_RT");
            $("#PG_11_RT_TX").val($("#" + id).html());
        }

        function ReviseText2() {
            var Text = $.trim($("#PG_11_RT_TX").val());
            if (!IsNullorEmpty(Text)) {
                $("#" + id_SelectedParagraph).html(Text);
            }
            id_SelectedParagraph = null;
            history.back(-1);
       }

        function SelectImage(id) {
            id_SelectedParagraph = id;
            window.external.SelectImage("");
        }

        function InsertImage(path, DataURL) {
            if (!IsNullorEmpty(path)) {
                var img = document.createElement("img");
                id_Paragraph += 1;
                img.id = id_Paragraph;
                img.className = "Paragraph";
                img.src = DataURL;
                img.alt = path;
                var div = document.createElement("div");
                div.id = "Ac" + id_Paragraph;
                div.innerHTML = "<a href='#' onclick='ReviseImage1(\"" + id_Paragraph + "\")'>修改</a>  <a href='#' onclick='InsertText1(\"" + id_Paragraph + "\")'>插入文字</a>  <a href='#' onclick='SelectImage(\"" + id_Paragraph + "\")'>插入图片</a>  <a href='#' onclick='MoveUp(\"" + id_Paragraph + "\")'>上移</a>  <a href='#' onclick='MoveDown(\"" + id_Paragraph + "\")'>下移</a>  <a href='#' onclick='DeleteParagraph(\"" + id_Paragraph + "\")'>删除</a>";
                var EditArea = document.getElementById("PG_11_TX_ED");
                if (IsNullorEmpty(id_SelectedParagraph)) {
                    EditArea.appendChild(img);
                    EditArea.appendChild(div);
                } else {
                    var SelectedParagraph = document.getElementById(id_SelectedParagraph);
                    EditArea.insertBefore(img, SelectedParagraph);
                    EditArea.insertBefore(div, SelectedParagraph);
                }
            }
        }

        function ReviseImage1(id) {
            id_SelectedParagraph = null;
            window.external.SelectImage(id);
        }

        function ReviseImage(id, path, DataURL) {
            if (!IsNullorEmpty(path)) {
                var img = $("#" + id);
                img.prop("src", DataURL);
                img.prop("alt", path);
           }
        }

        function MoveUp(id) {
            var id1, id2;
            $("#PG_11_TX_ED").children(".Paragraph").each(function (i) {
                id2 = $(this).prop("id");
                if (id2 == id) {
                    return false;
                } else {
                    id1 = id2;
                }
            });
            if (!IsNullorEmpty(id1)) {
                var NextParagraph = document.getElementById(id1);
                var Paragraph = document.getElementById(id);
                var div = document.getElementById("Ac" + id);
                Paragraph.remove();
                div.remove();
                var EditArea = document.getElementById("PG_11_TX_ED");
                EditArea.insertBefore(Paragraph, NextParagraph);
                EditArea.insertBefore(div, NextParagraph);
            }
        }

        function MoveDown(id) {
            var id1, id2;
            var Found = false;
            $("#PG_11_TX_ED").children(".Paragraph").each(function (i) {
                id2 = $(this).prop("id");
                if (id2 == id) {
                    id1 = id2;
                } else if (!IsNullorEmpty(id1)) {
                    id1 = id2;
                    Found = true;
                    return false;
                }
            });
            if (Found == true) {
                var NextParagraph = document.getElementById(id);
                var Paragraph = document.getElementById(id1);
                var div = document.getElementById("Ac" + id1);
                Paragraph.remove();
                div.remove();
                var EditArea = document.getElementById("PG_11_TX_ED");
                EditArea.insertBefore(Paragraph, NextParagraph);
                EditArea.insertBefore(div, NextParagraph);
            }
        }

        function DeleteParagraph(id) {
            document.getElementById(id).remove();
            document.getElementById("Ac" + id).remove();
        }

        function SelectVideo() {
            window.external.SelectVideo();
        }

        function InsertVideo(VideoPath, PreviewPath, DataURL) {
            if (!IsNullorEmpty(VideoPath)) {
                $("#PG_11_VD_PA").text(VideoPath);
                $("#PG_11_VD_DV").show();
                if (!IsNullorEmpty(PreviewPath) && !IsNullorEmpty(DataURL)) {
                    var img = $("#PG_11_VD_PV");
                    img.prop("src", DataURL);
                    img.prop("alt", PreviewPath);
                    img.show();
                } else {
                    var img = $("#PG_11_VD_PV");
                    img.prop("src", "");
                    img.prop("alt", "");
                    img.hide();
                }
            }
        }

        function SelectVideoPreview() {
            window.external.SelectVideoPreview();
        }

        function InsertVideoPreview(PreviewPath, DataURL) {
            if (!IsNullorEmpty(PreviewPath) && !IsNullorEmpty(DataURL)) {
                var img = $("#PG_11_VD_PV");
                img.prop("src", DataURL);
                img.prop("alt", PreviewPath);
                img.show();
            }
        }

        function SSPalTags(id, html) {
            $("#" + id + "_TG").html(html);
        }

        function ChangePermission() {
            if (IsNullorEmpty(WriteCredential)) {
                EditWhat = 0;
                $.mobile.changePage("#PG_10");
                return;
            }
            var value = $("#PG_12_PM").val();
            if (IsNullorEmpty(value)) { return; }
            value = Number(value);
            var tag;
            if (value == "<%=流星语访问权限_常量集合.某标签讯友 %>") {
                tag = $("#PG_12_TG").val();
                if (IsNullorEmpty(tag)) { return; }
            } else {
                tag = "";
            }
            history.back(-1);
            var text = GetPermissionText(value, tag);
            if (text == $("#P" + CurrentMeteorRainID).text()) { return; }
            var popupid = "PG_1_PP";
            var urlpart = "?C=ChangePermission&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(WriteCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&Permission=" + value + "&Tag=" + encodeURIComponent(tag);
            RequestServer(Domain_Write, urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    $("#P" + CurrentMeteorRainID).text(text);
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function Publish() {
            var Title = $.trim($("#PG_11_TT").val());
            if (IsNullorEmpty(Title)) { return; }
            if (EditWhat == 1) {
                var Type = $("#PG_11_TP").val();
                switch (Type) {
                    case "<%=流星语类型_常量集合.图文 %>":
                        if ($("#PG_11_TX_ED").html() == "") { return; }
                        break;
                    case "<%=流星语类型_常量集合.视频 %>":
                        if ($("#PG_11_VD_PA").text() == "") { return; }
                        var img = document.getElementById("PG_11_VD_PV");
                        if (IsNullorEmpty(img.src) || IsNullorEmpty(img.alt)) { return; }
                        break;
                    default:
                        return;
                }
                var XML = "<MeteorRain><Type>" + Type + "</Type>";
                XML += "<Title>" + HandleXMLSpecialChars(Title) + "</Title>";
                var Permission = $("#PG_11_PM").val();
                XML += "<Permission>" + Permission + "</Permission>";
                if (Permission == "<%=流星语访问权限_常量集合.某标签讯友 %>") {
                    var tag = $("#PG_11_TG").val();
                    if (IsNullorEmpty(tag)) { return; }
                    XML += "<Tag>" + tag + "</Tag>";
                }
                XML += "<Domain_Read>" + Domain_Read + "</Domain_Read>";
                switch (Type) {
                     case "<%=流星语类型_常量集合.图文 %>":
                         var ImageNumber = 0;
                         var CharNumber = 0;
                         var Style = $("#PG_11_TX_ST").val();
                         XML += "<Style>" + Style + "</Style>";
                         XML += "<Body>";
                         $("#PG_11_TX_ED").children(".Paragraph").each(function (i) {
                             switch ($(this).prop("tagName")) {
                                 case "P":
                                 case "p":
                                    var Text = $(this).html();
                                    XML += "<Text>" + HandleXMLSpecialChars(Text) + "</Text>";
                                    CharNumber += Text.length;
                                    break;
                                case "IMG":
                                case "img":
                                    XML += "<Image>" + HandleXMLSpecialChars($(this).prop("alt")) + "</Image>";
                                    ImageNumber += 1;
                                    break;
                            }
                        });
                        if (CharNumber > 3000) {
                            showPopupInfo("PG_11_PP", "字数不得超过3000字。");
                            return;
                        }
                        if (ImageNumber > 10) {
                            showPopupInfo("PG_11_PP", "图片数量不得超过10幅。");
                            return;
                        }
                        switch (Style) {
                            case "<%=流星语列表项样式_常量集合.一幅小图片 %>":
                            case "<%=流星语列表项样式_常量集合.一幅大图片 %>":
                                if (ImageNumber < 1) {
                                    showPopupInfo("PG_11_PP", "至少要有1幅图片");
                                    return;
                                }
                                break;
                            case "<%=流星语列表项样式_常量集合.三幅小图片 %>":
                                if (ImageNumber < 3) {
                                    showPopupInfo("PG_11_PP", "至少要有3幅图片");
                                    return;
                                }
                                break;
                        }
                        XML += "</Body>";
                        break;
                    case "<%=流星语类型_常量集合.视频 %>":
                        XML += "<Style>" + $("#PG_11_VD_ST").val() + "</Style>";
                        XML += "<Video>" + HandleXMLSpecialChars($("#PG_11_VD_PA").text()) + "</Video>";
                        XML += "<Image>" + HandleXMLSpecialChars(document.getElementById("PG_11_VD_PV").alt) + "</Image>";
                        break;
                }
                XML += "</MeteorRain>";
            } else {
                if ($("#PG_11_TX_ED").html() == "") { return; }
                var XML = "<Goods><Title>" + HandleXMLSpecialChars(Title) + "</Title>";
                XML += "<Domain_Read>" + Domain_Read + "</Domain_Read>";
                var ImageNumber = 0;
                var CharNumber = 0;
                var Style = $("#PG_11_TX_ST").val();
                XML += "<Style>" + Style + "</Style>";
                XML += "<Price>" + HandleXMLSpecialChars($("#PG_11_PR").val()) + "</Price>";
                XML += "<Currency>" + HandleXMLSpecialChars($("#PG_11_CR").val()) + "</Currency>";
                XML += "<Buy>" + HandleXMLSpecialChars($("#PG_11_BY").val()) + "</Buy>";
                XML += "<Body>";
                $("#PG_11_TX_ED").children(".Paragraph").each(function (i) {
                    switch ($(this).prop("tagName")) {
                        case "P":
                        case "p":
                            var Text = $(this).html();
                            XML += "<Text>" + HandleXMLSpecialChars(Text) + "</Text>";
                            CharNumber += Text.length;
                            break;
                        case "IMG":
                        case "img":
                            XML += "<Image>" + HandleXMLSpecialChars($(this).prop("alt")) + "</Image>";
                            ImageNumber += 1;
                            break;
                    }
                });
                if (CharNumber > 3000) {
                    showPopupInfo("PG_11_PP", "字数不得超过3000字。");
                    return;
                }
                if (ImageNumber > 10) {
                    showPopupInfo("PG_11_PP", "图片数量不得超过10幅。");
                    return;
                }
                switch (Style) {
                    case "<%=流星语列表项样式_常量集合.一幅小图片 %>":
                        case "<%=流星语列表项样式_常量集合.一幅大图片 %>":
                            if (ImageNumber < 1) {
                                showPopupInfo("PG_11_PP", "至少要有1幅图片");
                                return;
                            }
                            break;
                        case "<%=流星语列表项样式_常量集合.三幅小图片 %>":
                        if (ImageNumber < 3) {
                            showPopupInfo("PG_11_PP", "至少要有3幅图片");
                            return;
                        }
                        break;
                }
                XML += "</Body></Goods>";
            }
            $("body").append('<div class="overlay"></div>');
            $.mobile.loading("show", {
                text: "请稍等……",
                textVisible: true,
                theme: "b",
                textonly: false,
                html: ""
            });
            if (EditWhat == 1) {
                window.external.PublishMeteorRain(XML);
            } else {
                window.external.PublishGoods(XML);
            }
        }

        function PublishSuccessful() {
            $("#PG_11_TT").val("")
            $("#PG_11_TX_ED").html("");
            $("#PG_11_PR").val("");
            $("#PG_11_CR").val("");
            $("#PG_11_BY").val("");
            $("#PG_11_VD_PA").html("");
            $("#PG_11_VD_DV").hide();
            $("#PG_11_VD_PV").prop("src", "").hide();
           $.mobile.loading("hide");
            $("div.overlay").remove();
            if (EditWhat == 1) {
                $.mobile.changePage("#PG_1");
            } else {
                $.mobile.changePage("#PG_2");
            }
        }

        function PublishFailed() {
            $.mobile.loading("hide");
            $("div.overlay").remove();
        }

        function CancelPublish() {
            if (EditWhat == 1) {
                $.mobile.changePage("#PG_1");
            } else {
                $.mobile.changePage("#PG_2");
            }
        }

        
        function RequestServer(domain, urlpart, popupid, func) {
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
                if (domain.indexOf("localhost:") != 0) {
                    domain = "https://" + domain + "/"
                } else {
                    domain = "../"
                }
                xhr.open("POST", domain + "Default.aspx" + urlpart, true);
                xhr.send(null);
            } else {
                showPopupInfo(popupid, "你正在使用的浏览器太陈旧。");
            }
        }

        function findReason($xmlDoc, popupid) {
            var $R = $xmlDoc.find("INVALIDCREDENTIAL");
            if ($R.length > 0) {
                window.external.RequestReadCredential(Domain_Read);
            } else {
                $R = $xmlDoc.find("NOTAUTHORIZED");
                if ($R.length > 0) {
                    showPopupInfo(popupid, "你无权操作。");
                } else {
                    $R = $xmlDoc.find("REACHLIMIT");
                    if ($R.length > 0) {
                        showPopupInfo(popupid, "今日评论或回复的次数已达上限。");
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

        function IsNullorEmpty(text) {
            if (text == undefined || text == null || text == "") {
                return true;
            } else {
                return false;
            }
        }

        function HandleXMLSpecialChars(text) {
            if (IsNullorEmpty(text)) { return ""; }
            var reg1 = new RegExp("<", "g");
            var reg2 = new RegExp(">", "g");
            var reg3 = new RegExp("&", "g");
            var reg4 = new RegExp("'", "g");
            var reg5 = new RegExp('"', "g");
            return text.replace(reg1, "&lt;").replace(reg2, "&gt;").replace(reg3, "&amp;").replace(reg4, "&apos;").replace(reg5, "&quot;");
        }

        function ReadCredentialReady(Domain, ReadCredential2, EnglishUsername2, EnglishSSAddress2, IsGoodsEditor2) {
            if (Domain != Domain_Read) { return; }
            ReadCredential = ReadCredential2;
            EnglishUsername = EnglishUsername2;
            EnglishSSAddress = EnglishSSAddress2;
            $.mobile.changePage("#PG_2");
            if (IsGoodsEditor2 == "true") {
                IsGoodsEditor = true;
                $("#PG_2_PB").show();
            }
        }

        function WriteCredentialReady(Domain, WriteCredential2) {
            if (Domain != Domain_Write) { return; }
            WriteCredential = WriteCredential2;
            if (EditWhat > 0) {
                $.mobile.changePage("#PG_11");
                OpenEditor2();
            } else {
                history.back(-1);
            }
        }

    </script>
</head>
<body>
    <div data-role="page" id="PG_0">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>我的小宇宙</h1>
        </div>
        <div role="main" class="ui-content">
            <div style="width:100%;height:100%;text-align:center;padding-top:200px;">
                <img src="../images/loading.gif" />
                <p>正在获取数据读取凭据。请稍等。</p>
            </div>
        </div>
    </div>
    
    <div data-role="page" id="PG_2">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>我的小宇宙-商圈</h1>
        </div>
        <div role="main" class="ui-content">
            <div class="ui-grid-b">
                <div class="ui-block-a"><a class="ui-btn ui-corner-all" href="#" onclick="ListGoods(PG_2_PN - 1)">上一页</a></div>
                <div class="ui-block-b"><a class="ui-btn ui-corner-all" href="#" onclick="ListGoods(PG_2_PN + 1)">下一页</a></div>
                <div class="ui-block-c"><a class="ui-btn ui-corner-all" href="#" onclick="ListGoods()">刷新</a></div>
            </div>
            <div id="PG_2_DT"></div>
            <div class="ui-grid-b" id="PG_2_PG" style="display:none;">
                <div class="ui-block-a"><a class="ui-btn ui-corner-all" href="#" onclick="ListGoods(PG_2_PN - 1)">上一页</a></div>
                <div class="ui-block-b"><a class="ui-btn ui-corner-all" href="#" onclick="ListGoods(PG_2_PN + 1)">下一页</a></div>
                <div class="ui-block-c"><a class="ui-btn ui-corner-all" href="#PG_CP" onclick="PrepareToChangePage(PG_2_TP)" id="PG_2_PN"></a></div>
            </div>
            <a href="#" class="ui-btn ui-corner-all" onclick="OpenEditor(2)" style="display:none;" id="PG_2_PB">发布新商品</a>
        </div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar">
                <ul>
                    <li><a href="#PG_2" data-icon="shop" class="ui-btn-active ui-state-persist">商圈</a></li>
                    <li><a href="#PG_1" data-icon="heart">流星语</a></li>
                    <li><a href="#PG_3" data-icon="mail">邮箱</a></li>
                    <li><a href="#PG_4" data-icon="cloud">网盘</a></li>
                </ul>
            </div>
            <h4 style="display:none;"></h4>
        </div>
        <div data-role="popup" id="PG_2_DL" data-theme="a" data-overlay-theme="b" class="ui-content" style="max-width:340px; padding-bottom:2em;">
            <h3>删除此商品吗？</h3>
            <a href="#" class="ui-shadow ui-btn ui-corner-all ui-btn-b ui-btn-inline ui-mini" onclick="DeleteGoods()">是</a>
            <a href="#" data-rel="back" class="ui-shadow ui-btn ui-corner-all ui-btn-inline ui-mini">否</a>
        </div>
        <div class="ui-content" id="PG_2_PP" data-role="popup" data-theme="a" data-position-to="window">
            <p></p>
        </div>
    </div>
    
    <div data-role="page" id="PG_2_VG">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>商品</h1>
        </div>
        <div role="main" class="ui-content">
            <h4 id="PG_2_VG_TT"></h4>
            <p class="Price"><span id="PG_2_VG_PR"></span> <span id="PG_2_VG_CR"></span></p>
            <a href="#" class="ui-btn ui-corner-all" target="_blank" id="PG_2_VG_BY">购买</a>
            <div id="PG_2_VG_BD"></div>
        </div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar">
                <ul>
                    <li><a href="#" data-icon="back" data-rel="back">返回</a></li>
                </ul>
            </div>
            <h4 style="display:none;"></h4>
        </div>
    </div>
    
    <div data-role="page" id="PG_1">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>我的小宇宙-流星语</h1>
        </div>
        <div role="main" class="ui-content">
            <div class="ui-grid-b">
                <div class="ui-block-a"><a class="ui-btn ui-corner-all" href="#" onclick="ListMeteorRains(PG_1_PN - 1)">上一页</a></div>
                <div class="ui-block-b"><a class="ui-btn ui-corner-all" href="#" onclick="ListMeteorRains(PG_1_PN + 1)">下一页</a></div>
                <div class="ui-block-c"><a class="ui-btn ui-corner-all" href="#" onclick="ListMeteorRains()">刷新</a></div>
            </div>
            <div id="PG_1_DT"></div>
            <div class="ui-grid-b" id="PG_1_PG" style="display:none;">
                <div class="ui-block-a"><a class="ui-btn ui-corner-all" href="#" onclick="ListMeteorRains(PG_1_PN - 1)">上一页</a></div>
                <div class="ui-block-b"><a class="ui-btn ui-corner-all" href="#" onclick="ListMeteorRains(PG_1_PN + 1)">下一页</a></div>
                <div class="ui-block-c"><a class="ui-btn ui-corner-all" href="#PG_CP" onclick="PrepareToChangePage(PG_1_TP)" id="PG_1_PN"></a></div>
            </div>
            <a href="#" class="ui-btn ui-corner-all" onclick="OpenEditor(1)">发流星语</a>
        </div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar">
                <ul>
                    <li><a href="#PG_2" data-icon="shop">商圈</a></li>
                    <li><a href="#PG_1" data-icon="heart" class="ui-btn-active ui-state-persist">流星语</a></li>
                    <li><a href="#PG_3" data-icon="mail">邮箱</a></li>
                    <li><a href="#PG_4" data-icon="cloud">网盘</a></li>
                </ul>
            </div>
            <h4 style="display:none;"></h4>
        </div>
        <div data-role="popup" id="PG_1_DL" data-theme="a" data-overlay-theme="b" class="ui-content" style="max-width:340px; padding-bottom:2em;">
            <h3>删除此流星语吗？</h3>
            <a href="#" class="ui-shadow ui-btn ui-corner-all ui-btn-b ui-btn-inline ui-mini" onclick="DeleteMeteorRain()">是</a>
            <a href="#" data-rel="back" class="ui-shadow ui-btn ui-corner-all ui-btn-inline ui-mini">否</a>
        </div>
        <div data-role="popup" id="PG_1_SK" data-theme="a" data-overlay-theme="b" class="ui-content" style="max-width:340px; padding-bottom:2em;">
            <h3>将此流星语固定在首页的顶部吗？</h3>
            <a href="#" class="ui-shadow ui-btn ui-corner-all ui-btn-b ui-btn-inline ui-mini" onclick="Sticky()">是</a>
            <a href="#" data-rel="back" class="ui-shadow ui-btn ui-corner-all ui-btn-inline ui-mini">否</a>
        </div>
        <div data-role="popup" id="PG_1_CS" data-theme="a" data-overlay-theme="b" class="ui-content" style="max-width:340px; padding-bottom:2em;">
            <h3>取消置顶吗？</h3>
            <a href="#" class="ui-shadow ui-btn ui-corner-all ui-btn-b ui-btn-inline ui-mini" onclick="CancelSticky()">是</a>
            <a href="#" data-rel="back" class="ui-shadow ui-btn ui-corner-all ui-btn-inline ui-mini">否</a>
        </div>
        <div class="ui-content" id="PG_1_PP" data-role="popup" data-theme="a" data-position-to="window">
            <p></p>
        </div>
    </div>
    
    <div data-role="page" id="PG_1_VM">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>流星语</h1>
        </div>
        <div role="main" class="ui-content">
            <h4 id="PG_1_VM_TT"></h4>
            <p id="PG_1_VM_NT" class="Note"></p>
            <div id="PG_1_VM_BD"></div>
            <p>评论</p>
            <div id="PG_1_VM_CM"></div>
            <textarea id="PG_1_VM_TX" maxlength="<%=最大值_常量集合.流星语评论和回复的文字长度 %>"></textarea>
            <div class="ui-grid-a">
                <div class="ui-block-a"><a href="#" class="ui-btn ui-corner-all" onclick="PostComment()">发表评论</a></div>
                <div class="ui-block-b"><a href="#" class="ui-btn ui-corner-all" onclick="MoreComments()">加载更多评论</a></div>
            </div>
        </div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar">
                <ul>
                    <li><a href="#" data-icon="back" data-rel="back">返回</a></li>
                </ul>
            </div>
            <h4 style="display:none;"></h4>
        </div>
        <div data-role="popup" id="PG_1_VM_DL" data-theme="a" data-overlay-theme="b" class="ui-content" style="max-width:340px; padding-bottom:2em;">
            <h3>删除此评论吗？</h3>
            <a href="#" class="ui-shadow ui-btn ui-corner-all ui-btn-b ui-btn-inline ui-mini" onclick="DeleteComment()">是</a>
            <a href="#" data-rel="back" class="ui-shadow ui-btn ui-corner-all ui-btn-inline ui-mini">否</a>
        </div>
        <div class="ui-content" id="PG_1_VM_PP" data-role="popup" data-theme="a" data-position-to="window">
            <p></p>
        </div>
    </div>
    
    <div data-role="page" id="PG_1_VC">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>评论及回复</h1>
        </div>
        <div role="main" class="ui-content">
            <div id="PG_1_VC_DT"></div>
            <p>回复</p>
            <div id="PG_1_VC_RP"></div>
            <a href="#" class="ui-btn ui-corner-all" onclick="MoreReplies()" style="margin-top:20px;">加载更多回复</a>
        </div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar">
                <ul>
                    <li><a href="#" data-icon="back" data-rel="back">返回</a></li>
                </ul>
            </div>
            <h4 style="display:none;"></h4>
        </div>
        <div data-role="popup" id="PG_1_VC_DL" data-theme="a" data-overlay-theme="b" class="ui-content" style="max-width:340px; padding-bottom:2em;">
            <h3>删除此回复吗？</h3>
            <a href="#" class="ui-shadow ui-btn ui-corner-all ui-btn-b ui-btn-inline ui-mini" onclick="DeleteReply()">是</a>
            <a href="#" data-rel="back" class="ui-shadow ui-btn ui-corner-all ui-btn-inline ui-mini">否</a>
        </div>
        <div class="ui-content" id="PG_1_VC_PP" data-role="popup" data-theme="a" data-position-to="window">
            <p></p>
        </div>
    </div>
    
    <div data-role="page" id="PG_1_RP">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>回复</h1>
        </div>
        <div role="main" class="ui-content">
            <textarea id="PG_1_RP_TX" maxlength="<%=最大值_常量集合.流星语评论和回复的文字长度 %>"></textarea>
            <div class="ui-grid-a">
                <div class="ui-block-a"><a href="#" class="ui-btn ui-corner-all ui-btn-b" onclick="PostReply()">确定</a></div>
                <div class="ui-block-b"><a href="#" data-rel="back" class="ui-btn ui-corner-all">取消</a></div>
            </div>
        </div>
        <div class="ui-content" id="PG_1_RP_PP" data-role="popup" data-theme="a" data-position-to="window">
            <p></p>
        </div>
    </div>
    
    <div data-role="page" id="PG_3">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>我的小宇宙-邮箱</h1>
        </div>
        <div role="main" class="ui-content" id="PG_3_C">目前未提供邮箱服务。</div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar">
                <ul>
                    <li><a href="#PG_2" data-icon="shop">商圈</a></li>
                    <li><a href="#PG_1" data-icon="heart">流星语</a></li>
                    <li><a href="#PG_3" data-icon="mail" class="ui-btn-active ui-state-persist">邮箱</a></li>
                    <li><a href="#PG_4" data-icon="cloud">网盘</a></li>
                </ul>
            </div>
            <h4 style="display:none;"></h4>
        </div>
        <div class="ui-content" id="PG_3_PP" data-role="popup" data-theme="a" data-position-to="window">
            <p></p>
        </div>
    </div>
    
    <div data-role="page" id="PG_4">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>我的小宇宙-网盘</h1>
        </div>
        <div role="main" class="ui-content" id="PG_4_C">目前未提供网盘服务。</div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar">
                <ul>
                    <li><a href="#PG_2" data-icon="shop">商圈</a></li>
                    <li><a href="#PG_1" data-icon="heart">流星语</a></li>
                    <li><a href="#PG_3" data-icon="mail">邮箱</a></li>
                    <li><a href="#PG_4" data-icon="cloud" class="ui-btn-active ui-state-persist">网盘</a></li>
                </ul>
            </div>
            <h4 style="display:none;"></h4>
        </div>
        <div class="ui-content" id="PG_4_PP" data-role="popup" data-theme="a" data-position-to="window">
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
    </div>
    

    <div data-role="page" id="PG_10">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>我的小宇宙</h1>
        </div>
        <div role="main" class="ui-content">
            <div style="width:100%;height:100%;text-align:center;padding-top:200px;">
                <img src="../images/loading.gif" />
                <p>正在获取数据写入凭据。请稍等。</p>
            </div>
        </div>
    </div>

    <div data-role="page" id="PG_11">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>编辑器</h1>
        </div>
        <div role="main" class="ui-content">
            <div id="PG_11_TP_DV">
                <select id="PG_11_TP" data-native-menu="false" onchange="ChooseType()">
                    <option selected="selected" value="<%=流星语类型_常量集合.图文 %>">图文</option>
                    <option value="<%=流星语类型_常量集合.视频 %>">视频</option>
                </select>
            </div>
            <p>标题</p>
            <input id="PG_11_TT" type="text" data-theme="a" maxlength="<%=最大值_常量集合.流星语标题字符数 %>" />
            <div id="PG_11_GD" style="display:none;">
                <p>价格</p>
                <input id="PG_11_PR" type="text" data-theme="a" maxlength="10" />
                <p>币种</p>
                <input id="PG_11_CR" type="text" data-theme="a" maxlength="3" />
                <p>购买链接</p>
                <input id="PG_11_BY" type="text" data-theme="a" maxlength="2048" />
            </div>
            <p>列表项样式</p>
            <div id="PG_11_TX">
                <select id="PG_11_TX_ST" data-native-menu="false">
                    <option selected="selected" value="<%=流星语列表项样式_常量集合.无图 %>">纯文字标题</option>
                    <option value="<%=流星语列表项样式_常量集合.一幅小图片 %>">一幅小图片加文字标题</option>
                    <option value="<%=流星语列表项样式_常量集合.三幅小图片 %>">三幅小图片加文字标题</option>
                    <option value="<%=流星语列表项样式_常量集合.一幅大图片 %>">一幅大图片加文字标题</option>
                </select>
                <p>图文</p>
                <div id="PG_11_TX_ED"></div>
                <div class="ui-grid-a" style="margin-top:20px;">
                    <div class="ui-block-a"><a href="#" class="ui-btn ui-corner-all ui-btn-b" onclick="InsertText1()">添加一段文字</a></div>
                    <div class="ui-block-b"><a href="#" class="ui-btn ui-corner-all ui-btn-b" onclick="SelectImage()">添加一幅图片</a></div>
                </div>
            </div>
            <div id="PG_11_VD" style="display:none;">
                <select id="PG_11_VD_ST" data-native-menu="false">
                    <option selected="selected" value="<%=流星语列表项样式_常量集合.一幅大图片 %>">一幅大图片加文字标题</option>
                    <option value="<%=流星语列表项样式_常量集合.一幅小图片 %>">一幅小图片加文字标题</option>
                </select>
                <p>视频</p>
                <p id="PG_11_VD_PA" style="word-break:break-all;"></p>
                <a href="#" class="ui-btn ui-corner-all ui-btn-b" onclick="SelectVideo()">选择视频文件</a>
                <div id="PG_11_VD_DV" style="display:none;">
                    <img id="PG_11_VD_PV" src="" style="display:none;" />
                    <a href="#" class="ui-btn ui-corner-all ui-btn-b" onclick="SelectVideoPreview()">选择视频预览图片</a>
                </div>
            </div>
            <div id="PG_11_PM_DV">
                <p>谁可访问</p>
                <select id="PG_11_PM" data-native-menu="false" onchange="ChoosePermission()">
                    <option selected="selected" value="<%=流星语访问权限_常量集合.任何人%>">任何人</option>
                    <option value="<%=流星语访问权限_常量集合.全部讯友 %>">全部讯友</option>
                    <option value="<%=流星语访问权限_常量集合.某标签讯友 %>">某标签讯友</option>
                    <option value="<%=流星语访问权限_常量集合.只有我 %>">只有我</option>
                </select>
                <div id="PG_11_TG_DV" style="display:none;">
                    <select id="PG_11_TG" data-native-menu="false"></select>
                </div>
            </div>
            <div class="ui-grid-a">
                <div class="ui-block-a"><a href="#" class="ui-btn ui-corner-all ui-btn-b" onclick="Publish()">发布</a></div>
                <div class="ui-block-b"><a class="ui-btn ui-corner-all" onclick="CancelPublish()">取消</a></div>
            </div>
        </div>
        <div class="ui-content" id="PG_11_PP" data-role="popup" data-theme="a" data-position-to="window">
            <p></p>
        </div>
    </div>

    <div data-role="page" id="PG_11_IT">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>添加一段文字</h1>
        </div>
        <div role="main" class="ui-content">
            <textarea id="PG_11_IT_TX" maxlength="<%=最大值_常量集合.讯宝文本长度 %>"></textarea>
            <div class="ui-grid-a">
                <div class="ui-block-a"><a href="#" class="ui-btn ui-corner-all ui-btn-b" onclick="InsertText2()">确定</a></div>
                <div class="ui-block-b"><a href="#" data-rel="back" class="ui-btn ui-corner-all">取消</a></div>
            </div>
        </div>
    </div>
    
    <div data-role="page" id="PG_11_RT">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>修改文字</h1>
        </div>
        <div role="main" class="ui-content">
            <textarea id="PG_11_RT_TX" maxlength="<%=最大值_常量集合.讯宝文本长度 %>"></textarea>
            <div class="ui-grid-a">
                <div class="ui-block-a"><a href="#" class="ui-btn ui-corner-all ui-btn-b" onclick="ReviseText2()">确定</a></div>
                <div class="ui-block-b"><a href="#" data-rel="back" class="ui-btn ui-corner-all">取消</a></div>
            </div>
        </div>
    </div>
    
    <div data-role="page" id="PG_12">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>修改访问权限</h1>
        </div>
        <div role="main" class="ui-content">
            <select id="PG_12_PM" data-native-menu="false" onchange="ChoosePermission2()">
                <option value="<%=流星语访问权限_常量集合.任何人%>">任何人</option>
                <option value="<%=流星语访问权限_常量集合.全部讯友 %>">全部讯友</option>
                <option value="<%=流星语访问权限_常量集合.某标签讯友 %>">某标签讯友</option>
                <option value="<%=流星语访问权限_常量集合.只有我 %>">只有我</option>
            </select>
            <div id="PG_12_TG_DV" style="display:none;">
                <select id="PG_12_TG" data-native-menu="false"></select>
            </div>
            <div class="ui-grid-a">
                <div class="ui-block-a"><a href="#" class="ui-btn ui-corner-all ui-btn-b" onclick="ChangePermission()">确定</a></div>
                <div class="ui-block-b"><a href="#" data-rel="back" class="ui-btn ui-corner-all">取消</a></div>
            </div>
        </div>
    </div>
    
</body>
</html>
