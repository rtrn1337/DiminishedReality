using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
 
public static class LinearLightsToggle
{
    [MenuItem("Tools/Graphics/Toggle lightsUseLinearIntensity")]
    public static void UseLinearLightIntensity()
    {
        GraphicsSettings.lightsUseLinearIntensity = !GraphicsSettings.lightsUseLinearIntensity;
        GraphicsSettings.lightsUseColorTemperature = GraphicsSettings.lightsUseLinearIntensity;
        Debug.Log("lightsUseLinearIntensity & lightsUseColorTemperature is " + GraphicsSettings.lightsUseLinearIntensity);
    }
}