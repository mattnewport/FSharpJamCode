namespace FSharpJamCode

open UnityEngine
open System.Collections.Generic
open NaughtyAttributes

type GrabTrigger() = 
    inherit MonoBehaviour()
    let colliders = new Dictionary<Collider, int>()
    [<DefaultValue>]
    val mutable private hb: HandBehaviour
    [<ShowNativeProperty>]
    member private this.Colliders = System.String.Join(", ", colliders |> Seq.map (fun x -> x.Key.gameObject.name) |> Array.ofSeq)
    member this.OnTriggerEnter(other: Collider) = 
        colliders.[other] <- Time.frameCount
        // Debug.Log($"OnTriggerEnter({other.gameObject.name}), Colliders: {this.Colliders}")
        ()
    member this.OnTriggerStay(other: Collider) = 
        colliders.[other] <- Time.frameCount
        ()
    member this.OnTriggerExit(other: Collider) =
        colliders.Remove(other) |> ignore
        // Debug.Log($"OnTriggerExit({other.gameObject.name}), Colliders: {this.Colliders}")
        ()
    member this.Start() = 
        this.hb <- this.GetComponentInParent<HandBehaviour>()
        ()
    member this.Update() =
        let deadColliders = colliders.Keys |> Seq.filter (fun c -> not (Utils.isValid c)) |> Array.ofSeq
        deadColliders |> Seq.iter (fun c -> colliders.Remove(c) |> ignore)
        let staleColliders = colliders |> Seq.filter (fun kv -> Time.frameCount - kv.Value > 2) |> Array.ofSeq
        staleColliders |> Seq.iter (fun kv -> colliders.Remove(kv.Key) |> ignore)
        this.hb.UpdateBestGrabCandidate(colliders.Keys)
        ()
