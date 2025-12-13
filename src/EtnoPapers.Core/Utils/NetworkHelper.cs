using System.Net.NetworkInformation;

namespace EtnoPapers.Core.Utils;

/// <summary>
/// Provides network connectivity checking utilities.
/// Used before cloud API calls to provide better error messages.
/// </summary>
public static class NetworkHelper
{
    /// <summary>
    /// Checks if internet connection is available by pinging a reliable host.
    /// </summary>
    /// <param name="timeout">Timeout in milliseconds (default: 3000ms)</param>
    /// <returns>True if internet is available, false otherwise</returns>
    public static bool IsInternetAvailable(int timeout = 3000)
    {
        try
        {
            // Try to ping Google's public DNS server (8.8.8.8)
            using var ping = new Ping();
            var reply = ping.Send("8.8.8.8", timeout);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            // If ping fails for any reason (firewall, etc.), assume no internet
            return false;
        }
    }

    /// <summary>
    /// Checks if internet connection is available asynchronously.
    /// </summary>
    /// <param name="timeout">Timeout in milliseconds (default: 3000ms)</param>
    /// <returns>True if internet is available, false otherwise</returns>
    public static async Task<bool> IsInternetAvailableAsync(int timeout = 3000)
    {
        try
        {
            // Try to ping Google's public DNS server (8.8.8.8)
            using var ping = new Ping();
            var reply = await ping.SendPingAsync("8.8.8.8", timeout);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            // If ping fails for any reason (firewall, etc.), assume no internet
            return false;
        }
    }
}
