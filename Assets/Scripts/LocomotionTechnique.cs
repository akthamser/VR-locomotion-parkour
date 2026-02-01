using UnityEngine;
using UnityEngine.UI;
public class LocomotionTechnique : MonoBehaviour
{
    [Header("Gauntlet Setup")]
    public Gauntlet leftGauntlet;
    public Gauntlet rightGauntlet;
    public OVRInput.Controller leftController = OVRInput.Controller.LTouch;
    public OVRInput.Controller rightController = OVRInput.Controller.RTouch;


    [Header("UI Display")]
    public Text leftText;  
    public Text rightText;
    public Image leftSliderImage;  
    public Image rightSliderImage; 



    [Header("Flight Physics")]
    public float thrustForce = 1500f;
    public float maxSpeed = 20f;
    public Rigidbody playerRigidbody;


    [Header("Control Feel")]
    public float brakingDrag = 5.0f;
    public float flyingDrag = 1.0f;
    [Range(0f, 1f)] public float lookBias = 0.2f;
    public Transform headCamera; 

    public GameObject hmd;
    public ParkourCounter parkourCounter;
    public string stage;
    public SelectionTaskMeasure selectionTaskMeasure;

    void Start()
    {
        if (playerRigidbody == null) playerRigidbody = GetComponent<Rigidbody>();
        if (headCamera == null) headCamera = Camera.main.transform; 
    }

    void FixedUpdate()
    {
        float leftValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, leftController);
        float rightValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, rightController);

        if (leftText != null)
            leftText.text = Mathf.RoundToInt(leftValue * 100).ToString();
        if (rightText != null)
            rightText.text = Mathf.RoundToInt(rightValue * 100).ToString();

        if (leftSliderImage != null)
        {
          
            leftSliderImage.fillAmount = Mathf.MoveTowards(leftSliderImage.fillAmount, leftValue, 10 * Time.deltaTime);
        }

        if (rightSliderImage != null)
        {
            rightSliderImage.fillAmount = Mathf.MoveTowards(rightSliderImage.fillAmount, rightValue, 10 * Time.deltaTime);
        }

        bool leftThrusting = leftValue > 0.1f;
        bool rightThrusting = rightValue > 0.1f;
        bool isThrusting = leftThrusting || rightThrusting;

        bool isSingleHanded = (leftThrusting && !rightThrusting) || (!leftThrusting && rightThrusting);

        if (isThrusting)
        {

            playerRigidbody.linearDamping = flyingDrag; 

            if (leftThrusting) ApplyThrust(leftGauntlet, leftValue, leftController, isSingleHanded);
            if (rightThrusting) ApplyThrust(rightGauntlet, rightValue, rightController, isSingleHanded);

            // Cap Speed
            if (playerRigidbody.linearVelocity.magnitude > maxSpeed) 
            {
                playerRigidbody.linearVelocity = playerRigidbody.linearVelocity.normalized * maxSpeed;
            }
        }
        else
        {

            playerRigidbody.linearDamping = brakingDrag;

            // Turn off effects
            leftGauntlet.Stop();
            rightGauntlet.Stop();
            OVRInput.SetControllerVibration(0, 0, leftController);
            OVRInput.SetControllerVibration(0, 0, rightController);
        }
    }

    void ApplyThrust(Gauntlet gauntlet, float value, OVRInput.Controller controller, bool isSingleHanded)
    {

        Vector3 handDir = gauntlet.GetDirection();
        Vector3 lookDir = headCamera.forward;

        Vector3 finalDir = Vector3.Lerp(handDir, lookDir, lookBias).normalized;


        float finalMultiplier = 1.0f;


        float forwardDot = Vector3.Dot(handDir, lookDir);
        if (forwardDot > 0.5f)
        {
            finalMultiplier = 1.25f;
        }


        float verticalDot = Vector3.Dot(handDir, Vector3.up);
        if (isSingleHanded && verticalDot > 0.7f)
        {
            finalMultiplier = 1.5f;
        }

        // Apply Force with Multiplier
        playerRigidbody.AddForce(finalDir * thrustForce * value * finalMultiplier * Time.deltaTime, ForceMode.Force);

        // Visuals
        gauntlet.Play(value);
        OVRInput.SetControllerVibration(0.1f, value * 0.4f, controller);
    }

    void Update()
    {
        // These are for the game mechanism.
        if (OVRInput.Get(OVRInput.Button.Two) || OVRInput.Get(OVRInput.Button.Four))
        {
            if (parkourCounter.parkourStart)
            {
                transform.position = parkourCounter.currentRespawnPos;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {

        // These are for the game mechanism.
        if (other.CompareTag("banner"))
        {
            stage = other.gameObject.name;
            parkourCounter.isStageChange = true;
        }
        else if (other.CompareTag("objectInteractionTask"))
        {
            selectionTaskMeasure.isTaskStart = true;
            selectionTaskMeasure.scoreText.text = "";
            selectionTaskMeasure.partSumErr = 0f;
            selectionTaskMeasure.partSumTime = 0f;
            // rotation: facing the user's entering direction
            float tempValueY = other.transform.position.y > 0 ? 12 : 0;
            Vector3 tmpTarget = new(hmd.transform.position.x, tempValueY, hmd.transform.position.z);
            selectionTaskMeasure.taskUI.transform.LookAt(tmpTarget);
            selectionTaskMeasure.taskUI.transform.Rotate(new Vector3(0, 180f, 0));
            selectionTaskMeasure.taskStartPanel.SetActive(true);
        }
        else if (other.CompareTag("coin"))
        {
            parkourCounter.coinCount += 1;
            GetComponent<AudioSource>().Play();
            other.gameObject.SetActive(false);
        }
        // These are for the game mechanism.
    }
}