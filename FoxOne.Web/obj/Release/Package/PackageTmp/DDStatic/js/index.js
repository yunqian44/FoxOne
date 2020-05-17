;(function(){
	var hash;
	var isShow = true;
	var t = 0;
	var pullDtd;
	var Util = {
		getQuery:function(param){
			var url = window.location.href;
            var searchIndex = url.indexOf('?');
            var searchParams = url.slice(searchIndex + 1).split('&');
            for (var i = 0; i < searchParams.length; i++) {
                var items = searchParams[i].split('=');
                if (items[0].trim() == param) {
                    return items[1].trim();
                }
            }
		},
		getTargetUrl:function(replaceUrl,targetUrl){
			var protocol = location.protocol;
			var host = location.host;
			var pathname = location.pathname.replace(replaceUrl,targetUrl);
			return protocol+'//'+host+pathname;
		}

	};

	var Page = {
		init:function(){
			var that = this;
			//防止300毫秒点击延迟
			FastClick.attach(document.body);

			dd.biz.navigation.setRight({
			    show: true,
			    control: true,
			    showIcon: true,
			    text: "新增预订",
			    onSuccess: function () {
			        that.go("add");
			    }
			});
			//绑定下拉事件
			dd.ui.pullToRefresh.enable({
				onSuccess: function() {
					setTimeout(function(){
						//todo 相关数据更新操作
						dd.ui.pullToRefresh.stop();
					},2000);
				},
				onFail: function() {
				}
			});
			//绑定每个任务的点击事件，事件采用代理的方式
			$('.doc').on('click','.item',function(){
				var _this = $(this);
				_this.addClass('active');
				setTimeout(function(){
					_this.removeClass('active');
				},100);
				
			});
		},
		go:function(page,taskId,taskType){
			var that = this;
			if(page=='add'){
				//这里替换为对应的页面url
				dd.biz.util.openLink({
				    url:Util.getTargetUrl('Index','Add')
				});
				return;
				
			}else if(page=='detail'){
				dd.biz.util.openLink({
				    url: Util.getTargetUrl('Index', 'Detail')
				});
				return;
			}
		}
	};
	if (dd.version) { 
	    dd.ready(function () {
			Page.init();
		});
	} else {
		Page.init();
	} 
})();