using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace FiaCR.UnityLogic
{
	public class FCR_MovesUI_Julia : Singleton<FCR_MovesUI_Julia>
	{
		public Sprite MoveOptionSprite;
		public Color NormalMoveColor = Color.yellow,
					 GoodMoveColor = Color.green;


		public void Init()
		{
			SpritePool.Instance.DeallocateSprites(activeSprites);
			activeSprites.Clear();

			Board board = (Board)GameMode.FCR_Game_Offline.Instance.TheBoard;
			List<Action_PlaceFriendly> placements =
				board.GetActions(Board.Player_Humans).Cast<Action_PlaceFriendly>().ToList();
			activeSprites = SpritePool.Instance.AllocateSprites(placements.Count, MoveOptionSprite);

			for (int i = 0; i < placements.Count; ++i)
			{
				activeSprites[i].color = placements[i].IsSpecial ?
											 GoodMoveColor :
											 NormalMoveColor;
				activeSprites[i].transform.position = board.ToWorld(placements[i].Pos);

				AddPlaceSelectionResponder(placements[i], activeSprites[i].gameObject);
			}
		}
		public void DeInit()
		{
			SpritePool.Instance.DeallocateSprites(activeSprites);
			activeSprites.Clear();
		}
		
		private List<SpriteRenderer> activeSprites = new List<SpriteRenderer>();


		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		private void AddPlaceSelectionResponder(Action_PlaceFriendly placement, GameObject go)
		{
			var collider = go.AddComponent<BoxCollider2D>();
			collider.size = Vector2.one;

			var input = go.AddComponent<InputResponder>();
			input.OnStopClick += (_input, mPos) =>
			{
				placement.DoAction();
			};
		}
	}
}