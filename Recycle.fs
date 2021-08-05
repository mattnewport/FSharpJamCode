namespace FSharpJamCode

open UnityEngine

type Recycle() = 
    inherit MonoBehaviour()
    member private this.OnTriggerStay(other : Collider) =
        let grabbables = other.gameObject.GetComponentsInParent<Grabbable>()
        if not (Array.isEmpty grabbables)
        then 
            let topmostGrabbable = grabbables |> Array.last
            if not topmostGrabbable.IsHeld
            then GameObject.Destroy(topmostGrabbable.gameObject)