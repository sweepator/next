namespace Next.Abstractions.Domain
{
    public sealed class Id : Identity<Id>
    {
        public Id(string value) 
            : base(value)
        {
        }
    }
}