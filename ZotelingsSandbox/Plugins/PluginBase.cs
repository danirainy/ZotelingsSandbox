namespace ZotelingsSandbox.Plugins;
internal class PluginBase
{
    public virtual List<(string, string)> GetPreloadNames()
    {
        return [];
    }
    public virtual void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
    }
    public virtual void ActiveSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
    {
    }
    public virtual void BeginApplyMusicCue(On.AudioManager.orig_BeginApplyMusicCue orig, AudioManager self, MusicCue musicCue, float delayTime, float transitionTime, bool applySnapshot)
    {
    }
}
