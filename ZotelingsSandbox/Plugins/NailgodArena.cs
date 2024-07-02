namespace ZotelingsSandbox.Plugins;
internal class NailgodArena : PluginBase
{
    private class KeepGlow : MonoBehaviour
    {
        private void Start()
        {
            glowResponse = GetComponent<GlowResponse>();
        }
        private void FixedUpdate()
        {
            glowResponse.Reflect().OnTriggerEnter2D(null);
        }
        private GlowResponse glowResponse;
    }
    public override List<(string, string)> GetPreloadNames()
    {
        var preloadNames = new List<(string, string)>();
        foreach (var keyword in Abyss_05Keywords)
        {
            preloadNames.Add(("Abyss_05", keyword));
        }
        foreach (var keyword in Deepnest_East_HornetKeywords)
        {
            preloadNames.Add(("Deepnest_East_Hornet", keyword));
        }
        foreach (var keyword in GG_Hollow_KnightKeywords)
        {
            preloadNames.Add(("GG_Hollow_Knight", keyword));
        }
        return preloadNames;
    }
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        foreach (var keyword in Abyss_05Keywords)
        {
            Abyss_05Templates.Add(preloadedObjects["Abyss_05"][keyword]);
        }
        foreach (var keyword in Deepnest_East_HornetKeywords)
        {
            var template = preloadedObjects["Deepnest_East_Hornet"][keyword];
            if (template.name.StartsWith("_Scenery"))
            {
                var whitelist = new List<string>()
                {
                    "white_ash_scenery_0007_8",
                    "white_ash_scenery_0007_8 (2)",
                    "white_ash_scenery_0007_8 (3)",
                    "white_ash_scenery_0007_8 (4)",
                    "blizzard_particles",
                    "deepnest_fog_02 (3)",
                    "deepnest_fog_02 (5)",
                    "deepnest_fog_02 (7)",
                    "deepnest_fog_02 (9)",
                };
                var children = new List<GameObject>();
                for (int i = 0; i < template.transform.childCount; i++)
                {
                    var child = template.transform.GetChild(i).gameObject;
                    children.Add(child);
                }
                foreach (var child in children)
                {
                    if (!whitelist.Contains(child.name))
                    {
                        UnityEngine.Object.Destroy(child);
                    }
                }
            }
            Deepnest_East_HornetTemplates.Add(template);
        }
        foreach (var keyword in GG_Hollow_KnightKeywords)
        {
            GG_Hollow_KnightTemplates.Add(preloadedObjects["GG_Hollow_Knight"][keyword]);
        }
    }
    public override void ActiveSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
    {
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene != "GG_Sly")
        {
            return;
        }
        if (ModHooks.GetMod("Nailgod") is Mod)
        {
            Log.LogKey("Plugin", "Nailgod is installed. Disabling NailgodArena plugin.");
            return;
        }
        foreach (var gameObject in to.GetAllGameObjects())
        {
            List<string> keywords = new List<string>
                {
                    "Active",
                    "Candle",
                    "bg_pillar",
                    "black_fader_GG",
                    "GG_crowd",
                    "GG_extra_walls",
                    "gg_gold_egg",
                    "GG_gold_egg",
                    "gg_incense",
                    "GG_scene_arena_extra",
                    "GG_scenery",
                    "gg_sly_candle_single",
                    "gg_sly_storeroom",
                    "GG_step",
                    "haze",
                    "throne",
                };
            List<string> whitelist = new List<string>
                {
                    "GG_scenery_0005_16_statue_clouds",
                    "GG_scenery_0005_16_statue_clouds (1)",
                    "GG_scenery_0005_16_statue_clouds (2)",
                    "GG_scenery_0005_16_statue_clouds (3)",
                };
            foreach (var keyword in keywords)
            {
                if (gameObject.name.StartsWith(keyword) && !whitelist.Contains(gameObject.name))
                {
                    UnityEngine.Object.Destroy(gameObject);
                }
            }
        }
        UnityEngine.Object.Destroy(GameObject.Find("Battle Scene").Find("Godseeker Crowd"));
        foreach (var template in Abyss_05Templates)
        {
            var item = UnityEngine.Object.Instantiate(template);
            item.SetActive(true);
            item.transform.Translate(-5, -12, 0);
            if (item.name.StartsWith("white_bridge"))
            {
                item.transform.Find("white_bridge_back").Translate(0, -0.7f, 0);
            }
            else if (item.name.StartsWith("_0026_white (1)"))
            {
                item.transform.Translate(0, -0.7f, 0);
            }
        }
        foreach (var template in Deepnest_East_HornetTemplates)
        {
            var item = UnityEngine.Object.Instantiate(template);
            item.SetActive(true);
            item.transform.Translate(20, -23, 0);
            if (item.name.StartsWith("_Scenery"))
            {
                for (int i = 0; i < item.transform.childCount; i++)
                {
                    var child = item.transform.GetChild(i).gameObject;
                    if (child.name.StartsWith("blizzard_particles"))
                    {
                        var fsm = child.GetComponent<PlayMakerFSM>();
                        fsm.ChangeTransition("Init", "FINISHED", "Active");
                    }
                    else if (child.name.StartsWith("white_ash_scenery"))
                    {
                        var pos = child.transform.position;
                        pos.z = 0.04f;
                        child.transform.position = pos;
                    }
                }
            }
            else if (item.name.StartsWith("Audio Player Blizzard"))
            {
                item.GetComponent<PlayMakerFSM>().SetState("State 2");
            }
            else if (item.name.StartsWith("pre_blizzard_particles"))
            {
                item.LocateMyFSM("FSM").SetState("Follow");
            }
        }
        int HK_glow_wallCount = 0;
        foreach (var template in GG_Hollow_KnightTemplates)
        {
            var item = UnityEngine.Object.Instantiate(template);
            item.SetActive(true);
            if (item.name.StartsWith("HK_glow_wall"))
            {
                HK_glow_wallCount += 1;
                if (HK_glow_wallCount == 1)
                {
                    item.transform.Translate(-0.5f, -2, 0);
                }
                else
                {
                    item.transform.Translate(2.5f, -2, 0);
                }
                foreach (var glowResponse in item.GetComponentsInChildren<GlowResponse>())
                {
                    glowResponse.gameObject.AddComponent<KeepGlow>();
                }
            }
        }
    }
    public override void BeginApplyMusicCue(On.AudioManager.orig_BeginApplyMusicCue orig, AudioManager self, MusicCue musicCue, float delayTime, float transitionTime, bool applySnapshot)
    {
    }
    List<string> Abyss_05Keywords = new List<string>()
    {
        "_0026_white (1)",
        "_0033_white_lamp1",
        "_0033_white_lamp1 (1)",
        "_0033_white_lamp1 (2)",
        "_0033_white_lamp1 (3)",
        "_0033_white_lamp1 (4)",
        "_0033_white_lamp2",
        "white_bridge 1",
        "white_bridge 1 (1)",
        "white_bridge 1 (2)",
        "white_bridge 1 (3)",
        "white_bridge 1 (4)",
        "white_floor",
        "white_floor (1)",
        "white_floor (2)",
        "white_floor (3)",
        "white_floor (4)",
        "white_floor (5)",
        "white_floor (6)",
        "white_floor (7)",
        "white_floor (8)",
        "white_floor (9)",
        "white_floor (11)",
    };
    List<GameObject> Abyss_05Templates = new List<GameObject>();
    List<string> Deepnest_East_HornetKeywords = new List<string>()
    {
        "_Scenery",
        "Audio Player Blizzard",
        "pre_blizzard_particles",
    };
    List<GameObject> Deepnest_East_HornetTemplates = new List<GameObject>();
    List<string> GG_Hollow_KnightKeywords = new List<string>()
    {
        "HK_glow_wall",
        "HK_glow_wall",
    };
    List<GameObject> GG_Hollow_KnightTemplates = new List<GameObject>();
}
