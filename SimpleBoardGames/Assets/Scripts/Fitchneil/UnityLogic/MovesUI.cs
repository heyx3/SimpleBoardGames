using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Fitchneil.UnityLogic
{
	public class MovesUI : Singleton<MovesUI>
	{
		public Sprite MoveOptionSprite;
		public Color NormalMoveColor = Color.yellow,
					 GoodMoveColor = Color.green;


		public Piece CurrentPiece
		{
			get { return currentPiece; }
			set
			{
				if (currentPiece != null)
				{
					SpritePool.Instance.DeallocateSprites(activeSprites);
					activeSprites.Clear();
				}

				currentPiece = value;
				if (currentPiece != null)
				{
					//Allocate one "MoveOption" sprite for each possible movement.
					List<Action_Move> movements =
						currentPiece.TheBoard.GetActions(currentPiece).Cast<Action_Move>().ToList();
					activeSprites = SpritePool.Instance.AllocateSprites(movements.Count,
																		MoveOptionSprite);

					//Set up the color and position of each sprite,
					//    and let the user click them to select the movement.
					for (int i = 0; i < movements.Count; ++i)
					{
						activeSprites[i].color = movements[i].GetIsSpecial() ?
												     GoodMoveColor :
													 NormalMoveColor;
						activeSprites[i].transform.position = Board.ToWorld(movements[i].EndPos);

						AddMoveSelectionResponder(movements[i], activeSprites[i].gameObject);
					}
				}
			}
		}
		private Piece currentPiece = null;
		
		private List<SpriteRenderer> activeSprites = new List<SpriteRenderer>();

		
		protected override void OnDestroy()
		{
			base.OnDestroy();
			CurrentPiece = null;
		}

		private void AddMoveSelectionResponder(Action_Move move, GameObject go)
		{
			var collider = go.AddComponent<BoxCollider2D>();

			var input = go.AddComponent<InputResponder>();
			input.OnStopClick += (_input, mPos) =>
			{
				CurrentPiece = null;
				move.DoAction();
			};
		}
	}
}
