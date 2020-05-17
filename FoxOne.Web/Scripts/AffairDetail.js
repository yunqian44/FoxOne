/*********************************************************
* 作　　者：刘海峰
* 联系邮箱：mailTo:liuhf@foxone.net
* 创建时间：$time$
* 描述说明：
* *******************************************************/
$(document).ready(function () {
    $("#baseInfo").find("input,select,textarea").each(function () {
        $(this).attr("original-value", $(this).val());
    });
    /*
    window.onbeforeunload = function () {
    if (formHasChanged() && !IsAction) {
    return "您已更改表单，确定要离开此页面吗？";
    }
    return;
    };
    */
});

function formHasChanged() {
    var returnValue = false;
    $("#baseInfo").find("input,select,textarea").not("[area]").each(function () {
        if ($(this).attr("original-value") != $(this).val()) {
            returnValue = true;
            return;
        }
    });
    return returnValue;
}


//发送
function send(e) {
    Common.Util.EventPreventDefault(e);
    if (typeof (beforeFormSendValidate) == "function") {
        if (!beforeFormSendValidate()) return false;
    }
    if ($.validation.validate(document.forms[0]).isError) return false;
    if (appCaseID == "") {
        alert("请先保存表单数据！");
        return false;
    }
    if (typeof (beforeFormSend) == "function") {
        if (!beforeFormSend())
            return false;
    }
    if (formHasChanged()) {
        if (confirm("检测到表单已被更改，是否在发送前先保存？")) {
            theForm.__EVENTTARGET.value = "lkSave";
            Common.Util.Ajax(theForm.action, $(theForm).serialize(), "post", function () {
                $("#baseInfo").find("input,select,textarea").each(function () {
                    if ($(this).attr("original-value") != $(this).val()) {
                        $(this).attr("original-value", $(this).val());
                    }
                });
            }, "text", true);
        }
    }
    var parameter = {};
    parameter.procID = procID;
    parameter.taskID = taskID;
    var opinionControl = $("textarea[area]");
    parameter.opinionContent = "";
    parameter.isCoverBefore = false;
    parameter.area = 1;
    if (opinionControl.length >= 1) {
        parameter.opinionContent = opinionControl.val();
        parameter.area = parseInt(opinionControl.attr("area"));
    }
    Common.UI.Block();
    Common.Util.DataServices("WorkflowDataServices$GetNextStep", parameter, onNextStep, "json");
    return false;
}

function showStepName(stepName) {
    while (stepName.indexOf('.') >= 0) {
        stepName = stepName.substring(stepName.indexOf('.') + 1);
    }
    return stepName;
}

function sendSuccess() {
    alert("发送成功!");
    if (IsSimulate == 'True') {
        window.location.href = window.location.href;
    }
    else {
        closeWin();
    }
}

function hasSameMultipleTag(currentStep, step) {
    if (!currentStep.MultipleSelectTag || currentStep.MultipleSelectTag == "") return false;
    var res = false;
    $.each(step, function (index, i) {
        if (i.MultipleSelectTag == currentStep.MultipleSelectTag && i.StepName != currentStep.StepName) {
            res = true;
            return;
        }
    });
    return res;
}

