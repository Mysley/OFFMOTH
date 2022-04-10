using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI_Basic : MonoBehaviour
{
	[SerializeField] Transform groundDetection;
	[SerializeField] LayerMask mask;
	
	Transform eTransform;
	Unit unit;
	
	bool movingRight;
	const float groundDistance = 0.4f;
	const float wallDistance = 1f;
	
	static readonly Vector3 left = new Vector3(0, 180, 0);
	static readonly Vector3 right = Vector3.zero;
	
	void Start()
	{
		eTransform = transform;
		unit = GetComponent<Unit>();
		
		movingRight = (eTransform.eulerAngles.y == 0f) ? true : false;
	}
	
	public void UpdateAI()
	{
		if (unit.PlayerControlled) return;
		if (!unit.grounded) return;
		
		unit.moveDestination = Vector2.right * unit.MoveSpeed * Time.deltaTime;
		Vector3 wallPos = eTransform.position;
		wallPos.y -= 0.5f;
		
		RaycastHit2D groundInfo = Physics2D.Raycast( groundDetection.position, Vector2.down, groundDistance, mask);
		RaycastHit2D wallInfo = Physics2D.Raycast( wallPos, eTransform.right, wallDistance, mask);
		
		if (!groundInfo.collider || wallInfo.collider)
		{
			RotateAI();
		}
	}
	
	public void RotateAI()
	{
		if (movingRight)
		{
			eTransform.eulerAngles = left;
			movingRight = false;
		} else {
			eTransform.eulerAngles = right;
			movingRight = true;
		}
	}
}
