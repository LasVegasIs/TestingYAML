using Crey.Contracts;
using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using Crey.MessageStream;
using Crey.MessageStream.ServiceBus;
using Crey.Misc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Crey.Web.Controllers
{
    [EnableCors]
    public class HealthCheckController : Controller
    {
        private readonly ICreyService<ProvidedService> providedService_;

        public HealthCheckController(ICreyService<ProvidedService> providedService)
        {
            providedService_ = providedService;
        }

        [HttpGet("/api/v1/info")]
        [HttpGet("/info")]
        public CreyActionResult<string, Error> GetName()
        {
            return providedService_.Value.GetInfo().Map(ok => ok.Name);
        }

        [HttpGet("/api/v1/info/datetime")]
        public string GetDate()
        {
            return DateTime.UtcNow.ToString();
        }

        [HttpHead("/api/v1/info/detail")]
        [HttpHead("/info/detail")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [HttpGet("/api/v1/info/detail")]
        [HttpGet("/info/detail")]
        public CreyActionResult<ServiceInfo, Error> GetInfo()
        {
            return providedService_.Value.GetInfo();
        }

        [HttpGet("/api/v1/info/services/{service}")]
        [HttpGet("/info/services/{service}")]
        public CreyActionResult<string, Error> GetService(string service)
        {
            return providedService_.Value.GetServiceURI(service);
        }

        [HttpPost("/api/v1/gc")]
        [AuthenticateStrict]
        [Authorize(Policy = CreyAuthorizationDefaults.CreyUser, Roles = UserRoles.InternalUser)]
        public ActionResult<String> PerformGC()
        {
            var pre = GC.GetTotalMemory(true);
            GC.Collect(2, System.GCCollectionMode.Forced, true, true);
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var post = GC.GetTotalMemory(true);
            return $"GC; allocation changed from {pre} to {post} (bytes)";
        }

        public class EPInfo
        {
            public string Controler { get; set; }
            public string ImplementingMethod { get; set; }
            public List<string> Routes { get; set; }
            public List<string> Obsolete { get; set; }
        }

        [HttpGet("/api/v1/info/ep/deprecated")]
        [AuthenticateStrict]
        [Authorize(Policy = CreyAuthorizationDefaults.CreyUser, Roles = UserRoles.InternalUser)]
        public ActionResult<List<EPInfo>> GetDeprecatedEP(bool all, string scope)
        {
            var eps = new List<EPInfo>();
            var assembly = Assembly.GetEntryAssembly();
            var controllers = assembly
                .GetTypes()
                .Where(x => typeof(ControllerBase).IsAssignableFrom(x) && (string.IsNullOrEmpty(scope) || x.Name == scope));

            foreach (var controller in controllers)
            {
                var filteredMethod = controller
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Where(x => (all || x.GetCustomAttributes<ObsoleteAttribute>().Any()) && x.GetCustomAttributes<HttpMethodAttribute>().Any());

                var deprecatedEp = filteredMethod
                    .Select(method =>
                    {
                        var routes = GetRoutes(controller, method);
                        if (!routes.Any())
                            return null;
                        return new EPInfo
                        {
                            Controler = controller.Name,
                            ImplementingMethod = method.Name,
                            Obsolete = method.GetCustomAttributes<ObsoleteAttribute>().Select(x => x.Message).Where(x => x != null).ToList(),
                            Routes = routes,
                        };
                    })
                    .Where(x => x != null);

                eps.AddRange(deprecatedEp);
            }

            return eps;
        }

        private static List<string> GetRoutes(Type controller, MethodInfo method)
        {
            return method.GetCustomAttributes<HttpMethodAttribute>()
                    .Select(m =>
                    {
                        return m.HttpMethods.Select(h => $"[{h}] {m.Template}");
                    })
                    .SelectMany(x => x)
                    .ToList();
        }

        public class SBInfo
        {
            public string ClassType { get; set; }
            public string MessageType { get; set; }
            public List<string> Obsolete { get; set; }
        }

        [HttpGet("/api/v1/info/sb/deprecated")]
        [AuthenticateStrict]
        [Authorize(Policy = CreyAuthorizationDefaults.CreyUser, Roles = UserRoles.InternalUser)]
        public ActionResult<List<SBInfo>> GetDeprecatedSBSend([FromQuery] bool all)
        {
            var assembly = Assembly.GetEntryAssembly();
            var obsoleteSent = assembly
                .GetTypes()
                .Where(x =>
                    typeof(ISentServiceBusMessage).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface
                    || typeof(IReceivedServiceBusMessage).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface)
                .Where(x => all || x.GetCustomAttributes<ObsoleteAttribute>().Any())
                .Select(x => new SBInfo
                {
                    ClassType = x.Name,
                    MessageType = TryGetSBType(x),
                    Obsolete = x.GetCustomAttributes<ObsoleteAttribute>().Select(x => x.Message).Where(x => x != null).ToList(),
                })
                .ToList();

            return obsoleteSent;
        }

        private string TryGetSBType(Type type)
        {
            try
            {
                var instance = (IStreamedMessage)Activator.CreateInstance(type);
                return instance.Type;
            }
            catch
            {
                return null;
            }
        }
    }

}