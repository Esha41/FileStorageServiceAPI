using FileStorage.Application.Interfaces;
using FileStorage.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text.Json;

namespace FileStorage.Infrastructure.LocalFileStorageService
{
    public class LocalFileStorageService : ILocalFileStorageService
    {
        private readonly string _basePath;

        public LocalFileStorageService(IConfiguration configuration)
        {
            _basePath = configuration["Storage:BaseFilePath"] ?? "_storage";
            Directory.CreateDirectory(_basePath);
        }
        public async Task<StoredObject> UploadFile(Stream fileStream, string originalName, string contentType, string[]? tags, string userId)
        {
            try
            {
                // create folder structure to store file locally
                var datePath = Path.Combine(DateTime.UtcNow.ToString("yyyy"),
                                            DateTime.UtcNow.ToString("MM"),
                                            DateTime.UtcNow.ToString("dd"));
                var folderPath = Path.Combine(_basePath, datePath);
                Directory.CreateDirectory(folderPath);

                var fileKey = Guid.NewGuid().ToString();
                var fileFolder = Path.Combine(folderPath, fileKey);
                Directory.CreateDirectory(fileFolder);

                var filePath = Path.Combine(fileFolder, "content.bin");
                var tempFilePath = filePath + ".tmp";

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
                var metadataPath = Path.Combine(fileFolder, "metadata.json");
                var metadataJson = JsonSerializer.Serialize(storedObject, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(metadataPath, metadataJson);

                return storedObject;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<Stream> DownloadFile(string key)
        {
            try
            {
                var folderPath = Directory.GetDirectories(_basePath, key, SearchOption.AllDirectories).FirstOrDefault();
                if (folderPath == null) throw new FileNotFoundException();

                var filePath = Path.Combine(folderPath, "content.bin");
                if (!File.Exists(filePath)) throw new FileNotFoundException();

                return Task.FromResult<Stream>(File.OpenRead(filePath));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteFile(string key)
        {
            try
            {
                var folderPath = Directory.GetDirectories(_basePath, key, SearchOption.AllDirectories).FirstOrDefault();
                if (folderPath == null) throw new FileNotFoundException();

                var filePath = Path.Combine(folderPath, "content.bin");
                if (File.Exists(filePath))
                    File.Delete(filePath);

                var metadataPath = Path.Combine(folderPath, "metadata.json");
                if (File.Exists(metadataPath))
                    File.Delete(metadataPath);

                if (Directory.Exists(folderPath))
                    Directory.Delete(folderPath);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
