using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;


namespace Fitchneil
{
	public class Board : BoardGames.Board<Vector2i>
	{
		public static readonly BoardGames.Players Player_Defender = BoardGames.Players.Two,
											      Player_Attacker = BoardGames.Players.One;

		public static readonly int BoardSize = 7;


		public static Vector2i ToBoard(Vector3 world)
		{
			return new Vector2i(Mathf.Clamp((int)world.x, 0, BoardSize - 1),
								Mathf.Clamp((int)world.y, 0, BoardSize - 1));
		}
		public static Vector3 ToWorld(Vector2i boardPos)
		{
			return new Vector3(boardPos.x + 0.5f, boardPos.y + 0.5f, 0.0f);
		}
		public static bool IsInBounds(Vector2i boardPos)
		{
			return boardPos.x >= 0 && boardPos.x < BoardSize &&
				   boardPos.y >= 0 && boardPos.y < BoardSize;
		}


		/// <summary>
		/// Raised when a piece captures another piece.
		/// The arguments are as follows:
		///     1) The board
		///     2) The piece that was captured
		///     3) The piece whose movement resulted in the capture.
		/// </summary>
		public event Action<Board, Piece, Piece> OnPieceCaptured;
		/// <summary>
		/// Raised when a piece is added to the board.
		/// This happens when a move that captures pieces is undone.
		/// </summary>
		public event Action<Board, Piece> OnPieceAdded;

		/// <summary>
		/// When this board deserializes a stream to read a new state,
		///     all current pieces are thrown away and new pieces are put on the board.
		/// </summary>
		public event Action<Board> OnBoardDeserialized;


		private Piece[,] theBoard;


		public Board()
		{
			//Create the board.
			theBoard = new Piece[BoardSize, BoardSize];
			for (int y = 0; y < BoardSize; ++y)
				for (int x = 0; x < BoardSize; ++x)
					theBoard[x, y] = null;


			//Create the pieces.

			int centerPos = (BoardSize / 2);

			//King:
			theBoard[centerPos, centerPos] =
				new Piece(true, new Vector2i(centerPos, centerPos), Player_Defender, this);
			//Defenders:
			for (int i = 1; i <= 2; ++i)
			{
				theBoard[centerPos - i, centerPos] =
					new Piece(false, new Vector2i(centerPos - i, centerPos), Player_Defender, this);
				theBoard[centerPos + i, centerPos] =
					new Piece(false, new Vector2i(centerPos + i, centerPos), Player_Defender, this);
				theBoard[centerPos, centerPos - i] =
					new Piece(false, new Vector2i(centerPos, centerPos - i), Player_Defender, this);
				theBoard[centerPos, centerPos + i] =
					new Piece(false, new Vector2i(centerPos, centerPos + i), Player_Defender, this);
			}
			//Attackers:
			for (int i = -1; i <= 1; ++i)
			{
				theBoard[0, centerPos + i] =
					new Piece(false, new Vector2i(0, centerPos + i), Player_Attacker, this);
				theBoard[BoardSize - 1, centerPos + i] =
					new Piece(false, new Vector2i(BoardSize - 1, centerPos + i), Player_Attacker, this);
				theBoard[centerPos + i, 0] =
					new Piece(false, new Vector2i(centerPos + i, 0), Player_Attacker, this);
				theBoard[centerPos + i, BoardSize - 1] =
					new Piece(false, new Vector2i(centerPos + i, BoardSize - 1), Player_Attacker, this);
			}
			theBoard[0, 0] =
				new Piece(false, new Vector2i(0, 0), Player_Attacker, this);
			theBoard[0, BoardSize - 1] =
				new Piece(false, new Vector2i(0, BoardSize - 1), Player_Attacker, this);
			theBoard[BoardSize - 1, 0] =
				new Piece(false, new Vector2i(BoardSize - 1, 0), Player_Attacker, this);
			theBoard[BoardSize - 1, BoardSize - 1] =
				new Piece(false, new Vector2i(BoardSize - 1, BoardSize - 1), Player_Attacker, this);

			//When a piece moves, switch its place in the grid.
			OnAction += (thisBoard, action) =>
			{
				Action_Move movement = (Action_Move)action;

				theBoard[movement.EndPos.x, movement.EndPos.y] =
					theBoard[movement.StartPos.x, movement.StartPos.y];
				theBoard[movement.StartPos.x, movement.StartPos.y] = null;
			};
		}


