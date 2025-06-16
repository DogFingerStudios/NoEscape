using UnityEngine;

public class FlashlightInteraction : ItemEntity
{
    [ItemAction("take_equip")]
    protected override void onTakeEquip()
    {
        Debug.Log("Flashlight equipped");
    }
}
