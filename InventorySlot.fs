namespace FSharpJamCode

open UnityEngine

type InventorySlot() = 
    inherit MonoBehaviour()
    [<DefaultValue; SerializeField>]
    val mutable private prefab : GameObject
    [<DefaultValue>]
    val mutable private item : GameObject
    member private this.Update() =
        if this.item = Unchecked.defaultof<GameObject>
        then 
            let newItem = GameObject.Instantiate<GameObject>(this.prefab, this.transform)
            newItem.transform.localPosition <- Vector3.zero
            newItem.transform.localRotation <- Quaternion.identity
            this.item <- newItem
        else
            if this.item.transform.parent <> this.transform
            then this.item <- null
