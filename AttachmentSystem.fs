namespace FSharpJamCode

open UnityEngine
open System.Collections.Generic

type AttachmentSystem() = 
    inherit MonoBehaviour()
    let mutable attachCandidates = new HashSet<int * GameObject * GameObject>()
    let distBetweenAttachPointAndAttachTarget (candidate : int * GameObject * GameObject) =
        let _, attachTarget, attachPoint = candidate
        Vector3.SqrMagnitude(attachTarget.transform.position - attachPoint.transform.position)
    let recentlyHeldGrabbable (frame : int, attachTarget : GameObject, attachPoint : GameObject) =
        let grabbable = attachPoint.GetComponentInParent<Grabbable>()
        if grabbable <> Unchecked.defaultof<Grabbable> && not grabbable.IsHeld
        then (Time.time - grabbable.LastHeldTime) < 0.5f
        else false
    interface IAttachmentSystem with
        member this.RegisterAttachCandidatePair frame attachTarget attachPoint =
            attachCandidates.Add((frame, attachTarget, attachPoint)) |> ignore
    [<DefaultValue; SerializeField>]
    val mutable private attachSocketPrefab : AttachSocket
    member this.Update() = 
        let validAttachCandidates = attachCandidates |> Seq.filter recentlyHeldGrabbable
        if Seq.length validAttachCandidates > 0
        then
            let (frame, attachTarget, attachPoint) = 
                validAttachCandidates
                |> Seq.minBy distBetweenAttachPointAndAttachTarget

            let grabbable = attachPoint.GetComponentInParent<Grabbable>()
            Debug.Log($"Frame: {frame}; Attaching attach candidate pair: ({attachTarget.name}, {attachPoint.name})")
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
            if attachTargetC <> Unchecked.defaultof<AttachTarget> then attachTargetC.SetAttachSocket(attachSocket)
            ()
        attachCandidates.Clear()