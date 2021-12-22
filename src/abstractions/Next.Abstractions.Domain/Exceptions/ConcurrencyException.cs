using System;

namespace Next.Abstractions.Domain.Exceptions
{
    public class ConcurrencyException : Exception
    {
        public object AggregateId { get; }

        public long ExpectedVersion { get; }

        public long CurrentVersion { get; }

        public ConcurrencyException(
            object aggregateId,
            long expectedVersion,
            long currentVersion)
        {
            AggregateId = aggregateId;
            ExpectedVersion = expectedVersion;
            CurrentVersion = currentVersion;
        }
    }
}
