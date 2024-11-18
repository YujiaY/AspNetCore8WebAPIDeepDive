using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;

namespace CourseLibrary.API.Services;

public class PropertyMappingService : IPropertyMappingService
{
    private readonly Dictionary<string, PropertyMappingValue> _authorPropertyMapping =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "Id", new(new[] { "Id" }) },
            { "MainCategory", new(new[] { "MainCategory" }) },
            { "Age", new(new[] { "DateOfBirth" }, true) },
            { "Name", new(new[] { "FirstName", "LastName" }) },
        };
    
    private readonly IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();
    // private readonly IPropertyMappingService _IPropertyMappingService;

    public PropertyMappingService()
    {
        _propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(_authorPropertyMapping));
    }

    public Dictionary<string, PropertyMappingValue> GetPropertyMapping
        <TSource, TDestination>()
    {
        // Get matching mapping
        List<PropertyMapping<TSource, TDestination>> matchingMapping = _propertyMappings
            .OfType<PropertyMapping<TSource, TDestination>>()
            .ToList();

        if (matchingMapping.Count == 1) return matchingMapping.First().MappingDictionary;

        throw new Exception($"Cannot find exact property mapping instance " +
                            $"for <{typeof(TSource)}, {typeof(TDestination)}>.");
    }

    public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
    {
        var propertyMapping = GetPropertyMapping<TSource, TDestination>();

        if (string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }
        
        // the string is separated by "," so we split it
        var fieldsAfterSplit = fields.Split(',');
        
        // run through the fields clause
        foreach (string field in fieldsAfterSplit)
        {
            // trim
            var fieldTrimmed = field.Trim();
            
            // remove everything after the first " ", if the fields
            // are coming from an orderBy string, this part must be ignored
            var indexOfFirstSpace = fieldTrimmed.IndexOf(" ", StringComparison.Ordinal);
            var propertyName = indexOfFirstSpace == -1 ?
                fieldTrimmed : fieldTrimmed.Remove(indexOfFirstSpace);
            
            // find the matching property
            if (!propertyMapping.ContainsKey(propertyName))
            {
                return false;
            }
        }
        
        return true;
    }

}