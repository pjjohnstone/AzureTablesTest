module AzureTables

open Microsoft.Azure.Cosmos.Table

let insertOperation (e: ITableEntity) =
  TableOperation.InsertOrReplace(e)

let executeOperation (table: CloudTable) operation: Result<TableResult, StorageException> =
  try
    Ok(table.Execute(operation))
  with
    | :? StorageException as ex -> Error(ex)