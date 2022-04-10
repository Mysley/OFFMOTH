using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;
using DATA;

public class Unit : MonoBehaviour
{
	//==========================================================================
	//==========================================================================
	[Header("Data")]
	public bool ScenePlaced;
	public bool PlayerControlled;
	public UnitRace Race;
	public Transform Prefab;
	public Sprite BaseSprite;
	public Sprite AttackIcon;
	public string AttackName;
	[Space(10)]
	
	[Header("References")]
	[SerializeField] LayerMask UnitLayer;
	[SerializeField] ParticleSystem FX_WalkTrail;
	[SerializeField] ParticleSystem FX_Hurt;
	[SerializeField] ParticleSystem FX_Death;
	[SerializeField] Effect FX_MeleeSweep;
	public SpriteRenderer Shadow;
	[Space(10)]
	
	[Header("Stats")]
    public float MaxHealth;
	public float AttackDamage;
	public float AttackSpeed;
	public float AttackRange;
	public float MoveSpeed;
	[Space(10)]
	
	[Header("Actions")]
	public bool DelayedAttack;
	public bool Ranged;
	public Transform RangeAttack;
	public float RangeSpeed;
	[SerializeField] Vector3 RangeProjectileOffset;
	[Space(10)]
	
	[Header("Audio")]
	[SerializeField] AudioClip SwingSound;
	[SerializeField] AudioClip HitSound;
	public AudioClip LandSound;
	//==========================================================================
	//==========================================================================
	
	PlayerController controller;
	EnemyAI_Basic ai;
	
	[HideInInspector] public Transform Utransform;
	[HideInInspector] public Animator Uanimator;
	[HideInInspector] public BoxCollider2D Ucollider;
	[HideInInspector] public Rigidbody2D Urigidbody;
	[HideInInspector] public AudioSource Uaudio;
	[HideInInspector] public SpriteRenderer Usprite;
	
	[HideInInspector] public bool Dead;
	[HideInInspector] public bool Staggered;
	[HideInInspector] public bool pressed_Attack;
	[HideInInspector] public bool pressed_Ability;
	
	[HideInInspector] public Vector3 moveDestination;
	[HideInInspector] public bool grounded;
	
	[HideInInspector] public float currentHealth;
	[HideInInspector] public float attackRate;
	//==========================================================================
	//==========================================================================
	
	Vector3 attackPosition;
	int hash_IsWalking;
	
	bool iswalking;
	float staggerTime;
	float combatTime;
	
	WaitForSeconds deathTimer = new WaitForSeconds(2f);
	const float _attackRateReturn = 0.5f;
	const float _jumpHeight = 5f;
	const float _jumpForce = 500f;
	const float _groundingRayBase = -0.10f;
	const float _groundingRayDistance = -0.10f;
	const float _groundingRayMax = 0.5f;
	static readonly string tag_Player = "Player";
	static readonly string anim_attack = "Attack";
	static readonly Color32 c_normal = new Color32( 255, 255, 255, 255);
	static readonly Color32 c_stealth = new Color32( 255, 255, 255, 100);
	static readonly Color32 c_damaged = new Color32( 255, 0, 0, 255);
	static readonly Color32 c_stealthdamaged = new Color32( 255, 0, 0, 100);
	
	//=================================================================================
	//=========== Mono
	//=================================================================================
	
	void Start() {
		Utransform = transform;
		Uanimator = GetComponent<Animator>();
		Usprite = GetComponent<SpriteRenderer>();
		Urigidbody = GetComponent<Rigidbody2D>();
		Ucollider = GetComponent<BoxCollider2D>();
		
		if (PlayerControlled)
		{
			controller = GetComponentInParent<PlayerController>();
			Ucollider.enabled = false;
			Urigidbody.simulated  = false;
		} else {
			ai = GetComponent<EnemyAI_Basic>();
		}
		
		Uaudio = GetComponent<AudioSource>();
		
		hash_IsWalking = Animator.StringToHash("isWalking");
		
		currentHealth = MaxHealth;
		attackRate = 0f;
	}
	
	public void Init() {
		Utransform = transform;
		Uanimator = GetComponent<Animator>();
		Usprite = GetComponent<SpriteRenderer>();
		Urigidbody = GetComponent<Rigidbody2D>();
		Ucollider = GetComponent<BoxCollider2D>();
		
		if (ScenePlaced) ScenePlaced = false;
		
		if (PlayerControlled)
		{
			controller = GetComponentInParent<PlayerController>();
		} else {
			ai = GetComponent<EnemyAI_Basic>();
		}
		
		Uaudio = GetComponent<AudioSource>();
		
		hash_IsWalking = Animator.StringToHash("isWalking");
		
		currentHealth = MaxHealth;
		attackRate = 0f;
		Utransform.localScale = Vector3.one;
		
		if (Dead)
		{
			Dead = false;
			Shadow.enabled = true;
			Ucollider.enabled = true;
			Urigidbody.bodyType = RigidbodyType2D.Dynamic;
			Usprite.enabled = true;
			attackRate = 0f;
		}
		
		if (PlayerControlled)
		{
			Ucollider.enabled = false;
			Urigidbody.simulated  = false;
		} else {
			Ucollider.enabled = true;
			Urigidbody.simulated  = true;
		}
	}
	
