namespace FSharpJamCode

open UnityEngine
open UniRx
open System.Collections.Generic
open NaughtyAttributes
open Unity.Linq
open System.Linq

type Hand = 
| Left = 0
| Right = 1

module HandState = 
    type HandAction = 
        | Grab of Grabbable
        | Release of Grabbable
        | ForceRelease of Grabbable

    type HandState = 
        | Open
        | Holding of Grabbable
        | Releasing of Grabbable
        | ForceReleasing of Grabbable
        | Regrabbing of Grabbable
        | EmptyFist

    let triggerGripDown handState grabbable = 
        match handState, grabbable with
        | Open, Some g -> Holding g, Some (Grab g)
        | Open, None -> EmptyFist, None
        | s, _ -> raise (System.InvalidOperationException($"Invalid state transition in triggerGripDown, state {s}"))

    let triggerGripUp handState = 
        match handState with 
        | Holding g -> Releasing g, Some (Release g)
        | EmptyFist -> Open, None
        | s -> raise (System.InvalidOperationException($"Invalid state transition in triggerGripUp, state {s}"))

    let releaseFinished handState = 
        match handState with
        | Releasing g -> Open
        | ForceReleasing g -> Regrabbing g
        | s -> raise (System.InvalidOperationException($"Invalid state transition in relaseFinished, state {s}"))

    let forceRelease handState = 
        match handState with
        | Holding g -> ForceReleasing g, ForceRelease g
        | s -> raise (System.InvalidOperationException($"Invalid state transition in forceRelease, state {s}"))

    let tryRegrab handState = 
        match handState with 
        | Regrabbing g -> EmptyFist
        | s -> s


type HandBehaviour() = 
    inherit MonoBehaviour()
    [<DefaultValue; SerializeField>]
    val mutable private hand : Hand
    [<DefaultValue; ShowNonSerializedField>]
    val mutable private grabbable : Grabbable
    let mutable handState = HandState.Open
    let maybeGrabbable g = if g = Unchecked.defaultof<Grabbable> then None else Some g
    let findBestGrabbable (go : GameObject) =
        let candidates = go.GetComponentsInParent<Grabbable>()
        if candidates |> Array.exists (fun g -> g.IsHeld)
        then candidates |> Array.tryHead
        else candidates |> Array.tryLast
    let selectHighlightLayer isHighlighted (hand : Hand) =
        match (isHighlighted, hand) with
        | (false, _) -> Utils.Layer.Default
        | (_, Hand.Left) -> Utils.Layer.GrabbableHighlightL
        | (_, Hand.Right) -> Utils.Layer.GrabbableHighlightR
        |> int
    let setLayerForObjectAndDescendants (g : Grabbable) highlightLayer =
        if g <> Unchecked.defaultof<Grabbable>
        then
            for mf in g.gameObject.GetComponentsInChildren<MeshFilter>() do
                if mf.gameObject.layer <> highlightLayer then
                    // Debug.Log($"SetLayer {highlightLayer} for {mf.gameObject.name}")
                    mf.gameObject.layer <- highlightLayer
    interface IGrabber with
        member this.ForceRelease() = 
            let hs, action = HandState.forceRelease handState
            handState <- hs
            match action with
            | HandState.ForceRelease g -> 
                g.Release(this)
                handState <- HandState.releaseFinished handState
            | _ -> raise (System.InvalidOperationException("Unexpected action in ForceRelease"))
    [<ShowNativeProperty>]
    member private this.HandState = handState.ToString()
    member private this.Update() =
        let hs = HandState.tryRegrab handState
        handState <- hs
    member public this.OnTriggerGripDown() = 
        let hs, action = HandState.triggerGripDown handState (maybeGrabbable this.grabbable)
        handState <- hs
        match action with
        | Some (HandState.Grab g) -> 
            if g.transform.parent <> Unchecked.defaultof<Transform>
            then 
                let attachSocket = g.transform.parent.gameObject.GetComponent<AttachSocket>()
                if attachSocket <> Unchecked.defaultof<AttachSocket> then GameObject.Destroy(attachSocket.gameObject)
                let attachTarget = g.GetComponentInParent<AttachTarget>()
                if attachTarget <> Unchecked.defaultof<AttachTarget> then attachTarget.SetAttachSocket(Unchecked.defaultof<AttachSocket>)
            if g.IsHeld then g.ForceRelease()
            g.Grab(this)
            g.transform.SetParent(this.gameObject.transform, true)
            let rb = g.GetComponent<Rigidbody>()
            GameObject.Destroy(rb)
        | _ -> ()
        // Debug.Log($"OnTriggerGripDown() {this.hand}")
        ()
    member public this.OnTriggerGripUp() = 
        let hs, action = HandState.triggerGripUp handState
        handState <- hs
        match action with
        | Some (HandState.Release g) -> 
            g.Release(this)
            g.transform.SetParent(null, true)
            let rb = g.gameObject.AddComponent<Rigidbody>()
            rb.isKinematic <- true
            rb.useGravity <- false
            handState <- HandState.releaseFinished handState
        | _ -> ()
        // Debug.Log($"OnTriggerGripUp() {this.hand}")
        ()
    member public this.UpdateBestGrabCandidate(colliders: Collider seq) =
        let candidates = 
            colliders
            |> Seq.map (fun x -> x.gameObject)
            |> Seq.map findBestGrabbable
            |> Seq.filter Option.isSome
            |> Seq.map Option.get
        let newBest = if Seq.isEmpty candidates then Unchecked.defaultof<Grabbable> else Seq.head candidates
        if (this.grabbable <> newBest)
        then
            setLayerForObjectAndDescendants this.grabbable (selectHighlightLayer false this.hand)
        this.grabbable <- newBest
        setLayerForObjectAndDescendants this.grabbable (selectHighlightLayer true this.hand)
        // Debug.Log($"UpdateBestGrabCandidate() - grabbable = {this.grabbable}")
        ()
