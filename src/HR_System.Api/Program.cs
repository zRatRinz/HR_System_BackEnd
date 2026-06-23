using HR_System.Api.Api.Common;
using HR_System.Api.Middleware;
using HR_System.Api.Services;
using HR_System.Application.Interfaces;
using HR_System.Application.UseCases.Approval;
using HR_System.Application.UseCases.Attendance;
using HR_System.Application.UseCases.Auth;
using HR_System.Application.UseCases.Dashboard;
using HR_System.Application.UseCases.Department;
using HR_System.Application.UseCases.Division;
using HR_System.Application.UseCases.Employee;
using HR_System.Application.UseCases.Leave;
using HR_System.Application.UseCases.Payroll;
using HR_System.Application.UseCases.Position;
using HR_System.Application.UseCases.Reports;
using HR_System.Application.UseCases.Role;
using HR_System.Application.UseCases.Settings;
using HR_System.Domain.Entities;
using HR_System.Infrastructure.Repositories;
using HR_System.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties;
});
//builder.Services.AddOpenApi(options =>
//{
//    options.AddSchemaTransformer((schema, context, cancellationToken) =>
//    {
//        if (schema.Format == "int32" || schema.Format == "int64")
//        {
//            schema.Type = JsonSchemaType.Integer;
//            schema.Pattern = null;
//        }
//        return Task.CompletedTask;
//    });
//});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        return new BadRequestObjectResult(ApiResponse.Fail("Validation error"));
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "PulsePointHR",
            ValidAudience = "PulsePointHR",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("PulsePoint-HR-Secret-Key-2024-This-Is-A-Very-Long-Secret-Key-For-JWT-Token"))
        };
    });

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IPayrollRepository, PayrollRepository>();
builder.Services.AddScoped<IApprovalRepository, ApprovalRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IDivisionRepository, DivisionRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IScopeService, ScopeService>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IPermissionService, PermissionService>();

builder.Services.AddScoped<AuthUseCase>();
builder.Services.AddScoped<EmployeeUseCase>();
builder.Services.AddScoped<LeaveUseCase>();
builder.Services.AddScoped<LeaveBalanceUseCase>();
builder.Services.AddScoped<LeaveCalendarUseCase>();
builder.Services.AddScoped<AttendanceUseCase>();
builder.Services.AddScoped<AttendanceTodayStatsUseCase>();
builder.Services.AddScoped<PayrollUseCase>();
builder.Services.AddScoped<ApprovalUseCase>();
builder.Services.AddScoped<PositionUseCase>();
builder.Services.AddScoped<DashboardUseCase>();
builder.Services.AddScoped<ReportUseCase>();
builder.Services.AddScoped<SettingsUseCase>();
builder.Services.AddScoped<RoleUseCase>();
builder.Services.AddScoped<DivisionUseCase>();
builder.Services.AddScoped<DepartmentUseCase>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("PulsePoint HR API");
    });
}

app.UseErrorHandling();

app.UseCors("AllowFrontend");

app.UseHttpLogging();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/scalar", permanent: false));

app.Run();
