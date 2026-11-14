# SudokuGen

Generates **6x6** and **9x9** Sudoku puzzles with a **difficulty from 1 to 10**.
Every puzzle is guaranteed to have **exactly one solution**.

You get two `int[,]` arrays back: the puzzle (hidden cells are `0`) and the full solution.

## Install

```
dotnet add package SudokuGen
```

Targets `netstandard2.0`, so it works with .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+, Unity, MAUI and Xamarin.

## Usage

```csharp
using SudokuGen;

var result = SudokuGenerator.Generate(new SudokuConfig
{
    Size = 9,         // 6 or 9
    Difficulty = 7,   // 1 (easy) .. 10 (hard)
    Seed = 42         // optional: same seed + settings = same puzzle
});

int[,] puzzle   = result.Puzzle;    // [row, column], 0 = hidden cell
int[,] solution = result.Solution;  // fully solved board

int visible = result.Clues;         // number of visible cells
int seed    = result.Seed;          // store it to regenerate this exact puzzle later
```

Invalid settings throw `ArgumentOutOfRangeException` (size other than 6 or 9, difficulty outside 1..10).

### Boards

| Size | Boxes |
|------|-------|
| 9x9  | 3 rows x 3 columns |
| 6x6  | 2 rows x 3 columns |

### Difficulty

Difficulty controls how many cells are visible (linear between the two ends):

| Difficulty | 9x9 visible cells | 6x6 visible cells |
|-----------:|------------------:|------------------:|
| 1          | 46                | 26                |
| 5          | 37                | 20                |
| 10         | 25                | 12                |

Notes:
- Difficulty is based on the number of clues only. It does not rate which solving techniques are required.
- At the hardest levels, the generator may not always be able to reach the target without losing uniqueness. It retries and returns the best result. Check `result.Clues` against `result.TargetClues` if you care. Uniqueness is always guaranteed.
- `SudokuGenerator.Generate` is thread-safe.
## License

MIT
