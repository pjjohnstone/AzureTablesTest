﻿module Person

open AzureTables
open Microsoft.Azure.Cosmos.Table

type T = {FirstName:string; LastName:string; Email:string; Phone:string}

type Dto(firstName, lastName, email: string, phone: string) =
  inherit TableEntity(partitionKey=lastName, rowKey=firstName)
  new() = Dto(null, null, null, null)
  member val Email = email with get, set
  member val PhoneNumber = phone with get, set
  static member fromDynamicTableEntity (e: DynamicTableEntity) =
    Dto(e.RowKey, e.PartitionKey, e.Properties.["Email"].StringValue, e.Properties.["PhoneNumber"].StringValue)

let validateResult (r: Result<TableResult, StorageException>) =
  match r with
  | Ok o ->
    match o.Result with
    | null -> Error(StorageException("Result was null"))
    | _ -> Ok(o)
  | Error e -> Error(e)

let fromDomain person =
  Dto(person.FirstName, person.LastName, person.Email, person.Phone)

let toDomain (r: Result<TableResult, StorageException>) =
  match r with
  | Ok r ->
    let dto = Dto.fromDynamicTableEntity (r.Result :?> DynamicTableEntity)
    Ok({FirstName=dto.RowKey; LastName=dto.PartitionKey; Email=dto.Email; Phone=dto.PhoneNumber})
  | Error ex -> Error(ex)

let save (table: CloudTable) person: Result<TableResult, StorageException> =
  let executor = executeOperation table
  person
  |> fromDomain
  |> insertOperation
  |> executor

let upCastDtos (ds: Dto list) =
  List.map (fun d -> d :> ITableEntity) ds

let saveBatch (t: CloudTable) ps: Result<TableBatchResult, StorageException> =
  let executor = executeBatchOperation t
  ps
  |> List.map (fun p -> fromDomain p)
  |> upCastDtos
  |> batchInsertOperation
  |> executor

let personRetrieveOperation (p: T) =
  let d = fromDomain p
  retrieveOperation (d :> ITableEntity)

let personDeleteOperation (r: Result<TableResult, StorageException>) =
  match r with
  | Ok p -> Ok(deleteOperation (p.Result :?> ITableEntity))
  | Error e -> Error(e)

let delete t p =
  let executor = executeOperation t
  let resultExecutor = executeWrappedOperation t
  p
  |> personRetrieveOperation
  |> executor
  |> validateResult
  |> personDeleteOperation
  |> resultExecutor

let load (table: CloudTable) person: Result<T, StorageException> =
  let executor = executeOperation table
  person
  |> personRetrieveOperation
  |> executor
  |> validateResult
  |> toDomain
  