using System.Security.Cryptography.X509Certificates;
using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        Move bestMove = moves[0];
        int bestEval = int.MinValue;

        bool isWhiteToMove = board.IsWhiteToMove;
        const int searchDepth = 2;  // Depth 2 search

    foreach (Move move in moves)
        {
            board.MakeMove(move);
            int eval = Minimax(board, searchDepth, isWhiteToMove);
            board.UndoMove(move);

            if (eval > bestEval)
            {
                bestEval = eval;
                bestMove = move;
            }
        }

    return bestMove;

        public static int Evaluate(Board board) {
            int whiteEval = TotalMaterialCount(true, board);
            int blackEval = TotalMaterialCount(false, board);
            int eval = whiteEval - blackEval;
            
            if (!board.IsWhiteToMove) {
                eval = -eval;
            }
            
            // Should output into the 'Diverted Console'
            DivertedConsole.Write(eval.ToString);
    }

        public static int TotalMaterialCount(bool isWhite, Board board) {
            // Thank you ChatGPT!
            // Values below will need more tuning, but we'll see
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

    public static int Minimax(Board board, int depth, bool IsWhiteToMove)
    {
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw()) {
            return Evaluate(board);
        }

        Move[] moves = board.GetLegalMoves();
        int bestEval = IsWhiteToMove ? int.MinValue : int.MaxValue;

        foreach (Move move in moves) {
            board.MakeMove(move);
            int eval = Minimax(board, depth - 1, !IsWhiteToMove);
            board.UndoMove(move);

            if (IsWhiteToMove) {
                bestEval = Math.Max(bestEval, eval);
            }
            else {
                bestEval = Math.Min(bestEval, eval);
            }
        }

        return bestEval;
    }
    }
}