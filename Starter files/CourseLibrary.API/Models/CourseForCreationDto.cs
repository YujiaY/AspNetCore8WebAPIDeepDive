using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models;

public class CourseForCreationDto
{
    [Required(ErrorMessage = "You should provide a title.")]
    [MaxLength(100, ErrorMessage = "The title should not exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    // [Required]
    [MaxLength(1500, ErrorMessage = "The description should not exceed 1500 characters.")]
    public string Description { get; set; } = string.Empty;
}