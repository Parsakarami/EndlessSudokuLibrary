<div align="center">

<img src="https://raw.githubusercontent.com/Parsakarami/EndlessSudokuLibrary/main/endless-sudoku-icon.png" alt="EndlessSudoku logo" width="120" />

# EndlessSudoku

**Unlimited Sudoku puzzles for .NET, with one line of code.**

Generate **9×9** and **6×6** puzzles at any **difficulty from 1 to 10**.
Every puzzle has **exactly one solution**.

[![NuGet](https://img.shields.io/nuget/v/EndlessSudoku.svg?logo=nuget&label=NuGet)](https://www.nuget.org/packages/EndlessSudoku)
[![Downloads](https://img.shields.io/nuget/dt/EndlessSudoku.svg?label=Downloads)](https://www.nuget.org/packages/EndlessSudoku)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

</div>

---

## ✨ Features

- 🧩 **Two board sizes**: classic 9×9 (3×3 boxes) and compact 6×6 (2×3 boxes)
- 🎚️ **10 difficulty levels**, from relaxed to very hard
- ✅ **Exactly one solution** for every puzzle
- 🎲 **Repeatable puzzles**: the same seed always gives the same puzzle
- 📦 **No dependencies**: a single small library
- 🧵 **Thread-safe**: call it from anywhere
- ⚡ **Fast**: uses a bitmask backtracking solver

---

## 📦 Installation

**.NET CLI**

```bash
dotnet add package EndlessSudoku
```

**Package Manager Console**

```powershell
Install-Package EndlessSudoku
```

**PackageReference**

```xml
<PackageReference Include="EndlessSudoku" Version="*" />
```

> Requires **.NET 10** or later.

---

## 🚀 Quick Start

```csharp
using EndlessSudokuLibrary;

var result = SudokuGenerator.Generate(new SudokuConfig
{
    Size = 9,        // 6 or 9
    Difficulty = 5   // 1 (easy) to 10 (hard)
});

int[,] puzzle   = result.Puzzle;    // the board to show the player (0 = empty cell)
int[,] solution = result.Solution;  // the fully solved board
```

That's it. Both boards are indexed as `[row, column]`.

The puzzle above (with `Seed = 42`) looks like this:

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

---

## ⚙️ Configuration

`SudokuConfig` has three settings:

| Property     | Type   | Default | Description                                                          |
|--------------|--------|:-------:|----------------------------------------------------------------------|
| `Size`       | `int`  | `9`     | Board size. Must be `6` or `9`.                                      |
| `Difficulty` | `int`  | `5`     | `1` (easiest) to `10` (hardest).                                     |
| `Seed`       | `int?` | `null`  | Optional. Use the same seed to get the same puzzle again. Leave it `null` for a random puzzle. |

Since every setting has a default, this also works:

```csharp
var result = SudokuGenerator.Generate(new SudokuConfig()); // 9x9, difficulty 5, random
```

## 📤 Result

`SudokuGenerator.Generate` returns a `SudokuResult`:

| Property      | Type      | Description                                                    |
|---------------|-----------|----------------------------------------------------------------|
| `Puzzle`      | `int[,]`  | The puzzle. Empty cells are `0`.                                |
| `Solution`    | `int[,]`  | The complete solution.                                          |
| `Clues`       | `int`     | How many cells are filled in `Puzzle`.                          |
| `TargetClues` | `int`     | How many filled cells the chosen difficulty aimed for.          |

---

## 🎚️ Difficulty Levels

Difficulty sets how many cells are filled in at the start. Fewer filled cells means a harder puzzle.

| Difficulty | 9×9 filled cells | 6×6 filled cells | Feels like |
|:----------:|:----------------:|:----------------:|------------|
| 1          | 46               | 26               | 🟢 Easy    |
| 2          | 44               | 24               | 🟢 Easy    |
| 3          | 41               | 23               | 🟢 Easy    |
| 4          | 39               | 21               | 🟡 Medium  |
| 5          | 37               | 20               | 🟡 Medium  |
| 6          | 34               | 18               | 🟡 Medium  |
| 7          | 32               | 17               | 🟠 Hard    |
| 8          | 30               | 15               | 🟠 Hard    |
| 9          | 27               | 14               | 🔴 Expert  |
| 10         | 25               | 12               | 🔴 Expert  |

> **Good to know**
> - Difficulty is based only on the number of filled cells. It does not look at which solving techniques a player needs.
> - At levels 9–10, the generator sometimes can't remove enough cells while keeping one unique solution. It tries several times and returns the closest result. Compare `result.Clues` with `result.TargetClues` if this matters to you. The puzzle **always** has exactly one solution.

---

## 📚 Examples

### Print a board to the console

```csharp
using EndlessSudokuLibrary;

var result = SudokuGenerator.Generate(new SudokuConfig { Size = 9, Difficulty = 5 });
Print(result.Puzzle);

static void Print(int[,] board)
{
    int n = board.GetLength(0);
    int boxRows = n == 9 ? 3 : 2, boxCols = 3;

    for (int r = 0; r < n; r++)
    {
        if (r > 0 && r % boxRows == 0)
            Console.WriteLine(string.Join("-+-",
                Enumerable.Repeat(new string('-', boxCols * 2 - 1), n / boxCols)));

        for (int c = 0; c < n; c++)
        {
            if (c > 0) Console.Write(c % boxCols == 0 ? " | " : " ");
            Console.Write(board[r, c] == 0 ? "." : board[r, c].ToString());
        }
        Console.WriteLine();
    }
}
```

### A small 6×6 puzzle

Good for kids, beginners, or small screens.

```csharp
var result = SudokuGenerator.Generate(new SudokuConfig { Size = 6, Difficulty = 3 });
```

```
. 6 1 | 5 3 4
. . 4 | . . 6
------+------
. 3 6 | 1 2 5
. . 5 | . 6 3
------+------
6 . 2 | 3 . 1
5 . 3 | 6 . 2
```

### Check a player's move

```csharp
bool IsCorrect(SudokuResult game, int row, int col, int value)
    => game.Solution[row, col] == value;

if (!IsCorrect(game, row: 0, col: 3, value: 8))
    Console.WriteLine("Not quite. Try again!");
```

### Daily puzzle (same puzzle for everyone today)

Use the date as the seed so every player gets the same puzzle on the same day.

```csharp
int todaySeed = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd")); // e.g. 20261122

var daily = SudokuGenerator.Generate(new SudokuConfig
{
    Size = 9,
    Difficulty = 6,
    Seed = todaySeed
});
```

### Save and restore a puzzle

The result does not store the seed. To save a game, pick a seed yourself and keep it together with the size and difficulty:

```csharp
int seed = Random.Shared.Next();
var config = new SudokuConfig { Size = 9, Difficulty = 7, Seed = seed };
var game = SudokuGenerator.Generate(config);

// Save: seed, 9, 7
// Later, the same settings rebuild the exact same puzzle:
var restored = SudokuGenerator.Generate(new SudokuConfig { Size = 9, Difficulty = 7, Seed = seed });
```

### Invalid settings

Bad values throw an `ArgumentOutOfRangeException`:

```csharp
try
{
    SudokuGenerator.Generate(new SudokuConfig { Size = 8 });
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine(ex.Message); // Size must be 6 or 9.
}
```

---

## 🔍 How It Works

1. **Fill**: build a complete, valid grid by placing numbers in random order.
2. **Remove**: visit the cells in random order and clear each one, but only if the puzzle still has exactly one solution.
3. **Stop**: once the number of filled cells reaches the difficulty's target, return the puzzle and its solution.

---

## 🤝 Contributing

Issues and pull requests are welcome on [GitHub](https://github.com/Parsakarami/EndlessSudokuLibrary).

1. Fork the repository
2. Create a branch: `git checkout -b my-feature`
3. Commit your changes
4. Open a pull request

---

## 📄 License

Released under the [MIT License](https://opensource.org/licenses/MIT).

<div align="center">

Made with ❤️ by **Parsa Karami** · [NuGet](https://www.nuget.org/packages/EndlessSudoku) · [GitHub](https://github.com/Parsakarami/EndlessSudokuLibrary)

</div>
