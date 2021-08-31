// All data for an avatar 'state'
public class AvatarState {
    public float roll = 0, pitch = 0, yaw = 0;
    public float ear_left = 0, ear_right = 0;
    public float mouth_aspect_ratio = 0, mouth_dist = 0;
    // Generally for Live2D
    public float x_ratio_left = 0, y_ratio_left = 0, x_ratio_right = 0, y_ratio_right = 0;
}

// Interface for avatars - describes controlling them.
interface IAvatar {
    AvatarState state {set;}
}
