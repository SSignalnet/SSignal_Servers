<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Default.aspx.vb" Inherits="SSignal_TinyUniverse.TinyUniverse" %>
<%@ Import Namespace="SSignal_Protocols" %>
<%@ Import Namespace="SSignal_GlobalCommonCode"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>个人的小宇宙</title>
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
        .Price {
            color:red;
        }
        .NavBar3 {

        }
        .NavBar4 {

        }
        .td_SSicon {
            width: 40px;
        }
        .SSicon {
            width:40px;
            height:40px;
            background-color:whitesmoke;
            border-radius:8px 8px 8px 8px;
        }
        .Contact {
            font-size:small;
            word-wrap:break-word;
        }
        -->
    </style>

    <script>

        var EnglishSSAddress, ReadCredential, WriteCredential, EnglishUsername;
        var JumpToPage;
        var CurrentMeteorRainID, CurrentCommentID, CurrentReplyID;
        <%=OtherJS()%>

        $(document).on("pageshow", "#PG_0", function () { window.external.RequestReadCredential(Domain_Read); });
        $(document).on("pageshow", "#PG_1", function () { ListMeteorRains(undefined, true); });
        $(document).on("pageshow", "#PG_2", function () { ListGoods(undefined, true); });
        $(document).on("pageshow", "#PG_4", function () { RequestMemberList(); });
        $(document).on("pageshow", "#PG_10", function () { window.external.RequestWriteCredential(Domain_Write); });

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
                            STICKY = " 置顶 ";
                        } else {
                            STICKY = " ";
                        }
                        var tag;
                        $R3 = $R2.find("TAG");
                        if ($R3.length > 0) {
                            tag = $R3.text();
                        }
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
                                s += "</div><span class='Note'>评论 " + COMMENTS + " 点赞 " + LIKES + STICKY + DATE + "</span>";
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
                                s += "</div><span class='Note'>评论 " + COMMENTS + " 点赞 " + LIKES + STICKY + DATE + "</span>";
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
                        var NOTSSPAL;
                        $R3 = $R.find("NOTSSPAL");
                        if ($R3.length > 0) {
                            NOTSSPAL = true;
                        } else {
                            NOTSSPAL = false;
                        }
                        var s;
                        if (ENGLISH != EnglishSSAddress && !NOTSSPAL) {
                            s = " <a href='#' id='PG_1_VM_LK' onclick='LikeMeteorRain()'>点赞</a>";
                        } else {
                            s = "";
                        }
                        $("#PG_1_VM_NT").html("评论 " + COMMENTS + " 点赞 " + LIKES + s + "<br>" + AUTHOR + " " + DATE);
                        s = "";
                        switch (TYPE) {
                            case <%=流星语类型_常量集合.图文%>:
                                var BODY = $R.find("BODY2");
                                var j = 0;
                                var src = "../media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&EnglishUsername=" + encodeURIComponent(EnglishUsername) + "&FileName=" + ID + "_";
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
                            var s, ss;
                            if (!IsNullorEmpty(ENGLISH)) {
                                if (ENGLISH != EnglishSSAddress) {
                                    if (!NOTSSPAL) {
                                        s = " <a href='#' id='PG_1_VC_LK' onclick='LikeComment(\"" + ID + "\")'>点赞</a> ";
                                    } else {
                                        s = "";
                                    }
                                    ss = "";
                                } else {
                                    s = " ";
                                    ss = " <a href='#PG_1_VM_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setCommentID(\"" + ID + "\")'>删除</a>";
                                }
                            } else {
                                if ((EnglishUsername + "<%=讯宝地址标识 %>" + EnglishDomain) != EnglishSSAddress) {
                                    if (!NOTSSPAL) {
                                        s = " <a href='#' id='PG_1_VC_LK' onclick='LikeComment(\"" + ID + "\")'>点赞</a> ";
                                    } else {
                                        s = "";
                                    }
                                    ss = "";
                                } else {
                                    s = " ";
                                    ss = " <a href='#PG_1_VM_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setCommentID(\"" + ID + "\")'>删除</a>";
                                }
                            }
                            var div = document.createElement("div");
                            div.id = "C" + ID;
                            div.innerHTML = "<div class='Comment' onclick='GetComment(\"" + ID + "\")'><span class='Note'>" + AUTHOR + "</span><br><span>" + BODY + "</span></div><span class='Note'>回复 " + REPLIES + " 点赞 " + LIKES + s + DATE + ss + "</span>";
                            CommentArea.appendChild(div)
                        });
                        if (!NOTSSPAL) {
                            $("#PG_1_VM_PC").show();
                        } else {
                            $("#PG_1_VM_PC").hide();
                        }
                        CommentsEarlierThan = ID;
                    } else {

                    }
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function LikeMeteorRain() {
            if (IsNullorEmpty(WriteCredential)) {
                $.mobile.changePage("#PG_10");
                return;
            }
            if ($("#PG_1_VM_LK").text() == "已点赞") { return; }
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
                    var NOTSSPAL;
                    var $R3 = $R.find("NOTSSPAL");
                    if ($R3.length > 0) {
                        NOTSSPAL = true;
                    } else {
                        NOTSSPAL = false;
                    }
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
                        var s, ss;
                        if (!IsNullorEmpty(ENGLISH)) {
                            if (ENGLISH != EnglishSSAddress) {
                                if (!NOTSSPAL) {
                                    s = " <a href='#' id='PG_1_VC_LK' onclick='LikeComment(\"" + ID + "\")'>点赞</a> ";
                                } else {
                                    s = "";
                                }
                                ss = "";
                            } else {
                                s = " ";
                                ss = " <a href='#PG_1_VM_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setCommentID(\"" + ID + "\")'>删除</a>";
                            }
                        } else {
                            if ((EnglishUsername + "<%=讯宝地址标识 %>" + EnglishDomain) != EnglishSSAddress) {
                                if (!NOTSSPAL) {
                                    s = " <a href='#' id='PG_1_VC_LK' onclick='LikeComment(\"" + ID + "\")'>点赞</a> ";
                                } else {
                                    s = "";
                                }
                                ss = "";
                            } else {
                                s = " ";
                                ss = " <a href='#PG_1_VM_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setCommentID(\"" + ID + "\")'>删除</a>";
                            }
                        }
                        var div = document.createElement("div");
                        div.id = "C" + ID;
                        div.innerHTML = "<div class='Comment' onclick='GetComment(\"" + ID + "\")'><span class='Note'>" + AUTHOR + "</span><br><span>" + BODY + "</span></div><span class='Note'>回复 " + REPLIES + " 点赞 " + LIKES + s + DATE + ss + "</span>";
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
                    var NOTSSPAL;
                    var $R3 = $R.find("NOTSSPAL");
                    if ($R3.length > 0) {
                        NOTSSPAL = true;
                    } else {
                        NOTSSPAL = false;
                    }
                    var sssss;
                    if (!NOTSSPAL) {
                        sssss = " <a href='#PG_1_RP' onclick='setReplyID()'>回复</a>";
                    } else {
                        sssss = "";
                    }
                    var s = "<span class='Note'>" + AUTHOR + "</span><br><span>" + BODY + "</span><br><span class='Note'>回复 " + REPLIES + " 点赞 " + LIKES + " " + DATE + sssss + "</span>";
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
                        var ss, sss;
                        if (!IsNullorEmpty(ENGLISH)) {
                            if (ENGLISH != EnglishSSAddress) {
                                if (!NOTSSPAL) {
                                    ss = " <a href='#' id='PG_1_RP_LK' onclick='LikeReply(\"" + ID + "\")'>点赞</a> ";
                                } else {
                                    ss = "";
                                }
                                sss = "";
                            } else {
                                ss = " ";
                                sss = " <a href='#PG_1_VC_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setReplyID(\"" + ID + "\")'>删除</a>";
                            }
                        } else {
                            if ((EnglishUsername + "<%=讯宝地址标识 %>" + EnglishDomain) != EnglishSSAddress) {
                                if (!NOTSSPAL) {
                                    ss = " <a href='#' id='PG_1_RP_LK' onclick='LikeReply(\"" + ID + "\")'>点赞</a> ";
                                } else {
                                    ss = "";
                                }
                                sss = "";
                            } else {
                                ss = " ";
                                sss = " <a href='#PG_1_VC_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setReplyID(\"" + ID + "\")'>删除</a>";
                            }
                        }
                        var ssss;
                        if (!NOTSSPAL) {
                            ssss = " <a href='#PG_1_RP' onclick='setReplyID(\"" + ID + "\")'>回复</a>";
                        } else {
                            ssss = "";
                        }
                        var div = document.createElement("div");
                        div.id = "R" + ID;
                        div.className = "Reply";
                        div.innerHTML = "<span class='Note'>" + AUTHOR + "</span><br><span>" + BODY + "</span><br><span class='Note'>点赞 " + LIKES + ss + DATE + ssss + sss + "</span>" + s;
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
                    var NOTSSPAL;
                    var $R3 = $R.find("NOTSSPAL");
                    if ($R3.length > 0) {
                        NOTSSPAL = true;
                    } else {
                        NOTSSPAL = false;
                    }
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
                        var ss, sss;
                        if (!IsNullorEmpty(ENGLISH)) {
                            if (ENGLISH != EnglishSSAddress) {
                                if (!NOTSSPAL) {
                                    ss = " <a href='#' id='PG_1_RP_LK' onclick='LikeReply(\"" + ID + "\")'>点赞</a> ";
                                } else {
                                    ss = "";
                                }
                                sss = "";
                            } else {
                                ss = " ";
                                sss = "<a href='#PG_1_VC_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setReplyID(\"" + ID + "\")'>删除</a>";
                            }
                        } else {
                            if ((EnglishUsername + "<%=讯宝地址标识 %>" + EnglishDomain) != EnglishSSAddress) {
                                if (!NOTSSPAL) {
                                    ss = " <a href='#' id='PG_1_RP_LK' onclick='LikeReply(\"" + ID + "\")'>点赞</a> ";
                                } else {
                                    ss = "";
                                }
                                sss = "";
                            } else {
                                ss = " ";
                                sss = "<a href='#PG_1_VC_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setReplyID(\"" + ID + "\")'>删除</a>";
                            }
                        }
                        var ssss;
                        if (!NOTSSPAL) {
                            ssss = " <a href='#PG_1_RP' onclick='setReplyID(\"" + ID + "\")'>回复</a>";
                        } else {
                            ssss = "";
                        }
                        var div = document.createElement("div");
                        div.id = "R" + ID;
                        div.className = "Reply";
                        div.innerHTML = "<span class='Note'>" + AUTHOR + "</span><br><span>" + BODY + "</span><br><span class='Note'>点赞 " + LIKES + ss + DATE + ssss + sss + "</span>" + s;
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

        function setMeteorRainID(ID) {
            CurrentMeteorRainID = ID;
        }

        function setCommentID(ID) {
            CurrentCommentID = ID;
        }

        function setReplyID(ID) {
            CurrentReplyID = ID;
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


        var PG4showed = false;
        function RequestMemberList() {
            if (PG4showed != true) {
                PG4showed = true;
            }
            if ($("#PG_4_DT").html() == "") {
                window.external.RequestMemberList();
            }
        }

        function ListMembers(xml) {
            var divs = document.getElementsByClassName("NavBar3");
            var i;
            for (i = 0; i < divs.length; i++) {
                divs[i].style.display = "none";
            }
            divs = document.getElementsByClassName("NavBar4");
            for (i = 0; i < divs.length; i++) {
                divs[i].style.display = "";
            }
            if (IsNullorEmpty(xml) || !PG4showed) {
                return;
            }
            var $xmlDoc = $(xml);
            var $R = $xmlDoc.find("MEMBERS");
            if ($R.length > 0) {
                var s = "";
                $R.find("MEMBER").each(function (i) {
                    var $R2 = $(this);
                    var ADDRESS;
                    var ENGLISH = $R2.find("ENGLISH").text();
                    var NATIVE = $R2.find("NATIVE");
                    if (NATIVE.length > 0) {
                        ADDRESS = NATIVE.text() + " / " + ENGLISH;
                    } else {
                        ADDRESS = ENGLISH;
                    }
                    var ROLE = $R2.find("ROLE").text();
                    switch (Number(ROLE)) {
                        case <%=群角色_常量集合.成员_可以发言 %>:
                                ROLE = "群成员";
                                break;
                            case <%=群角色_常量集合.邀请加入_可以发言 %>:
                                ROLE = "等待加入";
                                break;
                            case <%=群角色_常量集合.群主 %>:
                                ROLE = "群主";
                                break;
                            default:
                                ROLE = "";
                        }
                        var ICON = $R2.find("ICON").text();
                    s += "<div id='ml" + i + "' onclick='ClickAMember(\"ml" + i + "\")' onmouseover='OnMouseOver(\"ml" + i + "\")' onmouseout='OnMouseOut(\"ml" + i + "\")' onmousedown='OnMouseDown(\"ml" + i + "\")' onmouseup='OnMouseOut(\"ml" + i + "\")' ontouchstart='OnMouseDown(\"ml" + i + "\")' ontouchend='OnMouseOut(\"ml" + i + "\")'><table><tr><td class='td_SSicon' valign='top'><img class='SSicon' src='" + ICON + "'/></td><td valign='top' class='Contact'><span id='ml" + i + "a'>" + ADDRESS + "</span><br>" + ROLE + "</td></tr></table></div>";
                    });
                $("#PG_4_DT").html(s);
                $.mobile.silentScroll(0);
            }
        }

        function OnMouseOver(id) {
            document.getElementById(id).style.backgroundColor = "lightblue";
        }

        function OnMouseOut(id) {
            document.getElementById(id).style.backgroundColor = "";
        }

        function OnMouseDown(id) {
            document.getElementById(id).style.backgroundColor = "lightgreen";
        }

        function ClickAMember(id) {
            var member = document.getElementById(id);
            if (!IsNullorEmpty(member)) {
                window.external.ClickAMember(document.getElementById(id + "a").innerText);
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
                    showPopupInfo(popupid, "你不是其讯友。");
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

        function ReadCredentialReady(Domain, ReadCredential2, EnglishUsername2, EnglishSSAddress2) {
            if (Domain != Domain_Read) { return; }
            ReadCredential = ReadCredential2;
            EnglishUsername = EnglishUsername2;
            EnglishSSAddress = EnglishSSAddress2;
            $.mobile.changePage("#PG_2");
        }

        function WriteCredentialReady(Domain, WriteCredential2) {
            if (Domain != Domain_Write) { return; }
            WriteCredential = WriteCredential2;
            history.back(-1);
        }

    </script>

</head>
<body>
    <div data-role="page" id="PG_0">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>个人的小宇宙</h1>
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
            <h1>个人的小宇宙-商圈</h1>
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
        </div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar" class="NavBar3">
                <ul>
                    <li><a href="#PG_2" data-icon="shop" class="ui-btn-active ui-state-persist">商圈</a></li>
                    <li><a href="#PG_1" data-icon="heart">流星语</a></li>
                    <li><a href="#PG_3" data-icon="cloud">网盘</a></li>
                </ul>
            </div>
            <div data-role="navbar" class="NavBar4" style="display:none;">
                <ul>
                    <li><a href="#PG_2" data-icon="shop" class="ui-btn-active ui-state-persist">商圈</a></li>
                    <li><a href="#PG_1" data-icon="heart">流星语</a></li>
                    <li><a href="#PG_3" data-icon="cloud">网盘</a></li>
                    <li><a href="#PG_4" data-icon="user">群成员</a></li>
                </ul>
            </div>
            <h4 style="display:none;"></h4>
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
            <h1>个人的小宇宙-流星语</h1>
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
        </div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar" class="NavBar3">
                <ul>
                    <li><a href="#PG_2" data-icon="shop">商圈</a></li>
                    <li><a href="#PG_1" data-icon="heart" class="ui-btn-active ui-state-persist">流星语</a></li>
                    <li><a href="#PG_3" data-icon="cloud">网盘</a></li>
                </ul>
            </div>
            <div data-role="navbar" class="NavBar4" style="display:none;">
                <ul>
                    <li><a href="#PG_2" data-icon="shop">商圈</a></li>
                    <li><a href="#PG_1" data-icon="heart" class="ui-btn-active ui-state-persist">流星语</a></li>
                    <li><a href="#PG_3" data-icon="cloud">网盘</a></li>
                    <li><a href="#PG_4" data-icon="user">群成员</a></li>
                </ul>
            </div>
            <h4 style="display:none;"></h4>
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
            <a href="#" class="ui-btn ui-corner-all" onclick="MoreComments()">加载更多评论</a>
            <div id="PG_1_VM_PC">
                <textarea id="PG_1_VM_TX" maxlength="<%=最大值_常量集合.流星语评论和回复的文字长度 %>"></textarea>
                <a href="#" class="ui-btn ui-corner-all" onclick="PostComment()">发表评论</a>
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
            <h1>个人的小宇宙-网盘</h1>
        </div>
        <div role="main" class="ui-content" id="PG_3_DT">无文件</div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar" class="NavBar3">
                <ul>
                    <li><a href="#PG_2" data-icon="shop">商圈</a></li>
                    <li><a href="#PG_1" data-icon="heart">流星语</a></li>
                    <li><a href="#PG_3" data-icon="cloud" class="ui-btn-active ui-state-persist">网盘</a></li>
                </ul>
            </div>
            <div data-role="navbar" class="NavBar4" style="display:none;">
                <ul>
                    <li><a href="#PG_2" data-icon="shop">商圈</a></li>
                    <li><a href="#PG_1" data-icon="heart">流星语</a></li>
                    <li><a href="#PG_3" data-icon="cloud" class="ui-btn-active ui-state-persist">网盘</a></li>
                    <li><a href="#PG_4" data-icon="user">群成员</a></li>
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
            <h1>小聊天群成员</h1>
        </div>
        <div role="main" class="ui-content" id="PG_4_DT"></div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar">
                <ul>
                    <li><a href="#PG_2" data-icon="shop">商圈</a></li>
                    <li><a href="#PG_1" data-icon="heart">流星语</a></li>
                    <li><a href="#PG_3" data-icon="cloud">网盘</a></li>
                    <li><a href="#PG_4" data-icon="user" class="ui-btn-active ui-state-persist">群成员</a></li>
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
            <h1>个人的小宇宙</h1>
        </div>
        <div role="main" class="ui-content">
            <div style="width:100%;height:100%;text-align:center;padding-top:200px;">
                <img src="../images/loading.gif" />
                <p>正在获取数据写入凭据。请稍等。</p>
            </div>
        </div>
    </div>

</body>
</html>
