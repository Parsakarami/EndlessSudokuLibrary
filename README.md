<div align="center">

<img src="https://raw.githubusercontent.com/Parsakarami/EndlessSudokuLibrary/main/endless-sudoku-icon.png" alt="EndlessSudoku logo" width="110" />

# EndlessSudoku

**Unlimited Sudoku puzzles for .NET**

[![NuGet](https://img.shields.io/nuget/v/EndlessSudoku.svg?logo=nuget&label=NuGet)](https://www.nuget.org/packages/EndlessSudoku)
[![Downloads](https://img.shields.io/nuget/dt/EndlessSudoku.svg?label=Downloads)](https://www.nuget.org/packages/EndlessSudoku)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

🧩 9×9 and 6×6 boards &nbsp;·&nbsp; 🎚️ Difficulty 1–10 &nbsp;·&nbsp; ✅ Always one solution &nbsp;·&nbsp; 🧠 Solver & hints

</div>

---

## 📦 Install

Get it from **[NuGet](https://www.nuget.org/packages/EndlessSudoku)**:

```bash
dotnet add package EndlessSudoku
```

## 🚀 Quick Start

```csharp
using EndlessSudokuLibrary;

var result = SudokuGenerator.Generate(size: 9, difficulty: 5);

int[,] puzzle   = result.Puzzle;    // 0 = empty cell
int[,] solution = result.Solution;

Console.WriteLine(result);          // prints the puzzle
```

```
5 6 3 | . . 9 | 2 . .
. 8 4 | . 2 6 | . 3 7
. . . | 4 . 3 | . 6 .
------+-------+------
2 . . | . . . | . . .
8 . 6 | 2 3 7 | 4 9 .
4 7 . | . . 8 | . 2 .
------+-------+------
6 . . | 9 8 . | . . 1
. . . | 3 . 5 | . 7 2
. . . | 7 1 . | . 5 .
```

## ⚙️ Options

| Option       | Values        | Default |
|--------------|---------------|:-------:|
| `Size`       | `6` or `9`    | `9`     |
| `Difficulty` | `1` – `10`    | `5`     |
| `Seed`       | any `int`     | random  |

Pass them directly, or use a `SudokuConfig`:

```csharp
var result = SudokuGenerator.Generate(new SudokuConfig { Size = 6, Difficulty = 3, Seed = 42 });
```

The same seed and settings always give the same puzzle. Every result has a `Seed`, so you can recreate even a random puzzle later.

## 🎚️ Difficulty

| Level        | Difficulty | 9×9 clues | 6×6 clues |
|--------------|:----------:|:---------:|:---------:|
| 🟢 Easy      | 1 – 3      | 46 – 41   | 26 – 23   |
| 🟡 Medium    | 4 – 6      | 39 – 34   | 21 – 18   |
| 🟠 Hard      | 7 – 8      | 32 – 30   | 17 – 15   |
| 🔴 Expert    | 9 – 10     | 27 – 25   | 14 – 12   |

> [!NOTE]
> At the hardest levels the puzzle may keep a few extra clues so it stays uniquely solvable.
> Compare `result.Clues` with `result.TargetClues` to check.

## 🎮 Game Helpers

```csharp
result.IsCorrect(row, col, value);     // is this the right digit?
result.IsSolvedBy(board);              // has the player finished?

var hint = result.GetHint(board);      // next move, or null if solved
// hint.Row, hint.Column, hint.Value, hint.IsCorrection (true = fix a wrong digit)

SudokuBoard.GetCandidates(board, row, col);   // possible digits, for pencil marks
```

## 🧠 Solver

Works with any 6×6 or 9×9 board, including ones you didn't generate.

```csharp
int[,]? solved = SudokuSolver.Solve(board);   // null if there's no solution

SudokuSolver.HasUniqueSolution(board);        // exactly one solution?
SudokuSolver.IsValid(board);                  // no repeated digits?
SudokuSolver.IsSolved(board);                 // full and valid?
```

## 💾 Save & Load

```csharp
string saved = SudokuBoard.ToCompactString(board);   // "563..92...84.26.37..."
int[,] loaded = SudokuBoard.Parse(saved);

string pretty = SudokuBoard.ToText(board);           // grid with box borders
```

## ⚡ Async & Batches

```csharp
// Run in the background so the UI stays responsive (supports cancellation)
var result = await SudokuGenerator.GenerateAsync(new SudokuConfig { Difficulty = 9 });

// Generate several puzzles at once
var puzzles = SudokuGenerator.GenerateMany(new SudokuConfig { Difficulty = 4 }, count: 10);
```

---

<div align="center">

[MIT License](https://opensource.org/licenses/MIT) · Made by **Parsa Karami**

</div>
