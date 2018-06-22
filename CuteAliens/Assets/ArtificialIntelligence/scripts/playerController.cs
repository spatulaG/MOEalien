using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class playerController : MonoBehaviour {
    public Camera cam;
    public NavMeshAgent agent;
    public Animator anim;
    private Vector3 nextPos;
    private float speed = 0f;
	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
        nextPos = this.transform.position;
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
               
                agent.SetDestination(hit.point);
                

            }
            //speed = this.rigidbody.velocity;


            {
                CharacterController controller = this.GetComponent<CharacterController>();
                Vector3 horizontalVelocity = controller.velocity;
                horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
                float horizontalSpeed = horizontalVelocity.magnitude;
                float verticalSpeed = controller.velocity.y;
                float overallSpeed = controller.velocity.magnitude;

                //anim.SetFloat("VelocityX", Input.GetAxis("Vertical"));
                //anim.SetFloat("VelocityZ", Input.GetAxis("Horizontal"));
                Debug.Log(horizontalSpeed);
                anim.SetFloat("Speed", horizontalSpeed);
            }//speed


            anim.Play("RUN00_F", -1, 0f);



        }
        /*
        else {
            Debug.Log(Vector3.Distance(this.transform.position, nextPos));
            if (Vector3.Distance(this.transform.position, nextPos) <= 0.2f)
            {
                nextPos = this.transform.position + new Vector3(Random.Range(-1, 1) == -1 ? Random.Range(-0.3f, -0.5f) : Random.Range(0.3f, 0.5f), 0,
                    Random.Range(-1, 1) == -1 ? Random.Range(-0.3f, -0.5f) : Random.Range(0.3f, 0.5f));
            }
            else if (Vector3.Distance(this.transform.position, nextPos) > 1f)
            {
                nextPos = this.transform.position;
            }

            anim.Play("RUN00_F", -1, 0f);
                agent.SetDestination(nextPos);
            

        }
        */

	}
}
