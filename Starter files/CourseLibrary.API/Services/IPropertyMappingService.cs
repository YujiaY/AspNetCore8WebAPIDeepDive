namespace CourseLibrary.API.Services;

public interface IPropertyMappingService
{
    public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();

}