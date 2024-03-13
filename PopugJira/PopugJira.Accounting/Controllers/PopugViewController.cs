using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using PopugJira.Accounting.Contracts;
using PopugJira.Accounting.Db;
using PopugJira.Accounting.Models;
using PopugJira.Accounting.ViewModels;

namespace PopugJira.Accounting.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class PopugViewController : ControllerBase
{
    [HttpGet]
    [Authorize("worker")]
    public async Task<PopugBalanceViewModel> GetWorkerInfo([FromServices] AccountingDbContext db)
    {
        var popugId = Guid.Parse(HttpContext.User.GetClaim(OpenIddictConstants.Claims.Subject) ?? string.Empty);
        var popug = await db.Popugs.FirstOrDefaultAsync(x => x.ReplicationId == popugId);
        if (popug == null)
            return new PopugBalanceViewModel(0, new List<LogRecord>());

        var lastCycles = await db.BillingCycles.AsNoTracking()
            .OrderByDescending(x => x.Start)
            .Take(7)
            .Select(x => x.Id)
            .ToListAsync();
        var transactions = await db.Transactions.AsNoTracking()
            .Where(x => lastCycles.Contains(x.BillingCycle))
            .ToListAsync();
        var taskIds = transactions.Where(x => x.TaskId != null).Select(x => x.TaskId);
        var tasks = await db.Tasks.AsNoTracking()
            .Where(x => taskIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, v => v.Description);


        return new PopugBalanceViewModel(popug.Balance, transactions.Select(x =>
            new LogRecord(x.Date, x.Debit - x.Credit,
                x.TaskId != null
                    ? tasks[x.TaskId!.Value] ?? ""
                    : "Выплата заработка"
            )).ToList());
    }

    [HttpGet]
    [Authorize("accountant")]
    public async Task<TopPopugsViewModel> GetTopsEarnings([FromServices] AccountingDbContext db)
    {
        var lastCycles = await db.BillingCycles.AsNoTracking()
            .OrderByDescending(x => x.Start)
            .Take(7)
            .ToListAsync();
        
        var transactions = await db.Transactions.AsNoTracking()
            .Where(x => x.TransactionType != TransactionTypes.Payment)
            .Where(x => lastCycles.Select(c => c.Id).Contains(x.BillingCycle))
            .ToListAsync();

        var cycleTrans = transactions.GroupBy(x => x.BillingCycle)
            .ToDictionary(x => x.Key);
        var currentCycle = lastCycles.First(x => x.End == null);

        return new TopPopugsViewModel(
            GetEarning(cycleTrans[currentCycle.Id]), lastCycles
                .Where(x => x != currentCycle)
                .Select(x => new PreviousEarn(GetEarning(cycleTrans[x.Id]), x.Start, x.End!.Value))
                .ToList()
        );

        int GetEarning(IEnumerable<Transaction> cycleTransactions)
        {
            var result = 0;
            foreach (var trans in cycleTransactions)
            {
                if (trans.TransactionType == TransactionTypes.Assign)
                    result += trans.Credit;
                if (trans.TransactionType == TransactionTypes.Complete)
                    result -= trans.Debit;
            }

            return result;
        }
    }
}