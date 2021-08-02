namespace FSharpJamCode

open UnityEngine

type IAttachmentSystem = 
    abstract RegisterAttachCandidatePair : frame : int -> attachTarget : GameObject -> attachPoint : GameObject -> unit