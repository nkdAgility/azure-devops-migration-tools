---
name: speckit-engineering-checks-mcp-story
description: 'Spec bar for a Product Backlog Item: MCP story'
compatibility: Requires spec-kit project structure with .specify/ directory
metadata:
  author: nkdAgility
  source: engineering-checks:commands/mcp-story.md
---

# MCP story

For the item in `.specify/items/current.json` and its current spec and plan:

How an agent will use this capability through the MCP tools: which tools change or are added, what each takes and returns, and one worked example. If the change has no MCP surface, say why.

$ARGUMENTS

Write it to the item's WKR file (`.specify/wkr/<number>.md`) as a section headed `## MCP story`. If something cannot be settled, write it under `## Questions` as `- Q-n <question> (for <who>) open`, and leave the section saying `open`.