	void Update() {
		attackPosition = Utransform.position + (Utransform.right * AttackRange);
		call_Cooldown();
		call_GroundingRaycast();
		call_StaggerTime();
		
		if (FX_MeleeSweep != null) FX_MeleeSweep.Update_Effect();
		
		if (PlayerControlled) return;
		
		if (ai != null) ai.UpdateAI();
		u_Move( moveDestination );
		
		combatTime += Time.deltaTime;
		
		if (attackRate > 0f) return;
		if (Time.frameCount % 30 != 0) return;
		
		Collider2D[] find_Targets = Physics2D.OverlapCircleAll( attackPosition, AttackRange, UnitLayer );
		for (int i = 0; i < find_Targets.Length; i++)
		{
			if (Ranged)
			{
				float pointS = Utransform.position.y;
				float pointT = find_Targets[i].transform.position.y;
				
				float pointCompare = (pointS > pointT) ? pointS - pointT : pointT - pointS;
				if ( pointCompare > 0.5f) continue;
			}
			
			Unit target = null;
			if ( find_Targets[i].gameObject.CompareTag(tag_Player) )
			{
				target = find_Targets[i].GetComponent<PlayerController>().currentUnit;
			} else {
				target = find_Targets[i].GetComponent<Unit>();
			}
			
			if (target.Dead) continue;
			if (target.pressed_Ability) continue;
			if (!Utils.CheckTeams( this, target)) continue;
			I_Attack();
		}
	}
	
