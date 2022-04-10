using UnityEngine;
using Lean.Pool;

public class ProjectileAudio : MonoBehaviour
{
	AudioSource audio_Source;
	float audio_Duration;
	
    public void Instanced( AudioClip clip )
	{
		audio_Source = GetComponent<AudioSource>();
		audio_Source.clip = clip;
		audio_Source.Play();
		
		audio_Duration = clip.length;
		LeanPool.Despawn(gameObject, audio_Duration);
	}
	
	static Transform static_Transform;
	static ProjectileAudio static_Audio;
	
	public static void Play( Transform audioPrefab, Vector3 position, AudioClip clip )
	{
		static_Transform = LeanPool.Spawn( audioPrefab, position, Quaternion.identity );
		static_Audio = static_Transform.GetComponent<ProjectileAudio>();
		static_Audio.Instanced( clip );
	}
}
