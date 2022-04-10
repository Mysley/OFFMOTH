using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;

public class Projectile : MonoBehaviour
{
	public float Damage;
	public Vector3 Rotating;
	[SerializeField] float limitProjectile;
	[SerializeField] float DamageArea;
	[SerializeField] Transform impactEffect;
	[SerializeField] AudioClip impactSound;
	[SerializeField] LayerMask UnitLayer;
	[Space(10)]
	[SerializeField] Transform audioPrefab;
	
	Unit source;
	bool hasHit;
	float lifespan;
	string proTag;
	static readonly string tag_Player = "Player";
	
	public void Spawn( Unit thrower, Vector2 force )
	{
		source = thrower;
		GetComponent<Rigidbody2D>().AddForce( force );
		GetComponent<AudioSource>().Play();
		lifespan = 0f;
		hasHit = false;
		proTag = gameObject.tag;
	}
	
	void Update()
	{
		lifespan += Time.deltaTime;
		if (lifespan >= limitProjectile)
		{
			LeanPool.Despawn(gameObject);
		}
	}
	void FixedUpdate()
	{
		if (Rotating != Vector3.zero)
		{
			transform.eulerAngles += Rotating;
		}
	}
	
	void OnTriggerEnter2D( Collider2D col )
	{
		if (hasHit) return;
		
		if (col.gameObject.layer == 6)
		{
			if ( col.gameObject.CompareTag(tag_Player) )
			{
				Unit pTarget = col.GetComponent<PlayerController>().currentUnit;
				if ( pTarget == source ) return;
				if ( !Utils.CheckTeams( source, pTarget)) return;
				pTarget.TakeDamage( source, Damage, null );
				DestroyProjectile();
				return;
			}
			
			Unit target = col.GetComponent<Unit>();
			if ( target == null ) return;
			if ( target == source ) return;
			if ( !Utils.CheckTeams( source, target)) return;
			
			if (DamageArea > 0f)
			{
				AoEdamage();
			} else {
				target.TakeDamage( source, Damage, null );
			}
			DestroyProjectile();
			
		} else if (col.gameObject.layer == 0)
		{
			if (col.gameObject.CompareTag(proTag)) return;
			
			if (DamageArea > 0f) AoEdamage();
			
			DestroyProjectile();
		}
	}
	
	void AoEdamage()
	{
		Collider2D[] hit_enemyColliders = Physics2D.OverlapCircleAll( transform.position, DamageArea, UnitLayer );
		for (int i = 0; i < hit_enemyColliders.Length; i++)
		{
			if ( hit_enemyColliders[i].gameObject.CompareTag(tag_Player) )
			{
				Unit pTarget = hit_enemyColliders[i].GetComponent<PlayerController>().currentUnit;
				if (pTarget != source) pTarget.TakeDamage( source, Damage, null );
				
			} else {
				Unit target = hit_enemyColliders[i].GetComponent<Unit>();
				if (Utils.CheckTeams( source, target))
				{
					target.TakeDamage( source, Damage, null );
				}
			}
		}
	}
	
	void DestroyProjectile()
	{
		hasHit = true;
		ProjectileAudio.Play( audioPrefab, transform.position, impactSound );
		if (impactEffect != null)
		{
			Transform impact = LeanPool.Spawn( impactEffect, transform.position, Quaternion.identity );
			impact.GetComponent<SpriteAnimator>().Spawned();
		}
		if (gameObject.activeSelf) LeanPool.Despawn(gameObject);
	}
	
	//===============================================================================
	//======================= S P A W N   R E M O T E L Y ===========================
	//===============================================================================
	
	static Transform projectile_static_Transform;
	static Rigidbody projectile_static_Rigidbody;
	
	public static void Create( Unit source, Vector3 pos, Transform projectile, Vector2 force  )
	{
		projectile_static_Transform = LeanPool.Spawn( projectile, pos, source.Utransform.rotation );
		projectile_static_Transform.GetComponent<Projectile>().Spawn( source, force );
	}
}
