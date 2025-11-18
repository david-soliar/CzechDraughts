namespace CzechDraughts
{
    internal class GameInitializer
    {
        private readonly Data data;
        public Graphics graphics;
        public InputHandler inputHandler;
        public Logic logic;
        private int type;

        public GameInitializer(Data data, Graphics graphics, InputHandler inputHandler, Logic logic)
        {
            this.data = data;
            this.graphics = graphics;
            this.inputHandler = inputHandler;
            this.logic = logic;

            type = 0;
            if (Start() == 0) return;
            data.NonifyPiecesOnBoard();
            CreateCustomBoard();
        }

        private int Start()
        {
            while (true)
            {
                graphics.InitialScreen(type);

                ConsoleKeyInfo keyInput = Console.ReadKey();
                switch (keyInput.Key) 
                {
                    case ConsoleKey.Enter:
                        return type;
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.DownArrow:
                        type = (type - 1 + 2) % 2;
                        break;
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.UpArrow:
                        type = (type + 1 + 2) % 2;
                        break;
                    default:
                        break;
                }
            }
        }

        private void CreateCustomBoard()
        {
            Square currentSquare = new Square(4, 4);

            while (true)
            {
                graphics.RenderCurrentState(currentSquare, creative: true);
                data.status = GameStatus.InProgress;
                ConsoleKeyInfo keyInput = Console.ReadKey();
                switch (keyInput.Key)
                {
                    case ConsoleKey.Q:
                    case ConsoleKey.Escape:
                        PrepareToFinishBoardSettingUp();
                        if (data.status != GameStatus.UpdateFinished)
                        {
                            graphics.RenderCurrentState(currentSquare, creative: true, message: "Invalid board layout");
                            data.status = GameStatus.InProgress;
                            Console.ReadKey();
                            break;
                        }
                        HandleFinishedBoard();
                        return;
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.LeftArrow:
                        inputHandler.MoveCurrentSquare(keyInput.Key, ref currentSquare);
                        break;
                    case ConsoleKey.D0:
                    case ConsoleKey.NumPad0:
                        data.pieces[currentSquare.Y, currentSquare.X] = Piece.None;
                        break;
                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        data.pieces[currentSquare.Y, currentSquare.X] = Piece.BlackRegular;
                        break;
                    case ConsoleKey.D6:
                    case ConsoleKey.NumPad6:
                        data.pieces[currentSquare.Y, currentSquare.X] = Piece.BlackQueen;
                        break;
                    case ConsoleKey.D8:
                    case ConsoleKey.NumPad8:
                        data.pieces[currentSquare.Y, currentSquare.X] = Piece.WhiteRegular;
                        break;
                    case ConsoleKey.D9:
                    case ConsoleKey.NumPad9:
                        data.pieces[currentSquare.Y, currentSquare.X] = Piece.WhiteQueen;
                        break;
                    case ConsoleKey.W:
                        data.currentPlayer = Piece.White;
                        break;
                    case ConsoleKey.B:
                        data.currentPlayer = Piece.Black;
                        break;
                    default:
                        break;
                }
            }
        }
        

        private void HandleFinishedBoard()
        {
            string message = "Did you finish setting up your board? Do you wish to start the game (Y/N)";
            string warningMessage = "Please use Y or N\n";
            graphics.RenderCurrentState(message: message, creative: true);

            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey();
                switch (info.Key)
                {
                    case System.ConsoleKey.Y:
                        data.status = GameStatus.InProgress;
                        return;
                    case System.ConsoleKey.N:
                        CreateCustomBoard();
                        return;
                    default:
                        graphics.RenderCurrentState(message: warningMessage + message, creative: true);
                        break;
                }
            }
        }

        private void PrepareToFinishBoardSettingUp()
        {
            if (data.currentPlayer == Piece.Black)
            {
                data.FlipBoardAndPieces();
                data.currentPlayer = Piece.Black;
            }
            logic.UpdateGameStatus();
        }
    }
}
