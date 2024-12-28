using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System.Net;
using System.Text.Json;

namespace Amazon.DynamoDBv2;

/// <summary>
/// Provides extension methods for the <see cref="IAmazonDynamoDB"/> interface.
/// </summary>
public static class IAmazonDynamoDBExtentions {

    /// <summary>
    /// Creates a new item. If an item that has the same primary key as the new item already 
    /// exists in the specified table, the request will be rejected with a 
    /// <c>ValidationException</c> exception.
    /// 
    /// <para>
    /// When you add an item, the primary key attributes are the only required attributes.
    /// </para>
    ///  
    /// <para>
    /// Empty String and Binary attribute values are allowed. Attribute values of type String
    /// and Binary must have a length greater than zero if the attribute is used as a key
    /// attribute for a table or index. Set type attributes cannot be empty. 
    /// </para>
    ///  
    /// <para>
    /// Invalid Requests with empty values will be rejected with a <c>ValidationException</c>
    /// exception.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of item to create.</typeparam>
    /// <param name="amazonDynamoDB">Interface for accessing DynamoDB.</param>
    /// <param name="tableName">The name of the table to contain the item. You can also 
    /// provide the Amazon Resource Name (ARN) of the table in this parameter.</param>
    /// <param name="item">The item to create.</param>
    /// <param name="keyName">The name of the table its <c>[Partition key]</c>. 
    /// 
    /// <para>
    /// The partition key is part of the table's primary key. It is a hash value that 
    /// is used to retrieve items from your table and allocate data across hosts for 
    /// scalability and availability.
    /// </para>
    /// 
    /// </param>
    /// <param name="sortName">The name of the table its <c>[Sort key]</c>. 
    /// 
    /// <para>
    /// You can use a sort key as the second part of a table's primary key. The sort key 
    /// allows you to sort or search among all items sharing the same partition key.
    /// </para>
    /// 
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used by other objects or threads to receive notice of cancellation.
    /// </param>
    /// <returns>A <c>bool</c>, indicating if the item was created.</returns>
    /// <exception cref="ConditionalCheckFailedException">
    /// A condition specified in the operation could not be evaluated.
    /// </exception>
    /// <exception cref="InternalServerErrorException">
    /// An error occurred on the server side.
    /// </exception>
    /// <exception cref="ItemCollectionSizeLimitExceededException">
    /// An item collection is too large. This exception is only returned for tables that have
    /// one or more local secondary indexes.
    /// </exception>
    /// <exception cref="ProvisionedThroughputExceededException">
    /// Your request rate is too high. The Amazon Web Services SDKs for DynamoDB automatically
    /// retry requests that receive this exception. Your request is eventually successful,
    /// unless your retry queue is too large to finish. Reduce the frequency of requests and
    /// use exponential backoff. For more information, go to <a href="https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Programming.Errors.html#Programming.Errors.RetryAndBackoff">Error
    /// Retries and Exponential Backoff</a> in the <i>Amazon DynamoDB Developer Guide</i>.
    /// </exception>
    /// <exception cref="RequestLimitExceededException">
    /// Throughput exceeds the current throughput quota for your account. Please contact <a
    /// href="https://aws.amazon.com/support">Amazon Web Services Support</a> to request a
    /// quota increase.
    /// </exception>
    /// <exception cref="ResourceNotFoundException">
    /// The operation tried to access a nonexistent table or index. The resource might not
    /// be specified correctly, or its status might not be <c>ACTIVE</c>.
    /// </exception>
    /// <exception cref="TransactionConflictException">
    /// Operation was rejected because there is an ongoing transaction for the item.
    /// </exception>
    public static async Task<bool> CreateItemAsync<T>(this IAmazonDynamoDB amazonDynamoDB, string tableName, T item, string keyName = "PK", string sortName = "SK", CancellationToken cancellationToken = default) where T : class {
        string itemAsJson = JsonSerializer.Serialize(item);
        Document itemAsDocument = Document.FromJson(itemAsJson);

        string conditionExpression = $"attribute_not_exists({keyName})";
        if (item.GetType().GetProperty(sortName) is not null)
            conditionExpression += $" AND attribute_not_exists({sortName})";

        PutItemRequest request = new() {
            TableName = tableName,
            Item = itemAsDocument.ToAttributeMap(),
            ConditionExpression = conditionExpression
        };

        PutItemResponse response = await amazonDynamoDB.PutItemAsync(request, cancellationToken);

        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    /// <summary>
    /// The <c>GetItem</c> operation returns a set of attributes for the item with the given
    /// primary key. If there is no matching item, <c>GetItem</c> does not return any data
    /// and there will be no <c>Item</c> element in the response.
    /// 
    ///  
    /// <para>
    /// <c>GetItem</c> provides an eventually consistent read by default. If your application
    /// requires a strongly consistent read, set <c>ConsistentRead</c> to <c>true</c>. Although
    /// a strongly consistent read might take more time than an eventually consistent read,
    /// it always returns the last updated value.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of item to get.</typeparam>
    /// <param name="amazonDynamoDB">Interface for accessing DynamoDB.</param>
    /// <param name="tableName">The name of the table containing the requested item. You can also provide the Amazon
    /// Resource Name (ARN) of the table in this parameter.</param>
    /// <param name="pk">The partition key is part of the table's primary key.</param>
    /// <param name="sk">You can use a sort key as the second part of a table's primary 
    /// key.</param>
    /// <param name="keyName">The name of the table its <c>[Partition key]</c>. 
    /// 
    /// <para>
    /// The partition key is part of the table's primary key. It is a hash value that 
    /// is used to retrieve items from your table and allocate data across hosts for 
    /// scalability and availability.
    /// </para>
    /// 
    /// </param>
    /// <param name="sortName">The name of the table its <c>[Sort key]</c>. 
    /// 
    /// <para>
    /// You can use a sort key as the second part of a table's primary key. The sort key 
    /// allows you to sort or search among all items sharing the same partition key.
    /// </para>
    /// 
    /// </param>
    /// <param name="consistentRead">Determines the read consistency model: If set to <c>true</c>, 
    /// then the operation uses strongly consistent reads; otherwise, the operation uses eventually 
    /// consistent reads.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used by other objects or threads to receive notice of cancellation.
    /// </param>
    /// <returns>The requested item.</returns>
    /// <exception cref="InternalServerErrorException">
    /// An error occurred on the server side.
    /// </exception>
    /// <exception cref="ProvisionedThroughputExceededException">
    /// Your request rate is too high. The Amazon Web Services SDKs for DynamoDB automatically
    /// retry requests that receive this exception. Your request is eventually successful,
    /// unless your retry queue is too large to finish. Reduce the frequency of requests and
    /// use exponential backoff. For more information, go to <a href="https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Programming.Errors.html#Programming.Errors.RetryAndBackoff">Error
    /// Retries and Exponential Backoff</a> in the <i>Amazon DynamoDB Developer Guide</i>.
    /// </exception>
    /// <exception cref="RequestLimitExceededException">
    /// Throughput exceeds the current throughput quota for your account. Please contact <a
    /// href="https://aws.amazon.com/support">Amazon Web Services Support</a> to request a
    /// quota increase.
    /// </exception>
    /// <exception cref="ResourceNotFoundException">
    /// The operation tried to access a nonexistent table or index. The resource might not
    /// be specified correctly, or its status might not be <c>ACTIVE</c>.
    /// </exception>
    public static async Task<T?> GetItemAsync<T>(this IAmazonDynamoDB amazonDynamoDB, string tableName, object pk, object? sk = null, string keyName = "PK", string sortName = "SK", bool consistentRead = false, CancellationToken cancellationToken = default) {
        GetItemRequest request = new() {
            TableName = tableName,
            Key = new Dictionary<string, AttributeValue> {
                { keyName, pk.ToAttributeValue() }
            },
            ConsistentRead = consistentRead,
        };

        if (sk is not null)
            request.Key.Add(sortName, sk.ToAttributeValue());

        GetItemResponse response = await amazonDynamoDB.GetItemAsync(request, cancellationToken);

        if (response.Item.Count == 0)
            return default;

        Document itemAsDocument = Document.FromAttributeMap(response.Item);

        return JsonSerializer.Deserialize<T>(itemAsDocument.ToJson());
    }

    /// <summary>
    /// Creates a new item, or replaces an old item with a new item. If an item that has the
    /// same primary key as the new item already exists in the specified table, the new item
    /// completely replaces the existing item. 
    ///  
    /// <para>
    /// When you add an item, the primary key attributes are the only required attributes.
    /// </para>
    ///  
    /// <para>
    /// Empty String and Binary attribute values are allowed. Attribute values of type String
    /// and Binary must have a length greater than zero if the attribute is used as a key
    /// attribute for a table or index. Set type attributes cannot be empty. 
    /// </para>
    ///  
    /// <para>
    /// Invalid Requests with empty values will be rejected with a <c>ValidationException</c>
    /// exception.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of item to create or update.</typeparam>
    /// <param name="amazonDynamoDB">Interface for accessing DynamoDB.</param>
    /// <param name="tableName">The name of the table to contain the item. You can also provide the Amazon Resource
    /// Name (ARN) of the table in this parameter.</param>
    /// <param name="item">The item to create.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used by other objects or threads to receive notice of cancellation.
    /// </param>
    /// <returns>A <c>bool</c>, indicating if the item was created or updated.</returns>
    /// <exception cref="InternalServerErrorException">
    /// An error occurred on the server side.
    /// </exception>
    /// <exception cref="ItemCollectionSizeLimitExceededException">
    /// An item collection is too large. This exception is only returned for tables that have
    /// one or more local secondary indexes.
    /// </exception>
    /// <exception cref="ProvisionedThroughputExceededException">
    /// Your request rate is too high. The Amazon Web Services SDKs for DynamoDB automatically
    /// retry requests that receive this exception. Your request is eventually successful,
    /// unless your retry queue is too large to finish. Reduce the frequency of requests and
    /// use exponential backoff. For more information, go to <a href="https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Programming.Errors.html#Programming.Errors.RetryAndBackoff">Error
    /// Retries and Exponential Backoff</a> in the <i>Amazon DynamoDB Developer Guide</i>.
    /// </exception>
    /// <exception cref="RequestLimitExceededException">
    /// Throughput exceeds the current throughput quota for your account. Please contact <a
    /// href="https://aws.amazon.com/support">Amazon Web Services Support</a> to request a
    /// quota increase.
    /// </exception>
    /// <exception cref="ResourceNotFoundException">
    /// The operation tried to access a nonexistent table or index. The resource might not
    /// be specified correctly, or its status might not be <c>ACTIVE</c>.
    /// </exception>
    /// <exception cref="TransactionConflictException">
    /// Operation was rejected because there is an ongoing transaction for the item.
    /// </exception>
    public static async Task<bool> PutItemAsync<T>(this IAmazonDynamoDB amazonDynamoDB, string tableName, T item, CancellationToken cancellationToken = default) {
        string itemAsJson = JsonSerializer.Serialize(item);
        Document itemAsDocument = Document.FromJson(itemAsJson);

        PutItemRequest request = new() {
            TableName = tableName,
            Item = itemAsDocument.ToAttributeMap()
        };

        PutItemResponse response = await amazonDynamoDB.PutItemAsync(request, cancellationToken);

        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    /// <summary>
    /// Deletes a single item in a table by primary key. 
    ///  
    /// <para>
    /// In addition to deleting an item, you can also return the item's attribute values in
    /// the same operation, using the <c>ReturnValues</c> parameter.
    /// </para>
    ///  
    /// <para>
    /// Unless you specify conditions, the <c>DeleteItem</c> is an idempotent operation; running
    /// it multiple times on the same item or attribute does <i>not</i> result in an error
    /// response.
    /// </para>
    ///  
    /// <para>
    /// Conditional deletes are useful for deleting items only if specific conditions are
    /// met. If those conditions are met, DynamoDB performs the delete. Otherwise, the item
    /// is not deleted.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of item to get.</typeparam>
    /// <param name="amazonDynamoDB">Interface for accessing DynamoDB.</param>
    /// <param name="tableName">The name of the table from which to delete the item. You can also provide the Amazon
    /// Resource Name (ARN) of the table in this parameter.</param>
    /// <param name="pk">The partition key is part of the table's primary key.</param>
    /// <param name="sk">You can use a sort key as the second part of a table's primary 
    /// key.</param>
    /// <param name="keyName">The name of the table its <c>[Partition key]</c>. 
    /// 
    /// <para>
    /// The partition key is part of the table's primary key. It is a hash value that 
    /// is used to retrieve items from your table and allocate data across hosts for 
    /// scalability and availability.
    /// </para>
    /// 
    /// </param>
    /// <param name="sortName">The name of the table its <c>[Sort key]</c>. 
    /// 
    /// <para>
    /// You can use a sort key as the second part of a table's primary key. The sort key 
    /// allows you to sort or search among all items sharing the same partition key.
    /// </para>
    /// 
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used by other objects or threads to receive notice of cancellation.
    /// </param>
    /// 
    /// <returns>A <c>bool</c>, indicating if the item was created or updated.</returns>
    /// <exception cref="InternalServerErrorException">
    /// An error occurred on the server side.
    /// </exception>
    /// <exception cref="ItemCollectionSizeLimitExceededException">
    /// An item collection is too large. This exception is only returned for tables that have
    /// one or more local secondary indexes.
    /// </exception>
    /// <exception cref="ProvisionedThroughputExceededException">
    /// Your request rate is too high. The Amazon Web Services SDKs for DynamoDB automatically
    /// retry requests that receive this exception. Your request is eventually successful,
    /// unless your retry queue is too large to finish. Reduce the frequency of requests and
    /// use exponential backoff. For more information, go to <a href="https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Programming.Errors.html#Programming.Errors.RetryAndBackoff">Error
    /// Retries and Exponential Backoff</a> in the <i>Amazon DynamoDB Developer Guide</i>.
    /// </exception>
    /// <exception cref="RequestLimitExceededException">
    /// Throughput exceeds the current throughput quota for your account. Please contact <a
    /// href="https://aws.amazon.com/support">Amazon Web Services Support</a> to request a
    /// quota increase.
    /// </exception>
    /// <exception cref="ResourceNotFoundException">
    /// The operation tried to access a nonexistent table or index. The resource might not
    /// be specified correctly, or its status might not be <c>ACTIVE</c>.
    /// </exception>
    /// <exception cref="TransactionConflictException">
    /// Operation was rejected because there is an ongoing transaction for the item.
    /// </exception>
    public static async Task<bool> DeleteItemAsync(this IAmazonDynamoDB amazonDynamoDB, string tableName, object pk, object? sk = null, string keyName = "PK", string sortName = "SK", CancellationToken cancellationToken = default) {
        DeleteItemRequest request = new() {
            TableName = tableName,
            Key = new Dictionary<string, AttributeValue> {
                { keyName, pk.ToAttributeValue() }
            }
        };

        if (sk is not null)
            request.Key.Add(sortName, sk.ToAttributeValue());

        DeleteItemResponse response = await amazonDynamoDB.DeleteItemAsync(request, cancellationToken);

        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    /// <summary>
    /// The <c>Scan</c> operation returns one or more items and item attributes by accessing
    /// every item in a table or a secondary index. To have DynamoDB return fewer items, you
    /// can provide a <c>FilterExpression</c> operation.
    /// 
    ///  
    /// <para>
    /// If the total size of scanned items exceeds the maximum dataset size limit of 1 MB,
    /// the scan completes and results are returned to the user. The <c>LastEvaluatedKey</c>
    /// value is also returned and the requestor can use the <c>LastEvaluatedKey</c> to continue
    /// the scan in a subsequent operation. Each scan response also includes number of items
    /// that were scanned (ScannedCount) as part of the request. If using a <c>FilterExpression</c>,
    /// a scan result can result in no items meeting the criteria and the <c>Count</c> will
    /// result in zero. If you did not use a <c>FilterExpression</c> in the scan request,
    /// then <c>Count</c> is the same as <c>ScannedCount</c>.
    /// </para>
    ///  <note> 
    /// <para>
    ///  <c>Count</c> and <c>ScannedCount</c> only return the count of items specific to a
    /// single scan request and, unless the table is less than 1MB, do not represent the total
    /// number of items in the table. 
    /// </para>
    ///  </note> 
    /// <para>
    /// A single <c>Scan</c> operation first reads up to the maximum number of items set (if
    /// using the <c>Limit</c> parameter) or a maximum of 1 MB of data and then applies any
    /// filtering to the results if a <c>FilterExpression</c> is provided. If <c>LastEvaluatedKey</c>
    /// is present in the response, pagination is required to complete the full table scan.
    /// For more information, see <a href="https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Scan.html#Scan.Pagination">Paginating
    /// the Results</a> in the <i>Amazon DynamoDB Developer Guide</i>.
    /// </para>
    ///  
    /// <para>
    ///  <c>Scan</c> operations proceed sequentially; however, for faster performance on a
    /// large table or secondary index, applications can request a parallel <c>Scan</c> operation
    /// by providing the <c>Segment</c> and <c>TotalSegments</c> parameters. For more information,
    /// see <a href="https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Scan.html#Scan.ParallelScan">Parallel
    /// Scan</a> in the <i>Amazon DynamoDB Developer Guide</i>.
    /// </para>
    ///  
    /// <para>
    /// By default, a <c>Scan</c> uses eventually consistent reads when accessing the items
    /// in a table. Therefore, the results from an eventually consistent <c>Scan</c> may not
    /// include the latest item changes at the time the scan iterates through each item in
    /// the table. If you require a strongly consistent read of each item as the scan iterates
    /// through the items in the table, you can set the <c>ConsistentRead</c> parameter to
    /// true. Strong consistency only relates to the consistency of the read at the item level.
    /// </para>
    ///  <note> 
    /// <para>
    ///  DynamoDB does not provide snapshot isolation for a scan operation when the <c>ConsistentRead</c>
    /// parameter is set to true. Thus, a DynamoDB scan operation does not guarantee that
    /// all reads in a scan see a consistent snapshot of the table when the scan operation
    /// was requested. 
    /// </para>
    ///  </note>
    /// </summary>
    /// <param name="amazonDynamoDB">The DynamoDB client.</param>
    /// <param name="tableName">The name of the table to scan.</param>
    /// <param name="filterExpression">The filter expression to apply to the scan.</param>
    /// <param name="parameters">An object containing the values for the expression attributes.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used by other objects or threads to receive notice of cancellation.
    /// </param>
    /// 
    /// <returns>The response from the Scan service method, as returned by DynamoDB.</returns>
    /// <exception cref="InternalServerErrorException">
    /// An error occurred on the server side.
    /// </exception>
    /// <exception cref="ProvisionedThroughputExceededException">
    /// Your request rate is too high. The Amazon Web Services SDKs for DynamoDB automatically
    /// retry requests that receive this exception. Your request is eventually successful,
    /// unless your retry queue is too large to finish. Reduce the frequency of requests and
    /// use exponential backoff. For more information, go to <a href="https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Programming.Errors.html#Programming.Errors.RetryAndBackoff">Error
    /// Retries and Exponential Backoff</a> in the <i>Amazon DynamoDB Developer Guide</i>.
    /// </exception>
    /// <exception cref="RequestLimitExceededException">
    /// Throughput exceeds the current throughput quota for your account. Please contact <a
    /// href="https://aws.amazon.com/support">Amazon Web Services Support</a> to request a
    /// quota increase.
    /// </exception>
    /// <exception cref="ResourceNotFoundException">
    /// The operation tried to access a nonexistent table or index. The resource might not
    /// be specified correctly, or its status might not be <c>ACTIVE</c>.
    /// </exception>
    public static async Task<List<T>> ScanAsync<T>(this IAmazonDynamoDB amazonDynamoDB, string tableName, string filterExpression, object? parameters, CancellationToken cancellationToken = default) {
        List<T> result = [];

        ScanRequest request = new() {
            TableName = tableName,
            FilterExpression = filterExpression,
        };

        if (parameters is not null)
            request.ExpressionAttributeValues = parameters.ToExpressionAttributeValues();

        ScanResponse? response = await amazonDynamoDB.ScanAsync(request, cancellationToken);

        if (response is null || response.Items.Count == 0)
            return result;

        foreach (Dictionary<string, AttributeValue>? item in response.Items) {
            Document itemAsDocument = Document.FromAttributeMap(item);

            result.Add(JsonSerializer.Deserialize<T>(itemAsDocument.ToJson())!);
        }

        return result;
    }

    /// <summary>
    /// You must provide the name of the partition key attribute and a single value for that
    /// attribute. <c>Query</c> returns all items with that partition key value. Optionally,
    /// you can provide a sort key attribute and use a comparison operator to refine the search
    /// results.
    /// 
    ///  
    /// <para>
    /// Use the <c>KeyConditionExpression</c> parameter to provide a specific value for the
    /// partition key. The <c>Query</c> operation will return all of the items from the table
    /// or index with that partition key value. You can optionally narrow the scope of the
    /// <c>Query</c> operation by specifying a sort key value and a comparison operator in
    /// <c>KeyConditionExpression</c>. To further refine the <c>Query</c> results, you can
    /// optionally provide a <c>FilterExpression</c>. A <c>FilterExpression</c> determines
    /// which items within the results should be returned to you. All of the other results
    /// are discarded. 
    /// </para>
    ///  
    /// <para>
    ///  A <c>Query</c> operation always returns a result set. If no matching items are found,
    /// the result set will be empty. Queries that do not return results consume the minimum
    /// number of read capacity units for that type of read operation. 
    /// </para>
    ///  <note> 
    /// <para>
    ///  DynamoDB calculates the number of read capacity units consumed based on item size,
    /// not on the amount of data that is returned to an application. The number of capacity
    /// units consumed will be the same whether you request all of the attributes (the default
    /// behavior) or just some of them (using a projection expression). The number will also
    /// be the same whether or not you use a <c>FilterExpression</c>. 
    /// </para>
    ///  </note> 
    /// <para>
    ///  <c>Query</c> results are always sorted by the sort key value. If the data type of
    /// the sort key is Number, the results are returned in numeric order; otherwise, the
    /// results are returned in order of UTF-8 bytes. By default, the sort order is ascending.
    /// To reverse the order, set the <c>ScanIndexForward</c> parameter to false. 
    /// </para>
    ///  
    /// <para>
    ///  A single <c>Query</c> operation will read up to the maximum number of items set (if
    /// using the <c>Limit</c> parameter) or a maximum of 1 MB of data and then apply any
    /// filtering to the results using <c>FilterExpression</c>. If <c>LastEvaluatedKey</c>
    /// is present in the response, you will need to paginate the result set. For more information,
    /// see <a href="https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Query.html#Query.Pagination">Paginating
    /// the Results</a> in the <i>Amazon DynamoDB Developer Guide</i>. 
    /// </para>
    ///  
    /// <para>
    ///  <c>FilterExpression</c> is applied after a <c>Query</c> finishes, but before the
    /// results are returned. A <c>FilterExpression</c> cannot contain partition key or sort
    /// key attributes. You need to specify those attributes in the <c>KeyConditionExpression</c>.
    /// 
    /// </para>
    ///  <note> 
    /// <para>
    ///  A <c>Query</c> operation can return an empty result set and a <c>LastEvaluatedKey</c>
    /// if all the items read for the page of results are filtered out. 
    /// </para>
    ///  </note> 
    /// <para>
    /// You can query a table, a local secondary index, or a global secondary index. For a
    /// query on a table or on a local secondary index, you can set the <c>ConsistentRead</c>
    /// parameter to <c>true</c> and obtain a strongly consistent result. Global secondary
    /// indexes support eventually consistent reads only, so do not specify <c>ConsistentRead</c>
    /// when querying a global secondary index.
    /// </para>
    /// </summary>
    /// <param name="amazonDynamoDB">The DynamoDB client.</param>
    /// <param name="tableName">The name of the table to query.</param>
    /// <param name="filterExpression">The filter expression to apply to the query.</param>
    /// <param name="parameters">An object containing the values for the expression attributes.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used by other objects or threads to receive notice of cancellation.
    /// </param>
    /// 
    /// <returns>The response from the Query service method, as returned by DynamoDB.</returns>
    /// <exception cref="InternalServerErrorException">
    /// An error occurred on the server side.
    /// </exception>
    /// <exception cref="ProvisionedThroughputExceededException">
    /// Your request rate is too high. The Amazon Web Services SDKs for DynamoDB automatically
    /// retry requests that receive this exception. Your request is eventually successful,
    /// unless your retry queue is too large to finish. Reduce the frequency of requests and
    /// use exponential backoff. For more information, go to <a href="https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Programming.Errors.html#Programming.Errors.RetryAndBackoff">Error
    /// Retries and Exponential Backoff</a> in the <i>Amazon DynamoDB Developer Guide</i>.
    /// </exception>
    /// <exception cref="RequestLimitExceededException">
    /// Throughput exceeds the current throughput quota for your account. Please contact <a
    /// href="https://aws.amazon.com/support">Amazon Web Services Support</a> to request a
    /// quota increase.
    /// </exception>
    /// <exception cref="ResourceNotFoundException">
    /// The operation tried to access a nonexistent table or index. The resource might not
    /// be specified correctly, or its status might not be <c>ACTIVE</c>.
    /// </exception>
    /// <seealso href="http://docs.aws.amazon.com/goto/WebAPI/dynamodb-2012-08-10/Query">REST API Reference for Query Operation</seealso>
    public static async Task<List<T>> QueryAsync<T>(this IAmazonDynamoDB amazonDynamoDB, string tableName, string keyConditionExpression, object? parameters, CancellationToken cancellationToken = default) {
        List<T> result = [];

        QueryRequest request = new() {
            TableName = tableName,
            KeyConditionExpression = keyConditionExpression
        };

        if (parameters is not null)
            request.ExpressionAttributeValues = parameters.ToExpressionAttributeValues();

        QueryResponse? response = await amazonDynamoDB.QueryAsync(request, cancellationToken);

        if (response is null || response.Items.Count == 0)
            return result;

        foreach (Dictionary<string, AttributeValue>? item in response.Items) {
            Document itemAsDocument = Document.FromAttributeMap(item);

            result.Add(JsonSerializer.Deserialize<T>(itemAsDocument.ToJson())!);
        }

        return result;
    }
}
