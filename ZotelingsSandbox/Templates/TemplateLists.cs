namespace ZotelingsSandbox.Templates;
internal class TemplateLists
{
    static private (string, List<TemplateBase>) GetEnemies()
    {
        return (Interface.MainInterface.enemiesCategoryName, new List<TemplateBase>
        {
            new Standard.PrimalAspid { name = "Primal Aspid" },
            new Standard.ZoteTheMighty { name = "Zote the Mighty" },
            new Standard.MossKnight { name = "Moss Knight" },
            new Standard.HuskGuard { name = "Husk Guard" },
        });
    }
    static private (string, List<TemplateBase>) GetBosses()
    {
        return (Interface.MainInterface.bossesCategoryName, new List<TemplateBase>
        {
            new Standard.GruzMother { name = "Gruz Mother" },
            new TemplateGroup
            {
                name= "Vengefly King",
                templates =
                [
                    (new Standard.VengeflyKing { name = "Vengefly King" }, new Vector2(-2, 0)),
                    (new Standard.VengeflyKing { name = "Vengefly King" }, new Vector2(2, 0))
                ]
            },
            new Standard.BroodingMawlek { name = "Brooding Mawlek" },
            new Standard.FalseKnight { name = "False Knight" },
            new Standard.FailedChampion { name = "Failed Champion" },
            new Standard.HornetProtector { name = "Hornet Protector" },
            new Standard.HornetSentinel { name = "Hornet Sentinel" },
            new TemplateGroup
            {
                name= "Oblobbles",
                templates =
                [
                    (new Standard.Oblobble { name = "Oblobble" }, new Vector2(-3, 0)),
                    (new Standard.Oblobble { name = "Oblobble" }, new Vector2(3, 0))
                ]
            },
            new Standard.HiveKnight { name = "Hive Knight" },
            new Standard.BrokenVessel { name = "Broken Vessel" },
            new Standard.LostKin { name = "Lost Kin" },
            new Standard.Nosk { name = "Nosk" },
            new Standard.WingedNosk { name = "Winged Nosk" },
            new Standard.CrystalGuardian { name = "Crystal Guardian" },
            new Standard.EnragedGuardian { name = "Enraged Guardian" },
            new Standard.TraitorLord { name = "Traitor Lord" },
            new Standard.GreyPrinceZote { name = "Grey Prince Zote" },
            new Standard.SoulWarrior { name = "Soul Warrior" },
            new Standard.Marmu { name = "Marmu" },
            new Standard.Gorb { name = "Gorb" },
            new Standard.PaintmasterSheo { name = "Paintmaster Sheo" },
            new Standard.NailsageSly { name = "Nailsage Sly" },
            new Standard.PureVessel { name = "Pure Vessel" },
        });
    }
    static private (string, List<TemplateBase>) GetBossCombos()
    {
        return (Interface.MainInterface.bossCombosCategoryName, new List<TemplateBase>
        {
            new TemplateGroup
            {
                name= "Double Princes",
                templates =
                [
                    (new Standard.GreyPrinceZote { name = "Grey Prince Zote" }, new Vector2(-2, 0)),
                    (new Standard.PureVessel { name = "Pure Vessel" }, new Vector2(2, 14.1396f - 13.6869f))
                ]
            },
            new TemplateGroup
            {
                name = "Bothers of Betrayal",
                templates =
                [
                    (new Standard.TraitorLord { name = "Traitor Lord" }, new Vector2(-4, 0)),
                    (new Standard.TraitorLord { name = "Traitor Lord" }, new Vector2(0, 0)),
                    (new Standard.TraitorLord { name = "Traitor Lord" }, new Vector2(4, 0)),
                ]
            },
            new TemplateGroup
            {
                name= "Nemesis",
                templates =
                [
                    (new Standard.GreyPrinceZote { name = "Grey Prince Zote" }, new Vector2(-2, 0)),
                    (new Standard.VengeflyKing { name = "Vengefly King" }, new Vector2(2, 10.6556f - 13.6869f))
                ]
            },
        });
    }
    static private (string, List<TemplateBase>) GetPlayableCharacters()
    {
        return (Interface.MainInterface.playableCharactersCategoryName, new List<TemplateBase>
        {
            new Controllable.WingedZote {name = "Winged Zote"},
            new Controllable.ZoteTheMighty { name = "Zote the Mighty" },
        });
    }
    static public List<(string, List<TemplateBase>)> Get()
    {
        if (templateLists == null)
        {
            templateLists = [];
            templateLists.Add(GetEnemies());
            templateLists.Add(GetBosses());
            templateLists.Add(GetBossCombos());
            templateLists.Add(GetPlayableCharacters());
        }
        return templateLists;
    }
    private static List<(string, List<TemplateBase>)> templateLists;
}
