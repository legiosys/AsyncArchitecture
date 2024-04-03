using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PopugJira.Analytics.Db;
using PopugJira.Analytics.Models;
using PopugJira.Analytics.ViewModels;

namespace PopugJira.Analytics.Controllers;

[Route("[controller]/[action]")]
[ApiController]
[Authorize("admin")]
public class DashboardController : ControllerBase
{
    [HttpGet]
    public async Task<MostExpensiveTasksView> MostExpensive([FromServices] AnalyticsDbContext db)
    {
        var lastMonthTasks = await db.PopugTasks.AsNoTracking()
            .Where(x => x.CompleteDate != null && x.CompleteDate.Value.Date >= DateTimeOffset.UtcNow.AddMonths(-1).Date)
            .OrderBy(x => x.CompleteDate)
            .ToListAsync();

        return new MostExpensiveTasksView(
            GetMostExpensive(lastMonthTasks.Where(x => x.CompleteDate?.Date == DateTimeOffset.UtcNow)),
            GetMostExpensive(lastMonthTasks.Where(x => x.CompleteDate?.Date >= DateTimeOffset.UtcNow.AddDays(7).Date)),
            GetMostExpensive(lastMonthTasks)
        );
        
        static MostExpensiveTask? GetMostExpensive(IEnumerable<PopugTask> tasks)
        {
            var task = tasks.MaxBy(x => x.CompletePrice);
            return task == null 
                ? null 
                : new MostExpensiveTask(task.CompletePrice, task.Description ?? "");
        }
    }

    [HttpGet]
    public async Task<int> TopsEarnings([FromServices] AnalyticsDbContext db)
    {
        var todayChanges = await db.TopsBalanceChanges.AsNoTracking()
            .Where(x => x.Date.Date == DateTimeOffset.UtcNow.Date)
            .ToListAsync();

        return todayChanges.Sum(change => change.Debit - change.Credit);
    }
    
    [HttpGet]
    public async Task<int> MinusPopugs([FromServices] AnalyticsDbContext db)
    {
        var popugIds = await db.PopugBalanceChanges.AsNoTracking()
            .Select(x => x.PopugId)
            .Distinct()
            .ToListAsync();
        
        var count = 0;
        foreach (var popugId in popugIds)
        {
            var lastZero = await db.PopugBalanceChanges.AsNoTracking()
                .Where(x => x.PopugId == popugId)
                .OrderByDescending(x => x.Date)
                .FirstOrDefaultAsync(x => x.ZeroBalance);
            
            var todayChanges = await db.PopugBalanceChanges.AsNoTracking()
                .Where(x => x.PopugId == popugId)
                .Where(x => lastZero == null || x.Date >= lastZero.Date)
                .ToListAsync();
            
            var result = todayChanges.Sum(change => change.Debit - change.Credit);

            if (result < 0)
                count++;
        }

        return count;
    }
}