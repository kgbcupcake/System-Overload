# "Hostile Terminal" Tech Stack

## Core Technologies
- **Programming Language:** C#
- **Framework:** .NET 8.0

## Key Libraries & APIs
- **`Pastel`:** Utilized for 24-bit console coloring, specifically for "Emergency Red" (`#FF0000`) and "Toxic Green" (`#00FF00`).
- **`System.Text.StringBuilder`:** Critical for high-performance, flicker-free rendering of dense ASCII corruption effects.
- **`System.Threading`:** Used to simulate hardware struggle and system hangs, specifically with `Thread.Sleep(800)`.
- **`System.Console`:** Employed for precise cursor control (`SetCursorPosition`) and writing (`Write`) to maintain a static 85x25 viewport.

## Technical Constraints & Global Rules
1.  **Strictly Text-Based:** The application is a pure console application. No GUI libraries (e.g., ImGui, ClickableTransparentOverlay) are to be used. Existing references to these are considered deprecated. No new dependencies should be added.
2.  **Zero-Deletions Policy:** Existing logic, variables, functions, and strings must not be removed or modified. Development is purely additive, focusing on "infecting" the codebase rather than refactoring or cleaning it.
3.  **Cross-Platform Safe Zone:** To ensure compatibility across Windows and Linux without buffer-scrolling issues, all rendering and logic must be contained within a `84x24` safe zone. Loops must iterate up to `width - 1` and `height - 1`.
4.  **Buffer Stability:** `Console.WriteLine()` is strictly forbidden. All output must be managed with `Console.SetCursorPosition()` and `Console.Write()` to prevent any scrolling or shifting of the display.

## Architectural Mandates
- **Atmospheric Hostility:** The UI is an antagonist, characterized by 'Forbidden' Unicode corruption and 50% density ASCII rain.
- **The Reality Anchor:** The `85x25` grid is a hard-coded, immutable containment fence.
- **StringBuilder Implementation:** All visual frames must be pre-rendered in a `StringBuilder` before being written to the console to ensure visual stability.
- **Hardware Agony:** A corruption phase must loop 20 times, with a mandatory `Thread.Sleep(800)` on each iteration to simulate a struggling terminal.
- **Perfect Centering:** All centered text must be calculated using the hard-coded 85-width offset.
- **The Immutable Seed:** All existing icons (💀, ⸸, ☣, ☢) and the 'DUMGERAGN' logo must remain the focal points of every screen. Corruption effects must swirl around these elements but never overwrite them, ensuring the 'Cursed Artifact' identity remains recognizable through the decay.
- **The Total Frame Reset:** To prevent "Ghosting," every iteration of the `StringBuilder` must begin by filling the `84x24` buffer with the "Immutable Seed" (Logo/Icons) and then layering the ASCII decay on top. `Console.Clear()` is forbidden to avoid white-flash flickering; instead, the entire buffer must be overwritten frame-by-frame.
