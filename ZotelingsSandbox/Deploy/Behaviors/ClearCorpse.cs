namespace ZotelingsSandbox.Deploy.Behaviors;
internal class ClearCorpse : MonoBehaviour
{
    private void Start()
    {
        tk2DSprite = GetComponent<tk2dSprite>();
        timer = 10;
    }
    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            var a = tk2DSprite.color.a - Time.deltaTime;
            if (a < 0)
            {
                GameObject.Destroy(gameObject);
            }
            else
            {
                tk2DSprite.color = new Color(tk2DSprite.color.r, tk2DSprite.color.g, tk2DSprite.color.b, a);
            }
        }
    }
    private tk2dSprite tk2DSprite;
    private float timer;
}
