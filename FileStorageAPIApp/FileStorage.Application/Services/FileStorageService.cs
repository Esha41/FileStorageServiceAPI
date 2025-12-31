using FileStorage.Application.DTOs;
using FileStorage.Application.Interfaces;
using FileStorage.Domain.Entities;
using FileStorage.Domain.Interfaces;

namespace FileStorage.Application.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IFileStorageRepository _fileStorageRepository;
        private readonly ILocalFileStorageService _localFileStorageService;

        public FileStorageService(IFileStorageRepository fileStorageRepository, ILocalFileStorageService localFileStorageService)
        {
            _fileStorageRepository = fileStorageRepository;
            _localFileStorageService = localFileStorageService;
        }

        public async Task<StoredObject> UploadFile(Stream fileStream, string originalName, string contentType, string[] tags, string userId)
        {
            try
            {
                var storedObject = await _localFileStorageService.UploadFile(fileStream, originalName, contentType, tags, userId);
                await _fileStorageRepository.AddFile(storedObject);
                return storedObject;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<FileDownloadResponseDto> DownloadFile(string id)
        {
            try
            {
                var storedObject = await _fileStorageRepository.GetFileById(id);
                if (storedObject.DeletedAtUtc != null) throw new InvalidOperationException("File is deleted");

                var fileStream = await _localFileStorageService.DownloadFile(storedObject.Key);

                return new FileDownloadResponseDto
                {
                    Stream = fileStream,
                    ContentType = storedObject.ContentType,
                    FileName = storedObject.OriginalName
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SoftDeleteFileById(string id)
        {
            try
            {
                await _fileStorageRepository.SoftDeleteFileById(id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> HardDeleteFileById(string id)
        {
            try
            {
                bool isFileDeleted = true;
                var storedObject = await _fileStorageRepository.GetFileById(id);
                isFileDeleted = _localFileStorageService.DeleteFile(storedObject.Key);

                await _fileStorageRepository.HardDeleteFileById(storedObject);
                return isFileDeleted;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<(IEnumerable<StoredObject> Items, int TotalCount)> GetAllFiles(FilesQueryDto filesQuery)
        {
            try
            {
                return await _fileStorageRepository.GetAllFiles(filesQuery.Name, filesQuery.Tag, filesQuery.ContentType, filesQuery.DateFrom, filesQuery.DateTo, filesQuery.PageNumber, filesQuery.PageSize);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
