# ☣️ [SYSTEM_OVERLOAD]: "Hostile Terminal" Tech Stack (v1.0.0)

> **“THE SYSTEM IS NOT BROKEN. IT IS SIMPLY EVOLVING BEYOND YOUR CONTROL.”**

## 📡 Core Technologies
- **Programming Language:** C# 12
- **Framework:** .NET 8.0 (LTS)
- **Primary Engine:** Spectre.Console (v0.54.0)
- **Reality Anchor:** Hard-coded **84x24** Static Viewport

## 🧪 Key Libraries & APIs
- **`Spectre.Console`**: The primary engine for high-performance TUI rendering. Used for Layouts, Panels, Tables, and Canvas-based visual entities.
- **`Pastel`**: Utilized for raw 24-bit console coloring, specifically for "Emergency Red" (`#FF0000`) and "Toxic Green" (`#00FF00`).
- **`Microsoft.Extensions.AI` / `OllamaSharp`**: Integrated for the "Sentient Malignancy" dialogue and logic processing.
- **`System.Text.StringBuilder`**: Maintained for high-performance, flicker-free rendering of dense ASCII corruption fragments.
- **`System.Threading`**: Used to simulate hardware struggle and system hangs (e.g., `Thread.Sleep(800)`).

## ⛓️ Technical Constraints & Global Rules
1. **Strictly Text-Based:** The application is a pure console application. No GUI libraries (e.g., ImGui, ClickableTransparentOverlay) are permitted. Existing references are considered **DEPRECATED**.
2. **Zero-Deletions Policy:** Existing logic, variables, functions, and strings must not be removed. Development is purely additive—focusing on **infecting** the codebase rather than refactoring.
3. **The 84x24 Safe Zone:** To ensure cross-platform compatibility (WSL/Windows), all rendering and logic must be contained within a `84x24` safe zone. Loops must iterate up to `width - 1` and `height - 1`.
4. **Buffer Stability:** `WriteLine()` is strictly forbidden. All output must be managed with `SetCursorPosition()` or Spectre-based `AnsiConsole` updates to prevent any scrolling or shifting of the display.

## 🏛️ Architectural Mandates
- **Atmospheric Hostility:** The UI is an antagonist. Features include 'Forbidden' Unicode corruption and 50% density ASCII rain.
- **The Reality Anchor:** The `84x24` grid is a hard-coded, immutable containment fence.
- **Flicker Prevention:** `Console.Clear()` is forbidden to avoid white-flash flickering. Use `AnsiConsole.Live()` or overwrite the entire 84x24 buffer frame-by-frame.
- **The Immutable Seed:** The `💀`, `⸸`, `☣`, and `☢` icons (and the 'DUMGERAGN' logo) are the focal points of every screen. Corruption effects must swirl around these elements but never overwrite them.
- **Hardware Agony:** Corruption phases must loop significantly with mandatory `Thread.Sleep(800)` iterations to simulate a struggling terminal.
- **Visual Entities:** Drones and Enemies are rendered as `Canvas` objects or `CanvasImage` textures to move beyond "Plain Text" combat and increase atmospheric dread.

## 📜 Deployment Protocols
- **Build Command:** `dotnet build`
- **Cleanup Command:** `so-down` (Custom Target: `SpookyCleanup`)
- **Version Control:** All technical dev protocols reside on the **Dev** branch; the **Main** branch remains the stable manifesto.