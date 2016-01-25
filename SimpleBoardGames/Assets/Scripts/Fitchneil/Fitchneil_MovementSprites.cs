using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// An expandable collection of GameObjects that represent movement indicators.
/// </summary>
public class Fitchneil_MovementSprites : Singleton<Fitchneil_MovementSprites>
{
	private List<Transform> usedSprites = new List<Transform>(),
							unusedSprites = new List<Transform>();


	public List<Transform> AllocateSprites(List<Fitchneil_Board.Move> moves)
	{
		while (unusedSprites.Count < moves.Count)
		{
			unusedSprites.Add(Utilities.CreateSprite(Fitchneil_Art.Instance.Movement, "Movement",
													 null, null, 1).transform);
			unusedSprites[unusedSprites.Count - 1].gameObject.SetActive(false);
		}

		List<Transform> used = new List<Transform>();
		for (int i = 0; i < moves.Count; ++i)
		{
			//Set up the GameObject for this move.
			unusedSprites[0].gameObject.SetActive(true);
			unusedSprites[0].position = new Vector3(moves[i].Pos.x + 0.5f, moves[i].Pos.y + 0.5f, 0.0f);
			unusedSprites[0].GetComponent<SpriteRenderer>().color =
				(moves[i].IsSpecial ?
					Fitchneil_Art.Instance.SpecialMovement :
					Fitchneil_Art.Instance.NormalMovement);
			
			//Set up the various lists storing used/unused sprites.
			used.Add(unusedSprites[0]);
			usedSprites.Add(unusedSprites[0]);
			unusedSprites.RemoveAt(0);
		}

		return used;
	}
	public void	DeallocateSprites(List<Transform> sprites)
	{
		foreach (Transform tr in sprites)
		{
			usedSprites.Remove(tr);
			unusedSprites.Add(tr);

			tr.gameObject.SetActive(false);
		}
	}
}