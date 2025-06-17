#nullable enable

using UnityEngine;

/// <summary>
/// This script is attached to the root `Player` object and handles the mechanics
/// of the equipped item (e.g. dropping, storing, weilding, etc.)
/// </summary>
public class EquippedItem : MonoBehaviour
{
    private PlayerInteractor? _interactor;

    [SerializeField] private Transform? _equippedItemPos;

    private ItemEntity? _itemInteraction = null;

    private GameObject? _equippedItem;
    public GameObject? EquippedItemObject { get => _equippedItem; }

    public void SetEquippedItem(GameObject item)
    {
        if (item == null)
        {
            Debug.LogError("EquippedItem: Attempted to equip a null item.");
            return;
        }

        if (_equippedItem != null)
        {
            DropEquippedItem();
        }

        _equippedItem = item;
        _equippedItem.transform.SetParent(_equippedItemPos);

        _itemInteraction = _equippedItem.GetComponent<ItemEntity>();
        if (!_itemInteraction)
        {
            throw new System.Exception($"EquippedItem: Item '{_equippedItem.name}' does not have an ItemEntity component.");
        }

        var itemData = _itemInteraction!.ItemData;
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

        _itemInteraction!.onEquipped();
    }

    public void DropEquippedItem()
    {
        if (_equippedItem != null)
        {
            _itemInteraction!.onUnequipped();
            if (_equippedItem.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = false;
            }

            ClearEquippedItemData();
        }
    }

    private void ThrowEquippedItem()
    {
        if (_equippedItem != null)
        {
            _itemInteraction!.onUnequipped();
            if (_equippedItem.TryGetComponent<Rigidbody>(out var rb))
            {
                float _throwForce = 10f;
                rb.isKinematic = false;
                rb.AddForce(Camera.main.transform.forward * _throwForce, ForceMode.VelocityChange);
            }

            ClearEquippedItemData();
        }
    }

    private void ClearEquippedItemData()
    {
        if (_equippedItem != null)
        {
            _equippedItem.transform.SetParent(null);
            _equippedItem = null;
        }

        if (_itemInteraction != null)
        {
            _itemInteraction = null; // Make sure we can't send the Attack command after dropping an item
        }
    }

    void Awake()
    {
        _interactor = GetComponentInChildren<PlayerInteractor>();
        if (_interactor == null)
        {
            Debug.LogError("EquippedItem script requires a PlayerInteractor component on the same GameObject.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_interactor!.FocusedObject != null)
            {
                this.SetEquippedItem(_interactor.FocusedObject);
            }
            else if (_equippedItem != null)
            {
                DropEquippedItem();
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (_itemInteraction != null)
            {
                _itemInteraction.Attack();
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ThrowEquippedItem();
        }
    }
}
