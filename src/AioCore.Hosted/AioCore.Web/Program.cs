using AioCore.Domain;
using AioCore.Mongo;
using AioCore.Services;
using AioCore.Shared;
using AioCore.Shared.Extensions;
using AioCore.Web.Helpers;
using AioCore.Web.Helpers.HangfireHelpers;
using AioCore.Web.Jobs.DanTriJob;
using AioCore.Web.Jobs.SyncCacheJob;
using AioCore.Web.Middlewares;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;
var environment = builder.Environment;
var appSettings = new AppSettings();

configuration.Bind(appSettings);

services.AddSingleton(appSettings);
services.AddRazorPages();
services.AddServerSideBlazor();
services.AddHttpClient();
services.AddMongoContext<AioCoreContext>(appSettings.MongoConfigs);
services.AddHangfireServer(appSettings.MongoConfigs);
services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
services.RegisterServices();
services.AddScoped<ICronJob, DanTriJob>();
services.AddScoped<ICronJob, SyncCacheJob>();

var app = builder.Build();


app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.UseLoading();
app.UseJobs(environment);
app.UseHangfire();
app.MigrateDatabase<AioCoreContext>((context, provider) =>
{
    var logger = provider.GetService<ILogger<AioCoreContextSeed>>();
    AioCoreContextSeed.SeedAsync(context, logger).Wait();
});

app.Run();