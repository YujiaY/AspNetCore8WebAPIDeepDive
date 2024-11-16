using System.ComponentModel.DataAnnotations;
using CourseLibrary.API.ValidationAttributes;

namespace CourseLibrary.API.Models;

[CourseTitleMustBeDifferentFromDescription]
public abstract class CourseForManipulationDto// : IValidatableObject
{
    [Required(ErrorMessage = "You should provide a title.")]
    [MaxLength(100, ErrorMessage = "The title should not exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1500, ErrorMessage = "The description should not exceed 1500 characters.")]
    public virtual string Description { get; set; } = string.Empty;

    // public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    // {
    //     if (Title == Description)
    //     {
    //         ValidationResult validationResults = new ValidationResult(
    //             "The provided description should be different from the title.",
    //             // new[] { nameof(Title), nameof(Description) });
    //             new[] { nameof(Course)});
    //         // new[] { "Course"});
    //         
    //         yield return validationResults;
    //     }
    // }
}