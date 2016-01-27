using System;
using System.Collections.Generic;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;


namespace TronsTour
{
	public class Board : BoardGames.Board<Piece, Vector2i, Movement>
	{
		public Vector2i ToBoard(Vector3 worldPos)
		{
			return new Vector2i(Mathf.Clamp((int)worldPos.x, 0, Width - 1),
								Mathf.Clamp((int)worldPos.y, 0, Height - 1));
		}
		public Vector3 ToWorld(Vector2i boardPos)
		{
			return new Vector3(boardPos.x + 0.5f, boardPos.y + 0.5f, 0.0f);
		}

		/// <summary>
		/// Pieces move like a Knight in chess (2 steps along one axis, 1 step along another axis).
		/// </summary>
		private static readonly Vector2i[] possibleMoves = new Vector2i[]
		{
			new Vector2i(2, 1), new Vector2i(1, 2),
			new Vector2i(2, -1), new Vector2i(-1, 2),
			new Vector2i(-2, 1), new Vector2i(1, -2),
			new Vector2i(-2, -1), new Vector2i(-1, -2),
		};


		public int Width = 6,
				   Height = 9;
		public Piece[] Pieces = new Piece[2] { null, null };


		private bool[,] visitedSpaces;
		private SpriteRenderer[,] spaceSprs;


		protected override void Awake()
		{
			base.Awake();

			visitedSpaces = new bool[Width, Height];
			spaceSprs = new SpriteRenderer[Width, Height];
			for (int x = 0; x < Width; ++x)
			{
				for (int y = 0; y < Height; ++y)
				{
					visitedSpaces[x, y] = false;
					spaceSprs[x, y] = null;
				}
			}

			Assert.IsTrue(Pieces.Length == 2, "Must have exactly two pieces");
		}
		void Start()
		{
			//Create a sprite for each space.
			for (int x = 0; x < Width; ++x)
			{
				for (int y = 0; y < Height; ++y)
				{
					spaceSprs[x, y] = SpritePool.Instance.AllocateSprites(1,
																		  Constants.Instance.OpenSpaceSprite,
																		  1)[0];
					spaceSprs[x, y].transform.position = new Vector3(x + 0.5f, y + 0.5f, 0.0f);
				}
			}

			//Position the pieces.
			Pieces[0].CurrentPos = new Vector2i((Width - 1) / 2, 0);
			Pieces[1].CurrentPos = new Vector2i(Width / 2, Height - 1);
			foreach (Piece p in Pieces)
			{
				p.transform.position = new Vector3(p.CurrentPos.x + 0.5f, p.CurrentPos.y + 0.5f, 0.0f);

				visitedSpaces[p.CurrentPos.x, p.CurrentPos.y] = true;
				spaceSprs[p.CurrentPos.x, p.CurrentPos.y].sprite = Constants.Instance.ClosedSpaceSprite;
			}
		}


		public override Piece GetPiece(Vector2i space)
		{
			if (Pieces[0].CurrentPos == space)
				return Pieces[0];
			else if (Pieces[1].CurrentPos == space)
				return Pieces[1];

			return null;
		}
		public Piece GetPiece(BoardGames.Players player)
		{
			return Pieces[(int)player];
		}
		public override IEnumerable<Piece> GetPieces(BoardGames.Players team) { yield return GetPiece(team); }

		public override IEnumerable<Movement> GetMoves(Piece piece)
		{
			//Get all possible spaces to move to and filter out the illegal ones.
			foreach (Vector2i v in possibleMoves)
			{
				Movement mv = new Movement();
				mv.Pos = piece.CurrentPos + v;
				mv.IsMoving = piece;

				if (mv.Pos.x >= 0 && mv.Pos.x < Width && mv.Pos.y >= 0 && mv.Pos.y < Height &&
					!visitedSpaces[mv.Pos.x, mv.Pos.y])
				{
					yield return mv;
				}
			}
		}

		public override void ApplyMove(Movement move)
		{
			//Move the piece.
			move.IsMoving.CurrentPos = move.Pos;
			move.IsMoving.MyTr.position = new Vector3(move.Pos.x + 0.5f, move.Pos.y + 0.5f, 0.0f);

			//Close off the new position.
			visitedSpaces[move.Pos.x, move.Pos.y] = true;
			spaceSprs[move.Pos.x, move.Pos.y].sprite = Constants.Instance.ClosedSpaceSprite;

			Instantiate(Constants.Instance.PlacePieceEffectPrefab).transform.position =
				move.IsMoving.MyTr.position;
		}
	}
}