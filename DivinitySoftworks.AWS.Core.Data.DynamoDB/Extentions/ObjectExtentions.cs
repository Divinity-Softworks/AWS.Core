using Amazon.DynamoDBv2.Model;
using System.Reflection;

namespace System;

/// <summary>
/// Provides extension methods for the <see cref="object"/> type.
/// </summary>
public static class ObjectExtentions {

    /// <summary>
    /// Converts an object's properties to a dictionary of DynamoDB expression attribute values.
    /// </summary>
    /// <param name="parameters">The object containing the properties to convert.</param>
    /// <returns>A dictionary mapping attribute names to DynamoDB attribute values.</returns>
    /// <exception cref="NotSupportedException">Thrown when a property type is not supported.</exception>
    public static Dictionary<string, AttributeValue> ToExpressionAttributeValues(this object parameters) {
        Dictionary<string, AttributeValue> result = [];

        foreach (PropertyInfo property in parameters.GetType().GetProperties()) 
            result[$":{property.Name}"] = property.GetValue(parameters).ToAttributeValue();

        return result;
    }

    /// <summary>
    /// Converts an object to an AWS DynamoDB <see cref="AttributeValue"/>.
    /// </summary>
    /// <param name="value">The object to convert. Can be null.</param>
    /// <returns>
    /// An <see cref="AttributeValue"/> representing the given object.
    /// If the object is null, an <see cref="AttributeValue"/> with the NULL property set to true is returned.
    /// For strings, an <see cref="AttributeValue"/> with the S property set to the string value is returned.
    /// For numeric types (int, long, double, float, decimal), an <see cref="AttributeValue"/> with the N property set to the string representation of the number is returned.
    /// For boolean values, an <see cref="AttributeValue"/> with the BOOL property set to the boolean value is returned.
    /// For <see cref="DateTime"/> values, an <see cref="AttributeValue"/> with the N property set to the string representation of the DateTime ticks is returned.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown if the type of the object is not supported.
    /// </exception>
    public static AttributeValue ToAttributeValue(this object? value) {
        if (value is null) 
            return new AttributeValue { NULL = true };

        return value switch {
            string =>
                new AttributeValue { S = value.ToString() },
            int or long or double or float or decimal =>
                new AttributeValue { N = value.ToString() },
            bool =>
                new AttributeValue { BOOL = (bool)value },
            DateTime dateTime =>
                new AttributeValue { N = dateTime.Ticks.ToString() },
            _ => throw new NotSupportedException($"Type {value.GetType()} is not supported."),
        };
    }
}
