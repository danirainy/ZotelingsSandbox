namespace ZotelingsSandbox.Interface;
public class MainInterface
{
    public static void Initialize()
    {
        On.InputHandler.OnGUI += InputHandlerOnGUI;
        On.HeroController.Update += HeroControllerUpdate;
    }
    public bool Visible()
    {
        return GameManager.instance != null && !GameManager.instance.isPaused && Config.mainInterfaceActive;
    }
    private static void InputHandlerOnGUI(On.InputHandler.orig_OnGUI orig, InputHandler self)
    {
        var mainInterface = ZotelingsSandbox.instance.mainInterface;
        if (mainInterface != null && mainInterface.Visible())
        {
            Cursor.visible = mainInterface.cursorVisible;
            InputHandler.Instance.StartUIInput();
        }
        else
        {
            orig(self);
        }
    }
    private void GenerateDefinitions(int cellSize, int n, NotifyingCollection<GridDimension> definitions)
    {
        for (int i = 0; i < n; ++i)
        {
            definitions.Add(new GridDimension(cellSize, GridUnit.AbsoluteMin));
        }
    }
    private Button GenerateButton(int cellSize, int x, int y, int w, int h, string content)
    {
        return new Button(layoutRoot, buttonName)
        {
            MinWidth = w * cellSize,
            MinHeight = h * cellSize,
            Content = Lang.Translate(content),
            BorderColor = buttonOrignalColor
        }.WithProp(MagicUI.Elements.GridLayout.Column, x).WithProp(MagicUI.Elements.GridLayout.ColumnSpan, w)
         .WithProp(MagicUI.Elements.GridLayout.Row, y).WithProp(MagicUI.Elements.GridLayout.RowSpan, h);
    }
    private MagicUI.Elements.GridLayout GenerateLayout(int cellSize, int x, int y, int w, int h)
    {
        return new MagicUI.Elements.GridLayout(layoutRoot)
        {
            MinWidth = w * cellSize,
            MinHeight = h * cellSize,
        }.WithProp(MagicUI.Elements.GridLayout.Column, x).WithProp(MagicUI.Elements.GridLayout.ColumnSpan, w)
         .WithProp(MagicUI.Elements.GridLayout.Row, y).WithProp(MagicUI.Elements.GridLayout.RowSpan, h);
    }
    private void CollapseAllCategories()
    {
        foreach (var categoryButton in categoryButtons)
        {
            categoryButton.BorderColor = buttonOrignalColor;
        }
        foreach (var categoryLayout in categoryLayouts)
        {
            categoryLayout.Visibility = Visibility.Hidden;
        }
    }
    private void GenerateToolBar()
    {
        var cellSize = 30;
        var buttonWidth = 6;
        var buttonHeight = 1;
        var mainLayout = new MagicUI.Elements.GridLayout(layoutRoot)
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Padding = new(cellSize)
        };
        var templateLists = Templates.TemplateLists.Get();
        var numColumns = 1 + templateLists.Count + 1 + 1;
        var numRows = 1 + templateLists.Max(templateList => templateList.Item2.Count);
        GenerateDefinitions(cellSize, numColumns * buttonWidth, mainLayout.ColumnDefinitions);
        GenerateDefinitions(cellSize, numRows * buttonHeight, mainLayout.RowDefinitions);
        var titleButton = GenerateButton(cellSize, 0, 0, buttonWidth, buttonHeight, "Zoteling's Sandbox");
        titleButton.Click += (Button button) => { button.Content = Lang.Translate("Zote is adorably cute!"); };
        mainLayout.Children.Add(titleButton);
        var columnIndex = 1;
        foreach (var (category, templateList) in templateLists)
        {
            var categoryButton = GenerateButton(cellSize, columnIndex * buttonWidth, 0, buttonWidth, buttonHeight, category);
            mainLayout.Children.Add(categoryButton);
            categoryButtons.Add(categoryButton);
            var numSubRows = 11;
            var numSubCols = (templateList.Count + numSubRows - 1) / numSubRows;
            var categoryLayout = GenerateLayout(cellSize, columnIndex * buttonWidth, buttonHeight, numSubCols * buttonWidth, numSubRows * buttonHeight);
            GenerateDefinitions(cellSize, numSubCols * buttonWidth, categoryLayout.ColumnDefinitions);
            GenerateDefinitions(cellSize, numSubRows * buttonHeight, categoryLayout.RowDefinitions);
            var currentRow = 0;
            var currentCol = 0;
            foreach (var template in templateList)
            {
                var templateButton = GenerateButton(cellSize, currentCol * buttonWidth, currentRow * buttonHeight, buttonWidth, buttonHeight, template.name);
                var currentTemplate = template;
                templateButton.Click += (Button button) =>
                {
                    ButtonClicked();
                    CollapseAllCategories();
                    if (category != playableCharactersCategoryName)
                    {
                        int hp;
                        if (category == enemiesCategoryName || category == enemyCombosCategoryName)
                        {
                            if (template is Templates.TemplateGroup templateGroup)
                            {
                                hp = Deploy.Common.DefaultEnemyComboHealth;
                            }
                            else
                            {
                                hp = Deploy.Common.DefaultEnemyHealth;
                            }
                        }
                        else if (category == bossesCategoryName || category == bossCombosCategoryName)
                        {
                            if (template is Templates.TemplateGroup templateGroup)
                            {
                                hp = Deploy.Common.DefaultBossComboHealth;
                            }
                            else
                            {
                                hp = Deploy.Common.DefaultBossHealth;
                            }
                        }
                        else
                        {
                            hp = -1;
                            Log.LogError($"Unknown category {category}");
                        }
                        currentTemplate.Place(new Templates.PlaceConfig
                        {
                            groupID = UnityEngine.Random.Range(0, 65536),
                            hp = hp,
                            damage = Deploy.Common.DefaultDamage,
                            hpBar = true
                        });
                    }
                    else
                    {
                        var heroHandler = HeroController.instance.GetComponent<Deploy.Behaviors.HeroHandler>();
                        if (heroHandler.HasControllable)
                        {
                            heroHandler.UninstallControllable();
                        }
                        else
                        {
                            currentTemplate.Control();
                        }
                    }
                };
                categoryLayout.Children.Add(templateButton);
                if (category == playableCharactersCategoryName)
                {
                    controllableButtons.Add(templateButton);
                }
                currentRow += 1;
                if (currentRow >= numSubRows)
                {
                    currentRow = 0;
                    currentCol += 1;
                }
            }
            mainLayout.Children.Add(categoryLayout);
            categoryLayouts.Add(categoryLayout);
            categoryLayout.Visibility = Visibility.Hidden;
            categoryButton.Click += (Button button) =>
            {
                ButtonClicked();
                if (categoryLayout.Visibility != Visibility.Visible)
                {
                    CollapseAllCategories();
                    categoryButton.BorderColor = buttonClickedColor;
                    categoryLayout.Visibility = Visibility.Visible;
                }
                else
                {
                    CollapseAllCategories();
                }
            };
            columnIndex += 1;
        }
    }
    private void AddButtonBackground(GameObject canvasGameObject, GameObject buttonGameObject)
    {
        var background = new GameObject("ZotelingsSandbox.Background");
        background.transform.SetParent(canvasGameObject.transform, false);
        var rectTransform = background.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(100, 100);
        var image = background.AddComponent<UnityEngine.UI.Image>();
        image.color = backgroundColor;
        var buttonSyncer = new GameObject("ZotelingsSandbox.ButtonSyncer");
        buttonSyncer.transform.SetParent(canvasGameObject.transform, false);
        var syncButton = buttonSyncer.AddComponent<SyncButton>();
        syncButton.self = background;
        syncButton.target = buttonGameObject;
    }
    private void AddBackgrounds()
    {
        var canvasGameObject = new GameObject("ZotelingsSandbox.Canvas");
        var canvas = canvasGameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        canvasGameObject.transform.SetAsFirstSibling();
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var gameObject in scene.GetRootGameObjects())
        {
            if (gameObject.name == layoutRootName)
            {
                for (int i = 0; i < gameObject.transform.childCount; ++i)
                {
                    var child = gameObject.transform.GetChild(i).gameObject;
                    if (child.name.StartsWith(buttonName))
                    {
                        AddButtonBackground(canvasGameObject, child);
                    }
                }
            }
        }
    }
    public MainInterface()
    {
        layoutRoot = new LayoutRoot(false, layoutRootName)
        {
            VisibilityCondition = () => Visible(),
        };
        cursorPosition = Input.mousePosition;
        GenerateToolBar();
        AddBackgrounds();
    }
    private void ButtonClicked()
    {
        notEmptyClick = true;
        emptyClicked = false;
    }
    private static void CanvasClicked()
    {
        var mainInterface = ZotelingsSandbox.instance.mainInterface;
        if (mainInterface == null)
        {
            return;
        }
        if (mainInterface.notEmptyClick)
        {
            mainInterface.notEmptyClick = false;
            mainInterface.emptyClicked = false;
            return;
        }
        if (Input.GetMouseButtonUp(0))
        {
            mainInterface.notEmptyClick = false;
            mainInterface.emptyClicked = true;
            return;
        }
        if (mainInterface.emptyClicked)
        {
            mainInterface.notEmptyClick = false;
            mainInterface.emptyClicked = false;
            ZotelingsSandbox.instance.mainInterface.CollapseAllCategories();
        }
    }
    private static void RefreshCursor()
    {
        var mainInterface = ZotelingsSandbox.instance.mainInterface;
        if (mainInterface == null)
        {
            return;
        }
        if (mainInterface.cursorTime > 0)
        {
            mainInterface.cursorTime -= Time.deltaTime;
        }
        if (Input.GetMouseButtonDown(0))
        {
            mainInterface.cursorTime = cursorTimeMax;
        }
        if (Input.GetMouseButtonDown(1))
        {
            mainInterface.cursorTime = cursorTimeMax;
        }
        if (mainInterface.cursorPosition != Input.mousePosition)
        {
            mainInterface.cursorPosition = Input.mousePosition;
            mainInterface.cursorTime = cursorTimeMax;
        }
        mainInterface.cursorVisible = mainInterface.cursorTime > 0;
    }
    private static void ActivateInstances()
    {
        var mainInterface = ZotelingsSandbox.instance.mainInterface;
        if (mainInterface == null)
        {
            return;
        }
        if (Input.GetMouseButtonDown(1))
        {
            foreach (var instance in ZotelingsSandbox.instance.RefreshInstances())
            {
                var instanceInfo = instance.GetComponent<Deploy.Behaviors.InstanceInfo>();
                if (instanceInfo != null && instanceInfo.status == Deploy.Behaviors.InstanceInfo.Status.Sleeping)
                {
                    instanceInfo.status = Deploy.Behaviors.InstanceInfo.Status.Awaking;
                }
            }
        }
    }
    private static void ClearAll()
    {
        var mainInterface = ZotelingsSandbox.instance.mainInterface;
        if (mainInterface == null)
        {
            return;
        }
        if (Input.GetMouseButtonDown(2))
        {
            foreach (var instance in ZotelingsSandbox.instance.RefreshInstances())
            {
                GameObject.Destroy(instance);
            }
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Log.LogError($"Destroying root {root.name}");
                foreach (var healthManger in root.GetComponentsInChildren<HealthManager>())
                {
                    if (healthManger != null)
                    {
                        Log.LogError($"Destroying HealthManager {healthManger.gameObject.name}");
                        GameObject.Destroy(healthManger.gameObject);
                    }
                }
            }
        }
    }
    private static void RefreshControllableButtons()
    {
        var mainInterface = ZotelingsSandbox.instance.mainInterface;
        if (mainInterface == null)
        {
            return;
        }
        var heroController = HeroController.instance;
        var heroInfo = heroController.GetComponent<Deploy.Behaviors.HeroHandler>();
        foreach (var button in mainInterface.controllableButtons)
        {
            if (!heroController.controlReqlinquished || (heroInfo != null && heroInfo.Controllable))
            {
                button.Enabled = true;
            }
            else
            {
                button.Enabled = false;
            }
        }
    }
    private static void HeroControllerUpdate(On.HeroController.orig_Update orig, HeroController self)
    {
        CanvasClicked();
        RefreshCursor();
        ActivateInstances();
        ClearAll();
        RefreshControllableButtons();
        orig(self);
    }
    private static string layoutRootName = "ZotelingsSandbox.LayoutRoot";
    private static string buttonName = "ZotelingsSandbox.Button";
    private static Color buttonOrignalColor = new Color(0.75f, 0.75f, 0.75f);
    private static Color buttonClickedColor = new Color(0.5f, 0.5f, 0.5f);
    private static Color backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.85f);
    public static string enemiesCategoryName = "Enemies";
    public static string enemyCombosCategoryName = "Enemy Combos";
    public static string bossesCategoryName = "Bosses";
    public static string bossCombosCategoryName = "Boss Combos";
    public static string playableCharactersCategoryName = "Playable Characters";
    private LayoutRoot layoutRoot;
    private List<Button> categoryButtons = [];
    private List<MagicUI.Elements.GridLayout> categoryLayouts = [];
    private List<Button> controllableButtons = [];
    private bool notEmptyClick;
    private bool emptyClicked;
    private bool cursorVisible;
    private Vector3 cursorPosition;
    private float cursorTime;
    private static float cursorTimeMax = 10;
}