//需要弹出选人框时
var globalNextStep = undefined;
function onNextStep(nextStep) {
    Common.UI.UnBlock();
    $("#slUserSelected").find("option").remove();
    var canPostback = false;
    if (nextStep == null || nextStep.length == 0) {
        alert("无可用迁移！");
        return;
    }
    if (nextStep != null && nextStep[0].StepName == "自动发送") {
        sendSuccess();
    }
    else {
        globalNextStep = nextStep;
        var stepListHtml = "";
        var treeViewHtml = "";
        $.each(nextStep, function (index, i) {
            if (hasSameMultipleTag(i, nextStep)) {
                stepListHtml += Common.Util.StringFormat("<p><input type='checkbox' name='step' value='{0}' id='step{1}' index='{1}' onclick=\"chShowUserList({1},'{2}',this)\" MultipleSelectTag='{2}' /><label for='step{1}' style='cursor:pointer;' >{3}</label></p>", i.StepName, index, i.MultipleSelectTag, i.Label); // showStepName(i.StepName));
            }
            else {
                stepListHtml += Common.Util.StringFormat("<p><input type='radio' name='step' value='{0}' id='step{1}' index='{1}' onclick='showUserList({1})' /><label for='step{1}' style='cursor:pointer;' >{2}</label></p>", i.StepName, index, i.Label); // showStepName(i.StepName));
            }
            treeViewHtml += "<div id='userList" + index + "' stepName='" + i.StepName + "' needUser='" + i.NeedUser + "' allowSelect='" + i.AllowSelect + "' autoSelectAll='" + i.AutoSelectAll + "' onlySingleSel='" + i.OnlySingleSel + "' class='hide' style='font-size:13px;'><img src='Css/itemdetail/root.gif'>";
            if (i.OnlySingleSel) {
                treeViewHtml += "<label class='ml5'>待选用户</label><br />";
                if (i.Users && i.Users.length > 0) {
                    var tempHtml = "";
                    var j = i.Users[0];
                    var tempOrgId = j.OrgId;
                    treeViewHtml += Common.Util.StringFormat("<div  class='ml20' id='{0}'><img src='Css/itemdetail/dept.gif'><label  class='ml5'>{1}</label><br />", j.OrgId, j.OrgName);
                    $.each(i.Users, function (index1, j) {
                        if (tempOrgId != j.OrgId) {
                            treeViewHtml += Common.Util.StringFormat("</div><div  class='ml20' id='{0}'><img src='Css/itemdetail/dept.gif'><label  class='ml5'>{1}</label><br />", j.OrgId, j.OrgName);
                            tempOrgId = j.OrgId;
                        }
                        treeViewHtml += Common.Util.StringFormat("<img src='Css/itemdetail/user.gif' class='ml20'><input type='radio' name='users' value='{0}' id='user{1}' step='{2}' onclick='onUserRadioChecked(this);' orgId='{3}' UserId='{4}' /><label for='user{1}' >{5}</label><br />", j.Name, index + "-" + index1, i.StepName, j.OrgId, j.ID, j.Name);
                    });
                    treeViewHtml += "</div>";
                }
                treeViewHtml += "</div>";
            }
            else {
                treeViewHtml += "<input type='checkbox' id='ck" + index + "' name='trees' onclick='onOrgChecked(this)' /><label for='ck" + index + "' >待选用户</label><br />";
                if (i.Users && i.Users.length > 0) {
                    var tempHtml = "";
                    var j = i.Users[0];
                    var tempOrgId = j.OrgId;
                    treeViewHtml += Common.Util.StringFormat("<div  class='ml20' id='{0}'><img src='Css/itemdetail/dept.gif'><input type='checkbox' id='{0}ck' name='orgs' onclick='onOrgChecked(this);' /><label for='{0}ck' >{1}</label><br />", j.OrgId, j.OrgName);
                    $.each(i.Users, function (index1, j) {
                        if (tempOrgId != j.OrgId) {
                            treeViewHtml += Common.Util.StringFormat("</div><div  class='ml20' id='{0}'><img src='Css/itemdetail/dept.gif'><input type='checkbox' id='{0}ck' name='orgs' onclick='onOrgChecked(this);' /><label for='{0}ck' >{1}</label><br />", j.OrgId, j.OrgName);
                            tempOrgId = j.OrgId;
                        }
                        treeViewHtml += Common.Util.StringFormat("<img src='Css/itemdetail/user.gif' class='ml20'><input type='checkbox' name='users' value='{0}' onclick='onUserChecked(this);' id='user{1}' step='{2}' orgId='{3}' UserId='{4}' /><label for='user{1}' >{5}</label><br />", j.Name, index + "-" + index1, i.StepName, j.OrgId, j.ID, j.Name);
                    });
                    treeViewHtml += "</div>";
                }
                treeViewHtml += "</div>";
            }
        });
        $("#stepListDiv").html(stepListHtml);
        $("#treeViewDiv").html(treeViewHtml);
        Common.UI.ShowModalDialog({
            onOpen: function () {
                if (typeof (onsendOpen) == "function") {
                    onsendOpen();
                }
                $(".main-content").hide();
            }, onClose: function () {
                if (typeof (onsendClose) == "function") {
                    onsendClose();
                }
                $(".main-content").show();
            }, element: "UserSelector", Dragable: false
        });
    }
}
var userCkIndex = 1;
function onUserRadioChecked(ck) {
    if (!$(ck).attr("step") || $(ck).attr("step").length == 0) return;
    $("#slUserSelected").find("option[value*='" + $(ck).attr("step") + ":']").remove();
    $(ck).attr("selectIndex", userCkIndex++);
    $("#slUserSelected").append("<option value='" + $(ck).attr("step") + ":" + $(ck).attr("UserId") + "' >" + $(ck).attr("step") + ":" + $(ck).next().html() + "</option>");

}
function onUserChecked(ck) {
    if (!$(ck).attr("step") || $(ck).attr("step").length == 0) return;
    var c = $("#slUserSelected").find("option[value='" + $(ck).attr("step") + ":" + $(ck).attr("UserId") + "']");
    if (ck.checked) {
        $(ck).attr("selectIndex", userCkIndex++);
        if (c.length == 0) {
            $("#slUserSelected").append("<option value='" + $(ck).attr("step") + ":" + $(ck).attr("UserId") + "' >" + $(ck).attr("step") + ":" + $(ck).next().html() + "</option>");
        }
    }
    else {
        if (c.length > 0) {
            c.remove();
        }
    }
}
function onOrgChecked(ck) {
    $(ck).parent().find("input[type='checkbox']").attr("checked", ck.checked);
    $(ck).parent().find("[step]").each(function () {
        onUserChecked(this);
    });
}
function runFlow() {
    var selectedSteps = $("#stepListDiv").find(":checked");
    if (selectedSteps.length == 0) {
        alert("请至少选择一个步骤！");
        return;
    }
    var errorInfo = "";
    var userChoice = [];
    $.each(selectedSteps, function (i, item) {
        var index = $(item).attr("index");
        var div = $("#userList" + index);
        var needUser = div.attr("needUser");
        var selected = div.find(":checked");
        if (needUser == "true") {
            var count = 0;
            selected.each(function () {
                if ($(this).attr("name") == "users") {
                    count++;
                    userChoice.push(Common.Util.StringFormat("{0}_{1}_{2}_{3}", $(this).attr("UserId"), $(this).attr("orgId"), $(this).attr("step"), $(this).attr("selectIndex")));
                }
            });
            if (count == 0) {
                errorInfo += "步骤【" + $(item).val() + "】需要至少一个参与者";
            }
        }
        else {
            userChoice.push(Common.Util.StringFormat("{0}_{1}_{2}_{3}", "NULL", "NULL", $(item).val(), "NULL"));
        }
    });
    if (errorInfo != "") {
        alert(errorInfo);
        return;
    }
    var opinionControl = $("textarea[area]");
    var opinionContent = "";
    var opinionArea = 1;
    if (opinionControl.length >= 1) {
        opinionContent = opinionControl.val();
        opinionArea = parseInt(opinionControl.attr("area"));
    }
    Common.UI.Block();
    Common.Util.DataServices("WorkflowDataServices$RunFlow", { isCoverBefore: false, opinionContent: opinionContent, area: opinionArea, procID: procID, taskID: taskID, userChoice: userChoice.join(',') }, function (res) {
        if (res) {
            if (IsSimulate == 'true') {
                window.location.href = 'WorkflowDetail.aspx?ProcID=' + procID + '&TaskID=' + (taskID + 1) + '&IsSimulate=true';
            }
            else {
                sendSuccess();
            }
        }
        else {
            alert("发送失败！");
        }
        Common.UI.UnBlock();
    }, "json");
}

