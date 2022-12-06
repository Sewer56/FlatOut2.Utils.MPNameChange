using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace p5rpc.modloader.Utilities;

/// <summary>
/// Utility class for querying sigscans to be done in parallel.
/// </summary>
public class SigScanHelper
{
    private readonly IStartupScanner? _startupScanner;
    private readonly ILogger? _logger;

    public SigScanHelper(ILogger? logger, IStartupScanner? startupScanner)
    {
        _logger = logger;
        _startupScanner = startupScanner;
    }

    public void FindPatternOffset(string? pattern, Action<uint> action, string? name = null)
    {
        _startupScanner?.AddMainModuleScan(pattern, (res) =>
        {
            if (res.Found)
            {
                if (!string.IsNullOrEmpty(name))
                    _logger?.WriteLineAsync($"{name} found");

                action((uint)res.Offset);
            }
            else if (!string.IsNullOrEmpty(name))
            {
                _logger?.WriteLineAsync($"{name} not found. You need a DRM-free copy of FlatOut 2. Such as the one from GOG.");
            }
        });
    }
}