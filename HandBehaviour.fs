namespace FSharpJamCode

open UnityEngine
open UniRx
open System.Collections.Generic

type Hand = 
| Left = 0
| Right = 1

module HandState = 
    type HandAction = 
        | Grab of Grabbable
        | Release of Grabbable

    type HandState = 
        | Open
        | Holding of Grabbable
        | Releasing of Grabbable
        | EmptyFist

    let triggerGripDown handState grabbable = 
        match handState, grabbable with
        | Open, Some g -> Holding g, Some (Grab g)
        | Open, None -> EmptyFist, None
        | Holding g, _ -> Holding g, None
        | EmptyFist, _ -> EmptyFist, None
        | Releasing g, _ -> Releasing g, None

    let triggerGripUp handState = 
        match handState with 
        | Open -> Open, None
        | Holding g -> Releasing g, Some (Release g)
        | EmptyFist -> Open, None
        | Releasing g -> Releasing g, None

    let releaseFinished handState = 
        match handState with
        | Releasing g -> Open
        | _ -> raise (System.ArgumentException($"Unexpected relaseFinished trigger in state {handState}"))


type HandBehaviour() = 
    inherit MonoBehaviour()
    [<DefaultValue; SerializeField>]
    val mutable private hand : Hand
    [<DefaultValue; SerializeField>]
    val mutable private grabbable : Grabbable
    let mutable handState = HandState.Open
    let maybeGrabbable g = if g = Unchecked.defaultof<Grabbable> then None else Some g
    member public this.OnTriggerGripDown() = 
        let hs, action = HandState.triggerGripDown handState (maybeGrabbable this.grabbable)
        handState <- hs
        match action with
        | Some (HandState.Grab g) -> g.transform.SetParent(this.gameObject.transform, true)
        | _ -> ()
        Debug.Log($"OnTriggerGripDown() {this.hand}")
        ()
    member public this.OnTriggerGripUp() = 
        let hs, action = HandState.triggerGripUp handState
        handState <- hs
        match action with
        | Some (HandState.Release g) -> 
            g.transform.SetParent(null, true)
            handState <- HandState.releaseFinished handState
        | _ -> ()
        Debug.Log($"OnTriggerGripUp() {this.hand}")
        ()
    member public this.UpdateBestGrabCandidate(colliders: HashSet<Collider>) =
        let candidates = 
            colliders
            |> Seq.map (fun x -> x.GetComponentInParent<Grabbable>())
            |> Seq.filter (fun x -> x <> Unchecked.defaultof<Grabbable>)
        let best = if Seq.isEmpty candidates then Unchecked.defaultof<Grabbable> else Seq.head candidates
        this.grabbable <- best
        ()
