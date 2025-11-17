using System.ComponentModel.DataAnnotations;

namespace BarqTMS.API.DTOs
{
    public class TaskDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PriorityId { get; set; }
        public string PriorityLevel { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public int? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public int DeptId { get; set; }
        public string DeptName { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int CommentCount { get; set; }
        public int AttachmentCount { get; set; }
    }

    public class CreateTaskDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        public int PriorityId { get; set; }
        
        [Required]
        public int StatusId { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public int? AssignedTo { get; set; }
        
        [Required]
        public int DeptId { get; set; }
        
        [Required]
        public int ProjectId { get; set; }
    }

    public class UpdateTaskDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public int PriorityId { get; set; }
        public int StatusId { get; set; }
        public DateTime? DueDate { get; set; }
        public int? AssignedTo { get; set; }
        public int DeptId { get; set; }
        public int ProjectId { get; set; }
    }

    public class TaskCommentDto
    {
        public int CommentId { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTaskCommentDto
    {
        [Required]
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;
    }
}