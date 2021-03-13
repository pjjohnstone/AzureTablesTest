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

    let addPerson p =
      let result = Person.save table p
      match result with
      | Ok _ -> printfn "Person added: %A" bob
      | Error e -> printfn "Not OK: %A" e

    let fetchPerson p =
      let result = Person.load table p
      match result with
      | Ok r -> printfn "Person retrieved: %A" r
      | Error e -> printfn "Not OK: %A" e

    let deletePerson p =
      let result = Person.delete table p
      match result with
      | Ok r -> printfn "Person deleted: %A" p
      | Error e -> printfn "Not OK: %A" e

    addPerson bob
    fetchPerson bob
    deletePerson bob
    fetchPerson bob

    0 // return an integer exit code