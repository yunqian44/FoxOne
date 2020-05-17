; (function () {
    var Util = {
        getTargetUrl: function (replaceUrl, targetUrl) {
            var protocol = location.protocol;
            var host = location.host;
            var pathname = location.pathname.replace(replaceUrl, targetUrl);
            return protocol + '//' + host + pathname;
        }

    };
    var Page = {
        init: function () {
            var that = this;
            //防止300毫秒点击延迟
            FastClick.attach(document.body);
            //绑定事件
            this.bind();
            dd.biz.navigation.setLeft({
                show: true,//控制按钮显示， true 显示， false 隐藏， 默认true
                control: true,//是否控制点击事件，true 控制，false 不控制， 默认false
                showIcon: true,//是否显示icon，true 显示， false 不显示，默认true； 注：具体UI以客户端为准       
                onSuccess: function (result) {
                    dd.device.notification.confirm({
                        message: "新增任务还未保存",
                        title: "提示",
                        buttonLabels: ['不保存', '继续'],
                        onSuccess: function (result) {
                            /*
					        {
					            buttonIndex: 0 //被点击按钮的索引值，Number类型，从0开始
					        }
					        */
                            if (result.buttonIndex == 0) {
                                dd.biz.navigation.close();
                            }
                        },
                        onFail: function (err) { }
                    });
                },
                onFail: function (err) { }
            });

            dd.biz.navigation.setRight({
                show: true,//控制按钮显示， true 显示， false 隐藏， 默认true
                control: true,//是否控制点击事件，true 控制，false 不控制， 默认false
                showIcon: true,//是否显示icon，true 显示， false 不显示，默认true； 注：具体UI以客户端为准
                text: "保存",
                onSuccess: function () {
                    var obj = {};
                    obj.Title = $('#Title').val();
                    if (!obj.Title) {
                        that.alert("请输入会议主题");
                        return;
                    }
                    obj.BeginTime = $('#StartDate').html() + " " + $('#StartTime').html();
                    if (!obj.BeginTime) {
                        that.alert("请选择开始时间");
                        return;
                    }
                    obj.EndTime = $('#EndDate').html() + " " + $('#EndTime').html();
                    if (!obj.EndTime) {
                        that.alert("请选择结束时间");
                        return;
                    }
                    obj.MeetingRoomId = $('#MeetingRoomId').val();
                    if (!obj.MeetingRoomId) {
                        that.alert("请选择会议室");
                        return;
                    }
                    obj.Description = $("#Description").val();
                    var originDate = JSON.stringify(obj);
                    $.ajax({
                        url: '/DD/Book',
                        type: "post",
                        async: true,
                        dataType: 'json',
                        data: obj,
                        error: function (res) {
                            that.alert("请求处理异常:" + res.responseText);
                        },
                        success: function (response) {
                            if (response.Result) {
                                if (response.NoAuthority) {
                                    that.alert(response.ErrorMessage);
                                }
                                else {
                                    if (response.LoginTimeOut) {
                                        that.alert("无权限");
                                    }
                                    else {
                                        if (response.Data)
                                        {
                                            that.alert("预订成功");
                                            dd.biz.navigation.close();
                                        }
                                        
                                    }
                                }
                            }
                            else {
                                that.alert(response.ErrorMessage);
                            }
                        },
                    });
                }
            });
        },
        alert: function (msg, title) {
            title = title || "系统提示";
            dd.device.notification.alert(
                {
                    message: msg,
                    title: title,
                    buttonName: '关闭'
                })
        },
        bind: function () {
            $('#task-add .datepicker').on('click', function () {
                var node = $(this).find('.date');
                var v = node.html().trim();
                dd.biz.util.datepicker({
                    format: 'yyyy-MM-dd',
                    value: v, 
                    onSuccess: function (result) {
                        node.html(result.value);
                    },
                    onFail: function () { }
                });
            });
            $('#task-add .timepicker').on('click', function () {
                var node = $(this).find('.date');
                var v = node.html().trim();
                dd.biz.util.timepicker({
                    format: 'HH:mm',
                    value: v, //默认显示日期
                    onSuccess: function (result) {
                        node.html(result.value);
                    },
                    onFail: function () { }
                });
            });

            $('#task-add .select-type').on('click', function () {
                var node = $(this).find('.select');
                var s = $(this).find('input[name=MeetingRoomId]');
                var v = node.html().trim();
                dd.biz.util.chosen({
                    source: meetingRoom,
                    selectedKey: v,
                    onSuccess: function (result) {
                        node.html(result.key);
                        s.val(result.value);
                    },
                    onFail: function () { }
                })
            });
        }
    };
    //为了能够在PC端进行测试
    if (dd.version) {
        dd.ready(function () {
            Page.init();
        });
    } else {
        Page.init();
    }
})();