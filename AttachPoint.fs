namespace FSharpJamCode

open UnityEngine

type AttachPoint() = 
    inherit MonoBehaviour()
    let mutable lastPreferredCandidateTime = System.Single.MinValue
    member public this.SetLastPreferredCandidateTime t = lastPreferredCandidateTime <- t
    [<DefaultValue; SerializeField>]
    val mutable private axes : GameObject
    member this.Update() =
        this.axes.SetActive(Time.time - lastPreferredCandidateTime < 0.33f)
