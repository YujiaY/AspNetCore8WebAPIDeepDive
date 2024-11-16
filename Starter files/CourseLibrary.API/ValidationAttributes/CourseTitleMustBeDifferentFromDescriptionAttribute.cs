using System.ComponentModel.DataAnnotations;
using CourseLibrary.API.Models;

namespace CourseLibrary.API.ValidationAttributes;

public class CourseTitleMustBeDifferentFromDescriptionAttribute() : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is not CourseForManipulationDto courseDto)
            throw new Exception($"Attribute {nameof(CourseTitleMustBeDifferentFromDescriptionAttribute)} " +
                                $"must be applied to a " +
                                $"{nameof(CourseForManipulationDto)} or derived type.");

        if (courseDto.Title == courseDto.Description)
            return new ValidationResult("The title cannot be the same as the description.",
                new[] { nameof(CourseForManipulationDto) });

        return ValidationResult.Success;
    }
}