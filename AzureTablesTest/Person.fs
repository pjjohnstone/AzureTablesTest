module Person

open AzureTables
open Microsoft.Azure.Cosmos.Table

type T = {FirstName:string; LastName:string; Email:string; Phone:string}

type Dto(firstName, lastName, email: string, phone: string) =
  inherit TableEntity(partitionKey=lastName, rowKey=firstName)
  new() = Dto(null, null, null, null)
  member val Email = email with get, set
  member val PhoneNumber = phone with get, set

let fromDomain person =
  Dto(person.FirstName, person.LastName, person.Email, person.Phone)

let toDomain (r: Result<TableResult, StorageException>) =
  match r with
  | Ok r ->
    let dto = r.Result :?> Dto
    Ok({FirstName=dto.RowKey; LastName=dto.PartitionKey; Email=dto.Email; Phone=dto.PhoneNumber})
  | Error ex -> Error(ex)

let save person (table: CloudTable): Result<TableResult, StorageException> =
  let executor = executeOperation table
  person
  |> fromDomain
  |> insertOperation
  |> executor

let personRetrieveOperation person =
  let {FirstName=r;LastName=p} = person
  retrieveOperation p r

let load person (table: CloudTable): Result<T, StorageException> =
  let executor = executeOperation table
  person
  |> personRetrieveOperation
  |> executor
  |> toDomain
  