using System.ComponentModel.DataAnnotations;

namespace BarqTMS.API.DTOs
{
    public class ProjectDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ClientId { get; set; }
        public string? ClientName { get; set; }
        public int? TeamLeaderId { get; set; }
        public string? TeamLeaderName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TaskCount { get; set; }
    }

    public class CreateProjectDto
    {
        [Required]
        [StringLength(200)]
        public string ProjectName { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public int? ClientId { get; set; }
        public int? TeamLeaderId { get; set; }
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UpdateProjectDto
    {
        [Required]
        [StringLength(200)]
        public string ProjectName { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public int? ClientId { get; set; }
        public int? TeamLeaderId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}