		public override IEnumerable<BoardGames.Piece<Vector2i>> GetPieces()
		{
			for (int y = 0; y < theBoard.GetLength(1); ++y)
				for (int x = 0; x < theBoard.GetLength(0); ++x)
					if (theBoard[x, y] != null)
						yield return theBoard[x, y];
		}
		public override IEnumerable<BoardGames.Piece<Vector2i>> GetPieces(Vector2i space)
		{
			if (theBoard[space.x, space.y] != null)
				yield return theBoard[space.x, space.y];
		}
		
		public override IEnumerable<BoardGames.Action<Vector2i>> GetActions(BoardGames.Piece<Vector2i> piece)
		{
			//See how far to the left and right the piece can move.
			for (int xDir = -1; xDir <= 1; xDir += 2)
			{
				int x = piece.CurrentPos.Value.x + xDir;
				while (x >= 0 && x < BoardSize)
				{
					bool isBlocking;
					Action_Move move = TryMove(piece.CurrentPos,
											   new Vector2i(x, piece.CurrentPos.Value.y),
											   out isBlocking);

					if (move != null)
						yield return move;

					if (isBlocking)
						break;

					x += xDir;
				}
			}
			//See how far up/down the piece can move.
			for (int yDir = -1; yDir <= 1; yDir += 2)
			{
				int y = piece.CurrentPos.Value.y + yDir;
				while (y >= 0 && y < BoardSize)
				{
					bool isBlocking;
					Action_Move move = TryMove(piece.CurrentPos,
											   new Vector2i(piece.CurrentPos.Value.x, y),
											   out isBlocking);

					if (move != null)
						yield return move;

					if (isBlocking)
						break;

					y += yDir;
				}
			}
		}
		/// <summary>
		/// Evaluates the possibility of moving from the given postion to the given other position.
		/// Assumes the spaces between the two positions are clear of pieces.
		/// Returns "null" if the move isn't allowed.
		/// Also outputs whether something is blocking any further movement in this direction.
		/// </summary>
		/// <param name="isBlocking">
		/// This variable will tell whether something is blocking
		///     any further movement in this direction.
		/// </param>
		private Action_Move TryMove(Vector2i from, Vector2i to, out bool isBlocking)
		{
			Piece p = theBoard[from.x, from.y];
			Assert.IsNotNull(p, "No piece at " + from.ToString());

			//If something is already there, no moves are allowed and the movement is blocked.
			if (theBoard[to.x, to.y] != null)
			{
				isBlocking = true;
				return null;
			}

			//If this piece isn't a king and this is the throne square,
			//    it can't move into the throne but it can move *through* it.
			if (!p.IsKing && to.x == (BoardSize / 2) && to.y == (BoardSize / 2))
			{
				isBlocking = false;
				return null;
			}

			//Otherwise, it's a valid move.
			isBlocking = false;
			return new Action_Move(to, p, GetCaptures(from, to));
		}
		/// <summary>
		/// Gets all captures that would happen when moving the piece currently at "from" to "to".
		/// </summary>
		private HashSet<Piece> GetCaptures(Vector2i from, Vector2i to)
		{
			Piece p = theBoard[from.x, from.y];
			HashSet<Piece> caps = new HashSet<Piece>();

			Assert.IsNotNull(p);
			Assert.IsNull(theBoard[to.x, to.y]);

			//Imagine the board after the move is completed.
			Func<Vector2i, Piece> getPiece = (tile) =>
			{
				if (tile == from)
					return null;
				else if (tile == to)
					return p;
				else
					return theBoard[tile.x, tile.y];
			};
			Func<Piece, Vector2i, bool> isAllyAtPos = (piece, pos) =>
			{
				Piece piece2 = getPiece(pos);
				return piece2 != null && piece2.Owner.Value == piece.Owner.Value;
			};
			Func<Piece, Vector2i, bool, bool> isEnemyAtPos = (piece, pos, includeKing) =>
			{
				Piece piece2 = getPiece(pos);
				return piece2 != null && piece2.Owner.Value != piece.Owner.Value &&
					   (includeKing || !piece2.IsKing);
			};
			Func<Vector2i, bool> isKingAtPos = (pos) =>
			{
				Piece piece = getPiece(pos);
				return piece != null && piece.IsKing;
			};
			Func<Vector2i, bool> isKingThreatenedByPos = (pos) =>
			{
				//The king is threatened by attackers *and* by the throne space itself.
				if (pos.x == (BoardSize / 2) && pos.y == (BoardSize / 2))
					return true;
				Piece piece = getPiece(pos);
				return piece != null && piece.Owner.Value == Player_Attacker;
			};

			//See if any normal (i.e. non-king) enemies were captured.
			if (to.x > 1 && isEnemyAtPos(p, to.LessX, false) && isAllyAtPos(p, to.LessX.LessX))
				caps.Add(getPiece(to.LessX));
			if (to.x < BoardSize - 2 && isEnemyAtPos(p, to.MoreX, false) && isAllyAtPos(p, to.MoreX.MoreX))
				caps.Add(getPiece(to.MoreX));
			if (to.y > 1 && isEnemyAtPos(p, to.LessY, false) && isAllyAtPos(p, to.LessY.LessY))
				caps.Add(getPiece(to.LessY));
			if (to.y < BoardSize - 2 && isEnemyAtPos(p, to.MoreY, false) && isAllyAtPos(p, to.MoreY.MoreY))
				caps.Add(getPiece(to.MoreY));

			//See if the King was captured.
			if (p.Owner.Value == Player_Attacker)
			{
				//First, see if the king is adjacent to the new position.
				Vector2i kingPos = new Vector2i(-1, -1);
				if (to.x > 0 && isKingAtPos(to.LessX))
					kingPos = to.LessX;
				else if (to.y > 0 && isKingAtPos(to.LessY))
					kingPos = to.LessY;
				else if (to.x < BoardSize - 1 && isKingAtPos(to.MoreX))
					kingPos = to.MoreX;
				else if (to.y < BoardSize - 1 && isKingAtPos(to.MoreY))
					kingPos = to.MoreY;

				//If he is, see if he is now surrounded on all sides by threats.
				if (kingPos != new Vector2i(-1, -1))
				{
					if (kingPos.x > 0 && isKingThreatenedByPos(kingPos.LessX) &&
						kingPos.y > 0 && isKingThreatenedByPos(kingPos.LessY) &&
						kingPos.x < BoardSize - 1 && isKingThreatenedByPos(kingPos.MoreX) &&
						kingPos.y < BoardSize - 1 && isKingThreatenedByPos(kingPos.MoreY))
					{
						caps.Add(getPiece(kingPos));
					}
				}
			}

			return caps;
		}

