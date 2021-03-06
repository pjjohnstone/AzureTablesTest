module Config

open Files

type ConfigValue = {Key:string; Value:string}

type ConfigError =
  | ValidationError of string
  | DeserializationException of exn

type ResultBuilder() =
  member this.Return x = Ok x
  member this.Zero() = Ok ()
  member this.Bind(xResult,f) = Result.bind f xResult

let private result = ResultBuilder()

let private parseJson jsonString :Result<ConfigValue list, ConfigError> =
  result {
    let! deserializedValue =
      jsonString
      |> Json.deserialize
      |> Result.mapError DeserializationException

    return deserializedValue
  }

let load filePath =
  filePath
  |> fileReader
  |> parseJson

let private getConfigValues (config: Result<ConfigValue list, ConfigError>) =
  match config with
  | Ok v -> v
  | Error e ->
    printfn "There was an error reading the config: %A" e
    []

let getValue config key =
  let configValues = getConfigValues config
  configValues
  |> List.tryFind (fun c -> c.Key = key)