namespace Nancy.LeakyBucket.Identifiers
{
    public class DefaultClientIdentifier : IClientIdentifier
    {
        public string UserAgentAddressAddress { get; }

        public DefaultClientIdentifier(string userAgentAddress)
        {
            UserAgentAddressAddress = userAgentAddress;
        }

        public override bool Equals(object obj)
        {
            return obj is DefaultClientIdentifier &&
                   ((DefaultClientIdentifier) obj).UserAgentAddressAddress == UserAgentAddressAddress;
        }

        public override int GetHashCode()
        {
            return UserAgentAddressAddress?.GetHashCode() ?? 0;
        }
    }
}
