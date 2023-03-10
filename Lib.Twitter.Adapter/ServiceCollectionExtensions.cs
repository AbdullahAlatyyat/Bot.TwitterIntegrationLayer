using System;
using System.Collections.Generic;
using System.Text;
using Lib.Twitter.Adapter.Hosting;
using Lib.Twitter.Webhooks.Authentication;
using Lib.Twitter.Adapter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Lib.Twitter.Adapter
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTwitterAdapter(this IServiceCollection collection, Action<TwitterOptions> contextDelegate)
        {
            collection.AddSingleton<IHostedService, WebhookHostedService>();
            collection.AddSingleton<WebhookMiddleware>();
            collection.AddSingleton<TwitterAdapter>();

            collection.AddOptions();
            collection.Configure(contextDelegate);
        }
    }

    public static class ApplicationBuilderExtensions
    {
        public static void UseTwitterAdapter(this IApplicationBuilder app)
        {
            var twitterOptions = app.ApplicationServices.GetRequiredService<IOptions<TwitterOptions>>().Value;
            var uriPath = new Uri(twitterOptions.WebhookUri);

            app.UseWhen(
                context => context.Request.Path.StartsWithSegments(uriPath.AbsolutePath), 
                builder => builder.UseMiddleware<WebhookMiddleware>());
        }
    }
}
