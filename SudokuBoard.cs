using System.Numerics;
using System.Text;

namespace EndlessSudokuLibrary;

/// <summary>
/// Helpers for working with boards: printing, saving/loading as text, and listing possible digits.
/// Boards are <c>int[row, column]</c> with 0 for empty cells.
/// </summary>
public static class SudokuBoard
{
    /// <summary>
    /// Formats a board as a readable grid with box borders. Empty cells are shown as <c>.</c>
    /// <code>
    /// 5 6 3 | . . 9 | 2 . .
    /// . 8 4 | . 2 6 | . 3 7
    /// ...
    /// </code>
    /// </summary>
    public static string ToText(int[,] board)
    {
        int n = Validate(board, nameof(board));
        var (boxRows, boxCols) = BoxShape(n);
        string divider = string.Join("-+-", Enumerable.Repeat(new string('-', boxCols * 2 - 1), n / boxCols));

        var sb = new StringBuilder();
        for (int r = 0; r < n; r++)
        {
            if (r > 0 && r % boxRows == 0) sb.AppendLine(divider);

            for (int c = 0; c < n; c++)
            {
                if (c > 0) sb.Append(c % boxCols == 0 ? " | " : " ");
                sb.Append(board[r, c] == 0 ? '.' : (char)('0' + board[r, c]));
            }
            if (r < n - 1) sb.AppendLine();
        }
        return sb.ToString();
    }

    /// <summary>
    /// Formats a board as a single line (36 or 81 characters, <c>.</c> for empty cells), e.g. to save a game.
    /// Load it back with <see cref="Parse"/>.
    /// </summary>
    public static string ToCompactString(int[,] board)
    {
        Validate(board, nameof(board));
        var sb = new StringBuilder(board.Length);
        foreach (int v in board) sb.Append(v == 0 ? '.' : (char)('0' + v));
        return sb.ToString();
    }

    /// <summary>
    /// Reads a board from text. Accepts the output of <see cref="ToCompactString"/> and <see cref="ToText"/>.
    /// Digits are cell values, <c>.</c> or <c>0</c> is an empty cell; spaces, new lines and <c>| - +</c> are ignored.
    /// The text must contain 36 cells (6x6) or 81 cells (9x9).
    /// </summary>
    /// <exception cref="FormatException">The text is not a valid board.</exception>
    public static int[,] Parse(string text)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));

        var cells = new List<int>(81);
        foreach (char ch in text)
        {
            if (ch >= '0' && ch <= '9') cells.Add(ch - '0');
            else if (ch == '.') cells.Add(0);
            else if (char.IsWhiteSpace(ch) || ch == '|' || ch == '-' || ch == '+') continue;
            else throw new FormatException($"Unexpected character '{ch}' in board text.");
        }

        int n = cells.Count == 81 ? 9 : cells.Count == 36 ? 6 : 0;
        if (n == 0) throw new FormatException($"Board text has {cells.Count} cells; expected 36 (6x6) or 81 (9x9).");

        foreach (int v in cells)
            if (v > n) throw new FormatException($"Digit {v} is not allowed on a {n}x{n} board.");

        return To2D(cells.ToArray(), n);
    }

    /// <summary>
    /// Returns the digits that can go in a cell without breaking a rule, based on the digits already on the board
    /// (useful for pencil marks). Returns an empty array for a filled cell.
    /// </summary>
    public static int[] GetCandidates(int[,] board, int row, int column)
    {
        int n = Validate(board, nameof(board));
        if (row < 0 || row >= n) throw new ArgumentOutOfRangeException(nameof(row));
        if (column < 0 || column >= n) throw new ArgumentOutOfRangeException(nameof(column));
        if (board[row, column] != 0) return Array.Empty<int>();

        int mask = CandidateMask(board, row, column);
        var result = new int[BitOperations.PopCount((uint)mask)];
        int k = 0;
        for (int m = mask; m != 0; m &= m - 1)
            result[k++] = BitOperations.TrailingZeroCount(m) + 1;
        return result;
    }

    // ---- internal helpers ----

    /// <summary>Box shape for a board size: 9 -> 3x3, 6 -> 2 rows x 3 columns.</summary>
    internal static (int Rows, int Cols) BoxShape(int n) => n == 9 ? (3, 3) : (2, 3);

    /// <summary>Checks the board is 6x6 or 9x9 with values in 0..size and returns its size.</summary>
    internal static int Validate(int[,] board, string paramName)
    {
        if (board == null) throw new ArgumentNullException(paramName);

        int n = board.GetLength(0);
        if (n != board.GetLength(1) || (n != 6 && n != 9))
            throw new ArgumentException("Board must be 6x6 or 9x9.", paramName);

        for (int r = 0; r < n; r++)
            for (int c = 0; c < n; c++)
            {
                int v = board[r, c];
                if (v < 0 || v > n)
                    throw new ArgumentException($"Cell [{r}, {c}] has value {v}; expected 0..{n}.", paramName);
            }
        return n;
    }

    /// <summary>Bitmask of digits (bit d-1 = digit d) not yet used in the cell's row, column and box.</summary>
    internal static int CandidateMask(int[,] board, int row, int column)
    {
        int n = board.GetLength(0);
        var (boxRows, boxCols) = BoxShape(n);
        int used = 0;

        for (int i = 0; i < n; i++)
        {
            if (board[row, i] != 0) used |= 1 << (board[row, i] - 1);
            if (board[i, column] != 0) used |= 1 << (board[i, column] - 1);
        }

        int r0 = row / boxRows * boxRows, c0 = column / boxCols * boxCols;
        for (int r = r0; r < r0 + boxRows; r++)
            for (int c = c0; c < c0 + boxCols; c++)
                if (board[r, c] != 0) used |= 1 << (board[r, c] - 1);

        return ((1 << n) - 1) & ~used;
    }

    internal static int[] ToFlat(int[,] board)
    {
        int n = board.GetLength(0);
        var flat = new int[n * n];
        for (int r = 0; r < n; r++)
            for (int c = 0; c < n; c++)
                flat[r * n + c] = board[r, c];
        return flat;
    }

    internal static int[,] To2D(int[] flat, int n)
    {
        var result = new int[n, n];
        for (int r = 0; r < n; r++)
            for (int c = 0; c < n; c++)
                result[r, c] = flat[r * n + c];
        return result;
    }
}
