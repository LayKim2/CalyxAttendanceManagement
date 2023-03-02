
using System.ComponentModel.DataAnnotations.Schema;

namespace CalyxAttendanceManagement.Shared.Model;

public class UserPTOHistory
{
    public int Id { get; set; }

    public int UserPTOId { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    public decimal Before { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string PTOType { get; set; } = string.Empty;
    public string CountType { get; set; } = string.Empty;
    public decimal Count { get; set; }
    public decimal Current { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime UpdatedTime { get; set; } = DateTime.Now;
    public string VerifiedType { get; set; } = "Pending";

    [NotMapped]
    public string Color { get; set; }
}
