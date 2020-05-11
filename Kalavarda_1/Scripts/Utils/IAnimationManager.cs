namespace Assets.Scripts
{
    public interface IAnimationManager
    {
        void SetState(AnimationState state);
    }

    public enum AnimationState
    {
        Idle,
        GoForward,
        GoBack,
        Jump,
        SimplePunch,
        Die
    }
}
