module Person

open ResultBuilder
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

let toDomain (dto: Dto) =
  result {
    return {FirstName=dto.RowKey; LastName=dto.PartitionKey; Email=dto.Email; Phone=dto.PhoneNumber}
  }

let save person (table: CloudTable): Result<TableResult, StorageException> =
  let executor = executeOperation table
  person
  |> fromDomain
  |> insertOperation
  |> executor
