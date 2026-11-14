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
- At the hardest levels the generator may not always be able to reach the target without losing uniqueness. It retries and returns the best result. Check `result.Clues` against `result.TargetClues` if you care. Uniqueness is always guaranteed.
- `SudokuGenerator.Generate` is thread-safe.

## Repository layout

```
SudokuGen.sln
src/SudokuGen/              the library (this is what gets packed)
tests/SudokuGen.Tests/      xUnit tests
samples/SudokuGen.Demo/     console demo
CHANGELOG.md
```

## Developing

```
dotnet build
dotnet test
dotnet run --project samples/SudokuGen.Demo
```

## Releasing a new version

The package version lives in **one place**: the `<Version>` element in `src/SudokuGen/SudokuGen.csproj`.
Follow [Semantic Versioning](https://semver.org/) (`MAJOR.MINOR.PATCH`):

| Change | Bump | Example |
|--------|------|---------|
| Bug fix, no API change | PATCH | 1.0.0 -> 1.0.1 |
| New feature, backwards compatible (e.g. a new optional config property) | MINOR | 1.0.1 -> 1.1.0 |
| Breaking API change | MAJOR | 1.1.0 -> 2.0.0 |

Because puzzles are reproducible from a seed, changing the generation algorithm changes which puzzle a given seed produces. Mention it in the changelog and treat it as at least a MINOR bump (MAJOR if consumers rely on stored seeds).

Release steps:

1. Edit `<Version>` in `src/SudokuGen/SudokuGen.csproj`, for example to `1.1.0`.
2. Move the entries under `## [Unreleased]` in `CHANGELOG.md` into a new `## [1.1.0] - YYYY-MM-DD` section.
3. Run the tests: `dotnet test`
4. Pack: `dotnet pack src/SudokuGen -c Release -o artifacts`
   This creates `artifacts/SudokuGen.1.1.0.nupkg` and `artifacts/SudokuGen.1.1.0.snupkg` (debug symbols).
5. Publish: `dotnet nuget push artifacts/SudokuGen.1.1.0.nupkg --api-key <YOUR_KEY> --source https://api.nuget.org/v3/index.json`
6. Tag the commit: `git tag v1.1.0 && git push --tags`

To try a version before publishing, add `-beta.1` to the version (`1.1.0-beta.1`). NuGet treats it as a pre-release.
You can also test locally by packing into a folder and adding that folder as a NuGet source.

A published version can never be changed or re-uploaded, only unlisted, so always bump `<Version>` before pushing.

## Before the first publish

Replace the placeholders:
- `YOUR NAME` and `YOUR-USER` in `src/SudokuGen/SudokuGen.csproj`, `LICENSE` and `CHANGELOG.md`
- `PackageId`, if `SudokuGen` is already taken on nuget.org (for example `YourName.SudokuGen`)

## License

MIT
