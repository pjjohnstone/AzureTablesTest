module AzureTables

open Microsoft.Azure.Cosmos.Table

let insertOperation (e: ITableEntity) =
  TableOperation.InsertOrReplace(e)

let retrieveOperation (e: ITableEntity) =
  TableOperation.Retrieve(e.PartitionKey, e.RowKey)

let deleteOperation (e: ITableEntity) =
  TableOperation.Delete(e)

let batchInsertOperation es =
  let batchOp = TableBatchOperation()
  es |> List.iter (fun i -> batchOp.InsertOrReplace(i))
  batchOp

let executeOperation (table: CloudTable) operation: Result<TableResult, StorageException> =
  try
    Ok(table.Execute(operation))
  with
    | :? StorageException as ex -> Error(ex)

let executeBatchOperation (t: CloudTable) o: Result<TableBatchResult, StorageException> =
  try
    Ok(t.ExecuteBatch(o))
  with
    | :? StorageException as ex -> Error(ex)