﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApiClientCore.Attributes
{
    /// <summary>
    /// 表示将请求和响应内容的输出为日志的过滤器
    /// </summary>
    public class LoggingFilterAttribute : ApiFilterAttribute
    {
        /// <summary>
        /// 获取或设置是否输出请求内容
        /// </summary>
        public bool LogRequest { get; set; } = true;

        /// <summary>
        /// 获取或设置是否输出响应内容
        /// </summary>
        public bool LogResponse { get; set; } = true;

        /// <summary>
        /// 将请求和响应内容的输出为日志的过滤器
        /// </summary>
        public LoggingFilterAttribute()
        {
            this.OrderIndex = int.MaxValue;
        }

        /// <summary>
        /// 请求之前
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        public sealed async override Task OnRequestAsync(ApiRequestContext context)
        {
            var logMessage = new LogMessage
            {
                RequestTime = DateTime.Now,
                HasRequest = this.LogRequest
            };

            if (this.LogRequest == true)
            {
                var request = context.HttpContext.RequestMessage;
                logMessage.RequestHeaders = request.GetHeadersString();
                logMessage.RequestContent = await this.ReadRequestContentAsync(request.Content).ConfigureAwait(false);
            }

            context.UserDatas.Set(typeof(LoggingFilterAttribute), logMessage);
        }



        /// <summary>
        /// 响应后
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        public sealed async override Task OnResponseAsync(ApiResponseContext context)
        {
            var response = context.HttpContext.ResponseMessage;
            var logMessage = context.UserDatas.Get<LogMessage>(typeof(LoggingFilterAttribute));

            logMessage.ResponseTime = DateTime.Now;
            logMessage.Exception = context.Exception;

            if (this.LogResponse && response != null)
            {
                logMessage.HasResponse = true;
                logMessage.ResponseHeaders = response.GetHeadersString();
                logMessage.ResponseContent = await this.ReadResponseContentAsync(response.Content).ConfigureAwait(false);
            }

            await this.WriteLogAsync(context, logMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// 读取请求内容
        /// </summary>
        /// <param name="httpContent"></param>
        /// <returns></returns>
        private async Task<string> ReadRequestContentAsync(HttpContent httpContent)
        {
            if (httpContent == null)
            {
                return null;
            }

            return httpContent is ICustomHttpContentConvertable convertable
                ? await convertable.ToCustomHttpContext().ReadAsStringAsync().ConfigureAwait(false)
                : await httpContent.ReadAsStringAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 读取响应内容
        /// </summary>
        /// <param name="httpContent"></param>
        /// <returns></returns>
        private Task<string> ReadResponseContentAsync(HttpContent httpContent)
        {
            return httpContent?.ReadAsStringAsync();
        }

        /// <summary>
        /// 写日志到LoggerFactory
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="logMessage">日志消息</param>
        /// <returns></returns>
        protected virtual Task WriteLogAsync(ApiResponseContext context, LogMessage logMessage)
        {
            var method = context.ApiAction.Member;
            var categoryName = $"{method.DeclaringType.Namespace}.{method.DeclaringType.Name}.{method.Name}";

            var loggerFactory = context.HttpContext.Services.GetService<ILoggerFactory>();
            if (loggerFactory == null)
            {
                return Task.CompletedTask;
            }

            var logger = loggerFactory.CreateLogger(categoryName);
            logger.LogInformation(logMessage.ToExcludeException().ToString());
            if (logMessage.Exception != null)
            {
                logger.LogError(logMessage.Exception, logMessage.Exception.Message);
            }

            return Task.CompletedTask;
        }
    }
}
