using System.Text.Json.Serialization;

namespace DivinitySoftworks.AWS.Core.Data.DynamoDB;

/// <summary>
/// Represents a generic record for DynamoDB, with a primary key (PK) of type <typeparamref name="T"/>.
/// This class includes functionality to mark the record as the latest version.
/// </summary>
/// <typeparam name="T">The type of the primary key.</typeparam>
public record DynamoDBRecord<T> {

    /// <summary>
    /// Gets or sets the primary key (PK) for the record.
    /// </summary>
    [JsonPropertyName("PK")]
    public virtual required T PK { get; set; }

    /// <summary>
    /// Gets a value indicating whether this record is the latest version.
    /// </summary>
    [JsonIgnore]
    public bool IsLatest { get; protected set; }

    /// <summary>
    /// Marks this record as the latest version.
    /// </summary>
    public void MarkAsLatest() {
        IsLatest = true;
    }
}
