using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Net;

namespace Crey.Instrumentation.Web
{
    public class CreyIPAddressTests
    {
        [Fact]
        public void IsInternal()
        {
            Assert.True(CreyIPAddress.IsInternal(IPAddress.Parse("::ffff:10.65.0.11")));
            Assert.True(CreyIPAddress.IsInternal(IPAddress.Parse("::1")));
            Assert.True(CreyIPAddress.IsInternal(IPAddress.Parse("10.0.0.0")));
            Assert.True(CreyIPAddress.IsInternal(IPAddress.Parse("10.255.255.255")));
            Assert.True(CreyIPAddress.IsInternal(IPAddress.Parse("127.0.0.0")));
            Assert.True(CreyIPAddress.IsInternal(IPAddress.Parse("127.255.255.255")));
            Assert.True(CreyIPAddress.IsInternal(IPAddress.Parse("172.16.0.0")));
            Assert.True(CreyIPAddress.IsInternal(IPAddress.Parse("172.31.255.255")));
            Assert.True(CreyIPAddress.IsInternal(IPAddress.Parse("192.168.0.0")));
            Assert.True(CreyIPAddress.IsInternal(IPAddress.Parse("192.168.255.255")));
            Assert.True(CreyIPAddress.IsInternal(IPAddress.Parse("fd12:3456:789a:1::1")));
            Assert.True(CreyIPAddress.IsInternal(IPAddress.Parse("169.254.0.0")));
            Assert.True(CreyIPAddress.IsInternal(IPAddress.Parse("169.254.255.255")));
            Assert.True(CreyIPAddress.IsInternal(IPAddress.Parse("fd80::")));
            Assert.True(CreyIPAddress.IsInternal(IPAddress.Parse("10.65.1.37")));

        }

        [Fact]
        public void NotIsInternal()
        {
            Assert.False(CreyIPAddress.IsInternal(IPAddress.Parse("::ffff:169.253.0.0")));
            Assert.False(CreyIPAddress.IsInternal(IPAddress.Parse("2001:db8::8a2e:370:7334")));
            Assert.False(CreyIPAddress.IsInternal(IPAddress.Parse("169.253.0.0")));
            Assert.False(CreyIPAddress.IsInternal(IPAddress.Parse("169.255.255.255")));
            Assert.False(CreyIPAddress.IsInternal(IPAddress.Parse("ff62:7fa4:80e5:d0bd:ffff:ffff:ffff:ffff")));
        }

        [Fact]
        public void Parse()
        {
            IPAddress _;
            Assert.True(CreyIPAddress.TryParse("::ffff:10.65.0.11", out _));
            Assert.Equal(_, IPAddress.Parse("10.65.0.11"));
            Assert.True(CreyIPAddress.TryParse("1fff:0:a88:85a3::ac1f:42", out _));
            Assert.True(CreyIPAddress.TryParse("127.0.0.1", out _));
            Assert.Equal(_, IPAddress.Parse("127.0.0.1"));
            Assert.True(CreyIPAddress.TryParse("127.0.0.1:42", out _));
            Assert.Equal(_, IPAddress.Parse("127.0.0.1"));
            Assert.True(CreyIPAddress.TryParse("  127.0.0.1 ", out _));
            Assert.True(CreyIPAddress.TryParse("  127.0.0.1:42   ", out _));
            Assert.Equal(_, IPAddress.Parse("127.0.0.1"));
            Assert.True(CreyIPAddress.TryParse("1fff:0:a88:85a3::ac1f", out _));
            Assert.Equal(_, IPAddress.Parse("1fff:0:a88:85a3::ac1f"));
            Assert.True(CreyIPAddress.TryParse("[1fff:0:a88:85a3::ac1f]", out _));
            Assert.Equal(_, IPAddress.Parse("1fff:0:a88:85a3::ac1f"));

            Assert.True(CreyIPAddress.TryParse("fe80::1ff:fe23:4567:890a%3", out _));

            Assert.True(CreyIPAddress.TryParse("[1fff:0:a88:85a3::ac1f]:42", out _));
            Assert.Equal(_, IPAddress.Parse("1fff:0:a88:85a3::ac1f"));

            Assert.True(CreyIPAddress.TryParse(" [1fff:0:a88:85a3::ac1f]:42 ", out _));
            Assert.Equal(_, IPAddress.Parse("1fff:0:a88:85a3::ac1f"));
        }
    }
}
