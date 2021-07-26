namespace FSharpJamCode

open UnityEngine

type FSharpMonoBehaviour() = 
    inherit MonoBehaviour()
    member this.Start() = Debug.Log("An F# MonoBehaviour!!!")
