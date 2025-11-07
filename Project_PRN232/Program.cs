using Project_PRN232.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Thêm HttpClient và AuthService
builder.Services.AddHttpClient<AuthService>();
builder.Services.AddScoped<AuthService>();

// Thêm JobService
builder.Services.AddHttpClient<JobService>();
builder.Services.AddScoped<JobService>();

// Thêm ApplicationService
builder.Services.AddHttpClient<ApplicationService>();
builder.Services.AddScoped<ApplicationService>();

// Thêm CheckinService
builder.Services.AddHttpClient<CheckinService>();
builder.Services.AddScoped<CheckinService>();

// Thêm IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Thêm Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Thêm Session middleware
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
