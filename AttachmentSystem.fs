namespace FSharpJamCode

open UnityEngine
open System.Collections.Generic

type AttachmentSystem() = 
    inherit MonoBehaviour()
    let mutable attachCandidates = new HashSet<int * GameObject * GameObject>()
    let candidateScore (frame : int, attachTarget : GameObject, attachPoint : GameObject) =
        Vector3.Magnitude(attachTarget.transform.position - attachPoint.transform.position) * 0.001f + Mathf.Abs(Quaternion.Angle(attachTarget.transform.rotation, attachPoint.transform.rotation))
    let currentlyHeldGrabbable (frame : int, attachTarget : GameObject, attachPoint : GameObject) =
        let grabbable = attachPoint.GetComponentInParent<Grabbable>()
        grabbable <> Unchecked.defaultof<Grabbable> && grabbable.IsHeld
    let recentlyHeldGrabbable (frame : int, attachTarget : GameObject, attachPoint : GameObject) =
        let grabbable = attachPoint.GetComponentInParent<Grabbable>()
        if grabbable <> Unchecked.defaultof<Grabbable> && not grabbable.IsHeld
        then (Time.time - grabbable.LastHeldTime) < 0.5f
        else false
    let setPreferredCandidates (frame : int, attachTarget : GameObject, attachPoint : GameObject) =
        let attachTargetC = attachTarget.GetComponent<AttachTarget>()
        if attachTargetC <> Unchecked.defaultof<AttachTarget> 
        then attachTargetC.SetLastPreferredCandidateTime(Time.time)
        let attachPointC = attachPoint.GetComponent<AttachPoint>()
        if attachPointC <> Unchecked.defaultof<AttachPoint>
        then attachPointC.SetLastPreferredCandidateTime(Time.time)
    interface IAttachmentSystem with
        member this.RegisterAttachCandidatePair frame attachTarget attachPoint =
            attachCandidates.Add((frame, attachTarget, attachPoint)) |> ignore
    [<DefaultValue; SerializeField>]
    val mutable private attachSocketPrefab : AttachSocket
    member this.Update() = 
        let orderedAttachCandidates = 
            attachCandidates
            |> Seq.sortBy candidateScore
            |> Array.ofSeq

        let orderedHeldCandidates =
            orderedAttachCandidates
            |> Array.filter currentlyHeldGrabbable

        if not (Array.isEmpty orderedHeldCandidates)
        then setPreferredCandidates (orderedHeldCandidates |> Array.head)
        
        let validAttachCandidates = orderedAttachCandidates |> Array.filter recentlyHeldGrabbable
        if not (Array.isEmpty validAttachCandidates)
        then
            let (frame, attachTarget, attachPoint) = 
                validAttachCandidates
                |> Array.head

            let grabbable = attachPoint.GetComponentInParent<Grabbable>()
            // Debug.Log($"Frame: {frame}; Attaching attach candidate pair: ({attachTarget.name}, {attachPoint.name})")
            GameObject.Destroy(grabbable.GetComponent<Rigidbody>())
            grabbable.transform.SetParent(null)
            grabbable.transform.localPosition <- Vector3.zero
            grabbable.transform.localRotation <- Quaternion.identity
            grabbable.transform.localScale <- Vector3.one
            let attachSocket = GameObject.Instantiate<AttachSocket>(this.attachSocketPrefab, attachPoint.transform.localPosition, attachPoint.transform.localRotation, attachPoint.transform.parent)
            attachSocket.transform.SetParent(null, true)
            grabbable.transform.SetParent(attachSocket.transform, true)
            attachSocket.transform.SetParent(attachTarget.transform)
            attachSocket.transform.localPosition <- Vector3.zero
            attachSocket.transform.localRotation <- Quaternion.identity
            attachSocket.transform.localScale <- Vector3.one
            let attachTargetC = attachTarget.GetComponent<AttachTarget>()
            if attachTargetC <> Unchecked.defaultof<AttachTarget> 
            then 
                attachTargetC.SetAttachSocket(attachSocket)
            ()
        attachCandidates.Clear()