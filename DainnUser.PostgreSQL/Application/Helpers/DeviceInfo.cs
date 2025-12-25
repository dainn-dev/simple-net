namespace DainnUser.PostgreSQL.Application.Helpers;

/// <summary>
/// Represents device information extracted from HTTP headers.
/// </summary>
public record DeviceInfo
{
    /// <summary>
    /// Gets or sets the user agent string.
    /// </summary>
    public string? UserAgent { get; init; }

    /// <summary>
    /// Gets or sets the device type (e.g., "Desktop", "Mobile", "Tablet", "Unknown").
    /// </summary>
    public string? DeviceType { get; init; }

    /// <summary>
    /// Gets or sets the browser name (e.g., "Chrome", "Firefox", "Safari").
    /// </summary>
    public string? Browser { get; init; }

    /// <summary>
    /// Gets or sets the operating system (e.g., "Windows", "macOS", "iOS", "Android").
    /// </summary>
    public string? OperatingSystem { get; init; }

    /// <summary>
    /// Gets or sets the device name/model (e.g., "iPhone 14 Pro", "Macbook Pro", "Samsung Galaxy S21", "Unknown").
    /// </summary>
    public string? DeviceName { get; init; }
}

/// <summary>
/// Helper class for extracting device information from HTTP headers.
/// </summary>
public static class DeviceInfoHelper
{
    /// <summary>
    /// Extracts device information from the User-Agent header.
    /// </summary>
    /// <param name="userAgent">The User-Agent string from the HTTP request.</param>
    /// <returns>A DeviceInfo object containing parsed device information.</returns>
    public static DeviceInfo ParseUserAgent(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return new DeviceInfo
            {
                UserAgent = null,
                DeviceType = "Unknown",
                Browser = "Unknown",
                OperatingSystem = "Unknown",
                DeviceName = "Unknown"
            };
        }

        var deviceType = DetectDeviceType(userAgent);
        var browser = DetectBrowser(userAgent);
        var operatingSystem = DetectOperatingSystem(userAgent);
        var deviceName = DetectDeviceName(userAgent, operatingSystem, deviceType);

