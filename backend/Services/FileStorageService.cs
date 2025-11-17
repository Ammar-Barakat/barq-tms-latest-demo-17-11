namespace BarqTMS.API.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder = "attachments");
        Task<bool> DeleteFileAsync(string fileName);
        Task<byte[]?> GetFileAsync(string fileName);
        string GetFileUrl(string fileName);
        bool FileExists(string fileName);
        long GetMaxFileSize();
        string[] GetAllowedExtensions();
    }

    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LocalFileStorageService> _logger;
        private readonly string _uploadPath;
        private readonly long _maxFileSize;
        private readonly string[] _allowedExtensions;

        public LocalFileStorageService(IWebHostEnvironment environment, IConfiguration configuration, ILogger<LocalFileStorageService> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
            
            _uploadPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads");
            _maxFileSize = _configuration.GetValue<long>("FileStorage:MaxFileSize", 10 * 1024 * 1024); // 10MB default
            _allowedExtensions = _configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>() 
                ?? new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".rar" };

            // Ensure upload directory exists
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder = "attachments")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null");

            if (file.Length > _maxFileSize)
                throw new ArgumentException($"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                throw new ArgumentException($"File type {extension} is not allowed");

            var folderPath = Path.Combine(_uploadPath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);
            var relativePath = Path.Combine(folder, fileName);

            try
            {
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                
                _logger.LogInformation("File saved successfully: {FilePath}", relativePath);
                return relativePath.Replace('\\', '/'); // Ensure forward slashes for URLs
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save file: {FileName}", file.FileName);
                throw;
            }
        }

        public Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_uploadPath, fileName.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("File deleted successfully: {FilePath}", fileName);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file: {FileName}", fileName);
                return Task.FromResult(false);
            }
        }

        public async Task<byte[]?> GetFileAsync(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_uploadPath, fileName.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(filePath))
                {
                    return await File.ReadAllBytesAsync(filePath);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get file: {FileName}", fileName);
                return null;
            }
        }

        public string GetFileUrl(string fileName)
        {
            return $"/uploads/{fileName}";
        }

        public bool FileExists(string fileName)
        {
            var filePath = Path.Combine(_uploadPath, fileName.Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(filePath);
        }

        public long GetMaxFileSize() => _maxFileSize;

        public string[] GetAllowedExtensions() => _allowedExtensions;
    }
}