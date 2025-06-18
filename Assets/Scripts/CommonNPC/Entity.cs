using System;
using UnityEngine;

public abstract class GameEntity : MonoBehaviour
{
    [SerializeField] public float Health = 100f;
    [SerializeField] public float MaxHealth = 100f;

    public abstract float TakeDamage(float amount);
    public abstract float Heal(float amount);
    public virtual bool IsAlive { get => Health > 0; }

    public event Action<float> OnHealthChanged;
    protected void onHealthChanged(float health)
    {
        OnHealthChanged?.Invoke(health);
    }
}

public class CharacterEntity : GameEntity
{
    public override float Heal(float amount)
    {
        Health += amount;
        if (Health > MaxHealth) Health = MaxHealth;
        onHealthChanged(Health);
        return Health;
    }

    public override float TakeDamage(float amount)
    {
        Health -= amount;
        if (Health < 0) Health = 0;
        onHealthChanged(Health);
        return Health;
    }
}
