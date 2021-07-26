namespace FSharpJamCode

open UnityEngine

type Grabbable() = 
    inherit MonoBehaviour()
    member this.Start() = Debug.Log("An F# MonoBehaviour!!!")
