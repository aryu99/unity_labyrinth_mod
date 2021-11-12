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
    private float distanceThreshold = 0.5f;
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

    private Dictionary<int, (Vector3, Vector3)> taskDescriptions;

    private Dictionary<string, string> openWith = new Dictionary<string, string>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Dictionary of task descriptions. 
        // The key corresponds to the index of the (sub)task.
        // The value consists of two vectors. The first is the task's initial
        // location, the second is the task's target location.
        taskDescriptions = new Dictionary<int, (Vector3, Vector3)>();
        taskDescriptions.Add(-1, (new Vector3(2f, 0.5f, -2f), new Vector3(1f, 0.5f, -19.5f)));
        taskDescriptions.Add(0, (new Vector3(2f, 0.5f, -2f), new Vector3(4f, 0.5f, -6.5f)));
        taskDescriptions.Add(1, (new Vector3(2f, 0.5f, -2f), new Vector3(6.5f, 0.5f, -3f)));
        taskDescriptions.Add(2, (new Vector3(6.5f, 0.5f, -3f), new Vector3(10f, 0.5f, -6.5f)));
        taskDescriptions.Add(3, (new Vector3(6.5f, 0.5f, -3f), new Vector3(16f, 0.5f, -6.5f)));
        taskDescriptions.Add(4, (new Vector3(4f, 0.5f, -6.5f), new Vector3(6f, 0.5f, -12f)));
        taskDescriptions.Add(5, (new Vector3(6f, 0.5f, -12f), new Vector3(4f, 0.5f, -16.5f)));
        taskDescriptions.Add(6, (new Vector3(10f, 0.5f, -6.5f), new Vector3(12f, 0.5f, -15f)));
        taskDescriptions.Add(7, (new Vector3(12f, 0.5f, -15f), new Vector3(10f, 0.5f, -6.5f)));
        taskDescriptions.Add(8, (new Vector3(16f, 0.5f, -6.5f), new Vector3(17f, 0.5f, -16.5f)));
        taskDescriptions.Add(9, (new Vector3(4f, 0.5f, -16.5f), new Vector3(1f, 0.5f, -19f)));
        taskDescriptions.Add(10, (new Vector3(17f, 0.5f, -16.5f), new Vector3(10f, 0.5f, -18.5f)));
        taskDescriptions.Add(11, (new Vector3(10f, 0.5f, -18.5f), new Vector3(1f, 0.5f, -19.5f)));

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
        rb.velocity = new Vector3(0, 0, 0);

        // Get a random initial position centered around the initial location
        float pos_r = distanceThreshold * Random.Range(0f, 1f);
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

        if(distanceToSubTaskTarget <= distanceThreshold)
        {
            sideChannel.SendStringToPython("Completed sub task: " + currentSubTask.ToString());
        }

        if(distanceToTarget <= distanceThreshold)
        {
            SetReward(1f);
            sideChannel.SendStringToPython("Completed task");
            EndEpisode();
        }

        // transform.position += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;
    }

    private void OnTriggerEnter(Collider other){
        if(other.gameObject.CompareTag("Lava")){
            SetReward(-1f);
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
