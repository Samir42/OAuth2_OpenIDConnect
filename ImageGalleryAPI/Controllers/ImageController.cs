using ImageGalleryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGalleryAPI.Controllers
{
    [Route("api/images")]
    [ApiController]
    [Authorize]
    public class ImageController : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "PayingUser")]
        public IEnumerable<Image> GetImages()
        {
            var images = new List<Image>
            {
              new Image(){ ImageName="Claire",OwnerId="1"},
              new Image(){ ImageName="Claire-Frank",OwnerId="1"},
              new Image(){ ImageName="Claire-Claire",OwnerId="1"},
              new Image(){ ImageName="Bob",OwnerId="2"},
              new Image(){ ImageName="Bob-Frank",OwnerId="2"},
              new Image(){ ImageName="Bob-Claire",OwnerId="2"},

            };

            var ownerId = User.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;

            var imagesToReturn = images.Where(x => x.OwnerId == ownerId);

            return imagesToReturn;
        }

        [Authorize(Policy = "MustOwnImage")]
        public string GetImage()
        {
            return "You got the image!";
        }
    }
}
