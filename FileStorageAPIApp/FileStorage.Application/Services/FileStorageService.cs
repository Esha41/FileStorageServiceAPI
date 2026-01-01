using FileStorage.Application.DTOs;
using FileStorage.Application.Interfaces;
using FileStorage.Domain.Entities;
using FileStorage.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileStorage.Application.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IFileStorageRepository _fileStorageRepository;
        private readonly ILocalFileStorageService _localFileStorageService;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(ILogger<FileStorageService> logger, IFileStorageRepository fileStorageRepository, ILocalFileStorageService localFileStorageService)
        {
            _logger = logger;
            _fileStorageRepository = fileStorageRepository;
            _localFileStorageService = localFileStorageService;
        }

        public async Task<StoredObject> UploadFile(Stream fileStream, string originalName, string contentType, string[] tags, string userId)
        {
            try
            {
                var storedObject = await _localFileStorageService.UploadFile(fileStream, originalName, contentType, tags, userId);
                await _fileStorageRepository.AddFile(storedObject);
                _logger.LogInformation("Upload completed. FileKey={FileKey}, OriginalName={originalName}, UserId={userId}", storedObject.Key, originalName, userId);
                return storedObject;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file. OriginalName={originalName}, UserId={userId}", originalName, userId);
                throw;
            }
        }


        public async Task<FileDownloadResponseDto> DownloadFile(string id)
        {
            try
            {
                var storedObject = await _fileStorageRepository.GetFileById(id);
                if (storedObject == null)
                {
                    _logger.LogWarning("Download failed: File not found. FileId={id}", id);
                    return null;
                }

                if (storedObject.DeletedAtUtc != null)
                {
                    _logger.LogWarning("Download failed: File is deleted. FileId={id}", id);
                    throw new InvalidOperationException("File is deleted");
                }

                var fileStream = await _localFileStorageService.DownloadFile(storedObject.Key);
                _logger.LogInformation("Download completed. FileKey={FileKey}, FileId={id}", storedObject.Key, id);

                return new FileDownloadResponseDto
                {
                    Stream = fileStream,
                    ContentType = storedObject.ContentType,
                    FileName = storedObject.OriginalName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file. FileId={id}", id);
                throw;
            }
        }

        public async Task<bool> SoftDeleteFileById(string id)
        {
            try
            {
                var storedObject = await _fileStorageRepository.GetFileById(id);
                if (storedObject == null)
                {
                    _logger.LogWarning("Soft delete failed: File not found. FileId={id}", id);
                    return false;
                }
                await _fileStorageRepository.SoftDeleteFileById(storedObject);
                _logger.LogInformation("Soft delete operation completed. FileId={id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during soft delete. FileId={id}", id);
                return false;
            }
        }

        public async Task<bool> HardDeleteFileById(string id)
        {
            try
            {
                bool isFileDeleted = true;
                var storedObject = await _fileStorageRepository.GetFileById(id);
                if (storedObject == null)
                {
                    _logger.LogWarning("Hard delete failed: File not found. FileId={id}", id);
                    return false;
                }

                _localFileStorageService.DeleteFile(storedObject.Key);

                await _fileStorageRepository.HardDeleteFileById(storedObject);
                _logger.LogInformation("Hard delete completed. FileKey={FileKey}, FileId={id}, Deleted={isFileDeleted}", storedObject.Key, id, isFileDeleted);

                return isFileDeleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during hard delete. FileId={id}", id);
                return false;
            }
        }

        public async Task<(IEnumerable<StoredObject> Items, int TotalCount)> GetAllFiles(FilesQueryDto filesQuery)
        {
            try
            {
                _logger.LogInformation("Fetching all files. Query={filesQuery}", filesQuery);
                var result = await _fileStorageRepository.GetAllFiles(
                 filesQuery.Name,
                 filesQuery.Tag,
                 filesQuery.ContentType,
                 filesQuery.DateFrom,
                 filesQuery.DateTo,
                 filesQuery.PageNumber,
                 filesQuery.PageSize
             );

                _logger.LogInformation("Fetched {Count} files matching query.", result.Items.Count());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching files. Query={filesQuery}", filesQuery);
                throw;
            }
        }
    }
}
