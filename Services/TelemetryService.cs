using LaunchControl.Models;

namespace LaunchControl.Services;

/// <summary>
/// Stand-in for the real ground data acquisition layer. Produces plausible,
/// slowly-varying values so the console behaves like a live system.
/// Replace with a real OPC-UA / EGSE / field-bus client later.
/// </summary>
public sealed class TelemetryService
{
    private readonly Random _rng = new();

    public double Jitter(double baseValue, double amplitude)
        => baseValue + (_rng.NextDouble() - 0.5) * 2 * amplitude;

    public double Drift(double value, double target, double rate)
        => value + Math.Clamp(target - value, -rate, rate);

    public bool Chance(double p) => _rng.NextDouble() < p;
}
