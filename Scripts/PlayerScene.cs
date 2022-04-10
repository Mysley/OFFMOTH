using UnityEngine;
using Cinemachine;
using TMPro;

public class PlayerScene : MonoBehaviour
{
    [Header("Gameplay and Scene")]
	public CinemachineVirtualCamera CinemachineCamera;
	[SerializeField] CinemachineImpulseSource CinemachineImpulse;
	public Canvas Quitter;
	public Canvas HelpCanvas;
	public Canvas LostCanvas;
	public Canvas WinCanvas;
	public TextMeshProUGUI LoseScore;
	
	public void ShakeScreen()
	{
		CinemachineImpulse.GenerateImpulse();
	}
}
