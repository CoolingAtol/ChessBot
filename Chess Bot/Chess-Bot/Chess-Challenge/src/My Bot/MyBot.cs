// using System.Security.Cryptography.X509Certificates;
using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        int[] pawnTable = {
            5, 5, 0, 0, 0, 0, 5, 5,
            15, 15, 10, 10, 10, 10, 15, 15,
            5, 10, 5, 5, 5, 5, 10, 5,
            0, 0, -10, -5, -5, -10, 0, 0,
            -20, -15, -10, 10, 10, -10, -15, -20,
            10, -30, -20, -20, -20, -20, -30, 10,
            -5, 5, 5, 5, 5, 5, 5, -5,
            0, 0, 0, 0, 0, 0, 0, 0,
        };

        int[] knightTable = {
            -10, 0, 0, 5, 5, 0, 0, -10,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 5, 5, 5, 5, 0, -5,
            -5, 0, 0, 5, 5, 0, 0, -5,
            -5, 0, 20, 0, 0, 20, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -15, -20, -10, -10, -10, -10, -20, -15,
        };

        int[] bishopTable = {
            -10, -5, 0, 0, 0, 0, -5, -10,
            -5, -5, 0, 0, 0, 0, -5, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 5, 0, 0, 0, 0, 5, 0,
            5, 0, 5, 0, 0, 5, 0, 5,
            0, 0, 0, -20, -20, 0, 0, 0,
            -10, -10, -10, -20, -20, -10, -10, -10,
        };

        int[] rookTable = {
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, -5, -5, -5, -5, -5, -5, 0,
            0, 0, 0, -5, -5, 0, 0, 0,
            0, 0, 0, -5, -5, 0, 0, 0,
            0, 0, 0, -5, -5, 0, 0, 0,
            0, 0, 0, -5, -5, 0, 0, 0,
            -25, 0, 0, 10, 0, 10, 0, -25,
        };

        int[] queenTable = {
            0, 10, 10, 5, 5, 10, 10, 0,
            5, 5, 0, 0, 0, 0, 5, 5,
            0, 5, 0, 0, 0, 0, 5, 0,
            -5, 0, 0, 0, 0, 0, 0, -5,
            0, 0, 0, 0, 0, 0, 5, -5,
            -10, 5, 0, 0, 0, 5, 0, -10,
            0, 0, -5, 0, -5, 0, 0, 0,
            -10, -10, -10, -5, -10, -10, -10, -10,
        };

        int[] kingTable = {
            -30, -10, -5, -5, -5, -5, -5, -30,
            -20, -10, 0, 0, 0, 0, -10, -20,
            -10, 0, 0, 0, 0, 0, 0, -10,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, -10, -10, -10, -10, 0, 0,
            -5, -5, -10, -10, -10, -10, -5, -5,
            -10, -10, -10, -10, -10, -10, -10, -10,
            0, 5, 10, -30, -15, -20, 30, 5,
        };
        

        Move[] moves = board.GetLegalMoves();
        Move bestMove = moves[0];

        int moveCount = 0;
        int bestEval = int.MinValue;
        bool isWhiteToMove = board.IsWhiteToMove;

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int eval = Minimax(board, 3, isWhiteToMove); // Depth = 3
            board.UndoMove(move);

            Console.WriteLine($"Move: {move}, Eval: {eval}");

            if (eval > bestEval)
            {
                bestEval = eval;
                bestMove = move;
            }
        }

        moveCount++;
        return bestMove;

        // ------------------------
        // Local function: Minimax
        // ------------------------
        int Minimax(Board board, int depth, bool isWhiteToMove)
        {
            if (depth == 0 || board.IsInCheckmate() || board.IsDraw())
            {
                return Evaluate(board);
            }

            Move[] moves = board.GetLegalMoves();
            int bestEval = isWhiteToMove ? int.MinValue : int.MaxValue;

            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int eval = Minimax(board, depth - 1, isWhiteToMove);
                board.UndoMove(move);

                if (isWhiteToMove)
                    bestEval = Math.Max(bestEval, eval);
                else
                    bestEval = Math.Min(bestEval, eval);
            }

            return bestEval;
        }

        // -------------------------
        // Local function: Evaluate
        // -------------------------
        int Evaluate(Board board)
        {
            int whiteEval = TotalMaterialCount(true, board);
            int blackEval = TotalMaterialCount(false, board);
            int eval = whiteEval - blackEval;

            if (!board.IsWhiteToMove)
            {
                eval = -eval;
            }
            board.GetAllPieceLists();
            
            return eval;
        }

        // ----------------------------------------
        // Local function: Total Material Count
        // ----------------------------------------
        int TotalMaterialCount(bool isWhite, Board board)
        {
            const int pawnVal = 100;
            const int knightVal = 300;
            const int bishopVal = 300;
            const int rookVal = 500;
            const int queenVal = 900;

            int material = 0;
            material += board.GetPieceList(PieceType.Pawn, isWhite).Count * pawnVal;
            material += board.GetPieceList(PieceType.Knight, isWhite).Count * knightVal;
            material += board.GetPieceList(PieceType.Bishop, isWhite).Count * bishopVal;
            material += board.GetPieceList(PieceType.Rook, isWhite).Count * rookVal;
            material += board.GetPieceList(PieceType.Queen, isWhite).Count * queenVal;

            return material;
        }
    }
}