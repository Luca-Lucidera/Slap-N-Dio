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
- Supporta tastiera + fino a 3 gamepad simultanei

## Game Systems Architecture

### 1. Player Management System (`PlayerManager.cs`)

**Responsabilità:**
- Gestione centralizzata di tutti i player attivi
- Rilevamento automatico dispositivi via `InputSystem.onDeviceChange`
- Supporta 1 tastiera + max 3 gamepad simultanei

**Struttura:**
- `keyboardPlayer` - Player controllato da tastiera (sempre attivo)
- `gamepadPlayers` - Dictionary<int, GameObject> per mapping deviceId → Player
- `deadPlayers` - HashSet<Transform> per tracking player morti

**Spawn Positions e Colori:**
| Slot | Dispositivo | Posizione | Colore |
|------|-------------|-----------|--------|
| 0 | Tastiera | [-3, 0.5, 0] | Blu |
| 1 | Gamepad 1 | [-1, 0.5, 0] | Rosso |
| 2 | Gamepad 2 | [1, 0.5, 0] | Verde |
| 3 | Gamepad 3 | [3, 0.5, 0] | Giallo |

### 2. Player Controller (`PlayerController.cs`)

**Controlli:**
- Tastiera: WASD per movimento 3D
- Gamepad: Left Stick analogico (deadzone 0.1)

**Inizializzazione:**
- `Initialize(null)` → modalità tastiera
- `Initialize(gamepad)` → modalità gamepad

### 3. Camera System (`CameraController.cs`)

**Parametri configurabili:**
- `minDistance = 15f` - Distanza minima
- `maxDistance = 35f` - Distanza massima
- `positionSmoothSpeed = 3f`
- `zoomSmoothSpeed = 2f`
- `centerSmoothSpeed = 4f`
- `cameraAngle = 45f` - Angolo isometrico

**Algoritmo Zoom:**
1. Calcola centro (baricentro) dei player attivi
2. Calcola spread tra player
3. `targetDistance = Lerp(min, max, Max(countFactor, spreadFactor))`
4. Applica SmoothDamp su tutti i parametri

**Integrazione:**
- Usa `PlayerManager.GetActivePlayerTransforms()` per ottenere player vivi
- Esclude automaticamente player morti dal calcolo

### 4. Kill & Respawn System (`KillY.cs`)

**Flusso:**
1. Player entra in trigger zone (Y negativo)
2. `playerManager.MarkPlayerAsDead(transform)`
3. Player disattivato
4. Attesa 5 secondi
5. Teleport a `respawnPoint`
6. Player riattivato
7. `playerManager.MarkPlayerAsAlive(transform)`

**Oggetti non-player:** Distrutti immediatamente

### 5. Altri Script
- `CubeDemonstratePhysic.cs` - Genera e lancia cubi con SPACE
- `SlapCube.cs` - Auto-distruzione cubi dopo collisione (1s delay)

### Diagramma Architettura
```
InputSystem.onDeviceChange
    ↓
PlayerManager (Core)
    ├── SpawnKeyboardPlayer() / TrySpawnGamepadPlayer()
    ├── RemoveGamepadPlayer()
    ├── MarkPlayerAsDead() / MarkPlayerAsAlive()
    └── GetActivePlayerTransforms()
            ↓
    ┌───────┴───────┐
    ↓               ↓
PlayerController  CameraController
(Input/Movement)  (Tracking/Zoom)
    ↓
KillY (Respawn) ←───┘
```

## Testing

Unity Test Framework is available (`com.unity.test-framework`). Run tests via:
- MCP: `run_tests` tool with `mode="EditMode"` or `mode="PlayMode"`
- Unity Editor: Window > General > Test Runner

## Key Packages

- `com.unity.inputsystem` - New Input System for input handling
- `com.unity.render-pipelines.universal` - URP graphics
- `com.unity.timeline` - Cinematic sequences
- `com.unity.visualscripting` - Visual scripting support
