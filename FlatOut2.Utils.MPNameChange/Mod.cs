using System.Diagnostics;
using System.Runtime.InteropServices;
using FlatOut2.Utils.MPNameChange.Configuration;
using FlatOut2.Utils.MPNameChange.Template;
using p5rpc.modloader.Utilities;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace FlatOut2.Utils.MPNameChange;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public unsafe class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    private IAsmHook _asmHook;
    private IReloadedHooks _hooks;
    private IntPtr _namePtr;
    private unsafe int* _namePtrPtr;
    
    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _logger = context.Logger;
        _hooks = context.Hooks;
        _modConfig = context.ModConfig;
        
        // For more information about this template, please see
        // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

        // If you want to implement e.g. unload support in your mod,
        // and some other neat features, override the methods in ModBase.
        _modLoader.GetController<IStartupScanner>().TryGetTarget(out var startupScanner);
        var scanHelper = new SigScanHelper(_logger, startupScanner);
        scanHelper.FindPatternOffset("56 E8 ?? ?? ?? ?? 8D 54 00 02 52 56 57 E8 ?? ?? ?? ?? 83 C4 10 5F B8 01 00 00 00 5E C3", ActivateNameChangeHook, "Change Name");

        _namePtrPtr = (int*)Marshal.AllocHGlobal(4);
        ApplyConfig(context.Configuration);
    }

    private void ActivateNameChangeHook(uint offset)
    {
        var baseAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;
        _asmHook = _hooks.CreateAsmHook(new[]
        {
            "use32",
            $"mov esi, dword [{(long)_namePtrPtr}]",
        }, (long)(baseAddress + (int)offset), AsmHookBehaviour.ExecuteFirst).Activate();
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        ApplyConfig(configuration);
    }
    #endregion

    private void ApplyConfig(Config configuration)
    {
        if (_namePtr != IntPtr.Zero)
            Marshal.FreeHGlobal(_namePtr);
        
        // Allocate name ptr.
        _namePtr = Marshal.StringToHGlobalUni(configuration.PlayerName.Substring(0, Math.Min(62, configuration.PlayerName.Length)));
        *_namePtrPtr = (int)_namePtr;
    }
    
    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}