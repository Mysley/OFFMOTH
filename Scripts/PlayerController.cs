using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Lean.Pool;
using DATA;

public class PlayerController : MonoBehaviour
{
	[Header("Playable")]
	[SerializeField] Unit offmoth;
	[Space(15)]
	[Header("UI Attributes")]
	[SerializeField] Image portrait;
	[SerializeField] Image attack;
	[SerializeField] TextMeshProUGUI attackText;
	[SerializeField] Image special;
	[SerializeField] TextMeshProUGUI specialText;
	[SerializeField] Image healthBar;
	[SerializeField] TextMeshProUGUI healthText;
	[SerializeField] Image eatBar;
	[SerializeField] TextMeshProUGUI eatText;
	[SerializeField] SpriteRenderer CurseIcon;
	[SerializeField] Special SpecialCooldown;
	[SerializeField] TextMeshProUGUI scoreText;
	[Space(15)]
	[Header("References")]
	[SerializeField] LayerMask UnitLayer;
	[SerializeField] LayerMask defaultLayer;
	[SerializeField] Sprite StealthSprite;
	[SerializeField] Sprite UnmutateSprite;
	[SerializeField] SpriteAnimator FX_Heal;
	[SerializeField] ParticleSystem FX_Jump;
	[SerializeField] ParticleSystem FX_Land;
	[SerializeField] ParticleSystem FX_Eaten;
	[SerializeField] ParticleSystem FX_Hidden;
	[SerializeField] ParticleSystem FX_Unmutate;
	[SerializeField] AudioClip clip_Eaten;
	[SerializeField] AudioClip clip_Hidden;
	[SerializeField] AudioClip clip_Unmutate;
	[SerializeField] AudioClip clip_Jump;
	[SerializeField] AudioClip clip_Heal;
	[SerializeField] AudioClip clip_Win;
	[SerializeField] AudioClip clip_Death;
	
	Transform pTransform;
	BoxCollider2D pCollider;
	Rigidbody2D pRigidbody;
	PlayerInput playerInput;
	[HideInInspector] public AudioSource pAudio;
	
	[HideInInspector] public PlayerScene scene;
	Vector2 moveInput;
	Vector2 move;
	
	[HideInInspector] public Unit currentUnit;
	[HideInInspector] public bool Fed;
	[HideInInspector] public int TargetsEaten;
	bool jumping;
	bool grounded;
	bool airborne;
	
	float jumpTimeCounter;
	
	const float _groundNudge = 0.1f;
	const float _jumpTimeMax = 0.35f;
	const float _flyTimeMax = 2.5f;
	const float _jumpForce = 10f;
	const float _clutchForce = 60f;
	const float _flyForce = 120f;
	const float _jumpBounce = 5f;
	const float _walkForce = 50f;
	const int _neededScore = 20;
	
	static readonly Color32 barColor_HE = new Color32(255, 0, 0, 255);
	static readonly Color32 barColor_SA = new Color32(0, 100, 255, 255);
	static readonly Color32 barColor_EX = new Color32(255, 150, 0, 255);
	static readonly Color32 barColor_PC = new Color32(255, 0, 255, 255);
	
	static readonly string _movement = "Movement";
	static readonly string _jump = "Jump";
	static readonly string _attack = "Attack";
	static readonly string _special = "Special";
	static readonly string _pause = "Pause";
	static readonly string divider = " / ";
	
	static readonly string LostFunction = "Lost";
	static readonly string HIDE = "Hide";
	static readonly string UNMUTATE = "Unmutate";
	static readonly string neededScore = " / 20";
	static readonly string loseScore = " out of 20";
	
	static readonly Vector2 jumpHeight = new Vector2(0f, 5.5f);
	static readonly Vector3 leftFace = new Vector3( 0f, 180f, 0f);
	static readonly Vector3 jumpBounce = new Vector3(0f, 0.3f, 0f);
	
	// ================================================
	// ================================================
	