function chShowUserList(i, MultipleSelectTag, ck) {
    if (ck.checked) {
        var selectedSteps = $("#stepListDiv").find(":checked");
        $.each(selectedSteps, function (index, item) {
            if ($(item).attr("MultipleSelectTag") != MultipleSelectTag) {
                var i = $(item).attr("index");
                var div = $("#userList" + i);
                div.find(":checked").attr("checked", false);
                var step = div.attr("stepName");
                $("#slUserSelected").find("option[value*='" + step + ":']").remove();
                $(item).attr("checked", false);
            }
        });
        $("div[id*='userList']").hide();
        showCurrentDiv(i);
    }
    else {
        var div = $("#userList" + i);
        div.find("input[type='checkbox'],input[type='radio']").attr("checked", false);
        var step = div.attr("stepName");
        $("#slUserSelected").find("option[value*='" + step + ":']").remove();
        div.hide();
    }
}

function showCurrentDiv(i) {
    var div = $("#userList" + i);
    if (div.attr("autoSelectAll") == "true" || div.find("input[name='users']").length == 1) {
        var chks = div.find("input[type='checkbox'],input[type='radio']");
        chks.each(function () {
            if (this.name == "users") {
                $("#slUserSelected").append("<option value='" + $(this).attr("step") + ":" + $(this).attr("UserId") + "' >" + $(this).attr("step") + ":" + $(this).next().html() + "</option>");
            }
            $(this).attr("checked", true);
        });
    }
    if (div.attr("allowSelect") == "false") {
        div.find("input[type='checkbox'],input[type='radio']").attr("disabled", true);
    }
    div.show();
}

function showUserList(i) {
    var selectedSteps = $("#stepListDiv").find("input[type='checkbox']").attr("checked", false);
    var divs = $("div[id*='userList']");
    divs.find(":checked").attr("checked", false);
    $("#slUserSelected").find("option").remove();
    divs.hide();
    showCurrentDiv(i);
}