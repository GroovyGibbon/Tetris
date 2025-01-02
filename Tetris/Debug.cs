using System;
using Microsoft.Xna.Framework;

namespace Tetris;

public static class Debug {
    public static void Log<T>(T message) {
        #if DEBUG
        Console.Write(message);
        #endif
    }

    public static void LogLine<T>(T message) {
        #if DEBUG
        Console.WriteLine(message);
        #endif
    }

    public static void LogLine() { LogLine(""); }
    
    public static void PrintBoolField(bool[,] aField) {
    #if DEBUG
        for (var row = 0; row < aField.GetLength(0); ++row) {
            for (var col = 0; col < aField.GetLength(1); ++col) {
                Console.Write($"{(aField[row, col] ? 'X' : '-')} ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    #endif
    }
    
    public static void PrintColorField(Color[,] aField) {
    #if DEBUG
        for (var row = 0; row < aField.GetLength(0); ++row) {
            for (var col = 0; col < aField.GetLength(1); ++col) {
                Console.Write($"{aField[row, col]} ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    #endif
    }

}