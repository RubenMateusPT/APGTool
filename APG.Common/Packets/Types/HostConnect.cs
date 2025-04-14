namespace APG.Common.Packets.Types
{
    public class HostConnect
    {
        public bool Success { get; set; }

        public HostConnect(bool success)
        {
            Success = success;
        }
    }
}
