namespace Next.Web.Health
{
    public static class HealthTags
    {
        public const string InfoTag = "info";
        public const string ReadyTag = "ready";
        public const string LiveTag = "live";

        public static readonly string[] InfoTags = new string[] { InfoTag };
        public static readonly string[] ReadyTags = new string[] { ReadyTag };
        public static readonly string[] LiveTags = new string[] { LiveTag };
    }
}