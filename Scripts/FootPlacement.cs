using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class FootPlacement : MonoBehaviour
{
    private TwoBoneIKConstraint L_Foot_Constraint;
    private TwoBoneIKConstraint R_Foot_Constraint;

    [SerializeField]
    private Transform R_Foot_Target;
    [SerializeField]
    private Transform L_Foot_Target;

    [SerializeField]
    private GameObject _character;
    private IKillable _characterKillable;

    [SerializeField]
    private NavMeshAgent _agent;
    private Rigidbody _rb;
    private Animator _animator;

    [SerializeField]
    private LayerMask _LayerMask;

    [SerializeField]
    private float _rayOffset, _rayDistance;

    //private Vector3 R_Foot_TargetPos, R_Foot_TargetRot, R_Foot_TargetNormal;
    //private Vector3 L_Foot_TargetPos, L_Foot_TargetRot, L_Foot_TargetNormal;

    private void Awake()
    {
        L_Foot_Constraint = transform.Find("L_Foot_IK").GetComponent<TwoBoneIKConstraint>();
        R_Foot_Constraint = transform.Find("R_Foot_IK").GetComponent<TwoBoneIKConstraint>();
        _animator = _agent.GetComponentInChildren<Animator>();
        _rb = _agent.GetComponent<Rigidbody>();
        _characterKillable = _character.GetComponent<IKillable>();
    }

    void Update()
    {
        if (_characterKillable.IsDead) return;

        if (_agent.enabled)
        {
            ArrangeRigWeights(_agent.velocity, false);
        }
        else if (_rb != null)
        {
            ArrangeRigWeights(_rb.velocity, true);
        }
        
    }
    private void ArrangeRigWeights(Vector3 velocity, bool isUsingRb)
    {
        if (velocity.magnitude < 0.75f)
        {
            L_Foot_Constraint.data.hintWeight = Mathf.Lerp(L_Foot_Constraint.data.hintWeight, 1f, Time.deltaTime * 5f);
            L_Foot_Constraint.data.targetPositionWeight = Mathf.Lerp(L_Foot_Constraint.data.targetPositionWeight, 1f, Time.deltaTime * 5f);
            R_Foot_Constraint.data.hintWeight = Mathf.Lerp(R_Foot_Constraint.data.hintWeight, 1f, Time.deltaTime * 5f);
            R_Foot_Constraint.data.targetPositionWeight = Mathf.Lerp(R_Foot_Constraint.data.targetPositionWeight, 1f, Time.deltaTime * 5f);
            L_Foot_Constraint.data.targetRotationWeight = Mathf.Lerp(L_Foot_Constraint.data.targetRotationWeight, 1f, Time.deltaTime * 8f);
            R_Foot_Constraint.data.targetRotationWeight = Mathf.Lerp(R_Foot_Constraint.data.targetRotationWeight, 1f, Time.deltaTime * 8f);
        }
        else
        {
            L_Foot_Constraint.data.hintWeight = Mathf.Lerp(L_Foot_Constraint.data.hintWeight, 0.25f, Time.deltaTime * 8f);
            L_Foot_Constraint.data.targetPositionWeight = Mathf.Lerp(L_Foot_Constraint.data.targetPositionWeight, 0.25f, Time.deltaTime * 8f);
            R_Foot_Constraint.data.hintWeight = Mathf.Lerp(R_Foot_Constraint.data.hintWeight, 0.25f, Time.deltaTime * 8f);
            R_Foot_Constraint.data.targetPositionWeight = Mathf.Lerp(R_Foot_Constraint.data.targetPositionWeight, 0.25f, Time.deltaTime * 8f);
            if (isUsingRb)
            {
                L_Foot_Constraint.data.targetRotationWeight = Mathf.Lerp(L_Foot_Constraint.data.targetRotationWeight, 0.25f, Time.deltaTime * 8f);
                R_Foot_Constraint.data.targetRotationWeight = Mathf.Lerp(R_Foot_Constraint.data.targetRotationWeight, 0.25f, Time.deltaTime * 8f);
            }
            else
            {
                L_Foot_Constraint.data.targetRotationWeight = Mathf.Lerp(L_Foot_Constraint.data.targetRotationWeight, 1f, Time.deltaTime * 8f);
                R_Foot_Constraint.data.targetRotationWeight = Mathf.Lerp(R_Foot_Constraint.data.targetRotationWeight, 1f, Time.deltaTime * 8f);
            }
        }
    }

    private void LateUpdate()
    {
        if (_characterKillable.IsDead) return;

        Ray ray = new Ray(R_Foot_Target.position + Vector3.up * _rayOffset, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, _rayDistance, _LayerMask))
        {
            R_Foot_Target.position = hit.point;
            R_Foot_Target.localPosition = new Vector3(R_Foot_Target.localPosition.x, Mathf.Clamp(R_Foot_Target.localPosition.y, -0.1f, 0.15f), R_Foot_Target.localPosition.z);
            R_Foot_Target.up = hit.normal;
            R_Foot_Target.Rotate(new Vector3(0f, _agent.transform.localEulerAngles.y, 0f), Space.Self);
        }

        ray = new Ray(L_Foot_Target.position + Vector3.up * _rayOffset, Vector3.down);
        if (Physics.Raycast(ray, out hit, _rayDistance, _LayerMask))
        {
            L_Foot_Target.position = hit.point;
            L_Foot_Target.localPosition = new Vector3(L_Foot_Target.localPosition.x, Mathf.Clamp(L_Foot_Target.localPosition.y, -0.1f, 0.15f), L_Foot_Target.localPosition.z);
            L_Foot_Target.up = hit.normal;
            L_Foot_Target.Rotate(new Vector3(0f, _agent.transform.localEulerAngles.y, 0f), Space.Self);
        }
    }
}
