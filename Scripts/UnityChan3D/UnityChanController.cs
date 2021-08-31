using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UnityChanController : MonoBehaviour, IAvatar
{
    private Animator anim;

    // for blinking stuff
    public SkinnedMeshRenderer ref_SMR_EYE_DEF;	//EYE_DEFへの参照
    public SkinnedMeshRenderer ref_SMR_EL_DEF;	//EL_DEFへの参照

    public float max_rotation_angle = 45.0f;

    public float ear_max_threshold = 0.38f;
    public float ear_min_threshold = 0.30f;

    [HideInInspector]
    public float eye_ratio_close = 85.0f;
    [HideInInspector]
    public float eye_ratio_half_close = 20.0f;
    [HideInInspector]
    public float eye_ratio_open = 0.0f;

    // for mouth movement stuff
    public SkinnedMeshRenderer ref_SMR_MTH_DEF;     // the mouth component

    public float mar_max_threshold = 1.0f;
    public float mar_min_threshold = 0.0f;

    private Transform neck;
    private Quaternion neck_quat;

    public AvatarState state {get; set;}

    // Start is called before the first frame update
    void Start()
    {
        state = new AvatarState();
        anim = GetComponent<Animator> ();
        neck = anim.GetBoneTransform(HumanBodyBones.Neck);
        neck_quat = Quaternion.Euler(0, 90, -90);
        SetEyes(eye_ratio_open);
    }

    // Update is called once per frame
    void Update()
    {
        // print(string.Format("Roll: {0:F}; Pitch: {1:F}; Yaw: {2:F}", roll, pitch, yaw));
        // print(string.Format("Left eye: {0:F}, {1:F}; Right eye: {2:F}, {3:F}",
        //     x_ratio_left, y_ratio_left, x_ratio_right, y_ratio_right));

        // do rotation at neck to control the movement of head
        HeadRotation();

        EyeBlinking();

        MouthMoving();
    }


    void HeadRotation()
    {
        // clamp the angles to prevent unnatural movement
        float pitch_clamp = Mathf.Clamp(state.pitch, -max_rotation_angle, max_rotation_angle);
        float yaw_clamp = Mathf.Clamp(state.yaw, -max_rotation_angle, max_rotation_angle);
        float roll_clamp = Mathf.Clamp(state.roll, -max_rotation_angle, max_rotation_angle);

        // do rotation at neck to control the movement of head
        neck.rotation = Quaternion.Euler(pitch_clamp, yaw_clamp, roll_clamp) * neck_quat;
        // neck.rotation = new Quaternion(qx, qy, qz, qw) * neck_quat;
    }

    // for eye blinking effects
    void EyeBlinking()
    {
        float ear_min = Mathf.Min(state.ear_left, state.ear_right);
        ear_min = Mathf.Clamp(ear_min, ear_min_threshold, ear_max_threshold);
        float x = Mathf.Abs((ear_min - ear_min_threshold) / (ear_max_threshold - ear_min_threshold) - 1);
        // formula found using desmos regression
        // dependent on the eye_ratio_half_close and eye_ratio_close
        float y = 90 * Mathf.Pow(x, 2) - 5 * x;
        SetEyes(y);
    }

    void SetEyes(float ratio) {
        ref_SMR_EYE_DEF.SetBlendShapeWeight (6, ratio);
        ref_SMR_EL_DEF.SetBlendShapeWeight (6, ratio);
    }

    // for mouth movement
    void MouthMoving() {
        float mar_clamped = Mathf.Clamp(state.mouth_aspect_ratio, mar_min_threshold, mar_max_threshold);
        float ratio = (mar_clamped - mar_min_threshold) / (mar_max_threshold - mar_min_threshold);
        // enlarge it to [0, 100]
        ratio = ratio * 100 / (mar_max_threshold - mar_min_threshold);
        SetMouth(ratio);
    }

    void SetMouth(float ratio)
    {
        ref_SMR_MTH_DEF.SetBlendShapeWeight(2, ratio);
    }
}
