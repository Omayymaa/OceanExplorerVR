using UnityEngine;

[DefaultExecutionOrder(1000)]
public class FirstPersonBody : MonoBehaviour
{
    public static bool ShowBody;

    Animator animator;
    Transform[] hidden;
    Vector3[] hiddenScale;
    Transform rightArm;
    Vector3 rightArmScale = Vector3.one;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null || !animator.isHuman)
        {
            Debug.LogWarning("FirstPersonBody : l'Avatar de " + name + " doit être Humanoïde, avec un Animator sur le même objet.");
            return;
        }

        HumanBodyBones[] bones =
        {
            HumanBodyBones.Head,
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg,
            HumanBodyBones.LeftLowerLeg, HumanBodyBones.RightLowerLeg,
            HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot,
            HumanBodyBones.LeftToes, HumanBodyBones.RightToes
        };

        hidden = new Transform[bones.Length];
        hiddenScale = new Vector3[bones.Length];

        for (int i = 0; i < bones.Length; i++)
        {
            hidden[i] = animator.GetBoneTransform(bones[i]);
            hiddenScale[i] = hidden[i] != null ? hidden[i].localScale : Vector3.one;
        }

        rightArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        if (rightArm != null) rightArmScale = rightArm.localScale;
    }

    void LateUpdate()
    {
        if (hidden == null) return;

        for (int i = 0; i < hidden.Length; i++)
            if (hidden[i] != null)
                hidden[i].localScale = ShowBody ? hiddenScale[i] : Vector3.zero;

        if (rightArm != null)
            rightArm.localScale = (BoatController.Driving && !ShowBody) ? Vector3.zero : rightArmScale;
    }
}