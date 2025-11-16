namespace CzechDraughts
{
    internal class Logic
    {
        private Data data;
        private List<List<Square>> listOfPossibleMoves;
        private bool queen;
        private bool tookSomething;

        public Logic(Data data) 
        { 
            this.data = data;
            listOfPossibleMoves = new List<List<Square>>();
        }

        public List<List<Square>> PossibleMoves(Square curentSquare)
        {
            List<List<Square>> possibleMovesOfCurrentSquare = GetPossibleMoves(curentSquare, onlyAggressive: false);
            List<List<Square>> allMoves = CountPlayerMoves(data.currentPlayer);

            for (int i = possibleMovesOfCurrentSquare.Count - 1; i >= 0; i--)
            {
                if (!MovesContainMove(allMoves, possibleMovesOfCurrentSquare[i]))
                {
                    possibleMovesOfCurrentSquare.RemoveAt(i);
                }
            }
            return possibleMovesOfCurrentSquare;
        }

        private List<List<Square>> GetPossibleMoves(Square curentSquare, bool onlyAggressive = false)
        {
            listOfPossibleMoves = new List<List<Square>>();
            queen = (data.pieces[curentSquare.Y, curentSquare.X] & Piece.Queen) == Piece.Queen;
            tookSomething = false;

            List<Square> initialMove = new List<Square>();
            List<Square> jumpedOverPieces = new List<Square>();
            initialMove.Add(curentSquare);

            if (queen)
            {
                FindPossibleMovesQueen(
                    curentSquare, 
                    initialMove, jumpedOverPieces,
                    -1);
                FindPossibleMovesQueen(curentSquare, 
                    initialMove, jumpedOverPieces,
                    1);
            }
            else
            {
                FindPossibleMovesRegular(curentSquare, 
                    initialMove, jumpedOverPieces, 
                    -1);
            }

            if (tookSomething || onlyAggressive) return RemoveNonAggresiveMoves(listOfPossibleMoves);

            return listOfPossibleMoves;
        }

        public void RemoveNonMoves(Square startingSquare)
        {
            for (int i = listOfPossibleMoves.Count - 1; i >= 0; i--)
            {
                if (listOfPossibleMoves[i].Equals(new List<Square>() { startingSquare }))
                {
                    listOfPossibleMoves.RemoveAt(i);
                }
                else if (listOfPossibleMoves[i].Count == 0)
                {
                    listOfPossibleMoves.RemoveAt(i);
                }
            }
        }

        public List<List<Square>> CountPlayerMoves(Piece player)
        {
            bool flipped = false;
            if (GetCurrentEnemy() == player)
            {
                data.FlipBoardAndPieces();
                flipped = true;
            }
            List<List<Square>> moves = new List<List<Square>>();
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if ((data.pieces[row, col] & player) == player)
                    {
                        moves.AddRange(GetPossibleMoves(new Square(col, row), onlyAggressive: false));
                    }
                }
            }
            
            if (flipped) data.FlipBoardAndPieces();

            foreach (List<Square> move in moves)
            {
                if (IsAggressiveMove(move))
                {
                    return RemoveNonAggresiveMoves(moves);
                }
            }
            
            return moves;
        }

        private bool MovesContainMove(List<List<Square>> moves, List<Square> move)
        {
            foreach (List<Square> moveFromMoves in moves)
            {
                if (MovesAreEqual(moveFromMoves, move)) return true;
            }
            return false;
        }

        private bool MovesAreEqual(List<Square> moveA, List<Square> moveB)
        {
            if (moveA.Count != moveB.Count) return false;
            for (int i = 0; i < moveA.Count; i++)
            {
                if (!moveA[i].Equals(moveB[i])) return false;
            }
            return true;
        }

        private bool IsAggressiveMove(List<Square> move)
        {
            foreach (Square square in move)
            {
                if (IsEnemyOn(square))
                {
                    return true;
                }
            }
            return false;
        }

        private List<List<Square>> RemoveNonAggresiveMoves(List<List<Square>> moves)
        {
            for (int i = moves.Count - 1; i >= 0; i--)
            {
                if (!IsAggressiveMove(moves[i]))
                {
                    moves.RemoveAt(i);
                }
            }
            return moves;
        }

        private bool IsSubmove(List<Square> alreadyProcessedMove, List<Square> newMove, bool returnValue)
        {
            if (alreadyProcessedMove.Count > newMove.Count) return false;

            for (int i = alreadyProcessedMove.Count - 1; i >= 0; i--)
            {
                if (!alreadyProcessedMove[i].Equals(newMove[i]))
                {
                    return false;
                }
            }

            if (alreadyProcessedMove.Count == newMove.Count) return false;
            return returnValue;
        }

        private void AddPossibleMove(List<Square> move, bool emptyAdd = false)
        {
            if (MovesContainMove(listOfPossibleMoves, move)) return;

            for (int i = listOfPossibleMoves.Count - 1; i >= 0; i--)
            {
                if (IsSubmove(listOfPossibleMoves[i], move, !emptyAdd))
                {
                    listOfPossibleMoves.RemoveAt(i);
                    listOfPossibleMoves.Add(move);
                    return;
                }
                if (IsSubmove(move, listOfPossibleMoves[i], !emptyAdd))
                {
                    return;
                }
            }
            listOfPossibleMoves.Add(move);
        }

        private bool LeftEmptyMove(Square leftSquare, Square rightSquare, int dy, int initialDirection, List<Square> initialMove, List<Square> jumpedOverPieces)
        {
            if (Empty(leftSquare) && (!EnemyThatCanBeDefeated(rightSquare, 1, dy) || queen) && initialDirection <= 0 && (!tookSomething || queen))
            {
                List<Square> newMoveLeftEmpty = new List<Square>(initialMove) { leftSquare };
                AddPossibleMove(newMoveLeftEmpty, emptyAdd: true);

                if (queen)
                {
                    FindPossibleMovesQueen(leftSquare,
                    newMoveLeftEmpty, new List<Square>(jumpedOverPieces),
                    dy, initialDirection: -1, wasEmptyMove: true);
                }
                return true;
            }
            return false;
        }

        private bool RightEmptyMove(Square leftSquare, Square rightSquare, int dy, int initialDirection, List<Square> initialMove, List<Square> jumpedOverPieces)
        {
            if (Empty(rightSquare) && (!EnemyThatCanBeDefeated(leftSquare, -1, dy) || queen) && initialDirection >= 0 && (!tookSomething || queen))
            {
                List<Square> newMoveRightEmpty = new List<Square>(initialMove) { rightSquare };
                AddPossibleMove(newMoveRightEmpty, emptyAdd: true);

                if (queen)
                {
                    FindPossibleMovesQueen(rightSquare,
                        newMoveRightEmpty, new List<Square>(jumpedOverPieces),
                        dy, initialDirection: 1, wasEmptyMove: true);
                }
                return true;
            }
            return false;
        }

        private bool LeftEnemyMove(Square leftSquare, Square rightSquare, int dy, int initialDirection, List<Square> initialMove, List<Square> jumpedOverPieces)
        {
            if (EnemyThatCanBeDefeated(leftSquare, -1, dy)
                && !(EnemyThatCanBeDefeated(rightSquare, 1, dy) && EnemyIsQueen(rightSquare))
                && !jumpedOverPieces.Contains(leftSquare))
            {
                Square newCurrentSquare = new Square(leftSquare, -1, dy);
                List<Square> newMoveLeftEnemy = new List<Square>(initialMove) { leftSquare, newCurrentSquare };

                tookSomething = true;
                if (!queen)
                {
                    FindPossibleMovesRegular(newCurrentSquare,
                        newMoveLeftEnemy, jumpedOverPieces,
                        dy, alreadyMoved: true);
                    return true;
                }

                jumpedOverPieces.Add(leftSquare);
                bool generalMove = false;

                Square newLeftSquare;
                Square newRightSquare;

                List<Square> initialMoveLeftEnemy = ExistDiagonalMove(newCurrentSquare, newMoveLeftEnemy, 1, dy);
                if (initialMoveLeftEnemy.Count > 0)
                {
                    newLeftSquare = new Square(initialMoveLeftEnemy.Last(), -1, dy);
                    newRightSquare = new Square(initialMoveLeftEnemy.Last(), 1, dy);
                    generalMove |= RightEnemyMove(newLeftSquare, newRightSquare, dy, -1, initialMoveLeftEnemy, new List<Square>(jumpedOverPieces));
                }
                
                initialMoveLeftEnemy = ExistDiagonalMove(newCurrentSquare, newMoveLeftEnemy, -1, -dy);
                if (initialMoveLeftEnemy.Count > 0)
                {
                    newLeftSquare = new Square(initialMoveLeftEnemy.Last(), -1, -dy);
                    newRightSquare = new Square(initialMoveLeftEnemy.Last(), 1, -dy);
                    generalMove |= LeftEnemyMove(newLeftSquare, newRightSquare, -dy, -1, initialMoveLeftEnemy, new List<Square>(jumpedOverPieces));
                }

                initialMoveLeftEnemy = ExistDiagonalMove(newCurrentSquare, newMoveLeftEnemy, -1, dy);
                if (initialMoveLeftEnemy.Count > 0)
                {
                    newLeftSquare = new Square(initialMoveLeftEnemy.Last(), -1, dy);
                    newRightSquare = new Square(initialMoveLeftEnemy.Last(), 1, dy);
                    generalMove |= LeftEnemyMove(newLeftSquare, newRightSquare, dy, -1, initialMoveLeftEnemy, new List<Square>(jumpedOverPieces));
                    generalMove |= RightEnemyMove(newLeftSquare, newRightSquare, dy, -1, initialMoveLeftEnemy, new List<Square>(jumpedOverPieces));
                }

                if (!generalMove)
                {
                    FindPossibleMovesQueen(newCurrentSquare,
                        newMoveLeftEnemy, new List<Square>(jumpedOverPieces),
                        dy, initialDirection: -1, alreadyMoved: true);
                }
                return true;
            }
            return false;
        }

        private bool RightEnemyMove(Square leftSquare, Square rightSquare, int dy, int initialDirection, List<Square> initialMove, List<Square> jumpedOverPieces)
        {
            if (EnemyThatCanBeDefeated(rightSquare, 1, dy)
                && !(EnemyThatCanBeDefeated(leftSquare, -1, dy) && EnemyIsQueen(leftSquare))
                && !jumpedOverPieces.Contains(rightSquare))
            {
                Square newCurrentSquare = new Square(rightSquare, 1, dy);
                List<Square> newMoveRightEnemy = new List<Square>(initialMove) { rightSquare, newCurrentSquare };
                tookSomething = true;

                if (!queen)
                {
                    FindPossibleMovesRegular(newCurrentSquare,
                        newMoveRightEnemy, jumpedOverPieces,
                        dy, alreadyMoved: true);
                    return true;
                }
                
                jumpedOverPieces.Add(rightSquare);
                bool generalMove = false;

                Square newLeftSquare;
                Square newRightSquare;

                List<Square> initialMoveRightEnemy = ExistDiagonalMove(newCurrentSquare, newMoveRightEnemy, -1, dy);
                if (initialMoveRightEnemy.Count > 0)
                {
                    newLeftSquare = new Square(initialMoveRightEnemy.Last(), -1, dy);
                    newRightSquare = new Square(initialMoveRightEnemy.Last(), 1, dy);
                    generalMove |= LeftEnemyMove(newLeftSquare, newRightSquare, dy, 1, initialMoveRightEnemy, new List<Square>(jumpedOverPieces));
                }

                initialMoveRightEnemy = ExistDiagonalMove(newCurrentSquare, newMoveRightEnemy, 1, -dy);
                if (initialMoveRightEnemy.Count > 0)
                {
                    newLeftSquare = new Square(initialMoveRightEnemy.Last(), -1, -dy);
                    newRightSquare = new Square(initialMoveRightEnemy.Last(), 1, -dy);
                    generalMove |= RightEnemyMove(newLeftSquare, newRightSquare, -dy, 1, initialMoveRightEnemy, new List<Square>(jumpedOverPieces));
                }

                initialMoveRightEnemy = ExistDiagonalMove(newCurrentSquare, newMoveRightEnemy, 1, dy);
                if (initialMoveRightEnemy.Count > 0)
                {
                    newLeftSquare = new Square(initialMoveRightEnemy.Last(), -1, dy);
                    newRightSquare = new Square(initialMoveRightEnemy.Last(), 1, dy);
                    generalMove |= RightEnemyMove(newLeftSquare, newRightSquare, dy, 1, initialMoveRightEnemy, new List<Square>(jumpedOverPieces));
                    generalMove |= LeftEnemyMove(newLeftSquare, newRightSquare, dy, 1, initialMoveRightEnemy, new List<Square>(jumpedOverPieces));
                }

                if (!generalMove)
                {
                    FindPossibleMovesQueen(newCurrentSquare,
                        newMoveRightEnemy, new List<Square>(jumpedOverPieces),
                        dy, initialDirection: 1, alreadyMoved: true);
                }
                return true;
            }
            return false;
        }

        public List<Square> ExistDiagonalMove(Square currentSquare, List<Square> initialMove, int dx, int dy)
        {
            Square nextSquare = new Square(currentSquare, dx, dy);
            List<Square> newInitialMove = new List<Square>(initialMove);

            while (InBoard(nextSquare))
            {
                if (IsCurrentPlayerOn(nextSquare))
                {
                    return new List<Square>();
                }
                if (IsEnemyOn(nextSquare))
                {
                    return newInitialMove;
                }
                newInitialMove.Add(nextSquare);
                nextSquare = new Square(nextSquare, dx, dy);
            }

            return newInitialMove;
        }

        private void FindPossibleMovesQueen(
            Square currentSquare,
            List<Square> initialMove,
            List<Square> jumpedOverPieces,
            int dy,
            int initialDirection = 0,
            bool alreadyMoved = false,
            bool wasEmptyMove = false)
        {
            Square leftSquare = new Square(currentSquare.X - 1, currentSquare.Y + dy);
            Square rightSquare = new Square(currentSquare.X + 1, currentSquare.Y + dy);

            bool leftEnemy = false;
            bool rightEnemy = false;
            bool leftEmpty = false;
            bool rightEmpty = false;

            if (InBoard(currentSquare))
            {
                List<Square> initialMoveLeftEnemy = ExistDiagonalMove(currentSquare, initialMove, -1, dy);
                if (initialMoveLeftEnemy.Count > 0 && initialDirection <= 0)
                {
                    leftEnemy = LeftEnemyMove(leftSquare, rightSquare, dy, initialDirection, initialMoveLeftEnemy, new List<Square>(jumpedOverPieces));
                }
                List<Square> initialMoveRightEnemy = ExistDiagonalMove(currentSquare, initialMove, 1, dy);
                if (initialMoveRightEnemy.Count > 0 && initialDirection >= 0)
                {
                    rightEnemy = RightEnemyMove(leftSquare, rightSquare, dy, initialDirection, initialMoveRightEnemy, new List<Square>(jumpedOverPieces));
                }

                if (initialDirection <= 0 && (!alreadyMoved || (!leftEnemy && !rightEnemy)))
                {
                    leftEmpty = LeftEmptyMove(leftSquare, rightSquare, dy, initialDirection, initialMove, new List<Square>(jumpedOverPieces));
                    if (IsEnemyOn(new Square(currentSquare, 1, -dy))) AddPossibleMove(initialMove, emptyAdd: true);
                }

                if (initialDirection >= 0 && (!alreadyMoved || (!leftEnemy && !rightEnemy)))
                {
                    rightEmpty = RightEmptyMove(leftSquare, rightSquare, dy, initialDirection, initialMove, new List<Square>(jumpedOverPieces));
                    if (IsEnemyOn(new Square(currentSquare, -1, -dy))) AddPossibleMove(initialMove, emptyAdd: true);
                }

                bool addedMove = leftEnemy || rightEnemy || leftEmpty || rightEmpty;
                if (!addedMove && alreadyMoved)
                {
                    AddPossibleMove(initialMove);
                }
            }
        }

        private void FindPossibleMovesRegular(
            Square currentSquare,
            List<Square> initialMove,
            List<Square> jumpedOverPieces,
            int dy,
            int initialDirection = 0,
            bool alreadyMoved = false)
        {
            Square leftSquare = new Square(currentSquare.X - 1, currentSquare.Y + dy);
            Square rightSquare = new Square(currentSquare.X + 1, currentSquare.Y + dy);

            bool leftEnemy = false;
            bool rightEnemy = false;
            bool leftEmpty = false;
            bool rightEmpty = false;

            if (InBoard(currentSquare))
            {
                leftEnemy = LeftEnemyMove(leftSquare, rightSquare, dy, initialDirection, initialMove, jumpedOverPieces);
                rightEnemy = RightEnemyMove(leftSquare, rightSquare, dy, initialDirection, initialMove, jumpedOverPieces);
                if (!alreadyMoved)
                {
                    leftEmpty = LeftEmptyMove(leftSquare, rightSquare, dy, initialDirection, initialMove, jumpedOverPieces);
                    rightEmpty = RightEmptyMove(leftSquare, rightSquare, dy, initialDirection, initialMove, jumpedOverPieces);
                }

                bool addedMove = leftEnemy || rightEnemy || leftEmpty || rightEmpty;
                if (!addedMove && alreadyMoved)
                {
                    AddPossibleMove(initialMove);
                }
            }
        }

        public void MoveToPosition(List<Square> toMove)
        {
            Square pieceSquare = toMove.First();
            Piece movingPiece = data.pieces[pieceSquare.Y, pieceSquare.X];
            Piece enemy = GetCurrentEnemy();

            foreach (Square square in toMove)
            {
                if (IsEnemyOn(square))
                {
                    if (data.currentPlayer == Piece.White)
                    {
                        data.statistics.WhiteScore++;
                    }
                    else
                    {
                        data.statistics.BlackScore++;
                    }
                }
                data.pieces[square.Y, square.X] = Piece.None;
            }

            pieceSquare = toMove.Last();
            data.pieces[pieceSquare.Y, pieceSquare.X] = movingPiece;
            if (pieceSquare.Y == 0)
            {
                data.pieces[pieceSquare.Y, pieceSquare.X] = Piece.Queen | data.currentPlayer;
            }
            data.UpdateHistory();
            UpdateGameStatus();
        }

        public bool IsEnemyOn(Square square)
        {
            if (!InBoard(square)) return false; 
            Piece enemy = GetCurrentEnemy();
            return (data.pieces[square.Y, square.X] & enemy) == enemy;
        }

        public bool IsCurrentPlayerOn(Square square)
        {
            if (!InBoard(square)) return false;
            return (data.pieces[square.Y, square.X] & data.currentPlayer) == data.currentPlayer;
        }

        public Piece GetCurrentEnemy()
        {
            return (data.currentPlayer == Piece.White ? Piece.Black : Piece.White);
        }

        private bool EnemyIsQueen(Square square)
        {
            if (!InBoard(square)) return false;
            Piece enemy = GetCurrentEnemy();
            return (data.pieces[square.Y, square.X] ^ enemy) == Piece.Queen;
        }

        private bool EnemyThatCanBeDefeated(Square square, int dx, int dy)
        {
            if (InBoard(square))
            {
                Square nextSquare = new Square(square, dx, dy);
                Piece enemy = GetCurrentEnemy();
                if ((data.pieces[square.Y, square.X] & enemy) == enemy)
                {
                    return Empty(nextSquare);
                }
            }
            return false;
        }

        public bool Empty(Square sqare)
        {
            return InBoard(sqare) && (data.pieces[sqare.Y, sqare.X] == Piece.None);
        }

        public bool InBoard(Square sqare)
        {
            return (-1 < sqare.X && sqare.X < 8) && (-1 < sqare.Y && sqare.Y < 8);
        }

        public void UpdateGameStatus()
        {
            data.chosenPiece = new Square();

            int whiteMoves = CountPlayerMoves(Piece.White).Count;
            int blackMoves = CountPlayerMoves(Piece.Black).Count;

            if ((whiteMoves == 0 && blackMoves == 0) || data.MaximumRepeatingRecordsInHistory())
            {
                data.status = GameStatus.Draw;
                return;
            }
            if (whiteMoves == 0)
            {
                data.status = GameStatus.BlackWin;
                return;
            }
            if (blackMoves == 0)
            {
                data.status = GameStatus.WhiteWin;
                return;
            }
            data.status = GameStatus.UpdateFinished;
        }
    }
}
