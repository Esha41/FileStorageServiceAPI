using FileStorage.Application.Interfaces;
using FileStorage.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace FileStorage.API.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileStorageController : ControllerBase
    {
        private readonly ILogger<FileStorageController> _logger;
        private readonly IFileStorageService _fileService;
        public FileStorageController(IFileStorageService fileService, ILogger<FileStorageController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        // POST /api/files
        [HttpPost]
        [Consumes("multipart/form-data")]
        // [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Upload([FromForm] UploadFileDto uploadFile)
        {
            try
            {
                var stream = uploadFile.File.OpenReadStream();
                var userId = User.Identity!.Name ?? "admin";
                var stored = await _fileService.UploadFile(stream, uploadFile.File.FileName, uploadFile.File.ContentType, uploadFile.Tags, userId);
                return Ok(stored);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET /api/files
        //[HttpGet]
        //// [Authorize(Roles = "Admin,User")]
        //public async Task<IActionResult> GetAll()
        //{
        //    var files = await _fileService.GetAllFiles();
        //    return Ok(files); // returns list of StoredObjectDto
        //}

        // GET /api/files/{id}/download
        [HttpGet("{id}/download")]
        // [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Download(string id)
        {
            var file = await _fileService.DownloadFile(id);
            if (file == null) return NotFound();

            return File(file.Stream, file.ContentType, file.FileName);
        }

        // GET /api/files/{id}/preview
        [HttpGet("{id}/preview")]
        // [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Preview(string id)
        {
            var file = await _fileService.DownloadFile(id);
            if (file == null) return NotFound();

            return File(file.Stream, file.ContentType, file.FileName);
        }

        // DELETE /api/files/{id} (soft delete)
        [HttpDelete("{id}")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _fileService.SoftDeleteFileById(id);
            if (!success) return NotFound();
            return Ok(true);
        }

        // DELETE /api/files/{id}/hard (hard delete)
        [HttpDelete("{id}/hard")]
        // [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> HardDelete(string id)
        {
            var success = await _fileService.HardDeleteFileById(id);
            if (!success) return NotFound();
            return Ok(true);
        }
    }
}
