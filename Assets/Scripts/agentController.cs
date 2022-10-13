using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class agentController : Agent
{
    private float moveSpeed = 7f;
    private float distanceThreshold = 0.3f;
    private float velThreshold = 0.25f;
    private float initialStateDistBuffer = 0.15f;
    private float initialStateVelBuffer = 0.15f;
    [SerializeField] private Transform targetTransform;
    private Rigidbody rb;
    private float distanceToTarget;
    private float distanceToSubTaskTarget;
    CustomSideChannel sideChannel;
    private int currentTask;
    private int currentSubTask;
    private Vector3 initLocation;
    private Vector3 targetLocation;
    private Vector3 subTaskTargetLocation;

    // The initial and target locations for the overall compositional task.
    private Vector3 overallInitLocation = new Vector3(-8.5f, 0.5f, 8.0f);
    private Vector3 overallTargetLocation = new Vector3(-8.5f, 0.5f, -28.5f);

    private Dictionary<int, (Vector3, Vector3)> taskDescriptions;

    private Dictionary<string, string> openWith = new Dictionary<string, string>();

    // Grab all of the locations of the subtask entry and exit locations (HLM states)
    [SerializeField] private Transform s0;
    [SerializeField] private Transform s1;
    [SerializeField] private Transform s2;
    [SerializeField] private Transform s3;
    [SerializeField] private Transform s4;
    [SerializeField] private Transform s5;
    [SerializeField] private Transform s6;
    [SerializeField] private Transform s7;
    [SerializeField] private Transform s8;
    [SerializeField] private Transform s9;
    [SerializeField] private Transform s10;
    [SerializeField] private Transform s11;
    [SerializeField] private Transform s12;
    [SerializeField] private Transform s13;
    [SerializeField] private Transform s14;
    [SerializeField] private Transform s15;
    [SerializeField] private Transform s16;
    [SerializeField] private Transform s17;
    [SerializeField] private Transform s18;
    [SerializeField] private Transform s19;
    [SerializeField] private Transform s20;
    [SerializeField] private Transform s21;
    [SerializeField] private Transform s22;
    [SerializeField] private Transform s23;
    [SerializeField] private Transform s24;
    [SerializeField] private Transform s25;
    [SerializeField] private Transform s26;
    [SerializeField] private Transform s27;
    [SerializeField] private Transform s28;
    [SerializeField] private Transform s29;
    [SerializeField] private Transform s30;
    [SerializeField] private Transform s31;
    [SerializeField] private Transform s32;
    [SerializeField] private Transform s33;
    [SerializeField] private Transform s35;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Dictionary of task descriptions. 
        // The key corresponds to the index of the (sub)task.
        // The value consists of two vectors. The first is the task's initial
        // location, the second is the task's target location.
        taskDescriptions = new Dictionary<int, (Vector3, Vector3)>();
        taskDescriptions.Add(-1, (overallInitLocation, overallTargetLocation));
        taskDescriptions.Add(0, (overallInitLocation, s0.position));
        taskDescriptions.Add(1, (overallInitLocation, s1.position));
        taskDescriptions.Add(2, (s1.position, s22.position));
        taskDescriptions.Add(3, (s1.position, s21.position));
        taskDescriptions.Add(4, (s1.position, s3.position));
        taskDescriptions.Add(5, (s1.position, s2.position));
        taskDescriptions.Add(6, (s0.position, s4.position));
        taskDescriptions.Add(7, (s4.position, s5.position));
        taskDescriptions.Add(8, (s5.position, s8.position));
        taskDescriptions.Add(9, (s5.position, s9.position));
        taskDescriptions.Add(10, (s8.position, s9.position));
        taskDescriptions.Add(11, (s9.position, s12.position));
        taskDescriptions.Add(12, (s9.position, s11.position));
        taskDescriptions.Add(13, (s11.position, s33.position));
        taskDescriptions.Add(14, (s12.position, s13.position));
        taskDescriptions.Add(15, (s13.position, s33.position));
        taskDescriptions.Add(16, (s20.position, s35.position));
        taskDescriptions.Add(17, (s29.position, s35.position));
        taskDescriptions.Add(18, (s2.position, s6.position));
        taskDescriptions.Add(19, (s6.position, s8.position));
        taskDescriptions.Add(20, (s6.position, s10.position));
        taskDescriptions.Add(21, (s10.position, s19.position));
        taskDescriptions.Add(22, (s7.position, s17.position));
        taskDescriptions.Add(23, (s3.position, s7.position));
        taskDescriptions.Add(24, (s3.position, s14.position));
        taskDescriptions.Add(25, (s15.position, s16.position));
        taskDescriptions.Add(26, (s16.position, s14.position));
        taskDescriptions.Add(27, (s14.position, s18.position));
        taskDescriptions.Add(28, (s21.position, s23.position));
        taskDescriptions.Add(29, (s22.position, s25.position));
        taskDescriptions.Add(30, (s22.position, s26.position));
        taskDescriptions.Add(31, (s26.position, s29.position));
        taskDescriptions.Add(32, (s26.position, s32.position));
        taskDescriptions.Add(33, (s25.position, s28.position));
        taskDescriptions.Add(34, (s25.position, s27.position));
        taskDescriptions.Add(35, (s25.position, s24.position));
        taskDescriptions.Add(36, (s18.position, s24.position));
        taskDescriptions.Add(37, (s28.position, s31.position));
        taskDescriptions.Add(38, (s31.position, s27.position));
        taskDescriptions.Add(39, (s27.position, s24.position));
        taskDescriptions.Add(40, (s32.position, s31.position));
        taskDescriptions.Add(41, (s32.position, s30.position));
        taskDescriptions.Add(42, (s24.position, s20.position));
        taskDescriptions.Add(43, (s19.position, s20.position));
        taskDescriptions.Add(44, (s14.position, s7.position));
        taskDescriptions.Add(45, (s8.position, s10.position));
        taskDescriptions.Add(46, (s3.position, s15.position));
        taskDescriptions.Add(47, (s35.position, s33.position));
        taskDescriptions.Add(48, (s7.position, s3.position));
        taskDescriptions.Add(49, (s10.position, s6.position));


        // taskDescriptions.Add(-1, (new Vector3(-8.5f, 0.5f, 8.0f), new Vector3(-8.5f, 0.5f, -28.5f)));
        // taskDescriptions.Add(0, (new Vector3(-8.5f, 0.5f, 8.0f), new Vector3(-4.7f, 0.5f, 2.8f)));
        // taskDescriptions.Add(1, (new Vector3(-8.5f, 0.5f, 8.0f), new Vector3(-2.7f, 0.5f, 6.5f)));
        // taskDescriptions.Add(2, (new Vector3(-2.7f, 0.5f, 6.5f), new Vector3(16.0f, 0.5f, 8.5f)));
        // taskDescriptions.Add(3, (new Vector3(-2.7f, 0.5f, 6.5f), new Vector3(16.0f, 0.5f, 3.5f)));
        // taskDescriptions.Add(4, (new Vector3(-2.7f, 0.5f, 6.5f), new Vector3(6.4f, 0.5f, 1.85f)));
        // taskDescriptions.Add(5, (new Vector3(-2.7f, 0.5f, 6.5f), new Vector3(0.45f, 0.5f, 1.8f)));
        // taskDescriptions.Add(6, (new Vector3(-4.7f, 0.5f, 2.8f), new Vector3(-3.55f, 0.5f, -3.65f)));
        // taskDescriptions.Add(7, (new Vector3(-3.55f, 0.5f, -3.65f), new Vector3(-5.55f, 0.5f, -8.1f)));
        // taskDescriptions.Add(8, (new Vector3(-5.55f, 0.5f, -8.1f), new Vector3(-2.8f, 0.5f, -13.25f)));
        // taskDescriptions.Add(9, (new Vector3(-5.55f, 0.5f, -8.1f), new Vector3(-6.2f, 0.5f, -17.75f)));
        // taskDescriptions.Add(10, (new Vector3(-2.8f, 0.5f, -13.25f), new Vector3(-6.2f, 0.5f, -17.75f)));
        // taskDescriptions.Add(11, (new Vector3(-6.2f, 0.5f, -17.75f), new Vector3(-2.8f, 0.5f, -22f)));
        // taskDescriptions.Add(12, (new Vector3(-6.2f, 0.5f, -17.75f), new Vector3(-7.6f, 0.5f, -24.8f)));
        // taskDescriptions.Add(13, (new Vector3(-7.6f, 0.5f, -24.8f), new Vector3(-8.5f, 0.5f, -28.5f)));
        // taskDescriptions.Add(14, (new Vector3(-2.8f, 0.5f, -22f), new Vector3(1.9f, 0.5f, -24.7f)));
        // taskDescriptions.Add(15, (new Vector3(1.9f, 0.5f, -24.7f), new Vector3(-8.5f, 0.5f, -28.5f)));
        // taskDescriptions.Add(16, (new Vector3(14.6f, 0.5f, -24.7f), new Vector3(4f, 0.5f, -27.5f)));
        // // taskDescriptions.Add(16, (new Vector3(14.6f, 0.5f, -24.7f), new Vector3(-8.5f, 0.5f, -28.5f)));
        // taskDescriptions.Add(17, (new Vector3(16.1f, 0.5f, -28.7f), new Vector3(-8.5f, 0.5f, -28.5f)));
        // taskDescriptions.Add(18, (new Vector3(0.45f, 0.5f, 1.8f), new Vector3(2.45f, 0.5f, -8f)));
        // taskDescriptions.Add(19, (new Vector3(2.45f, 0.5f, -8f), new Vector3(-2.8f, 0.5f, -13.25f)));
        // taskDescriptions.Add(20, (new Vector3(2.45f, 0.5f, -8f), new Vector3(4.0f, 0.5f, -15.3f)));
        // taskDescriptions.Add(21, (new Vector3(4.0f, 0.5f, -15.3f), new Vector3(9.5f, 0.5f, -23.0f)));
        // taskDescriptions.Add(22, (new Vector3(7.7f, 0.5f, -8.15f), new Vector3(5.0f, 0.5f, -12.3f)));
        // taskDescriptions.Add(23, (new Vector3(6.4f, 0.5f, 1.85f), new Vector3(7.7f, 0.5f, -8.15f)));
        // taskDescriptions.Add(24, (new Vector3(6.4f, 0.5f, 1.85f), new Vector3(10.8f, 0.5f, -6.6f)));
        // taskDescriptions.Add(25, (new Vector3(10.8f, 0.5f, 0.0f), new Vector3(14.7f, 0.5f, -3.5f)));
        // taskDescriptions.Add(26, (new Vector3(14.7f, 0.5f, -3.5f), new Vector3(10.8f, 0.5f, -6.6f)));
        // taskDescriptions.Add(27, (new Vector3(10.8f, 0.5f, -6.6f), new Vector3(14.3f, 0.5f, -8.2f)));
        // taskDescriptions.Add(28, (new Vector3(16.0f, 0.5f, 3.5f), new Vector3(23.8f, 0.5f, 4.5f)));
        // taskDescriptions.Add(29, (new Vector3(16.0f, 0.5f, 8.5f), new Vector3(17.8f, 0.5f, -5.2f)));
        // taskDescriptions.Add(30, (new Vector3(16.0f, 0.5f, 8.5f), new Vector3(28.2f, 0.5f, -5.2f)));
        // taskDescriptions.Add(31, (new Vector3(28.2f, 0.5f, -5.2f), new Vector3(16.1f, 0.5f, -28.7f)));
        // taskDescriptions.Add(32, (new Vector3(28.2f, 0.5f, -5.2f), new Vector3(26.3f, 0.5f, -25.8f)));
        // taskDescriptions.Add(33, (new Vector3(17.8f, 0.5f, -5.2f), new Vector3(21.6f, 0.5f, -7.3f)));
        // taskDescriptions.Add(34, (new Vector3(17.8f, 0.5f, -5.2f), new Vector3(21.6f, 0.5f, -21.5f)));
        // taskDescriptions.Add(35, (new Vector3(17.8f, 0.5f, -5.2f), new Vector3(16.0f, 0.5f, -13.6f)));
        // taskDescriptions.Add(36, (new Vector3(14.3f, 0.5f, -8.2f), new Vector3(16.0f, 0.5f, -13.6f)));
        // taskDescriptions.Add(37, (new Vector3(21.6f, 0.5f, -7.3f), new Vector3(25.1f, 0.5f, -22.9f)));
        // taskDescriptions.Add(38, (new Vector3(25.1f, 0.5f, -22.9f), new Vector3(21.6f, 0.5f, -21.5f)));
        // taskDescriptions.Add(39, (new Vector3(21.6f, 0.5f, -21.5f), new Vector3(16.0f, 0.5f, -13.6f)));
        // taskDescriptions.Add(40, (new Vector3(26.3f, 0.5f, -25.8f), new Vector3(25.1f, 0.5f, -22.9f)));
        // taskDescriptions.Add(41, (new Vector3(26.3f, 0.5f, -25.8f), new Vector3(17.2f, 0.5f, -25.2f)));
        // taskDescriptions.Add(42, (new Vector3(16.0f, 0.5f, -13.6f), new Vector3(14.6f, 0.5f, -24.7f)));
        // taskDescriptions.Add(43, (new Vector3(9.5f, 0.5f, -23.0f), new Vector3(14.6f, 0.5f, -24.7f)));
        // taskDescriptions.Add(44, (new Vector3(10.8f, 0.5f, -6.6f), new Vector3(7.7f, 0.5f, -8.15f)));
        // taskDescriptions.Add(45, (new Vector3(-2.8f, 0.5f, -13.25f), new Vector3(4.0f, 0.5f, -15.3f)));
        // taskDescriptions.Add(46, (new Vector3(6.4f, 0.5f, 1.85f), new Vector3(10.8f, 0.5f, 0.0f)));
        // taskDescriptions.Add(47, (new Vector3(4f, 0.5f, -27.5f), new Vector3(-8.5f, 0.5f, -28.5f)));

        // Set the current task to the value representing the overall task
        currentTask = -1;
        SetCurrentTask(currentTask);

        // Set the current subtask to the value representing the overall task
        currentSubTask = -1;
        SetCurrentSubTask(currentSubTask);

        // Instantiate and register the custom side channel
        sideChannel = new CustomSideChannel();
        SideChannelManager.RegisterSideChannel(sideChannel);
        sideChannel.MessageToPass += OnMessageReceived;

    }

    public void OnMessageReceived(object sender, MessageEventArgs msg)
    {
        // Message should be two integers separated by a comma.
        // The first integer corresponds to the current task to solve.
        // The second integer corresponds to the current sub-task to solve.
        string message = msg.message;
        string[] split_message = message.Split(',');

        currentTask = System.Int32.Parse(split_message[0]);
        currentSubTask = System.Int32.Parse(split_message[1]);

        SetCurrentTask(currentTask);
        SetCurrentSubTask(currentSubTask);

        Debug.Log(currentSubTask);
    }

    public override void OnEpisodeBegin()
    {
        // Get a random initial velocity
        float vel_r = (velThreshold + initialStateVelBuffer) * Random.Range(0f, 1f);
        float vel_theta = 2f * Mathf.PI * Random.Range(0f, 1f);
        float vel_x = vel_r * Mathf.Cos(vel_theta);
        float vel_z = vel_r * Mathf.Sin(vel_theta);

        // rb.velocity = new Vector3(0, 0, 0);
        rb.velocity = new Vector3(vel_x, 0, vel_z);

        // Get a random initial position centered around the initial location
        float pos_r = (distanceThreshold + initialStateDistBuffer) * Random.Range(0f, 1f);
        float pos_theta = 2f * Mathf.PI * Random.Range(0f, 1f);
        float pos_x = initLocation.x + pos_r * Mathf.Cos(pos_theta);
        float pos_z = initLocation.z + pos_r * Mathf.Sin(pos_theta);

        transform.position = new Vector3(pos_x, initLocation.y, pos_z);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(subTaskTargetLocation);
        // sensor.AddObservation(targetTransform.position);
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        rb.AddForce(new Vector3(moveX, 0, moveZ) * moveSpeed);

        distanceToTarget = 
            Vector3.Distance(this.transform.position, targetTransform.position);
        AddReward(-distanceToTarget);

        distanceToSubTaskTarget = 
            Vector3.Distance(this.transform.position, subTaskTargetLocation);

        // Compute current speed.
        float vel = Mathf.Sqrt((rb.velocity.x * rb.velocity.x) + (rb.velocity.y * rb.velocity.y));

        if((distanceToSubTaskTarget <= distanceThreshold) && (vel <= velThreshold)) 
        {
            sideChannel.SendStringToPython("Completed sub task: " + currentSubTask.ToString());
        }

        if((distanceToTarget <= distanceThreshold) && (vel <= velThreshold))
        {
            SetReward(100f);
            sideChannel.SendStringToPython("Completed task");
            EndEpisode();
        }

        // transform.position += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;
    }

    private void OnTriggerEnter(Collider other){
        if(other.gameObject.CompareTag("Lava")){
            SetReward(-100f);
            sideChannel.SendStringToPython("Failed task");
            EndEpisode();
        }
    }

    // Set the environment task
    public void SetCurrentTask(int currentTask)
    {
        (initLocation, targetLocation) = taskDescriptions[currentTask];
        targetTransform.position = targetLocation;
    }

    public void SetCurrentSubTask(int currentSubTask)
    {
        (_, subTaskTargetLocation) = taskDescriptions[currentSubTask];
    }

    // Defining a basic heuristic for the agent when not learning
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

}
