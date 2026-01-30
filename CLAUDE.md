# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Slap-N-Dio is a Unity game project using Unity 6000.3.6f1 with Universal Render Pipeline (URP 17.3.0).

## Unity MCP Integration

This project includes `com.coplaydev.unity-mcp` for direct Unity Editor interaction via Claude Code.

**Workflow obbligatorio:**
1. Quando viene richiesto di fare operazioni in engine o modificare script, usa sempre le chiamate al server MCP: UnityMCP
2. Prima di eseguire qualsiasi operazione, verifica la connessione e ottieni la scena attualmente aperta con `manage_scene(action="get_active")`
3. Per chiamate multiple in contemporanea, usa sempre il tool `batch_execute`

**Tools disponibili:**
- Scene management (`manage_scene`)
- GameObject operations (`manage_gameobject`, `find_gameobjects`)
- Script creation/editing (`manage_script`, `script_apply_edits`)
- Asset management (`manage_asset`)
- Console monitoring (`read_console`) - check after script changes for compilation errors

## Code Architecture

**Script Organization:**
- Runtime scripts: `Assets/Scripts/Runtime/`
- Namespace pattern: `Assets.Scripts.Runtime`

**Render Pipeline:**
- Dual configuration for PC (`PC_RPAsset.asset`) and Mobile (`Mobile_Renderer.asset`)
- Post-processing via `DefaultVolumeProfile.asset`

**Input System:**
- Uses Unity's new Input System
- Actions defined in `Assets/InputSystem_Actions.inputactions`

## Testing

Unity Test Framework is available (`com.unity.test-framework`). Run tests via:
- MCP: `run_tests` tool with `mode="EditMode"` or `mode="PlayMode"`
- Unity Editor: Window > General > Test Runner

## Key Packages

- `com.unity.inputsystem` - New Input System for input handling
- `com.unity.render-pipelines.universal` - URP graphics
- `com.unity.timeline` - Cinematic sequences
- `com.unity.visualscripting` - Visual scripting support
