using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Returns to the Main Menu scene after a certain amount of time.
/// </summary>
public class TimerToMainMenu : MonoBehaviour
{
	public float TimeToMainMenu = 2.5f;

	void Update()
	{
		TimeToMainMenu -= Time.deltaTime;

		if (TimeToMainMenu <= 0.0f)
			UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
	}
}