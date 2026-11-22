<div align="center">

<img src="https://raw.githubusercontent.com/Parsakarami/EndlessSudokuLibrary/main/endless-sudoku-icon.png" alt="EndlessSudoku logo" width="110" />

# EndlessSudoku

**Unlimited Sudoku puzzles for .NET**

[![NuGet](https://img.shields.io/nuget/v/EndlessSudoku.svg?logo=nuget&label=NuGet)](https://www.nuget.org/packages/EndlessSudoku)
[![Downloads](https://img.shields.io/nuget/dt/EndlessSudoku.svg?label=Downloads)](https://www.nuget.org/packages/EndlessSudoku)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

🧩 9×9 and 6×6 boards &nbsp;·&nbsp; 🎚️ Difficulty 1–10 &nbsp;·&nbsp; ✅ Always one solution &nbsp;·&nbsp; 🎲 Repeatable with a seed

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

var result = SudokuGenerator.Generate(new SudokuConfig
{
    Size = 9,
    Difficulty = 5
});

int[,] puzzle   = result.Puzzle;    // 0 = empty cell
int[,] solution = result.Solution;
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

The same `Seed` with the same settings always gives the same puzzle.

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

## 💡 Examples

**Daily puzzle**: everyone gets the same puzzle today

```csharp
int seed = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd"));
var daily = SudokuGenerator.Generate(new SudokuConfig { Difficulty = 6, Seed = seed });
```

**Check a player's move**

```csharp
bool correct = result.Solution[row, col] == value;
```

---

<div align="center">

[MIT License](https://opensource.org/licenses/MIT) · Made by **Parsa Karami**

</div>
