using Yet.Another.Shopping.Cart.Core.Domain.Statistics;
using Yet.Another.Shopping.Cart.Infrastructure.Services.Statistics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Yet.Another.Shopping.Cart.Web.Middleware
{
    public class VisitorCounterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public VisitorCounterMiddleware( RequestDelegate next,ILoggerFactory logger)
        {
            _next = next;
            _logger = logger.CreateLogger<VisitorCounterMiddleware>();
        }

        public Task Invoke(HttpContext context, IVisitorCountService _visitorCounterService)
        {
            if (context.Session.GetString("visitor_counter") == null || context.Session.GetString("visitor_counter") != "recorder")
            {
                context.Session.SetString("visitor_counter", "recorder");
                var visitorCountEntity = _visitorCounterService.GetVisitorCountByDate(DateTime.Now);
                if (visitorCountEntity != null)
                {
                    _visitorCounterService.UpdateVisitorCount(visitorCountEntity);
                }
                else
                {
                    var visitorModel = new VisitorCount
                    {
                        Date = DateTime.Now,
                        ViewCount = 1
                    };
                    _visitorCounterService.InsertVisitorCount(visitorModel);
                }
            }

            return _next(context);
        }
    }

    public static class VisitorCounterMiddlewareExtentions
    {
        public static IApplicationBuilder UseVisitorCounter(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<VisitorCounterMiddleware>();
        }
    }
}
