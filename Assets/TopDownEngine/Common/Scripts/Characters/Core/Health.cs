using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// This class manages the health of an object, pilots its potential health bar, handles what happens when it takes damage,
	/// and what happens when it dies.
	/// </summary>
	[AddComponentMenu("TopDown Engine/Character/Core/Health")]
	public class Health : MonoBehaviour
	{
        [Header("Bindings")]

		// the model to disable (if set so)
		[Tooltip("the model to disable (if set so)")]
		public GameObject Model;

        [Header("Status")]

        [MMReadOnly]
		[LabelText("当前血量")]
		public int CurrentHealth ;

		[MMReadOnly]
		[LabelText("是否无敌，无敌不受到伤害")]
		public bool Invulnerable = false;

		[Header("Health")]

		// 初始血量
		[LabelText("初始血量")]
		public int InitialHealth = 10;

		// 最大血量
		[LabelText("最大血量")]
		public int MaximumHealth = 10;

        [Header("Damage")]

        [MMInformation("Here you can specify an effect and a sound FX to instantiate when the object gets damaged, and also how long the object should flicker when hit (only works for sprites).", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		// 是否免疫击退
		[LabelText("是否免疫击退")]
		public bool ImmuneToKnockback = false;

		/// 受到伤害时的MMFeedBack
		[LabelText("受到伤害时的MMFeedBack")]
		public MMFeedbacks DamageMMFeedbacks;

        [Header("Death")]

        [MMInformation("在这里，您可以设置一个效果，在对象死亡时实例化，应用于对象的力（需要自上而下的控制器），向游戏分数添加多少点，设备是否应振动（仅适用于iOS和Android），以及角色应在何处重生（仅适用于非玩家角色）。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        // 是否在死亡时destroy
        [LabelText("是否在死亡时destroy")]
		public bool DestroyOnDeath = true;

        // 销毁前的延迟
        [LabelText("销毁前的延迟")]
		public float DelayBeforeDestruction = 0f;

		/// 死亡时增加的分数
		[LabelText("死亡时增加的分数")]
		public int PointsWhenDestroyed;
		/// if this is set to false, the character will respawn at the location of its death, otherwise it'll be moved to its initial position (when the scene started)
		[LabelText("是否在重生时重置位置")]
		[Tooltip("if this is set to false, the character will respawn at the location of its death, otherwise it'll be moved to its initial position (when the scene started)")]
		public bool RespawnAtInitialLocation = false;
		/// if this is true, the controller will be disabled on death
		[Tooltip("if this is true, the controller will be disabled on death")]
		public bool DisableControllerOnDeath = true;
		/// if this is true, the model will be disabled instantly on death (if a model has been set)
		[Tooltip("if this is true, the model will be disabled instantly on death (if a model has been set)")]
		public bool DisableModelOnDeath = true;
		/// if this is true, collisions will be turned off when the character dies
		[Tooltip("if this is true, collisions will be turned off when the character dies")]
		public bool DisableCollisionsOnDeath = true;
		/// if this is true, collisions will also be turned off on child colliders when the character dies
		[Tooltip("if this is true, collisions will also be turned off on child colliders when the character dies")]
		public bool DisableChildCollisionsOnDeath = false;
        /// whether or not this object should change layer on death
        [Tooltip("whether or not this object should change layer on death")]
        public bool ChangeLayerOnDeath = false;
        /// whether or not this object should change layer on death
        [Tooltip("whether or not this object should change layer on death")]
        public bool ChangeLayersRecursivelyOnDeath = false;
        /// the layer we should move this character to on death
        [Tooltip("the layer we should move this character to on death")]
        public MMLayer LayerOnDeath;
        /// the feedback to play when dying
        [LabelText("死亡时的MMFeedbacks")]
		public MMFeedbacks DeathMMFeedbacks;

        // hit delegate
        public delegate void OnHitDelegate();
        public OnHitDelegate OnHit;

        // respawn delegate
        public delegate void OnReviveDelegate();
		public OnReviveDelegate OnRevive;

        // death delegate
		public delegate void OnDeathDelegate();
		public OnDeathDelegate OnDeath;

		protected Vector3 _initialPosition;
		protected Renderer _renderer;
		protected Character _character;
		protected TopDownController _controller;
	    protected MMHealthBar _healthBar;
	    protected Collider2D _collider2D;
        protected Collider _collider3D;
        protected CharacterController _characterController;
        protected bool _initialized = false;
		protected Color _initialColor;
        protected AutoRespawn _autoRespawn;
        protected Animator _animator;
        protected int _initialLayer;

        /// <summary>
        /// On Start, we initialize our health
        /// </summary>
        protected virtual void Awake()
	    {
			Initialization();
	    }

	    /// <summary>
	    /// Grabs useful components, enables damage and gets the inital color
	    /// </summary>
		protected virtual void Initialization()
		{
			_character = this.gameObject.GetComponent<Character>();

            if (Model != null)
            {
                Model.SetActive(true);
            }

            if (gameObject.MMGetComponentNoAlloc<Renderer>() != null)
			{
				_renderer = GetComponent<Renderer>();
			}
			if (_character != null)
			{
				if (_character.CharacterModel != null)
				{
					if (_character.CharacterModel.GetComponentInChildren<Renderer> ()!= null)
					{
						_renderer = _character.CharacterModel.GetComponentInChildren<Renderer> ();
					}
				}
			}
            if (_renderer != null)
            {
                if (_renderer.material.HasProperty("_Color"))
                {
                    _initialColor = _renderer.material.color;
                }
            }

            // we grab our animator
            if (_character != null)
            {
                if (_character.CharacterAnimator != null)
                {
                    _animator = _character.CharacterAnimator;
                }
                else
                {
                    _animator = GetComponent<Animator>();
                }
            }
            else
            {
                _animator = GetComponent<Animator>();
            }

            if (_animator != null)
            {
                _animator.logWarnings = false;
            }

            _initialLayer = gameObject.layer;

            _autoRespawn = this.gameObject.GetComponent<AutoRespawn>();
            _healthBar = this.gameObject.GetComponent<MMHealthBar>();
            _controller = this.gameObject.GetComponent<TopDownController>();
            _characterController = this.gameObject.GetComponent<CharacterController>();
            _collider2D = this.gameObject.GetComponent<Collider2D>();
            _collider3D = this.gameObject.GetComponent<Collider>();

            DamageMMFeedbacks?.Initialization(this.gameObject);
            DeathMMFeedbacks?.Initialization(this.gameObject);

            _initialPosition = transform.position;
			_initialized = true;
			CurrentHealth = InitialHealth;
			DamageEnabled();
			UpdateHealthBar (false);
		}

		/// <summary>
		/// When the object is enabled (on respawn for example), we restore its initial health levels
		/// </summary>
	    protected virtual void OnEnable()
	    {
			CurrentHealth = InitialHealth;
            if (Model != null)
            {
                Model.SetActive(true);
            }
			DamageEnabled();
			UpdateHealthBar (false);
	    }

		/// <summary>
		/// 造成伤害
		/// </summary>
		/// <param name="damage">伤害值</param>
		/// <param name="instigator">造成伤害的物体</param>
		/// <param name="flickerDuration">The time (in seconds) the object should flicker after taking the damage.</param>
		/// <param name="invincibilityDuration">The duration of the short invincibility following the hit.</param>
		public virtual void Damage(int damage,GameObject instigator, float flickerDuration, float invincibilityDuration)
		{
			// if the object is invulnerable, we do nothing and exit
			if (Invulnerable)
			{
				return;
			}

			// if we're already below zero, we do nothing and exit
			if ((CurrentHealth <= 0) && (InitialHealth != 0))
			{
				return;
			}

			// we decrease the character's health by the damage
			float previousHealth = CurrentHealth;
			CurrentHealth -= damage;

            if (OnHit != null)
            {
                OnHit();
            }

            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
            }

            // we prevent the character from colliding with Projectiles, Player and Enemies
            if (invincibilityDuration > 0)
			{
				DamageDisabled();
				StartCoroutine(DamageEnabled(invincibilityDuration));
			}

			// we trigger a damage taken event
			MMDamageTakenEvent.Trigger(_character, instigator, CurrentHealth, damage, previousHealth);

            if (_animator != null)
            {
                _animator.SetTrigger("Damage");
            }

            DamageMMFeedbacks?.PlayFeedbacks(this.transform.position);

			// we update the health bar
			UpdateHealthBar(true);

			// if health has reached zero
			if (CurrentHealth <= 0)
			{
				// we set its health to zero (useful for the healthbar)
				CurrentHealth = 0;

				Kill();
			}
		}

		/// <summary>
		/// Kills the character, vibrates the device, instantiates death effects, handles points, etc
		/// </summary>
		public virtual void Kill()
        {
            if (_character != null)
            {
                // we set its dead state to true
                _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Dead);
                _character.Reset();

                if (_character.CharacterType == Character.CharacterTypes.Player)
                {
                    TopDownEngineEvent.Trigger(TopDownEngineEventTypes.PlayerDeath, _character);
                }
            }
            CurrentHealth = 0;

            // we prevent further damage
            DamageDisabled();

            DeathMMFeedbacks?.PlayFeedbacks(this.transform.position);

			// Adds points if needed.
			if(PointsWhenDestroyed != 0)
			{
				// we send a new points event for the GameManager to catch (and other classes that may listen to it too)
				TopDownEnginePointEvent.Trigger(PointsMethods.Add, PointsWhenDestroyed);
			}

            if (_animator != null)
            {
                _animator.SetTrigger("Death");
            }
            // we make it ignore the collisions from now on
            if (DisableCollisionsOnDeath)
            {
                if (_collider2D != null)
                {
                    _collider2D.enabled = false;
                }
                if (_collider3D != null)
                {
                    _collider3D.enabled = false;
                }

                // if we have a controller, removes collisions, restores parameters for a potential respawn, and applies a death force
                if (_controller != null)
			    {
					_controller.CollisionsOff();
                }

                if (DisableChildCollisionsOnDeath)
                {
                    foreach (Collider2D collider in this.gameObject.GetComponentsInChildren<Collider2D>())
                    {
                        collider.enabled = false;
                    }
                    foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
                    {
                        collider.enabled = false;
                    }
                }
            }

            if (ChangeLayerOnDeath)
            {
                gameObject.layer = LayerOnDeath.LayerIndex;
                if (ChangeLayersRecursivelyOnDeath)
                {
                    this.transform.ChangeLayersRecursively(LayerOnDeath.LayerIndex);
                }
            }

            OnDeath?.Invoke();

            if (DisableControllerOnDeath && (_controller != null))
            {
                _controller.enabled = false;
            }

            if (DisableControllerOnDeath && (_characterController != null))
            {
                _characterController.enabled = false;
            }

            if (DisableModelOnDeath && (Model != null))
            {
                Model.SetActive(false);
            }

			if (DelayBeforeDestruction > 0f)
			{
				Invoke ("DestroyObject", DelayBeforeDestruction);
			}
			else
			{
				// finally we destroy the object
				DestroyObject();
			}
		}

		/// <summary>
		/// Revive this object.
		/// </summary>
		public virtual void Revive()
		{
			if (!_initialized)
			{
				return;
			}

            if (_collider2D != null)
            {
                _collider2D.enabled = true;
            }
            if (_collider3D != null)
            {
                _collider3D.enabled = true;
            }
            if (DisableChildCollisionsOnDeath)
            {
                foreach (Collider2D collider in this.gameObject.GetComponentsInChildren<Collider2D>())
                {
                    collider.enabled = true;
                }
                foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
                {
                    collider.enabled = true;
                }
            }
            if (ChangeLayerOnDeath)
            {
                gameObject.layer = _initialLayer;
                if (ChangeLayersRecursivelyOnDeath)
                {
                    this.transform.ChangeLayersRecursively(_initialLayer);
                }
            }
            if (_characterController != null)
            {
                _characterController.enabled = true;
            }
            if (_controller != null)
			{
                _controller.enabled = true;
				_controller.CollisionsOn();
				_controller.Reset();
			}
			if (_character != null)
			{
				_character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
			}
            if (_renderer!= null)
            {
                _renderer.material.color = _initialColor;
            }

            if (RespawnAtInitialLocation)
			{
				transform.position = _initialPosition;
			}
            if (_healthBar != null)
            {
                _healthBar.Initialization();
            }

            Initialization();
			UpdateHealthBar(false);
            OnRevive?.Invoke();
        }

	    /// <summary>
	    /// Destroys the object, or tries to, depending on the character's settings
	    /// </summary>
	    protected virtual void DestroyObject()
        {
            if (_autoRespawn == null)
            {
                if (DestroyOnDeath)
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                _autoRespawn.Kill();
            }
        }

		/// <summary>
		/// Called when the character gets health (from a stimpack for example)
		/// </summary>
		/// <param name="health">The health the character gets.</param>
		/// <param name="instigator">The thing that gives the character health.</param>
		public virtual void GetHealth(int health,GameObject instigator)
		{
			// this function adds health to the character's Health and prevents it to go above MaxHealth.
			CurrentHealth = Mathf.Min (CurrentHealth + health,MaximumHealth);
			UpdateHealthBar(true);
		}

	    /// <summary>
	    /// 重置血量到最大
	    /// </summary>
	    public virtual void ResetHealthToMaxHealth()
	    {
			CurrentHealth = MaximumHealth;
			UpdateHealthBar (false);
        }

        /// <summary>
        /// 设置当前血量，并且更新血条
        /// </summary>
        /// <param name="newValue"></param>
        public virtual void SetHealth(int newValue)
        {
            CurrentHealth = newValue;
            UpdateHealthBar(false);
        }

	    /// <summary>
	    /// 更新血条
	    /// </summary>
		protected virtual void UpdateHealthBar(bool show)
	    {
	    	if (_healthBar != null)
	    	{
				_healthBar.UpdateBar(CurrentHealth, 0f, MaximumHealth, show);
	    	}

	    	if (_character != null)
	    	{
	    		if (_character.CharacterType == Character.CharacterTypes.Player)
	    		{
					// We update the health bar
					if (GUIManager.Instance != null)
					{
						GUIManager.Instance.UpdateHealthBar(CurrentHealth, 0f, MaximumHealth, _character.PlayerID);
					}
	    		}
	    	}
	    }

	    /// <summary>
	    /// Prevents the character from taking any damage
	    /// 开启无敌
	    /// </summary>
	    public virtual void DamageDisabled()
	    {
			Invulnerable = true;
	    }

	    /// <summary>
	    /// Allows the character to take damage
	    /// 取消无敌状态
	    /// </summary>
	    public virtual void DamageEnabled()
	    {
	    	Invulnerable = false;
	    }

		/// <summary>
	    /// makes the character able to take damage again after the specified delay
	    /// delay取消无敌状态
	    /// </summary>
	    /// <returns>The layer collision.</returns>
	    public virtual IEnumerator DamageEnabled(float delay)
		{
			yield return new WaitForSeconds (delay);
			Invulnerable = false;
		}

        /// <summary>
        /// On Disable, we prevent any delayed destruction from running
        /// </summary>
        protected virtual void OnDisable()
        {
            CancelInvoke();
        }
	}
}