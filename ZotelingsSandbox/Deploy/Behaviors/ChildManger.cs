namespace ZotelingsSandbox.Deploy.Behaviors;
internal class ChildManager : MonoBehaviour
{
    public void SwitchTo(string newTargetName)
    {
        targetName = newTargetName;
        foreach (var child in controllableChildren)
        {
            child.Value.SetActive(child.Key == targetName);
        }
        var heroController = HeroController.instance;
        var heroCollider = heroController.GetComponent<BoxCollider2D>();
        var target = controllableChildren[targetName];
        var targetCollider = target.GetComponent<BoxCollider2D>();
        heroCollider.offset = targetCollider.offset;
        heroCollider.size = targetCollider.size;
        var heroBoxCollider = heroController.transform.Find("HeroBox").GetComponent<BoxCollider2D>();
        heroBoxCollider.offset = targetCollider.offset;
        heroBoxCollider.size = targetCollider.size;
    }
    private void Update()
    {
        var target = controllableChildren[targetName];
        if (target == null)
        {
            return;
        }
        transform.position = target.transform.position;
        var heroController = HeroController.instance;
        var heroBoxCollider = heroController.transform.Find("HeroBox").GetComponent<BoxCollider2D>();
        var targetCollider = target.GetComponent<BoxCollider2D>();
        heroBoxCollider.offset = targetCollider.offset;
        heroBoxCollider.size = targetCollider.size;
        if (target.transform.localScale.x < 0)
        {
            heroController.FaceRight();
        }
        else
        {
            heroController.FaceLeft();
        }
    }
    public Dictionary<string, GameObject> controllableChildren;
    private string targetName;
}