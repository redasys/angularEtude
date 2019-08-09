using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private readonly Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repository, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _cloudinaryConfig = cloudinaryConfig;
            _mapper = mapper;
            _repo = repository;

            var acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);

        }
        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var data = await _repo.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnDto>(data);

            return Ok(photo);
        }


        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, PhotoForCreationDto photoToUpload)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var user = await _repo.GetUser(userId);

            var file = photoToUpload.File;

            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }
            photoToUpload.Url = uploadResult.Uri.ToString();
            photoToUpload.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoToUpload);

            photo.IsMain = user.Photos.All(x => !x.IsMain);

            user.Photos.Add(photo);

            if (await _repo.SaveAll())
            {
                var result = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, result);
            }

            return BadRequest("Couldn't add photo");

        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMain(int userId, int id)
        {
            var response = await VerifyPhotoForUser(userId, id);
            if (response != null)
            {
                return response;
            }

            var photo = await _repo.GetPhoto(id);

            if (photo.IsMain)
            {
                return BadRequest("Photo is already main");
            }

            var curMain = await _repo.GetMainPhotoForUser(userId);

            if (curMain != null)
            {
                curMain.IsMain = false;
            }
            photo.IsMain = true;

            if (await _repo.SaveAll())
            {
                return NoContent();
            }

            return BadRequest("Could not set photo to main");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            var response = await VerifyPhotoForUser(userId, id);
            if (response != null)
            {
                return response;
            }

            var photo = await _repo.GetPhoto(id);
            var user = await _repo.GetUser(userId);

            if (photo.IsMain)
            {
                return BadRequest("You cannot delete your main pic.");
            }

            if (photo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photo.PublicId);
                var result = _cloudinary.Destroy(deleteParams);

                if (!result.Result.Equals("ok", StringComparison.InvariantCultureIgnoreCase))
                {
                    return BadRequest("Could not remove from cloud");
                }
            }

            _repo.Delete(photo);

            if (await _repo.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Could not delete photo");

        }

        private async Task<IActionResult> VerifyPhotoForUser(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await _repo.GetUser(userId);

            if (user.Photos.All(p => p.Id != id))
            {
                return Unauthorized();
            }

            return null;
        }
    }
}