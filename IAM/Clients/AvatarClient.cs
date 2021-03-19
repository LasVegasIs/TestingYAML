using Crey.Kernel;
using Crey.Kernel.Avatar;
using System.Net.Http;
using System.Threading.Tasks;

namespace IAM.Clients
{
    public static class AvatarClient
    {
        public static Task SetUserAvatar(this CreyRestClient creyRestClient, int avatarId)
        {
            return creyRestClient.CreateRequest(
                HttpMethod.Put,
                AvatarDefaults.SERVICE_NAME,
                $"/avatar/api/v1/avatars/{avatarId}")
                .AddUserAuthentication()
                .SendAndTryAckAsync();
        }
    }
}
