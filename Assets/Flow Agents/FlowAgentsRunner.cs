using UnityEngine;
using System.Runtime.InteropServices;

public class FlowAgentsRunner : MonoBehaviour
{
    public ComputeShader FlowAgentsShader;

    [Header("Initial values")]
    [SerializeField] private int agentCount = 8;
    [SerializeField] private int textureDimension = 1024;
    [Tooltip("Green channel gives the angle of the vectors")]
    [SerializeField] private Texture VectorField;

    [Header("Runtime values")]
    public FlowType flowType;
    [Range(0f, 1f)] public float decay = 0.01f;

    struct Agent
    {
	    Vector2 pos; 	// between 0-1
        Vector2 velocity;
        float speed;
        Vector2 enforcingDirection;
    };

    public enum FlowType{
        LeftToRight = 0,
        CenterToOut = 1,
        RandomToRandom = 2, 
        CenterToHorizontal = 3,
        HorizontalEdgesToCenter = 4
    }
    private const int threadGroupCount = 8;
    private int initAgentsHandle, updateAgentsHandle, updateTrailHandle;

    private ComputeBuffer agentBuffer;

    private RenderTexture trailTexture;

    void OnValidate()
    {
        if(agentCount < threadGroupCount)
        {
            agentCount = threadGroupCount;
        }
    }
    void Start()
    {
        if(FlowAgentsShader == null)
        {
            Debug.LogError("Assign FlowAgents.compute shader");
            this.enabled = false;
            return;
        }

        if(VectorField == null)
        {
            Debug.LogError("Assign Vector Field texture");
            this.enabled = false;
            return;
        }

        initAgentsHandle = FlowAgentsShader.FindKernel("InitAgents");
        updateAgentsHandle = FlowAgentsShader.FindKernel("UpdateAgents");
        updateTrailHandle = FlowAgentsShader.FindKernel("UpdateTrail");

        InitializeAgents();
        InitializeVectorField();
        InitializeTrail();
    }
    //initializes buffer and fills values in the compute shader
    void InitializeAgents()
    {
        Agent[] agentsData = new Agent[agentCount];
        agentBuffer = new ComputeBuffer(agentsData.Length,  Marshal.SizeOf(typeof(Agent)));
        agentBuffer.SetData(agentsData);

        UpdateParameters();
        FlowAgentsShader.SetBuffer(initAgentsHandle, "AgentBuffer", agentBuffer);
        FlowAgentsShader.SetBuffer(updateAgentsHandle, "AgentBuffer", agentBuffer);
        FlowAgentsShader.Dispatch(initAgentsHandle, agentCount/threadGroupCount, 1, 1);
    }

    void InitializeTrail()
    {
        trailTexture = new RenderTexture(textureDimension, textureDimension, 24);
        trailTexture.enableRandomWrite = true;
        trailTexture.Create();

        var rend = GetComponent<Renderer>();
        rend.material.mainTexture = trailTexture;
        rend.material.SetTexture("_ModTex", trailTexture);

        UpdateParameters();
        FlowAgentsShader.SetVector("trailDimension", Vector2.one * textureDimension); 
        FlowAgentsShader.SetTexture(updateAgentsHandle, "TrailTexture", trailTexture);
        FlowAgentsShader.SetTexture(updateTrailHandle, "TrailTexture", trailTexture);
    }

    void InitializeVectorField()
    {
        FlowAgentsShader.SetVector("vectorFieldDimension", new Vector2(VectorField.width, VectorField.height));
        FlowAgentsShader.SetTexture(updateAgentsHandle, "VectorFieldTexture", VectorField);
    }
    // Update is called once per frame
    void Update()
    {
        UpdateParameters();
        FlowAgentsShader.Dispatch(updateAgentsHandle, agentCount/threadGroupCount, 1, 1);
        FlowAgentsShader.Dispatch(updateTrailHandle, textureDimension/threadGroupCount, textureDimension/threadGroupCount, 1);
    }

    void UpdateParameters()
    {
        FlowAgentsShader.SetInt("flowType", (int)flowType);
        FlowAgentsShader.SetFloat("decay", decay);
    }

    void OnDestroy()
    {
        if(agentBuffer != null)
        {
            agentBuffer.Dispose();
        }
    }
}
