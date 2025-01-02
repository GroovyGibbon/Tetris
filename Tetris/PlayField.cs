using Microsoft.Xna.Framework;

//using static Tetris.Debug;

namespace Tetris;

public static class PlayField {
    public static bool[,] BlockField { get; private set; }
    public static bool[,] CurrentTetrominoField { get; private set;  }
    public static Color[,] ColorField { get; private set; }

    public const int FieldOffset = 4; // field has 4 extra rows/cols on each side
    
    // TODO: create block class/struct that has bool and Color types and replace BlockField & ColorField w/ one object

    public static void Initialize(int aGridHeight, int aGridWidth) {
        var fieldHeight = aGridHeight + 2 * FieldOffset;
        var fieldWidth  = aGridWidth  + 2 * FieldOffset;
        BlockField = new bool[fieldHeight, fieldWidth];
        CurrentTetrominoField = new bool[fieldHeight, fieldWidth];
        ColorField = new Color[fieldHeight, fieldWidth];

        for (var row = 0; row < BlockField.GetLength(0); ++row) {
            for (var col = 0; col < BlockField.GetLength(1); ++col) {
                if ( (row < FieldOffset || row >= fieldHeight - FieldOffset) 
                  || (col < FieldOffset || col >= fieldWidth - FieldOffset) ) {
                    BlockField[row, col] = true;
                }
            }
        }
        
    }

    public static void Place(Tetromino aTetromino) {
        var topRow = aTetromino.TopLeftPosition[0] + FieldOffset;
        var leftCol = aTetromino.TopLeftPosition[1] + FieldOffset;
        
        var numRowsInShape = aTetromino.ShapeArray.GetLength(0);
        var numColsInShape = aTetromino.ShapeArray.GetLength(1);

        // iterate through the shape array and keep record of the placed tetrimino's
        // position and color
        for (var row = 0; row < numRowsInShape; ++row) {
            for (var col = 0; col < numColsInShape; ++col) {
                if (aTetromino.ShapeArray[row, col] == false) continue;
                BlockField[row + topRow, col + leftCol] = true;
                ColorField[row + topRow, col + leftCol] = aTetromino.Color;
            }
        }
    }

    public static void UpdateTetrominoLocation(Tetromino aTetromino) {
        // clear the field
        for (var row = 0; row < CurrentTetrominoField.GetLength(0); ++row) {
            for (var col = 0; col < CurrentTetrominoField.GetLength(1); ++col) {
                CurrentTetrominoField[row, col] = false;
            }
        }
        
        var topRow = aTetromino.TopLeftPosition[0] + FieldOffset;
        var leftCol = aTetromino.TopLeftPosition[1] + FieldOffset;
        
        var numRowsInShape = aTetromino.ShapeArray.GetLength(0);
        var numColsInShape = aTetromino.ShapeArray.GetLength(1);
        
        // place the tetromino in the proper location in the field
        for (var row = 0; row < numRowsInShape; ++row) {
            for (var col = 0; col < numColsInShape; ++col) {
                if (aTetromino.ShapeArray[row, col] == false) continue;
                CurrentTetrominoField[row + topRow, col + leftCol] = true;
            }
        }
    }

    public static void ClearAnyLines() {
        var blockCounter = 0;
        var lineCounter = 0;
        var rowsOfLines = new int[4]; // max num of full lines is 4
        // TODO: this loop doesnt work. change to for loop
        foreach (var i in rowsOfLines)
            rowsOfLines[i] = -1;

        // find all rows containing a completed line
        for (var row = FieldOffset; row < BlockField.GetLength(0) - FieldOffset; ++row) {
            for (var col = 0; col < BlockField.GetLength(1); ++col) {
                if (BlockField[row, col] == false) break;
                ++blockCounter;
            }
            // if the blocks span an entire row, add that row to rowsOfLines
            if (blockCounter == BlockField.GetLength(1)) {
                rowsOfLines[lineCounter] = row;
                ++lineCounter;
            }
            blockCounter = 0;
        }

        // remove the rows of the completed lines
        foreach (var rowOfLine in rowsOfLines) {
            if (rowOfLine == -1) break;
            for (var col = FieldOffset; col < BlockField.GetLength(1) - FieldOffset; ++col) {
                BlockField[rowOfLine, col] = false;
            }
            for (var row = rowOfLine; row >= FieldOffset; --row) {
                for (var col = FieldOffset; col < BlockField.GetLength(1) - FieldOffset; ++col) {
                    if (row == FieldOffset) {
                        BlockField[row, col] = false;
                        continue;
                    }
                    BlockField[row, col] = BlockField[row - 1, col];
                }
            }
        }
    }

    public static bool IsCollision() {
        for (var row = 0; row < BlockField.GetLength(0); ++row) {
            for (var col = 0; col < BlockField.GetLength(1); ++col) {
                // if there is one or no blocks at that location, continue
                if (!BlockField[row, col] || !CurrentTetrominoField[row, col]) continue;
                // if there are 2 blocks, there is a collision
                return true;
            }
        }
        return false;
    }

    public static bool IsDropped() {
        // return true if there is a block/ground underneath any part of tetromino
        for (var row = CurrentTetrominoField.GetLength(0) - FieldOffset - 1; row >= FieldOffset; --row) {
            for (var col = FieldOffset; col < CurrentTetrominoField.GetLength(1) - FieldOffset; ++col) {
                // if there is tetromino at this location and there is a block beneath it
                if (CurrentTetrominoField[row, col] && BlockField[row + 1, col]) return true;
            }
        }
        return false;
    }
}