module AzureTables

open Microsoft.Azure.Cosmos.Table

let insertOperation (person: ITableEntity) =
  TableOperation.Insert(person)

let executeOperation (table: CloudTable) operation: Result<TableResult, StorageException> =
  try
    Ok(table.Execute(operation))
  with
    | :? StorageException as ex -> Error(ex)