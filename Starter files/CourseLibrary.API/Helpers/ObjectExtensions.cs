using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers;

public static class ObjectExtensions
{
    public static ExpandoObject ShapeData<TSource>(this TSource source,
        string? fields)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        
        var dataShapedObject = new ExpandoObject();

        if (string.IsNullOrWhiteSpace(fields))
        {
            // All public properties should be in the ExpandoObject
            PropertyInfo[] propertyInfos = typeof(TSource)
                .GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                // Get the value of the property on the source object
                object? propertyValue = propertyInfo.GetValue(source);
                
                // Add the field to the ExpandoObject
                ((IDictionary<string, object?>)dataShapedObject)
                    .Add(propertyInfo.Name, propertyValue);
            }
            
            return dataShapedObject;
        }
        
        // The fields are separated by "," so we split it
        var fieldsAfterSplit = fields.Split(',');

        foreach (string field in fieldsAfterSplit)
        {
            // trim
            string propertyName = field.Trim();

            // Use reflection to get the property on the source object
            // we need to include public and instance, b/c specifying a binding
            // flag overwrites the already-existing binding flags.
            PropertyInfo? propertyInfo = typeof(TSource)
                .GetProperty(propertyName,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo == null)
            {
                throw new Exception($"Property {propertyName} was not found on " +
                                    $"{typeof(TSource)}");
            }

            // Get the value of the property on the source object
            object? propertyValue = propertyInfo.GetValue(source);

            // Add the field to the ExpandoObject
            ((IDictionary<string, object?>)dataShapedObject)
                .Add(propertyInfo.Name, propertyValue);
        }

        // Return the shaped object
        return dataShapedObject;
    }
}