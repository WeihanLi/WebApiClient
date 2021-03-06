﻿using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace WebApiClientCore.Extensions.OAuths
{
    /// <summary>
    /// 表示用户名密码身份信息token提供者
    /// </summary>
    /// <typeparam name="THttpApi"></typeparam>
    public class PasswordCredentialsTokenProvider<THttpApi> : TokenProvider
    {
        /// <summary>
        /// 身份信息选项
        /// </summary>
        private readonly IOptions<PasswordCredentialsOptions<THttpApi>> credentialsOptions;

        /// <summary>
        /// 用户名密码身份信息token提供者
        /// </summary>
        /// <param name="services"></param>
        /// <param name="credentialsOptions"></param>
        public PasswordCredentialsTokenProvider(IServiceProvider services, IOptions<PasswordCredentialsOptions<THttpApi>> credentialsOptions)
            : base(services)
        {
            this.credentialsOptions = credentialsOptions;
        }

        /// <summary>
        /// 请求获取token
        /// </summary> 
        /// <returns></returns>
        protected override Task<TokenResult> RequestTokenAsync()
        {
            var options = this.credentialsOptions.Value;
            return this.CreateOAuthClient().RequestTokenAsync(options.Endpoint, options.Credentials);
        }

        /// <summary>
        /// 请求刷新token
        /// </summary> 
        /// <param name="refresh_token">刷新token</param>
        /// <returns></returns>
        protected override Task<TokenResult> RefreshTokenAsync(string refresh_token)
        {
            var options = this.credentialsOptions.Value;
            var credentials = new RefreshTokenCredentials
            {
                Client_id = options.Credentials.Client_id,
                Client_secret = options.Credentials.Client_secret,
                Extra = options.Credentials.Extra,
                Refresh_token = refresh_token
            };

            return this.CreateOAuthClient().RefreshTokenAsync(options.Endpoint, credentials);
        }
    }
}
