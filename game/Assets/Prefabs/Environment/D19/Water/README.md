# D19-A04 WaterRoot

This is the D19 water work package only. `WaterRoot.prefab` is not connected to `InitialHarbor`, ShipSystem, WeatherSystem, Registry, save data, or the formal camera.

- Sea level: `Y=0`.
- Visual-only interface: `D19WaterFrameParameters` (wind direction/speed, wave amplitude/length/speed, transparency, turbidity, quality tier).
- Water has no collider and no gameplay authority.
- External texture inputs are only the D19-S01 intake-eligible Sand 01 and Coral Mud 01 entries.
- `D19WaterPresentation` is the sole presentation owner for water material property blocks and quality keywords.
