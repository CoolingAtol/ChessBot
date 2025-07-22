using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    // Piece-square tables as static readonly arrays
    private static readonly int[] pawnTable = {
        5, 5, 0, 0, 0, 0, 5, 5,
        15, 15, 10, 10, 10, 10, 15, 15,
        5, 10, 5, 5, 5, 5, 10, 5,
        0, 0, -10, -5, -5, -10, 0, 0,
        -20, -15, -10, 10, 10, -10, -15, -20,
        10, -30, -20, -20, -20, -20, -30, 10,
        -5, 5, 5, 5, 5, 5, 5, -5,
        0, 0, 0, 0, 0, 0, 0, 0,
    };

    private static readonly int[] knightTable = {
        -10, 0, 0, 5, 5, 0, 0, -10,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 5, 5, 5, 5, 0, -5,
        -5, 0, 0, 5, 5, 0, 0, -5,
        -5, 0, 20, 0, 0, 20, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -15, -20, -10, -10, -10, -10, -20, -15,
    };

    private static readonly int[] bishopTable = {
        -10, -5, 0, 0, 0, 0, -5, -10,
        -5, -5, 0, 0, 0, 0, -5, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 5, 0, 0, 0, 0, 5, 0,
        5, 0, 5, 0, 0, 5, 0, 5,
        0, 0, 0, -20, -20, 0, 0, 0,
        -10, -10, -10, -20, -20, -10, -10, -10,
    };

    private static readonly int[] rookTable = {
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, -5, -5, -5, -5, -5, -5, 0,
        0, 0, 0, -5, -5, 0, 0, 0,
        0, 0, 0, -5, -5, 0, 0, 0,
        0, 0, 0, -5, -5, 0, 0, 0,
        0, 0, 0, -5, -5, 0, 0, 0,
        -25, 0, 0, 10, 0, 10, 0, -25,
    };

    private static readonly int[] queenTable = {
        0, 10, 10, 5, 5, 10, 10, 0,
        5, 5, 0, 0, 0, 0, 5, 5,
        0, 5, 0, 0, 0, 0, 5, 0,
        -5, 0, 0, 0, 0, 0, 0, -5,
        0, 0, 0, 0, 0, 0, 5, -5,
        -10, 5, 0, 0, 0, 5, 0, -10,
        0, 0, -5, 0, -5, 0, 0, 0,
        -10, -10, -10, -5, -10, -10, -10, -10,
    };

    private static readonly int[] kingTable = {
        -30, -10, -5, -5, -5, -5, -5, -30,
        -20, -10, 0, 0, 0, 0, -10, -20,
        -10, 0, 0, 0, 0, 0, 0, -10,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, -10, -10, -10, -10, 0, 0,
        -5, -5, -10, -10, -10, -10, -5, -5,
        -10, -10, -10, -10, -10, -10, -10, -10,
        0, 5, 10, -30, -15, -20, 30, 5,
    };

    public Move Think(Board board, Timer timer)
    {
        Move bestMove = Move.NullMove;
        int bestEval = int.MinValue;
        int depth = 4; // You can adjust the depth here for strength vs speed

        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int eval = -AlphaBeta(board, depth - 1, int.MinValue + 1, int.MaxValue - 1);
            board.UndoMove(move);

            Console.WriteLine($"Move: {move}, Eval: {eval}");

            if (eval > bestEval)
            {
                bestEval = eval;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private int AlphaBeta(Board board, int depth, int alpha, int beta)
    {
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw())
            return Evaluate(board);

        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int eval = -AlphaBeta(board, depth - 1, -beta, -alpha);
            board.UndoMove(move);

            if (eval >= beta)
                return beta;

            alpha = Math.Max(alpha, eval);
        }

        return alpha;
    }

    private int Evaluate(Board board)
    {
        int eval = 0;

        foreach (var pieceList in board.GetAllPieceLists())
        {
            bool isWhite = pieceList.IsWhitePieceList;

            foreach (var piece in pieceList)
            {
                int baseValue = piece.PieceType switch
                {
                    PieceType.Pawn => 100,
                    PieceType.Knight => 300,
                    PieceType.Bishop => 320,
                    PieceType.Rook => 500,
                    PieceType.Queen => 900,
                    PieceType.King => 10000,
                    _ => 0,
                };

                int pstValue = piece.PieceType switch
                {
                    PieceType.Pawn => pawnTable[piece.Square.Index],
                    PieceType.Knight => knightTable[piece.Square.Index],
                    PieceType.Bishop => bishopTable[piece.Square.Index],
                    PieceType.Rook => rookTable[piece.Square.Index],
                    PieceType.Queen => queenTable[piece.Square.Index],
                    PieceType.King => kingTable[piece.Square.Index],
                    _ => 0,
                };

                int pieceScore = baseValue + pstValue;

                eval += isWhite ? pieceScore : -pieceScore;
            }
        }

        // Evaluate from the perspective of the side to move (negamax)
        return board.IsWhiteToMove ? eval : -eval;
    }
}