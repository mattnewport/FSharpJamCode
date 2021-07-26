namespace FSharpJamCode

open UnityEngine

module Api =
    let Hello (name : string) =
        Debug.Log($"Hello {name}")
