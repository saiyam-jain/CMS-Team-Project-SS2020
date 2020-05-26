using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
	[Header("Initial values")]
	public int dimension = 1024;
	public int numberOfParticles = 524280;
	[SerializeField] private ComputeShader shader;

	[Header("Run time parameters")]
	[Range(0f, 1.0f)] public float startRadius = 0.5f;
	[Range(0f, 1f)] public float deposit = 1.0f;
	[Range(0f, 1f)] public float decay = 0.002f;
	[Range(0f, 180f)] public float sensorAngleDegrees = 45f;  //in degrees
	[Range(0f, 180f)] public float rotationAngleDegrees = 45f;//in degrees
	[Range(0f, 0.1f)] public float sensorOffsetDistance = 0.01f;
	[Range(0f, 0.01f)] public float stepSize = 0.001f;

	private float sensorAngle;              //in radians
	private float rotationAngle;            //in radians

	private RenderTexture trail;
	private int initHandle, particleHandle, trailHandle;
	private ComputeBuffer particleBuffer;

	private static int GroupCount = 8;       // Group size has to be same with the compute shader group size

	struct Particle
	{
		public Vector2 point;
		public float angle;

		public Particle(Vector2 pos, float angle)
		{
			point = pos;
			this.angle = angle;
		}
	};

	void OnValidate() // Called by Unity when someone changes a value in the Editor
	{
		if (dimension < GroupCount) dimension = GroupCount;
	}

	// Start is called before the first frame update
	void Start()
	{
		if (shader == null)
		{
			Debug.LogError("PhysarumSurface shader has to be assigned for PhysarumBehaviour to work.");
			this.enabled = false;
			return;
		}

		// Compute shader connections...
		initHandle = shader.FindKernel("Init");
		particleHandle = shader.FindKernel("MoveParticles");
		trailHandle = shader.FindKernel("StepTrail");

		UpdateRuntimeParameters();
		InitializeParticles();
		InitializeTrail();
	}

	void InitializeParticles()
	{
		if (numberOfParticles > GroupCount * 65535) numberOfParticles = GroupCount * 65535;

		Debug.Log("Particles: " + numberOfParticles + "Thread groups: " + numberOfParticles / GroupCount);

		Particle[] data = new Particle[numberOfParticles];
		particleBuffer = new ComputeBuffer(data.Length, 12);
		particleBuffer.SetData(data);

		//initialize particles with random positions
		shader.SetInt("numberOfParticles", numberOfParticles);
		shader.SetVector("trailDimension", Vector2.one * dimension);
		shader.SetBuffer(initHandle, "particleBuffer", particleBuffer);

		Dispatch(initHandle, numberOfParticles / GroupCount, 1, 1);

		shader.SetBuffer(particleHandle, "particleBuffer", particleBuffer);
	}

	void InitializeTrail()
	{
		// By default, a ARGB32 texture is created. Currently we are only using the R channel,
		// so this could be changed to just RenderTextureFormat.R8
		// But note that you must also change RWTexture2D<float4> TrailBuffer; declaration in the compute shader 
		trail = new RenderTexture(dimension, dimension, 24); //, RenderTextureFormat.R8); //, RenderTextureFormat.ARGBFloat);
		trail.enableRandomWrite = true;
		trail.Create();
		Debug.Log(trail.format);

		// Set the TrailBuffer as the texture of the material of this objects
		var rend = GetComponent<Renderer>();
		rend.material.mainTexture = trail;

		shader.SetTexture(particleHandle, "TrailBuffer", trail);
		shader.SetTexture(trailHandle, "TrailBuffer", trail);
	}

	// Update is called once per frame
	void Update()
	{
		UpdateRuntimeParameters();
		UpdateParticles();
		UpdateTrail();
	}

	void UpdateRuntimeParameters()
	{
		sensorAngle = sensorAngleDegrees * 0.0174533f;
		rotationAngle = rotationAngleDegrees * 0.0174533f;
		shader.SetFloat("sensorAngle", sensorAngle);
		shader.SetFloat("rotationAngle", rotationAngle);
		shader.SetFloat("sensorOffsetDistance", sensorOffsetDistance);
		shader.SetFloat("stepSize", stepSize);
		shader.SetFloat("decay", decay);
		shader.SetFloat("deposit", deposit);
		shader.SetFloat("startRadius", startRadius);
	}

	void UpdateParticles()
	{
		Dispatch(particleHandle, numberOfParticles / GroupCount, 1, 1);
	}

	void UpdateTrail()
	{
		Dispatch(trailHandle, dimension / GroupCount, dimension / GroupCount, 1);
	}

	void Dispatch(int kernelIndex, int threadGroupsX, int threadGroupsY, int threadGroupsZ)
	{
		shader.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, threadGroupsZ);
	}

	void OnDestroy()
	{
		if (particleBuffer != null) particleBuffer.Release();
	}
}
// public class Physarum : SimulationManager