#nullable enable

using UnityEngine;

/// <summary>
/// This script is attached to the root `Player` object and handles the mechanics
/// of the equipped item (e.g. dropping, storing, weilding, etc.)
/// </summary>
public class EquippedItemController : MonoBehaviour
{
    [SerializeField] private Transform? _equippedItemPos;

    private ItemEntity? _itemEntity = null;

    private GameObject? _equippedItem;
    public GameObject? EquippedItemObject { get => _equippedItem; }

    public ItemEntity SetEquippedItem(GameObject item)
    {
        if (item == null)
        {
            throw new System.ArgumentNullException(nameof(item), "EquippedItem: Cannot set equipped item to null.");
        }

        if (_equippedItem != null)
        {
            DropEquippedItem();
        }

        _equippedItem = item;
        _equippedItem.transform.SetParent(_equippedItemPos);

        _itemEntity = _equippedItem.GetComponent<ItemEntity>();
        if (!_itemEntity)
        {
            throw new System.Exception($"EquippedItem: Item '{_equippedItem.name}' does not have an ItemEntity component.");
        }

        var itemData = _itemEntity!.ItemData;
        if (itemData == null)
        {
            throw new System.Exception($"EquippedItem: Item '{_equippedItem.name}' does not have ItemData.");
        }

        // Once parented to the item position, set the local position and rotation to zero,
        // so it matches the item pos transform.
        _equippedItem.transform.localPosition = itemData.EquippedPosition;
        _equippedItem.transform.localRotation = Quaternion.Euler(itemData.EquippedRotation);

        // Do we want to disable physics while we equip the item?
        // If item doesn't have physics, we don't need to do anything
        if (_equippedItem.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }

        _itemEntity!.onEquipped();
        return _itemEntity;
    }

    public void DropEquippedItem()
    {
        if (_equippedItem != null)
        {
            _itemEntity!.onUnequipped();
            if (_equippedItem.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = false;
            }

            clearEquippedItemData();
        }
    }

    public void ThrowEquippedItem()
    {
        if (_equippedItem != null)
        {
            _itemEntity!.onUnequipped();
            if (_equippedItem.TryGetComponent<Rigidbody>(out var rb))
            {
                float _throwForce = 10f;
                rb.isKinematic = false;
                rb.AddForce(Camera.main.transform.forward * _throwForce, ForceMode.VelocityChange);
            }

            clearEquippedItemData();
        }
    }

    private void clearEquippedItemData()
    {
        if (_equippedItem != null)
        {
            _equippedItem.transform.SetParent(null);
            _equippedItem = null;
        }

        if (_itemEntity != null)
        {
            _itemEntity = null; // Make sure we can't send the Attack command after dropping an item
        }
    }
}
