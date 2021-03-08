module Person

open ResultBuilder
open Microsoft.Azure.Cosmos.Table

type T = {FirstName:string; LastName:string; Email:string; Phone:string}

type PersonDto(firstName, lastName, email: string, phone: string) =
  inherit TableEntity(partitionKey=lastName, rowKey=firstName)
  new() = PersonDto(null, null, null, null)
  member val Email = email with get, set
  member val PhoneNumber = phone with get, set

let fromDomain person =
  PersonDto(person.FirstName, person.LastName, person.Email, person.Phone)

let toDomain (dto: PersonDto) =
  result {
    return {FirstName=dto.RowKey; LastName=dto.PartitionKey; Email=dto.Email; Phone=dto.PhoneNumber}
  }     
   
let insertOperation (person: PersonDto) =
  TableOperation.Insert(person)

let save person (table: CloudTable): Result<TableResult, StorageException> =
  result {    
    return person
    |> fromDomain
    |> insertOperation
    |> table.Execute
  }
