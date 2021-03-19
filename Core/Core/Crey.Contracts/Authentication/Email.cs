using System.Collections.Generic;

namespace Crey.Contracts.Authentication
{
    public class EmailTemplateParameters
    {
        public Dictionary<string, string> Parameters { get; set; }
    }

    public class FeedbackParameters
    {
        public string Feedback { get; set; }
    }
}
