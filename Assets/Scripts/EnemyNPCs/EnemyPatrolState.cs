#nullable enable

using UnityEngine;
using UnityEngine.AI;

[NPCStateTag("EnemyPatrol")]
public class EnemyPatrolState : NPCState
{
    private NavMeshAgent _agent;
    private readonly EnemyNPC _enemyNPC;

    private readonly int _targetMask = LayerMask.GetMask("Player", "FriendlyNPC");
    private readonly int _obstacleMask = LayerMask.GetMask("Default");

    private int _currentIndex = 0;
    private float _timer = 0f;
    private readonly float _scanInterval = 0.5f;

    private EntityScanner _entityScanner;

    public EnemyPatrolState(EnemyNPC enemyNPC) 
        : base(enemyNPC)
    {
        if (this.NPC == null)
        {
            throw new System.Exception("BaseNPC is not an EnemyNPC. Cannot enter patrol state.");
        }

        _enemyNPC = enemyNPC;
        _agent = this.NPC.GetComponent<NavMeshAgent>();

        _entityScanner = new EntityScanner
        {
            ViewDistance = _enemyNPC.ViewDistance,
            ViewAngle = _enemyNPC.ViewAngle,
            SourceTransform = this.NPC.transform,
            EyeOffset = _enemyNPC.EyeOffset,
            TargetMask = _targetMask,
            ObstacleMask = _obstacleMask
        };
    }

    public override void Enter()
    {
        if (_enemyNPC.PatrolPoints.Length > 0)
        {
            _agent.SetDestination(_enemyNPC.PatrolPoints[_currentIndex].position);
        }
    }

    public override INPCState? Update()
    {
        if (!_agent.pathPending && _agent.remainingDistance < _enemyNPC.ArrivalThreshold)
        {
            _currentIndex = (_currentIndex + 1) % _enemyNPC.PatrolPoints.Length;
            _agent.SetDestination(_enemyNPC.PatrolPoints[_currentIndex].position);
        }

        _timer += Time.deltaTime;
        if (_timer >= _scanInterval)
        {
            foreach(var collider in _entityScanner.doScan())
            {
                Debug.Log($"Target detected: {collider.name}");
            }
            _timer = 0f;
        }

        return null;
    }

    public override void Exit()
    {
        // nothing to do
    }
}
