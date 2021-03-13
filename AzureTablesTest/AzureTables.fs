module AzureTables

open Microsoft.Azure.Cosmos.Table

let insertOperation (e: ITableEntity) =
  TableOperation.InsertOrReplace(e)

let executeOperation (table: CloudTable) operation: Result<TableResult, StorageException> =
  try
    Ok(table.Execute(operation))
  with
    | :? StorageException as ex -> Error(ex)

let executeWrappedOperation (t: CloudTable) r: Result<TableResult, StorageException> =
  match r with
  | Ok o -> executeOperation t o
  | Error e -> Error(e)