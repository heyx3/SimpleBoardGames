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

		
		public Board.Sizes Size { get; private set; }
		public System.Random RNG { get; private set; }
		
		public bool IsJuliaTurn { get; private set; }
		public uint MovesLeft { get; private set; }

		protected override string GameName { get { return "FiaCR"; } }


		protected override Board<Vector2i> CreateNewBoard()
		{
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

		public void AdvanceTurn()
		{
			if (CurrentTurn.Value == Board.Player_Humans)
			{
				if (IsJuliaTurn)
				{
					IsJuliaTurn = false;
					MovesLeft = Board.NBillyMovesByBoardSize[Size];
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
			Size = (Board.Sizes)PlayerPrefs.GetInt(PlayerPrefs_Size, (int)Size);
			RNG = new System.Random(PlayerPrefs.GetInt(PlayerPrefs_Seed,
													   UnityEngine.Random.Range(0, int.MaxValue)));

			Screen.orientation = ScreenOrientation.Portrait;

			base.Awake();
		}
	}
}