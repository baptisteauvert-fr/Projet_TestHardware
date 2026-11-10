"""Wait for Unity import, then verify scene reading and a temporary edit."""
import asyncio
import json
import os
import uuid
from pathlib import Path
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

ROOT = Path(__file__).resolve().parent

def unpack(result):
    return result.model_dump(mode='json')

async def main():
    parameters = StdioServerParameters(
        command=str(ROOT / 'unity-mcp/Server/.venv/Scripts/mcp-for-unity.exe'),
        args=['--transport', 'stdio'], env=dict(os.environ, DISABLE_TELEMETRY='true'))
    async with stdio_client(parameters) as (read, write):
        async with ClientSession(read, write) as session:
            await session.initialize()
            for attempt in range(90):
                result = await session.read_resource('mcpforunity://instances')
                state = json.loads(result.contents[0].text)
                if state.get('instance_count', 0):
                    break
                if attempt % 6 == 0:
                    print('Waiting for Unity to finish importing...', flush=True)
                await asyncio.sleep(10)
            else:
                raise RuntimeError('Unity is still unavailable after 15 minutes')
            report = {'instances': state}
            report['scene'] = unpack(await session.call_tool('manage_scene', {'action': 'get_active'}))
            report['project'] = unpack(await session.read_resource('mcpforunity://project/info'))
            name = 'MCP_Verification_' + uuid.uuid4().hex
            created = await session.call_tool('manage_gameobject', {'action': 'create', 'name': name})
            report['create'] = unpack(created)
            try:
                report['modify'] = unpack(await session.call_tool('manage_gameobject', {
                    'action': 'modify', 'target': name, 'search_method': 'by_name', 'position': [1, 2, 3]}))
                report['find'] = unpack(await session.call_tool('find_gameobjects', {'search_term': name}))
            finally:
                report['cleanup'] = unpack(await session.call_tool('manage_gameobject', {
                    'action': 'delete', 'target': name, 'search_method': 'by_name'}))
            report['after_cleanup'] = unpack(await session.call_tool('find_gameobjects', {'search_term': name}))
            report['errors'] = unpack(await session.call_tool('read_console', {
                'action': 'get', 'types': ['error'], 'count': 10, 'include_stacktrace': False}))
            (ROOT / 'unity-verification.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
            print(json.dumps(report), flush=True)

asyncio.run(main())
