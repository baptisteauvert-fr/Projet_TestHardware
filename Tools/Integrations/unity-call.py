"""Send a JSON {tool, args} request (or list of requests) through Unity MCP."""
import asyncio
import json
import os
from pathlib import Path
import sys
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

async def main():
    root = Path(__file__).resolve().parent
    request = json.loads(Path(sys.argv[1]).read_text(encoding='utf-8-sig'))
    requests = request if isinstance(request, list) else [request]
    params = StdioServerParameters(command=str(root / 'unity-mcp/Server/.venv/Scripts/mcp-for-unity.exe'),
        args=['--transport', 'stdio'], env=dict(os.environ, DISABLE_TELEMETRY='true'))
    async with stdio_client(params) as (read, write):
        async with ClientSession(read, write) as session:
            await session.initialize()
            for req in requests:
                result = await session.call_tool(req['tool'], req.get('args', {}))
                data = result.model_dump(mode='json')
                print(json.dumps({'tool':req['tool'], 'result':data}), flush=True)
            await asyncio.sleep(0.25)

asyncio.run(main())
