using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CourseLibrary.API.Helpers;

public class ArrayModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        // Our binder works only on enumerable types
        if (!bindingContext.ModelMetadata.IsEnumerableType)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        // Get the inputted value through the value provider
        string value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();

        if (string.IsNullOrWhiteSpace(value))
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        // The value is not null or whitespace,
        // and the type of the model is enumerable.
        // Get the enumerable's type, and a converter
        TypeInfo elementTypeInfo = bindingContext.ModelType.GetTypeInfo();
        Type elementType = elementTypeInfo.GenericTypeArguments[0];
        TypeConverter converter = TypeDescriptor.GetConverter(elementType);

        // Convert each item in the value list to the enumerable type
        object?[] values = value.Split(new[] {","},
            StringSplitOptions.RemoveEmptyEntries)
            .Select(x => converter.ConvertFromString(x.Trim()))
            .ToArray();

        // Create an array of that type, and set it as the Model value
        Array typedValues = Array.CreateInstance(elementType, values.Length);
        values.CopyTo(typedValues, 0);
        bindingContext.Model = typedValues;

        // return a successful result, passing in the Model
        bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
        return Task.CompletedTask;

    }
}