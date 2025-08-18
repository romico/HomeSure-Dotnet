using Microsoft.EntityFrameworkCore;
using RealEstate.Infrastructure.Data;
using RealEstate.Infrastructure.Extensions;
using RealEstate.Core.Extensions;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog 설정
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "RealEstate.API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/realestate-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}",
        fileSizeLimitBytes: 10_000_000,
        rollOnFileSizeLimit: true)
    .WriteTo.File(
        "logs/errors/realestate-errors-.log",
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 90,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// 서비스 추가
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "RealEstate Backend API", 
        Version = "v1",
        Description = "부동산 거래 플랫폼 API - 블록체인 기반 부동산 거래 시스템",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "RealEstate",
            Email = "romico@gmail.com"
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // API 그룹화
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
});

// 데이터베이스 설정
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("데이터베이스 연결 문자열이 설정되지 않았습니다.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
           .EnableDetailedErrors(builder.Environment.IsDevelopment()));

// CORS 설정
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT 인증 설정
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT SecretKey이 설정되지 않았습니다.");
}

var key = Encoding.ASCII.GetBytes(secretKey);
Log.Information("JWT 인증 설정 완료. 키 길이: {KeyLength}", key.Length);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// 커스텀 서비스 등록
builder.Services.AddCoreServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// 데이터베이스 초기화
await RealEstate.Infrastructure.Data.DbInitializer.InitializeAsync(app.Services);

// 시작 로그
Log.Information("=== RealEstate API Starting ===");
Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
Log.Information("ContentRoot: {ContentRoot}", app.Environment.ContentRootPath);

// HTTP 요청 로깅
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? Serilog.Events.LogEventLevel.Error
        : httpContext.Response.StatusCode > 499
            ? Serilog.Events.LogEventLevel.Error
            : Serilog.Events.LogEventLevel.Information;
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
        diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
    };
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 데이터베이스 마이그레이션 자동 실행
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Log.Information("데이터베이스 연결 확인 중...");
        context.Database.EnsureCreated();
        Log.Information("데이터베이스 초기화 완료");
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "데이터베이스 초기화 실패");
    throw;
}

Log.Information("=== RealEstate API Started Successfully ===");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "애플리케이션이 예기치 않게 종료되었습니다");
    throw;
}
finally
{
    Log.Information("=== RealEstate API Shutting Down ===");
    Log.CloseAndFlush();
}
