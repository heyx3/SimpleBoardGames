using System;
using System.Collections.Generic;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;


namespace Fitchneil
{
	public class Board : BoardGames.Board<Piece, Vector2i, Movement>
	{
		public static int BoardSize = 7;

		public static Vector2i ToBoard(Vector3 world)
		{
			return new Vector2i(Mathf.Clamp((int)world.x, 0, BoardSize - 1),
								Mathf.Clamp((int)world.y, 0, BoardSize - 1));
		}
		public static Vector3 ToWorld(Vector2i boardPos)
		{
			return new Vector3(boardPos.x + 0.5f, boardPos.y + 0.5f, 0.0f);
		}


		/// <summary>
		/// The number of attackers left on the board.
		/// </summary>
		public int NAttackerPieces { get; private set; }

		private Piece[,] theBoard;


		protected override void Awake()
		{
			base.Awake();

			theBoard = new Piece[BoardSize, BoardSize];
			for (int x = 0; x < BoardSize; ++x)
				for (int y = 0; y < BoardSize; ++y)
					theBoard[x, y] = null;
			NAttackerPieces = 0;
		}
		void Start()
		{
			//Set up the board.

			int centerPos = (BoardSize / 2);

			MakePiece(new Vector2i(centerPos, centerPos), Constants.Instance.KingPrefab);

			for (int i = 1; i <= 2; ++i)
			{
				MakePiece(new Vector2i(centerPos - i, centerPos), Constants.Instance.DefenderPrefab);
				MakePiece(new Vector2i(centerPos + i, centerPos), Constants.Instance.DefenderPrefab);
				MakePiece(new Vector2i(centerPos, centerPos - i), Constants.Instance.DefenderPrefab);
				MakePiece(new Vector2i(centerPos, centerPos + i), Constants.Instance.DefenderPrefab);
			}

			for (int i = -1; i <= 1; ++i)
			{
				MakePiece(new Vector2i(0, centerPos + i), Constants.Instance.AttackerPrefab);
				MakePiece(new Vector2i(BoardSize - 1, centerPos + i), Constants.Instance.AttackerPrefab);
				MakePiece(new Vector2i(centerPos + i, 0), Constants.Instance.AttackerPrefab);
				MakePiece(new Vector2i(centerPos + i, BoardSize - 1), Constants.Instance.AttackerPrefab);
			}
			MakePiece(new Vector2i(0, 0), Constants.Instance.AttackerPrefab);
			MakePiece(new Vector2i(0, BoardSize - 1), Constants.Instance.AttackerPrefab);
			MakePiece(new Vector2i(BoardSize - 1, 0), Constants.Instance.AttackerPrefab);
			MakePiece(new Vector2i(BoardSize - 1, BoardSize - 1), Constants.Instance.AttackerPrefab);
		}

		private void MakePiece(Vector2i pos, GameObject prefab)
		{
			GameObject go = Instantiate(prefab);
			go.transform.position = ToWorld(pos);

			Piece p = go.GetComponent<Piece>();
			p.CurrentPos = pos;
			theBoard[pos.x, pos.y] = p;

			if (p.Owner == Piece.Attackers)
				NAttackerPieces += 1;
		}


		public override Piece GetPiece(Vector2i space)
		{
			return theBoard[space.x, space.y];
		}
		public override IEnumerable<Piece> GetPieces(BoardGames.Players team)
		{
			if (team == Piece.Attackers)
				return Piece.AttackerPieces;
			else if (team == Piece.Defenders)
				return Piece.DefenderPieces;
			else throw new NotImplementedException("Unknown team " + team.ToString());
		}

		public override IEnumerable<Movement> GetMoves(Piece piece)
		{
			Movement move = new Movement();
			bool isBlocking;

			int x = piece.CurrentPos.x + 1;
			isBlocking = false;
			while (x < BoardSize)
			{
				if (TryMove(piece.CurrentPos, new Vector2i(x, piece.CurrentPos.y),
							ref move, out isBlocking))
				{
					yield return move;
				}
				if (isBlocking)
					break;

				x += 1;
			}

			x = piece.CurrentPos.x - 1;
			isBlocking = false;
			while (x >= 0)
			{
				if (TryMove(piece.CurrentPos, new Vector2i(x, piece.CurrentPos.y),
							ref move, out isBlocking))
				{
					yield return move;
				}
				if (isBlocking)
					break;

				x -= 1;
			}


			int y = piece.CurrentPos.y + 1;
			isBlocking = false;
			while (y < BoardSize)
			{
				if (TryMove(piece.CurrentPos, new Vector2i(piece.CurrentPos.x, y),
							ref move, out isBlocking))
				{
					yield return move;
				}
				if (isBlocking)
					break;

				y += 1;
			}

			y = piece.CurrentPos.y - 1;
			isBlocking = false;
			while (y >= 0)
			{
				if (TryMove(piece.CurrentPos, new Vector2i(piece.CurrentPos.x, y),
							ref move, out isBlocking))
				{
					yield return move;
				}
				if (isBlocking)
					break;

				y -= 1;
			}
		}

