using FoxOne.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace FoxOne.Business.OAuth
{
    public class WechatAuthenticationHandler : AuthenticationHandler
    {

        private readonly HttpClient _httpClient;
        public WechatAuthenticationHandler(AuthenticationOptions options)
            : base(options)
        {
            this._httpClient = new HttpClient();
        }
        public override string GetAuthorizationUrl(AuthenticationScope scope)
        {
            string url = string.Format("{0}/connect/qrconnect?appid={1}&redirect_uri={2}&response_type=code&scope=snsapi_login&state={3}#wechat_redirect",
                            this._options.AuthorizeUrl, this._options.AppId, Uri.EscapeDataString(string.Concat(_options.Host, _options.Callback)), scope.State);
            return url;
        }
        public override AuthenticationTicket PreAuthorization(AuthenticationTicket ticket)
        {
            //构建获取Access Token的参数
            string url = string.Format("{0}/sns/oauth2/access_token?appid={1}&secret={2}&code={3}&grant_type=authorization_code",
                                             this._options.AuthorizeUrl, this._options.AppId, this._options.AppSecret, ticket.Code);
            string tokenResponse = _httpClient.GetStringAsync(url).Result.ToString();
            Logger.Info("请求url：{0}，返回值：{1}", url, tokenResponse);
            if (tokenResponse.IndexOf("errcode") > 0)
            {
                throw new FoxOneException(tokenResponse);
            }
            var callback = JSONHelper.Deserialize(tokenResponse,typeof(Callback)) as Callback;
            ticket.OpenId = callback.openid;
            ticket.AccessToken = callback.access_token;
            ticket.RefreshToken = callback.refresh_token;
            return ticket;
        }
        public override AuthenticationTicket AuthenticateCore(AuthenticationTicket ticket)
        {
            return ticket;
        }

        /*
        public override IUser GetUserInfo(AuthenticationTicket ticket)
        {
            string url = string.Format("{0}/sns/userinfo?access_token={1}&openid={2}",
                                          this._options.AuthorizeUrl, ticket.AccessToken, ticket.OpenId);
            string tokenResponse = _httpClient.GetStringAsync(url).Result.ToString();
            if (tokenResponse.IndexOf("errcode") > 0)
            {
                throw new FoxOneException(tokenResponse);
            }
            WeChat qzone = JSONHelper.Deserialize(tokenResponse,typeof(WeChat)) as WeChat;
            return new User
            {
                Id = ticket.OpenId,
                Name = qzone.nickname
            };
        }*/
        /// <summary>
        /// 根据access_token获得对应用户身份的openid
        /// </summary>
        private class Callback
        {
            /// <summary>
            /// 客户端Id
            /// </summary>
            public string access_token { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string refresh_token { get; set; }
            /// <summary>
            /// 用户Id
            /// </summary>
            public string openid { get; set; }
        }
        private class WeChat
        {
            /// <summary>
            /// 昵称 
            /// </summary>
            public string nickname { get; set; }
            /// <summary>
            /// 头像URL
            /// </summary>
            public string headimgurl { get; set; }
            /// <summary>
            /// 性别
            /// </summary>
            public int sex { get; set; }
            public string unionid { get; set; }
        }
    }
}
