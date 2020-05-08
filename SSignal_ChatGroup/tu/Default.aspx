<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Default.aspx.vb" Inherits="SSignal_ChatGroup.TinyUniverse" %>
<%@ Import Namespace="SSignal_Protocols" %>
<%@ Import Namespace="SSignal_GlobalCommonCode"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>群的小宇宙</title>
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

        var EnglishSSAddress, UserCredential, GroupID, AuthorSSAddress;
        var JumpToPage;
        var CurrentMeteorRainID, CurrentCommentID, CurrentReplyID;
        var Domain_Read, ReadCredential;

        $(document).on("pageshow", "#PG_2", function () { ListGoods(undefined, true); });
        $(document).on("pageshow", "#PG_1", function () { ListMeteorRains(undefined, true); });
        $(document).on("pageshow", "#PG_3", function () { ListMembers(undefined, true); });

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
            if (IsNullorEmpty(UserCredential)) { return; }
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
            var urlpart = "?C=ListMeteorRains&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&GroupID=" + GroupID + "&PageNumber=" + PageNumber + "&TimezoneOffset=" + TimezoneOffset;
            RequestServer(urlpart, popupid, function (response) {
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
                    var ROLE = Number($R.find("ROLE").text());
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
                        if (ROLE < <%=群角色_常量集合.管理员%>) {
                            if ($R3.length > 0) {
                                STICKY = " 已置顶";
                            } else {
                                STICKY = "";
                            }
                        } else {
                            if ($R3.length > 0) {
                                STICKY = " <a href='#PG_1_CS' data-theme='a' data-rel='popup' data-position-to='window' onclick='setMeteorRainID(\"" + ID + "\")'>取消置顶</a>";
                            } else {
                                STICKY = " <a href='#PG_1_SK' data-theme='a' data-rel='popup' data-position-to='window' onclick='setMeteorRainID(\"" + ID + "\")'>置顶</a>";
                            }
                        }
                        var DELETE;
                        if (ROLE < <%=群角色_常量集合.管理员%>) {
                            DELETE = "";
                        } else {
                            DELETE = " <a href='#PG_1_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setMeteorRainID(\"" + ID + "\")'>删除</a>";
                        }
                        switch (Number(TYPE)) {
                            case <%=流星语类型_常量集合.图文%>:
                                var COMMENTS = $R2.find("COMMENTS").text();
                                var LIKES = $R2.find("LIKES").text();
                                s += "<div class='MeteorRain' id='" + ID + "' onclick='GetMeteorRain(\"" + ID + "\")'>";
                                switch (Number(STYLE)) {
                                    case <%=流星语列表项样式_常量集合.一幅小图片 %>:
                                        var src = "media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&GroupID=" + GroupID + "&FileName=" + ID + "_1_pre.jpg";
                                        s += "<table><tr><td valign='top'><img src='" + src + "' style='cursor: pointer;' /></td><td valign='top' style='padding-left:10px;'><span class='Title'>" + TITLE + "</span></td><tr></table>";
                                        break;
                                    case <%=流星语列表项样式_常量集合.三幅小图片 %>:
                                        s += "<span class='Title'>" + TITLE + "</span><br>";
                                        var src = "media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&GroupID=" + GroupID + "&FileName=" + ID + "_";
                                        s += "<table style='width:100%;margin-top:10px;'><tr><td valign='top'><img style='width:100%;height:auto;cursor: pointer;' src='" + src + "1_pre.jpg' /></td><td valign='top' style='padding-left:10px;'><img style='width:100%;height:auto;cursor: pointer;' src='" + src + "2_pre.jpg' /></td><td valign='top' style='padding-left:10px;'><img style='width:100%;height:auto;cursor: pointer;' src='" + src + "3_pre.jpg' /></td><tr></table>";
                                        break;
                                    case <%=流星语列表项样式_常量集合.一幅大图片 %>:
                                        var src = "media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&GroupID=" + GroupID + "&FileName=" + ID + "_1_pre.jpg";
                                        s += "<span class='Title'>" + TITLE + "</span><br><img style='width:100%;height:auto;margin-top:10px;cursor: pointer;' src='" + src + "' /><br>";
                                        break;
                                    default:
                                        s += "<span class='Title'>" + TITLE + "</span><br>";
                                }
                                s += "</div><div id='M" + ID + "'><span class='Note'>评论 " + COMMENTS + " 点赞 " + LIKES + DELETE + STICKY + "<br>" + DATE + "</span></div>";
                                break;
                            case <%=流星语类型_常量集合.视频%>:
                                var COMMENTS = $R2.find("COMMENTS").text();
                                var LIKES = $R2.find("LIKES").text();
                                s += "<div class='MeteorRain' id='" + ID + "' onclick='GetMeteorRain(\"" + ID + "\")'>";
                                switch (Number(STYLE)) {
                                    case <%=流星语列表项样式_常量集合.一幅小图片 %>:
                                        var src = "media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&GroupID=" + GroupID + "&FileName=" + ID + ".jpg";
                                        s += "<table><tr><td valign='top'><img src='" + src + "' style='cursor: pointer;' /></td><td valign='top' style='padding-left:10px;'><span class='Title'>" + TITLE + "</span></td><tr></table>";
                                        break;
                                    case <%=流星语列表项样式_常量集合.一幅大图片 %>:
                                        var src = "media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&GroupID=" + GroupID + "&FileName=" + ID + ".jpg";
                                        s += "<span class='Title'>" + TITLE + "</span><br><img style='width:100%;height:auto;margin-top:10px;cursor: pointer;' src='" + src + "' /><br>";
                                        break;
                                }
                                s += "</div><div id='M" + ID + "'><span class='Note'>评论 " + COMMENTS + " 点赞 " + LIKES + DELETE + STICKY + "<br>" + DATE + "</span></div>";
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
                    if (ROLE < <%=群角色_常量集合.管理员%>) {
                        $("#PG_1_PB").hide();
                    } else {
                        $("#PG_1_PB").show();
                    }
                    $.mobile.silentScroll(0);
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function GetMeteorRain(ID) {
            var TimezoneOffset = -(new Date().getTimezoneOffset());
            var popupid = "PG_1_PP";
            var urlpart = "?C=GetMeteorRain&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&MeteorRainID=" + ID + "&TimezoneOffset=" + TimezoneOffset;
            RequestServer(urlpart, popupid, function (response) {
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
                        AuthorSSAddress = $R.find("ENGLISH2").text();
                        var AUTHOR;
                        var $R3 = $R.find("NATIVE2");
                        if ($R3.length > 0) {
                            AUTHOR = $R3.text() + " / " + AuthorSSAddress;
                        } else {
                            AUTHOR = AuthorSSAddress;
                        }
                        $("#PG_1_VM_TT").html($R.find("TITLE").text())
                        var COMMENTS = $R.find("COMMENTS").text();
                        var LIKES = $R.find("LIKES2").text();
                        var DATE = $R.find("DATE2").text();
                        var ROLE = Number($R.find("ROLE").text());
                        $("#PG_1_VM_NT").html("评论 " + COMMENTS + " 点赞 " + LIKES + "<br>" + AUTHOR + " " + DATE);
                        var s = "";
                        switch (TYPE) {
                            case <%=流星语类型_常量集合.图文%>:
                                var j = 0;
                                var src = "media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&GroupID=" + GroupID + "&FileName=" + ID + "_";
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
                                var src = "media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&GroupID=" + GroupID + "&FileName=" + ID + ".jpg";
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
                            if (ENGLISH == EnglishSSAddress) {
                                AUTHOR = "我";
                            } else if (ENGLISH == AuthorSSAddress) {
                                AUTHOR = "本文作者";
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
                            if (ENGLISH != EnglishSSAddress) {
                                s = " <a href='#' id='PG_1_VC_LK' onclick='LikeComment(\"" + ID + "\")'>点赞</a> ";
                                if (ROLE < <%=群角色_常量集合.管理员 %>) {
                                    ss = "";
                                } else {
                                    ss = " <a href='#PG_1_VM_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setCommentID(\"" + ID + "\")'>删除</a>";
                                }
                            } else {
                                s = " ";
                                ss = " <a href='#PG_1_VM_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setCommentID(\"" + ID + "\")'>删除</a>";
                            }
                            var div = document.createElement("div");
                            div.id = "C" + ID;
                            div.innerHTML = "<div class='Comment' onclick='GetComment(\"" + ID + "\")'><span class='Note'>" + AUTHOR + "</span><br><span>" + BODY + "</span></div><span class='Note'>回复 " + REPLIES + " 点赞 " + LIKES + s + DATE + ss + "</span>";
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
            var popupid = "PG_1_VM_PP";
            var urlpart = "?C=PostComment&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&MeteorRainID=" + CurrentMeteorRainID;
            RequestServer(urlpart, popupid, function (response) {
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
            var text = $("#PG_1_VM_TX").val();
            if (IsNullorEmpty(text)) { return; }
            var TimezoneOffset = -(new Date().getTimezoneOffset());
            var popupid = "PG_1_VM_PP";
            var urlpart = "?C=PostComment&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&Text=" + encodeURIComponent(text) + "&TimezoneOffset=" + TimezoneOffset;
            RequestServer(urlpart, popupid, function (response) {
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
            var popupid = "PG_1_VM_PP";
            var urlpart = "?C=DeleteComment&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&CommentID=" + CurrentCommentID;
            RequestServer(urlpart, popupid, function (response) {
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
            var urlpart = "?C=MoreComments&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&EarlierThan=" + CommentsEarlierThan + "&TimezoneOffset=" + TimezoneOffset;
            RequestServer(urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    var ROLE = Number($R.find("ROLE").text());
                    var CommentArea = document.getElementById("PG_1_VM_CM");
                    var ID;
                    $R.find("COMMENT").each(function (i) {
                        var $R2 = $(this);
                        ID = $R2.find("ID").text();
                        var ENGLISH = $R2.find("ENGLISH").text();
                        var AUTHOR;
                        if (ENGLISH == EnglishSSAddress) {
                            AUTHOR = "我";
                        } else if (ENGLISH == AuthorSSAddress) {
                            AUTHOR = "本文作者";
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
                        if (ENGLISH != EnglishSSAddress) {
                            s = " <a href='#' id='PG_1_VC_LK' onclick='LikeComment(\"" + ID + "\")'>点赞</a> ";
                            if (ROLE < <%=群角色_常量集合.管理员 %>) {
                                ss = "";
                            } else {
                                ss = " <a href='#PG_1_VM_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setCommentID(\"" + ID + "\")'>删除</a>";
                            }
                        } else {
                            s = " ";
                            ss = " <a href='#PG_1_VM_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setCommentID(\"" + ID + "\")'>删除</a>";
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
            var urlpart = "?C=MoreReplies&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&CommentID=" + ID + "&EarlierThan=0&TimezoneOffset=" + TimezoneOffset;
            RequestServer(urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    $.mobile.changePage("#PG_1_VC");
                    CurrentCommentID = ID;
                    var ROLE = Number($R.find("ROLE").text());
                    var $R2 = $R.find("COMMENT");
                    var ENGLISH = $R2.find("ENGLISH").text();
                    var AUTHOR;
                    if (ENGLISH == EnglishSSAddress) {
                        AUTHOR = "我";
                    } else if (ENGLISH == AuthorSSAddress) {
                        AUTHOR = "本文作者";
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
                        if (ENGLISH == EnglishSSAddress) {
                            AUTHOR = "我";
                        } else if (ENGLISH == AuthorSSAddress) {
                            AUTHOR = "本文作者";
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
                            if (TOENGLISH == EnglishSSAddress) {
                                TOWHOM = "我";
                            } else if (TOENGLISH == AuthorSSAddress) {
                                TOWHOM = "本文作者";
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
                        if (ENGLISH != EnglishSSAddress) {
                            ss = " <a href='#' id='PG_1_RP_LK' onclick='LikeReply(\"" + ID + "\")'>点赞</a> ";
                            if (ROLE < <%=群角色_常量集合.管理员 %>) {
                                sss = "";
                            } else {
                                sss = " <a href='#PG_1_VC_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setReplyID(\"" + ID + "\")'>删除</a>";
                            }
                        } else {
                            ss = " ";
                            sss = " <a href='#PG_1_VC_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setReplyID(\"" + ID + "\")'>删除</a>";
                        }
                        var div = document.createElement("div");
                        div.id = "R" + ID;
                        div.className = "Reply";
                        div.innerHTML = "<span class='Note'>" + AUTHOR + "</span><br><span>" + BODY + "</span><br><span class='Note'>点赞 " + LIKES + ss + DATE + " <a href='#PG_1_RP' onclick='setReplyID(\"" + ID + "\")'>回复</a>" + sss + "</span>" + s;
                        ReplyArea.appendChild(div)
                    });
                    RepliesEarlierThan = ID;
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function LikeComment(ID) {
            if ($("#PG_1_VC_LK").text() == "已点赞") { return; }
            var popupid = "PG_1_VM_PP";
            var urlpart = "?C=PostReply&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&CommentID=" + ID;
            RequestServer(urlpart, popupid, function (response) {
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
            var urlpart = "?C=MoreReplies&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&CommentID=" + CurrentCommentID + "&EarlierThan=" + RepliesEarlierThan + "&TimezoneOffset=" + TimezoneOffset;
            RequestServer(urlpart, popupid, function (response) {
                if (response == null) {
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
                if ($R.length > 0) {
                    var ROLE = Number($R.find("ROLE").text());
                    var ReplyArea = document.getElementById("PG_1_VC_RP");
                    var ID;
                    $R.find("REPLY").each(function (i) {
                        var $R2 = $(this);
                        ID = $R2.find("ID").text();
                        var ENGLISH = $R2.find("ENGLISH").text();
                        var AUTHOR;
                        if (ENGLISH == EnglishSSAddress) {
                            AUTHOR = "我";
                        } else if (ENGLISH == AuthorSSAddress) {
                            AUTHOR = "本文作者";
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
                            if (TOENGLISH == EnglishSSAddress) {
                                TOWHOM = "我";
                            } else if (TOENGLISH == AuthorSSAddress) {
                                TOWHOM = "本文作者";
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
                        if (ENGLISH != EnglishSSAddress) {
                            ss = " <a href='#' id='PG_1_RP_LK' onclick='LikeReply(\"" + ID + "\")'>点赞</a> ";
                            if (ROLE < <%=群角色_常量集合.管理员 %>) {
                                sss = "";
                            } else {
                                sss = " <a href='#PG_1_VC_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setReplyID(\"" + ID + "\")'>删除</a>";
                            }
                        } else {
                            ss = " ";
                            sss = " <a href='#PG_1_VC_DL' data-theme='a' data-rel='popup' data-position-to='window' onclick='setReplyID(\"" + ID + "\")'>删除</a>";
                        }
                        var div = document.createElement("div");
                        div.id = "R" + ID;
                        div.className = "Reply";
                        div.innerHTML = "<span class='Note'>" + AUTHOR + "</span><br><span>" + BODY + "</span><br><span class='Note'>点赞 " + LIKES + ss + DATE + " <a href='#PG_1_RP' onclick='setReplyID(\"" + ID + "\")'>回复</a>" + sss + "</span>" + s;
                        ReplyArea.appendChild(div)
                    });
                    RepliesEarlierThan = ID;
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
        }

        function PostReply() {
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
            var urlpart = "?C=PostReply&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&CommentID=" + CurrentCommentID + s + "&Text=" + encodeURIComponent(text) + "&TimezoneOffset=" + TimezoneOffset;
            RequestServer(urlpart, popupid, function (response) {
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
                        if (TOENGLISH == EnglishSSAddress) {
                            TOWHOM = "我";
                        } else if (TOENGLISH == AuthorSSAddress) {
                            TOWHOM = "本文作者";
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
            var popupid = "PG_1_VC_PP";
            var urlpart = "?C=DeleteReply&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&CommentID=" + CurrentCommentID + "&ReplyID=" + CurrentReplyID;
            RequestServer(urlpart, popupid, function (response) {
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
            if ($("#PG_1_RP_LK").text() == "已点赞") { return; }
            var popupid = "PG_1_VC_PP";
            var urlpart = "?C=PostReply&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&MeteorRainID=" + CurrentMeteorRainID + "&CommentID=" + CurrentCommentID + "&ReplyID=" + ID;
            RequestServer(urlpart, popupid, function (response) {
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

        function DeleteMeteorRain() {
            $("#PG_1_DL").popup("close");
            var popupid = "PG_1_PP";
            var urlpart = "?C=DeleteMeteorRain&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&MeteorRainID=" + CurrentMeteorRainID;
            RequestServer(urlpart, popupid, function (response) {
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
            var popupid = "PG_1_PP";
            var urlpart = "?C=Sticky&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&MeteorRainID=" + CurrentMeteorRainID;
            RequestServer(urlpart, popupid, function (response) {
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
            var popupid = "PG_1_PP";
            var urlpart = "?C=CancelSticky&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&MeteorRainID=" + CurrentMeteorRainID;
            RequestServer(urlpart, popupid, function (response) {
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
            var i = window.location.href.lastIndexOf("/");
            var url = window.location.href.substr(0, i) + "/media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&GroupID=" + GroupID + "&FileName=" + ID + ".mp4";
            window.external.PlayVideo(url);
        }


        var PG_2_PN = 1, PG_2_TP = 0;
        function ListGoods(PageNumber, IsPageShow) {
            if (IsNullorEmpty(Domain_Read)) {
                return;
            }
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
            RequestServer(urlpart, popupid, function (response) {
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
                                var src = "https://" + Domain_Read + "/media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&FileName=" + ID + "_1_pre.jpg";
                                s += "<table><tr><td valign='top'><img src='" + src + "' style='cursor: pointer;' /></td><td valign='top' style='padding-left:10px;'><span class='Title'>" + TITLE + "</span><br><span class='Price'>" + PRICE + " " + CURRENCY + "</span></td><tr></table>";
                                break;
                            case <%=流星语列表项样式_常量集合.三幅小图片 %>:
                                s += "<span class='Title'>" + TITLE + "</span><br><span class='Price'>" + PRICE + " " + CURRENCY + "</span><br>";
                                var src = "https://" + Domain_Read + "/media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&FileName=" + ID + "_";
                                s += "<table style='width:100%;margin-top:10px;'><tr><td valign='top'><img style='width:100%;height:auto;cursor: pointer;' src='" + src + "1_pre.jpg' /></td><td valign='top' style='padding-left:10px;'><img style='width:100%;height:auto;cursor: pointer;' src='" + src + "2_pre.jpg' /></td><td valign='top' style='padding-left:10px;'><img style='width:100%;height:auto;cursor: pointer;' src='" + src + "3_pre.jpg' /></td><tr></table>";
                                break;
                            case <%=流星语列表项样式_常量集合.一幅大图片 %>:
                                var src = "https://" + Domain_Read + "/media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&FileName=" + ID + "_1_pre.jpg";
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
            }, Domain_Read);
        }

        function ViewGoods(ID) {
            var popupid = "PG_2_PP";
            var urlpart = "?C=ViewGoods&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&GoodsID=" + ID;
            RequestServer(urlpart, popupid, function (response) {
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
                    var src = "https://" + Domain_Read + "/media/?EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(ReadCredential) + "&FileName=" + ID + "_";
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
            }, Domain_Read);
        }


        var PG_3_PN = 1, PG_3_TP = 0;
        function ListMembers(PageNumber, IsPageShow) {
            if (IsNullorEmpty(UserCredential)) { return; }
            if (IsPageShow == true) {
                if (IsNullorEmpty(JumpToPage)) {
                    if ($("#PG_3_DT").html() != "") { return; }
                } else {
                    PageNumber = JumpToPage;
                    JumpToPage = undefined;
                }
            }
            if (PageNumber == undefined) {
                PageNumber = 1;
            } else if (PageNumber < 1) {
                return;
            } else if (PageNumber > PG_3_TP) {
                return;
            }
            var popupid = "PG_3_PP";
            var urlpart = "?C=ListMembers&EnglishSSAddress=" + encodeURIComponent(EnglishSSAddress) + "&Credential=" + encodeURIComponent(UserCredential) + "&GroupID=" + GroupID + "&PageNumber=" + PageNumber;
            RequestServer(urlpart, popupid, function (response) {
                if (response == null) {
                    PG_3_PN = 1;
                    PG_3_TP = 0;
                    $("#PG_3_DT").html("");
                    $("#PG_3_PN").text("");
                    $("#PG_3_PG").hide();
                    return;
                }
                var $xmlDoc = $(response);
                var $R = $xmlDoc.find("SUCCEED");
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
                        var NAME = $R2.find("NAME");
                        if (NAME.length > 0) {
                            NAME = "<span style='color:blue;'>" + HandleXMLSpecialChars(NAME.text()) + "</span> ";
                        } else {
                            NAME = ""
                        }
                        var ROLE = $R2.find("ROLE").text();
                        switch (Number(ROLE)) {
                            case <%=群角色_常量集合.成员_可以发言 %>:
                                ROLE = "群成员（可以发言）";
                                break;
                            case <%=群角色_常量集合.成员_不可发言 %>:
                                ROLE = "群成员（不可发言）";
                                break;
                            case <%=群角色_常量集合.邀请加入_可以发言 %>:
                                ROLE = "等待加入（可以发言）";
                                break;
                           case <%=群角色_常量集合.邀请加入_不可发言 %>:
                                ROLE = "等待加入（不可发言）";
                                break;
                            case <%=群角色_常量集合.管理员 %>:
                                ROLE = "管理员";
                                break;
                            case <%=群角色_常量集合.群主 %>:
                                ROLE = "群主";
                                break;
                            default:
                                ROLE = "";
                        }
                        var ICON = $R2.find("ICON").text();
                        s += "<div id='ml" + i + "' onclick='ClickAMember(\"ml" + i + "\")' onmouseover='OnMouseOver(\"ml" + i + "\")' onmouseout='OnMouseOut(\"ml" + i + "\")' onmousedown='OnMouseDown(\"ml" + i + "\")' onmouseup='OnMouseOut(\"ml" + i + "\")' ontouchstart='OnMouseDown(\"ml" + i + "\")' ontouchend='OnMouseOut(\"ml" + i + "\")'><table><tr><td class='td_SSicon' valign='top'><img class='SSicon' src='" + ICON + "'/></td><td valign='top' class='Contact'><span id='ml" + i + "a'>" + ADDRESS + "</span><br>" + NAME + ROLE + "</td></tr></table></div>";
                    });
                    $("#PG_3_DT").html(s);
                    PG_3_PN = Number($R.find("PAGENUMBER").text());
                    PG_3_TP = Number($R.find("TOTALPAGES").text());
                    if (IsNullorEmpty(s)) {
                        $("#PG_3_PG").hide();
                    } else {
                        $("#PG_3_PG").show();
                    }
                    $("#PG_3_PN").text(PG_3_TP);
                    $.mobile.silentScroll(0);
                } else {
                    findReason($xmlDoc, popupid);
                }
            });
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

        function Publish() {
            var Title = $.trim($("#PG_11_TT").val());
            if (IsNullorEmpty(Title)) { return; }
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
            var XML = "<MeteorRain>"
            XML += "<Type>" + Type + "</Type>";
            XML += "<Title>" + HandleXMLSpecialChars(Title) + "</Title>";
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
            $("body").append('<div class="overlay"></div>');
            $.mobile.loading("show", {
                text: "请稍等……",
                textVisible: true,
                theme: "b",
                textonly: false,
                html: ""
            });
            window.external.PublishMeteorRain(XML);
        }

        function PublishSuccessful() {
            $("#PG_11_TT").val("")
            $("#PG_11_TX_ED").html("");
            $("#PG_11_VD_PA").html("");
            $("#PG_11_VD_DV").hide();
            $("#PG_11_VD_PV").prop("src", "").hide();
            $.mobile.loading("hide");
            $("div.overlay").remove();
            $.mobile.changePage("#PG_1");
        }

        function PublishFailed() {
            $.mobile.loading("hide");
            $("div.overlay").remove();
        }


        function RequestServer(urlpart, popupid, func, domain) {
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
                if (domain == undefined) {
                    xhr.open("POST", "../Default.aspx" + urlpart, true);
                } else {
                    xhr.open("POST", "https://" + domain + "/Default.aspx" + urlpart, true);
                }
                xhr.send(null);
            } else {
                showPopupInfo(popupid, "你正在使用的浏览器太陈旧。");
            }
        }

        function findReason($xmlDoc, popupid) {
            var $R = $xmlDoc.find("INVALIDCREDENTIAL");
            if ($R.length > 0) {
                showPopupInfo(popupid, "凭据失效，请重新打开小宇宙。");
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

        function CredentialReady(EnglishSSAddress2, UserCredential2, GroupID2) {
            EnglishSSAddress = EnglishSSAddress2;
            UserCredential = UserCredential2;
            GroupID = GroupID2;
            window.external.RequestReadCredential2();
        }

        function ReadCredentialReady(Domain, ReadCredential2) {
            Domain_Read = Domain;
            ReadCredential = ReadCredential2;
            $.mobile.changePage("#PG_2");
        }

    </script>
</head>
<body>
    <div data-role="page" id="PG_0">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>群的小宇宙</h1>
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
            <h1>群的小宇宙-商圈</h1>
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
            <div data-role="navbar">
                <ul>
                    <li><a href="#PG_2" data-icon="shop" class="ui-btn-active ui-state-persist">商圈</a></li>
                    <li><a href="#PG_1" data-icon="heart">流星语</a></li>
                    <li><a href="#PG_3" data-icon="user">群成员</a></li>
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
            <h1>群的小宇宙-流星语</h1>
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
            <a href="#PG_11" id="PG_1_PB" class="ui-btn ui-corner-all" style="display:none;">发流星语</a>
        </div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar">
                <ul>
                    <li><a href="#PG_2" data-icon="shop">商圈</a></li>
                    <li><a href="#PG_1" data-icon="heart" class="ui-btn-active ui-state-persist">流星语</a></li>
                    <li><a href="#PG_3" data-icon="user">群成员</a></li>
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
            <h1>大聊天群成员</h1>
        </div>
        <div role="main" class="ui-content">
            <div class="ui-grid-b">
                <div class="ui-block-a"><a class="ui-btn ui-corner-all" href="#" onclick="ListMembers(PG_3_PN - 1)">上一页</a></div>
                <div class="ui-block-b"><a class="ui-btn ui-corner-all" href="#" onclick="ListMembers(PG_3_PN + 1)">下一页</a></div>
                <div class="ui-block-c"><a class="ui-btn ui-corner-all" href="#" onclick="ListMembers()">刷新</a></div>
            </div>
            <div id="PG_3_DT"></div>
            <div class="ui-grid-b" id="PG_3_PG" style="display:none;">
                <div class="ui-block-a"><a class="ui-btn ui-corner-all" href="#" onclick="ListMembers(PG_3_PN - 1)">上一页</a></div>
                <div class="ui-block-b"><a class="ui-btn ui-corner-all" href="#" onclick="ListMembers(PG_3_PN + 1)">下一页</a></div>
                <div class="ui-block-c"><a class="ui-btn ui-corner-all" href="#PG_CP" onclick="PrepareToChangePage(PG_3_TP)" id="PG_3_PN"></a></div>
            </div>
        </div>
        <div data-role="footer" data-position="fixed" data-tap-toggle="false">
            <div data-role="navbar">
                <ul>
                    <li><a href="#PG_2" data-icon="shop">商圈</a></li>
                    <li><a href="#PG_1" data-icon="heart">流星语</a></li>
                    <li><a href="#PG_3" data-icon="user" class="ui-btn-active ui-state-persist">群成员</a></li>
                </ul>
            </div>
            <h4 style="display:none;"></h4>
        </div>
        <div class="ui-content" id="PG_3_PP" data-role="popup" data-theme="a" data-position-to="window">
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
    

   <div data-role="page" id="PG_11">
        <div data-role="header" data-position="fixed" data-tap-toggle="false">
            <h1>编辑器</h1>
        </div>
        <div role="main" class="ui-content">
            <select id="PG_11_TP" data-native-menu="false" onchange="ChooseType()">
                <option selected="selected" value="<%=流星语类型_常量集合.图文 %>">图文</option>
                <option value="<%=流星语类型_常量集合.视频 %>">视频</option>
            </select>
            <p>标题</p>
            <input id="PG_11_TT" type="text" data-theme="a" maxlength="<%=最大值_常量集合.流星语标题字符数 %>" />
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
            <div class="ui-grid-a">
                <div class="ui-block-a"><a href="#" class="ui-btn ui-corner-all ui-btn-b" onclick="Publish()">发布</a></div>
                <div class="ui-block-b"><a href="#PG_1" class="ui-btn ui-corner-all">取消</a></div>
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

</body>
</html>
