using System.Text.Json.Serialization;

namespace AFFZ_Admin.Models;

public partial class Role
{
    [JsonPropertyName("roleId")]
    public int RoleId { get; set; }

    [JsonPropertyName("roleName")]
    public string RoleName { get; set; } = null!;

    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; }

    [JsonPropertyName("createdBy")]
    public int CreatedBy { get; set; }

    [JsonPropertyName("modifyDate")]
    public DateTime? ModifyDate { get; set; }

    [JsonPropertyName("modifiedBy")]
    public int? ModifiedBy { get; set; }

    [JsonPropertyName("permissions")]

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
