namespace CzechDraughts
{
    internal class Data
    {
        public BoardSquare[,] board;
        public Piece[,] pieces;
        public List<Piece[]> history;

        public Piece currentPlayer;
        public GameStatus status;
        public GameStatistics statistics;
        public Square chosenPiece;
        public int maxRepeatedHistory;

        public Data()
        {
            board = new BoardSquare[8, 8];
            pieces = new Piece[8, 8];
            history = new List<Piece[]>();
            currentPlayer = Piece.White;
            status = GameStatus.InProgress;
            statistics = new GameStatistics();
            chosenPiece = new Square(-1, -1);
            SetInitialGame();
        }

        public void SetInitialGame()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    bool isRed = (row + col) % 2 == 0;

                    board[row, col] = isRed ? BoardSquare.Red : BoardSquare.Green;
                    pieces[row, col] = isRed && (row < 3 || row > 4)
                        ? (row < 3 ? Piece.BlackRegular : Piece.WhiteRegular)
                        : Piece.None;
                }
            }
        }

        public void NonifyPiecesOnBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    pieces[row, col] = Piece.None;
                }
            }
        }

        public void FlipBoardAndPieces()
        {
            currentPlayer = (currentPlayer == Piece.White ? Piece.Black : Piece.White); 

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    int oppositeRow = 8 - row - 1;

                    (board[row, col], board[oppositeRow, col]) = (board[oppositeRow, col], board[row, col]);
                    (pieces[row, col], pieces[oppositeRow, col]) = (pieces[oppositeRow, col], pieces[row, col]);
                }
            }
        }

        public void UpdateHistory()
        {
            Piece[] newHistoryRecord = new Piece[64];
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    newHistoryRecord[row * 8 + col] = pieces[row, col];
                }
            }
            history.Add(newHistoryRecord);
        }

        public bool MaximumRepeatingRecordsInHistory()
        {
            foreach (Piece[] historyRecord in history)
            {
                int repeatCounter = history.FindAll(record => record.SequenceEqual(historyRecord)).Count;
                if (repeatCounter == 3)
                {
                    return true;
                }
                if (repeatCounter > maxRepeatedHistory) maxRepeatedHistory = repeatCounter;
            }
            return false;
        }
    }

    internal enum BoardSquare
    {
        Red = ConsoleColor.DarkRed,
        Green = ConsoleColor.DarkGreen
    }

    internal enum GameStatus : byte
    {
        InProgress = 0x00,
        UpdateFinished = 0x01,
        BlackWin = 0x04,
        WhiteWin = 0x08,
        Draw = 0x10
    }

    internal enum Piece : byte
    {
        None = 0x00,

        Regular = 0x01,
        Queen = 0x02,
        Types = Regular | Queen,

        Black = 0x04,
        White = 0x08,
        Colors = Black | White,

        BlackRegular = Regular | Black,
        BlackQueen = Queen | Black,

        WhiteRegular = Regular | White,
        WhiteQueen = Queen | White
    }

    internal struct Square : IEquatable<Square>
    {
        public int X;
        public int Y;

        public Square()
        {
            X = -1;
            Y = -1;
        }

        public Square(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Square(Square other, int dx, int dy)
        {
            X = other.X + dx;
            Y = other.Y + dy;
        }

        public bool Equals(Square other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object? other)
        {
            if (other is null) return false;

            return other is Square otherSquare && Equals(otherSquare);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }

    internal struct GameStatistics
    {
        public int Move;
        public int WhiteScore;
        public int BlackScore;
    }
}
