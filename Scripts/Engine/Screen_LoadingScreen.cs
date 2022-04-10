using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Screen_LoadingScreen : MonoBehaviour
{
	[SerializeField] Image skeletonLoader;
	
    public void LoadLevel( int sceneIndex )
	{
		StartCoroutine( LoadAsynchronously(sceneIndex) );
		skeletonLoader.enabled = true;
	}
	
	IEnumerator LoadAsynchronously( int sceneIndex )
	{
		AsyncOperation operation = SceneManager.LoadSceneAsync( sceneIndex );
		
		while ( !operation.isDone )
		{
			yield return null;
		}
		
		SceneManager.LoadScene(sceneIndex, LoadSceneMode.Additive);
	}
}
