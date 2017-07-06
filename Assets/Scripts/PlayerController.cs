using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {

	public float jetSpeed = 10f;
	public float maxSpeed = 50f;
	public float turnSpeed = 60f;
	public float sensX = 1f;
	public float sensY = 1f;

	public float magPow = 10f;
	public float magRange = 10f;
	public float shootPow = 10f;

	float lastShot;
	public float shotCooldown = 1f;

	CharacterController controller;

	Vector3 moveVector;
	Vector3 rotVector;
	float maxRot = 5f;
	bool stahp;

	public GUIText GUIDebug;
	
	enum parts { HEAD, TORSO, LEFTARM, RIGHTARM};
	GameObject[] body = new GameObject[4];

	public Camera myCam;
	bool firstPerson = true;
	Vector3 firstPos = new Vector3( 0f, 1.5f, 0f);
	Vector3 thirdPos = new Vector3( 0f, 2.5f, -10f);
	
	public ParticleSystem jet;
	public ParticleSystem attract;

	public GameObject debris;

	public GameObject magCan;
	public GameObject canPrefab;

	public AudioSource[] myAudio;

	void Start(){
		controller = GetComponent<CharacterController> ();
		Screen.lockCursor = true;

		body[(int)parts.HEAD] = GameObject.Find ("Head");
		body[(int)parts.TORSO] = GameObject.Find ("Torso");
		body[(int)parts.LEFTARM] = GameObject.Find ("LeftArm");
		body[(int)parts.RIGHTARM] = GameObject.Find ("RightArm");
		
		myAudio = GetComponents<AudioSource> ();

		debris = GameObject.Find ("Debris");

		init();
	}

	void Update(){

		//Move Camera
		float camH = Input.GetAxis("Mouse X");
		float camV = Input.GetAxis("Mouse Y");

		rotVector.x = -camV *sensY;
		rotVector.y = camH *sensX;
		
		rotVector.x = Mathf.Clamp(rotVector.x, -60f, 60f);
		rotVector.x = Mathf.Clamp(rotVector.x, -60f, 60f);
		
		transform.Rotate (rotVector);

		//Move Player
		float inputH = Input.GetAxis ("Horizontal");
		float inputV = Input.GetAxis ("Vertical");

		float inputY = 0;

		if (Input.GetKey (KeyCode.Space)) {
			inputY += 1f;
		}
		if(Input.GetKey (KeyCode.LeftShift)) {
			inputY -= 1f;
		}

		float axisRot = 0;

		if (Input.GetKey (KeyCode.Q)) {
			axisRot = maxRot;
		} else if(Input.GetKey (KeyCode.E)) {
			axisRot = -maxRot;
		}

		transform.RotateAround(transform.position,transform.TransformDirection(Vector3.forward), axisRot);

		inputV *= jetSpeed;
		inputH *= jetSpeed;
		inputY *= jetSpeed;

		Vector3 addVector;

		if (!stahp) {
			addVector = inputV * transform.TransformDirection (Vector3.forward) 
							  + inputH * transform.TransformDirection (Vector3.right)
							  + inputY * transform.TransformDirection (Vector3.up);
		}else{
			addVector = moveVector.normalized * -jetSpeed;
			if(moveVector.magnitude <.1){
				addVector = -moveVector;
				stahp = false;
			}
		}
		if (addVector.magnitude > .01) {
			jet.enableEmission = true;
			jet.transform.LookAt(jet.transform.position - addVector);
			if(!myAudio[2].isPlaying){
				myAudio[2].Play();
			}
		}else{
			jet.enableEmission = false;
			myAudio[2].Stop();
		}

		moveVector += addVector;

		if(moveVector.magnitude > maxSpeed){
			moveVector = moveVector.normalized * maxSpeed;
		}

		controller.Move (moveVector * Time.deltaTime);
		
		transform.position = new Vector3(Mathf.Clamp(transform.position.x,-50,50),Mathf.Clamp(transform.position.y,-50,50),Mathf.Clamp(transform.position.z,-50,50));

		foreach (Transform child in debris.transform)
		{
			child.position = new Vector3(Mathf.Clamp(child.position.x,-50,50),Mathf.Clamp(child.position.y,-50,50),Mathf.Clamp(child.position.z,-50,50));
		}

		if (magCan.activeInHierarchy) {
			if (Input.GetKeyDown(KeyCode.Mouse0)){
				shootMag();
			}
			attract.enableEmission = false;
			myAudio[1].Stop();
		}else{
			if (Input.GetKey (KeyCode.Mouse0)) {
				attract.enableEmission = true;
				if(!myAudio[1].isPlaying){
					myAudio[1].Play();
				}
				useMagnet();
			}else{
				attract.enableEmission = false;
				myAudio[1].Stop();
			}
		}

		if(Input.GetKeyDown(KeyCode.Escape)){
			Screen.lockCursor = !Screen.lockCursor;
		}

		if (Input.GetKeyDown (KeyCode.Tab)) {
			stahp = true;
		}

		if (Input.GetKeyDown (KeyCode.BackQuote)) {
			if(firstPerson){
				firstPerson = false;
				myCam.transform.localPosition = thirdPos;
				body[ (int) parts.HEAD].SetActive(true);
			}else{
				firstPerson = true;
				myCam.transform.localPosition = firstPos;
				body[ (int) parts.HEAD].SetActive(false);
			}
		}

		if (Input.GetKeyDown (KeyCode.R)) {
			init ();
		}

		GUIDebug.text = "Position:" + transform.position + "\n" +
						"Velocity:" + moveVector + "\n" +
						"Acceleration:" + addVector;
	}

	void init(){

		firstPerson = true;
		myCam.transform.localPosition = firstPos;
		body [(int)parts.HEAD].SetActive(false);
		
		attract.enableEmission = false;
		jet.enableEmission = false;
		magCan.SetActive(false);
		
		moveVector = Vector3.zero;
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.Euler(Vector3.zero);

		stahp = false;

		foreach (Transform child in debris.transform)
		{
			Destroy(child.gameObject);
		}

		/*
		makeCan (new Vector3(-5f, 0f, 10f));
		makeCan (new Vector3( 0f, 0f, 10f));
		makeCan (new Vector3( 5f, 0f, 10f));
		makeCan (new Vector3(-2.5f, 2.5f, 10f));
		makeCan (new Vector3( 2.5f, 2.5f, 10f));
		makeCan (new Vector3( 0f, 5f, 10f));
		*/

		for (int i = 0; i < 100; i++) {
			makeCan(new Vector3(Random.Range(-40,40),Random.Range(-40,40),Random.Range(0,40)));
		}

		foreach (Transform child in debris.transform)
		{
			child.gameObject.rigidbody.angularVelocity = new Vector3 (Random.Range(-1f,1f),Random.Range(-1f,1f),Random.Range(-1f,1f));
		}
	}

	GameObject makeCan(Vector3 pos){
		return makeCan (pos,Quaternion.Euler(Vector3.zero));
	}

	GameObject makeCan(Vector3 pos, Quaternion rot){
		GameObject ret = Instantiate (canPrefab, pos, rot) as GameObject;
		ret.transform.parent = debris.transform;
		return ret;
	}

	void useMagnet(){
		Collider[] hitColliders = Physics.OverlapSphere(magCan.transform.position, magRange);

		foreach(Collider coll in hitColliders){
			if(coll.gameObject.tag == "Metal" && !((Time.time - lastShot) < shotCooldown)){
				Vector3 dist = coll.transform.position - magCan.transform.position;
				if(dist.magnitude < 1f){
					Destroy(coll.gameObject);
					magCan.SetActive(true);
					return;
				}
				float force = magPow/(dist.magnitude * dist.magnitude);

				coll.rigidbody.AddForce(dist.normalized * -force);
			}
		}
	}

	void shootMag(){
		magCan.SetActive (false);
		GameObject newCan = makeCan (magCan.transform.position, magCan.transform.rotation);
		newCan.rigidbody.velocity = moveVector;
		newCan.rigidbody.AddForce(transform.TransformDirection(Vector3.forward) * shootPow);
		newCan.rigidbody.angularVelocity = new Vector3 (Random.Range(-1f,1f),Random.Range(-1f,1f),Random.Range(-1f,1f));
		lastShot = Time.time;
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit) {
		
		Rigidbody body = hit.collider.attachedRigidbody;

		if (body == null || body.isKinematic)
			return;

		if (hit.moveDirection.y < -0.3F)
			return;

		Vector3 pushDir = hit.moveDirection;
		body.velocity += pushDir * moveVector.magnitude;
		moveVector -= moveVector / 2;
	}
}
