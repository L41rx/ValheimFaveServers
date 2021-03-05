using System.Net;

namespace ValheimFaveServers
{
    public class ValheimFaveServerData
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public int Port { get; set; }
        public string Host { get; set; }
        public IPAddress Ip { get; set; }
        public string Password { get; set; }

        public string toString()
        {
            return "[key: " + this.Key + " name: " + this.Name + " host: " + this.Host + " ip: " + this.Ip.ToString() + "]";
        }
    }
}
