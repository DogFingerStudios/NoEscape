using System;
using UnityEngine;

public interface IHasHealth 
{
    float Health { get; set; }
    float MaxHealth { get; set; }

    float TakeDamage(float amount);
    float Heal(float amount);
    bool IsAlive { get; }

    event Action<float> OnHealthChanged;
}

public abstract class GameEntity : MonoBehaviour
{
    [SerializeField] public float Health = 100f;
    [SerializeField] public float MaxHealth = 100f;

    public abstract float TakeDamage(float amount);
    public abstract float Heal(float amount);
    public virtual bool IsAlive { get => Health > 0; }

    event Action<float> OnHealthChanged;
}

