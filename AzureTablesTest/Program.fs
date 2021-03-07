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

    Person.save bob table |> ignore
    0 // return an integer exit code