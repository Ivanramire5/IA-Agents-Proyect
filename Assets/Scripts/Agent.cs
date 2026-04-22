using UnityEngine;
using System.Collections.Generic;

public class Agent : MonoBehaviour
{
    private enum SteeringModes { Seek, Flee, Arrive, Pursuit, Evade, Flocking }

    [Header("Stats")]
    [SerializeField]
    private float _maxSpeed = 5f;
    [SerializeField]
    private float _maxForce = 10f;
    [SerializeField]
    private float _viewRadius = 5f;
    [SerializeField]
    private SteeringModes _currentMode;
    [SerializeField]
    private float _arriveRadius = 3f;

    [Header("Flocking values")]
    [SerializeField]
    private float _separationRadius = 2f;
    [SerializeField, Range(0f, 3f)]
    private float _separationWeight = 1f;
    [SerializeField, Range(0f, 3f)]
    private float _cohesionWeight = 1f;
    [SerializeField, Range(0f, 3f)]
    private float _alignmentWeight = 1f;

    [Header("References")]
    [SerializeField]
    private Transform target;
    [SerializeField]
    private Agent targetAgent;

    [Header("Interactions")]
    [SerializeField] private string _targetTagObject = "InterestObject";
    [SerializeField] private string _targetTagHunter = "Hunter";
    [SerializeField] private float _damagePerSecond = 20f;
    [SerializeField] private float _eatDistance = 1.5f;

    private static readonly List<Agent> _allAgents = new();

    private Vector3 _velocity;
    public Vector3 Velocity => _velocity;

    private void Awake()
    {
        _allAgents.Add(this);
    }

