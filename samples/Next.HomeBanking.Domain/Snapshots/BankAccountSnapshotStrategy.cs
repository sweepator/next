using System;
using Next.Abstractions.EventSourcing.Snapshot;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Domain.Snapshots
{
    public class BankAccountSnapshotStrategy : SnapshotStrategy<BankAccountAggregate, BankAccountId, BankAccountState>
    {
        public override ISnapshot<BankAccountAggregate, BankAccountId, BankAccountState> Create(BankAccountAggregate aggregateRoot)
        {
            if (aggregateRoot.State.Version % 10 == 0)
            {
                return new Snapshot<BankAccountAggregate, BankAccountId, BankAccountState>(
                    aggregateRoot.Id,
                    aggregateRoot.State,
                    DateTime.UtcNow);
            }

            return null;
        }
    }
}