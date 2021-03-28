module PersonDto

open AzureTables
open Microsoft.Azure.Cosmos.Table

type T(firstName, lastName, email: string, phone: string) =
  inherit TableEntity(partitionKey=lastName, rowKey=firstName)
  new() = T(null, null, null, null)
  member val Email = email with get, set
  member val PhoneNumber = phone with get, set
  static member fromDynamicTableEntity (e: DynamicTableEntity) =
    T(e.RowKey, e.PartitionKey, e.Properties.["Email"].StringValue, e.Properties.["PhoneNumber"].StringValue)

let (<!>) x f =
  Result.map f x

let fromDomain (person: Person.T) =
  T(person.FirstName, person.LastName, person.Email, person.Phone)

let toDomain (r: Result<TableResult, StorageException>): Result<Person.T, StorageException> =
  match r with
  | Ok r ->
    let dto = T.fromDynamicTableEntity (r.Result :?> DynamicTableEntity)
    Ok({FirstName=dto.RowKey; LastName=dto.PartitionKey; Email=dto.Email; Phone=dto.PhoneNumber})
  | Error ex -> Error(ex)

let toDomain2 (d: T): Person.T =
  {FirstName=d.RowKey; LastName=d.PartitionKey; Email=d.Email; Phone=d.PhoneNumber}

let validateResult (r: Result<TableResult, StorageException>) =
  match r with
  | Ok o ->
    match o.Result with
    | null -> Error(StorageException("Result was null"))
    | _ -> Ok(o)
  | Error e -> Error(e)

let save (table: CloudTable) person: Result<TableResult, StorageException> =
  let executor = executeOperation table
  person
  |> fromDomain
  |> insertOperation
  |> executor

let upCastDtos (ds: T list) =
  List.map (fun d -> d :> ITableEntity) ds

let saveBatch (t: CloudTable) ps: Result<TableBatchResult, StorageException> =
  let executor = executeBatchOperation t
  ps
  |> List.map (fun p -> fromDomain p)
  |> upCastDtos
  |> batchInsertOperation
  |> executor

let personRetrieveOperation (p: Person.T) =
  let d = fromDomain p
  retrieveOperation (d :> ITableEntity)

let personDeleteOperation (r: Result<TableResult, StorageException>) =
  match r with
  | Ok p -> Ok(deleteOperation (p.Result :?> ITableEntity))
  | Error e -> Error(e)

let delete t p =
  let executor = executeOperation t
  p
  |> personRetrieveOperation
  |> executor
  |> validateResult
  |> personDeleteOperation
  <!> executor

let load (table: CloudTable) person: Result<Person.T, StorageException> =
  let executor = executeOperation table
  person
  |> personRetrieveOperation
  |> executor
  |> validateResult
  |> toDomain

let loadByKey t p k =
  let executor = executeQuery t
  retrievePropertyQuery p k
  |> executor
  <!> Seq.toList
  <!> List.map T.fromDynamicTableEntity
  <!> List.map toDomain2