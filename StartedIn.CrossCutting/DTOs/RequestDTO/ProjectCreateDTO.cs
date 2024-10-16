using System.ComponentModel.DataAnnotations;

namespace StartedIn.CrossCutting.DTOs.RequestDTO;

public class ProjectCreateDTO
{
    [Required(ErrorMessage = "Vui lòng điền tên dự án")]
    public string ProjectName { get; set; }
    public string Description { get; set; }
}