using System;

namespace Crey.MessageContracts.Notification
{
    [Flags]
    public enum NotificationType
    {
        Silent = 0,
        Offline = 1,
        Push = 2,
        //Email = 4,
        //SMS = 8, :)
    }
}
