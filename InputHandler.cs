namespace CzechDraughts
{
    internal class InputHandler
    {
        private Data data;
        private Graphics graphics;
        private Logic logicResolver;

        public InputHandler(Data data, Graphics graphics, Logic logicResolver) 
        { 
            this.data = data;
            this.graphics = graphics;
            this.logicResolver = logicResolver;
        }

        public void NextMove()
        {
            Square currentSquare = new Square(4, 4);
            List<List<Square>>? possibleMoves = null;
            int moveIndex = 0;
            string message = string.Empty;
            int messageTimer = 0;

            while (data.status == GameStatus.InProgress)
            {
                graphics.RenderCurrentState(message: message, currentSquare: currentSquare, possibleMoves: possibleMoves, moveIndex: moveIndex);

                if (messageTimer == 4) message = string.Empty;
                if (message.Length == 0) messageTimer = 0;

                ConsoleKeyInfo keyInput = Console.ReadKey();
                ProcessKeyInput(keyInput, ref message, ref possibleMoves, ref moveIndex, ref currentSquare);
                messageTimer++;
            }
        }

        private void ProcessKeyInput(ConsoleKeyInfo keyInput, ref string message, ref List<List<Square>>? possibleMoves, ref int moveIndex, ref Square currentSquare)
        {
            switch (keyInput.Key)
            {
                case ConsoleKey.Enter:
                    HandleEnterKey(ref message, ref possibleMoves, ref moveIndex, currentSquare);
                    break;
                case ConsoleKey.UpArrow:
                case ConsoleKey.DownArrow:
                case ConsoleKey.RightArrow:
                case ConsoleKey.LeftArrow:
                    HandleArrowKey(keyInput.Key, ref possibleMoves, ref moveIndex, ref currentSquare);
                    break;
                case ConsoleKey.D:
                    ResetNextMoveSelectionData(ref possibleMoves, ref moveIndex);
                    HandleDrawRequest();
                    break;
                case ConsoleKey.G:
                    ResetNextMoveSelectionData(ref possibleMoves, ref moveIndex);
                    HandleGiveUp();
                    break;
                case ConsoleKey.R:
                    ResetNextMoveSelectionData(ref possibleMoves, ref moveIndex);
                    message = "You reset your chosen piece";
                    break;
                case ConsoleKey.M:
                    message = "Showing all possible end positions (moves) for this piece";
                    graphics.RenderCurrentState(message: message, possibleMoves: logicResolver.PossibleMoves(currentSquare));
                    Console.ReadKey();
                    break;
                case ConsoleKey.H:
                    HandleHelp();
                    break;
                default:
                    break;
            }
        }

        private void HandleArrowKey(ConsoleKey key, ref List<List<Square>>? possibleMoves, ref int moveIndex, ref Square currentSquare)
        {
            if (possibleMoves == null)
            {
                MoveCurrentSquare(key, ref currentSquare);
                return;
            }
            SelectMove(key, ref moveIndex, possibleMoves);
        }


        private void ResetNextMoveSelectionData(ref List<List<Square>>? possibleMoves, ref int moveIndex)
        {
            data.chosenPiece = new Square();
            possibleMoves = null;
            moveIndex = 0;
        }

        private void HandleEnterKey(ref string message, ref List<List<Square>>? possibleMoves, ref int moveIndex, Square currentSquare)
        {
            if (possibleMoves != null)
            {
                logicResolver.MoveToPosition(possibleMoves[moveIndex]);
                return;
            }

            if (!logicResolver.IsCurrentPlayerOn(currentSquare))
            {
                message = "Square does not contain your piece";
                return;
            }

            possibleMoves = logicResolver.PossibleMoves(currentSquare);

            if (possibleMoves.Count == 0)
            {
                possibleMoves = null;
                message = "This piece cannot move :(";
                return;
            }

            data.chosenPiece = currentSquare;
            message = "You chose a square";
        }

        public void MoveCurrentSquare(ConsoleKey key, ref Square currentSquare)
        {
            switch (key)
            {
                case ConsoleKey.UpArrow:    currentSquare.Y = (currentSquare.Y - 1 + 8) % 8; break;
                case ConsoleKey.DownArrow:  currentSquare.Y = (currentSquare.Y + 1 + 8) % 8; break;
                case ConsoleKey.RightArrow: currentSquare.X = (currentSquare.X + 1 + 8) % 8; break;
                case ConsoleKey.LeftArrow:  currentSquare.X = (currentSquare.X - 1 + 8) % 8; break;
                default: break;
            }
        }

        private void SelectMove(ConsoleKey key, ref int moveIndex, List<List<Square>> possibleMoves)
        {
            int possibleMovesCount = possibleMoves.Count;
            switch (key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.RightArrow:
                    moveIndex = (moveIndex + 1 + possibleMovesCount) % possibleMovesCount;
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.LeftArrow:
                    moveIndex = (moveIndex - 1 + possibleMovesCount) % possibleMovesCount;
                    break;
                default: break;
            }
        }

        private void HandleDecision(string message, Action onYes, Action onNo)
        {
            string warningMessage = "Please use Y or N\n";
            graphics.RenderCurrentState(message: message);

            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey();
                switch (info.Key)
                {
                    case ConsoleKey.Y:
                        onYes();
                        return;
                    case ConsoleKey.N:
                        onNo();
                        return;
                    default:
                        graphics.RenderCurrentState(message: warningMessage + message);
                        break;
                }
            }
        }

        private void HandleDrawRequest()
        {
            data.FlipBoardAndPieces();

            string message = "Player " + logicResolver.GetCurrentEnemy() + " suggests a draw. Accept ? (Y/N)";
            HandleDecision(message,
                () => data.status = GameStatus.Draw,
                () => { data.FlipBoardAndPieces(); NextMove(); });
        }

        private void HandleGiveUp()
        {
            string message = "Player " + data.currentPlayer + ", are you sure u want to give up ? (Y/N)";
            HandleDecision(message,
                () => data.status = (data.currentPlayer == Piece.White ? GameStatus.BlackWin : GameStatus.WhiteWin),
                () => NextMove());
        }

        public void HandleHelp()
        {
            graphics.RenderCurrentState(message: "*Press any key to return back to game*", help: true);
            Console.ReadKey();
            return;
        }
    }
}