    private void Start()
    {
        Vector3 randomVector = new(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        _velocity += randomVector.normalized * _maxSpeed;
    }

    private void Update()
    {
        Vector3 steeringForce = Vector3.zero;
        Collider[] closeObjects = Physics.OverlapSphere(transform.position, _viewRadius);
        
        Transform hunterTransform = null;
        Transform ObjectTransform = null;

        foreach (var col in closeObjects)
        {
            if (col.CompareTag(_targetTagHunter)) 
            {
                hunterTransform = col.transform;
                break;
            }
            if (col.CompareTag(_targetTagObject))
            {
                ObjectTransform = col.transform;
            }
        }
        if (hunterTransform != null)
        {
            _currentMode = SteeringModes.Evade;
            Agent hunterAgent = hunterTransform.GetComponent<Agent>();
            
            if (hunterAgent != null) 
                steeringForce += CalculateEvade(hunterAgent);
            else 
                steeringForce += CalculateFlee(hunterTransform.position);
        }
        else if (ObjectTransform != null)
        {
            _currentMode = SteeringModes.Arrive;
            steeringForce += CalculateArrive(ObjectTransform.position);
            float dist = Vector3.Distance(transform.position, ObjectTransform.position);
            if (dist <= _eatDistance)
            {
                InterestObject targetScript = ObjectTransform.GetComponent<InterestObject>();
                if (targetScript != null) targetScript.RecieveDamage(_damagePerSecond * Time.deltaTime);
            }
        }
        else
        {
            _currentMode = SteeringModes.Flocking;
            steeringForce += CalculateAlignment(_allAgents, _viewRadius) * _alignmentWeight;
            steeringForce += CalculateCohesion(_allAgents, _viewRadius) * _cohesionWeight;
        }
        steeringForce += CalculateSeparation(_allAgents, _separationRadius) * _separationWeight;
        _velocity += steeringForce;
        _velocity = Vector3.ClampMagnitude(_velocity, _maxSpeed);

        transform.position += _velocity * Time.deltaTime;
        if (_velocity.sqrMagnitude > 0.01f) transform.forward = _velocity;
        transform.position = Bounds.Instance.CalculateBoundPosition(transform.position);
    }

    #region Steering Behaviors

    #region Flocking
    private void CalculateFlocking()
    {
        _velocity += CalculateSeparation(_allAgents, _separationRadius) * _separationWeight
                    + CalculateAlignment(_allAgents, _viewRadius) * _alignmentWeight
                    + CalculateCohesion(_allAgents, _viewRadius) * _cohesionWeight;
    }

    private void ApplyForce(Vector3 force) => _velocity = Vector3.ClampMagnitude(_velocity + force, _maxSpeed);

    private Vector3 CalculateCohesion(IEnumerable<Agent> agents, float radius)
    {
        Vector3 desiredPosition = default;
        int count = 0;

        foreach (Agent item in agents)
        {
            if (item == this) continue;
            if (InRange(item.transform.position, radius))
            {
                desiredPosition += item.transform.position;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        desiredPosition /= count;

        return CalculateSeek(desiredPosition);
    }

    private Vector3 CalculateSeparation(IEnumerable<Agent> agents, float radius)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;

        foreach (Agent item in agents)
        {
            if (item == this) continue;
            
            float distanceSqr = (item.transform.position - transform.position).sqrMagnitude;
            
            if (distanceSqr <= radius * radius && distanceSqr > 0.0001f) 
            {
                Vector3 awayFromNeighbor = transform.position - item.transform.position;
                
                desired += awayFromNeighbor.normalized / Mathf.Sqrt(distanceSqr);
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        desired /= count;

        return CalculateSteering(desired.normalized * _maxSpeed); 
    }

    private Vector3 CalculateAlignment(List<Agent> agents, float radius)
    {
        Vector3 desired = default;
        int count = 0;

        foreach (Agent item in agents)
        {
            if (item == this) continue;
            if (InRange(item.transform.position, radius))
            {
                desired += item.Velocity;
                count++;
            }
        }
        if (count == 0) return Vector3.zero;
        desired /= count;

        return CalculateSteering(desired.normalized * _maxSpeed);
    }

    private bool InRange(Vector3 position, float radius) => (position - transform.position).sqrMagnitude <= radius * radius;

    #endregion

    private Vector3 CalculatePursuit(Agent target) => CalculateSeek(GetFuturePosition(target));
    private Vector3 CalculateEvade(Agent target) => CalculateFlee(GetFuturePosition(target));
    private Vector3 GetFuturePosition(Agent target)
    {
        float distanceToTarget = (target.transform.position - transform.position).magnitude;
        float predictedTime = distanceToTarget / (_maxSpeed + target.Velocity.magnitude);
        return target.transform.position + target.Velocity * predictedTime;
    }

    private Vector3 CalculateSeek(Vector3 targetPosition)
    {
        Vector3 desired = (targetPosition - transform.position).normalized * _maxSpeed;
        return CalculateSteering(desired);
    }

    private Vector3 CalculateFlee(Vector3 targetPosition)
    {
        Vector3 desired = (targetPosition - transform.position).normalized * _maxSpeed;
        return CalculateSteering(-desired);
    }

    private Vector3 CalculateArrive(Vector3 targetPosition)
    {
        Vector3 dir = (targetPosition - transform.position);
        float speed = _maxSpeed;
        float distance = dir.magnitude;

        if (distance <= _arriveRadius)
        {
            float percentDistance = distance / _arriveRadius;
            speed *= percentDistance;
        }

        Vector3 desired = dir.normalized * speed;
        return CalculateSteering(desired);
    }

    #endregion

    #region Steering
    private Vector3 GetCurrentSteeringMode() => GetSteering(_currentMode, target.position);
    private Vector3 GetSteering(SteeringModes mode, Vector3 targetPosition)
    {
        return mode switch
        {
            SteeringModes.Seek => CalculateSeek(targetPosition),
            SteeringModes.Flee => CalculateFlee(targetPosition),
            SteeringModes.Arrive => CalculateArrive(targetPosition),
            _ => Vector3.zero,
        };
    }

    private Vector3 CalculateSteering(Vector3 desired)
    {
        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxForce);
        return steering * Time.deltaTime;
    }
    #endregion

    private void OnDestroy()
    {
        _allAgents.Remove(this);
    }

    private void OnDrawGizmos()
    {
        if (_currentMode == SteeringModes.Arrive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, _arriveRadius);
        }
        else if (_currentMode == SteeringModes.Pursuit)
        {
            Agent target = targetAgent;
            float distanceToTarget = (target.transform.position - transform.position).magnitude;
            float predictedTime = distanceToTarget / (_maxSpeed + target.Velocity.magnitude);
            Vector3 futurePos = target.transform.position + target.Velocity * predictedTime;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(futurePos, 0.25f);
            Gizmos.DrawLine(targetAgent.transform.position, futurePos);
        }
        else if (_currentMode == SteeringModes.Flocking)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _separationRadius);
        }
        else
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, _viewRadius);
        }

    }
}
