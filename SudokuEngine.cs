using System.Numerics;

namespace EndlessSudokuLibrary;

/// <summary>
/// Bitmask-based backtracking solver (picks the most constrained cell first).
/// Used to build a full grid, to solve puzzles and to count solutions (up to a limit).
/// Not thread-safe: create one instance per call.
/// </summary>
internal sealed class SudokuEngine
{
    private readonly int _n;
    private readonly int _fullMask;

    private readonly int[] _cells;
    private readonly int[] _rowMask;
    private readonly int[] _colMask;
    private readonly int[] _boxMask;
    private readonly int[] _boxOf;

    private int[]? _snapshot;
    private bool _capture;

    public SudokuEngine(int n, int boxRows, int boxCols)
    {
        _n = n;
        _fullMask = (1 << n) - 1;

        _cells = new int[n * n];
        _rowMask = new int[n];
        _colMask = new int[n];
        _boxMask = new int[n];
        _boxOf = new int[n * n];

        int boxesPerRow = n / boxCols;
        for (int r = 0; r < n; r++)
            for (int c = 0; c < n; c++)
                _boxOf[r * n + c] = (r / boxRows) * boxesPerRow + (c / boxCols);
    }

    public static SudokuEngine ForSize(int n)
    {
        var (boxRows, boxCols) = SudokuBoard.BoxShape(n);
        return new SudokuEngine(n, boxRows, boxCols);
    }

    /// <summary>Returns a random, fully solved grid.</summary>
    public int[] GenerateFullGrid(Random rng)
    {
        Load(null);
        _capture = true;
        Search(1, rng);
        return _snapshot!;
    }

    /// <summary>True if the grid is consistent and has exactly one solution.</summary>
    public bool HasUniqueSolution(int[] grid) => CountSolutions(grid, 2) == 1;

    /// <summary>Counts solutions, stopping at <paramref name="limit"/>. Returns 0 for conflicting grids.</summary>
    public int CountSolutions(int[] grid, int limit)
    {
        if (!Load(grid)) return 0;
        _capture = false;
        return Search(limit, null);
    }

    /// <summary>Returns a solution, or null if the grid has none.</summary>
    public int[]? Solve(int[] grid)
    {
        if (!Load(grid)) return null;
        _capture = true;
        _snapshot = null;
        return Search(1, null) == 1 ? _snapshot : null;
    }

    /// <summary>True if no digit appears twice in a row, column or box.</summary>
    public bool IsConsistent(int[] grid) => Load(grid);

    private bool Load(int[]? grid)
    {
        Array.Clear(_cells, 0, _cells.Length);
        Array.Clear(_rowMask, 0, _n);
        Array.Clear(_colMask, 0, _n);
        Array.Clear(_boxMask, 0, _n);

        if (grid == null) return true;

        for (int i = 0; i < grid.Length; i++)
        {
            int d = grid[i];
            if (d == 0) continue;

            int bit = 1 << (d - 1);
            if ((Candidates(i) & bit) == 0) return false; // conflicting givens
            Place(i, d);
        }
        return true;
    }

    private int Candidates(int idx)
    {
        int used = _rowMask[idx / _n] | _colMask[idx % _n] | _boxMask[_boxOf[idx]];
        return _fullMask & ~used;
    }

    private void Place(int idx, int digit)
    {
        int bit = 1 << (digit - 1);
        _cells[idx] = digit;
        _rowMask[idx / _n] |= bit;
        _colMask[idx % _n] |= bit;
        _boxMask[_boxOf[idx]] |= bit;
    }

    private void Unplace(int idx, int digit)
    {
        int bit = ~(1 << (digit - 1));
        _cells[idx] = 0;
        _rowMask[idx / _n] &= bit;
        _colMask[idx % _n] &= bit;
        _boxMask[_boxOf[idx]] &= bit;
    }

    /// <summary>
    /// Counts solutions up to <paramref name="limit"/>. When <paramref name="rng"/> is supplied,
    /// digits are tried in random order. When capturing, the solution found is stored in _snapshot.
    /// </summary>
    private int Search(int limit, Random? rng)
    {
        // Find the empty cell with the fewest candidates.
        int best = -1, bestMask = 0, bestCount = int.MaxValue;
        for (int i = 0; i < _cells.Length; i++)
        {
            if (_cells[i] != 0) continue;

            int mask = Candidates(i);
            int count = BitOperations.PopCount((uint)mask);
            if (count == 0) return 0; // dead end

            if (count < bestCount)
            {
                best = i;
                bestMask = mask;
                bestCount = count;
                if (count == 1) break;
            }
        }

        if (best < 0) // no empty cells: solved
        {
            if (_capture) _snapshot = (int[])_cells.Clone();
            return 1;
        }

        Span<int> digits = stackalloc int[_n];
        int k = 0;
        for (int m = bestMask; m != 0; m &= m - 1)
            digits[k++] = BitOperations.TrailingZeroCount(m) + 1;

        if (rng != null)
        {
            for (int i = k - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (digits[i], digits[j]) = (digits[j], digits[i]);
            }
        }

        int found = 0;
        for (int i = 0; i < k; i++)
        {
            int d = digits[i];
            Place(best, d);
            found += Search(limit - found, rng);
            Unplace(best, d);

            if (found >= limit) break;
        }
        return found;
    }
}
