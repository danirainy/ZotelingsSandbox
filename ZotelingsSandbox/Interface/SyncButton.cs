namespace ZotelingsSandbox.Interface;
internal class SyncButton : MonoBehaviour
{
    private void Start()
    {
        myRectTransform = self.GetComponent<RectTransform>();
        targetRectTransform = target.GetComponent<RectTransform>();
        if (targetRectTransform == null)
        {
            Log.LogError("Target does not have a RectTransform");
        }
    }
    private void Update()
    {
        if (targetRectTransform != null)
        {
            var mainInterface = ZotelingsSandbox.instance.mainInterface;
            if (mainInterface != null)
            {
                self.SetActive(target.activeInHierarchy && mainInterface.Visible());
                self.transform.position = target.transform.position;
                var w = targetRectTransform.sizeDelta.x * (Screen.width * 1.0f / 1920);
                var h = targetRectTransform.sizeDelta.y * (Screen.height * 1.0f / 1080);
                myRectTransform.sizeDelta = new Vector2(w, h);
            }
        }
    }
    public GameObject self;
    public GameObject target;
    private RectTransform myRectTransform;
    private RectTransform targetRectTransform;
}
