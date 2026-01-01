using FileStorage.Application.Interfaces;
using FileStorage.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text.Json;

namespace FileStorage.Infrastructure.LocalFileStorageService
{
    public class LocalFileStorageService : ILocalFileStorageService
    {
        private readonly string _basePath;
        private readonly string _fileName;
        private readonly string _metaName;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
        {
            _basePath = configuration["FileStorage:BaseFilePath"] ?? "_storage";
            _fileName = configuration["FileStorage:FileName"] ?? "content.bin";
            _metaName = configuration["FileStorage:MetaFile"] ?? "metadata.json";
            Directory.CreateDirectory(_basePath);
            _logger = logger;
        }
        public async Task<StoredObject> UploadFile(Stream fileStream, string originalName, string contentType, string[]? tags, string userId)
        {
            var fileKey = Guid.NewGuid().ToString();
            try
            {
                // create folder structure to store file locally
                var datePath = Path.Combine(DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"), DateTime.UtcNow.ToString("dd"));
                var folderPath = Path.Combine(_basePath, datePath);
                Directory.CreateDirectory(folderPath);

                var fileFolder = Path.Combine(folderPath, fileKey);
                Directory.CreateDirectory(fileFolder);

                var filePath = Path.Combine(fileFolder, _fileName);
                var tempFilePath = filePath + ".tmp";

                _logger.LogInformation("Writing file to temporary path {TempFilePath}", tempFilePath);

                // compute checksum to ensure file integrity
                string checksum;
                long sizeBytes = 0;

                using (var sha256 = SHA256.Create())
                using (var outputStream = File.Create(tempFilePath))
                {
                    var buffer = new byte[81920];
                    int bytesRead;

                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await outputStream.WriteAsync(buffer, 0, bytesRead);
                        sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
                        sizeBytes += bytesRead;
                    }

                    sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                    checksum = BitConverter.ToString(sha256.Hash!).Replace("-", "").ToLower();
                }

                File.Move(tempFilePath, filePath);

                _logger.LogInformation("File saved successfully. FilePath={filePath}, SizeBytes={sizeBytes}, Checksum={checksum}",
                                   filePath, sizeBytes, checksum);

                // create StoredObject entity for db save
                var storedObject = new StoredObject
                {
                    Id = Guid.NewGuid(),
                    Key = fileKey,
                    OriginalName = originalName,
                    SizeBytes = sizeBytes,
                    ContentType = contentType,
                    Checksum = checksum,
                    Tags = tags == null ? string.Empty : string.Join(',', tags),
                    CreatedAtUtc = DateTime.UtcNow,
                    Version = 1,
                    CreatedByUserId = userId
                };

                // file metadata, store data to metadata.json
                var metadataPath = Path.Combine(fileFolder, _metaName);
                var metadataJson = JsonSerializer.Serialize(storedObject, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(metadataPath, metadataJson);

                _logger.LogInformation("File metadata written successfully. MetadataPath={metadataPath}", metadataPath);

                return storedObject;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {originalName} for user {userId}, FileKey={FileKey}",
                              originalName, userId, fileKey);
                throw;
            }
        }

        public Task<Stream> DownloadFile(string key)
        {
            try
            {
                var folderPath = Directory.GetDirectories(_basePath, key, SearchOption.AllDirectories).FirstOrDefault();
                if (folderPath == null)
                {
                    _logger.LogError("Download failed: Folder not found for FileKey={key}", key);
                    throw new FileNotFoundException($"Folder not found for key {key}");
                }

                var filePath = Path.Combine(folderPath, "content.bin");
                if (!File.Exists(filePath))
                {
                    _logger.LogError("Download failed: File not found at path {filePath} for FileKey={key}", filePath, key);
                    throw new FileNotFoundException($"File not found at path {filePath}");
                }

                _logger.LogInformation("File found. FilePath={filePath}, FileKey={key}", filePath, key);
                return Task.FromResult<Stream>(File.OpenRead(filePath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file with FileKey={key}", key);
                throw;
            }
        }

        public bool DeleteFile(string key)
        {
            try
            {
                var folderPath = Directory.GetDirectories(_basePath, key, SearchOption.AllDirectories).FirstOrDefault();
                if (folderPath == null)
                {
                    _logger.LogError("Delete failed: Folder not found for FileKey={key}", key);
                    return false;
                }

                //delete file from path
                var filePath = Path.Combine(folderPath, "content.bin");

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted file content at {filePath} for FileKey={key}", filePath, key);
                }
                else
                {
                    _logger.LogError("File content not found at {filePath} for FileKey={key}", filePath, key);
                }

                //delete metadata.json from path
                var metadataPath = Path.Combine(folderPath, "metadata.json");

                if (File.Exists(metadataPath))
                {
                    File.Delete(metadataPath);
                    _logger.LogInformation("Deleted metadata at {metadataPath} for FileKey={key}", metadataPath, key);
                }
                else
                {
                    _logger.LogError("Metadata not found at {metadataPath} for FileKey={key}", metadataPath, key);
                }

                //delete folder
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath);
                    _logger.LogInformation("Deleted folder at {folderPath} for FileKey={key}", folderPath, key);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file with FileKey={FileKey}", key);
                throw;
            }
        }
    }
}
