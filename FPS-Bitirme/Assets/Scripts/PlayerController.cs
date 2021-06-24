using System;
using Windows;
using DefaultNamespace;
using DuloGames.UI;
using fps.Item;
using fps.Quest;
using Lightbug.CharacterControllerPro.Core;
using UnityEngine;

namespace fps
{
    public enum CharacterState
    {
        Idle,
        Walking,
        Running
    }
    
    [RequireComponent(typeof(CharacterActor))]
    [DefaultExecutionOrder(ExecutionOrder.CharacterActorOrder - 1)]
    public class PlayerController : MonoBehaviour
    {
        #region COMPONENTS
        private CharacterActor   characterActor;
        private Transform        graphics;
        private GunController    GunController;
        private HudDamageManager hudDamageManager;
        private UIManager        uiManager;
        private ObjectiveTracker objectiveTracker;
        private Camera           mainCamera;
        private InventoryWindow  inventoryWindow;
            #endregion

        #region VARIABLES
        [Header("Speed")] [SerializeField] [Range(5, 25)]
        private float moveSpeed = 10.0f;

        [Header("Run Speed")] [SerializeField] [Range(1, 3)]
        private float runSpeedMultiplier = 1.45f;

        [SerializeField] [Range(5, 25)]
        private float verticalRotationSpeed = 10.0f;

        [SerializeField] [Range(5, 25)]
        private float horizontalRotationSpeed = 10.0f;

        [SerializeField]
        private float maxVerticalRotationAngle = 30;

        [SerializeField]
        private float gravity = 45.0f;

        [Header("Stats")]
        [SerializeField]
        private int maxHealth;
        [SerializeField]
        private int currentHealth;
        
        private float regenTimer;

        private bool isInteractingWithNpc;

        private CharacterState state;

        private int goldCount;
        #endregion

        #region PROPERTIES
        public bool HasObjectiveCompleted => objectiveTracker.HasObjectiveCompleted;
        
        public bool IsInteractingWithNpc => isInteractingWithNpc;
        
        public CharacterState State => state;
        #endregion

        private void Awake()
        {
            characterActor   = GetComponent<CharacterActor>();
            GunController    = GetComponent<GunController>();
            hudDamageManager = FindObjectOfType<HudDamageManager>();
            uiManager        = FindObjectOfType<UIManager>();
            objectiveTracker = FindObjectOfType<ObjectiveTracker>();
            mainCamera       = Camera.main;
            inventoryWindow  = FindObjectOfType<InventoryWindow>();
            
            graphics = transform.Find("Graphics");
            Cursor.lockState = CursorLockMode.Locked;

            isGameOver = false;
            regenTimer = 0.0f;
            Time.timeScale = 1.0f;
            state = CharacterState.Idle;

            uiManager.OnWindowOpened = OnWindowOpened;
            uiManager.OnWindowClosed = OnWindowClosed;
        }

        private void OnWindowOpened(UIWindow window)
        {
            Cursor.lockState = CursorLockMode.None;
            uiManager.HideCrossHair();
        }

        private void OnWindowClosed(UIWindow window)
        {
            Cursor.lockState = CursorLockMode.Locked;
            uiManager.ShowCrossHair();
        }
        
        private void Update()
        {
            HandleLootItem();
            HandleRegeneration();
        }

