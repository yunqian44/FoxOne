using FoxOne.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace FoxOne.Business.OAuth
{
    public class QQAuthenticationHandler : AuthenticationHandler
    {

        private readonly HttpClient _httpClient;
        public QQAuthenticationHandler(AuthenticationOptions options) : base(options)
        {
            this._httpClient = new HttpClient();
        }
        public override string GetAuthorizationUrl(AuthenticationScope scope)
        {
            string url = string.Empty;
            if (string.IsNullOrEmpty(scope.Scope))
            {
                url = string.Format("{0}/oauth2.0/authorize?response_type=code&client_id={1}&redirect_uri={2}&state={3}", _options.AuthorizeUrl, _options.AppId, string.Concat(_options.Host, _options.Callback), scope.State);
            }
            else
            {
                url = string.Format("{0}/oauth2.0/authorize?response_type=code&client_id={1}&redirect_uri={2}&state={3}&scope={4}", _options.AuthorizeUrl, _options.AppId, Uri.EscapeDataString(string.Concat(_options.Host, _options.Callback)), scope.State, scope.Scope);
            }
            return url;
        }
        public override AuthenticationTicket PreAuthorization(AuthenticationTicket ticket)
        {
            string tokenEndpoint = string.Concat(_options.AuthorizeUrl, "/oauth2.0/token?grant_type=authorization_code&client_id={0}&client_secret={1}&code={2}&redirect_uri={3}");
            var url = string.Format(
                     tokenEndpoint,
                     Uri.EscapeDataString(_options.AppId),
                     Uri.EscapeDataString(_options.AppSecret),
                     Uri.EscapeDataString(ticket.Code), Uri.EscapeDataString(string.Concat(_options.Host, _options.Callback)));
            string tokenResponse = _httpClient.GetStringAsync(url).Result.ToString();
            Logger.Info("请求url：{0}，返回值：{1}", url, tokenResponse);
            if (tokenResponse.IndexOf('&') > 0)
            {
                var parameters = tokenResponse.Split('&');
                foreach (var parameter in parameters)
                {
                    var accessTokens = parameter.Split('=');
                    if (accessTokens[0] == "access_token")
                    {
                        ticket.AccessToken = accessTokens[1];
                    }
                    else if (accessTokens[0] == "refresh_token")
                    {
                        ticket.RefreshToken = accessTokens[1];
                    }

                }
            }
            return ticket;
        }
        public override AuthenticationTicket AuthenticateCore(AuthenticationTicket ticket)
        {
            string tokenEndpoint = string.Concat(_options.AuthorizeUrl, "/oauth2.0/me?access_token={0}");
            var url = string.Format(
                     tokenEndpoint, ticket.AccessToken);
            string tokenResponse = _httpClient.GetStringAsync(url).Result.ToString();
            Logger.Info("请求url：{0}，返回值：{1}", url, tokenResponse);
            string strJson = tokenResponse.Replace("callback(", "").Replace(");", "");
            var payload = JSONHelper.Deserialize(strJson, typeof(Callback)) as Callback;
            ticket.OpenId = payload.openid;
            return ticket;
        }

        /*
        public override IUser GetUserInfo(AuthenticationTicket ticket)
        {
            string tokenEndpoint = string.Concat(_options.AuthorizeUrl, "/user/get_user_info?access_token={0}&oauth_consumer_key={1}&openid={2}");
            var url = string.Format(
                     tokenEndpoint, ticket.AccessToken, _options.AppId, ticket.OpenId);
            string tokenResponse = _httpClient.GetStringAsync(url).Result.ToString();
            Qzone qzone = JSONHelper.Deserialize(tokenResponse,typeof(Qzone)) as Qzone;
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
            public string client_id { get; set; }

            /// <summary>
            /// 用户Id
            /// </summary>
            public string openid { get; set; }
        }
        private class Qzone
        {
            public int ret { get; set; }

            public string msg { get; set; }
            /// <summary>
            /// 昵称 
            /// </summary>
            public string nickname { get; set; }
            /// <summary>
            /// 头像URL
            /// </summary>
            public string figureurl { get; set; }
            /// <summary>
            /// 性别
            /// </summary>
            public string gender { get; set; }
        }
    }
}
