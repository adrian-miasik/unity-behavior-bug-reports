using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Agent navigates to location while looking at QueueBase", story: "[Agent] navigates to [location] while looking at [QueueBase]", category: "Action", id: "955bce537bc45f0af9e504726b1c3d13")]
public partial class AgentNavigatesToLocationWhileLookingAtQueueBaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Location;
    [SerializeReference] public BlackboardVariable<QueueBase> QueueBase;
    [SerializeReference] public BlackboardVariable<float> Speed = new(1.0f);
    [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new(0.2f);

    [SerializeReference]
    public BlackboardVariable<string> AnimatorSpeedParam = new("SpeedMagnitude");

    // This will only be used in movement without a navigation agent.
    [SerializeReference] public BlackboardVariable<float> SlowDownDistance = new(1.0f);

    private float m_PreviousStoppingDistance;
    private NavMeshAgent m_NavMeshAgent;
    private Animator m_Animator;

    protected override Status OnStart()
    {
        if (ReferenceEquals(Agent?.Value, null) || ReferenceEquals(Location?.Value, null))
        {
            return Status.Failure;
        }

        return Initialize();
    }

    protected override Status OnUpdate()
    {
        if (ReferenceEquals(Agent?.Value, null) || ReferenceEquals(Location, null))
        {
            return Status.Failure;
        }

        if (m_NavMeshAgent == null)
        {
            Vector3 agentPosition, locationPosition;
            float distance = GetDistanceToLocation(out agentPosition, out locationPosition);
            if (distance <= DistanceThreshold)
            {
                return Status.Success;
            }

            float speed = Speed;

            if (SlowDownDistance > 0.0f && distance < SlowDownDistance)
            {
                float ratio = distance / SlowDownDistance;
                speed = Mathf.Max(0.1f, Speed * ratio);
            }

            Vector3 toDestination = locationPosition - agentPosition;
            toDestination.y = 0.0f;
            toDestination.Normalize();
            agentPosition += toDestination * (speed * Time.deltaTime);
            Agent.Value.transform.position = agentPosition;

            // Look at the QueueSlot
            Agent.Value.transform.LookAt(QueueBase.Value.transform.position);
        }
        else if (!m_NavMeshAgent.pathPending && m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance)
        {
            return Status.Success;
        }

        return Status.Running;
    }
    
    protected override void OnEnd()
    {
        if (m_Animator != null)
        {
            m_Animator.SetFloat(AnimatorSpeedParam, 0);
        }

        if (m_NavMeshAgent != null)
        {
            if (m_NavMeshAgent.isOnNavMesh)
            {
                m_NavMeshAgent.ResetPath();
            }

            m_NavMeshAgent.stoppingDistance = m_PreviousStoppingDistance;
        }

        m_NavMeshAgent = null;
        m_Animator = null;
    }

    protected override void OnDeserialize()
    {
        Initialize();
    }

    private Status Initialize()
    {
        if (GetDistanceToLocation(out Vector3 agentPosition, out Vector3 locationPosition) <= DistanceThreshold)
        {
            return Status.Failure;
        }

        // If using animator, set speed parameter.
        m_Animator = Agent.Value.GetComponentInChildren<Animator>();
        if (m_Animator != null)
        {
            m_Animator.SetFloat(AnimatorSpeedParam, Speed);
        }

        // If using a navigation mesh, set target position for navigation mesh agent.
        m_NavMeshAgent = Agent.Value.GetComponentInChildren<NavMeshAgent>();
        if (m_NavMeshAgent != null)
        {
            if (m_NavMeshAgent.isOnNavMesh)
            {
                m_NavMeshAgent.ResetPath();
            }

            m_NavMeshAgent.speed = Speed;
            m_PreviousStoppingDistance = m_NavMeshAgent.stoppingDistance;
            m_NavMeshAgent.stoppingDistance = DistanceThreshold;
            m_NavMeshAgent.SetDestination(locationPosition);
        }

        return Status.Running;
    }

    private float GetDistanceToLocation(out Vector3 agentPosition, out Vector3 locationPosition)
    {
        agentPosition = Agent.Value.transform.position;
        locationPosition = Location.Value;
        return Vector3.Distance(new Vector3(agentPosition.x, locationPosition.y, agentPosition.z), locationPosition);
    }
}