using System;
using UnityEngine;


/// <summary>
/// Returns to the main menu scene if the back button or escape key
///     is pressed twice in quick succession.
/// </summary>
public class BackToMainMenu : Singleton<BackToMainMenu>
{
	public float MaxPressTime = 0.25f;

	private float lastPressed = 99999.0f;


	private void Update()
	{
		lastPressed += Time.deltaTime;

		//Note that the escape key corresponds to the back button on Android.
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (lastPressed <= MaxPressTime)
			{
				UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
			}
			else
			{
				lastPressed = 0.0f;
			}
		}
	}
}