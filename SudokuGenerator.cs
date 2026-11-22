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

        internal SudokuConfig Clone() => new SudokuConfig { Size = Size, Difficulty = Difficulty, Seed = Seed };
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

        /// <summary>Board size (6 or 9).</summary>
        public int Size => Puzzle.GetLength(0);

        /// <summary>The difficulty level (1..10) used to generate this puzzle.</summary>
        public int Difficulty { get; }

        /// <summary>
        /// The seed used to generate this puzzle. Pass it back in <see cref="SudokuConfig.Seed"/>
        /// (with the same size and difficulty) to generate the exact same puzzle again.
        /// </summary>
        public int Seed { get; }

        internal SudokuResult(int[,] puzzle, int[,] solution, int clues, int targetClues, int difficulty, int seed)
        {
            Puzzle = puzzle;
            Solution = solution;
            Clues = clues;
            TargetClues = targetClues;
            Difficulty = difficulty;
            Seed = seed;
        }

        /// <summary>True if <paramref name="value"/> is the correct digit for the given cell.</summary>
        public bool IsCorrect(int row, int column, int value)
        {
            if (row < 0 || row >= Size) throw new ArgumentOutOfRangeException(nameof(row));
            if (column < 0 || column >= Size) throw new ArgumentOutOfRangeException(nameof(column));
            return Solution[row, column] == value;
        }

        /// <summary>True if <paramref name="board"/> is completely filled and matches the solution.</summary>
        public bool IsSolvedBy(int[,] board)
        {
            EnsureSameSize(board);
            for (int r = 0; r < Size; r++)
                for (int c = 0; c < Size; c++)
                    if (board[r, c] != Solution[r, c]) return false;
            return true;
        }

        /// <summary>
        /// Suggests the next move for a player's current <paramref name="board"/>.
        /// A wrong digit is reported first (<see cref="SudokuHint.IsCorrection"/> = true).
        /// Otherwise it picks the empty cell with the fewest possible digits, which is usually the easiest one to fill.
        /// Returns null when the board is already solved.
        /// </summary>
        public SudokuHint? GetHint(int[,] board)
        {
            EnsureSameSize(board);
            int n = Size;

            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    int v = board[r, c];
                    if (v != 0 && v != Solution[r, c])
                        return new SudokuHint(r, c, Solution[r, c], isCorrection: true);
                }

            int bestRow = -1, bestCol = -1, bestCount = int.MaxValue;
            for (int r = 0; r < n && bestCount > 1; r++)
                for (int c = 0; c < n; c++)
                {
                    if (board[r, c] != 0) continue;

                    int count = System.Numerics.BitOperations.PopCount((uint)SudokuBoard.CandidateMask(board, r, c));
                    if (count < bestCount)
                    {
                        bestRow = r;
                        bestCol = c;
                        bestCount = count;
                        if (count == 1) break;
                    }
                }

            return bestRow < 0 ? null : new SudokuHint(bestRow, bestCol, Solution[bestRow, bestCol], isCorrection: false);
        }

        /// <summary>The puzzle as readable text (see <see cref="SudokuBoard.ToText"/>).</summary>
        public override string ToString() => SudokuBoard.ToText(Puzzle);

        private void EnsureSameSize(int[,] board)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (board.GetLength(0) != Size || board.GetLength(1) != Size)
                throw new ArgumentException($"Board must be {Size}x{Size}.", nameof(board));
        }
    }

    /// <summary>A suggested move returned by <see cref="SudokuResult.GetHint"/>.</summary>
    public sealed class SudokuHint
    {
        /// <summary>Row of the cell (0-based).</summary>
        public int Row { get; }

        /// <summary>Column of the cell (0-based).</summary>
        public int Column { get; }

        /// <summary>The correct digit for the cell.</summary>
        public int Value { get; }

        /// <summary>True if the cell currently holds a wrong digit that should be replaced.</summary>
        public bool IsCorrection { get; }

        internal SudokuHint(int row, int column, int value, bool isCorrection)
        {
            Row = row;
            Column = column;
            Value = value;
            IsCorrection = isCorrection;
        }

        /// <inheritdoc />
        public override string ToString() =>
            $"{(IsCorrection ? "Fix" : "Fill")} [{Row}, {Column}] = {Value}";
    }

    /// <summary>Generates Sudoku puzzles that are guaranteed to have exactly one solution.</summary>
    public static class SudokuGenerator
    {
        // Clues at difficulty 1 and difficulty 10 for each supported size.
        private const int Easy9 = 46, Hard9 = 25;
        private const int Easy6 = 26, Hard6 = 12;

        // How many times we restart the "remove cells" phase when the target isn't reached.
        private const int MaxAttempts = 50;

        /// <summary>Generates a puzzle using the given configuration.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Size or difficulty is out of range.</exception>
        public static SudokuResult Generate(SudokuConfig config) => Generate(config, CancellationToken.None);

        /// <summary>Generates a puzzle. Shortcut for <see cref="Generate(SudokuConfig)"/>.</summary>
        /// <param name="size">6 or 9.</param>
        /// <param name="difficulty">1 (easy) to 10 (hard).</param>
        /// <param name="seed">Optional seed for a reproducible puzzle.</param>
        public static SudokuResult Generate(int size = 9, int difficulty = 5, int? seed = null) =>
            Generate(new SudokuConfig { Size = size, Difficulty = difficulty, Seed = seed });

        /// <summary>Generates a puzzle and stops early if <paramref name="cancellationToken"/> is cancelled.</summary>
        /// <exception cref="OperationCanceledException">The operation was cancelled.</exception>
        public static SudokuResult Generate(SudokuConfig config, CancellationToken cancellationToken)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            config.Validate();

            int n = config.Size;
            int seed = config.Seed ?? Random.Shared.Next();
            var rng = new Random(seed);
            var engine = SudokuEngine.ForSize(n);

            // 1) Build a random, completely filled valid grid.
            int[] solution = engine.GenerateFullGrid(rng);

            // 2) Remove cells (keeping the solution unique) until we reach the difficulty's clue target.
            int target = TargetClues(n, config.Difficulty);
            int[]? bestPuzzle = null;
            int bestClues = int.MaxValue;

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int[] puzzle = (int[])solution.Clone();
                int clues = RemoveCells(puzzle, engine, rng, target, cancellationToken);

                if (clues < bestClues)
                {
                    bestClues = clues;
                    bestPuzzle = puzzle;
                }
                if (bestClues <= target) break;
            }

            return new SudokuResult(
                SudokuBoard.To2D(bestPuzzle!, n), SudokuBoard.To2D(solution, n),
                bestClues, target, config.Difficulty, seed);
        }

        /// <summary>
        /// Generates a puzzle on a background thread, so UI apps (MAUI, WPF, Unity, ...) stay responsive.
        /// </summary>
        public static Task<SudokuResult> GenerateAsync(SudokuConfig config, CancellationToken cancellationToken = default)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            config.Validate();

            var copy = config.Clone(); // later changes to config by the caller don't affect this call
            return Task.Run(() => Generate(copy, cancellationToken), cancellationToken);
        }

        /// <summary>
        /// Generates <paramref name="count"/> puzzles with the same size and difficulty.
        /// If <see cref="SudokuConfig.Seed"/> is set, the whole set is reproducible.
        /// </summary>
        public static IReadOnlyList<SudokuResult> GenerateMany(SudokuConfig config, int count)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            config.Validate();
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), count, "Count cannot be negative.");

            Random? seeds = config.Seed.HasValue ? new Random(config.Seed.Value) : null;
            var results = new List<SudokuResult>(count);

            for (int i = 0; i < count; i++)
            {
                results.Add(Generate(new SudokuConfig
                {
                    Size = config.Size,
                    Difficulty = config.Difficulty,
                    Seed = seeds?.Next()
                }));
            }
            return results;
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
        private static int RemoveCells(int[] puzzle, SudokuEngine engine, Random rng, int targetClues, CancellationToken cancellationToken)
        {
            int[] order = new int[puzzle.Length];
            for (int i = 0; i < order.Length; i++) order[i] = i;
            Shuffle(order, rng);

            int clues = puzzle.Length;
            foreach (int idx in order)
            {
                if (clues <= targetClues) break;
                cancellationToken.ThrowIfCancellationRequested();

                int backup = puzzle[idx];
                puzzle[idx] = 0;

                if (engine.HasUniqueSolution(puzzle))
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
    }
