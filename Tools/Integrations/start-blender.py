"""Enable the local Blender MCP add-on without changing existing preferences."""
import os
import addon_utils

os.environ['DISABLE_TELEMETRY'] = 'true'
if not addon_utils.check('blender_mcp')[1]:
    addon_utils.enable('blender_mcp', default_set=False)
