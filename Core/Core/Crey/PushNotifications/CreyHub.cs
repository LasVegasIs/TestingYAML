using Crey.Contracts;
using Crey.Kernel.Authentication;
using Crey.MessageContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crey.PushNotifications
{
    public interface ICreySignal<T>
    where T : IPushNotification
    {
        Task OnConnectedAsync(SessionInfo sessionInfo);
        Task OnDisconnectedAsync(Exception exception);
        Task SendToAllAsync(params T[] messages);
        Task SendToUser(int accountId, params T[] messages);
    }


    [Authorize]
    class CreyHub<T> : Hub
        where T : IPushNotification
    {
        private readonly IServiceProvider services_;

        public CreyHub(IServiceProvider services)
        {
            services_ = services;
        }

        public override async Task OnConnectedAsync()
        {
            SessionInfo sessionInfo = Context.User.IntoSessionInfo();
            services_.GetService<ILogger<CreyHub<T>>>().LogInformation($"Client connected to signalR hub, userid: {sessionInfo.AccountId}, user identifier: {Context.UserIdentifier}");

            foreach (var ctx in services_.GetServices<ICreySignal<T>>())
            {
                await ctx.OnConnectedAsync(sessionInfo);
            }
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            SessionInfo sessionInfo = Context.User.IntoSessionInfo();
            services_.GetService<ILogger<CreyHub<T>>>().LogInformation($"Client disconnected from signalR hub, userid: {sessionInfo.AccountId}, user identifier: {Context.UserIdentifier}");

            foreach (var ctx in services_.GetServices<ICreySignal<T>>())
            {
                await ctx.OnDisconnectedAsync(exception);
            }
        }
    }

    class CreySignal<T> : ICreySignal<T>
      where T : IPushNotification
    {
        protected readonly IHubContext<CreyHub<T>> hubContext_;

        public CreySignal(IHubContext<CreyHub<T>> hubContext)
        {
            hubContext_ = hubContext;
        }

        public virtual Task OnConnectedAsync(SessionInfo sessionInfo)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnDisconnectedAsync(Exception exception)
        {
            return Task.CompletedTask;
        }

        public virtual Task SendToAllAsync(params T[] messages)
        {
            return SendManyToClientProxy(hubContext_.Clients.All, messages);
        }

        public virtual Task SendToUser(int accountId, params T[] messages)
        {
            return SendManyToClientProxy(hubContext_.Clients.User(accountId.ToString()), messages);
        }

        private async Task SendManyToClientProxy(IClientProxy clientProxy, T[] messages)
        {
            var map = new Dictionary<string, List<T>>();

            foreach (T m in messages)
            {
                if (!map.ContainsKey(m.Category))
                {
                    map[m.Category] = new List<T>();
                }
                map[m.Category].Add(m);
            }

            foreach (var m in map)
            {
                await clientProxy.SendAsync(m.Key, m.Value);
            }
        }
    }

    public class AccountIdAsUserIdProvider : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(CreyClaimTypes.AccountId)?.Value;
        }
    }

    public static class SignalRExtensions
    {
        public static IEndpointRouteBuilder MapCreyHub<T>(this IEndpointRouteBuilder routes, string path)
            where T : IPushNotification
        {
            routes.MapHub<CreyHub<T>>(path);
            return routes;
        }

        public static IServiceCollection AddCreySignalR(this IServiceCollection services, string azureConnectionString, bool isProduction)
        {
            services.AddSingleton<IUserIdProvider, AccountIdAsUserIdProvider>();

            services.AddSignalR(
                hub =>
                {
                    hub.EnableDetailedErrors = !isProduction;
                })
                .AddAzureSignalR(options =>
                {
                    options.ConnectionString = azureConnectionString;
                    options.ConnectionCount = isProduction ? 5 : 2;
                })
                .AddNewtonsoftJsonProtocol(); // using old Json stack until new will get generics        
            return services;
        }

        public static IServiceCollection AddCreySignal<T>(this IServiceCollection services)
            where T : IPushNotification
        {
            services.AddScoped(typeof(ICreySignal<T>), typeof(CreySignal<T>));
            return services;
        }
    }
}
