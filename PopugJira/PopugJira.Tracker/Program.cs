using Microsoft.EntityFrameworkCore;
using PopugJira.Tracker.Db;
using PopugJira.Tracker.DiExt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TrackerDbContext>(options =>
{
    options.UseInMemoryDatabase("tracker");
    options.UseOpenIddict();
});
builder.Services.AddAuth();
builder.Services.AddControllersWithViews();

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
