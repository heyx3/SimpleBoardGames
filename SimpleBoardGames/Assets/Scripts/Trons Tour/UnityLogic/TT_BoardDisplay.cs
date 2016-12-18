using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace TronsTour.UnityLogic
{
	public class TT_BoardDisplay : Singleton<TT_BoardDisplay>
	{
		public GameObject PiecePrefab, PiecePlacementEffectsPrefab;
		
		public Sprite Piece1Sprite, Piece2Sprite;
		public Sprite OpenSpaceSprite, ClosedSpaceSprite;
		
		public AnimationCurve PieceMovementCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
		public float PieceMovementTime = 0.5f;


		public Board TheBoard { get; private set; }
		
		private TT_Piece piece1, piece2;
		private SpriteRenderer[,] tileSprites = null;

		private Transform myTr;


		protected override void Awake()
		{
			base.Awake();
			myTr = transform;
		}
		private void Start()
		{
			TheBoard = (Board)BoardGames.UnityLogic.GameMode.GameMode<Vector2i>.Instance.TheBoard;
			TheBoard.OnBoardDeserialized += Callback_BoardReset;

			TheBoard.OnTileVisitedChanged += Callback_TileVisitedChanged;

			piece1 = Instantiate(PiecePrefab).GetComponent<TT_Piece>();
			piece2 = Instantiate(PiecePrefab).GetComponent<TT_Piece>();

			Callback_BoardReset(TheBoard);
		}

		private void Callback_BoardReset(Board theBoard)
		{
			piece1.ToTrack = theBoard.GetPiece(BoardGames.Players.One);
			piece2.ToTrack = theBoard.GetPiece(BoardGames.Players.Two);

			//Reset the "tileSprites" array.
			if (tileSprites == null ||
				tileSprites.GetLength(0) != theBoard.Width ||
				tileSprites.GetLength(1) != theBoard.Height)
			{
				if (tileSprites != null)
					foreach (SpriteRenderer rnd in tileSprites)
						Destroy(rnd.gameObject);

				tileSprites = new SpriteRenderer[theBoard.Width, theBoard.Height];
				for (int y = 0; y < theBoard.Height; ++y)
					for (int x = 0; x < theBoard.Width; ++x)
					{
						Transform tr = MakeTile(new Vector2i(x, y));
						tr.SetParent(myTr, false);
						tileSprites[x, y] = tr.GetComponent<SpriteRenderer>();
					}
			}
			for (int y = 0; y < theBoard.Height; ++y)
				for (int x = 0; x < theBoard.Width; ++x)
				{
					tileSprites[x, y].sprite = (theBoard.WasVisited(new Vector2i(x, y)) ?
													ClosedSpaceSprite :
													OpenSpaceSprite);
				}
		}
		private void Callback_TileVisitedChanged(Board theBoard, Vector2i tilePos, bool isVisited)
		{
			tileSprites[tilePos.x, tilePos.y].sprite = (isVisited ?
															ClosedSpaceSprite :
															OpenSpaceSprite);

			Instantiate(PiecePlacementEffectsPrefab).transform.position = theBoard.ToWorld(tilePos);
		}

		private Transform MakeTile(Vector2i tile)
		{
			GameObject go = new GameObject("Tile " + tile.ToString());

			Transform tr = go.transform;
			tr.position = TheBoard.ToWorld(tile);

			SpriteRenderer spr = go.AddComponent<SpriteRenderer>();
			spr.sprite = OpenSpaceSprite;

			return tr;
		}
	}
}