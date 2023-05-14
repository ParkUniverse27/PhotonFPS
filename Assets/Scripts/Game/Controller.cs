using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Controller : MonoBehaviourPunCallbacks, IPunObservable
{
    public PhotonView PV;
    public Animator Anim;

    public float Health;

    public float Speed;
    public float GunRange;
    public float Sensivity;
    public float JumpForce;
    public float RespawnTimer = 7;
    public int Ammo;

    public Camera FPPCamera;
    public GameObject PlayerParent;
    public Transform BulletPoint;
    public Transform GroundChecker;
    public Rigidbody Rigid;
    public GameObject TPPModel;

    public Vector3 BoxSize;

    Vector3 pos;


    public bool IsSpectatorMode;

    public Controller SpectatorPlayer;

    float xRotate;

    float timer;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        if (Camera.main != null)
            //Camera.main.gameObject.SetActive(false);

        TPPModel.SetActive(true);
        FPPCamera.gameObject.SetActive(false);
        Camera.main.gameObject.SetActive(true);
        Camera.main.transform.SetParent(this.transform);
        Camera.main.transform.localPosition = new Vector3(0.46f, 1.72f, -2.3f);
        Camera.main.transform.localEulerAngles = Vector3.zero;
        UIManager.Instance.SwipeFade(null);

        // if (PV.IsMine)
        // {
        //     SetSurface(false);
        //     FPPCamera.gameObject.SetActive(true);
        //     UIManager.Instance.SwipeFade(null);
        // }
        // else
        // {
        //     SetSurface(true);
        //     FPPCamera.gameObject.SetActive(false);
        // }

        UIManager.Instance.UpdateHP(Health);
    }

    private void Update()
    {
        if (!PV.IsMine)
        {
            if((transform.position-pos).sqrMagnitude > 100) transform.position = pos;
            else transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime*10);
        }
        else
        {
            if (!IsSpectatorMode)
            {
                float yRotateSize = Input.GetAxis("Mouse X") * Sensivity;
                float yRotate = yRotateSize;

                float xRotateSize = -Input.GetAxis("Mouse Y") * Sensivity;
                xRotate = Mathf.Clamp(xRotate + xRotateSize, -45, 80);

                FPPCamera.transform.localEulerAngles = new Vector3(xRotate, 0, 0);
                transform.eulerAngles += new Vector3(0, yRotate, 0);

                float hor = Input.GetAxis("Horizontal");
                float ver = Input.GetAxis("Vertical");

                if(Mathf.Approximately(hor, 0) && Mathf.Approximately(ver, 0))
                {
                    Anim.SetBool("Moving", false);
                }
                else
                {
                    Anim.SetBool("Moving", true);
                    Anim.SetFloat("xDir", hor, 0.1f, Time.deltaTime);
                    Anim.SetFloat("yDir", ver, 0.1f, Time.deltaTime);
                }


                var inputDir = new Vector3(hor, 0 ,ver).normalized;
                //var dir = new Vector3(hor * n.x, n.y, ver * n.z) * Time.deltaTime * Speed;
                Rigid.MovePosition(Rigid.position + inputDir*Speed*Time.deltaTime);
                //transform.Translate(new Vector3(hor, 0, ver) * Time.deltaTime * Speed, Space.Self);

                var isGround = Physics.CheckBox(GroundChecker.position, BoxSize, Quaternion.identity, 7);
                if(isGround)
                {
                    Anim.SetBool("Jumping", false);
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        Rigid.AddForce(new Vector3(0, JumpForce, 0), ForceMode.Impulse);
                    }
                }
                else
                {
                    Anim.SetBool("Jumping", true);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }
            }
            else
            {
                if (SpectatorPlayer != null)
                {
                    //관전하던 플레이어가 죽으면
                    if (SpectatorPlayer.IsSpectatorMode)
                    {
                        SetSpectator(SpectatorPlayer.SpectatorPlayer);
                    }
                    UIManager.Instance.UpdateHP(SpectatorPlayer.Health);
                }

                timer += Time.deltaTime;
                UIManager.Instance.SetRespawnText(Mathf.RoundToInt(RespawnTimer - timer));
                //부활처리
                if (timer >= RespawnTimer)
                {
                    Respawn();
                }
            }
        }


    }

    public void SetSurface(bool show)
    {
        foreach(Transform tpp in TPPModel.transform)
        {
            tpp.gameObject.SetActive(show);
        }
    }

    void Respawn()
    {
        if (SpectatorPlayer != null)
        {
            SpectatorPlayer.FPPCamera.gameObject.SetActive(false);
            SpectatorPlayer.SetSurface(true);
        }
        UIManager.Instance.DoFade(1, delegate
        {
            UIManager.Instance.RespawnCanvas.SetActive(false);
            NetworkManager.Instance.CreateCharacter();
        });
    }

    void Shoot()
    {
        var screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = FPPCamera.ScreenPointToRay(screenCenter);

        Vector3 dir = ray.direction;
        if (Physics.Raycast(ray, out RaycastHit hit, GunRange))
        {
            if (hit.transform != null)
            {
                var target = hit.transform.gameObject;
                if (target.CompareTag("Player"))
                {
                    int viewID = target.GetComponent<PhotonView>().ViewID;
                    PV.RPC("HitRPC", RpcTarget.AllBuffered, viewID, PV.ViewID, 10f);
                }
            }
        }
    }


    //맞은 사람, 공격한 사람 뷰 아이디, 대미지
    [PunRPC]
    public void HitRPC(int viewID, int attackerViewId, float damage)
    {
        var target = PhotonView.Find(viewID)?.gameObject;
        // if(target == null)
        //     return;

        var player = target.GetComponent<Controller>();
        player.Health -= damage;

        bool isMine = player.PV.IsMine;


        if (isMine)
        {
            UIManager.Instance.UpdateHP(player.Health);
        }


        //게임시작전 사망처리X
        if (!NetworkManager.Instance.GameStarted)
            return;

        if (player.Health <= 0)
        {
            var attacker = PhotonView.Find(attackerViewId)?.gameObject.GetComponent<Controller>();
            if (isMine)
            {
                if (attacker != null)
                {
                    //로컬에 적용
                    player.SetSpectator(attacker);
                }
                else
                {
                    Camera.main.gameObject.SetActive(true);
                }

                UIManager.Instance.SwipeFade(delegate
                {
                    UIManager.Instance.RespawnCanvas.SetActive(true);
                });
            }

            //모튼 플레이어에게 적용
            player.SpectatorPlayer = attacker;
            player.IsSpectatorMode = true;

            player.PlayerParent.SetActive(false);

            var to = player.PV.Owner.NickName;
            var from = attacker.PV.Owner.NickName;
            string content = $"<b>{from}</b>이(가) <b>{to}</b>을(를) 처치했습니다!";
            UIManager.Instance.ShowKillLog(content);
        }
    }


    void SetSpectator(Controller target)
    {
        IsSpectatorMode = true;
        SpectatorPlayer = target;
        target.FPPCamera.gameObject.SetActive(true);
        target.SetSurface(false);
        UIManager.Instance.SpectatorText.text = $"관전: <b>{SpectatorPlayer.PV.Owner.NickName}</b>";
        UIManager.Instance.UpdateHP(target.Health);
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, BoxSize);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            pos = (Vector3)stream.ReceiveNext();
        }
    }

}
