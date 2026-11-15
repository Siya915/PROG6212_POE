using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => { options.Cookie.Name=".RaceDay.Mvc.Session"; options.Cookie.HttpOnly=true; options.Cookie.IsEssential=true; options.IdleTimeout=TimeSpan.FromMinutes(30); });
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<ApiClient>(client => { client.BaseAddress=new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7041/"); client.Timeout=TimeSpan.FromSeconds(30); }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies=false });
var app=builder.Build();
if(!app.Environment.IsDevelopment()){app.UseExceptionHandler("/Home/Error");app.UseHsts();}
if (!app.Environment.IsDevelopment() && !app.Configuration.GetValue<bool>("Docker:DisableHttpsRedirection")) app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.UseSession(); app.UseAuthorization();
app.MapControllerRoute(name:"default",pattern:"{controller=Home}/{action=Index}/{id?}");
app.Run();
