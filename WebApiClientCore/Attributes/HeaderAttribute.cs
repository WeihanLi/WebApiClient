﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WebApiClientCore.Attributes
{
    /// <summary>
    /// 表示Http请求Header的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    [DebuggerDisplay("{name} = {value}")]
    public class HeaderAttribute : ApiActionAttribute, IApiParameterAttribute
    {
        /// <summary>
        /// Header名称
        /// </summary>
        private readonly string name;

        /// <summary>
        /// Header值 
        /// </summary>
        private readonly string value;

        /// <summary>
        /// 获取是对cookie的Value进行Url utf-8编码
        /// 默认为true
        /// </summary>
        public bool EncodeCookie { get; set; }

        /// <summary>
        /// 将参数值设置到Header        
        /// </summary>
        /// <param name="name">header名称</param>
        [AttributeCtorUsage(AttributeTargets.Parameter)]
        public HeaderAttribute(HttpRequestHeader name)
            : this(RequestHeader.GetName(name), null)
        {
        }

        /// <summary>
        /// 将参数值设置到Header      
        /// </summary>
        /// <param name="name">header名称</param>
        /// <exception cref="ArgumentNullException"></exception>
        [AttributeCtorUsage(AttributeTargets.Parameter)]
        public HeaderAttribute(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// 将指定值设置到Header       
        /// </summary>
        /// <param name="name">header名称</param>
        /// <param name="value">header值</param>
        [AttributeCtorUsage(AttributeTargets.Interface | AttributeTargets.Method)]
        public HeaderAttribute(HttpRequestHeader name, string value)
            : this(RequestHeader.GetName(name), value)
        {
        }

        /// <summary>
        /// 将指定值设置到Header      
        /// </summary>
        /// <param name="name">header名称</param>
        /// <param name="value">header值</param>
        /// <exception cref="ArgumentNullException"></exception>
        [AttributeCtorUsage(AttributeTargets.Interface | AttributeTargets.Method)]
        public HeaderAttribute(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            this.name = name;
            this.value = value;
            this.EncodeCookie = true;
        }

        /// <summary>
        /// 执行前
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        public override Task OnRequestAsync(ApiRequestContext context)
        {
            if (string.IsNullOrEmpty(this.value) == false)
            {
                context.HttpContext.RequestMessage.Headers.TryAddWithoutValidation(this.name, this.value);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// http请求之前
        /// 值从参数过来
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="next"></param> 
        /// <returns></returns>
        public Task OnRequestAsync(ApiParameterContext context, Func<Task> next)
        {
            var headerValue = context.ParameterValue?.ToString();
            if (string.IsNullOrEmpty(headerValue) == false)
            {
                context.HttpContext.RequestMessage.Headers.TryAddWithoutValidation(this.name, headerValue);
            }
            return next();
        }
    }
}
