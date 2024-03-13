using Microsoft.EntityFrameworkCore;
using Npgsql;
using PopugJira.Accounting.Db;
using PopugJira.Accounting.DiExt;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AccountingDbContext>(options =>
{
    options.UseNpgsql(new NpgsqlConnection("Server=localhost;Port = 5432;Database=accounting;Persist Security Info=True;User ID=postgres;Password=Passw0rd!;"));
    options.UseOpenIddict();
});
builder.Services.AddAuth();
builder.Services.AddKafka();
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();