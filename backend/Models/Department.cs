using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("DEPARTMENT")]
    public class Department
    {
        [Key]
        [Column("dept_id")]
        public int DeptId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("dept_name")]
        public string DeptName { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<UserDepartment> UserDepartments { get; set; } = new List<UserDepartment>();
        public virtual ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
    }
}