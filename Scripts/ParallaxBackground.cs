using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] Vector2 parallaxEffectMultiplier;
    [SerializeField] bool infiniteHorizontal;
    [SerializeField] bool infiniteVertical;
	[SerializeField] Transform cameraTransform;
	
	private Transform pTransform;
	private Vector3 lastCameraPosition;
	private float textureUnitSizeX;
	
    void Start()
    {
        pTransform = transform;
		
		lastCameraPosition = cameraTransform.position;
		Sprite sprite = GetComponent<SpriteRenderer>().sprite;
		Texture2D texture = sprite.texture;
		textureUnitSizeX = texture.width / sprite.pixelsPerUnit;
    }


    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
		pTransform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier.x , deltaMovement.y * parallaxEffectMultiplier.y, 0f);
		lastCameraPosition = cameraTransform.position;
		
		if (infiniteHorizontal)
		{
			if (Mathf.Abs( cameraTransform.position.x - pTransform.position.x ) >= textureUnitSizeX)
			{
				float offsetPositionX = (cameraTransform.position.x - pTransform.position.x) % textureUnitSizeX;
				pTransform.position = new Vector3(cameraTransform.position.x + offsetPositionX, pTransform.position.y);
			}
		}
		if (infiniteVertical)
		{
			if (Mathf.Abs( cameraTransform.position.y - pTransform.position.y ) >= textureUnitSizeX)
			{
				float offsetPositionY = (cameraTransform.position.y - pTransform.position.y) % textureUnitSizeX;
				pTransform.position = new Vector3(pTransform.position.x, cameraTransform.position.y + offsetPositionY);
			}
		}
    }
}
