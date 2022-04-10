using UnityEngine;
using UnityEngine.UI;
using Lean.Pool;

public class SpriteAnimator : MonoBehaviour
{
	[SerializeField] bool Persist;
	[SerializeField] bool PlayOnAwake;
	[SerializeField] bool isImage;
	[SerializeField] float FPS;
    [SerializeField] bool Loop;
	[SerializeField] Sprite[] SpritesInOrder = new Sprite[1];
	
	Image imageRenderer;
	SpriteRenderer spriteRenderer;
	Effect spriteEffect;
	bool started;
	int sequence;
	int endClip;
	Transform cachedTransform;
	
	float actualFPS;
	float frame;
	
	void Awake()
	{
		cachedTransform = transform;
		actualFPS = FPS/100;
		if (isImage)
		{
			imageRenderer = GetComponent<Image>();
		} else {
			spriteRenderer = GetComponent<SpriteRenderer>();
		}
		spriteEffect = GetComponent<Effect>();
		endClip = (SpritesInOrder.Length - 1);
		if (PlayOnAwake) Spawned();
	}
	
	public void Spawned()
	{
		if (isImage)
		{
			imageRenderer.enabled = true;
		} else {
			spriteRenderer.enabled = true;
		}
		if (spriteEffect != null) spriteEffect.BeginWithoutReset();
		started = true;
		sequence = 0;
		if (isImage)
		{
			imageRenderer.sprite = SpritesInOrder[sequence];
		} else {
			spriteRenderer.sprite = SpritesInOrder[sequence];
		}
	}
	
	void Update()
	{
		if (!started) return;
		
		if (spriteEffect != null) spriteEffect.Update_Effect();
		
		frame += Time.deltaTime;
		//if (Time.frameCount % FPS == 0)
		if ( frame >= actualFPS )
		{
			frame = 0;
			sequence++;
			
			if (sequence > endClip)
			{
				if (Loop)
				{
					sequence = 0;
				} else {
					if (Persist)
					{
						if (isImage)
						{
							imageRenderer.enabled = false;
						} else {
							spriteRenderer.enabled = false;
						}
					} else {
						LeanPool.Despawn(gameObject);
					}
					return;
				}
			}
			
			if (isImage)
			{
				imageRenderer.sprite = SpritesInOrder[sequence];
			} else {
				spriteRenderer.sprite = SpritesInOrder[sequence];
			}
		}
	}
}
