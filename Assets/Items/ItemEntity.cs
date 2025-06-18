#nullable enable
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class ItemAction : Attribute
{
    public string ActionName { get; }

    public ItemAction(string actionName)
    {
        ActionName = actionName;
    }
}

/// <summary>
/// This script is attached to an item in the game world to
/// allow entities to interact with it.
/// </summary>
public class ItemEntity : GameEntity, Interactable
{
    [SerializeField] private ItemData? _itemData;
    public ItemData? ItemData { get => _itemData; }
    
    private GameEntity? _ownerEntity;
    public GameEntity? OwnerEntity
    {
        get => _ownerEntity;
        set => _ownerEntity = value;
    }
    
    private GameEntity? _interactorEntity;
    private EquippedItemController? _equippedItemCtrl;

    public string HelpText => $"Press [E] to interact with '{_itemData?.ItemName}'";

    public void Interact(GameEntity? interactor = null)
    {
        InteractionManager.Instance.OnInteractionAction += this.DoInteraction;
        InteractionManager.Instance.OnMenuClosed += () =>
        {
            InteractionManager.Instance.OnInteractionAction -= this.DoInteraction;
            InteractionManager.Instance.OnMenuClosed -= () => { };
            _interactorEntity = null;
        };
        InteractionManager.Instance.OpenMenu(_itemData?.Interactions);

        _interactorEntity = interactor;
    }

    private void DoInteraction(string actionName)
    {
        Type type = this.GetType();
        while (type != null && type != typeof(MonoBehaviour))
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (MethodInfo method in methods)
            {
                ItemAction attr = method.GetCustomAttribute<ItemAction>();
                if (attr != null && attr.ActionName == actionName)
                {
                    method.Invoke(this, null);
                    return;
                }
            }

            type = type.BaseType;
        }

        Debug.LogWarning($"No action found for '{actionName}' in {this.GetType().Name}");
    }

    [ItemAction("take_equip")]
    protected virtual void onTakeEquip()
    {
        _ownerEntity = _interactorEntity;
        // _ownerEntity.SetEquippedItem(this.gameObject);

        _equippedItemCtrl?.SetEquippedItem(this.gameObject);
    }

    public virtual void onUnequipped()
    {
        if (_hitCollider) _hitCollider.enabled = true;
        _ownerEntity = null;
        _interactorEntity = null;
    }

    public void OnFocus()
    {
        // nothing to do
    }

    public void OnDefocus()
    {
        // nothing to do
    }

    protected virtual void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            throw new Exception("Player GameObject not found. Make sure the Player has the 'Player' tag.");
        }

        _equippedItemCtrl = player.GetComponent<EquippedItemController>();
        if (_equippedItemCtrl == null)
        {
            throw new Exception("EquippedItem script not found on Player. Make sure the Player has the EquippedItem component.");
        }

        _hitCollider = GetComponent<Collider>();
        if (_hitCollider == null)
        {
            throw new Exception("Collider not found on ItemEntity. Make sure the item has a Collider component.");
        }
    }

    public override float Heal(float amount)
    {
        // Items typically don't heal, so we can just return 0
        return 0f;
    }

    public override float TakeDamage(float damage)
    {
        // Items typically don't take damage, so we can just return 0
        return 0f;
    }

    /// <summary>
    /// Code in this region is used as a default attack for items. This 
    /// has the player swing the item in a direction.
    /// </summary>
#region AttackCode

    private Vector3 _defaultLocalPosition;
    private Quaternion _defaultLocalRotation;
    private Coroutine? _swingCoroutine;
    protected Collider? _hitCollider;
    private readonly HashSet<Collider> _alreadyHit = new HashSet<Collider>();

    public virtual void onEquipped()
    {
        _defaultLocalPosition = transform.localPosition;
        _defaultLocalRotation = transform.localRotation;
        if (_hitCollider) _hitCollider.enabled = false;
    }

    public virtual void Attack()
    {
        if (_swingCoroutine != null)
        {
            StopCoroutine(_swingCoroutine);
        }

        _swingCoroutine = StartCoroutine(AnimateSwing());
    }

    private IEnumerator AnimateSwing()
    {
        OnStartAttack();

        float duration = 0.25f;
        float elapsed = 0f;

        // AI: Define arc positions
        Vector3 rightStart = _defaultLocalPosition + new Vector3(0.25f, -0.05f, -0.1f);
        Vector3 leftEnd    = _defaultLocalPosition + new Vector3(-0.25f,  0.05f, -0.1f);

        // AI: Correct rotation for sideways twist (no forward thrust)
        Quaternion startRot = _defaultLocalRotation * Quaternion.Euler(0f, 45f, 15f);   // cocked back
        Quaternion endRot   = _defaultLocalRotation * Quaternion.Euler(0f, -45f, -15f); // full swing

        transform.localPosition = rightStart;
        transform.localRotation = startRot;

        yield return null;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.localPosition = Vector3.Lerp(rightStart, leftEnd, t);
            transform.localRotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        OnEndAttack();

        transform.localPosition = _defaultLocalPosition;
        transform.localRotation = _defaultLocalRotation;
        _swingCoroutine = null;
    }
    
    // used for collision detection when swinging the item
    private void OnTriggerEnter(Collider other)
    {
        if (_itemData == null) return;
        if ((_itemData.TargetCollisionLayers & (1 << other.gameObject.layer)) == 0) return;
        if (this.gameObject.transform.root == other.gameObject.transform.root) return;
        if (_alreadyHit.Contains(other)) return;

        var entity = other.GetComponent<GameEntity>();
        if (entity != null)
        {
            entity.TakeDamage(_itemData.DamageScore);
            _alreadyHit.Add(other);
        }
    }

    protected virtual void OnStartAttack()
    {
        if (_hitCollider)
        {
            _hitCollider.enabled = true;
            _hitCollider.isTrigger = true;
        }
    }

    protected virtual void OnEndAttack()
    {
        if (_hitCollider)
        {
            _hitCollider.enabled = false;
            _hitCollider.isTrigger = false;
        }
        _alreadyHit.Clear();
    }

#endregion AttackCode
  
}
