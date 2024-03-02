using Microsoft.AspNetCore.Identity;
using PopugJira.Auth.DiExt;
using PopugJira.Auth.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.ResolveConflictingActions(x => x.First()));
builder.Services.AddAuth();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

//app.MapIdentityApi<Popug>();
app.MapControllers();
app.MapDefaultControllerRoute();
app.MapRazorPages();
/*app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapDefaultControllerRoute();
    endpoints.MapRazorPages();
});*/


await using (var scope = app.Services.CreateAsyncScope())
{
    await InitAuthExtensions.InitRoles(scope.ServiceProvider);
    await InitAuthExtensions.InitOAuthApps(scope.ServiceProvider);
    Console.WriteLine("PopugAuth initialized");
}
app.Run();
