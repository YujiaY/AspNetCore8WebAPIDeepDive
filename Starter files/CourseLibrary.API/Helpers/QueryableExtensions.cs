using CourseLibrary.API.Services;
using System.Linq.Dynamic.Core;
namespace CourseLibrary.API.Helpers;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        string orderBy,
        Dictionary<string, PropertyMappingValue> mappingDictionary)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        
        if (mappingDictionary == null)
        {
            throw new ArgumentNullException(nameof(mappingDictionary));
        }

        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return source;
        }
        
        string orderByString = string.Empty;
        
        // The orderBy string is separated by "," like "name desc , age asc", so we split it.
        var orderByAfterSplit = orderBy.Split(",");
        
        // apply each orderBy clause
        foreach (string orderByClause in orderByAfterSplit)
        {
            var trimmedOrderByClause = orderByClause.Trim();
            
            var orderDescending = trimmedOrderByClause.EndsWith(" desc");
            
            // remove " asc" or " desc" from the orderBy clause,so we
            // get the property name to look for in the mapping dictionary
            var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ", StringComparison.Ordinal);
            var propertyName = indexOfFirstSpace == -1 ?
                trimmedOrderByClause : trimmedOrderByClause.Remove(indexOfFirstSpace);
            
            // Find the matching property
            if (!mappingDictionary.ContainsKey(propertyName))
            {
                throw new ArgumentException($"Key mapping for {propertyName} is missing.");
            }
            
            // Get the PropertyMappingValue
            var propertyMappingValue = mappingDictionary[propertyName];

            if (propertyMappingValue == null)
            {
                throw new ArgumentNullException(nameof(propertyMappingValue));
            }
            
            // Revert the sort order if necessary
            if (propertyMappingValue.Revert)
            {
                orderDescending = !orderDescending;
            }
            
            // Run through the property names
            foreach (string destinationProperty in propertyMappingValue.DestinationProperties)
            {
                orderByString = orderByString +
                                (string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ", ") +
                                      destinationProperty +
                                      (orderDescending ? " descending" : " ascending");
            }
        }
            
        return source.OrderBy(orderByString);
    }
}