using AioCore.Domain;
using AioCore.Mongo;
using AioCore.Shared;
using AioCore.Web.Middlewares;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;
var appSettings = new AppSettings();

configuration.Bind(appSettings);

services.AddSingleton(appSettings);
services.AddRazorPages();
services.AddServerSideBlazor();
services.AddMongoContext<AioCoreContext>(appSettings.MongoConfigs);

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.UseLoading();

app.Run();