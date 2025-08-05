# Flashlight Mode Feature

## Overview
This feature adds a tactical flashlight mode to the IndoorRange scene, simulating a realistic low-light shooting environment with a weapon-mounted light similar to a SureFire X300.

## Controls
- **F6**: Toggle Dark Mode (turns off scene lights and activates flashlight)

## Components

### FlashlightController.cs
- Manages the flashlight GameObject and Light component
- Configurable settings for intensity, range, spot angle, and color
- Automatically attaches to the ProjectionPlaneCamera for proper tracking alignment

### LightingModeManager.cs
- Controls scene lighting states (Normal/Dark)
- Manages all existing scene lights
- Handles ambient lighting adjustments
- Automatically activates flashlight in dark mode

### InputHandlers.cs (Modified)
- Added F6 keybinding for dark mode toggle
- Integrated with existing tracking system
- Maintains compatibility with Odyssey Hub tracking

## Setup Instructions

1. **The system automatically initializes when the scene starts**
   - LightingModeManager is created if not present
   - FlashlightController attaches to ProjectionPlaneCamera

2. **Adjusting Flashlight Settings**
   - Find the LightingModeManager GameObject in the scene
   - Adjust FlashlightController settings:
     - Intensity: 1000 (lumens)
     - Range: 50 meters
     - Spot Angle: 15 degrees
     - Inner Spot Angle: 5 degrees
     - Color: Cool white (0.95, 0.95, 1.0)

3. **Customizing Dark Mode**
   - In LightingModeManager:
     - Dark Mode Ambient Intensity: 0.05
     - Dark Mode Ambient Color: Dark blue tint
     - Normal Mode settings preserve original lighting

## Technical Details

- **Lighting System**: Uses Unity's Universal Render Pipeline (URP)
- **Shadow Support**: Soft shadows enabled for realistic light casting
- **Performance**: Optimized for real-time rendering with limited shadow distance
- **Tracking Integration**: Fully compatible with OdysseyHubClient tracking system

## Testing

1. Start the IndoorRange scene
2. Press F6 to toggle dark mode
3. The flashlight will automatically activate when entering dark mode
4. Move around using your tracking system - the flashlight follows your aim
5. Press F6 again to return to normal lighting

## Troubleshooting

- **Flashlight not visible**: Check that URP is properly configured in Project Settings
- **No dark mode toggle**: Ensure InputHandlers component has the ToggleDarkMode action reference set
- **Flashlight not following aim**: Verify ProjectionPlaneCamera is active and tracked

## Future Enhancements

- Add flashlight strobe mode
- Implement battery/power management
- Add volumetric fog for atmospheric effects
- Support for multiple flashlight beam patterns
- Integration with weapon switching system