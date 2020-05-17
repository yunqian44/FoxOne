using FoxOne.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace FoxOne.Business.OAuth
{
    public class WeiboAuthenticationHandler : AuthenticationHandler
    {

        private readonly HttpClient _httpClient;
        public WeiboAuthenticationHandler(AuthenticationOptions options)
            : base(options)
        {
            this._httpClient = new HttpClient();
            this._httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
        }
        public override string GetAuthorizationUrl(AuthenticationScope scope)
        {
            string url = string.Format("{0}/oauth2/authorize?client_id={1}&redirect_uri={2}&response_type=code&state={3}",
                            this._options.AuthorizeUrl, this._options.AppId, Uri.EscapeDataString(string.Concat(_options.Host, _options.Callback)), scope.State);
            return url;
        }
        public override AuthenticationTicket PreAuthorization(AuthenticationTicket ticket)
        {
            //构建获取Access Token的参数
            string url = string.Format("{0}/oauth2/access_token",
                                             this._options.AuthorizeUrl);
            string tokenResponse;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            string param = "client_id=" + this._options.AppId + "&client_secret=" + this._options.AppSecret + "&grant_type=authorization_code" + "&code=" + ticket.Code + "&redirect_uri=" + Uri.EscapeDataString(string.Concat(_options.Host, _options.Callback));
            request.ContentType = "application/x-www-form-urlencoded";
            byte[] bs = Encoding.ASCII.GetBytes(param);
            request.ContentLength = bs.Length;
            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(bs, 0, bs.Length);
                dataStream.Close();
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader dataStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                tokenResponse = dataStream.ReadToEnd();
            }
            Logger.Info("请求url：{0}，返回值：{1}", url, tokenResponse);
            if (tokenResponse.IndexOf("errcode") > 0)
            {
                throw new FoxOneException(tokenResponse);
            }
            var callback = JSONHelper.Deserialize(tokenResponse,typeof(Callback)) as Callback;
            ticket.OpenId = callback.uid;
            ticket.AccessToken = callback.access_token;
            ticket.RefreshToken = callback.refresh_token;
            return ticket;
        }
        public override AuthenticationTicket AuthenticateCore(AuthenticationTicket ticket)
        {
            return ticket;
        }

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
            public string uid { get; set; }
        }
    }
}
