using System.Numerics;

namespace EndlessSudokuLibrary;

    /// <summary>Configuration for the generator.</summary>
    public sealed class SudokuConfig
    {
        /// <summary>Board size: 6 (2x3 boxes) or 9 (3x3 boxes).</summary>
        public int Size { get; set; } = 9;

        /// <summary>Difficulty from 1 (easiest, most visible cells) to 10 (hardest, fewest visible cells).</summary>
        public int Difficulty { get; set; } = 5;

        /// <summary>Optional seed for reproducible puzzles.</summary>
        public int? Seed { get; set; }

        internal void Validate()
        {
            if (Size != 6 && Size != 9)
                throw new ArgumentOutOfRangeException(nameof(Size), Size, "Size must be 6 or 9.");
            if (Difficulty < 1 || Difficulty > 10)
                throw new ArgumentOutOfRangeException(nameof(Difficulty), Difficulty, "Difficulty must be between 1 and 10.");
        }
    }

    /// <summary>The generated puzzle and its unique solution.</summary>
    public sealed class SudokuResult
    {
        /// <summary>The puzzle. Hidden cells are 0.</summary>
        public int[,] Puzzle { get; }

        /// <summary>The fully solved board.</summary>
        public int[,] Solution { get; }

        /// <summary>Number of visible cells (clues) in <see cref="Puzzle"/>.</summary>
        public int Clues { get; }

        /// <summary>Number of clues the difficulty level was aiming for.</summary>
        public int TargetClues { get; }

        internal SudokuResult(int[,] puzzle, int[,] solution, int clues, int targetClues)
        {
            Puzzle = puzzle;
            Solution = solution;
            Clues = clues;
            TargetClues = targetClues;
        }
    }

    /// <summary>Generates Sudoku puzzles that are guaranteed to have exactly one solution.</summary>
    public static class SudokuGenerator
    {
        // Clues at difficulty 1 and difficulty 10 for each supported size.
        private const int Easy9 = 46, Hard9 = 25;
        private const int Easy6 = 26, Hard6 = 12;

        // How many times we restart the "remove cells" phase when the target isn't reached.
        private const int MaxAttempts = 50;

        public static SudokuResult Generate(SudokuConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            config.Validate();

            int n = config.Size;
            int boxRows = n == 9 ? 3 : 2;
            int boxCols = 3;

            Random rng = config.Seed.HasValue ? new Random(config.Seed.Value) : new Random();
            var solver = new SudokuSolver(n, boxRows, boxCols);

            // 1) Build a random, completely filled valid grid.
            int[] solution = solver.GenerateFullGrid(rng);

            // 2) Remove cells (keeping the solution unique) until we reach the difficulty's clue target.
            int target = TargetClues(n, config.Difficulty);
            int[]? bestPuzzle = null;
            int bestClues = int.MaxValue;

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                int[] puzzle = (int[])solution.Clone();
                int clues = RemoveCells(puzzle, solver, rng, target);

                if (clues < bestClues)
                {
                    bestClues = clues;
                    bestPuzzle = puzzle;
                }
                if (bestClues <= target) break;
            }

            return new SudokuResult(To2D(bestPuzzle, n), To2D(solution, n), bestClues, target);
        }

        /// <summary>Maps difficulty 1..10 to a number of visible cells (linear interpolation).</summary>
        private static int TargetClues(int size, int difficulty)
        {
            int easy = size == 9 ? Easy9 : Easy6;
            int hard = size == 9 ? Hard9 : Hard6;
            double t = (difficulty - 1) / 9.0;
            return (int)Math.Round(easy + (hard - easy) * t);
        }

        /// <summary>
        /// Visits cells in random order and hides each one only if the puzzle still has exactly one solution.
        /// Returns the number of remaining clues.
        /// </summary>
        private static int RemoveCells(int[] puzzle, SudokuSolver solver, Random rng, int targetClues)
        {
            int[] order = new int[puzzle.Length];
            for (int i = 0; i < order.Length; i++) order[i] = i;
            Shuffle(order, rng);

            int clues = puzzle.Length;
            foreach (int idx in order)
            {
                if (clues <= targetClues) break;

                int backup = puzzle[idx];
                puzzle[idx] = 0;

                if (solver.HasUniqueSolution(puzzle))
                    clues--;
                else
                    puzzle[idx] = backup; // removing it would allow multiple solutions
            }
            return clues;
        }

        internal static void Shuffle(int[] array, Random rng)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        private static int[,] To2D(int[] flat, int n)
        {
            var result = new int[n, n];
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    result[r, c] = flat[r * n + c];
            return result;
        }
    }

    /// <summary>
    /// Bitmask-based backtracking solver (picks the most constrained cell first).
    /// Used both to build a full grid and to count solutions (up to a limit).
    /// </summary>
    internal sealed class SudokuSolver
    {
        private readonly int _n;
        private readonly int _boxRows;
        private readonly int _boxCols;
        private readonly int _fullMask;

        private readonly int[] _cells;
        private readonly int[] _rowMask;
        private readonly int[] _colMask;
        private readonly int[] _boxMask;
        private readonly int[] _boxOf;

        private int[] _snapshot;

        public SudokuSolver(int n, int boxRows, int boxCols)
        {
            _n = n;
            _boxRows = boxRows;
            _boxCols = boxCols;
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

        /// <summary>Returns a random, fully solved grid.</summary>
        public int[] GenerateFullGrid(Random rng)
        {
            Load(null);
            Search(1, rng);
            return _snapshot;
        }

        /// <summary>True if the grid is consistent and has exactly one solution.</summary>
        public bool HasUniqueSolution(int[] grid)
        {
            if (!Load(grid)) return false;
            return Search(2, null) == 1;
        }

        private bool Load(int[] grid)
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
        /// digits are tried in random order and the first solution found is stored in _snapshot.
        /// </summary>
        private int Search(int limit, Random rng)
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
                if (rng != null) _snapshot = (int[])_cells.Clone();
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
                    int tmp = digits[i];
                    digits[i] = digits[j];
                    digits[j] = tmp;
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