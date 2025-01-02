using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using static Tetris.Debug;

namespace Tetris;

public class Game1 : Game {
    private readonly Random _rand = new Random();
    
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _texture;
    
    private const double WindowScale = 0.5; // scale factor of monitor height

    private const double Fps = 30.0;
    private int _frameCounter;
    private bool _placeNextFrame;

    private const int BackgroundColorVal = 16; // dark gray
    private const int GridColorVal       = 64; // dark, but lighter, gray
    private readonly Color _backgroundColor = new Color(BackgroundColorVal, BackgroundColorVal, BackgroundColorVal);
    private readonly Color _gridColor       = new Color(GridColorVal, GridColorVal, GridColorVal);
    
    // TODO: move to PlayField?
    private const int GridWidth  = 10; // num of cells
    private const int GridHeight = 20; // num of cells
    private int _cellSize; // determined by monitor height
    private int _gridXOffset; // how far left border of grid is from left window edge
    private int _gridYOffset; // how far top border of grid is from top window edge

    private Tetromino _tetromino;
    private Tetromino[] _tetrominoBag1;
    private Tetromino[] _tetrominoBag2;
    private readonly int _numTetrominoes = Enum.GetNames(typeof(TetrominoShape)).Length;
    private int _currentTetrominoIndex;

    private KeyboardState _currentKeyboardState;
    private KeyboardState _previousKeyboardState;

#if DEBUG
    private bool _fallingDisabled;
#endif
    
    // CONSTRUCTOR
    public Game1() {
        Content.RootDirectory = "Content";
        
        _graphics = new GraphicsDeviceManager(this);
        IsMouseVisible = true;
        Window.AllowUserResizing = false;
        InitializeWindowSize();
    }

/*
 *  CUSTOM METHODS
 */
    // NEW TETROMINO BAG
    private Tetromino[] NewTetrominoBag() {
        var aTetrominoBag = new Tetromino[_numTetrominoes];
        var uniqueIndices = new int[_numTetrominoes];
        for (var i = 0; i < _numTetrominoes; ++i)
            uniqueIndices[i] = -1;

        // TODO: change to get rid of uniqueIndices 
        for (var i = 0; i < _numTetrominoes; ++i) {
            int randInt;
            do {
                randInt = _rand.Next(0, _numTetrominoes);
            } while (Array.Exists(uniqueIndices, n => n == randInt));
            uniqueIndices[i] = randInt;
            aTetrominoBag[i] = new Tetromino((TetrominoShape)randInt, GridWidth);
        }

        return aTetrominoBag;
    }
    // NEW RANDOM TETROMINO
    private void NewTetromino() {
        // update the current tetromino
        _tetromino = _tetrominoBag1[_currentTetrominoIndex];
        
        // log current and next tetromino
        Log($"Current Tetromino: {_tetrominoBag1[_currentTetrominoIndex].Shape.ToString()} || ");
        try {
            Log($"Next Tetromino: {_tetrominoBag1[_currentTetrominoIndex + 1].Shape.ToString()}");
        } catch (IndexOutOfRangeException) {
            //Log($"Exception: {e}");
            Log($"Next Tetromino: {_tetrominoBag2[0].Shape.ToString()}");
        }
        LogLine();
        
        ++_currentTetrominoIndex;
        // when bag 1 is used up, swap bags and refill bag 2
        if (_currentTetrominoIndex == _numTetrominoes) {
            _currentTetrominoIndex = 0;
            _tetrominoBag1 = _tetrominoBag2;
            _tetrominoBag2 = NewTetrominoBag();
        }

        PlayField.UpdateTetrominoLocation(_tetromino);
        
        // if there is spawn-collision, the game is over
        if (!PlayField.IsCollision()) return;
        LogLine("Game Over");
        Exit();
    }
    
#if DEBUG
    private void NewTetrominoDebug(TetrominoShape aShape) {
        _tetromino = new Tetromino(aShape, GridWidth);
        PlayField.UpdateTetrominoLocation(_tetromino);
        if (!PlayField.IsCollision()) return;
        LogLine("Game Over");
        Exit();
    }
#endif

    // INITIALIZE WINDOW SIZE
    private void InitializeWindowSize() {
        // ApplyChanges() called to initiate the window in the current monitor so the program
        // can get the size of that monitor. otherwise, the wrong monitor size may be grabbed
        _graphics.ApplyChanges();
        LogLine($"Monitor Width: {GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width}");
        LogLine($"Monitor Height: {GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height}");

        // square frame based off of monitor height and predetermined scale
        _graphics.PreferredBackBufferWidth  = (int)(WindowScale * GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
        _graphics.PreferredBackBufferHeight = _graphics.PreferredBackBufferWidth;
        
        _graphics.ApplyChanges();
    }

    // IS KEY JUST PRESSED
    // used to prevent an action from repeating if key is held down
    private bool IsKeyJustPressed(Keys aKey) {
        return _currentKeyboardState.IsKeyDown(aKey) && _currentKeyboardState != _previousKeyboardState;
    }

/*
 *  INITIALIZING METHODS
 */
    // INITIALIZE
    protected override void Initialize() {
        // set constant framerate ??
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / Fps);

        // cell size is 1/48 of the monitor height. keeps grid in the window 
        _cellSize = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 48;
        LogLine($"Cell Size: {_cellSize}");
        
        PlayField.Initialize(GridHeight, GridWidth);

        _tetrominoBag1 = NewTetrominoBag();
        _tetrominoBag2 = NewTetrominoBag();
        
        NewTetromino();
        
        // call parent's Initialize()
        base.Initialize();
    }

