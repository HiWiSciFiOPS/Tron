using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : NetworkBehaviour {

	public MeshFilter line;

	// general variables
	public byte ID = 0;
	private bool _dead = false;
	public bool dead { get { return _dead; } set { _dead = value; StopCoroutine(sendData()); } }

	// movement variables
	private const float movingSpeed = 5.0f;
	private const float boostMultiplier = 2.5f;
	private const int boostCooldownSeconds = 20;
	private const int boostDurationSeconds = 3;
	private bool boostAvailable = true;
	public byte boosted = 0;
	public bool moveable = false;

	public List<Vector2> linePoints = new List<Vector2>();

	// mouse variables
	private float rotY = 0f;
	// private Quaternion q = new Quaternion(0, 0, 0, Quaternion.Identity);
	
	void Awake() {
		
	}

	public void Initialize(bool local, Color color, byte ID)
	{
		GameObject go = new GameObject();
		go.name = "lineInstance";
		line = go.AddComponent<MeshFilter>();
		MeshRenderer mr = go.AddComponent<MeshRenderer>();
		mr.sharedMaterial = Resources.Load<Material>("LineMaterial");

		this.local = local;
		this.ID = ID;
		setColor(color);

		if (local)
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;

			// set rotX for mouse rotation
			rotY = transform.localRotation.eulerAngles.y;

			StartCoroutine(sendData());
		}
		else
		{
			Debug.Log("deactivating camera");
			GetComponentInChildren<Camera>().gameObject.SetActive(false);
		}
	}
	
	private void Update() {
		
		if (!dead)
		{
			if (local)
			{
				// turn player with mouse
				float mouseX = Input.GetAxis("Mouse X");
				rotY += mouseX * PlayerSettings.mouseSpeed;
				transform.rotation = Quaternion.Euler(0f, rotY, 0f);

				// boost
				if (Input.GetKeyDown(KeyCode.Space) && boostAvailable)
				{
					StartCoroutine(BoostCountdown());
				}
			}
			else
			{
				// not local player
			}

			if (moveable)
			{
				// move player forward
				transform.Translate(new Vector3(0, 0, movingSpeed * Time.deltaTime + boosted * boostMultiplier * movingSpeed * Time.deltaTime), Space.Self);
			}
		}

		if (local && Input.GetKeyDown(KeyCode.Escape))
		{
			NetworkCommunication.Disconnect();
		}
	}

	IEnumerator sendData()
	{
		while (true)
		{
			Debug.Log("Sending updated Data to server");
			NetworkCommunication.SendUpdate(this);
			linePoints.Add(new Vector2(transform.position.x, transform.position.z));
			line.GetComponent<MeshFilter>().sharedMesh = Line.GenerateMesh(linePoints, 1, 0);
			yield return new WaitForSeconds(0.1f);
		}
	}

	private IEnumerator BoostCountdown()
	{
		boostAvailable = false;
		boosted = 1;
		yield return new WaitForSeconds(boostDurationSeconds);
		boosted = 0;
		yield return new WaitForSeconds(boostCooldownSeconds);
		boostAvailable = true;
	}

	public void setColor(Color color)
	{
		MaterialPropertyBlock block = new MaterialPropertyBlock();
		block.SetColor("_BaseColor", color);
		GetComponentInChildren<MeshRenderer>().SetPropertyBlock(block);
	}
}