        return new DeviceInfo
        {
            UserAgent = userAgent,
            DeviceType = deviceType,
            Browser = browser,
            OperatingSystem = operatingSystem,
            DeviceName = deviceName
        };
    }

    private static string DetectDeviceType(string userAgent)
    {
        var ua = userAgent.ToLowerInvariant();

        if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone") || ua.Contains("ipod"))
        {
            if (ua.Contains("tablet") || ua.Contains("ipad") || (ua.Contains("android") && !ua.Contains("mobile")))
            {
                return "Tablet";
            }
            return "Mobile";
        }

        return "Desktop";
    }

    private static string DetectBrowser(string userAgent)
    {
        var ua = userAgent.ToLowerInvariant();

        if (ua.Contains("edg/") || ua.Contains("edgios/") || ua.Contains("edga/"))
            return "Edge";
        if (ua.Contains("chrome/") && !ua.Contains("edg"))
            return "Chrome";
        if (ua.Contains("firefox/"))
            return "Firefox";
        if (ua.Contains("safari/") && !ua.Contains("chrome"))
            return "Safari";
        if (ua.Contains("opera/") || ua.Contains("opr/"))
            return "Opera";
        if (ua.Contains("msie") || ua.Contains("trident/"))
            return "Internet Explorer";
        if (ua.Contains("brave/"))
            return "Brave";

        return "Unknown";
    }

    private static string DetectOperatingSystem(string userAgent)
    {
        var ua = userAgent.ToLowerInvariant();

        if (ua.Contains("windows nt 10.0") || ua.Contains("windows 10"))
            return "Windows 10";
        if (ua.Contains("windows nt 11.0") || ua.Contains("windows 11"))
            return "Windows 11";
        if (ua.Contains("windows nt 6.3"))
            return "Windows 8.1";
        if (ua.Contains("windows nt 6.2"))
            return "Windows 8";
        if (ua.Contains("windows nt 6.1"))
            return "Windows 7";
        if (ua.Contains("windows nt 6.0"))
            return "Windows Vista";
        if (ua.Contains("windows"))
            return "Windows";
        if (ua.Contains("mac os x") || ua.Contains("macintosh"))
            return "macOS";
        if (ua.Contains("iphone") || ua.Contains("ipad") || ua.Contains("ipod"))
            return "iOS";
        if (ua.Contains("android"))
            return "Android";
        if (ua.Contains("linux"))
            return "Linux";
        if (ua.Contains("ubuntu"))
            return "Ubuntu";

        return "Unknown";
    }

    private static string DetectDeviceName(string userAgent, string operatingSystem, string deviceType)
    {
        var ua = userAgent;

        // iOS Device Detection
        if (operatingSystem == "iOS")
        {
            // iPhone model detection
            if (ua.Contains("iPhone"))
            {
                // Extract iPhone model identifier (e.g., iPhone14,2 = iPhone 13 Pro)
                var modelMatch = System.Text.RegularExpressions.Regex.Match(ua, @"iPhone(\d+),(\d+)");
                if (modelMatch.Success)
                {
                    var modelNumber = modelMatch.Groups[1].Value;
                    var subModelNumber = modelMatch.Groups[2].Value;
                    return GetIPhoneModelName(modelNumber, subModelNumber);
                }

                // Check for newer iPhone models by CPU identifier
                if (ua.Contains("iPhone15,"))
                {
                    if (ua.Contains("iPhone15,2")) return "iPhone 14 Pro";
                    if (ua.Contains("iPhone15,3")) return "iPhone 14 Pro Max";
                    if (ua.Contains("iPhone15,4")) return "iPhone 14";
                    if (ua.Contains("iPhone15,5")) return "iPhone 14 Plus";
                }
                if (ua.Contains("iPhone16,"))
                {
                    if (ua.Contains("iPhone16,1")) return "iPhone 15 Pro";
                    if (ua.Contains("iPhone16,2")) return "iPhone 15 Pro Max";
                    if (ua.Contains("iPhone16,3")) return "iPhone 15";
                    if (ua.Contains("iPhone16,4")) return "iPhone 15 Plus";
                }

                // Generic iPhone if no specific model found
                return "iPhone";
            }

            // iPad model detection
            if (ua.Contains("iPad"))
            {
                var modelMatch = System.Text.RegularExpressions.Regex.Match(ua, @"iPad(\d+),(\d+)");
                if (modelMatch.Success)
                {
                    var modelNumber = modelMatch.Groups[1].Value;
                    var subModelNumber = modelMatch.Groups[2].Value;
                    return GetIPadModelName(modelNumber, subModelNumber);
                }
                return "iPad";
            }

            // iPod
            if (ua.Contains("iPod"))
            {
                return "iPod Touch";
            }
        }

        // Android Device Detection
        if (operatingSystem == "Android")
        {
            // Try to extract device model from Android User-Agent
            // Format: "Build/[model]" or "SM-[model]" for Samsung
            var buildMatch = System.Text.RegularExpressions.Regex.Match(ua, @"Build/([A-Z0-9\-]+)");
            if (buildMatch.Success)
            {
                var buildId = buildMatch.Groups[1].Value;
                
                // Samsung devices (SM-xxxxx)
                var samsungMatch = System.Text.RegularExpressions.Regex.Match(ua, @"SM-([A-Z0-9]+)");
                if (samsungMatch.Success)
                {
                    var model = samsungMatch.Groups[1].Value;
                    return $"Samsung Galaxy {GetSamsungModelName(model)}";
                }

                // Try to extract device name from wv (WebView) or other patterns
                var deviceMatch = System.Text.RegularExpressions.Regex.Match(ua, @"\(Linux; Android[^;]+; ([^)]+)\)");
                if (deviceMatch.Success)
                {
                    var deviceInfo = deviceMatch.Groups[1].Value;
                    // Clean up the device name
                    deviceInfo = deviceInfo.Replace("Build/", "").Trim();
                    if (!string.IsNullOrEmpty(deviceInfo) && deviceInfo.Length < 100)
                    {
                        return deviceInfo;
                    }
                }
            }

            // Generic Android device
            return "Android Device";
        }

        // macOS - Try to detect Mac model (limited info in User-Agent)
        if (operatingSystem == "macOS")
        {
            // macOS User-Agent doesn't typically include Mac model
            // But we can infer from CPU architecture in some cases
            if (ua.Contains("Macintosh"))
            {
                // For Apple Silicon Macs
                if (ua.Contains("ARM64") || ua.Contains("Apple"))
                {
                    return "Mac (Apple Silicon)";
                }
                return "Mac";
            }
        }

        // Windows - Generic PC
        if (operatingSystem.StartsWith("Windows"))
        {
            return "Windows PC";
        }

        // Desktop default
        if (deviceType == "Desktop")
        {
            return operatingSystem != "Unknown" ? $"{operatingSystem} Device" : "Desktop";
        }

        return "Unknown";
    }

    private static string GetIPhoneModelName(string modelNumber, string subModelNumber)
    {
        // iPhone model identifier mapping
        // This is a simplified mapping - real-world User-Agent strings may vary
        var model = $"{modelNumber},{subModelNumber}";
        
        // iPhone 15 series
        if (model == "16,1") return "iPhone 15 Pro";
        if (model == "16,2") return "iPhone 15 Pro Max";
        if (model == "16,3") return "iPhone 15";
        if (model == "16,4") return "iPhone 15 Plus";
        
        // iPhone 14 series
        if (model == "15,2") return "iPhone 14 Pro";
        if (model == "15,3") return "iPhone 14 Pro Max";
        if (model == "15,4") return "iPhone 14";
        if (model == "15,5") return "iPhone 14 Plus";
        
        // iPhone 13 series
        if (model == "14,2") return "iPhone 13 Pro";
        if (model == "14,3") return "iPhone 13 Pro Max";
        if (model == "14,4") return "iPhone 13 mini";
        if (model == "14,5") return "iPhone 13";
        
        // iPhone 12 series
        if (model == "13,1") return "iPhone 12 mini";
        if (model == "13,2") return "iPhone 12";
        if (model == "13,3") return "iPhone 12 Pro";
        if (model == "13,4") return "iPhone 12 Pro Max";
        
        // iPhone 11 series
        if (model == "12,1") return "iPhone 11";
        if (model == "12,3") return "iPhone 11 Pro";
        if (model == "12,5") return "iPhone 11 Pro Max";
        
        // iPhone XS/XS Max/XR
        if (model == "11,8") return "iPhone XR";
        if (model == "11,2") return "iPhone XS";
        if (model == "11,4" || model == "11,6") return "iPhone XS Max";
        
        // iPhone X
        if (model == "10,3" || model == "10,6") return "iPhone X";
        
        // Older models (iPhone 8 and earlier)
        if (model.StartsWith("10,")) return "iPhone 8";
        if (model.StartsWith("9,")) return "iPhone 7";
        if (model.StartsWith("8,")) return "iPhone 6s";
        
        return "iPhone";
    }

    private static string GetIPadModelName(string modelNumber, string subModelNumber)
    {
        var model = $"{modelNumber},{subModelNumber}";
        
        // iPad Pro models
        if (model.StartsWith("13,")) return "iPad Pro";
        if (model.StartsWith("8,")) return "iPad Pro";
        if (model.StartsWith("7,")) return "iPad Pro";
        
        // iPad Air
        if (model.StartsWith("11,")) return "iPad Air";
        if (model.StartsWith("4,")) return "iPad Air";
        
        // iPad mini
        if (model.StartsWith("14,")) return "iPad mini";
        if (model.StartsWith("5,")) return "iPad mini";
        
        // Standard iPad
        if (model.StartsWith("12,")) return "iPad";
        if (model.StartsWith("6,")) return "iPad";
        
        return "iPad";
    }

    private static string GetSamsungModelName(string modelCode)
    {
        // Samsung Galaxy model code mapping (simplified)
        // Full mapping would be extensive - this covers common models
        if (modelCode.StartsWith("S9")) return "S9";
        if (modelCode.StartsWith("S10")) return "S10";
        if (modelCode.StartsWith("S20")) return "S20";
        if (modelCode.StartsWith("S21")) return "S21";
        if (modelCode.StartsWith("S22")) return "S22";
        if (modelCode.StartsWith("S23")) return "S23";
        if (modelCode.StartsWith("S24")) return "S24";
        if (modelCode.StartsWith("N")) return "Note";
        if (modelCode.StartsWith("A")) return $"A{modelCode.Substring(1)}";
        
        return modelCode;
    }
}

