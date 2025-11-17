using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BarqTMS.API.Services;
using BarqTMS.API.Data;
using BarqTMS.API.Models;
using BarqTMS.API.DTOs;
using BarqTMS.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BarqTMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly BarqTMSDbContext _context;
        private readonly AuditService _auditService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(IFileStorageService fileStorageService, BarqTMSDbContext context, 
            AuditService auditService, ILogger<FilesController> logger)
        {
            _fileStorageService = fileStorageService;
            _context = context;
            _auditService = auditService;
            _logger = logger;
        }

        [HttpPost("upload/{taskId}")]
        public async Task<ActionResult<AttachmentDto>> UploadFile(int taskId, IFormFile file)
        {
            try
            {
                if (!await _context.Tasks.AnyAsync(t => t.TaskId == taskId))
                {
                    return NotFound($"Task with ID {taskId} not found.");
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
                var fileName = await _fileStorageService.SaveFileAsync(file, "task-attachments");

                var attachment = new Attachment
                {
                    TaskId = taskId,
                    FileName = file.FileName,
                    FileUrl = fileName,
                    UploadedBy = currentUserId,
                    UploadedAt = DateTime.UtcNow
                };

                _context.Attachments.Add(attachment);
                await _context.SaveChangesAsync();

                // Log the action
                await _auditService.LogAsync("Attachment", attachment.FileId, "Created", 
                    $"File '{file.FileName}' uploaded to task {taskId}", currentUserId);

                var attachmentDto = await _context.Attachments
                    .Where(a => a.FileId == attachment.FileId)
                    .Include(a => a.UploadedByUser)
                    .Select(a => new AttachmentDto
                    {
                        FileId = a.FileId,
                        TaskId = a.TaskId,
                        FileName = a.FileName,
                        FileUrl = a.FileUrl,
                        UploadedBy = a.UploadedBy,
                        UploadedByName = a.UploadedByUser.Name,
                        UploadedAt = a.UploadedAt
                    })
                    .FirstOrDefaultAsync();

                return Ok(attachmentDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file for task {TaskId}", taskId);
                return StatusCode(500, "An error occurred while uploading the file.");
            }
        }

        [HttpGet("download/{fileId}")]
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            try
            {
                var attachment = await _context.Attachments.FindAsync(fileId);
                if (attachment == null)
                {
                    return NotFound("File not found.");
                }

                var fileBytes = await _fileStorageService.GetFileAsync(attachment.FileUrl);
                if (fileBytes == null)
                {
                    return NotFound("File not found on storage.");
                }

                var contentType = GetContentType(attachment.FileName);
                return File(fileBytes, contentType, attachment.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file {FileId}", fileId);
                return StatusCode(500, "An error occurred while downloading the file.");
            }
        }

        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            try
            {
                var attachment = await _context.Attachments.FindAsync(fileId);
                if (attachment == null)
                {
                    return NotFound("File not found.");
                }

                var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);

                // Delete from storage
                await _fileStorageService.DeleteFileAsync(attachment.FileUrl);

                // Delete from database
                _context.Attachments.Remove(attachment);
                await _context.SaveChangesAsync();

                // Log the action
                await _auditService.LogAsync("Attachment", fileId, "Deleted", 
                    $"File '{attachment.FileName}' deleted", currentUserId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileId}", fileId);
                return StatusCode(500, "An error occurred while deleting the file.");
            }
        }

        [HttpGet("info")]
        public IActionResult GetUploadInfo()
        {
            return Ok(new
            {
                MaxFileSize = _fileStorageService.GetMaxFileSize(),
                MaxFileSizeMB = _fileStorageService.GetMaxFileSize() / (1024 * 1024),
                AllowedExtensions = _fileStorageService.GetAllowedExtensions()
            });
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
        }
    }
}