	void Start() {
		pTransform = transform;
		pCollider = GetComponent<BoxCollider2D>();
		pRigidbody = GetComponent<Rigidbody2D>();
		pAudio = GetComponent<AudioSource>();
		
		scene = GetComponent<PlayerScene>();
		
		playerInput = GetComponent<PlayerInput>();
		
		playerInput.actions[_movement].started += PlayerInput_Movement;
		playerInput.actions[_movement].performed += PlayerInput_Movement;
		playerInput.actions[_movement].canceled += PlayerInput_Movement;
		
		playerInput.actions[_jump].performed += PlayerInput_Jump;
		playerInput.actions[_jump].canceled += PlayerInput_Jump;
		
		playerInput.actions[_attack].performed += PlayerInput_Attack;
		
		playerInput.actions[_special].performed += PlayerInput_Special;
		playerInput.actions[_special].canceled += PlayerInput_Special;
		
		playerInput.actions[_pause].performed += PlayerInput_Pause;
		
		currentUnit = offmoth;
		
		Init_HealthText();
		Reset_EatText();
	}
	
	void Update() {
		if (currentUnit.Dead) return;
		SpecialCooldown.UpdateCD( currentUnit );
		u_Move();
		u_GroundRaycast();
	}
	void FixedUpdate() {
		if (currentUnit.Dead) return;
		u_Movement();
	}
	
	// ================================================
	//{ I N P U T
	
	void PlayerInput_Movement( InputAction.CallbackContext context ) {
		if (scene.Quitter.enabled) return;
		if (currentUnit.Dead) return;
		moveInput = context.ReadValue<Vector2>();
		moveInput.x = Mathf.RoundToInt(moveInput.x);
		moveInput.y = 0f;
		
		if (moveInput.x != 0f)
		{
			currentUnit.I_Move( true );
		} else {
			currentUnit.I_Move( false );
		}
		
		if (moveInput.x < 0f)
		{
			pTransform.eulerAngles = leftFace;
		} else if (moveInput.x > 0f) {
			pTransform.eulerAngles = Vector3.zero;
		}
		
		move = Vector2.ClampMagnitude( moveInput, 1f );
	}
	
	void PlayerInput_Jump( InputAction.CallbackContext context ) {
		
		if (context.performed)
		{
			jumping = true;
			
			if (currentUnit.Dead) return;
			if (currentUnit.pressed_Attack) return;
			if (currentUnit.pressed_Ability) return;
			if (scene.Quitter.enabled) return;
			
			if (grounded)
			{
				airborne = true;
				pRigidbody.AddForce(jumpHeight * _jumpForce, ForceMode2D.Impulse);
				jumpTimeCounter = (Fed) ? _jumpTimeMax : _flyTimeMax;
				
				pAudio.PlayOneShot(clip_Jump);
				FX_Jump.Play();
				FX_Land.Play();
				currentUnit.grounded = false;
			}
			
		} else if (context.canceled) {
			jumping = false;
		}
	}
	
	void PlayerInput_Attack( InputAction.CallbackContext context ) {currentUnit.I_Attack();}
	void PlayerInput_Special( InputAction.CallbackContext context ) { SpecialAction(context.performed); }
	
	void SpecialAction( bool performed ) {
		if (Fed)
		{
			currentUnit.TakeDamage( currentUnit, 100, clip_Unmutate);
		} else {
			if ( (performed && !currentUnit.pressed_Ability) || (!performed && currentUnit.pressed_Ability) )
			{
				currentUnit.I_Move( false );
				currentUnit.I_Special();
			}
		}
	}
	
	void PlayerInput_Pause( InputAction.CallbackContext context ) {
		if (scene.WinCanvas.enabled) return;
		if (scene.HelpCanvas.enabled) return;
		Time.timeScale = scene.Quitter.enabled ? 1 : 0;
		scene.Quitter.enabled = !scene.Quitter.enabled ;
	}
	public void Pause( bool yes ) {
		Time.timeScale = yes ? 0 : 1;
	}
	
	void Lost() {
		scene.LostCanvas.enabled = true;
		scene.LoseScore.SetText( TargetsEaten.ToString() + loseScore );
		pAudio.PlayOneShot( clip_Death );
	}
	void Win() {
		Time.timeScale = 0f;
		scene.WinCanvas.enabled = true;
		pAudio.PlayOneShot( clip_Win );
	}
	
