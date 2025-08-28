namespace AFFZ_API.Models;

public partial class AdminUser
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? RoleId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? ModifyDate { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual Role? Role { get; set; }
}
