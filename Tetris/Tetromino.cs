using Microsoft.Xna.Framework;

using static Tetris.Debug;

namespace Tetris;

public class Tetromino {
    public bool[,] ShapeArray { get; private set; }
    public Color Color { get; }
    public TetrominoShape Shape { get; }
    
    // TODO: get rid of position and use Playfield class for it instead
    // TODO: set this to accomodate for PlayField.FieldOffset
    public int[] TopLeftPosition { get; private set; } = [0, 0]; // (row, col), in the visible grid

    private Direction _previousDirection = Direction.Down;
    private Direction _oppositeDirectionOfCollision;
    
    public Tetromino(TetrominoShape aShape, int aGridWidth) {
        Shape = aShape;
        
        // "draw" the tetromino w/ trues in a minimally size square array of falses
        switch (aShape) {
            case TetrominoShape.I:
                --TopLeftPosition[0]; // because top row of ShapeArray is empty (false)
                Color = Color.DeepSkyBlue;
                ShapeArray = new[,] {
                    { false, false, false, false },
                    { true,  true,  true,  true  },
                    { false, false, false, false },
                    { false, false, false, false }
                };
                break;
            case TetrominoShape.J:
                Color = Color.Blue;
                ShapeArray = new[,] {
                    { true,  false, false },
                    { true,  true,  true  },
                    { false, false, false }
                };
                break;
            case TetrominoShape.L:
                Color = Color.Orange;
                ShapeArray = new [,] {
                    { false, false, true  },
                    { true,  true,  true  },
                    { false, false, false }
                };
                break;
            case TetrominoShape.O:
                Color = Color.Yellow;
                ShapeArray = new[,] {
                    { true, true },
                    { true, true }
                };
                break;
            case TetrominoShape.S:
                Color = Color.LimeGreen;
                ShapeArray = new[,] {
                    { false, true,  true  },
                    { true,  true,  false },
                    { false, false, false }
                };
                break;
            case TetrominoShape.T:
                Color = Color.DarkViolet;
                ShapeArray = new[,] {
                    { false, true,  false },
                    { true,  true,  true  },
                    { false, false, false }
                };
                break;
            case TetrominoShape.Z:
                Color = Color.Red;
                ShapeArray = new[,] {
                    { true,  true,  false },
                    { false, true,  true  },
                    { false, false, false }
                };
                break;
            default:
                Color = Color.White;
                ShapeArray = new[,] { {false} };
                Log("Shape does not exist");
                break;
        }
        
        // set to center
        TopLeftPosition[1] += (aGridWidth - ShapeArray.GetLength(0)) / 2;
    }

    public int TopRow {
        get => TopLeftPosition[0];
        private set => TopLeftPosition[0] = value;
    }

    public int LeftCol {
        get => TopLeftPosition[1];
        private set => TopLeftPosition[1] = value;
    }

    // rotate 90 clockwise
    private void Rotate(bool aFailedToRotate) {
        var bufferArray = new bool[ShapeArray.GetLength(0), ShapeArray.GetLength(1)];
        for (var row = 0; row < bufferArray.GetLength(0); ++row) {
            for (var col = 0; col < bufferArray.GetLength(1); ++col) {
                bufferArray[col, bufferArray.GetLength(0) - row - 1] = ShapeArray[row, col];
            }
        }
        ShapeArray = bufferArray;
        
        PlayField.UpdateTetrominoLocation(this);
        if (PlayField.IsCollision() && !aFailedToRotate) Kick();
    }
    
    public void Rotate() { Rotate(false); }

    private void Kick(bool aTryAgain = false) {
        var numMoves = 1;
        if (aTryAgain) ++numMoves;
        
        // in this order, move down, left, right, and then up to avoid collision
        for (var i = 0; i < numMoves; ++i) Move(Direction.Down, false);
        if (PlayField.IsCollision()) {
            for (var i = 0; i < numMoves; ++i) Move(Direction.Up, false);
            for (var i = 0; i < numMoves; ++i) Move(Direction.Left, false);
        }
        if (PlayField.IsCollision()) {
            for (var i = 0; i < numMoves * 2; ++i) Move(Direction.Right, false);
        }
        if (PlayField.IsCollision()) {
            for (var i = 0; i < numMoves; ++i) Move(Direction.Left, false);
            for (var i = 0; i < numMoves; ++i) Move(Direction.Up, false);
        }
        // if there is still a collision after moving in all 4 directions,
        if (PlayField.IsCollision()) {
            // move back to original position
            for (var i = 0; i < numMoves; ++i) Move(Direction.Down, false);
            // if block is 4 long (i.e. I piece), kick again but w/ more 1 move in each direction
            if (ShapeArray.GetLength(0) == 4 && !aTryAgain) {
                Kick(true);
                return;
            }
            // rotate back to original position if all else fails
            for (var i = 0; i < 3; ++i) Rotate(true);
        }
    }

    private void StopCollision() {
        _oppositeDirectionOfCollision = _previousDirection switch {
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            _ => _oppositeDirectionOfCollision
        };

        // keep moving away from collision until there is no more collision
        while (PlayField.IsCollision()) {
            Move(_oppositeDirectionOfCollision, true);
        }
    }

    public void Move(Direction aDirection, bool aCheckForCollision) {
        // move in given direction
        switch (aDirection) {
            case Direction.Left:
                --LeftCol;
                break;
            case Direction.Right:
                ++LeftCol;
                break;
            case Direction.Up:
                --TopRow;
                break;
            case Direction.Down:
                ++TopRow;
                break;
            default:
                Log("bruh you must be an idiot. can't move like that lol");
                break;
        }
        
        // update previous direction for collision prevention
        _previousDirection = aDirection;
        
        PlayField.UpdateTetrominoLocation(this);
        // check for and prevent collision if caller wants
        if (aCheckForCollision && PlayField.IsCollision()) StopCollision();
    }
    
}