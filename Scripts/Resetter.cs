using UnityEngine;
using UnityEngine.SceneManagement;

public class Resetter : MonoBehaviour
{
    [SerializeField] PlayerController thePlayer;
	
	public void ResetToMenu()
	{
		thePlayer.Reset();
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}
