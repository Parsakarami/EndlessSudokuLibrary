<div align="center">

<img src="https://raw.githubusercontent.com/Parsakarami/EndlessSudokuLibrary/main/endless-sudoku-icon.png" alt="EndlessSudoku logo" width="120" />

# EndlessSudoku

Generate 9×9 and 6×6 Sudoku puzzles with difficulty 1–10.
Every puzzle has exactly one solution.

[![NuGet](https://img.shields.io/nuget/v/EndlessSudoku.svg?logo=nuget&label=NuGet)](https://www.nuget.org/packages/EndlessSudoku)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

</div>

## Installation

Get it from [NuGet](https://www.nuget.org/packages/EndlessSudoku):

```bash
dotnet add package EndlessSudoku
```

Requires .NET 10 or later.

## Usage

```csharp
using EndlessSudokuLibrary;

var result = SudokuGenerator.Generate(new SudokuConfig
{
    Size = 9,        // 6 or 9 (default 9)
    Difficulty = 5,  // 1 = easy, 10 = hard (default 5)
    Seed = 42        // optional: same seed gives the same puzzle
});

int[,] puzzle   = result.Puzzle;    // [row, column], 0 = empty cell
int[,] solution = result.Solution;  // the solved board
```

Example output:

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

## Difficulty

Difficulty sets how many cells are filled in at the start:

| Difficulty | 1  | 3  | 5  | 7  | 10 |
|------------|----|----|----|----|----|
| 9×9        | 46 | 41 | 37 | 32 | 25 |
| 6×6        | 26 | 23 | 20 | 17 | 12 |

At levels 9–10 the generator may stop with a few more filled cells than the target. `result.Clues` gives the actual count and `result.TargetClues` gives the target.

## License

[MIT](https://opensource.org/licenses/MIT)
