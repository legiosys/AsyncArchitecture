using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PopugJira.Accounting.Contracts;
using PopugJira.Accounting.Db;
using PopugJira.Accounting.Models;
using PopugJira.Accounting.Services;
using PopugJira.Auth.Contracts;

namespace PopugJira.Accounting.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class BillingDayController : ControllerBase
{
    [HttpPost]
    public async Task FinishDay([FromServices] AccountingDbContext dbContext, [FromServices] EventSender eventSender)
    {
        var popugs = await dbContext.Popugs.Where(x => x.Position.Equals(PopugPositionsEnum.Worker)).ToListAsync();
        var cycle = await dbContext.BillingCycles.FirstOrDefaultAsync(x => x.End == null);
        if (cycle == null)
        {
            cycle = new BillingCycle()
            {
                Start = DateTimeOffset.UtcNow
            };
            dbContext.BillingCycles.Add(cycle);
        }
        foreach (var popug in popugs.Where(x => x.Balance > 0))
        {
            var transaction = new Transaction()
            {
                PopugId = popug.Id,
                Credit = popug.Balance,
                Debit = 0,
                TransactionType = TransactionTypes.Payment,
                Date = DateTimeOffset.UtcNow,
                BillingCycle = cycle.Id
            };
            dbContext.Transactions.Add(transaction);
            
            //увы, батчей в этой библиотеке нет
            await eventSender.TransactionApplied(transaction, popug.ReplicationId);
            await SomePaymentLogic(eventSender, transaction, popug);
        }
        cycle.End = DateTimeOffset.UtcNow;
        var newCycle = new BillingCycle()
        {
            Start = DateTimeOffset.UtcNow
        };
        dbContext.BillingCycles.Add(newCycle);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SomePaymentLogic(EventSender eventSender, Transaction transaction, Popug popug)
    {
        popug.Balance = 0;
        await eventSender.PaymentCompleted(transaction, popug);
    }
}