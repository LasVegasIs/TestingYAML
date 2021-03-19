using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MockServer.Rules
{
    /// <summary>
    ///  Base for the route rules. Once a role is added don't modify it. Take the AddRule function as a "move" operation.
    /// </summary>
    public interface Rule
    {
        int Id { get; set; }
        IEnumerable<HttpMethod> Methods { get; }
        Regex RoutePattern { get; }
        bool IsInternal { get; }
        int Priority { get; }

        Task Handle(IServiceProvider serviceProvider, HttpListenerContext ctx);
    }
}
