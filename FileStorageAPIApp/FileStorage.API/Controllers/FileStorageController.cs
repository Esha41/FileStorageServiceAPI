using FileStorage.Application.DTOs;
using FileStorage.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FileStorage.API.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileStorageController : ControllerBase
    {
        private readonly ILogger<FileStorageController> _logger;
        private readonly IFileStorageService _fileService;
        private readonly string[] _allowedContentTypes;
        public FileStorageController(IFileStorageService fileService, ILogger<FileStorageController> logger, IConfiguration configuration)
        {
            _allowedContentTypes = configuration.GetSection("FileStorage:AllowedContentTypes").Get<string[]>() ?? Array.Empty<string>();

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
                if (!_allowedContentTypes.Contains(uploadFile.File.ContentType))
                    throw new InvalidOperationException($"File type '{uploadFile.File.ContentType}' is not allowed.");

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
        [HttpGet]
        // [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetAll([FromQuery] FilesQueryDto filesQuery)
        {
            try
            {
                var (items, totalCount) = await _fileService.GetAllFiles(filesQuery);

                var response = new
                {
                    TotalCount = totalCount,
                    PageNumber = filesQuery.PageNumber,
                    PageSize = filesQuery.PageSize,
                    Items = items
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET /api/files/{id}/download
        [HttpGet("{id}/download")]
        // [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Download(string id)
        {
            try
            {
                var file = await _fileService.DownloadFile(id);
                if (file == null) return NotFound();

                return File(file.Stream, file.ContentType, file.FileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET /api/files/{id}/preview
        [HttpGet("{id}/preview")]
        // [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Preview(string id)
        {
            try
            {
                var file = await _fileService.DownloadFile(id);
                if (file == null) return NotFound();

                return File(file.Stream, file.ContentType, file.FileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE /api/files/{id} (soft delete)
        [HttpDelete("{id}")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var success = await _fileService.SoftDeleteFileById(id);
                if (!success) return NotFound();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE /api/files/{id}/hard (hard delete)
        [HttpDelete("{id}/hard")]
        // [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> HardDelete(string id)
        {
            try
            {
                var success = await _fileService.HardDeleteFileById(id);
                if (!success) return NotFound();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
