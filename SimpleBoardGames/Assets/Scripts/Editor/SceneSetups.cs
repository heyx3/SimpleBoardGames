using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public static class SceneSetups
{
	[MenuItem("Board Games/Create Fitchneil Controller Object")]
	public static void CreateFitchneilController()
	{
		GameObject go = new GameObject("Fitchneil Controller");

		go.AddComponent<Fitchneil.Board>();
		go.AddComponent<Fitchneil.Constants>();
		go.AddComponent<Fitchneil.StateMachine>();
		go.AddComponent<BoardGames.MovementIndicators>();
		go.AddComponent<BoardGames.InputManager>();
	}
}