namespace FSharpJamCode

open UnityEngine

type Grabbable() = 
    inherit MonoBehaviour()
    let mutable heldBy : IGrabber option = None
    let mutable lastHeldTime = System.Single.MinValue
    member public this.LastHeldTime = lastHeldTime
    member public this.IsHeld = Option.isSome heldBy
    member public this.ForceRelease() =
        match heldBy with
        | Some g -> g.ForceRelease()
        | _ -> raise (System.InvalidOperationException("Unexpected ForceRelease on Grabbable with no heldBy object"))
    member public this.Grab(grabber : IGrabber) = 
        heldBy <- Some grabber
    member public this.Release(grabber : IGrabber) = 
        heldBy <- None
        lastHeldTime <- Time.time