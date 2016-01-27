using System;
using UnityEngine;


public class MainMenuController : MonoBehaviour
{
	void Awake()
	{
		Screen.orientation = ScreenOrientation.Landscape;
		Input.backButtonLeavesApp = true;
	}
	void OnDestroy()
	{
		Input.backButtonLeavesApp = false;
	}


	public void OnButton_Fitchneil()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene("Fitchneil");
	}
	public void OnButton_TronsTour()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene("TronsTour");
	}
}