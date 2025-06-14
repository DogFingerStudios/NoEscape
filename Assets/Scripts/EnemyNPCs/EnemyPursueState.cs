#nullable enable

using UnityEngine;

[NPCStateTag("EnemyPursue")]
public class EnemyPursueState : NPCState
{
    private UnityEngine.AI.NavMeshAgent? _agent = null;
    private AudioClip? _warningSound;
    private AudioClip? _willFindYouSound;

    private readonly GameEntity _targetEntity;
    

    // TODO: this is a poor man's way to stop chasing, eventually we will want to be a 
    // little smarter -- for example, if the NPC cannot "see" the target, then the NPC could
    // go to the last position it saw the target, and if the target is not in range or
    // not visible, then the NPC could return to patrol state
    private float _detectionRange;

    /// <param name="npc">The NPC to which this state is attached.</param>
    /// <param name="target">The target Transform that the NPC will pursue.</param>
    public EnemyPursueState(BaseNPC npc, GameEntity target)
        : base(npc)
    {
        // TODO: CHANGE ME!!
        this.NPC!.target = target.transform;

        if (this.NPC is not EnemyNPC)
        {
            throw new System.Exception("BaseNPC is not an EnemyNPC. Cannot enter pursue state.");
        }

        _warningSound = Resources.Load<AudioClip>("Sounds/Freeze");
        _willFindYouSound = Resources.Load<AudioClip>("Sounds/IWillFindYou");

        _targetEntity = this.NPC!.target!.GetComponent<GameEntity>();

    }

    public override void Enter()
    {
        _agent = this.NPC!.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (_agent == null)
        {
            throw new System.Exception("NavMeshAgent component is missing on the NPC.");
        }

        _detectionRange = this.NPC.DetectionDistance;
        this.NPC!.AudioSource.dopplerLevel = 0f;
        this.NPC!.AudioSource.spatialBlend = 1f;
        this.NPC!.AudioSource.PlayOneShot(_warningSound);
    }

    public override void Exit()
    {
        // nothing to do
    }

    public override NPCStateReturnValue? Update()
    {
        if (_agent == null) return null;
        if (!_targetEntity!.IsAlive)
        {
            // target is dead, go back to idle state
            _agent.isStopped = true;
            _agent.ResetPath();

            return new NPCStateReturnValue(
                NPCStateReturnValue.ActionType.PopState);
        }
        
        float distance = Vector3.Distance(this.NPC!.transform.position, this.NPC.target.position);
        if (distance < this.NPC!.stopDistance)
        {
            _agent.isStopped = true;
            _agent.ResetPath();

            this.NPC.PushState(this);
            return new NPCStateReturnValue(
                NPCStateReturnValue.ActionType.ChangeState,
                new EnemyAttackState(this.NPC, _targetEntity));
        }

        if (distance <= _detectionRange)
        {
            _agent.SetDestination(this.NPC.target.transform.position);
        }
        else
        {
            this.NPC!.AudioSource.PlayOneShot(_willFindYouSound);
            _agent.isStopped = true;
            _agent.ResetPath();

            // target is out of range, go back to idle state which we pushed earlier
            return new NPCStateReturnValue(
                NPCStateReturnValue.ActionType.PopState);
        }

        return null;
    }
}
