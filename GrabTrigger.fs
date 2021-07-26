namespace FSharpJamCode

open UnityEngine
open System.Collections.Generic

type GrabTrigger() = 
    inherit MonoBehaviour()
    let colliders = new HashSet<Collider>()
    [<DefaultValue>]
    val mutable private hb: HandBehaviour
    member this.OnTriggerEnter(other: Collider) = 
        Debug.Log($"OnTriggerEnter({other.gameObject.name})")
        colliders.Add(other) |> ignore
        ()
    member this.OnTriggerStay(other: Collider) = 
        colliders.Add(other) |> ignore
        ()
    member this.OnTriggerExit(other: Collider) =
        Debug.Log($"OnTriggerExit({other.gameObject.name})")
        colliders.Remove(other) |> ignore
        ()
    member this.Start() = 
        this.hb <- this.GetComponentInParent<HandBehaviour>()
        ()
    member this.Update() =
        this.hb.UpdateBestGrabCandidate(colliders)
        ()
