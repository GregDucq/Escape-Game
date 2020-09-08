using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        speed = 30f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector3(0.0f, 0.0f,speed) * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("wall"))
        {
            Destroy(gameObject);
            Destroy(this);
        }

        else if (other.gameObject.CompareTag("enemy"))
        {
            other.GetComponent<EnemyLogic>().stun();

            Destroy(gameObject);
            Destroy(this);
        }
    }
}
