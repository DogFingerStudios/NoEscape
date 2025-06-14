#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseNPC : GameEntity
{
    [SerializeField]
    [Tooltip("The AudioSource component for playing NPC sounds")]
    private AudioSource _audioSource;
    public AudioSource AudioSource { get => _audioSource; }

    [SerializeField] 
    EntityProfile _entityProfile;
    public EntityProfile Profile
    {
        get => _entityProfile;
        set => _entityProfile = value;
    }

    private Animator _animator;
    public Animator Animator
    {
        get => _animator;
        set => _animator = value;
    }

    [SerializeField]
    [Tooltip("The rate at which the NPC rotates towards the target")]
    public float rotationSpeed = 90f;

    [SerializeField]
    [Tooltip("The speed at which the NPC moves")]
    public float moveSpeed = 3.5f;

    [SerializeField]
    [Tooltip("The distance at which the NPC will detect the target")]
    public float DetectionDistance = 5f;

    [SerializeField]
    [Tooltip("The distance at which the NPC will stop moving towards the target")]
    public float stopDistance = 1f;

    public Transform target;

#region State Management
    public void setState(NPCState state)
    {
        stateMachine.SetState(state);
    }

    public void PushState(NPCState state)
    {
        stateMachine.StateStack.Push(state);
    }

    public NPCState? PopState()
    {
        if (stateMachine.StateStack.Count > 0)
        {
            return stateMachine.StateStack.Pop();
        }

        return null;
    }
    #endregion

    public event Action<float> OnHealthChanged;

    public override float TakeDamage(float damage)
    {
        Health -= damage;
        if (Health < 0) Health = 0;
        this.OnHealthChanged?.Invoke(Health);
        if (Health <= 0)
        {
            onDeath();
            bool foo = this.IsAlive;
            Debug.Log($"{this.name} is dead: {foo}");
        }
        return Health;
    }

    public override float Heal(float amount)
    {
        Health += amount;
        if (Health > MaxHealth) Health = MaxHealth;
        this.OnHealthChanged?.Invoke(Health);
        return Health;
    }

    private void onDeath()
    {
        Debug.Log($"{this.name} has died.");
        this.setState(new NPCDeathState(this));
    }

    protected NPCStateMachine stateMachine = new NPCStateMachine();
    public NPCStateMachine StateMachine => stateMachine;

    protected virtual void Start()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void Panic()
    {
         StartCoroutine(BlinkTwiceCoroutine());
    }

    private IEnumerator BlinkTwiceCoroutine()
    {
        if (_animator == null)
        {
            yield break;
        }

        _animator.SetTrigger("RefuseBlink");
        yield return new WaitForSeconds(0.4f); // Match animation clip duration
        _animator.SetTrigger("RefuseBlink");
        yield return new WaitForSeconds(0.4f);
        
        // Optional: force back to Idle (if needed)
        _animator.Play("Idle");
    }

    protected virtual void Update()
    {
        if (stateMachine == null) return;
        stateMachine.Update();
    }
    
}