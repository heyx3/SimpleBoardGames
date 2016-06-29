using System;
using System.Collections.Generic;
using UnityEngine;


namespace Surakarta
{
	public class Board : BoardGames.Board<Piece, Vector2i, Movement>
	{
		public static readonly int BoardSize = 6;


		public Piece[,] Grid = new Piece[BoardSize, BoardSize];

		/// <summary>
		/// Used to determine the positions of every spot on the board.
		/// </summary>
		public Transform BottomLeftPos, BottomLeftPosPlusOneX;

		public Curves.UnityMultiCurve SmallMovementBottomLeft,
									  SmallMovementBottomRight,
									  SmallMovementTopLeft,
									  SmallMovementTopRight,
									  LargeMovementBottomLeft,
									  LargeMovementBottomRight,
									  LargeMovementTopLeft,
									  LargeMovementTopRight;


		private Vector2 pos0;
		private float posIncrement;
		

		private Vector2 GetWorldPos(Vector2i boardPos)
		{
			return pos0 + new Vector2(posIncrement * boardPos.x, posIncrement * boardPos.y);
		}

		protected override void Awake()
		{
			base.Awake();

			pos0 = BottomLeftPos.position;
			posIncrement = (pos0.x - BottomLeftPosPlusOneX.position.x);

			for (int y = 0; y < BoardSize; ++y)
				for (int x = 0; x < BoardSize; ++x)
					Grid[x, y] = null;
		}

		public override Piece GetPiece(Vector2i space)
		{
			return Grid[space.x, space.y];
		}
		public override IEnumerable<Piece> GetPieces(BoardGames.Players team)
		{
			foreach (Piece p in Grid)
				if (p != null && p.Owner == team)
					yield return p;
		}

		public override IEnumerable<Movement> GetMoves(Piece piece)
		{
			Vector2i p = piece.CurrentPos;

			List<Movement> mvs = new List<Movement>();
			bool minXEdge = (p.x == 0),
				 maxXEdge = (p.x == BoardSize - 1),
				 minYEdge = (p.y == 0),
				 maxYEdge = (p.y == BoardSize - 1);

			//Normal movements.
			if (!minXEdge && GetPiece(p.LessX) == null)
				mvs.Add(new Movement(p, p.LessX, piece, false));
			if (!maxXEdge && GetPiece(p.MoreX) == null)
				mvs.Add(new Movement(p, p.MoreX, piece, false));
			if (!minYEdge && GetPiece(p.LessY) == null)
				mvs.Add(new Movement(p, p.LessY, piece, false));
			if (!maxYEdge && GetPiece(p.MoreY) == null)
				mvs.Add(new Movement(p, p.MoreY, piece, false));
			if (!minXEdge && !minYEdge && GetPiece(p.LessX.LessY) == null)
				mvs.Add(new Movement(p, p.LessX.LessY, piece, false));
			if (!minXEdge && !maxYEdge && GetPiece(p.LessX.MoreY) == null)
				mvs.Add(new Movement(p, p.LessX.MoreY, piece, false));
			if (!maxXEdge && !minYEdge && GetPiece(p.MoreX.LessY) == null)
				mvs.Add(new Movement(p, p.MoreX.LessY, piece, false));
			if (!maxXEdge && !maxYEdge && GetPiece(p.MoreX.MoreY) == null)
				mvs.Add(new Movement(p, p.MoreX.MoreY, piece, false));

			//Capture movements.
			if ((!minXEdge && !maxXEdge) || (!minYEdge && !maxYEdge))
			{

			}

			return mvs;
		}
		public override void ApplyMove(Movement move)
		{
			if (move.IsCapturing)
			{
				UnityEngine.Assertions.Assert.IsNotNull(Grid[move.Pos.x, move.Pos.y]);

				Transform tr = Instantiate<GameObject>(move.IsMoving.Owner == BoardGames.Players.One ?
													   Constants.Instance.CapturePlayer1EffectsPrefab :
													   Constants.Instance.CapturePlayer2EffectsPrefab).transform;
				tr.position = move.IsMoving.MyTr.position;
			}
			else
			{
				UnityEngine.Assertions.Assert.IsNull(Grid[move.Pos.x, move.Pos.y]);

				Grid[move.PreviousPos.x, move.PreviousPos.y] = null;
				Grid[move.Pos.x, move.Pos.y] = move.IsMoving;

				MoveToPosition mtp = move.IsMoving.gameObject.AddComponent<MoveToPosition>();
				mtp.MovementCurve = Constants.Instance.NormalMoveCurve;
				mtp.TotalTime = Constants.Instance.NormalMoveTime;
				mtp.EndPos = GetWorldPos(move.Pos);
			}
		}
	}
}