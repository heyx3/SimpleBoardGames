using System;
using System.Collections.Generic;
using UnityEngine;

using Board = Fitchneil_Board;


public class Fitchneil_Controller : Singleton<Fitchneil_Controller>
{
	public SpriteRenderer AttackersText, DefendersText;
	public SpriteRenderer BoardSprite;

	private Color attackersTextCol, defendersTextCol;

	public bool IsAttackersTurn
	{
		get { return isAttackersTurn; }
		private set
		{
			if (value)
			{
				AttackersText.color = Fitchneil_Art.Instance.GreyTextColor;
				DefendersText.color = defendersTextCol;
			}
			else
			{
				AttackersText.color = attackersTextCol;
				DefendersText.color = Fitchneil_Art.Instance.GreyTextColor;
			}

			isAttackersTurn = value;
		}
	}
	private bool isAttackersTurn;


	protected override void Awake()
	{
		base.Awake();

		attackersTextCol = AttackersText.color;
		defendersTextCol = DefendersText.color;
	}

	void Start()
	{
		Screen.orientation = ScreenOrientation.Landscape;

		StartCoroutine(GameLogicCoroutine());
	}



	//The following stuff is used in the game logic coroutine.

	private Vector2i? clickedPiece = null;

	private List<Board.Move> movements = null;
	private int clickedMove = -1;

	private bool isGameOver = false;
	/// <summary>
	/// Ends the game. The winner is whoever's turn it currently is.
	/// </summary>
	public void EndGame() { isGameOver = true; }


	private System.Collections.IEnumerator GameLogicCoroutine()
	{
		yield return null;

		//Will be flipped to "false" at the beginning of the main game loop.
		IsAttackersTurn = true;
		
		yield return null;

		//Set up piece click responders for when the player is selecting a piece to move.
		foreach (Vector2i pos in Board.Instance.GetPieces(Board.Spaces.Defender))
			AddPieceDelegate(pos, Board.Instance.GetPiece(pos), false);
		foreach (Vector2i pos in Board.Instance.GetPieces(Board.Spaces.King))
			AddPieceDelegate(pos, Board.Instance.GetPiece(pos), false);
		foreach (Vector2i pos in Board.Instance.GetPieces(Board.Spaces.Attacker))
			AddPieceDelegate(pos, Board.Instance.GetPiece(pos), true);

		//Set up movement click responder, for when the player is selecting a position to move to.
		SpriteSelector.Instance.Objects.Add(BoardSprite, (r, p) =>
			{
				if (movements != null && clickedMove == -1)
				{
					//Get the board position that was clicked on.
					Vector2i pI = new Vector2i((int)p.x, (int)p.y);
					pI.x = Mathf.Clamp(pI.x, 0, Board.BoardSize - 1);
					pI.y = Mathf.Clamp(pI.y, 0, Board.BoardSize - 1);

					//If this position is a valid movement, use it.
					if (pI != clickedPiece.Value)
					{
						for (int i = 0; i < movements.Count; ++i)
						{
							if (movements[i].Pos == pI)
							{
								clickedMove = i;
								return true;
							}
						}
					}

					//Was not a valid place to move, so deselect the current piece.
					clickedPiece = null;
				}

				return false;
			});


		//Run the main game loop.
		List<Transform> moveSprites;
		while (true)
		{
			//Reset game state.
			IsAttackersTurn = !IsAttackersTurn;
			clickedPiece = null;
			movements = null;
			clickedMove = -1;

			yield return new WaitForSeconds(0.5f);

		WaitForClickedPiece:

			//Wait for a piece to be clicked on.
			while (!clickedPiece.HasValue)
				yield return null;

			//Show the player all possible movements for his piece.
			movements = Board.Instance.GetAllowedMoves(clickedPiece.Value);
			moveSprites = Fitchneil_MovementSprites.Instance.AllocateSprites(movements);


			//Wait for a movement option to be clicked on.
			while (clickedMove == -1)
			{
				//If the clicked piece became de-selected,
				//    go back and wait for another piece to be selected.
				if (!clickedPiece.HasValue)
				{
					Fitchneil_MovementSprites.Instance.DeallocateSprites(moveSprites);
					movements = null;
					goto WaitForClickedPiece;
				}

				yield return null;
			}

			Fitchneil_MovementSprites.Instance.DeallocateSprites(moveSprites);

			//Get whether this is a game-ending move.
			if (IsAttackersTurn)
			{
				//The attackers win if they capture the king.
				foreach (Vector2i p in Board.Instance.GetCapturesFromMove(clickedPiece.Value,
																		  movements[clickedMove].Pos))
				{
					if (Board.Instance.Board[p.x, p.y] == Board.Spaces.King)
					{
						EndGame();
						break;
					}
				}
			}
			else
			{
				//The defenders win if the king escapes.
				Vector2i pieceP = clickedPiece.Value,
						 moveP = movements[clickedMove].Pos;
				if (Board.Instance.Board[pieceP.x, pieceP.y] == Board.Spaces.King &&
					(moveP.x == 0 || moveP.y == 0 ||
					 moveP.x == Board.BoardSize - 1 || moveP.y == Board.BoardSize - 1))
				{
					EndGame();
				}

				//TODO: Also end the game if no attackers are left after this move is done.
			}

			//Move the piece to the new position.
			bool doneMoving = false;
			Board.Instance.MovePiece(clickedPiece.Value, movements[clickedMove].Pos,
									 (t) => { doneMoving = true; });
			while (!doneMoving)
				yield return null;
			
			//See if the game just ended.
			if (isGameOver)
			{
				//TODO: Do game-ending stuff.
				Debug.Log("Game Over!");
			}

			yield return null;
		}
	}
	private void AddPieceDelegate(Vector2i pos, SpriteRenderer spr, bool isAttacker)
	{
		SpriteSelector.Instance.Objects.Add(spr, (r, p) =>
			{
				if (movements == null && !clickedPiece.HasValue && IsAttackersTurn == isAttacker)
				{
					clickedPiece = pos;
				}

				return false;
			});
	}
}