namespace FSharpJamCode

open UnityEngine
open System.Collections.Generic
open NaughtyAttributes

type GrabTrigger() = 
    inherit MonoBehaviour()
    let colliders = new HashSet<Collider>()
    [<DefaultValue>]
    val mutable private hb: HandBehaviour
    [<ShowNativeProperty>]
    member private this.Colliders = System.String.Join(", ", colliders |> Seq.map (fun x -> x.gameObject.name) |> Array.ofSeq)
    member this.OnTriggerEnter(other: Collider) = 
        colliders.Add(other) |> ignore
        // Debug.Log($"OnTriggerEnter({other.gameObject.name}), Colliders: {this.Colliders}")
        ()
    member this.OnTriggerStay(other: Collider) = 
        colliders.Add(other) |> ignore
        ()
    member this.OnTriggerExit(other: Collider) =
        colliders.Remove(other) |> ignore
        // Debug.Log($"OnTriggerExit({other.gameObject.name}), Colliders: {this.Colliders}")
        ()
    member this.Start() = 
        this.hb <- this.GetComponentInParent<HandBehaviour>()
        ()
    member this.Update() =
        this.hb.UpdateBestGrabCandidate(colliders)
        colliders.Clear()
        ()
