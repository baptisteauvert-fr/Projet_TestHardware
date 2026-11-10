"""Check both configured MCP servers using the actual MCP protocol."""
import asyncio
import json
import os
from pathlib import Path
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

ROOT = Path(__file__).resolve().parent

async def inspect_server(name, executable, args):
    env = dict(os.environ, DISABLE_TELEMETRY='true', BLENDER_HOST='127.0.0.1', BLENDER_PORT='9876')
    parameters = StdioServerParameters(command=str(ROOT / executable), args=args, env=env)
    async with stdio_client(parameters) as (read, write):
        async with ClientSession(read, write) as session:
            await session.initialize()
            tools = await session.list_tools()
            resources = await session.list_resources()
            summary = {'server': name, 'tools': [t.name for t in tools.tools],
                       'resources': [{'uri': str(r.uri), 'name': r.name} for r in resources.resources]}
            selected = {'get_scene_info', 'execute_blender_code', 'manage_gameobject', 'manage_scene', 'find_gameobjects', 'read_console'}
            summary['schemas'] = {t.name: t.inputSchema for t in tools.tools if t.name in selected}
            if name == 'blender':
                result = await session.call_tool('get_scene_info', {})
                summary['scene'] = result.model_dump(mode='json')
                result = await session.call_tool('execute_blender_code', {'code': '''
import bpy, json, uuid
name = "MCP_Verification_" + uuid.uuid4().hex
obj = bpy.data.objects.new(name, None)
try:
    bpy.context.scene.collection.objects.link(obj)
    obj.location = (1.0, 2.0, 3.0)
    assert bpy.data.objects[name].location[:] == (1.0, 2.0, 3.0)
    print(json.dumps({"created": True, "modified": True, "name": name}))
finally:
    bpy.data.objects.remove(obj, do_unlink=True)
assert name not in bpy.data.objects
print("Temporary object removed successfully")
'''})
                summary['edit_check'] = result.model_dump(mode='json')
            else:
                instances = [r for r in resources.resources if 'instances' in str(r.uri)]
                if instances:
                    summary['instances'] = (await session.read_resource(instances[0].uri)).model_dump(mode='json')
            (ROOT / f'{name}-check.json').write_text(json.dumps(summary, indent=2), encoding='utf-8')
            print(json.dumps(summary), flush=True)

async def main():
    await inspect_server('blender', 'blender-mcp/.venv/Scripts/mcp-for-blender.exe', [])
    await inspect_server('unity', 'unity-mcp/Server/.venv/Scripts/mcp-for-unity.exe', ['--transport', 'stdio'])

if __name__ == '__main__':
    asyncio.run(main())
