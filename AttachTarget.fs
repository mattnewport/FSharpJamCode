namespace FSharpJamCode

open UnityEngine

type AttachTarget() = 
    inherit MonoBehaviour()
    let mutable attachmentSystem = Unchecked.defaultof<IAttachmentSystem>
    member this.Start() =
        let attachmentSystemGo = GameObject.Find("AttachmentSystem")
        attachmentSystem <- attachmentSystemGo.GetComponent<IAttachmentSystem>()
    member this.OnTriggerEnter(other: Collider) = 
        let attachPoint = other.GetComponent<AttachPoint>()
        if attachPoint <> Unchecked.defaultof<AttachPoint>
        then
            attachmentSystem.RegisterAttachCandidatePair Time.frameCount this.gameObject attachPoint.gameObject
        // Debug.Log($"OnTriggerEnter({other.gameObject.name}), Colliders: {this.Colliders}")
        ()
    member this.OnTriggerStay(other: Collider) = 
        let attachPoint = other.GetComponent<AttachPoint>()
        if attachPoint <> Unchecked.defaultof<AttachPoint>
        then
            attachmentSystem.RegisterAttachCandidatePair Time.frameCount this.gameObject attachPoint.gameObject
        ()
    member this.OnTriggerExit(other: Collider) =
        // colliders.Remove(other) |> ignore
        // Debug.Log($"OnTriggerExit({other.gameObject.name}), Colliders: {this.Colliders}")
        ()
