using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Screen_BlackFade : MonoBehaviour
{
	[SerializeField] bool FadeIn;
    [SerializeField] int NewScene = -1;
    [SerializeField] Screen_LoadingScreen LevelLoader;
    [SerializeField] Screen_BlackFade Unfader;
	[SerializeField] Transform Player;
	[SerializeField] UnityEvent AfterFadingEvents;
	
	Image image;
	bool started;
	bool loaded;
	const float modder = 0.02f;
	const float maxAlpha = 1f;
	
	public event Action onLoadNewScene;
	public void LoadNewScene()
	{
		if (onLoadNewScene != null)
		{
			onLoadNewScene();
		}
	}
	
	void Start()
	{
		image = GetComponent<Image>();
	}
	
	void Update()
	{
		if (!started) return;
		
		Color newColor = image.color;
		
		if (FadeIn)
		{
			if (newColor.a > 0f)
			{
				newColor.a -= modder;
			} else if (newColor.a < 0f)
			{
				newColor.a = 0f;
			}
			
			if (newColor.a == 0f && !loaded)
			{
				if (Player != null)
				{
					loaded = true;
					AfterFadingEvents.Invoke();
					Invoke("Unload", 1f);
				}
			}
			
		} else {
			if (newColor.a < maxAlpha)
			{
				newColor.a += modder;
			} else if (newColor.a > maxAlpha)
			{
				newColor.a = maxAlpha;
			}
			
			if (newColor.a == maxAlpha && !loaded)
			{
				if (NewScene != -1)
				{
					loaded = true;
					Invoke("GoNewScene", 1f);
				} else if (Player != null)
				{
					AfterFadingEvents.Invoke();
				}
			}
		}
		
		image.color = newColor;
	}
	
	void Unload()
	{
		if (Unfader != null) Unfader.FadeScreen();
	}
	
	void GoNewScene()
	{
		LoadNewScene();
		LevelLoader.LoadLevel( NewScene );
	}
	
	public void FadeScreen()
	{
		started = true;
	}public void InvokedFadeScreen()
	{
		Invoke("FadeScreen", 1f);
	}
}
