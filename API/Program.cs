using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ====== 1. JWT AUTH ====== dang ki vao DI
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

// ====== 2. Authorization ====== dki vao DI
builder.Services.AddAuthorization(); 

// ====== 3. Services ======
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// ====== 4. DbContext ======
builder.Services.AddDbContext<ProjectPrn232Context>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// ====== 5. OData Model ======
var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<User>("Users");
modelBuilder.EntitySet<Application>("Applications");
modelBuilder.EntitySet<CheckinRecord>("CheckinRecords");

// ====== 6. OData ======
builder.Services.AddControllers()
	.AddOData(opt => opt
		.AddRouteComponents("api", modelBuilder.GetEdmModel())
		.Filter()
		.Select()
		.OrderBy()
		.SetMaxTop(100)
		.Count()
	)
	.AddJsonOptions(o =>
	{
		o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
		o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
	});

// ====== 7. Swagger ======
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ====== 8. CORS ====== CHO PHEP DOMAIN (CLIENT) NAO CALL API O SERVER
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin() // ALLOW ALL DOMAIN (PORT KHAC NHAU) CALL API
			  .AllowAnyMethod() // ALLOW ALL GET/POST/PUT/UPDATE/...
			  .AllowAnyHeader(); // ALLOW ALL HEADER (Authorization, Content-Type…)
	});
});

var app = builder.Build();

// ====== 9. Middleware pipeline ======
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors("AllowAll"); // kich hoat

app.UseAuthentication(); // kICH HOAT AUTHEN VA AUTHOR DA DANG KI O TREN
app.UseAuthorization();

app.MapControllers();

app.Run();