	public void Reset() {
		playerInput.actions[_movement].started -= PlayerInput_Movement;
		playerInput.actions[_movement].performed -= PlayerInput_Movement;
		playerInput.actions[_movement].canceled -= PlayerInput_Movement;
		
		playerInput.actions[_jump].performed -= PlayerInput_Jump;
		playerInput.actions[_jump].canceled -= PlayerInput_Jump;
		
		playerInput.actions[_attack].performed -= PlayerInput_Attack;
		
		playerInput.actions[_special].performed -= PlayerInput_Special;
		playerInput.actions[_special].canceled -= PlayerInput_Special;
		
		playerInput.actions[_pause].performed -= PlayerInput_Pause;
	}
	
	//}
	// ================================================
	
	// ================================================
	//{ C H A R A C T E R S
	public void InitializeChar( Transform newUnit, Vector3 targetPos ) {
		
		if (newUnit == null)
		{
			Fed = false;
			CurseIcon.enabled = false;
			special.sprite = StealthSprite;
			specialText.SetText(HIDE);
			
			LeanPool.Despawn(currentUnit);
			currentUnit = offmoth;
			currentUnit.gameObject.SetActive( true );
			
			pAudio.PlayOneShot(clip_Unmutate);
			FX_Unmutate.Play();
			Init_HealthText();
			Reset_EatText();
		} else {
			Fed = true;
			TargetsEaten++;
			scoreText.SetText( TargetsEaten.ToString() + neededScore );
			CurseIcon.enabled = true;
			special.sprite = UnmutateSprite;
			specialText.SetText(UNMUTATE);
			
			currentUnit.gameObject.SetActive( false );
			currentUnit = LeanPool.Spawn( newUnit, pTransform.position, pTransform.rotation ).GetComponent<Unit>();
			currentUnit.PlayerControlled = true;
			currentUnit.transform.parent = pTransform;
			currentUnit.Init();
			
			pAudio.PlayOneShot(clip_Eaten);
			FX_Eaten.Play();
			
			pRigidbody.Sleep();
			pRigidbody.position = targetPos;
			pRigidbody.WakeUp();
			
			Init_EatText();
			scene.ShakeScreen();
			
			if (TargetsEaten >= _neededScore)
			{
				Win();
			}
		}
		
		attackText.SetText(currentUnit.AttackName);
		attack.sprite = currentUnit.AttackIcon;
		portrait.sprite = currentUnit.BaseSprite;
		
		ReReadInput();
	}
	
	public void Update_Health() {
		if (currentUnit.currentHealth <= 0f)
		{
			currentUnit.currentHealth = 0f;
			currentUnit.Dead = true;
			if ( !Fed )
			{
				Invoke(LostFunction, 2f);
			} else {
				InitializeChar(null, Vector3.zero);
			}
		}
		
		float percent = currentUnit.currentHealth / currentUnit.MaxHealth;
		int text_Max = (int)currentUnit.MaxHealth;
		int text_Current = (int)currentUnit.currentHealth;
		string text_Display = text_Current + divider + text_Max;
		
		if (Fed)
		{
			eatBar.fillAmount = percent;
			eatText.SetText(text_Display);
		} else {
			healthBar.fillAmount = percent;
			healthText.SetText(text_Display);
		}
	}
	public void Healing() {
		offmoth.currentHealth += Random.Range(2,5);
		
		if (offmoth.currentHealth > offmoth.MaxHealth)
		{
			offmoth.currentHealth = offmoth.MaxHealth;
		}
		
		FX_Heal.Spawned();
		pAudio.PlayOneShot( clip_Heal );
		
		float percent = offmoth.currentHealth / offmoth.MaxHealth;
		int text_Max = (int)offmoth.MaxHealth;
		int text_Current = (int)offmoth.currentHealth;
		string text_Display = text_Current + divider + text_Max;
		healthBar.fillAmount = percent;
		healthText.SetText(text_Display);
	}
	
