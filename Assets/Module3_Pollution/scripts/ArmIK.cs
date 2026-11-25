using UnityEngine;

public class ArmIK : MonoBehaviour
{
    public Animator animator;
    public Transform carryPoint;
    public Transform elbowHint;
    public float speed = 10f;

    [HideInInspector] public Transform goal;
    [HideInInspector] public bool active;

    float weight;
    Vector3 pos;
    Quaternion rot;
    bool init;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        weight = Mathf.MoveTowards(weight, active ? 1f : 0f, Time.deltaTime * 4f);
    }

    bool logged;

    void OnAnimatorIK(int layerIndex)
    {
        if (!logged)
        {
            Debug.Log("ArmIK : OnAnimatorIK appelé (layer " + layerIndex + ") | poids " + weight + " | active " + active);
            logged = true;
        }

        if (animator == null || carryPoint == null) return;

        if (!init)
        {
            Transform h = animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (h == null) return;
            pos = h.position;
            rot = h.rotation;
            init = true;
        }

        Vector3 target = goal != null ? goal.position : carryPoint.position;
        pos = Vector3.Lerp(pos, target, speed * Time.deltaTime);
        rot = Quaternion.Slerp(rot, carryPoint.rotation, speed * Time.deltaTime);

        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, weight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, weight);
        animator.SetIKPosition(AvatarIKGoal.RightHand, pos);
        animator.SetIKRotation(AvatarIKGoal.RightHand, rot);

        if (elbowHint != null)
        {
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, weight);
            animator.SetIKHintPosition(AvatarIKHint.RightElbow, elbowHint.position);
        }
    }
}