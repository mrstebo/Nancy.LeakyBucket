namespace Nancy.LeakyBucket.Identifiers
{
    public class DefaultClientIdentifier : IClientIdentifier
    {
        public string UserAgentAddress { get; }

        public DefaultClientIdentifier(string userAgentAddress)
        {
            UserAgentAddress = userAgentAddress;
        }

        public override bool Equals(object obj)
        {
            return obj is DefaultClientIdentifier &&
                   ((DefaultClientIdentifier) obj).UserAgentAddress == UserAgentAddress;
        }

        public override int GetHashCode()
        {
            return UserAgentAddress?.GetHashCode() ?? 0;
        }
    }
}