    // LOAD CONTENT
    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // used for all textures
        _texture = new Texture2D(GraphicsDevice, 1, 1);
        _texture.SetData([Color.White]);
        
    }

/*
 *  LOOPED METHODS
 */
    // UPDATE
    protected override void Update(GameTime gameTime) {

        _currentKeyboardState = Keyboard.GetState();
        
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (IsKeyJustPressed(Keys.A))
            _tetromino.Move(Direction.Left, true);

        if (IsKeyJustPressed(Keys.D))
            _tetromino.Move(Direction.Right, true);

        if (IsKeyJustPressed(Keys.Space))
            _tetromino.Rotate();

        if (_currentKeyboardState.IsKeyDown(Keys.LeftShift))
            _tetromino.Move(Direction.Down, true);

        // developer keymaps
#if DEBUG
        if (IsKeyJustPressed(Keys.S))
            _tetromino.Move(Direction.Down, true);
        if (IsKeyJustPressed(Keys.W))
            _tetromino.Move(Direction.Up, true);
        if (IsKeyJustPressed(Keys.Q)) {
            PlayField.Place(_tetromino);
            NewTetromino();
        }
        if (IsKeyJustPressed(Keys.E))
            PrintBoolField(PlayField.CurrentTetrominoField);
        if (IsKeyJustPressed(Keys.G))
            PrintBoolField(PlayField.BlockField);
        if (IsKeyJustPressed(Keys.F)) {
            _fallingDisabled = !_fallingDisabled;
            LogLine($"Falling: {!_fallingDisabled}");
        }
        if (IsKeyJustPressed(Keys.OemQuestion)) {
            if (IsKeyJustPressed(Keys.I))
                NewTetrominoDebug(TetrominoShape.I);
            if (IsKeyJustPressed(Keys.J))
                NewTetrominoDebug(TetrominoShape.J);
            if (IsKeyJustPressed(Keys.L))
                NewTetrominoDebug(TetrominoShape.L);
            if (IsKeyJustPressed(Keys.O))
                NewTetrominoDebug(TetrominoShape.O);
            if (IsKeyJustPressed(Keys.S))
                NewTetrominoDebug(TetrominoShape.S);
            if (IsKeyJustPressed(Keys.T))
                NewTetrominoDebug(TetrominoShape.T);
            if (IsKeyJustPressed(Keys.Z))
                NewTetrominoDebug(TetrominoShape.Z);
        }
#endif

        _previousKeyboardState = _currentKeyboardState;

        PlayField.ClearAnyLines();
        
#if DEBUG
        if (_fallingDisabled) goto afterFall;
#endif
        
        const int level = 1;
        if (++_frameCounter % (Fps / level) == 0) {
            _tetromino.Move(Direction.Down, true);
            if (PlayField.IsDropped() && _placeNextFrame == false) {
                _placeNextFrame = true;
                goto afterFall;
            }
            if (_placeNextFrame) {
                PlayField.Place(_tetromino);
                NewTetromino();
                _placeNextFrame = false;
            }
        }
        afterFall:

        base.Update(gameTime);
    }

    // DRAW
    protected override void Draw(GameTime gameTime) {
        // clear screen
        GraphicsDevice.Clear(_backgroundColor);

        // begin drawing
        _spriteBatch.Begin();
        
        // draw the playfield
        for (var row = 0; row < PlayField.BlockField.GetLength(0); ++row) {
            for (var col = 0; col < PlayField.BlockField.GetLength(1); ++col) {
                // go to next iteration if the tetromino does not occupy current cell
                if (PlayField.BlockField[row, col] == false) continue;
                
                var xPos = (col - PlayField.FieldOffset) * _cellSize + _gridXOffset;
                var yPos = (row - PlayField.FieldOffset) * _cellSize + _gridYOffset;
                
                // draw the cell
                _spriteBatch.Draw(_texture, 
                                  new Rectangle(xPos, yPos, _cellSize, _cellSize),
                                  PlayField.ColorField[row,col]);
            }
        }
        
        // draw the tetromino
        for (var col = 0; col < _tetromino.ShapeArray.GetLength(0); ++col) {
            for (var row = 0; row < _tetromino.ShapeArray.GetLength(1); ++row) {
                // go to next iteration if the tetromino does not occupy current cell
                if (!_tetromino.ShapeArray[row, col]) continue;
                
                var xPos = (col + _tetromino.LeftCol) * _cellSize + _gridXOffset;
                var yPos = (row + _tetromino.TopRow) * _cellSize + _gridYOffset;
                
                // draw the cell
                _spriteBatch.Draw(_texture, 
                                  new Rectangle(xPos, yPos, _cellSize, _cellSize),
                                  _tetromino.Color);
            }
        }
        
        // draw grid at center of screen atop the tetrominoes
        _gridXOffset = (GraphicsDevice.Viewport.Width - GridWidth*_cellSize) / 2;
        _gridYOffset = (GraphicsDevice.Viewport.Height - GridHeight*_cellSize) / 2;
        for (var x = 0; x <= GridWidth; x++) { // vertical lines
            var xPos = x * _cellSize + _gridXOffset;
            var yPos = _gridYOffset;
            _spriteBatch.Draw(_texture, new Rectangle(xPos, yPos, 1, GridHeight * _cellSize), _gridColor);
        }
        for (var y = 0; y <= GridHeight; y++) { // horizontal lines
            var xPos = _gridXOffset;
            var yPos = y * _cellSize + _gridYOffset;
            _spriteBatch.Draw(_texture, new Rectangle(xPos, yPos, GridWidth * _cellSize, 1), _gridColor);
        }
        
        // finalize drawing
        _spriteBatch.End();
        
        // calls base class's draw() method
        base.Draw(gameTime);
    }
}