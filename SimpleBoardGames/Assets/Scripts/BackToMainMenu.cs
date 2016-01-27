using System;
using UnityEngine;


/// <summary>
/// Returns to the main menu scene if the back button is pressed twice in quick succession.
/// </summary>
public class BackToMainMenu : MonoBehaviour
{
	public float MaxPressTime = 0.25f;

	private float lastPressed = 99999.0f;


	void Update()
	{
		lastPressed += Time.deltaTime;

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