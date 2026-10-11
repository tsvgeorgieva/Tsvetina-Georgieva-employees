using Employees.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IAssignmentParser, AssignmentCsvParser>();
builder.Services.AddSingleton<IPairFinder, PairFinder>();
builder.Services.AddScoped<IPairReportService, PairReportService>();

var app = builder.Build();
app.MapDefaultControllerRoute();
app.Run();
