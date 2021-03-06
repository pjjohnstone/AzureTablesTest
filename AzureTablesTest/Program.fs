open System.IO

[<EntryPoint>]
let main argv =
    let configPath = Path.Join(Directory.GetCurrentDirectory(), "config.json")
    let config = Config.load configPath
    let connectionString = Config.getValue config "tableStorageKey"
    0 // return an integer exit code