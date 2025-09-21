using HarmonyLib;
using SimpleEntityHealthbar.patches;
using Simpleentityhealthbarnameplate;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace SimpleEntityHealthbar;

public class SimpleEntityHealthbarModSystem : ModSystem
{
    public static ICoreClientAPI _capi;
    public static ModInfo ModInfo;

    private static string _configFile;
    private BlockInfoHudPatch  _blockInfoHudPatch = new ();
    
    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;
    public static string ModId = "SimpleEntityHealthbar";
    
    
    private Entity _mobhealthBarEntity;
    private static EntityAgentHealthBar _mobhealthBar;
    private static EntityAgentNameplateHealthBar _mobnameplatehealthBar;

    public override void Start(ICoreAPI api)
    {
        ModInfo = Mod.Info;
        _configFile = $"{Mod.Info.Name}.json".Replace(" ", "");
        Logger.Init(api.Logger);
        Logger.Event($"started '{ModInfo.Name}' (Version: {ModInfo.Version})"); 
    }
    

    public override void StartClientSide(ICoreClientAPI api)
    {
        _capi = api;
        
        ModConfig.Instance = api.LoadModConfig<ModConfig>(_configFile);
        if (ModConfig.Instance == null)
        {
            ModConfig.Instance = new ModConfig();
        }
        
        api.StoreModConfig(ModConfig.Instance, _configFile);

        if (ModConfig.Instance.ShowHudHealthBar)
            _mobhealthBar = new EntityAgentHealthBar(_capi);
        
        if (ModConfig.Instance.ShowNameplates)
            _mobnameplatehealthBar = new EntityAgentNameplateHealthBar(_capi);
        _blockInfoHudPatch.Patch();
        
        base.StartClientSide(_capi);
    }
    
    public static bool IsHealthBarActive()
    {
        return _mobhealthBar != null && (_mobhealthBar?.IsHealthBarActive() == true);
    }
    
    public static Harmony NewPatch(string description, string category)
    {
        Harmony patcher = null;
        if (!Harmony.HasAnyPatches(category))
        {
            patcher = new Harmony(category);
            patcher.PatchCategory(category);
            Logger.Log($"Patched {description}");
        }
        else Logger.Error($"Patch '{category}' ('{description}') failed. Check if other patches with same id have been loaded");

        return patcher;
    }
    
    public override void Dispose()
    {
        base.Dispose();
        _blockInfoHudPatch.Unpatch();
    }
}


public class Logger
{
    private static ILogger _logger;
    private string _logBaseFormat = string.Format("[{0}] {1}", SimpleEntityHealthbarModSystem.ModInfo.Name);

    public static void Init(ILogger logger) => _logger = logger;
    public static void Event(string message) => _logger.Log(EnumLogType.Event, message);
    public static void Log(string message) => _logger.Log(EnumLogType.Build, message);
    public static void Error(string message) => _logger.Log(EnumLogType.Error, message);
    public static void Debug(string message) => _logger.Log(EnumLogType.Debug, message);
}
