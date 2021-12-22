using System;

namespace Next.Data.EntityFramework.Model
{
    public interface ITimestampedEntity
    {
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
