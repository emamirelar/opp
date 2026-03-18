# MCP Server — PrimeNG Component Library

## Overview

[Model Context Protocol (MCP)](https://modelcontextprotocol.io/) is an open standard that enables AI models to connect with external tools and data sources. This project ships a PrimeNG MCP server that gives AI assistants comprehensive access to:

- **Component documentation** — props, events, templates, and methods
- **Theming and styling** — Pass Through API and design tokens
- **Code examples** — usage patterns and best practices
- **Migration guides** — version upgrade paths
- **Installation and configuration** — setup instructions

## Configuration

The MCP server is configured in `.cursor/mcp.json` at the project root. Cursor loads this file automatically.

```json
{
    "mcpServers": {
        "primeng": {
            "command": "npx",
            "args": ["-y", "@primeng/mcp"]
        }
    }
}
```

### Alternative: Global Configuration

To make the PrimeNG MCP server available across all Cursor projects, place the same JSON in your global config:

```
~/.cursor/mcp.json
```

## How It Works

1. Cursor detects the `mcp.json` configuration on startup.
2. When an AI assistant needs PrimeNG information, it calls the MCP server via `npx @primeng/mcp`.
3. The server returns up-to-date documentation, examples, and theming guidance directly in the assistant's context.

## Prerequisites

- **Node.js 18+** and **npm** (already required by the Angular client app)
- **Cursor IDE** with MCP support enabled

No additional installation is needed — `npx -y @primeng/mcp` downloads and runs the package on demand.

## Troubleshooting

| Problem | Solution |
|---------|----------|
| MCP server not appearing in Cursor | Restart Cursor after adding/editing `.cursor/mcp.json` |
| `npx` command fails | Verify Node.js and npm are on your `PATH` |
| Stale documentation | Delete the npx cache (`npx clear-npx-cache`) and restart Cursor |

## Related

- [PrimeNG Documentation](https://primeng.org/)
- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [Project README](../../README.md)
