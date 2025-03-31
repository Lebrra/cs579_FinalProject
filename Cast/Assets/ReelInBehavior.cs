using UnityEngine;
using UnityEngine.InputSystem;

public class ReelInBehavior : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform player;
    public float speed = 5f;
    private bool isReeling = false;
    private PlayerInput controls;
    void Start()
    {
        
    }
     void Awake()
    {
        controls = new PlayerInput();
    }
    private void OnEnable()
    {
        controls.Enable();
        controls.Gameplay.ReelIn.performed += ctx => StartReeling();
    }
    private void OnDisable()
    {
       controls.Gameplay.ReelIn.performed -= ctx => StartReeling();
       controls.Disable();
    }

    private void StartReeling()
    {
        isReeling = true;
    }

    // Update is called once per frame
    void Update()
    {
       
        if (isReeling && player != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

            // Stop movement if close enough
            if (Vector3.Distance(transform.position, player.position) < 0.5f)
            {
                isReeling = false;
            }
        }
    }
}