        private void HandleLootItem()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (activeLootObject != null)
                {
                    uiManager.OnLootObject(activeLootObject);
                    inventoryWindow.AddItem(activeLootObject.LootItem);
                    
                    activeLootObject.DoLoot();
                    activeLootObject = null;
                }
            }
        }

        private void HandleRegeneration()
        {
            if (regenTimer >= 2.0f)
            {
                RegenerateHealth(1);
                regenTimer = 0.0f;
            }
            
            regenTimer += Time.deltaTime;
        }

        private void FixedUpdate()
        {
            if (!uiManager.HasAnyWindowOpen && !isInteractingWithNpc)
            {
                UpdatePhysics();
            }
            
            SimulateGravity();
        }
        
        /// <summary>
        ///     Updates character physics simulation
        /// </summary>
        private void UpdatePhysics()
        {
            // Movement
            Vector3 velocity = GetMovementVelocity();

            bool isTryRunning = Input.GetKey(KeyCode.LeftShift);
            bool canRun = isTryRunning && !GunController.IsBusy && !GunController.CurrentGun.IsBusy;
            
            if (canRun)
            {
                velocity *= runSpeedMultiplier;
            }

            if (velocity.magnitude > 0.0f)
            {
                CharacterState newState = canRun ? CharacterState.Running : CharacterState.Walking;
                SetState(newState);
            }
            else
            {
                SetState(CharacterState.Idle);
            }

            characterActor.Velocity = velocity;

            // Rotation
            float yaw = Input.GetAxis("Mouse X") * horizontalRotationSpeed;
            float pitch = -Input.GetAxis("Mouse Y") * verticalRotationSpeed;

            Vector3 eulerAngles = characterActor.Rotation.eulerAngles + new Vector3(0, yaw, 0);
            characterActor.Rotation = Quaternion.Euler(eulerAngles);

            // vertical rotation should only affect visible graphics.
            float endAngle = QuaternionUtility.WrapAngle(graphics.localEulerAngles.x + pitch);
            if (endAngle > -maxVerticalRotationAngle && endAngle < maxVerticalRotationAngle)
            {
                graphics.Rotate(pitch, 0.0f, 0.0f, Space.Self);
            }
            // --
        }

        private Vector3 GetMovementVelocity()
        {
            Vector3 velocity = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
            {
                velocity += characterActor.Forward;
            }

            if (Input.GetKey(KeyCode.S))
            {
                velocity += -characterActor.Forward;
            }

            if (Input.GetKey(KeyCode.A))
            {
                velocity += -characterActor.Right;
            }

            if (Input.GetKey(KeyCode.D))
            {
                velocity += characterActor.Right;
            }

            return velocity * moveSpeed;
        }

        private void SimulateGravity()
        {
            float velocityY = 0;
            
            if (!characterActor.IsGrounded)
            {
                velocityY = -characterActor.Up.y * Mathf.Pow(characterActor.NotGroundedTime, 2) * gravity;
            }

            Vector3 currentVelocity = characterActor.Velocity;
            characterActor.Velocity = new Vector3(currentVelocity.x, velocityY, currentVelocity.z);
        }
        
        
        private void SetState(CharacterState characterState)
        {
            state = characterState;
        }

        public void TakeHit(int damage)
        {
            currentHealth -= damage;
            DamageDelegate.OnDamageEvent(new DamageInfo(damage));
            OnHealthChanged();
        }


        private void RegenerateHealth(int regenAmount)
        {
            if (currentHealth <= 0 || currentHealth >= 100)
            {
                return;
            }

            currentHealth = Mathf.Clamp(currentHealth + regenAmount, 0, 100);
            hudDamageManager.SetHealth(currentHealth);
            OnHealthChanged();
        }
        
        private void OnHealthChanged()
        {
            // Alpha
            float alpha = 0;
            if (currentHealth < 60 && currentHealth > 50)
            {
                alpha = 0.08f;
            }
            else if(currentHealth < 50 && currentHealth > 40)
            {
                alpha = 0.18f;
            }
            else if(currentHealth < 40 && currentHealth > 30)
            {
                alpha = 0.26f;
            }
            else if(currentHealth <= 30)
            {
                alpha = 0.35f;
            }
            hudDamageManager.SetAlpha(alpha);
            //

            if (currentHealth < 20)
            {
                Time.timeScale = 0.65f;
            }
            else
            {
                Time.timeScale = 1.0f;
            }

            if (currentHealth <= 0)
            {
                OnGameOver();
            }
        }

        private bool isGameOver = false;
        private async void OnGameOver()
        {
            DamageDelegate.OnDieEvent.Invoke();
            Cursor.lockState = CursorLockMode.None;
            isGameOver       = true;

            await TaskUtility.WaitForSeconds(0.1f);
            
            while (!isGameOver && Time.timeScale > 0.0f)
            {
                Time.timeScale -= 0.05f;
                await TaskUtility.WaitForSeconds(0.07f);
            }
        }
        
        public void OnEnterNpcInteraction()
        {
            isInteractingWithNpc = true;
            mainCamera.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
        }

        public void OnExitNpcInteraction()
        {
            isInteractingWithNpc = false;
            mainCamera.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void OnQuestAccepted(QuestData acceptedQuest)
        {
            objectiveTracker.OnQuestAccepted(acceptedQuest);
        }

        public void OnEnemyDeath(Enemy.Enemy enemy)
        {
            objectiveTracker.OnEnemyDeath(enemy);
        }

        public void OnQuestCompleted(QuestData completedQuest)
        { 
            uiManager.ShowGainObjectText($"<color=yellow>+{completedQuest.RewardCoin}</color> coin gained.");
            goldCount += completedQuest.RewardCoin;

            inventoryWindow.SetGold(goldCount);
            objectiveTracker.OnQuestCompleted();
        }

        private LootObject activeLootObject;
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag($"LootObject"))
            {
                activeLootObject = other.gameObject.GetComponent<LootObject>();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag($"LootObject"))
            {
                LootObject lootObject = other.gameObject.GetComponent<LootObject>();
                if (lootObject == activeLootObject)
                {
                    activeLootObject = null;
                }
            }
        }
    }
}