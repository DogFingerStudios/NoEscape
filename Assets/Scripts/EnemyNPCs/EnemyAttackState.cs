#nullable enable

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[NPCStateTag("EnemyAttack")]
public class EnemyAttackState : NPCState
{  
    public Transform _firePoint = new GameObject("FirePoint").transform;
    public AudioClip[] _gunshotSounds;

    public float fireRate = 1f;
    public float range = 50f;
    public float damage = 10f;
    public float defaultDamage = 10f;

    [Header("Audio")]
    [Tooltip("Audio clip to play when the gun is fired.")]
    private AudioSource _audioSource;

    private float nextFireTime = 0f;
    private LineRenderer _lineRenderer;
    private readonly GameEntity _targetEntity;

    public EnemyAttackState(BaseNPC npc, GameEntity target)
        : base(npc)
    {
        if (this.NPC!.target == null)
        {
            throw new System.Exception("BaseNPC is not an EnemyNPC. Cannot enter attack state.");
        }

        _firePoint.SetParent(this.NPC!.transform);
        _firePoint.localPosition = new Vector3(0.004f, 0.6019999f, 0.425f);
        _firePoint.localRotation = Quaternion.Euler(0f, 0f, 0f);
        _firePoint.localScale = new Vector3(1f, 0.5555556f, 1f);

        _lineRenderer = this.NPC!.GetComponent<LineRenderer>();
        _lineRenderer.enabled = false;
        _lineRenderer.positionCount = 2;
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;

        // set the volume dropoff curve
        AnimationCurve rolloff = new AnimationCurve();
        rolloff.AddKey(0f, 1f);    // Full volume at 0 distance
        rolloff.AddKey(10f, 0.8f); // 80% volume at 10 units
        rolloff.AddKey(30f, 0.3f); // 30% volume at 30 units
        rolloff.AddKey(50f, 0.15f);// almost silent at 50 units

        _audioSource = this.NPC!.GetComponent<AudioSource>();
        _audioSource.spatialBlend = 1f; // 3D sound
        _audioSource.rolloffMode = AudioRolloffMode.Logarithmic; // More realistic falloff
        _audioSource.playOnAwake = false;
        _audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, rolloff);
        _audioSource.maxDistance = 50f; // Can tweak based on how far you want it heard

        _gunshotSounds = new AudioClip[]
        {
            Resources.Load<AudioClip>("Sounds/gunshot1"),
            Resources.Load<AudioClip>("Sounds/gunshot2")
        };

        _targetEntity = target;
    }

    public override void Enter()
    {
        // nothing to do
    }

    public override void Exit()
    {
        // nothing to do
    }

    public override NPCStateReturnValue? Update()
    {
        if (_targetEntity!.IsAlive == false)
        {
            // target is dead, go back to idle state
            return new NPCStateReturnValue(NPCStateReturnValue.ActionType.PopState);
        }

        float distance = Vector3.Distance(this.NPC!.transform.position, this.NPC.target.position);
        if (distance > this.NPC!.stopDistance)
        {
            // target is out of range, go back to last state
            return new NPCStateReturnValue(NPCStateReturnValue.ActionType.PopState);
        }

        // turn in the direction of the target
        Vector3 direction = this.NPC.target.position - this.NPC.transform.position;
        direction.y = 0f; // Keep rotation flat
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            this.NPC.transform.rotation = Quaternion.RotateTowards(
                this.NPC.transform.rotation,
                targetRotation,
                this.NPC.rotationSpeed * Time.deltaTime
            );
        }

        if (Time.time >= nextFireTime)
        {
            Shoot();
            // get a random time between 0.5 and 1.5 seconds
            float randomTime = UnityEngine.Random.Range(0.5f, 1.5f);
            nextFireTime = Time.time + randomTime;
        }

        return null;
    }

    void Shoot()
    {
        var direction = this.NPC!.target.position - _firePoint.position;
        if (Physics.Raycast(_firePoint.position, direction, out RaycastHit hit, range))
        {
            this.NPC!.StartCoroutine(FireRayEffect(hit.point));

            // get the distance from the fire point to the hit point
            float distance = Vector3.Distance(_firePoint.position, hit.point);
            int damage = Mathf.RoundToInt(defaultDamage * (1 - (distance / range)));

            var targetEntity = hit.collider.GetComponent<GameEntity>();
            targetEntity?.TakeDamage(damage);
        }
        else
        {
            this.NPC!.StartCoroutine(FireRayEffect(_firePoint.position + direction * range));
        }

        _audioSource.PlayOneShot(_gunshotSounds[UnityEngine.Random.Range(0, _gunshotSounds.Length)]);
    }

    IEnumerator FireRayEffect(Vector3 hitPoint)
    {
        _lineRenderer.SetPosition(0, _firePoint.position);
        _lineRenderer.SetPosition(1, hitPoint);
        _lineRenderer.enabled = true;

        yield return new WaitForSeconds(0.05f);

        _lineRenderer.enabled = false;
        // // OnGunFired?.Invoke();
    }
}
