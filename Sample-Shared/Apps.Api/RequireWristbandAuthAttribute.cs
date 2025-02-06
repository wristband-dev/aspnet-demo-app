using System;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RequireWristbandAuthAttribute : Attribute
{
    // Marker attribute to indicate Wristband authentication is required
}
