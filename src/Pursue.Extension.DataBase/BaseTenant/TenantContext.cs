using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Pursue.Extension.DataBase
{
    public sealed class TenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId
        {
            get
            {
                var tenant = _httpContextAccessor?.HttpContext?.RequestServices?.GetRequiredService<ITenant>();
                if (tenant != default)
                {
                    return tenant.UserId;
                }
                else
                {
                    throw new NullReferenceException("未获取到用户编号!");
                }
            }
        }

        public string TenantId
        {
            get
            {
                var tenant = _httpContextAccessor?.HttpContext?.RequestServices?.GetRequiredService<ITenant>();
                if (tenant != default)
                {
                    return tenant.TenantId;
                }
                else
                {
                    throw new NullReferenceException("未获取到租户编号!");
                }
            }
        }

        public ITenant Tenant
        {
            get
            {
                var tenant = _httpContextAccessor?.HttpContext?.RequestServices?.GetRequiredService<ITenant>();
                if (tenant != default)
                {
                    return tenant;
                }
                else
                {
                    throw new NullReferenceException("未获取到租户信息!");
                }
            }
        }
    }
}
