namespace CzechDraughts
{
    internal class Graphics
    {
        private readonly Data data;

        private const string MyTab = "      ";
        private Dictionary<int, string> helpTexts = new Dictionary<int, string>();
        private Dictionary<int, string> statsTexts = new Dictionary<int, string>();
        private Dictionary<int, string> creativeTexts = new Dictionary<int, string>();

        public Graphics(Data data) 
        { 
            this.data = data;
            helpTexts = new Dictionary<int, string>
            {
                { 1,  "Current player: "},
                { 3,  "H          shows this dialog (help)" },
                { 4,  "G          give up" },
                { 5,  "D          suggest a draw" },
                { 6,  "R          resets your" },
                { 7,  "           choice of square" },
                { 8,  "M          shows where a piece on the" },
                { 9,  "           current square could end up" },
                { 10, "← ↑ → ↓    use arrows to move on the" },
                { 11, "           board or to select a move" },
                { 12, "Enter      chooses current square (blue)" }
            };
            statsTexts = new Dictionary<int, string>
            {
                { 1, "Current player: "},
                { 3, "Move: " },
                { 4, "White took " },
                { 5, "Black took " },
                { 7, "" },
                { 8, "" },
                { 10, "" },
                { 12, "(press H to show help)" }
            };
            creativeTexts = new Dictionary<int, string>
            {
                { 1,  "Starting player is: " },
                { 3,  "← ↑ → ↓    use arrows to move on the" },
                { 4,  "           board or to select a move" },
                { 5,  "Press numbers to place pieces:" },
                { 6,  "0          to remove a piece" },
                { 7,  "5          to place Black Regular piece" },
                { 8,  "6          to place Black Queen piece" },
                { 9,  "8          to place White Regular piece" },
                { 10, "9          to place White Queen piece" },
                { 11, "To change starting player press:" },
                { 12, "W          for White" },
                { 13, "B          for Black" },
                { 15, "Q/ESC      press Q or Escape to finish" },
                { 16, "           up and start game" }
            };
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Czech draughts";
            Console.ResetColor();
        }

        public void RenderCurrentState(
            Square? currentSquare = null,
            string message = "",
            bool help = false,
            bool end = false,
            bool creative = false,
            List<List<Square>>? possibleMoves = null,
            int moveIndex = -1)
        {
            Console.Clear();
            Square chosenSquare = currentSquare ?? new Square();
            
            Console.WriteLine();

            for (int xrow = 0; xrow < 16; xrow++)
            {
                bool isTop = xrow % 2 == 0;
                int row = xrow / 2;

                for (int xcol = 0; xcol < 16; xcol++)
                {
                    bool isLeft = xcol % 2 == 0;
                    int col = xcol / 2;

                    SetSquareBackgroundColor(row, col, chosenSquare, possibleMoves, moveIndex);

                    if (data.chosenPiece.X != -1 && row == data.chosenPiece.Y && col == data.chosenPiece.X)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                    }

                    RenderSquareContent(row, col, isTop, isLeft, possibleMoves, moveIndex);
                }
                RenderSidebar(help, end, creative, xrow);

                Console.WriteLine();
            }
            Console.Write(new string(' ', 32));
            RenderSidebar(help, end, creative, 16);

            if (currentSquare == null && possibleMoves == null) Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\n" + message + "\n");
        }

        private void RenderSidebar(bool help, bool end, bool creative, int xrow)
        {
            Console.ResetColor();
            if (!creative)
            {
                if (!help && !end) RenderMenu(statsTexts, xrow, stats: true);
                if (help && !end) RenderMenu(helpTexts, xrow);
                if (end) RenderEnd();
            }
            else RenderMenu(creativeTexts, xrow);
            Console.ResetColor();
        }

