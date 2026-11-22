using System.Diagnostics.CodeAnalysis;

namespace EndlessSudokuLibrary;

/// <summary>
/// Solves and checks any 6x6 or 9x9 board, including ones not made by <see cref="SudokuGenerator"/>.
/// Boards are <c>int[row, column]</c> with 0 for empty cells. All methods are thread-safe.
/// </summary>
public static class SudokuSolver
{
    /// <summary>Solves <paramref name="puzzle"/>. Returns null if it has no solution. The input is not changed.</summary>
    /// <remarks>If the puzzle has several solutions, one of them is returned. Use <see cref="HasUniqueSolution"/> to check.</remarks>
    /// <exception cref="ArgumentException">The board is not 6x6 or 9x9, or contains a value outside 0..size.</exception>
    public static int[,]? Solve(int[,] puzzle)
    {
        int n = SudokuBoard.Validate(puzzle, nameof(puzzle));
        int[]? solution = SudokuEngine.ForSize(n).Solve(SudokuBoard.ToFlat(puzzle));
        return solution == null ? null : SudokuBoard.To2D(solution, n);
    }

    /// <summary>Tries to solve <paramref name="puzzle"/>. Returns false if it has no solution.</summary>
    public static bool TrySolve(int[,] puzzle, [NotNullWhen(true)] out int[,]? solution)
    {
        solution = Solve(puzzle);
        return solution != null;
    }

    /// <summary>Counts the solutions of <paramref name="puzzle"/>, stopping once <paramref name="limit"/> is reached.</summary>
    /// <param name="puzzle">The board to check.</param>
    /// <param name="limit">Stop counting at this number (counting all solutions of an almost empty board would take forever).</param>
    public static int CountSolutions(int[,] puzzle, int limit = 2)
    {
        int n = SudokuBoard.Validate(puzzle, nameof(puzzle));
        if (limit < 1) throw new ArgumentOutOfRangeException(nameof(limit), limit, "Limit must be at least 1.");
        return SudokuEngine.ForSize(n).CountSolutions(SudokuBoard.ToFlat(puzzle), limit);
    }

    /// <summary>True if <paramref name="puzzle"/> has exactly one solution.</summary>
    public static bool HasUniqueSolution(int[,] puzzle) => CountSolutions(puzzle, 2) == 1;

    /// <summary>True if no digit appears twice in any row, column or box. Empty cells are allowed.</summary>
    /// <remarks>A valid board can still be unsolvable. Use <see cref="Solve"/> to check that.</remarks>
    public static bool IsValid(int[,] board)
    {
        int n = SudokuBoard.Validate(board, nameof(board));
        return SudokuEngine.ForSize(n).IsConsistent(SudokuBoard.ToFlat(board));
    }

    /// <summary>True if <paramref name="board"/> is completely filled and follows all Sudoku rules.</summary>
    public static bool IsSolved(int[,] board)
    {
        if (!IsValid(board)) return false;
        foreach (int v in board)
            if (v == 0) return false;
        return true;
    }
}
