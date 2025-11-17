using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("USER_DEPARTMENTS")]
    public class UserDepartment
    {
        [Key]
        [Column("user_dept_id")]
        public int UserDeptId { get; set; }
        
        [Column("user_id")]
        public int UserId { get; set; }
        
        [Column("dept_id")]
        public int DeptId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        [ForeignKey("DeptId")]
        public virtual Department Department { get; set; } = null!;
    }
}