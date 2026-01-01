using FileStorage.Application.DTOs;
using FileStorage.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
            var userId = User.Identity?.Name ?? "admin";

            _logger.LogInformation(
                "Upload request received. FileName={FileName}, ContentType={ContentType}, UserId={UserId}",
                uploadFile.File.FileName,
                uploadFile.File.ContentType,
                userId
            );

            try
            {
                if (!_allowedContentTypes.Contains(uploadFile.File.ContentType))
                {
                    _logger.LogWarning(
                        "Upload rejected due to invalid content type. ContentType={ContentType}, UserId={UserId}",
                        uploadFile.File.ContentType,
                        userId
                    );
                    throw new InvalidOperationException("File type not allowed.");
                }

                var stream = uploadFile.File.OpenReadStream();
                var storedFile = await _fileService.UploadFile(stream, uploadFile.File.FileName, uploadFile.File.ContentType, uploadFile.Tags, userId);

                _logger.LogInformation(
                    "File uploaded successfully. FileId={FileId}, StoredKey={Key}, SizeBytes={SizeBytes}, UserId={UserId}",
                    storedFile.Id,
                    storedFile.Key,
                    storedFile.SizeBytes,
                    userId
               );

                return Ok(storedFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                       ex,
                       "File upload failed. FileName={FileName}, UserId={UserId}",
                       uploadFile.File.FileName,
                       userId
               );

                throw;
            }
        }

        // GET /api/files
        [HttpGet]
        // [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetAll([FromQuery] FilesQueryDto filesQuery)
        {
            _logger.LogInformation(
                    "Get list of files. Page={Page}, Size={Size}, Filters={@Filters}",
                    filesQuery.PageNumber,
                    filesQuery.PageSize,
                    filesQuery
            );
            try
            {
                var (items, totalCount) = await _fileService.GetAllFiles(filesQuery);

                _logger.LogInformation("Files retrieved successfully. Count={Count}, Total={TotalCount}",
                                                                        items.ToList().Count, totalCount);

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
                _logger.LogError(ex, "Failed to fetch files list.");
                throw;
            }
        }

        // GET /api/files/{id}/download
        [HttpGet("{id}/download")]
        // [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Download(string id)
        {
            _logger.LogInformation("Download request received. FileId={id}", id);

            try
            {
                var file = await _fileService.DownloadFile(id);
                if (file == null)
                {
                    _logger.LogWarning("Download failed. File not found. FileId={id}", id);
                    throw new FileNotFoundException(" File not found");
                }

                _logger.LogInformation("File download started. FileId={id}, ContentType={file.FileName}",
                                                                                    id, file.ContentType);

                return File(file.Stream, file.ContentType, file.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File download failed. FileId={id}", id);
                throw;
            }
        }

        // GET /api/files/{id}/preview
        [HttpGet("{id}/preview")]
        // [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Preview(string id)
        {
            _logger.LogInformation("Preview request received. FileId={id}", id);

            try
            {
                var file = await _fileService.DownloadFile(id);

                if (file == null)
                {
                    _logger.LogWarning("Previewed failed. File not found. FileId={id}", id);
                    throw new FileNotFoundException(" File not found");
                }

                _logger.LogInformation("File previewed. FileId={id}, ContentType={file.FileName}",
                                                                                    id, file.ContentType);
                return File(file.Stream, file.ContentType, file.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File preview failed. FileId={id}", id);
                throw;
            }
        }

        // DELETE /api/files/{id} (soft delete)
        [HttpDelete("{id}")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation("Soft delete requested. FileId={id}", id);

            try
            {
                var success = await _fileService.SoftDeleteFileById(id);
                if (!success)
                {
                    _logger.LogWarning("Hard Soft failed. File not found. FileId={id}", id);
                    throw new FileNotFoundException(" File not found");
                }

                _logger.LogInformation("File Soft deleted successfully. FileId={id}", id);
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Soft delete failed. FileId={id}", id);
                throw;
            }
        }

        // DELETE /api/files/{id}/hard (hard delete)
        [HttpDelete("{id}/hard")]
        // [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> HardDelete(string id)
        {
            _logger.LogInformation("Hard delete requested. FileId={id}", id);

            try
            {
                var success = await _fileService.HardDeleteFileById(id);
                if (!success)
                {
                    _logger.LogWarning("Hard delete failed. File not found. FileId={id}", id);
                    throw new FileNotFoundException(" File not found");
                }

                _logger.LogInformation("File hard deleted successfully. FileId={id}", id);
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hard delete failed. FileId={id}", id);
                throw;
            }
        }
    }
}
