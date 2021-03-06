module Files

  open System.IO

  let fileWriter filePath content =
    File.WriteAllText(filePath, content)

  let fileReader filePath =
    File.ReadAllText(filePath)
