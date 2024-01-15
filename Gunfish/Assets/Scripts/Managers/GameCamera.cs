using Cinemachine;

public class GameCamera : Singleton<GameCamera> {
    public CinemachineTargetGroup targetGroup;

    public void Start() {
        targetGroup.AddMember(transform, 1, 1);
    }
}