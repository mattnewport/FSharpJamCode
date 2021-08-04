namespace FSharpJamCode

open UnityEngine

type AttachTarget() = 
    inherit MonoBehaviour()
    let mutable attachmentSystem = Unchecked.defaultof<IAttachmentSystem>
    let mutable lastPreferredCandidateTime = System.Single.MinValue
    member public this.SetLastPreferredCandidateTime t = lastPreferredCandidateTime <- t
    [<DefaultValue; SerializeField>]
    val mutable private attachSocket : AttachSocket
    [<DefaultValue; SerializeField>]
    val mutable private axes : GameObject
    member this.Start() =
        let attachmentSystemGo = GameObject.Find("AttachmentSystem")
        attachmentSystem <- attachmentSystemGo.GetComponent<IAttachmentSystem>()
    member this.Update() =
        this.axes.SetActive(Time.time - lastPreferredCandidateTime < 0.33f)
    member private this.RegisterCollider (other : Collider) = 
        let attachPoint = other.GetComponent<AttachPoint>()
        if attachPoint <> Unchecked.defaultof<AttachPoint> && this.attachSocket = Unchecked.defaultof<AttachSocket>
        then
            attachmentSystem.RegisterAttachCandidatePair Time.frameCount this.gameObject attachPoint.gameObject
    member this.SetAttachSocket(attachSocket : AttachSocket) = 
        this.attachSocket <- attachSocket
        Debug.Log("SetAttachSocket")
    member this.OnTriggerEnter(other: Collider) = 
        this.RegisterCollider other
        // Debug.Log($"OnTriggerEnter({other.gameObject.name}), Colliders: {this.Colliders}")
        ()
    member this.OnTriggerStay(other: Collider) = 
        this.RegisterCollider other
        ()
    member this.OnTriggerExit(other: Collider) =
        // colliders.Remove(other) |> ignore
        // Debug.Log($"OnTriggerExit({other.gameObject.name}), Colliders: {this.Colliders}")
        ()
