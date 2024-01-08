using UnityEngine;
using EFPController.Utils;

namespace EFPController
{
	public class SmoothLook : MonoBehaviour
	{
		// MEMBERS: -------------------------------------------------------------------------------

		[Tooltip("Mouse look sensitivity/camera move speed.")]
		public float sensitivityMouse = 0.2f;
		
		[Tooltip("Gamepad sensitivity/camera move speed.")]
		public float sensitivityGamepad = 2f;
		
		public AnimationCurve sensitivityGamepadCurve = new()
			{ keys = new Keyframe[] { new(0f, 0f), new(1f, 1f) } };
		
		public float verticalSensitivityMultiplier = 1f;
		
		[Tooltip("Minumum pitch of camera for mouselook.")]
		public float minimumYAngle = -89f;
		
		[Tooltip("Maximum pitch of camera for mouselook.")]
		public float maximumYAngle = 89f;
		
		public float maxXAngle { get; set; } = 360f;
		
		[Tooltip("Smooth speed of camera angles for mouse look.")]
		public float smoothSpeedMouse = 0.75f;
		
		[Tooltip("Smooth speed of camera angles for controller.")]
		public float smoothSpeedGamepad = 0.6f;

		// PROPERTIES: ----------------------------------------------------------------------------
		
		[HideInInspector] public Vector2 _lookInput;
		public float SensitivityAmt { get; set; } = 4f; // actual sensitivity modified by IronSights Script
		public float RotationX { get; set; } = 0f;
		public float RotationY { get; set; } = 0f;
		public float RotationZ { get; set; } = 0f;
		public float InputY { get; set; } = 0f;
		public float PlayerMovedTime { get; set; }
		public Quaternion OriginalRotation { get; set; }
		public float RecoilX { get; set; } // non recovering recoil amount managed by WeaponKick function of Item Behavior
		public float RecoilY { get; set; } // non recovering recoil amount managed by WeaponKick function of Item Behavior
		public float CurrentSensitivity { get; set; }
		public float CurrentSmoothSpeed { get; set; }
		public bool IsGamepad { get; private set; }
		public bool Smooth { get; set; } = true;
		public float MaxXAngleDef { get; private set; }
		
		// FIELDS: --------------------------------------------------------------------------------
		
		private const float MaximumX = 360f;
		
		private Player _player;
		private InputManager _inputManager;
		private Transform _transform;
		
		private Quaternion _xQuaternion;
		private Quaternion _yQuaternion;
		private Quaternion _zQuaternion;
		private float _minimumYAngleDef;
		private float _maximumYAngleDef;
		private float _sensitivityMouseDef;
		private float _sensitivityGamepadDef;
		private float _smoothSpeedMouseDef;
		private float _smoothSpeedGamepadDef;
		private float _sensitivityAmtX;
		private float _sensitivityAmtY;
		private bool _isGamepadPrev;
		private bool _inputDeviceChange;

		// GAME ENGINE METHODS: -------------------------------------------------------------------
		
		private void Awake() =>
			_transform = transform;

		private void Start()
		{
			_transform.SetParent(null);
		
			// sync the initial rotation of the main camera to the y rotation set in editor
			OriginalRotation = Quaternion.Euler(_transform.eulerAngles.x, _transform.eulerAngles.y, 0f);
		
			SensitivityAmt = InputManager.isGamepad ? sensitivityGamepad : sensitivityMouse; // initialize sensitivity amount from var set by player
		}

