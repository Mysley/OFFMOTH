using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
	[SerializeField] float speed;
	[SerializeField] float hoverAmount;
	
	float startingY;
	bool up;
	
	void Awake()
	{
		startingY = transform.localPosition.y;
		
		Vector3 hoverPos = transform.localPosition;
		hoverPos.y += hoverAmount;
		transform.localPosition = hoverPos;
		
		up = true;
	}
	
	void Update()
	{
		if (Time.frameCount % speed == 0)
		{
			Vector3 hoverPos = transform.localPosition;
			hoverPos.y = startingY;
			
			if (up)
			{
				up = false;
				hoverPos.y -= hoverAmount;
			} else {
				up = true;
				hoverPos.y += hoverAmount;
			}
			transform.localPosition = hoverPos;
		} 
	}
}
