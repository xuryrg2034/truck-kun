using UnityEngine;

namespace Code.Hub
{
  public class HubController : MonoBehaviour
  {
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 120f;

    private CharacterController _characterController;
    private Transform _cameraTransform;

    public void Initialize()
    {
      _characterController = GetComponent<CharacterController>();
      if (_characterController == null)
        _characterController = gameObject.AddComponent<CharacterController>();

      _characterController.height = 2f;
      _characterController.radius = 0.5f;
      _characterController.center = new Vector3(0f, 1f, 0f);

      gameObject.tag = "Player";
    }

    public void SetCamera(Transform cameraTransform)
    {
      _cameraTransform = cameraTransform;
    }

    private void Update()
    {
      HandleMovement();
      HandleRotation();
    }

    private void HandleMovement()
    {
      float horizontal = Input.GetAxis("Horizontal");
      float vertical = Input.GetAxis("Vertical");

      Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

      if (direction.magnitude >= 0.1f)
      {
        Vector3 moveDirection = transform.TransformDirection(direction);
        moveDirection.y = 0f;

        // Apply gravity
        moveDirection.y = -9.81f * Time.deltaTime;

        _characterController.Move(moveDirection * _moveSpeed * Time.deltaTime);
      }
      else
      {
        // Just apply gravity
        _characterController.Move(new Vector3(0f, -9.81f * Time.deltaTime, 0f));
      }
    }

    private void HandleRotation()
    {
      float mouseX = Input.GetAxis("Mouse X");
      transform.Rotate(Vector3.up, mouseX * _rotationSpeed * Time.deltaTime);
    }

    public static HubController Create(Vector3 position)
    {
      GameObject playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      playerObj.name = "Player";
      playerObj.transform.position = position;
      playerObj.tag = "Player";

      // Remove default collider (CharacterController will handle collision)
      Collider defaultCollider = playerObj.GetComponent<Collider>();
      if (defaultCollider != null)
        Destroy(defaultCollider);

      // Visual
      Renderer renderer = playerObj.GetComponent<Renderer>();
      Material mat = new Material(Shader.Find("Standard"));
      mat.color = new Color(0.2f, 0.5f, 0.8f);
      renderer.material = mat;

      HubController controller = playerObj.AddComponent<HubController>();
      controller.Initialize();

      return controller;
    }
  }
}
