using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers;

public static class IEnumerableExtentions
{
    public static IEnumerable<ExpandoObject> ShapeData<TSource>(
        this IEnumerable<TSource> sources,
        string? fields)
    {
        if (sources == null)
        {
            throw new ArgumentNullException(nameof(sources));
        }
        
        // Create a list to hold our ExpandoObjects
        List<ExpandoObject> expandoObjectList = new List<ExpandoObject>();
        
        // Create a list with PropertyInfo objects on TSource.
        // Reflection is expensive,so rather than doing it for each object in the List, we do
        // it once and reuse the results. After all, part of the reflection is on
        // type of the object （TSource），not on the instance
        List<PropertyInfo> propertyInfoList = new List<PropertyInfo>();

        if (string.IsNullOrWhiteSpace(fields))
        {
            // All public properties should be in the ExpandoObject
            PropertyInfo[] propertyInfos = typeof(TSource)
                .GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            
            propertyInfoList.AddRange(propertyInfos);
        }
        else
        {
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
                
                // Add propertyInfo to list
                propertyInfoList.Add(propertyInfo);
            }
        }
        
        // Run through the source objects
        foreach (TSource sourceObject in sources)
        {
            // Create an ExpandoObject that will hold the 
            // selected properties & values
            var dataShapedObject = new ExpandoObject();
            
            // Get the value of each property we have to return.
            // For that, we run through the list
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                // GetValue returns the value of the property on the source object
                var propertyValue = propertyInfo.GetValue(sourceObject);
                
                // Add the field to the ExpandoObject
                ((IDictionary<string, object?>)dataShapedObject)
                    .Add(propertyInfo.Name, propertyValue);
            }
            
            // Add the ExpandoObject to the list
            expandoObjectList.Add(dataShapedObject);
        }
        
        // Return the list
        return expandoObjectList;
    }
}