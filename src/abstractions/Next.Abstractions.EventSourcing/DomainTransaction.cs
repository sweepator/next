using System;
using System.Threading;

namespace Next.Abstractions.EventSourcing
{
    public class DomainTransaction : IDisposable
    {
        private static readonly AsyncLocal<DomainTransaction> DomainTransactionAccessor = new();

        public Guid Id { get; }
        
        public static DomainTransaction Current => DomainTransactionAccessor.Value;

        public DomainTransaction(Guid? id = null)
        {
            if (DomainTransactionAccessor.Value != null) return;
            
            Id = id ?? Guid.NewGuid();
            DomainTransactionAccessor.Value = this;
        }

        public void Dispose()
        {
            if (DomainTransactionAccessor.Value == this)
            {
                DomainTransactionAccessor.Value = null;
            }
        }
    }
}
