using Crey.Authentication;
using System.Collections.Generic;

namespace MockServer.Rules.IAM
{
    /// <summary>
    /// Simple in memory store for the created sessions/users
    /// </summary>
    public class SessionStore
    {
        Dictionary<string, SessionInfo> _sessions = new Dictionary<string, SessionInfo>();

        public void Add(SessionInfo info)
        {
            lock (this)
            {
                _sessions.Add(info.Key, info);
            }
        }

        public SessionInfo? Get(string key)
        {
            lock (this)
            {
                return _sessions.GetValueOrDefault(key);
            }
        }
    }
}
