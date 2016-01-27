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
		go.AddComponent<SpritePool>();
		go.AddComponent<BoardGames.InputManager>();
		go.AddComponent<BackToMainMenu>();
	}

	[MenuItem("Board Games/Set Up Tron's Tour (select the two pieces first)")]
	public static void CreateTronsTour()
	{
		GameObject go = new GameObject("Tron's Tour Controller");

		go.AddComponent<TronsTour.Constants>();
		go.AddComponent<TronsTour.Board>();
		go.AddComponent<TronsTour.StateMachine>();
		go.AddComponent<SpritePool>();
		go.AddComponent<BoardGames.InputManager>();
		go.AddComponent<BackToMainMenu>();

		if (Selection.gameObjects.Length != 2)
		{
			EditorUtility.DisplayDialog("Error", "You must select the 2 game pieces and nothing else!",
										"OK");
			return;
		}

		for (int i = 0; i < 2; ++i)
		{
			Selection.gameObjects[i].AddComponent<SpriteRenderer>();
			Selection.gameObjects[i].AddComponent<BoxCollider2D>();
			Selection.gameObjects[i].AddComponent<TronsTour.Piece>();
		}
	}
}