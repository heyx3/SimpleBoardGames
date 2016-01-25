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




	private Vector2i? clickedPiece = null;

	private List<Board.Move> movements = null;
	private int clickedMove = -1;

	private System.Collections.IEnumerator GameLogicCoroutine()
	{
		//Will be flipped to "false" at the beginning of the main game loop.
		IsAttackersTurn = true;

		yield return null;

		//Set up piece click responders.
		foreach (Vector2i pos in Board.Instance.GetPieces(Board.Spaces.Defender))
			AddPieceDelegate(pos, Board.Instance.GetPiece(pos), false);
		foreach (Vector2i pos in Board.Instance.GetPieces(Board.Spaces.King))
			AddPieceDelegate(pos, Board.Instance.GetPiece(pos), false);
		foreach (Vector2i pos in Board.Instance.GetPieces(Board.Spaces.Attacker))
			AddPieceDelegate(pos, Board.Instance.GetPiece(pos), true);

		//Set up movement click responders.
		SpriteSelector.Instance.Objects.Add(BoardSprite, (r, p) =>
			{
				if (movements != null)
				{
					Vector2i pI = new Vector2i((int)p.x, (int)p.y);
					pI.x = Mathf.Clamp(pI.x, 0, Board.BoardSize - 1);
					pI.y = Mathf.Clamp(pI.y, 0, Board.BoardSize - 1);

					for (int i = 0; i < movements.Count; ++i)
					{
						if (movements[i].Pos == pI)
						{
							clickedMove = i;
							return true;
						}
					}
				}

				return false;
			});


		//Run the main game loop.
		while (true)
		{
			//Reset game state.
			IsAttackersTurn = !IsAttackersTurn;
			clickedPiece = null;
			movements = null;
			clickedMove = -1;

			yield return new WaitForSeconds(0.5f);

			//Wait for a piece to be clicked on.
			while (!clickedPiece.HasValue)
				yield return null;

			//TODO: Highlight the piece and movement opportunities. Create a list of movement sprites to be enabled at will.

			//Wait for a movement option to be clicked on.
			while (clickedMove == -1)
				yield return null;

			//TODO: Animate the movement.

			yield return new WaitForSeconds(2.0f);

			//TODO: Calculate new state.

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