using System;
using UnityEngine;


public class Fitchneil_Art : Singleton<Fitchneil_Art>
{
	public Sprite King, Defender, Attacker;
	public Sprite Movement;

	public float MyTurnMultiplier = 0.25f;

	public Color NormalMovement = Color.yellow,
				 SpecialMovement = Color.green;

	public GameObject DestroyedPieceEffectPrefab;
}