public static class PlatformConfig {
    public static bool IsCabinet {
        get {
#if UNITY_EDITOR
            if (DevConfigOverride.TryGetSimulatedBuildTarget(out var target))
                return target == DevConfigOverride.SimulatedBuildTarget.ArcadeCabinet;
#endif
#if ARCADE_CABINET
            return true;
#else
            return false;
#endif
        }
    }
}