	void Init_HealthText() {
		float percent = currentUnit.MaxHealth;
		healthBar.fillAmount = percent;
		
		int text_Max = (int)currentUnit.MaxHealth;
		string text_Display = text_Max + divider + text_Max;
		healthText.SetText(text_Display);
	}
	
	void Reset_EatText() {
		eatBar.fillAmount = 0f;
		eatText.SetText(string.Empty);
	}
	void Init_EatText() {
		
		if (currentUnit.Race == UnitRace.Human_Expeditors)
		{
			eatBar.color = barColor_HE;
		} else if (currentUnit.Race == UnitRace.Sentient_Automatons)
		{
			eatBar.color = barColor_SA;
		} else if (currentUnit.Race == UnitRace.Extraterrestrials)
		{
			eatBar.color = barColor_EX;
		} else if (currentUnit.Race == UnitRace.Planet_Cultists)
		{
			eatBar.color = barColor_PC;
		}
		
		float percent = currentUnit.MaxHealth;
		eatBar.fillAmount = percent;
		
		int text_Max = (int)currentUnit.MaxHealth;
		string text_Display = text_Max + divider + text_Max;
		eatText.SetText(text_Display);
	}
	//}
	// ================================================
	
	// ================================================
	//{ M O V E M E N T
	public void ReReadInput() {
		if (moveInput.x != 0f)
		{
			currentUnit.I_Move( true );
		} else {
			currentUnit.I_Move( false );
		}	
	}
	public void Hide(bool hidden) {
		if (hidden)
		{
			FX_Hidden.Play();
			pAudio.PlayOneShot( clip_Hidden );
		} else {
			FX_Hidden.Stop();
		}
	}
	
	void u_Move() {
		
		if (jumping && !currentUnit.pressed_Ability)
		{
			if (jumpTimeCounter > 0)
			{
				if (Fed) {
					pRigidbody.AddForce((Vector2.up * _clutchForce) * Time.deltaTime, ForceMode2D.Impulse);
				} else if (pRigidbody.velocity.y <= 1f) {
					pRigidbody.AddForce((Vector2.up * _flyForce) * Time.deltaTime, ForceMode2D.Impulse);
				}
				jumpTimeCounter -= Time.deltaTime;
			} else {
				jumping = false;
			}
		}
		
		if (currentUnit.Utransform.localScale.y == 1f) return;
		
		Vector3 bounce = currentUnit.Utransform.localScale;
		
		if (!grounded)
		{
			bounce.y += (_jumpBounce * Time.deltaTime);
			if (bounce.y > 1.4f) bounce.y = 1f;
		} else {
			bounce.y += (_jumpBounce * Time.deltaTime);
			if (bounce.y > 1f) bounce.y = 1f;
		}
		
		currentUnit.Utransform.localScale = bounce;
	}

	void u_Movement() {
		
		Vector2 nextPos = pRigidbody.velocity;
		if (currentUnit.Dead || currentUnit.Staggered || currentUnit.pressed_Attack || currentUnit.pressed_Ability) 
		{
			nextPos.x = 0f;
		} else {
			nextPos.x = (move.x * (currentUnit.MoveSpeed*_walkForce)) * Time.deltaTime;
		}
		pRigidbody.velocity = nextPos;
	}
	
	void u_GroundRaycast() {
		
		if (!grounded && pRigidbody.velocity.y <= -1f)
		{
			airborne = true;
		}
		
		RaycastHit2D hit = Physics2D.BoxCast( pCollider.bounds.center, pCollider.bounds.size, 0f, Vector2.down, _groundNudge, defaultLayer );
		
		if (hit.collider != null)
		{
			grounded = true;
			if ( airborne )
			{
				airborne = false;
				currentUnit.Utransform.localScale -= jumpBounce;
				FX_Land.Play();
				pAudio.PlayOneShot(currentUnit.LandSound);
			}
		} else { 
			grounded = false;
		}
		
		currentUnit.Airborned(!grounded);
	}
	
	//}
	// ================================================
	
}
