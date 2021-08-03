namespace FSharpJamCode

open UnityEngine
open System.Collections.Generic

type AttachmentSystem() = 
    inherit MonoBehaviour()
    let mutable attachCandidates = new HashSet<int * GameObject * GameObject>()
    interface IAttachmentSystem with
        member this.RegisterAttachCandidatePair frame attachTarget attachPoint =
            attachCandidates.Add((frame, attachTarget, attachPoint)) |> ignore
    [<DefaultValue; SerializeField>]
    val mutable private attachSocketPrefab : AttachSocket
    member this.Update() = 
        if attachCandidates.Count > 0
        then
            let (frame, attachTarget, attachPoint) = attachCandidates |> Seq.head
            Debug.Log($"Frame: {frame}; attach candidate pair: ({attachTarget.name}, {attachPoint.name})")
            let grabbable = attachPoint.GetComponentInParent<Grabbable>()
            if grabbable <> Unchecked.defaultof<Grabbable> && not grabbable.IsHeld
            then 
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