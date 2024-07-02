namespace ZotelingsSandbox.Deploy;
internal class SharedObjects
{
    public static List<(string, string)> GetPreloadNames() => new()
    {
        ("Fungus1_26", "Moss Knight")
    };
    public static void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        var mossKnight = Load.Preload(preloadedObjects, "Fungus1_26", "Moss Knight");
        parryEffectPrefab = mossKnight.transform.Find("Slash Hitbox").gameObject;
    }
    public static void ActiveSceneChanged()
    {
        parryEffect = GameObject.Instantiate(parryEffectPrefab);
        parryEffect.name = "ZotelingsSandbox.ParryEffect";
        parryEffect.RemoveComponent<Collider2D>();
        parryEffect.SetActive(true);
    }
    private static GameObject parryEffectPrefab;
    public static GameObject parryEffect;
}
