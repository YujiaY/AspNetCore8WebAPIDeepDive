using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models;

public class CourseForUpdateDto : CourseForManipulationDto
{
    [Required(ErrorMessage = "You should provide a Description.")]
    [MaxLength(1500, ErrorMessage = "The description should not exceed 1500 characters.")]
    public override string Description
    {
        get => base.Description;
        set => base.Description = value;
    }
}