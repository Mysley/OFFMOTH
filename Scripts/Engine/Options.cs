using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Options : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
	[SerializeField] Slider slider_Music;
    [SerializeField] Slider slider_Sound;
	
	bool init = true;
	
	//public static readonly string PlayerPrefs_Volume_Music = "Volume_Music";
	//public static readonly string PlayerPrefs_Volume_Sound = "Volume_Sound";
	static readonly string mixerVolume_Sound = "mixerVolume_Sound";
	static readonly string mixerVolume_Music = "mixerVolume_Music";
	
	void Awake()
	{
		//slider_Music.value = PlayerPrefs.GetFloat( PlayerPrefs_Volume_Music, 0 );
		//slider_Sound.value = PlayerPrefs.GetFloat( PlayerPrefs_Volume_Sound, 0 );
		init = false;
	}
	
	public void Sound_Volume( float volume )
	{
		if (init) return;
		audioMixer.SetFloat(mixerVolume_Sound, volume);
		//PlayerPrefs.SetFloat(PlayerPrefs_Volume_Sound, volume);
		//PlayerPrefs.Save();
	}
	public void Music_Volume( float volume )
	{
		if (init) return;
		audioMixer.SetFloat(mixerVolume_Music, volume);
		//PlayerPrefs.SetFloat(PlayerPrefs_Volume_Music, volume);
		//PlayerPrefs.Save();
	}
}
