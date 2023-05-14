using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Bullet : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    public float Speed;
    public float Damage;
    public float DestroyTime;

    public Vector3 Dir;


    public float timer;
    Vector3 lastPos;

    private void Start() 
    {
        if(PV.IsMine)
        {
            PV.RPC("SetDir", RpcTarget.OthersBuffered, Dir);
        }
    }

    [PunRPC]
    void SetDir(Vector3 dir)
    {
        Dir = dir;
    }

    private void Update() 
    {
        GetComponent<Rigidbody>().MovePosition(lastPos + Dir * Speed * Time.deltaTime);
        lastPos += Dir*Speed*Time.deltaTime;
        //transform.Translate(Dir * Speed * Time.deltaTime, Space.Self);
        if(PV.IsMine)
        {
           


            // timer += Time.deltaTime;
            // if(DestroyTime <= timer)
            // {
            //     PhotonNetwork.Destroy(this.gameObject);
            // }
        }
    }

    void OnDrawGizmos()  
    {  
        //Gizmos.matrix = transform.localToWorldMatrix;  
        Gizmos.color = Color.yellow;  
        Gizmos.DrawSphere(transform.position, 1);  
    }  

    private void OnTriggerEnter(Collider other) 
    {
        if(other.CompareTag("Player"))
            return;

            
        Destroy(gameObject);
    }

    // private void OnTriggerStay(Collider other) 
    // {
    //     PhotonNetwork.Destroy(gameObject);
    // }
}
