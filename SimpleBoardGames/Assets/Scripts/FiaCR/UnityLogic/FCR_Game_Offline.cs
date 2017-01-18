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


		public float CursedPieceWaitTime = 0.5f;

		public Sprite GridCellSprite;
		public ScaleCameraToFit CameraScaler;

		private List<SpriteRenderer> gridCells = null;

		
		public Board.Sizes Size { get; private set; }
		public System.Random RNG { get; private set; }
		
		public bool IsJuliaTurn { get; private set; }
		public uint MovesLeft { get; private set; }

		protected override string GameName { get { return "FiaCR"; } }


		protected override Board<Vector2i> CreateNewBoard()
		{
			int size = (int)Size;
			CameraScaler.RegionToFit = new Rect(0.0f, 0.0f, size, size);
			StartCoroutine(RunBoardGridCreation());

			return new Board(Size, RNG.Next());
		}
		protected override void OnAction(BoardGames.Action<Vector2i> move)
		{
			if (CurrentTurn.Value == Board.Player_Humans)
			{
				if (IsJuliaTurn)
				{
					if (MovesLeft == 0)
						AdvanceTurn();
					else
						MovesLeft -= 1;
				}
				else
				{
					if (MovesLeft == 0)
						AdvanceTurn();
					else
						MovesLeft -= 1;
				}
			}
			else
			{
				//No need to do anything; the coroutine is handling it.
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
					MovesLeft = Board.NBillyMovesByBoardSize[Size];
					FCR_MovesUI_Julia.Instance.DeInit();
				}
				else
				{
					CurrentTurn.Value = Board.Player_TC;
					StartCoroutine(RunCurseTurn());
				}
			}
			else
			{
				IsJuliaTurn = true;
				MovesLeft = Board.NJuliaMovesByBoardSize[Size];
				CurrentTurn.Value = Board.Player_Humans;
				FCR_MovesUI_Julia.Instance.Init();
			}
		}

		private System.Collections.IEnumerator RunCurseTurn()
		{
			HashSet<Piece> cursedPieces = new HashSet<Piece>(
				TheBoard.GetPieces(piece => piece.Owner.Value == Board.Player_TC)
						.Cast<Piece>());

			Board board = (Board)TheBoard;
			foreach (Piece p in TheBoard.GetPieces(piece => piece.Owner.Value == Board.Player_TC)
										.Cast<Piece>())
			{
				if (board.NextFloat() < Board.ChanceCurseMoveByBoardSize[Size])
				{
					var moves = board.GetActions(p).ToList();
					if (moves.Count > 0)
						moves[board.NextInt(moves.Count)].DoAction();

					yield return new WaitForSeconds(CursedPieceWaitTime);
				}
			}

			AdvanceTurn();
		}

		protected override void Awake()
		{
			Size = (Board.Sizes)PlayerPrefs.GetInt(PlayerPrefs_Size, (int)Board.Sizes.SixBySix);
			RNG = new System.Random(PlayerPrefs.GetInt(PlayerPrefs_Seed,
													   UnityEngine.Random.Range(0, int.MaxValue)));

			Screen.orientation = ScreenOrientation.Portrait;

			base.Awake();
		}
		private void Start()
		{
			CurrentTurn.Value = Board.Player_TC;
			AdvanceTurn();
		}
	}
}