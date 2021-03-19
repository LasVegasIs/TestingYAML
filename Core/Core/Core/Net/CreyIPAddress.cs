using Crey.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using System.Linq;

namespace System.Net
{
    // moved to standard
    public static class CreyIPAddress
    {
        // handles whitespace and ports (that must be part of dotnet)
        // https://github.com/dotnet/runtime/issues/45194
        public static bool TryParse(string ipString, out IPAddress iPAddress)
        {
            ipString = ipString.Trim();
            if (ipString.StartsWith("::ffff:"))
                ipString = ipString.Remove(0, 7);
            if (IPEndPoint.TryParse(ipString, out var ep))
            {
                iPAddress = ep.Address;
                return true;
            }

            iPAddress = null;
            return false;
        }

        // https://stackoverflow.com/questions/8113546/how-to-determine-whether-an-ip-address-in-private
        // https://en.wikipedia.org/wiki/Private_network
        // https://en.wikipedia.org/wiki/Unique_local_address
        // https://docs.ipdata.co/api-reference/status-codes - as per logs, it returns 400 for internal, not only for local

        public static bool IsInternal(this IPAddress self)
        {
            if (IPAddress.IsLoopback(self))
                return true;

            byte[] ip = self.GetAddressBytes();
            switch (ip)
            {

                case byte[] _ when ip.Length == 16:
                    {
                        switch (ip)
                        {
                            // could you zero cost struct linq if needed in future
                            case byte[] _ when ip.Take(10).Select(x => (int)x).Sum() == 0 && ip[10] == 255 && ip[11] == 255:
                                return CheckIp(ip[12..]);
                            case byte[] _ when ip[0] == 253:
                                return true;
                            case byte[] _ when ip[0] == 254 && ip[1] == 128 && ip[2] == 0 && ip[3] == 0 && ip[4] == 0 && ip[5] == 0 && ip[6] == 0 && ip[7] == 0:
                                return true;
                            default:
                                return false;
                        }
                    }

                case byte[] _ when ip.Length == 4:
                    {
                        return CheckIp(ip);
                    }
                default:
                    throw new InvalidProgramException("Either 4 or 16 bytes");
            }
        }

        private static bool CheckIp(ReadOnlySpan<byte> ip)
        {
            switch (ip)
            {
                case ReadOnlySpan<byte> _ when ip[0] == 169 && ip[1] == 254:
                    return true;
                case ReadOnlySpan<byte> _ when ip[0] == 10:
                    return true;
                case ReadOnlySpan<byte> _ when ip[0] == 172:
                    return ip[1] >= 16 && ip[1] < 32;
                case ReadOnlySpan<byte> _ when ip[0] == 192:
                    return ip[1] == 168;
                default:
                    return false;
            }
        }

        public static string ToIPString(this IPNetwork self)
        {
            return self.PrefixLength == 0 ? self.Prefix.ToString() : self.Prefix + "/" + self.PrefixLength;
        }

        public static IPNetwork ParseNetwork(string value)
        {
            TryParseNetwork(value, out var network);
            return network;
        }

        internal static bool TryParseNetwork(string value, out IPNetwork iPNetwork)
        {
            iPNetwork = null;
            if (value.IsNullOrEmpty())
            {
                return false;
            }

            var split = value.Split("/");
            if (split.Length == 2)
            {
                if (IPAddress.TryParse(split[0], out var addr) && int.TryParse(split[1], out var prefixLength))
                {
                    iPNetwork = new IPNetwork(addr, prefixLength);
                    return true;
                }
            }
            else if (split.Length == 1)
            {
                if (IPAddress.TryParse(split[0], out var addr))
                {
                    iPNetwork = new IPNetwork(addr, 0);
                    return true;
                }
            }

            return false;
        }
    }
}
