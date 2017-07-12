namespace Nancy.LeakyBucket.Identifiers
{
    public class DefaultClientIdentifier : IClientIdentifier
    {
        public string UserAgentAddress { get; set; }
    }
}
