namespace CzechDraughts
{
    internal class Game
    {
        private Data data;
        private Graphics graphics;
        private InputHandler inputHandler;
        private Logic logic;

        public Game() 
        { 
            data = new Data();
            graphics = new Graphics(data);
            logic = new Logic(data);
            inputHandler = new InputHandler(data, graphics, logic);
        }

        public void Start()
        {
            new GameInitializer(data, graphics, inputHandler, logic);
            if (data.currentPlayer != Piece.White)
            {
                data.FlipBoardAndPieces();
                data.currentPlayer = Piece.Black;
            }

            while (true)
            {
                inputHandler.NextMove();
                if (data.status != GameStatus.UpdateFinished)
                {
                    ResolveEnd();
                    return;
                }
                data.status = GameStatus.InProgress;
                NextPlayerUpdate();
            }
        }

        public void NextPlayerUpdate()
        {
            data.FlipBoardAndPieces();
            if (data.currentPlayer == Piece.White)
            {
                data.statistics.Move++;
            }
        }

        public void ResolveEnd()
        {
            while (!Console.KeyAvailable)
            {
                graphics.RenderCurrentState(end: true, message: "*Press any key to end the game*");
                System.Threading.Thread.Sleep(500);
            }
        }
    }
}
