
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// ---- JWT ----
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["Jwt:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
	};
});

builder.Services.AddAuthorization();
builder.Services.AddSingleton<JwtService>();
// ---- -------

// ------ DI-----
builder.Services.AddDbContext<ProjectPrn232Context>(option
	=> option.UseSqlServer(builder.Configuration
	.GetConnectionString("MyCnn")));

builder.Services.AddScoped(typeof(ProjectPrn232Context));

// Configure OData
var modelBuilder = new ODataConventionModelBuilder();
//modelBuilder.EntitySet<Account>("Accounts");
modelBuilder.EntitySet<User>("Users");
// modelBuilder.EntitySet<Job>("Jobs"); // ❌ REMOVED - Conflict với JobsController
modelBuilder.EntitySet<Application>("Applications");

// Thêm CheckinRecord với key explicitly
var checkinRecordEntity = modelBuilder.EntitySet<CheckinRecord>("CheckinRecords");
checkinRecordEntity.EntityType.HasKey(c => c.CheckinId);

// Add Controllers with OData (chỉ gọi 1 lần)
builder.Services.AddControllers()
	.AddOData(opt => opt
		.AddRouteComponents("api", modelBuilder.GetEdmModel())
		.Filter()
		.Select()
		.OrderBy()
		.SetMaxTop(100)
		.Count());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});
var app = builder.Build();

// ---- JWT ----
// 2. Kích hoạt middleware Authentication và Authorization
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
//

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.Run();