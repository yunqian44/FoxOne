/// <reference path="jquery-1.8.2.js" />
/// <reference path="common.js" />
(function (window, $) {
    var foxOne = window.foxOne;
    var workflow = function () { };
    workflow.prototype = {
        _STEP_LIST_DIV: 'stepListDiv',
        _TREE_VIEW_DIV: 'treeViewDiv',
        _TREE_LIST_DIV: 'userList',
        _TREE_CTRL_TEMP: 'userTree',
        startParameter: { AppCode: '', InstanceName: '', DataLocator: '', ImportLevel: 0, SecurityLevel: 0 },
        runParameter: { IsSimulate: '0', Command: '', InstanceId: '', ItemId: 1, OpinionContent: '', OpinionArea: 0, UserChoice: [] },
        getNextStepUrl: '/Workflow/GetNextStep',
        startUrl: '/Workflow/Start',
        saveUrl: '/Workflow/Save',
        execUrl: '/Workflow/ExecCommand',
        userSelectDivHtml: "<div class='user-select'><table cellspacing='10' cellpadding='5' class='user-select-table' width='100%'><tr><td colspan='2' class='user-select-td-bg'>选择后续步骤以及相关处理人员</td></tr><tr><td>可选步骤：<div class='step-list' id='stepListDiv'></div></td><td class='tree-list-td'><div id='treeViewDiv' style='height: 450px;overflow-y:auto;'></div></td></tr><tr><td colspan='2' class='user-select-td-bg tar'><input type='button' id='btnRunFlow' class='btn btn-success btn-sm' value=' 发 送 ' />&nbsp;<input type='button' id='btnCancelRun' class='btn btn-danger btn-sm' value=' 取 消 ' /></td></tr></table></div>",
        setting: {
            view: {
                showLine: true,
                selectedMulti: false,
                dblClickExpand: false
            },
            data: {
                simpleData: {
                    enable: true
                }
            }
        },
        start: function (appCode, instanceName, dataLocator) {
            var that = this;
            that.startParameter.AppCode = appCode;
            that.startParameter.InstanceName = instanceName;
            that.startParameter.DataLocator = dataLocator;
            foxOne.dataService(that.startUrl, that.startParameter, function (res) {
                that.runParameter.InstanceId = res;
                that.runParameter.ItemId = 1;
            }, "post", true);
        },
        getNewUserChoice: function () {
            return { StepName: '', Id: '', Name: '', DepartmentId: '' };
        },
        getNextStep: function () {
            var that = this;
            if ($(".user-select").length == 0) {
                $(that.userSelectDivHtml).appendTo("body");
                $("#btnRunFlow").bind("click", function () {
                    that.run.call(foxOne.workflow);
                });
                $("#btnCancelRun").bind("click", $.closeModal);
            }
            foxOne.dataService(that.getNextStepUrl, that.runParameter, function (nextStep) {
                var canPostback = false;
                if (nextStep == null || nextStep.length == 0) {
                    foxOne.alert("无可用迁移！");
                    return;
                }
                if (nextStep != null && nextStep[0].StepName == "自动发送") {
                    foxOne.alert("发送成功");
                    if (that.runParameter.IsSimulate == '1') {
                        window.location.href = "/Workflow/AutoRun/" + that.runParameter.InstanceId;
                    }
                    else {
                        window.close();
                    }
                }
                else {
                    $("#stepListDiv").html("");
                    $.each(nextStep, function (index, i) {
                        var input = that.hasSameMultipleTag(i, nextStep) ?
                            "<input type='checkbox' />" :
                            "<input type='radio' />";
                        $(input).attr("id", "step" + index)
                            .attr("name", "step")
                            .data({ index: index, stepName: i.StepName, needUser: i.NeedUser, tag: i.MultipleSelectTag })
                            .css("margin-left", "10px")
                            .css("margin-top", "10px")
                            .bind("click", function (e) {
                                var inp = e.target;
                                var ckData = $(this).data();
                                if (inp.checked) {
                                    $("input[name='step']").not(inp).each(function () {
                                        var tempData = $(this).data();
                                        if (tempData.tag != ckData.tag) {
                                            $(this).attr("checked", false);
                                        }
                                    });
                                }
                                var index = ckData.index;
                                $("div[id*='" + that._TREE_LIST_DIV + "']").hide();
                                $("#" + that._TREE_LIST_DIV + index).show();
                            })
                            .appendTo("#" + that._STEP_LIST_DIV).after("<label style='margin-left:3px;' for='step" + index + "'>" + i.Label + "</label><br />")
                        $("<div id='" + that._TREE_LIST_DIV + index + "'><ul class='ztree' id='" + that._TREE_CTRL_TEMP + index + "'></ul></div>").appendTo("#" + that._TREE_VIEW_DIV);
                        that.setting.check = { enable: true, chkStyle: i.OnlySingleSel ? "radio" : "checkbox" };
                        $.fn.zTree.init($("#" + that._TREE_CTRL_TEMP + index), that.setting, that.convertToTreeData(i));
                    });
                    $("div[id*='userList']").hide();
                    $.modalInner($(".user-select"), true, function () { }, 700, 500, window, false);
                }
            }, "post", true);
        },
        exec: function (command) {
            this.runParameter.Command = command;
            foxOne.dataService(this.execUrl, this.runParameter, function (data) {
                if (data == true) {
                    foxOne.alert("操作成功");
                    window.close();
                }
            }, "post", true);
        },
        run: function () {
            try {
                var that = this;
                that.getUserChoice();
                that.runParameter.Command = "run";
                if (that.runParameter.UserChoice.length > 0) {
                    foxOne.dataService(that.execUrl, that.runParameter, function (data) {
                        if (data == true) {
                            foxOne.alert("发送成功");
                            if (that.runParameter.IsSimulate == '1') {
                                window.location.href = "/Workflow/AutoRun/" + that.runParameter.InstanceId;
                            }
                            else {
                                window.close();
                            }
                        }
                    }, "post", true);
                }
                else {
                    foxOne.alert("没有选中用户");
                }
            } catch (e) {
                foxOne.alert(e);
            }
        },
        getUserChoice: function () {
            var that = this;
            that.runParameter.OpinionArea = 0;
            //that.runParameter.OpinionContent = '';
            that.runParameter.UserChoice = [];
            var selected = $("#" + that._STEP_LIST_DIV).find(":checked");
            if (selected.length <= 0) {
                throw "请至少选择一下后续步骤";
            }
            selected.each(function () {
                var ckData = $(this).data();
                var treeId = that._TREE_CTRL_TEMP + ckData.index;
                var item = $.fn.zTree.getZTreeObj(treeId);
                var nodes = item.getCheckedNodes();
                if (nodes.length == 0) {
                    if (ckData.needUser) {
                        throw "步骤【" + ckData.stepName + "】需要选择参与者"
                    }
                    var choice = that.getNewUserChoice();
                    choice.StepName = ckData.stepName;
                    that.runParameter.UserChoice.push(choice);
                }
                else {
                    $.each(nodes, function (innerIndex, node) {
                        if (!node.isParent) {
                            var choice = that.getNewUserChoice();
                            choice.StepName = node.stepName;
                            choice.Id = node.id;
                            choice.Name = node.name;
                            choice.DepartmentId = node.pId;
                            that.runParameter.UserChoice.push(choice);
                        }
                    });
                }
            });
        },
        convertToTreeData: function (step) {
            var nodes = [];
            var users = step.Users;
            var chkDisabled = false;// !step.AllowSelect;
            if (chkDisabled) {
                step.OnlySingleSel = false;
                step.AutoSelectAll = true;
            }
            var nocheck = step.OnlySingleSel;
            var checked = step.AutoSelectAll;
            var that = this;
            if (!users || users.length == 0) {
                nodes.push({ nocheck: true, id: 'root', name: "步骤【" + step.StepName + '】无待选用户', open: false, pId: '' });
            }
            else {
                nodes.push({ nocheck: nocheck, chkDisabled: chkDisabled, checked: checked, id: 'root', name: "步骤【" + step.StepName + '】的待选用户', open: true, pId: '' });
                $.each(users, function (index, i) {
                    if (!that.ifExistInNodes(nodes, i.OrgId)) {
                        nodes.push({ nocheck: nocheck, chkDisabled: chkDisabled, checked: checked, id: i.OrgId, name: i.OrgName, open: true, pId: 'root' })
                    }
                    nodes.push({ chkDisabled: chkDisabled, checked: checked, id: i.ID, name: i.Name, pId: i.OrgId, stepName: step.StepName })
                });
            }
            return nodes;
        },
        ifExistInNodes: function (nodes, id) {
            for (var i = 0; i < nodes.length; i++) {
                if (nodes[i].id == id) return true;
            }
            return false;
        },
        hasSameMultipleTag: function (currentStep, step) {
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
    }
    foxOne.workflow = new workflow();
})(window, jQuery);