        private void RenderMenu(Dictionary<int, string> texts, int line, bool stats = false)
        {
            if (!texts.TryGetValue(line, out string? textToWrite)) return;

            Console.Write(MyTab + textToWrite);
            if (line == 1)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(data.currentPlayer);
            }
            if (stats) StatsDynamicValuesRender(line);
        }

        private void StatsDynamicValuesRender(int line)
        {
            switch (line)
            {
                case 3:
                    Console.Write(data.statistics.Move);
                    break;
                case 4:
                    Console.Write(data.statistics.WhiteScore + " black " + (data.statistics.WhiteScore == 1 ? "piece" : "pieces"));
                    break;
                case 5:
                    Console.Write(data.statistics.BlackScore + " white " + (data.statistics.BlackScore == 1 ? "piece" : "pieces"));
                    break;
                case 7:
                    if (data.maxRepeatedHistory == 2) Console.Write("You repeat your moves too much!");
                    break;
                case 8:
                    if (data.maxRepeatedHistory == 2) Console.Write("Game will end in draw!");
                    break;
                case 10:
                    if (data.statistics.Move == 7 && data.currentPlayer == Piece.White) {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.Write(" E\u0304\u0307\u0306a\u0302\u0310\u0307s\u0308\u0313\u0314t\u0311\u0313\u0309e\u0307\u0312\u0304r\u0310\u0303\u0305 \u0314\u0313E\u0308\u0313\u0314g\u0304\u0306\u0311g\u0310\u0308\u0312 "); 
                    }
                    break;
            }
        }

        private void RenderSquareContent(int row, int col, bool isTop, bool isLeft, List<List<Square>>? possibleMoves, int moveIndex)
        {
            Piece pieceColor = (data.pieces[row, col] | Piece.Types) ^ Piece.Types;
            Piece pieceType = (data.pieces[row, col] | Piece.Colors) ^ Piece.Colors;

            Console.ForegroundColor = pieceColor switch
            {
                Piece.White => ConsoleColor.White,
                Piece.Black => ConsoleColor.Black,
                _ => Console.BackgroundColor
            };

            if (!isTop && !isLeft && (possibleMoves != null))
            {
                string? moveIndexToRender = RenderPossibleMoveIndex(row, col, possibleMoves, moveIndex);
                if (moveIndexToRender != null)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(moveIndexToRender);
                    return;
                }
            }

            switch (pieceType)
            {
                case Piece.Queen:
                    RenderQueen(isTop, isLeft);
                    break;
                case Piece.Regular: 
                    RenderRegular(isTop, isLeft);
                    break;
                default:
                    Console.Write("  ");
                    break;
            }
        }

        private string? RenderPossibleMoveIndex(int row, int col, List<List<Square>> possibleMoves, int moveIndex)
        {
            if (moveIndex == -1)
            {
                foreach (List<Square> possibleMove in possibleMoves)
                {
                    Square lastSquare = possibleMove.LastOrDefault();
                    if (lastSquare.Y == row && lastSquare.X == col)
                    {
                        int currentPossibleMoveIndex = possibleMoves.IndexOf(possibleMove);
                        
                        if (currentPossibleMoveIndex >= 10) return $"{currentPossibleMoveIndex}";
                        return $" {currentPossibleMoveIndex}";
                    }
                }
            }
            else
            {
                List<Square> squaresInMove = possibleMoves[moveIndex];
                foreach (Square square in squaresInMove)
                {
                    if (square.Y == row && square.X == col)
                    {
                        int currentPossibleMoveIndex = squaresInMove.IndexOf(square);
                        
                        if (currentPossibleMoveIndex >= 10) return $"{currentPossibleMoveIndex}";
                        return $" {currentPossibleMoveIndex}";
                    }
                }
            }
            return null;
        }

        private void SetSquareBackgroundColor(int row, int col, Square chosenSquare, List<List<Square>>? possibleMoves, int moveIndex)
        {
            Console.BackgroundColor = (row == chosenSquare.Y && col == chosenSquare.X)
                ? ConsoleColor.Blue
                : (ConsoleColor) data.board[row, col];

            if (possibleMoves == null) return;

            if (moveIndex == -1)
            {
                foreach (List<Square> possibleMove in possibleMoves)
                {
                    Square lastSquare = possibleMove.LastOrDefault();
                    if (lastSquare.Y == row && lastSquare.X == col)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        return;
                    }
                }
            }
            else
            {
                List<Square> squaresInMove = possibleMoves[moveIndex];
                foreach (Square square in squaresInMove)
                {
                    if (square.Y == row && square.X == col)
                    {
                        Console.BackgroundColor = (data.pieces[square.Y, square.X] == Piece.None
                            ? ConsoleColor.Yellow
                            : ConsoleColor.DarkYellow);
                        return;
                    }
                }
            }
        }

        private void RenderQueen(bool top, bool left)
        {
            Console.Write(top 
                ? (left ? "\u251B\u2533" : "\u2533\u2517") 
                : (left ? "\u2513\u253B" : "\u253B\u250F"));
        }

        private void RenderRegular(bool top, bool left)
        {
            Console.Write(top 
                ? (left ? "\u250F\u2501" : "\u2501\u2513") 
                : (left ? "\u2517\u2501" : "\u2501\u251B"));
        }

        private void RenderEnd()
        {
            Console.Write(MyTab);
            SetForegroundAndBackgroundToContrastingRandomColors();
            for (int i = 0; i < 3; i++)
            {
                switch (data.status)
                {
                    case GameStatus.Draw:
                        Console.Write(" Draw Draw "); 
                        break;
                    case GameStatus.WhiteWin:
                        Console.Write(" White WON!! ");
                        break;
                    case GameStatus.BlackWin:
                        Console.Write(" Black WON!! ");
                        break;
                    default:
                        break;
                }
            }
        }

        private void SetForegroundAndBackgroundToContrastingRandomColors()
        {
            Random random = new Random();
            ConsoleColor[] contrastingColors = new ConsoleColor[] { ConsoleColor.Black, ConsoleColor.White, ConsoleColor.Red, 
                ConsoleColor.Green, ConsoleColor.Blue, ConsoleColor.Yellow, 
                ConsoleColor.Cyan, ConsoleColor.Magenta, ConsoleColor.Gray
            };

            do
            {
                Console.BackgroundColor = contrastingColors[random.Next(contrastingColors.Length)];
                Console.ForegroundColor = contrastingColors[random.Next(contrastingColors.Length)];
            }
            while (Console.BackgroundColor == Console.ForegroundColor);
        }

        public void InitialScreen(int type)
        {
            Console.ResetColor();
            Console.Clear();

            string paddingLine = new string('\n', 10);
            int width = Console.WindowWidth;

            string firstType =  "Classic PvP";
            string secondType = "Creative PvP";
            string message = "Use Enter and arrows to choose game type\n";

            Console.Write(paddingLine);
            Console.Write(new string(' ', (width - firstType.Length - secondType.Length - 1) / 2));
            if (type == 0) Console.BackgroundColor = ConsoleColor.Blue;
            Console.Write(firstType);
            Console.ResetColor();
            Console.Write(" ");
            if (type == 1) Console.BackgroundColor = ConsoleColor.Blue;
            Console.Write(secondType);
            Console.ResetColor();
            Console.Write(paddingLine);

            Console.Write(new string(' ', (width - message.Length + 1) / 2) + message);
        }
    }
}
