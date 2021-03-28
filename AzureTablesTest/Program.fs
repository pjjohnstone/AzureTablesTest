open System.IO
open Microsoft.Azure.Cosmos.Table

[<EntryPoint>]
let main argv =
    let configPath = Path.Join(Directory.GetCurrentDirectory(), "config.json")
    let config = Config.load configPath
    let connectionString =
      let value = Config.getValue config "tableStorageKey"
      match value with
      | Some v -> v.Value
      | None -> "UseDevelopmentStorage=true"
    
    let storageAccount = CloudStorageAccount.Parse(connectionString)
    let tableClient = storageAccount.CreateCloudTableClient()
    
    // Retrieve a reference to the table.
    let table = tableClient.GetTableReference("people")
    
    // Create the table if it doesn't exist.
    table.CreateIfNotExists() |> ignore

    let (bob: Person.T) = {FirstName="bob"; LastName="bobsson"; Email="bob@bob.bob"; Phone="123123123"}
    let sally = {bob with FirstName="sally"; Email="sally@bob.bob"}

    let resultWriter r m =
      match r with
      | Ok _ -> printfn m
      | Error e -> printfn "Error: %A" e

    let addPerson p =
      let result = PersonDto.save table p
      resultWriter result $"Person saved: %A{p}"

    let fetchPerson p =
      let result = PersonDto.load table p
      resultWriter result $"Person loaded: %A{p}"

    let deletePerson p =
      let result = PersonDto.delete table p
      resultWriter result $"Person deleted: %A{p}"

    let addPeople ps =
      let result = PersonDto.saveBatch table ps
      resultWriter result $"People saved: %A{ps}"

    let fetchFamily f =
      let result = PersonDto.loadByKey table "PartitionKey" f
      resultWriter result $"People loaded: %A{result}"

    addPerson bob
    fetchPerson bob
    deletePerson bob
    addPeople [bob; sally]
    fetchFamily "bobsson"

    0 // return an integer exit code