	void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere( attackPosition, AttackRange );
	}
	
	//=================================================================================
	//=========== Input
	//=================================================================================
	
	public void I_Move( bool walking ) {
		if (pressed_Ability) return;
		
		iswalking = walking;
		Uanimator.SetBool(hash_IsWalking, walking);
		
		if (FX_WalkTrail == null) return;
		if (walking) {
			FX_WalkTrail.Play();
		} else {
			FX_WalkTrail.Stop();
		}
	}
	
	public void I_Attack()
	{
		if (AttackDamage == 0f) return;
		if (Dead) return;
		if (attackRate > 0f) return;
		if (pressed_Ability) return;
		
		attackRate = _attackRateReturn / AttackSpeed;
		
		if (DelayedAttack)
		{
			pressed_Attack = true;
			Uanimator.Play(anim_attack);
		} else {
			DealDamage();
		}
	}
	
	public void I_Special()
	{
		if (Dead) 
		{
			Usprite.color = c_normal;
			if (controller != null) 
			{
				controller.Hide( false );
				controller.ReReadInput();
			}
			return;
		}
		
		if (pressed_Attack) return;
		
		pressed_Ability = !pressed_Ability;
		
		if (pressed_Ability)
		{
			Usprite.color = c_stealth;
			if (controller != null) controller.Hide( true );
		} else {
			Usprite.color = c_normal;
			if (controller != null) 
			{
				controller.Hide( false );
				controller.ReReadInput();
			}
		}
	}
	
	public void I_Jump()
	{
		if (Urigidbody.velocity.y >= -1f)
		{
			Vector2 v2_Jump = new Vector2(0f, _jumpHeight);
			Urigidbody.AddForce(v2_Jump * _jumpForce, ForceMode2D.Impulse);
			grounded = false;
		}
	}
	
	//=================================================================================
	//=========== Damage
	//=================================================================================
	
	void DealDamage() {
		pressed_Attack = false;
		if (Ranged)
		{
			Vector2 rangeForce = Utransform.right * RangeSpeed;
			Vector3 rangePos = Utransform.position + RangeProjectileOffset;
			Projectile.Create( this, rangePos, RangeAttack, rangeForce );
		} else {
			if (FX_MeleeSweep != null) FX_MeleeSweep.Begin();
			
			Collider2D[] hit_enemyColliders = Physics2D.OverlapCircleAll( attackPosition, AttackRange, UnitLayer );
			for (int i = 0; i < hit_enemyColliders.Length; i++)
			{
				if ( hit_enemyColliders[i].gameObject.CompareTag(tag_Player) )
				{
					Unit pTarget = hit_enemyColliders[i].GetComponent<PlayerController>().currentUnit;
					if (pTarget != this) 
					{
						pTarget.TakeDamage( this, AttackDamage, null );
						break;
					}
					
				} else {
					Unit target = hit_enemyColliders[i].GetComponent<Unit>();
					if (Utils.CheckTeams( this, target))
					{
						if (Race == UnitRace.Offmoths) 
						{
							Eat( target );
						} else {
							target.TakeDamage( this, AttackDamage, null );
						}
						break;
					}
				}
			}
			PlaySound(SwingSound);
		}
	}
	
	public void TakeDamage( Unit source, float damage, AudioClip hitSound )
	{
		if (Dead) return;
		currentHealth -= damage;
		Usprite.color = (pressed_Ability) ? c_stealthdamaged : c_damaged;
		if (PlayerControlled) controller.Update_Health();
		
		HitStagger();
		if (hitSound != null) 
		{	
			PlaySound( hitSound );
		} else if ( !source.Ranged ){
			PlaySound( HitSound );
		}
		
		if (source.PlayerControlled) source.controller.scene.ShakeScreen();
		
		if (currentHealth <= 0.415)
		{
			Usprite.color = c_normal;
			Dead = true;
			if (Ucollider != null)
			{
				Ucollider.enabled = false;
				Urigidbody.bodyType = RigidbodyType2D.Kinematic;
			}
			
			FX_Death.Play();
			if (FX_WalkTrail != null) FX_WalkTrail.Stop();
			I_Special();
			
			Shadow.enabled = false;
			Ucollider.enabled = false;
			Usprite.enabled = false;
			
			PlayerControlled = false;
			if (source.PlayerControlled)
			{
				source.controller.Healing();
			}
			
			if (gameObject.activeSelf) StartCoroutine( Death() );
		} else {
			FX_Hurt.Play();
			if (!PlayerControlled)
			{
				if (combatTime >= 2f)
				{
					combatTime = 0f;
					Vector3 aiAngle = (source.Utransform.position - Utransform.position).normalized;
					if (Vector3.Dot( Utransform.right, aiAngle ) <= 0f)
					{
						ai.RotateAI();
					}
				}
			}
		}
	}
	public void Eaten() {
		Dead = true;
		if (Ucollider != null)
		{
			Ucollider.enabled = false;
			Urigidbody.bodyType = RigidbodyType2D.Kinematic;
		}
		if ( !PlayerControlled )
		{	
			FX_WalkTrail.Stop();
			
			Shadow.enabled = false;
			Ucollider.enabled = false;
			Usprite.enabled = false;
			
			StartCoroutine( Death() );
		}
	}
	
	void Eat( Unit target ) {
		target.Eaten();
		
		if (controller != null)
		{
			controller.InitializeChar( target.Prefab, target.Utransform.position );
		}
	}
	
	IEnumerator Death()
	{
		yield return deathTimer;
		if (ScenePlaced)
		{
			Destroy(gameObject);
		} else {
			if (gameObject.activeSelf) LeanPool.Despawn(gameObject);
		}
	}
	
	//=================================================================================
	//=========== Anim
	//=================================================================================
	
	void HitStagger()
	{
		staggerTime = 0.15f;
		Staggered = true;
	}
	
	public void PlaySound( AudioClip clipp )
	{
		if (PlayerControlled)
		{
			controller.pAudio.PlayOneShot( clipp );
		} else {
			Uaudio.PlayOneShot( clipp );
		}
	}
	
	void u_Move( Vector3 destination )
	{
		if (Dead) return;
		if (Staggered) return;
		if (pressed_Ability) return;
		
		if (Utransform.position != moveDestination && attackRate == 0f)
		{
			I_Move( true );
			Utransform.Translate( moveDestination );
		} else {
			I_Move( false );
		}
	}
	
	//=================================================================================
	//=========== Getter and Setter
	//=================================================================================
	
	void call_GroundingRaycast() {
		if (Dead) return;
		if (PlayerControlled) return;
		
		RaycastHit2D hit = Physics2D.Raycast( Ucollider.bounds.center, Vector2.down, Ucollider.bounds.extents.y + 0.1f, ~UnitLayer );
		
		if (hit.collider != null)
		{
			Airborned(false);
		} else {
			Airborned(true);
		}
	}
	public void Airborned( bool yes )
	{
		Shadow.enabled = !yes;
		if (!grounded)
		{
			grounded = true;
		}
		if (Race == UnitRace.Offmoths) 
		{
			if (yes && !iswalking)
			{
				Uanimator.SetBool(hash_IsWalking, true);
			} else if (!yes && !iswalking)
			{
				Uanimator.SetBool(hash_IsWalking, false);
			}
		}
	}
	
	void call_StaggerTime()
	{
		if (Staggered)
		{
			staggerTime -= Time.deltaTime;
			if (staggerTime < 0)
			{
				staggerTime = 0f;
				Staggered = false;
				Usprite.color = (pressed_Ability) ? c_stealth : c_normal;
			}
		}
	}
	
	void call_Cooldown()
	{
		if (attackRate > 0f)
		{
			attackRate -= Time.deltaTime;
			if (attackRate < 0f)
			{
				attackRate = 0f;
			}
		}
	}
}
