using System.Reflection;

namespace CourseLibrary.API.Services;

public class PropertyCheckerService : IPropertyCheckerService
{
    public bool TypeHasProperties<T>(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }
        
        // The fields are separated by "," so we split it.
        var fieldsAfterSplit = fields.Split(",");
        
        // Check if the requested fields exist on the source
        foreach (string field in fieldsAfterSplit)
        {
            // trim
            var propertyName = field.Trim();
            
            // Use Reflection to check if the
            // property can be found on T.
            var propertyInfo = typeof(T).GetProperty(propertyName,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            
            // It cannot be found, return false
            if (propertyInfo == null)
            {
                return false;
            }
        }   
        
        // All check out, return true.
        return true;
    }
}