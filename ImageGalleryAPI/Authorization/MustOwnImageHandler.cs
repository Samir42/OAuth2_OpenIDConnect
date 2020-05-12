using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGalleryAPI.Authorization
{
    public class MustOwnImageHandler : AuthorizationHandler<MustOwnImageRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MustOwnImageHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ??
                throw new ArgumentNullException(nameof(httpContextAccessor));
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            MustOwnImageRequirement requirement)
        {
            var imageId = _httpContextAccessor.HttpContext.GetRouteValue("id").ToString();

            if (int.Parse(imageId) < 0)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var ownerId = context.User.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;

            //call repository.IsImageOwner. So i do not do it. Because i'm on the verge of learning.


            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
