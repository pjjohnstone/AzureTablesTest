module AzureTables

open Microsoft.Azure.Cosmos.Table

let insertOperation (entity: ITableEntity) =
  TableOperation.Insert(entity)

let retrieveOperation partitionKey rowKey =
  TableOperation.Retrieve<ITableEntity>(partitionKey, rowKey)

let executeOperation (table: CloudTable) operation: Result<TableResult, StorageException> =
  try
    Ok(table.Execute(operation))
  with
    | :? StorageException as ex -> Error(ex)