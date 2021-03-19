using Core.Crey.Configuration;
using Core.Extension.CreyNamePatterns;
using Crey.Configuration.ConfigurationExtensions;
using Crey.Contracts;
using Crey.Exceptions;
using Crey.FeatureControl;
using Crey.Kernel;
using Crey.Kernel.Authentication;
using Crey.Kernel.IAM;
using Crey.Kernel.ServiceDiscovery;
using Crey.MessageStream.ServiceBus;
using Crey.MigrationTool;
using Crey.Moderation;
using Crey.Web;
using Crey.Web.Analytics;
using IAM.Areas.Authentication;
using IAM.Areas.Authentication.ServiceBusMessages;
using IAM.Areas.FeatureGates;
using IAM.Data;
using Mandrill;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Prometheus;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace IAM
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }
        public ServiceOption ServiceOption { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ServiceOption = new ServiceOption(Configuration, IAMDefaults.SERVICE_NAME);
            services.AddSingleton(Configuration);
            ServiceOption.AddServiceOptionAccessor(services);

            services.AddSingletonCreyService<IProvidedService, ProvidedService>();
            services.AddCreyApplicationInsights(Configuration);

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });
            services.AddMemoryCache();

            // while we have no connection string let's use the auth. 
            // to be removed after auth/iam db split.
            string dbConnectionString;
            var isDBMigrated = Configuration.GetValue<bool>("IsDBMigrated", false);
            if (isDBMigrated)
            {
                dbConnectionString = ServiceOption.GetSqlCns();
            }
            else
            {
                dbConnectionString = GetAuthSqlConnectionStringAsync().Result;
            }

            services.AddInstrumentedDbContext<ApplicationDbContext>(ServiceOption, options =>
                options.UseSqlServer(dbConnectionString, builder => { builder.EnableRetryOnFailure(); }));
            services
                .AddDefaultIdentity<ApplicationUser>(options =>
                {
                    // Password settings
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 6; // $$$ get value from config
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredUniqueChars = 0;

                    // Lockout settings
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                    options.Lockout.MaxFailedAccessAttempts = 10;
                    options.Lockout.AllowedForNewUsers = true;

                    // User settings
                    options.User.AllowedUserNameCharacters += " ";
                    options.User.RequireUniqueEmail = true;

                    // SignIn settings
                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                })
                .AddSignInManager<CreySignInManager>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddRazorPages().AddMvcOptions(options =>
            {
                options.Filters.Add(typeof(CreyTrackingIdFilter));
                options.MaxModelValidationErrors = 50;
                options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
                    _ => "Required field");
            });

            services.AddScoped<CreyTrackingIdFilter>();

            services.AddIDInfoAccessor();
            services.AddCreyRestClientFactory();
            services.AddServiceBusTopicBrokerAsync<IAccountServiceBusMessage>(Configuration).Wait();
            if (!Configuration.GetValue<bool>("CodeFirstServiceBus", false))
            {
                services.AddSingleton(new AccountMessageFactory());
            }
            services.AddServiceBusTopicSubscriberAsync<AccountMessageFactory, IAccountServiceBusMessage>(Configuration, IAMDefaults.SERVICE_NAME).Wait();
            services.AddFeatureGates();
            services.AddRateLimiting(Configuration);
            services.TryAddScoped<AnalyticsClient>();

            services.AddScopedCreyServiceInternal<RegistrationHandler>();
            services.AddScoped<AfterRegistrationHandler>();
            services.AddScopedCreyServiceInternal<SessionRepository>();
            services.AddScopedCreyServiceInternal<AccountRepository>();
            services.AddScopedCreyServiceInternal<SessionTokenRepository>();
            services.AddScopedCreyServiceInternal<SingleAccessKeyRepository>();
            services.AddScopedCreyServiceInternal<PersistentTokenRepository>();
            services.AddScopedCreyServiceInternal<EmailSender>();
            services.AddScopedCreyServiceInternal<OAuthRepository>();
            services.AddScoped<SignatureFlowService>();

            services.AddReCaptcha();
            services.AddCreyModeratorClient();
            services.TryAddSingleton(serviceProvider => new MandrillApi(Configuration.GetValue<string>("MandrillCns")));

            services.AddKenticoClient();

            services.AddScopedCreyServiceInternal<GeoLocationQuery>();
            services.AddScopedCreyServiceInternal<FeatureManagerRepository>();

            services.AddCreySwagger("iam", Configuration.GetDeploymentSlot());

            services.ConfigureApplicationCookie(options =>
            {
                options.ForwardDefaultSelector = ctx => SessionCookieAuthenticationDefaults.AuthenticationScheme;
            });

            AddDataProtectionAsync(services, Configuration).Wait();

            services
                .AddCreyClientAuthenticationAndAuthorization(Configuration, new SessionCookieOptions(
                    new IdentityToSessionCookieAuthenticationEvents(),
                    IdentityConstants.ApplicationScheme,
                    "/Identity/Account/Login",
                    "/Identity/Account/Logout"
                ))
                .AddFacebook(facebookOptions =>
                {
                    facebookOptions.AppId = Configuration.GetValue<string>("Facebook:AppId");
                    facebookOptions.AppSecret = Configuration.GetValue<string>("Facebook:AppSecret");
                    facebookOptions.Events.OnRemoteFailure = HandleRemoteFailure;
                })
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = Configuration.GetValue<string>("Google:ClientId");
                    googleOptions.ClientSecret = Configuration.GetValue<string>("Google:ClientSecret");
                    googleOptions.Events.OnRemoteFailure = HandleRemoteFailure;
                });
            services.EnableCreyCors(Configuration);
        }

        private Task HandleRemoteFailure(RemoteFailureContext context)
        {
            context.Response.Redirect("/Identity/Account/Login");
            context.HandleResponse();
            return Task.CompletedTask;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCreySwagger("iam", Configuration.GetChangeset());
            app.UseForwardedHeaders();

            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            app.UseHttpMetrics();
            app.UseHealthChecks("/info/ready");
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseStaticFiles();
            app.UseSession();
            app.UseCreyLogging();
            app.UseService2ServiceValidation();

            app.UseCors();
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Content-Security-Policy", "frame-ancestors 'none'; report-uri /iam/cspreport");
                await next();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMetrics();
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });

            app.MigrateSqlDatabase<ApplicationDbContext>();
            app.RegisterMessageHandler<AccountMessageFactory, IAccountServiceBusMessage>().Wait();
            RegisterToGateway.RegisterAsync(
                env.IsDevelopment(),
                Configuration,
                ServiceOption,
                app.ApplicationServices.GetRequiredService<IHttpClientFactory>()).Wait();
        }

        private async Task AddDataProtectionAsync(IServiceCollection services, IConfiguration configuration)
        {
            var storageAccount = CloudStorageAccount.Parse(configuration.GetValue<string>("StorageAccountCns"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("keys");
            await container.CreateIfNotExistsAsync();

            services
                .AddDataProtection()
                    .SetApplicationName($"crey-iam-{configuration.GetDeploymentSlot()}")
                    .PersistKeysToAzureBlobStorage(container, "dpapi.xml");
        }

        private async Task<string> GetAuthSqlConnectionStringAsync()
        {
            string authConnectionString = Configuration.GetValue<string>("AuthSqlCns");
            ServiceInfo authServiceInfo = await GetAuthServiceInfoAsync();
            return authConnectionString.SubstituteCreyStagePattern(authServiceInfo.Stage, "", "");
        }

        private async Task<ServiceInfo> GetAuthServiceInfoAsync()
        {
            using (var httpClient = new HttpClient())
            {
                string requestUri = $"{Configuration.GetBaseURI(AuthenticationDefaults.SERVICE_NAME)}/info/detail";
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
                {
                    HttpResponseMessage httpResponse = await httpClient.SendAsync(requestMessage);
                    if (httpResponse.StatusCode != HttpStatusCode.OK)
                    {
                        throw new CommunicationErrorException($"{requestUri} returned error code: {httpResponse.StatusCode} and result {await httpResponse.Content.ReadAsStringAsync()}");
                    }

                    return await httpResponse.Content.ReadAsAsync<ServiceInfo>();
                }
            }
        }
    }
}
