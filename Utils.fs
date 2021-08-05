module Utils

open UnityEngine

type Layer = 
| Default = 0
| GrabbableHighlightL = 3
| GrabbableHighlightR = 6

let GetComponent<'T when 'T :> Component and 'T : equality> (go : GameObject) = 
    let c = go.GetComponent<'T>()
    if c <> Unchecked.defaultof<'T>
    then Some c
    else None

let GetComponentInParent<'T when 'T :> Component and 'T : equality> (go : GameObject) = 
    let c = go.GetComponentInParent<'T>()
    if c <> Unchecked.defaultof<'T>
    then Some c
    else None

let isValid<'T when 'T :> UnityEngine.Object and 'T : equality> (o : 'T) =
    UnityEngine.Object.op_Implicit o