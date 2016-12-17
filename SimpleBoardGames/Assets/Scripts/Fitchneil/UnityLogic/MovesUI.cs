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
					List<Movement> movements =
						currentPiece.TheBoard.GetMoves(currentPiece).Cast<Movement>().ToList();
					activeSprites = SpritePool.Instance.AllocateSprites(movements.Count,
																		MoveOptionSprite);

					//Set up the color and position of each sprite,
					//    and let the user click them to select the movement.
					for (int i = 0; i < movements.Count; ++i)
					{
						activeSprites[i].color = (movements[i].Captures.Count == 0 ?
													  NormalMoveColor :
													  GoodMoveColor);
						activeSprites[i].transform.position = Board.ToWorld(movements[i].Pos.Value);

						AddMoveSelectionResponder(movements[i], activeSprites[i].gameObject);
					}
				}
			}
		}
		private Piece currentPiece = null;

		private Transform tr;
		private List<SpriteRenderer> activeSprites = new List<SpriteRenderer>();


		protected override void Awake()
		{
			base.Awake();
			tr = transform;
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();
			CurrentPiece = null;
		}

		private void AddMoveSelectionResponder(Movement move, GameObject go)
		{
			var input = go.AddComponent<InputResponder>();
			input.OnStopClick += (_input, mPos) =>
			{
				CurrentPiece = null;
				move.ApplyMove();
			};
		}
	}
}
