module Config

open Files
open Json

type Value =
  | String of string
  | Int of int

type ConfigValue = {Key:string; Value:Value}

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