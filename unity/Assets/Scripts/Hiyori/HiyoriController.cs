using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// for Live2D model
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;

public class HiyoriController : MonoBehaviour
{

    private CubismModel model;

    // threshold to activate changes in whole model's x/z parameter
    // instead of just changes in head
    public float abs_body_roll_threshold = 30;
    public float abs_body_yaw_threshold = 30;
    public float abs_body_roll_yaw_max = 60;

    public float ear_max_threshold = 0.38f;
    public float ear_min_threshold = 0.30f;

    public float iris_left_ceiling = 0.2f;
    public float iris_right_ceiling = 0.85f;
    public float iris_up_ceiling = 0.8f;
    public float iris_down_ceiling = 0.2f;

    public float mar_max_threshold = 1.0f;
    public float mar_min_threshold = 0.0f;

    public bool change_mouth_form = false;
    public float mouth_dist_min = 60.0f;
    public float mouth_dist_max = 80.0f;

    private float t1;       // for breath
    public AvatarState state {get; set;}

    private bool blush = false;

    // Start is called before the first frame update
    void Start()
    {
        model = this.FindCubismModel();

        abs_body_roll_threshold = Mathf.Abs(abs_body_roll_threshold);
        abs_body_yaw_threshold = Mathf.Abs(abs_body_yaw_threshold);
        abs_body_roll_yaw_max = Mathf.Abs(abs_body_roll_yaw_max);
    }

    // Update is called once per frame
    void Update()
    {
        print(string.Format("Roll: {0:F}; Pitch: {1:F}; Yaw: {2:F}", state.roll, state.pitch, state.yaw));

        // control the blush of the avatar
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            if (blush == false)
                blush = true;
            else
                blush = false;
        }
    }

    // Apply all changes of control variables here~
    // https://docs.live2d.com/cubism-sdk-tutorials/about-parameterupdating-of-model/?locale=en_us
    // Tip 1
    void LateUpdate()
    {
        // yaw
        var parameter = model.Parameters[0];
        parameter.Value = -Mathf.Clamp(state.yaw, -30, 30);

        // pitch
        parameter = model.Parameters[1];
        parameter.Value = Mathf.Clamp(state.pitch, -30, 30);

        // roll
        parameter = model.Parameters[2];
        parameter.Value = -Mathf.Clamp(state.roll, -30, 30);

        // breath
        t1 += Time.deltaTime;
        float value = (Mathf.Sin(t1 * 3f) + 1) * 0.5f;
        parameter = model.Parameters[23];
        parameter.Value = value;

        if (blush) {
            parameter = model.Parameters[3];
            parameter.Value = 1;
        }
        else {
            parameter = model.Parameters[3];
            parameter.Value = 0;
        }

        EyeBlinking();

        IrisMovement();

        MouthOpening();

        if (change_mouth_form)
            MouthForm();

    }

    // whole body movement (body X/Z)
    // optional as the effect is not that pronounced
    void BodyMovement() {
        // roll
        var parameter = model.Parameters[22];
        if (Mathf.Abs(state.roll) > abs_body_roll_threshold) {
            parameter.Value = -(10 - 0) / (abs_body_roll_yaw_max - abs_body_roll_threshold) * ((Mathf.Abs(state.roll) - abs_body_roll_threshold) * Mathf.Sign(state.roll));
        }
        else {
            parameter.Value = 0;
        }

        // yaw
        parameter = model.Parameters[20];
        if (Mathf.Abs(state.yaw) > abs_body_yaw_threshold) {
            parameter.Value = -(10 - 0) / (abs_body_roll_yaw_max - abs_body_yaw_threshold) * ((Mathf.Abs(state.yaw) - abs_body_yaw_threshold) * Mathf.Sign(state.yaw));
        }
        else {
            parameter = model.Parameters[20];
            parameter.Value = 0;
        }
    }

    void EyeBlinking() {
        // my left eye = live2d's right (mirroring)
        state.ear_left = Mathf.Clamp(state.ear_left, ear_min_threshold, ear_max_threshold);
        float eye_L_value = (state.ear_left - ear_min_threshold) / (ear_max_threshold - ear_min_threshold) * 1;
        var parameter = model.Parameters[6];
        parameter.Value = eye_L_value;

        // my right eye = live2d's left (mirroring)
        state.ear_right = Mathf.Clamp(state.ear_right, ear_min_threshold, ear_max_threshold);
        float eye_R_value = (state.ear_right - ear_min_threshold) / (ear_max_threshold - ear_min_threshold) * 1;
        parameter = model.Parameters[4];
        parameter.Value = eye_R_value;
    }

    void IrisMovement() {
        float eyeball_x = (state.x_ratio_left + state.x_ratio_right) / 2;
        float eyeball_y = (state.y_ratio_left + state.y_ratio_right) / 2;

        eyeball_x = Mathf.Clamp(eyeball_x, iris_left_ceiling, iris_right_ceiling);
        eyeball_y = Mathf.Clamp(eyeball_y, iris_down_ceiling, iris_up_ceiling);

        // range is [-1, 1]
        eyeball_x = (eyeball_x - iris_left_ceiling) / (iris_right_ceiling - iris_left_ceiling) * 2 - 1;
        eyeball_y = (eyeball_y - iris_down_ceiling) / (iris_up_ceiling - iris_down_ceiling) * 2 - 1;

        // optional
        // pass the value to an "activation function"
        // to create a smoother effect (when the iris is near center)
        eyeball_x = Mathf.Pow(eyeball_x, 3);
        eyeball_y = Mathf.Pow(eyeball_y, 3);

        var parameter = model.Parameters[8];
        parameter.Value = eyeball_x;
        parameter = model.Parameters[9];
        parameter.Value = eyeball_y;
    }

    void MouthOpening() {
        // mouth aspect ratio -> mouth opening
        float mar_clamped = Mathf.Clamp(state.mouth_aspect_ratio, mar_min_threshold, mar_max_threshold);
        mar_clamped = (mar_clamped - mar_min_threshold) / (mar_max_threshold - mar_min_threshold) * 1;
        var parameter = model.Parameters[19];
        parameter.Value = mar_clamped;
    }

    void MouthForm() {
        // mouth distance -> mouth form
        float mouth_dist_clamped = Mathf.Clamp(state.mouth_dist, mouth_dist_min, mouth_dist_max);
        // range is [-1, 1]
        mouth_dist_clamped = (mouth_dist_clamped - mouth_dist_min) / (mouth_dist_max - mouth_dist_min) * 2 - 1;
        var parameter = model.Parameters[18];
        parameter.Value = mouth_dist_clamped;

    }
}
