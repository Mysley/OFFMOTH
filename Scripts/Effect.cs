using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
	[SerializeField] bool hideOnFinish;
	[SerializeField] float duration;
	[SerializeField] Vector3 movementPerFrame;
	[SerializeField] Vector3 scalePerFrame;
	SpriteRenderer spriteRenderer;
	bool started;
	float startTime;
	Transform cachedTransform;
	
	void Awake()
	{
		cachedTransform = transform;
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (hideOnFinish) spriteRenderer.enabled = false;
	}
	
	public void Begin()
	{
		if (hideOnFinish) spriteRenderer.enabled = true;
		started = true;
		startTime = duration;
		
		cachedTransform.localPosition = Vector3.zero;
		cachedTransform.localScale = Vector3.one;
		cachedTransform.localRotation = Quaternion.identity;
	}
	public void BeginWithoutReset()
	{
		started = true;
		startTime = duration;
	}
	
	public void Update_Effect()
	{
		if (!started) return;
		
		cachedTransform.localPosition += movementPerFrame * Time.deltaTime;
		cachedTransform.localScale += scalePerFrame * Time.deltaTime;
		
		startTime -= Time.deltaTime;
		
		if (startTime < 0f)
		{
			started = false;
			startTime = 0f;
			if (hideOnFinish) spriteRenderer.enabled = false;
		}
	}
}
