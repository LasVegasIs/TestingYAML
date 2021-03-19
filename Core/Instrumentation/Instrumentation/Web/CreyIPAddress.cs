
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;

namespace Crey.Instrumentation.Web
{
    public static class CreyIPAddress
    {
        // handles whitespace and ports (that must be part of dotnet)
        // https://github.com/dotnet/runtime/issues/45194
        public static bool TryParse(string ipString, out IPAddress? iPAddress)
        {
            ipString = ipString.Trim();
            if (ipString.StartsWith("::ffff:"))
                ipString = ipString.Remove(0, 7);
#if NETSTANDARD_2_1 || NETCORE_2_1            
            if (EPTryParse(ipString, out var ep))
#else
            if (IPEndPoint.TryParse(ipString, out var ep))
#endif
            {
                iPAddress = ep.Address;
                return true;
            }
            else
            {
                iPAddress = null;
                return false;
            }
        }

#if NETSTANDARD_2_1 || NETCORE_2_1
        private static bool EPTryParse(string s, out IPEndPoint? result)
        {
            return EPTryParse(s.AsSpan(), out result);
        }

        private static bool EPTryParse(ReadOnlySpan<char> s, out IPEndPoint? result)
        {
            int addressLength = s.Length;  // If there's no port then send the entire string to the address parser
            int lastColonPos = s.LastIndexOf(':');

            // Look to see if this is an IPv6 address with a port.
            if (lastColonPos > 0)
            {
                if (s[lastColonPos - 1] == ']')
                {
                    addressLength = lastColonPos;
                }
                // Look to see if this is IPv4 with a port (IPv6 will have another colon)
                else if (s.Slice(0, lastColonPos).LastIndexOf(':') == -1)
                {
                    addressLength = lastColonPos;
                }
            }

            if (IPAddress.TryParse(s.Slice(0, addressLength), out IPAddress? address))
            {
                uint port = 0;
                if (addressLength == s.Length ||
                    (uint.TryParse(s.Slice(addressLength + 1), NumberStyles.None, CultureInfo.InvariantCulture, out port) && port <= ushort.MaxValue))

                {
                    result = new IPEndPoint(address, (int)port);
                    return true;
                }
            }

            result = null;
            return false;
        }
#endif
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
                                return CheckIp(ip.Skip(12).ToArray()); // use subarray .. in .NET 3.1
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
    }
}
