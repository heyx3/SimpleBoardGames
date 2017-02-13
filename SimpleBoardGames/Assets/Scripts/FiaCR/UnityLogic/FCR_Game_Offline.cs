using System;
using System.Collections.Generic;
using System.Linq;
using BoardGames;
using UnityEngine;


namespace FiaCR.UnityLogic.GameMode
{
	public class FCR_Game_Offline : BoardGames.UnityLogic.GameMode.GameMode_Offline<Vector2i>
	{
		public static readonly string PlayerPrefs_Size = "FiaCR_Size",
									  PlayerPrefs_Seed = "FiaCR_Seed";


		/// <summary>
		/// A custom event for turn changing
		///     because of the weirdness introduced by having three "players".
		/// </summary>
		public event System.Action<FCR_Game_Offline> OnTurnChanged;

		public float CursedPieceWaitTime = 0.5f;

		public Sprite GridCellSprite;
		public ScaleCameraToFit CameraScaler;
		public float CameraScalerVerticalBorder = 1.0f;

		private List<SpriteRenderer> gridCells = null;
		private HashSet<Piece> friendlyPiecesToTireOut = new HashSet<Piece>();

		
		public Board.Sizes Size { get; private set; }
		public System.Random RNG { get; private set; }
		
		public bool IsJuliaTurn { get; private set; }
		public Stat<uint, FCR_Game_Offline> MovesLeft { get; private set; }

		protected override string GameName { get { return "FiaCR"; } }


		protected override Board<Vector2i> CreateNewBoard()
		{
			StartCoroutine(RunBoardGridCreation());

			Board b = new Board(Size, RNG.Next());
			b.OnPieceAdded += Callback_PieceAdded;

			return b;
		}
		protected override void OnAction(BoardGames.Action<Vector2i> move)
		{
			if (CurrentTurn.Value == Board.Player_Humans)
			{
				if (IsJuliaTurn)
				{
					if (MovesLeft.Value <= 1)
						AdvanceTurn();
					else
						MovesLeft.Value -= 1;
				}
				else
				{
					var billyMove = (Action_Move)move;
					FCR_PieceDispatcher.Instance.GetPiece(billyMove.ToMove).MovedAlready.Value = true;

					if (MovesLeft.Value <= 1)
						AdvanceTurn();
					else
						MovesLeft.Value -= 1;
				}
			}
			else
			{
				//Nothing to do here; the coroutine is handling actual turn logic for The Curse.
			}
		}

		private System.Collections.IEnumerator RunBoardGridCreation()
		{
			yield return null;

			if (gridCells != null)
				SpritePool.Instance.DeallocateSprites(gridCells);

			gridCells = SpritePool.Instance.AllocateSprites((int)Size * (int)Size, GridCellSprite);
			int i = 0;
			for (int y = 0; y < (int)Size; ++y)
				for (int x = 0; x < (int)Size; ++x)
					gridCells[i++].transform.position = new Vector3(x + 0.5f, y + 0.5f, 0.0f);
		}

		public void AdvanceTurn()
		{
			if (CurrentTurn.Value == Board.Player_Humans)
			{
				if (IsJuliaTurn)
				{
					IsJuliaTurn = false;
					MovesLeft.Value = Board.NBillyMovesByBoardSize[Size];
					FCR_MovesUI_Julia.Instance.DeInit();

					var pieceObjs = FCR_PieceDispatcher.Instance.Pieces;
					foreach (var piece in pieceObjs.Where(p => p.ToTrack.Owner.Value == Board.Player_Humans))
						piece.MovedAlready.Value = friendlyPiecesToTireOut.Contains((Piece)piece.ToTrack);
					friendlyPiecesToTireOut.Clear();
				}
				else
				{
					CurrentTurn.Value = Board.Player_TC;

					foreach (FCR_Piece p in FCR_PieceDispatcher.Instance.Pieces)
						p.MovedAlready.Value = false;

					StartCoroutine(RunCurseTurn());
				}
			}
			else
			{
				IsJuliaTurn = true;
				MovesLeft.Value = Board.NJuliaMovesByBoardSize[Size];
				CurrentTurn.Value = Board.Player_Humans;
				FCR_MovesUI_Julia.Instance.Init();
			}

			if (OnTurnChanged != null)
				OnTurnChanged(this);
		}

		private System.Collections.IEnumerator RunCurseTurn()
		{
			Board board = (Board)TheBoard;
			var cursedPieces = board.GetPieces(piece => piece.Owner.Value == Board.Player_TC)
									.Cast<Piece>();

			//Create a collection of all current cursed pieces and move them.
			var currentCursedPieces = new HashSet<Piece>(cursedPieces);
			foreach (Piece p in currentCursedPieces)
			{
				foreach (object o in MoveCursedPiece(board, p))
					yield return o;
			}
			//Then, do the same thing for any new cursed pieces that were created.
			var newCursedPieces = cursedPieces.Where(p => !currentCursedPieces.Contains(p)).ToList();
			while (newCursedPieces.Count > 0)
			{
				foreach (Piece p in newCursedPieces)
				{
					currentCursedPieces.Add(p);
					foreach (object o in MoveCursedPiece(board, p))
						yield return o;
				}
				newCursedPieces = cursedPieces.Where(p => !currentCursedPieces.Contains(p)).ToList();
			}

			AdvanceTurn();
		}
		private System.Collections.IEnumerable MoveCursedPiece(Board board, Piece p)
		{
			FCR_PieceDispatcher.Instance.GetPiece(p).MovedAlready.Value = true;

			if (board.NextFloat() < Board.ChanceCurseMoveByBoardSize[Size])
			{
				var moves = board.GetActions(p).ToList();
				if (moves.Count > 0)
					moves[board.NextInt(moves.Count)].DoAction();

				yield return new WaitForSeconds(CursedPieceWaitTime);
			}
		}

		private void Callback_PieceAdded(Board theBoard, Piece thePiece)
		{
			//If this piece was created on a friendly host tile, it should start with no movement.
			var hostTeam = theBoard.GetHost(thePiece.CurrentPos);
			if (hostTeam.HasValue && hostTeam.Value == thePiece.Owner.Value)
			{
				//Cursed pieces are handled by the coroutine,
				//    so we only have to worry about human pieces here.
				if (hostTeam.Value == Board.Player_Humans)
					friendlyPiecesToTireOut.Add(thePiece);
			}
		}

		protected override void Awake()
		{
			Size = (Board.Sizes)PlayerPrefs.GetInt(PlayerPrefs_Size, (int)Board.Sizes.SixBySix);
			RNG = new System.Random(PlayerPrefs.GetInt(PlayerPrefs_Seed,
													   UnityEngine.Random.Range(0, int.MaxValue)));

			Screen.orientation = ScreenOrientation.Portrait;

			MovesLeft = new Stat<uint, FCR_Game_Offline>(this, 0);

			base.Awake();
		}
		private void Start()
		{
			int size = (int)Size;
			CameraScaler.RegionToFit = new Rect(0.0f, -CameraScalerVerticalBorder,
												size, size + (2.0f * CameraScalerVerticalBorder));

			CurrentTurn.Value = Board.Player_TC;
			AdvanceTurn();
		}
	}
}