		private void LateUpdate()
		{
			if (!Player.canCameraControl || !Player.inited || Time.smoothDeltaTime <= 0f)
				return;

			_lookInput = _inputManager.GetLookInput();

			IsGamepad = _inputManager.LookIsGamepad();
			_inputDeviceChange = IsGamepad != _isGamepadPrev;

			CurrentSensitivity = IsGamepad ? sensitivityGamepad : sensitivityMouse;
			CurrentSmoothSpeed = IsGamepad ? smoothSpeedGamepad : smoothSpeedMouse;

			if (IsGamepad)
			{
				_lookInput.x *= Mathf.Clamp(sensitivityGamepadCurve.Evaluate(Mathf.Abs(_lookInput.x)), 1f, 10f);
				_lookInput.y *= Mathf.Clamp(sensitivityGamepadCurve.Evaluate(Mathf.Abs(_lookInput.y)), 1f, 10f);
			}
		
			_sensitivityAmtX = IsGamepad ? SensitivityAmt * Time.deltaTime : SensitivityAmt;
			_sensitivityAmtY = IsGamepad ? SensitivityAmt * Time.deltaTime : SensitivityAmt;

			_lookInput.x *= _sensitivityAmtX;
			_lookInput.y *= _sensitivityAmtY * verticalSensitivityMultiplier;

			RotationX += _lookInput.x; // lower sensitivity at slower time settings
			RotationY += _lookInput.y;

			// reset vertical recoilY value if it would exceed maximumY amount 
			if (maximumYAngle - _lookInput.y < RecoilY)
			{
				RotationY += RecoilY;
				RecoilY = 0f;
			}

			// reset horizontal recoilX value if it would exceed maximumX amount 
			if (MaximumX - _lookInput.x < RecoilX)
			{
				RotationX += RecoilX;
				RecoilX = 0f;
			}

			RotationX = GameUtils.ClampAngle(RotationX, -maxXAngle, maxXAngle);
			RotationY = GameUtils.ClampAngle(RotationY, minimumYAngle - RecoilY, maximumYAngle - RecoilY);
			RotationZ = -_player.controller.LeaningComponent.LeanPos * _player.controller.LeaningComponent.LeaningConfig.rotationLeanAmt;

			InputY = RotationY + RecoilY; // set public inputY value for use in other scripts
			_xQuaternion = Quaternion.AngleAxis(RotationX + RecoilX, Vector3.up);
			_yQuaternion = Quaternion.AngleAxis(RotationY + RecoilY, -Vector3.right);
			_zQuaternion = Quaternion.AngleAxis(RotationZ, Vector3.forward);

			Quaternion newRotation = OriginalRotation * _xQuaternion * _yQuaternion * _zQuaternion;

			if (_inputDeviceChange)
				newRotation = _transform.rotation;

			if (Smooth && PlayerMovedTime + 0.1f < Time.time && !_inputDeviceChange)
			{
				// smoothing camera rotation
				_transform.rotation = Quaternion.Lerp(_transform.rotation, newRotation, CurrentSmoothSpeed * Time.smoothDeltaTime * 60f); 
			}
			else
			{
				// snap camera instantly to angles with no smoothing
				_transform.rotation = newRotation; 
			}

			_isGamepadPrev = IsGamepad;
		}

		// PUBLIC METHODS: ------------------------------------------------------------------------

		public void Init(Player player)
		{
			_player = player;
			player.GetComponent<Rigidbody>().freezeRotation = true;
			
			MaxXAngleDef = maxXAngle;
			_minimumYAngleDef = minimumYAngle;
			_maximumYAngleDef = maximumYAngle;
			_sensitivityMouseDef = sensitivityMouse;
			_sensitivityGamepadDef = sensitivityGamepad;
			_smoothSpeedMouseDef = smoothSpeedMouse;
			_smoothSpeedGamepadDef = smoothSpeedGamepad;
			_inputManager = InputManager.instance;
		}
		
		public void RestoreDefaults()
		{
			minimumYAngle = _minimumYAngleDef;
			maximumYAngle = _maximumYAngleDef;
			maxXAngle = MaxXAngleDef;
			sensitivityGamepad = _sensitivityGamepadDef;
			sensitivityMouse = _sensitivityMouseDef;
			SensitivityAmt = InputManager.isGamepad ? sensitivityGamepad : sensitivityMouse;
			smoothSpeedMouse = _smoothSpeedMouseDef;
			smoothSpeedGamepad = _smoothSpeedGamepadDef;
			RotationX = _transform.eulerAngles.x;
			RotationY = _transform.eulerAngles.y;
			RecoilY = 0f;
			RecoilX = 0f;
			InputY = 0f;
			PlayerMovedTime = 0f;
			OriginalRotation = Quaternion.Euler(_transform.eulerAngles.x, _transform.eulerAngles.y, 0f);
		}
	}

}