using Project_PRN232.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<AuthService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddHttpClient<AdminService>();
builder.Services.AddScoped<AdminService>();

builder.Services.AddHttpClient<ProviderService>();
builder.Services.AddScoped<ProviderService>();

builder.Services.AddHttpClient<AttendanceService>();
builder.Services.AddScoped<AttendanceService>();

builder.Services.AddHttpClient<JobService>();
builder.Services.AddScoped<JobService>();

builder.Services.AddHttpClient<ApplicationService>();
builder.Services.AddScoped<ApplicationService>();

builder.Services.AddHttpClient<CheckinService>();
builder.Services.AddScoped<CheckinService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");

	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
