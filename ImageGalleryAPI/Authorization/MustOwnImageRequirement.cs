using Microsoft.AspNetCore.Authorization;

namespace ImageGalleryAPI.Authorization
{
    public class MustOwnImageRequirement : IAuthorizationRequirement
    {
        public MustOwnImageRequirement()
        {

        }
    }
}
