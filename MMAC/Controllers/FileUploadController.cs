using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;

namespace MMAC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly Cloudinary _cloudinary;

        private static readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
        private static readonly string[] _allowedMimeTypes = { "image/jpeg", "image/png", "application/pdf" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public FileUploadController(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        //  Helper: generate a signed URL for an "authenticated" raw asset
        private string GetSignedFileUrl(string publicId)
        {
            var url = _cloudinary.Api.UrlImgUp
                .ResourceType("raw")
                .Type("authenticated")
                .Signed(true)
                .Secure(true)
                .BuildUrl(publicId);

            return url;
        }

        [HttpPost("HealthRecord")]
        public async Task<IActionResult> UploadHealthRecord(IFormFile file)
        {
            // ── Validation 
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file received." });

            if (file.Length > MaxFileSizeBytes)
                return BadRequest(new { message = "File size must be under 5 MB." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(ext))
                return BadRequest(new { message = "File type not allowed. Allowed: jpg, png, pdf" });

            if (!_allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return BadRequest(new { message = "Invalid file content type." });

            // ── Upload to Cloudinary 
            using var stream = file.OpenReadStream();

            // ── OLD CODE (public "upload" type — kept for reference, do not delete) ──
            // var uploadParams = new RawUploadParams
            // {
            //     File = new FileDescription(file.FileName, stream),
            //     Folder = "mmac/health-records", // Cloudinary folder structure
            //     PublicId = Guid.NewGuid().ToString(),
            // };

            // ── NEW CODE: private/authenticated type so PDFs bypass the
            //    "untrusted account" raw-delivery block ──
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "mmac/health-records", // Cloudinary folder structure
                PublicId = Guid.NewGuid().ToString(),
                Type = "authenticated"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                return StatusCode(500, new { message = uploadResult.Error.Message });
            }

            return Ok(new
            {
                message = "File uploaded successfully.",

                // ── OLD CODE ──
                // fileUrl = uploadResult.SecureUrl.ToString(), // Cloudinary HTTPS URL

                // ── NEW CODE: build a signed URL instead of using the raw SecureUrl ──
                fileUrl = GetSignedFileUrl(uploadResult.PublicId),

                fileName = uploadResult.PublicId,
                originalFileName = file.FileName
            });
        }

        [HttpPost("DigitalRecord")]
        public async Task<IActionResult> UploadDigitalRecord(IFormFile file)
        {
            // ── Validation 
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file received." });

            if (file.Length > MaxFileSizeBytes)
                return BadRequest(new { message = "File size must be under 5 MB." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(ext))
                return BadRequest(new { message = "File type not allowed. Allowed: jpg, png, pdf" });

            if (!_allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return BadRequest(new { message = "Invalid file content type." });

            // ── Upload to Cloudinary 
            using var stream = file.OpenReadStream();

            // ── OLD CODE (public "upload" type — kept for reference, do not delete) ──
            // var uploadParams = new RawUploadParams
            // {
            //     File = new FileDescription(file.FileName, stream),
            //     Folder = "mmac/digital-records",
            //     PublicId = Guid.NewGuid().ToString(),
            // };

            // ── NEW CODE: private/authenticated type so PDFs bypass the
            //    "untrusted account" raw-delivery block ──
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "mmac/digital-records",
                PublicId = Guid.NewGuid().ToString(),
                Type = "authenticated"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                return StatusCode(500, new { message = uploadResult.Error.Message });
            }

            return Ok(new
            {
                message = "Digital record uploaded successfully.",

                // ── OLD CODE ──
                // fileUrl = uploadResult.SecureUrl.ToString(),

                // ── NEW CODE: build a signed URL instead of using the raw SecureUrl ──
                fileUrl = GetSignedFileUrl(uploadResult.PublicId),

                fileName = uploadResult.PublicId,
                originalFileName = file.FileName
            });
        }
    }
}



//using CloudinaryDotNet;
//using CloudinaryDotNet.Actions;
//using Microsoft.AspNetCore.Mvc;

//namespace MMAC.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class FileUploadController : ControllerBase
//    {
//        private readonly Cloudinary _cloudinary;

//        private static readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
//        private static readonly string[] _allowedMimeTypes = { "image/jpeg", "image/png", "application/pdf" };
//        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

//        public FileUploadController(Cloudinary cloudinary)
//        {
//            _cloudinary = cloudinary;
//        }

//        [HttpPost("HealthRecord")]
//        public async Task<IActionResult> UploadHealthRecord(IFormFile file)
//        {
//            // ── Validation 
//            if (file == null || file.Length == 0)
//                return BadRequest(new { message = "No file received." });

//            if (file.Length > MaxFileSizeBytes)
//                return BadRequest(new { message = "File size must be under 5 MB." });

//            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
//            if (!_allowedExtensions.Contains(ext))
//                return BadRequest(new { message = "File type not allowed. Allowed: jpg, png, pdf" });

//            if (!_allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
//                return BadRequest(new { message = "Invalid file content type." });

//            // ── Upload to Cloudinary 
//            using var stream = file.OpenReadStream();
//            var uploadParams = new RawUploadParams
//            {
//                File = new FileDescription(file.FileName, stream),
//                Folder = "mmac/health-records", // Cloudinary folder structure
//                PublicId = Guid.NewGuid().ToString(),

//            };

//            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

//            if (uploadResult.Error != null)
//            {
//                return StatusCode(500, new { message = uploadResult.Error.Message });
//            }

//            return Ok(new
//            {
//                message = "File uploaded successfully.",
//                fileUrl = uploadResult.SecureUrl.ToString(), // Cloudinary HTTPS URL
//                fileName = uploadResult.PublicId,
//                originalFileName = file.FileName
//            });
//        }

//        [HttpPost("DigitalRecord")]
//        public async Task<IActionResult> UploadDigitalRecord(IFormFile file)
//        {
//            // ── Validation 
//            if (file == null || file.Length == 0)
//                return BadRequest(new { message = "No file received." });

//            if (file.Length > MaxFileSizeBytes)
//                return BadRequest(new { message = "File size must be under 5 MB." });

//            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
//            if (!_allowedExtensions.Contains(ext))
//                return BadRequest(new { message = "File type not allowed. Allowed: jpg, png, pdf" });

//            if (!_allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
//                return BadRequest(new { message = "Invalid file content type." });

//            // ── Upload to Cloudinary 
//            using var stream = file.OpenReadStream();
//            var uploadParams = new RawUploadParams
//            {
//                File = new FileDescription(file.FileName, stream),
//                Folder = "mmac/digital-records",
//                PublicId = Guid.NewGuid().ToString(),

//            };

//            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

//            if (uploadResult.Error != null)
//            {
//                return StatusCode(500, new { message = uploadResult.Error.Message });
//            }

//            return Ok(new
//            {
//                message = "Digital record uploaded successfully.",
//                fileUrl = uploadResult.SecureUrl.ToString(),
//                fileName = uploadResult.PublicId,
//                originalFileName = file.FileName
//            });
//        }
//    }
//}