namespace Next.HomeBanking.Domain.SharedKernel
{
    public enum TransactionType
    {
        Credit,
        Debit
    }
    
    public enum TransactionState
    {
        Pending,
        Cancelled,
        Confirmed
    }
}