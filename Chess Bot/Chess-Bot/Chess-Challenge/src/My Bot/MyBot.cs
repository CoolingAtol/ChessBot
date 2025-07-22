// Enhanced: Minimax Bot with Alpha-Beta, Iterative Deepening, Move Ordering, Time Control, Eval Count, and Debug Display

using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    private const int MAX_DEPTH = 10;
    private readonly Dictionary<ulong, TranspositionEntry> transpositionTable = new();
    private Move bestMove;
    private int nodesSearched;
    private int currentDepth;
    private int currentEval;

    private static readonly Dictionary<PieceType, int> PieceValues = new()
    {
        { PieceType.Pawn, 100 },
        { PieceType.Knight, 320 },
        { PieceType.Bishop, 330 },
        { PieceType.Rook, 500 },
        { PieceType.Queen, 900 },
        { PieceType.King, 20000 }
    };

    private static readonly int[] PawnTable = new int[] {
        0, 5, 5, -10, -10, 5, 10, 0,
        0, 10, -5, 0, 0, -5, 10, 0,
        0, 5, 10, 20, 20, 10, 5, 0,
        5, 5, 5, 15, 15, 5, 5, 5,
        10, 10, 10, 20, 20, 10, 10, 10,
        20, 20, 20, 30, 30, 20, 20, 20,
        50, 50, 50, 50, 50, 50, 50, 50,
        0, 0, 0, 0, 0, 0, 0, 0
    };

    private static readonly int[] KnightTable = new int[] {
        -50, -40, -30, -30, -30, -30, -40, -50,
        -40, -20, 0, 5, 5, 0, -20, -40,
        -30, 5, 10, 15, 15, 10, 5, -30,
        -30, 0, 15, 20, 20, 15, 0, -30,
        -30, 5, 15, 20, 20, 15, 5, -30,
        -30, 0, 10, 15, 15, 10, 0, -30,
        -40, -20, 0, 0, 0, 0, -20, -40,
        -50, -40, -30, -30, -30, -30, -40, -50
    };

    private static int[] GetPieceSquareTable(PieceType type) => type switch
    {
        PieceType.Pawn => PawnTable,
        PieceType.Knight => KnightTable,
        _ => null
    };

    public Move Think(Board board, Timer timer)
    {
        bestMove = Move.NullMove;
        currentEval = int.MinValue;
        nodesSearched = 0;
        int depth = 1;
        int timeLimit = timer.MillisecondsRemaining / 30 + 50;
        int startTime = timer.MillisecondsElapsedThisTurn;

        while (depth <= MAX_DEPTH)
        {
            Move currentBest = Move.NullMove;
            int currentEvalTemp = int.MinValue;
            foreach (var move in OrderMoves(board.GetLegalMoves(), board))
            {
                board.MakeMove(move);
                int eval = -AlphaBeta(board, depth - 1, int.MinValue + 1, int.MaxValue - 1, timer);
                board.UndoMove(move);
                if (eval > currentEvalTemp)
                {
                    currentEvalTemp = eval;
                    currentBest = move;
                }
                if (timer.MillisecondsElapsedThisTurn - startTime > timeLimit)
                    break;
            }
            if (timer.MillisecondsElapsedThisTurn - startTime > timeLimit) break;
            bestMove = currentBest;
            currentEval = currentEvalTemp;
            currentDepth = depth;
            depth++;
        }

        Console.WriteLine($"info depth {currentDepth} nodes {nodesSearched} score cp {currentEval} bestmove {bestMove}");
        return bestMove;
    }

    private int AlphaBeta(Board board, int depth, int alpha, int beta, Timer timer)
    {
        if (depth == 0 || timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / 2)
            return Quiescence(board, alpha, beta);

        ulong key = board.ZobristKey;
        if (transpositionTable.TryGetValue(key, out var entry) && entry.Depth >= depth)
        {
            if (entry.Flag == 0) return entry.Value;
            if (entry.Flag == -1 && entry.Value <= alpha) return alpha;
            if (entry.Flag == 1 && entry.Value >= beta) return beta;
        }

        int bestValue = int.MinValue;
        Move bestMove = Move.NullMove;
        foreach (var move in OrderMoves(board.GetLegalMoves(), board))
        {
            board.MakeMove(move);
            int score = -AlphaBeta(board, depth - 1, -beta, -alpha, timer);
            board.UndoMove(move);
            nodesSearched++;

            if (score > bestValue)
            {
                bestValue = score;
                bestMove = move;
            }
            alpha = Math.Max(alpha, bestValue);
            if (alpha >= beta) break;
        }

        transpositionTable[key] = new TranspositionEntry
        {
            Value = bestValue,
            Depth = depth,
            Flag = bestValue >= beta ? 1 : bestValue <= alpha ? -1 : 0,
            BestMove = bestMove
        };

        return bestValue;
    }

    private int Quiescence(Board board, int alpha, int beta)
    {
        int eval = Evaluate(board);
        if (eval >= beta) return beta;
        if (eval > alpha) alpha = eval;

        foreach (var move in board.GetLegalMoves(true))
        {
            board.MakeMove(move);
            int score = -Quiescence(board, -beta, -alpha);
            board.UndoMove(move);
            nodesSearched++;
            if (score >= beta) return beta;
            if (score > alpha) alpha = score;
        }
        return alpha;
    }

    private IEnumerable<Move> OrderMoves(Move[] moves, Board board)
    {
        Array.Sort(moves, (a, b) => ScoreMove(b, board).CompareTo(ScoreMove(a, board)));
        return moves;
    }

    private int ScoreMove(Move move, Board board)
    {
        if (move.IsCapture)
        {
            Piece attacker = board.GetPiece(move.StartSquare);
            Piece target = board.GetPiece(move.TargetSquare);
            int targetVal = PieceValues.GetValueOrDefault(target.PieceType, 0);
            int attackerVal = PieceValues.GetValueOrDefault(attacker.PieceType, 1);
            return 10000 + targetVal - attackerVal;
        }
        if (move.IsPromotion) return 9000;
        if (move.IsCastles) return 500;
        return 0;
    }

    private int Evaluate(Board board)
    {
        int eval = 0;
        foreach (var list in board.GetAllPieceLists())
        {
            PieceType pt = list.TypeOfPieceInList;
            int value = PieceValues[pt];
            int[] table = GetPieceSquareTable(pt);
            foreach (var piece in list)
            {
                int idx = piece.IsWhite ? piece.Square.Index : 63 - piece.Square.Index;
                int positional = table != null ? table[idx] : 0;
                eval += (piece.IsWhite ? 1 : -1) * (value + positional);
            }
        }
        return eval;
    }

    private class TranspositionEntry
    {
        public int Value;
        public int Depth;
        public int Flag;
        public Move BestMove;
    }
}
