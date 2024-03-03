using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PopugJira.Tracker.Db;
using PopugJira.Tracker.DiExt;
using PopugJira.Tracker.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TrackerDbContext>(options =>
{
    options.UseNpgsql(new NpgsqlConnection("Server=localhost;Port = 5432;Database=tracker;Persist Security Info=True;User ID=postgres;Password=Passw0rd!;"));
    options.UseOpenIddict();
});
builder.Services.AddAuth();
builder.Services.AddControllersWithViews();

builder.Services.AddKafka();

builder.Services.AddScoped<TaskAssigner>();
builder.Services.AddScoped<EventSender>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();
