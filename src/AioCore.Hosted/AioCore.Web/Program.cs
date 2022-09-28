using AioCore.Domain;
using AioCore.Mongo;
using AioCore.Shared;
using AioCore.Web.Helpers;
using AioCore.Web.Helpers.HangfireHelpers;
using AioCore.Web.Jobs.DanTriJob;
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
services.AddScoped<ICronJob, DanTriJob>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.UseLoading();
app.UseJobs(environment);
app.UseHangfire();

app.Run();