		public override void ApplyMove(Movement move)
		{
			//Move the piece itself.
			theBoard[move.Pos.x, move.Pos.y] = move.IsMoving;
			theBoard[move.IsMoving.CurrentPos.x, move.IsMoving.CurrentPos.y] = null;
			move.IsMoving.CurrentPos = move.Pos;
			move.IsMoving.MyTr.position = ToWorld(move.Pos);

			//Destroy all captured pieces.
			foreach (Piece p in move.Captures)
			{
				//Spawn some effects.
				GameObject dpGO = Instantiate(Constants.Instance.DestroyedPieceEffectPrefab);
				dpGO.transform.position = ToWorld(p.CurrentPos);

				//Destroy the piece.
				if (p.Owner == Piece.Attackers)
					NAttackerPieces -= 1;
				theBoard[p.CurrentPos.x, p.CurrentPos.y] = null;
				Destroy(p.gameObject);
			}
		}

		
		//The below items are helpers for "GetMoves()".

		/// <summary>
		/// Evaluates the possibility of moving from the given postion to the given other position.
		/// Assumes the spaces between the two positions are clear.
		/// Returns whether the move is allowed.
		/// Also outputs whether something is blocking any further movement in this direction.
		/// If the move is allowed, the actual data will be output into the "move" ref variable.
		/// </summary>
		private bool TryMove(Vector2i from, Vector2i to, ref Movement move, out bool isBlocking)
		{
			Piece p = GetPiece(from);
			Assert.IsNotNull(p, "No piece at " + from.ToString());

			//If something is already there, no moves are allowed and the movement is blocked.
			if (GetPiece(to) != null)
			{
				isBlocking = true;
				return false;
			}

			//If this piece isn't a king and this is the throne square,
			//    it can't move into the throne but it can move *through* it.
			if (!p.IsKing && to.x == (BoardSize / 2) && to.y == (BoardSize / 2))
			{
				isBlocking = false;
				return false;
			}

			//Otherwise, it's a valid move.

			move.Pos = to;
			move.IsMoving = p;
			move.Captures = GetCaptures(from, to);
			isBlocking = false;
			return true;
		}

		private Piece[,] tempBoard = null;
		private List<Piece> GetCaptures(Vector2i from, Vector2i to)
		{
			Piece p = GetPiece(from);
			List<Piece> caps = new List<Piece>();

			Assert.IsNotNull(p);
			Assert.IsNull(GetPiece(to));

			//Create a copy of the board with the move completed.
			if (tempBoard == null)
				tempBoard = new Piece[BoardSize, BoardSize];
			Array.Copy(theBoard, tempBoard, theBoard.Length);
			tempBoard[from.x, from.y] = null;
			tempBoard[to.x, to.y] = p;

			//See if any normal (i.e. non-king) enemies were captured.
			if (to.x > 1 &&
				IsEnemyAt(p, to.LessX) &&
				IsAllyAt(p, to.LessX.LessX))
			{
				caps.Add(tempBoard[to.x - 1, to.y]);
			}
			if (to.x < BoardSize - 2 &&
				IsEnemyAt(p, to.MoreX) &&
				IsAllyAt(p, to.MoreX.MoreX))
			{
				caps.Add(tempBoard[to.x + 1, to.y]);
			}
			if (to.y > 1 &&
				IsEnemyAt(p, to.LessY) &&
				IsAllyAt(p, to.LessY.LessY))
			{
				caps.Add(tempBoard[to.x, to.y - 1]);
			}
			if (to.y < BoardSize - 2 &&
				IsEnemyAt(p, to.MoreY) &&
				IsAllyAt(p, to.MoreY.MoreY))
			{
				caps.Add(tempBoard[to.x, to.y + 1]);
			}

			//See if the King was captured.
			if (p.Owner == Piece.Attackers)
			{
				//First, see if the king is adjacent to the new position.
				Vector2i kingPos = new Vector2i(-1, -1);
				if (to.x > 0 && IsKingAt(to.LessX))
					kingPos = to.LessX;
				else if (to.y > 0 && IsKingAt(to.LessY))
					kingPos = to.LessY;
				else if (to.x < BoardSize - 1 && IsKingAt(to.MoreX))
					kingPos = to.MoreX;
				else if (to.y < BoardSize - 1 && IsKingAt(to.MoreY))
					kingPos = to.MoreY;

				//If he is, see if he is now surrounded on all sides by threats.
				if (kingPos != new Vector2i(-1, -1))
				{
					if (kingPos.x > 0 && IsThreateningToKing(kingPos.LessX) &&
						kingPos.y > 0 && IsThreateningToKing(kingPos.LessY) &&
						kingPos.x < BoardSize - 1 && IsThreateningToKing(kingPos.MoreX) &&
						kingPos.y < BoardSize - 1 && IsThreateningToKing(kingPos.MoreY))
					{
						caps.Add(tempBoard[kingPos.x, kingPos.y]);
					}
				}
			}

			return caps;
		}
		private bool IsKingAt(Vector2i space)
		{
			return tempBoard[space.x, space.y] != null &&
				   tempBoard[space.x, space.y].IsKing;
		}
		private bool IsEnemyAt(Piece p, Vector2i space)
		{
			return tempBoard[space.x, space.y] != null &&
				   tempBoard[space.x, space.y].Owner != p.Owner &&
				   !tempBoard[space.x, space.y].IsKing;
		}
		private bool IsAllyAt(Piece p, Vector2i space)
		{
			return tempBoard[space.x, space.y] != null &&
				   tempBoard[space.x, space.y].Owner == p.Owner;
		}
		private bool IsThreateningToKing(Vector2i space)
		{
			//The king is threatened by attackers *and* by the throne space itself.
			return (space.x == (BoardSize / 2) && space.y == (BoardSize / 2)) ||
				   (tempBoard[space.x, space.y] != null &&
				    tempBoard[space.x, space.y].Owner == Piece.Attackers);
		}
	}
}