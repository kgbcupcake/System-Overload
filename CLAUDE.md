# Dungeon Adventures – Reborn

## Code size rules

- **No file over 350 lines.** Aim for 300 or fewer. Check `wc -l` on every file you create or grow, and split it before it passes the limit.
- **No god classes.** Each class does one job. When a class starts collecting unrelated work (drawing, input, rules, data), move each part into its own class. Small one-job classes are better than one big class.
- **How to split:** by what each part does, one class per file (for example `Src/Scenes/Props/Torch.cs`, `Tombstone.cs`, `Stair.cs`). A `partial` class split across files (`CombatEncounter.Items.cs`) only hides a big class, so don't use it to get a god class under the limit. The class itself still has to be split up.