		/// <summary>
		/// Removes the given piece from this board and raises the proper event.
		/// </summary>
		public void CapturePiece(Piece captured, Piece capturedBy)
		{
			theBoard[captured.CurrentPos.Value.x, captured.CurrentPos.Value.y] = null;

			if (OnPieceCaptured != null)
				OnPieceCaptured(this, captured, capturedBy);
		}
		/// <summary>
		/// Adds a new piece to the board (assumed to have an empty spot at the piece's position).
		/// Used to undo actions that capture pieces.
		/// </summary>
		public void PlacePiece(Piece newPiece)
		{
			UnityEngine.Assertions.Assert.IsNull(theBoard[newPiece.CurrentPos.Value.x,
														  newPiece.CurrentPos.Value.y]);
			theBoard[newPiece.CurrentPos.Value.x, newPiece.CurrentPos.Value.y] = newPiece;

			if (OnPieceAdded != null)
				OnPieceAdded(this, newPiece);
		}

		public override void Serialize(BinaryWriter stream)
		{
			//Write the type of piece in each tile.
			for (int y = 0; y < BoardSize; ++y)
			{
				for (int x = 0; x < BoardSize; ++x)
				{
					Piece p = theBoard[x, y];
					byte val = 0;
					if (p != null)
					{
						if (p.Owner.Value == Player_Attacker)
							val = 1;
						else if (p.IsKing)
							val = 3;
						else
							val = 2;
					}
					stream.Write(val);
				}
			}
		}
		public override void Deserialize(BinaryReader stream)
		{
			//Read the type of piece in each tile.
			for (int y = 0; y < BoardSize; ++y)
			{
				for (int x = 0; x < BoardSize; ++x)
				{
					byte b = stream.ReadByte();
					switch (b)
					{
						case 0: theBoard[x, y] = null; break;

						case 1:
							theBoard[x, y] = new Piece(false, new Vector2i(x, y),
													   Player_Attacker, this);
							break;
						case 2:
							theBoard[x, y] = new Piece(false, new Vector2i(x, y),
													   Player_Defender, this);
							break;
						case 3:
							theBoard[x, y] = new Piece(true, new Vector2i(x, y),
													   Player_Defender, this);
							break;

						default:
							Debug.LogError("Unknown piece type " + b.ToString());
							theBoard[x, y] = null;
							break;
					}
				}
			}

			if (OnBoardDeserialized != null)
				OnBoardDeserialized(this);
		}
	}
}