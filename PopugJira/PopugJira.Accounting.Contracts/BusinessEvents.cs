namespace PopugJira.Accounting.Contracts;

public record FinTransaction_Applied_V1(Guid TransactionId, string Type, int Credit, int Debit, Guid PopugId, Guid? TaskId);

public record Payment_Completed_V1(Guid TransactionId, int Payment, Guid PopugId);