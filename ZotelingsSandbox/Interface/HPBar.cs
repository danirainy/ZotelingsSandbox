// Credit to https://github.com/ygsbzr/Enemy-HP-Bars
namespace ZotelingsSandbox;
internal static class EnemyHPBar
{
    [ModImportName(nameof(EnemyHPBar))]
    private static class EnemyHPBarImport
    {
#pragma warning disable 649
        public static Action<GameObject> DisableHPBar;
#pragma warning restore 649
    }
    static EnemyHPBar() => typeof(EnemyHPBarImport).ModInterop();
    public static void DisableHPBar(this GameObject go)
    {
        EnemyHPBarImport.DisableHPBar?.Invoke(go);
    }
}
internal class HPBar
{
    internal class Behavior : MonoBehaviour
    {
        private void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }
            float y;
            var collider2D = target.GetComponent<Collider2D>();
            if (collider2D != null && collider2D.enabled)
            {
                y = target.transform.position.y + collider2D.bounds.extents.y + 1;
            }
            else
            {
                y = target.transform.position.y + 1;
                foreach (var childCollider2D in target.GetComponentsInChildren<Collider2D>())
                {
                    if (childCollider2D.enabled && childCollider2D.gameObject.layer == target.layer)
                    {
                        y = Mathf.Max(y, target.transform.position.y + childCollider2D.bounds.extents.y + 1);
                    }
                }
            }
            gameObject.transform.localPosition = new Vector3(target.transform.position.x, y, 0);
            var hm = target.GetComponent<HealthManager>();
            if (hm == null)
            {
                Log.LogError("HealthManager not found");
                Destroy(gameObject);
                return;
            }
            var bar = gameObject.transform.Find("Bar");
            maxHP = Math.Max(maxHP, hm.hp);
            var percent = hm.hp * 1.0f / maxHP;
            bar.localPosition = new Vector3(-0.6f * (1 - percent), 0, 0);
            bar.localScale = new Vector3(percent, 1, 1);
        }
        public void SetVisible(bool visible)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                gameObject.transform.GetChild(i).gameObject.SetActive(visible);
            }
        }
        public GameObject target;
        public int maxHP;
    }
    internal class HPBarFinder : MonoBehaviour
    {
        public GameObject hpBar;
    }
    public static void Install(GameObject gameObject, int GroupID)
    {
        gameObject.DisableHPBar();
        if (frame is null)
        {
            frame = Load.LoadSprite("ZotelingsSandbox.Resources.HPBar.Frame.png");
            bar = Load.LoadSprite("ZotelingsSandbox.Resources.HPBar.Bar.png");
            background = Load.LoadSprite("ZotelingsSandbox.Resources.HPBar.Background.png");
        }
        var hpBar = new GameObject("ZotelingsSandbox.HPBar");
        hpBar.transform.localPosition = new Vector3(0, 0, 0);
        hpBar.transform.localScale = new Vector3(1.5f / gameObject.transform.localScale.x, 1.5f * 1.5f / gameObject.transform.localScale.y, 1);
        hpBar.AddComponent<Behavior>().target = gameObject;
        GameObject AddComponent(string name, float z, Sprite sprite)
        {
            var component = new GameObject(name);
            component.transform.SetParent(hpBar.transform);
            component.transform.localPosition = new Vector3(0, 0, z);
            component.transform.localScale = Vector3.one;
            var spriteRenderer = component.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            return component;
        }
        AddComponent("Frame", 0, frame);
        AddComponent("Bar", 1e-4f, bar);
        AddComponent("Background", 2e-4f, background);
        Color color;
        if (GroupID == 0)
        {
            color = new Color(94 / 255.0f, 22 / 255.0f, 117 / 255.0f);
        }
        else if (GroupID == 1)
        {
            color = new Color(238 / 255.0f, 66 / 255.0f, 102 / 255.0f);
        }
        else if (GroupID == 2)
        {
            color = new Color(51 / 255.0f, 115 / 255.0f, 87 / 255.0f);
        }
        else
        {
            Color backgroundColor = new Color(0x2A / 255f, 0x33 / 255f, 0x44 / 255f);
            float backgroundLuminance = 0.2126f * backgroundColor.r + 0.7152f * backgroundColor.g + 0.0722f * backgroundColor.b;
            Color GenerateHighContrastColor()
            {
                Color bestColor = Color.black;
                float bestContrast = 0;
                for (int i = 0; i < 2; i++)
                {
                    Color randomColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                    float colorLuminance = 0.2126f * randomColor.r + 0.7152f * randomColor.g + 0.0722f * randomColor.b;
                    float L1 = Mathf.Max(colorLuminance, backgroundLuminance);
                    float L2 = Mathf.Min(colorLuminance, backgroundLuminance);
                    float contrastRatio = (L1 + 0.05f) / (L2 + 0.05f);

                    if (contrastRatio > bestContrast)
                    {
                        bestContrast = contrastRatio;
                        bestColor = randomColor;
                    }
                }
                return bestColor;
            }
            UnityEngine.Random.InitState(GroupID);
            color = GenerateHighContrastColor();
        }
        hpBar.transform.Find("Bar").GetComponent<SpriteRenderer>().color = color;
        gameObject.AddComponent<HPBarFinder>().hpBar = hpBar;
    }
    private static Sprite frame;
    private static Sprite bar;
    private static Sprite background;
}
