namespace FSharpJamCode

open UnityEngine
open System.Collections.Generic

type AttachmentSystem() = 
    inherit MonoBehaviour()
    let mutable attachCandidates = new HashSet<int * GameObject * GameObject>()
    interface IAttachmentSystem with
        member this.RegisterAttachCandidatePair frame attachTarget attachPoint =
            attachCandidates.Add((frame, attachTarget, attachPoint)) |> ignore
    member this.Update() = 
        if attachCandidates.Count > 0
        then
            let (frame, attachTarget, attachPoint) = attachCandidates |> Seq.head
            Debug.Log($"Frame: {frame}; Attach candidate pair: ({attachTarget.name}, {attachPoint.name})")
            let grabbable = attachPoint.GetComponentInParent<Grabbable>()
            if grabbable <> Unchecked.defaultof<Grabbable> && not grabbable.IsHeld
            then 
                GameObject.Destroy(grabbable.GetComponent<Rigidbody>())
                grabbable.transform.SetParent(attachTarget.transform, true)
                let localMat = Matrix4x4.TRS(attachPoint.transform.localPosition, attachPoint.transform.localRotation, Vector3.one)
                let invLocalMat = localMat.inverse
                grabbable.transform.localPosition <- invLocalMat.MultiplyPoint(Vector3.zero)
                grabbable.transform.localRotation <- invLocalMat.rotation
            ()
        attachCandidates.Clear()