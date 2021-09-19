using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bigBossGnot : MonoBehaviour
{
    public bool isAwake = false;
    public float Hitpoints = 281f;
    public RawImage hitPointBar;
    public hammerController mainHC;
    public gameController mainGC;

    public AudioClip clipRoar;
    public AudioClip[] footSteps;

    public Transform posThrow1;
    public Transform posThrow2;
    public Transform posFinal;
    public Transform posProjectileSpawn;
    public Transform throwTarget1;
    public GameObject throwTarget1GO;
    public Transform throwTarget2;
    public GameObject throwTarget2GO;
    public GameObject heldProjectile;
    public GameObject flyingProjectile;
    public GameObject prefabProjectile;
    public bool reeling = false;

    private AudioSource myAS;
    private Animator myAnim;
    public float distance;
    public int bossStage = 0; // 0 = wake up, 1 = advance to throw1, 2= throw1, 3 = advance to throw2, 4= throw2, 5 = dance until spawners destroyed, 6 = advance fully, 7 = attack player
    public bool spawnersDestroyed = false; // when they are destroyed, he roars and runs to kill player

    public void playRoar()
    {
        myAS.clip = clipRoar;
        myAS.pitch = 0.5f;
        myAS.Play();
    }
    public void playTakeStep()
    {
        myAS.clip = footSteps[Random.Range(0, footSteps.Length)];
        myAS.pitch = 1f;
        myAS.Play();
    }
    public void eventDoneRoar()
    {
        bossStage = 1;
    }
    public void eventReeling()
    {
        reeling = true;
    }
    public void eventReelingRecover()
    {
        reeling = false;
    }
    public void eventPickupProjectile()
    {
        heldProjectile = Instantiate(prefabProjectile, Vector3.zero, Quaternion.identity) as GameObject;
        heldProjectile.transform.SetParent(posProjectileSpawn);
        heldProjectile.transform.localScale=new Vector3(0.1f, 0.1f, 0.1f);
        heldProjectile.transform.localPosition = Vector3.zero;
        heldProjectile.transform.localRotation = Quaternion.identity;
        myAnim.SetBool("throw", false);
    }
    public void eventReleaseProjectile()
    {
        heldProjectile.transform.SetParent(null);
        flyingProjectile = heldProjectile;
        heldProjectile = null;
    }
    public void Reset()
    {
        isAwake = false;
        myAnim.SetTrigger("reset");
        transform.position = new Vector3(-4.01f, 0.48f, 55.92f);
        transform.LookAt(throwTarget1.position);
    }
    public void wakeUp()
    {
        isAwake = true;
        myAnim.SetBool("wakeup", true);
        myAnim.SetTrigger("wakeup");
        bossStage = 0;
    }
    public void takeDamage(float damage)
    {
        Hitpoints -= damage;
        hitPointBar.GetComponent<RectTransform>().sizeDelta = new Vector2(Hitpoints, 65);
        if (damage > 40f)
        {
            myAnim.SetTrigger("takedamage");
            reeling = true;
            myAnim.SetBool("walking", false);
            myAnim.SetBool("running", false);
            myAnim.SetBool("throw", false);
            myAnim.SetBool("dance", false);
            if (heldProjectile) Destroy(heldProjectile);
        }
        if ( damage > 10f)
        {
            mainGC.addPoints((int)(damage * 0.1f));
        }
        myAnim.SetFloat("hitpoints", Hitpoints);
    }

    // Start is called before the first frame update
    void Start()
    {
        myAS = GetComponent<AudioSource>();
        myAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if ( flyingProjectile )
        {
            switch (bossStage)
            {
                case 2:
                    distance = Vector3.Distance(flyingProjectile.transform.position, throwTarget1.position);
                    flyingProjectile.transform.position = Vector3.MoveTowards(flyingProjectile.transform.position, throwTarget1.position + (Vector3.up * distance * 0.45f), Time.deltaTime * 5f);
                    if (distance < 1f)
                    {
                        throwTarget1GO.SetActive(false);
                        Destroy(flyingProjectile);
                        bossStage = 3;
                    }
                    break;
                case 4:
                    distance = Vector3.Distance(flyingProjectile.transform.position, throwTarget2.position);
                    flyingProjectile.transform.position = Vector3.MoveTowards(flyingProjectile.transform.position, throwTarget2.position + (Vector3.up * distance * 0.45f), Time.deltaTime * 5f);
                    if (distance < 1f)
                    {
                        throwTarget2GO.SetActive(false);
                        Destroy(flyingProjectile);
                        bossStage = 5;
                    }
                    break;
                default:
                    distance = Vector3.Distance(flyingProjectile.transform.position, mainGC.posGameOn.position);
                    flyingProjectile.transform.position = Vector3.MoveTowards(flyingProjectile.transform.position, mainGC.posGameOn.position + (Vector3.up * distance * 0.45f), Time.deltaTime * 5f);
                    if (distance < 0.5f)
                    {
                        // something bad
                        Destroy(flyingProjectile);
                    }
                    break;
            }
        }
        //distance = Vector3.Distance(transform.position, currentTarget.position);
        //myAnim.SetFloat("moveSpeed", distance);
        if ( !reeling && isAwake )
        {
            switch (bossStage)
            {
                case 1: // advance to throw 1
                    myAnim.SetBool("walking", true);
                    transform.position = Vector3.MoveTowards(transform.position, posThrow1.position, Time.deltaTime * 1.5f);
                    transform.LookAt(posThrow1.position);
                    distance = Vector3.Distance(transform.position, posThrow1.position);
                    if (distance < 1f)
                    {
                        myAnim.SetBool("walking", false);
                        bossStage = 2;
                    }
                    break;
                case 2: // throw1
                    transform.LookAt(throwTarget1.position);
                    if ( !flyingProjectile )
                    {
                        myAnim.SetBool("throw", true);
                    } else {
                        myAnim.SetBool("throw", false);  
                    }
                    break;
                case 3: // run to throw 2
                    myAnim.SetBool("walking", true);
                    myAnim.SetBool("running", true);
                    transform.position = Vector3.MoveTowards(transform.position, posThrow2.position, Time.deltaTime * 3f);
                    transform.LookAt(posThrow2.position);
                    distance = Vector3.Distance(transform.position, posThrow2.position);
                    if (distance < 1f)
                    {
                        myAnim.SetBool("walking", false);
                        myAnim.SetBool("running", false);
                        bossStage = 4;
                    }
                    break;
                case 4: // throw 2
                    transform.LookAt(throwTarget2.position);
                    if (!flyingProjectile)
                    {
                        myAnim.SetBool("throw", true);
                    } else {
                        myAnim.SetBool("throw", false);
                    }
                    break;
                case 5: // dance until spawners destroyed
                    myAnim.SetBool("dance", true);
                    transform.LookAt(posFinal.position);
                    if (spawnersDestroyed)
                    {
                        //myAnim.SetTrigger("roar");
                        myAnim.SetBool("dance", false);
                        bossStage = 6;
                    }
                    break;
                case 6: // walk to final
                    myAnim.SetBool("walking", true);
                    transform.position = Vector3.MoveTowards(transform.position, posFinal.position, Time.deltaTime * 1.5f);
                    transform.LookAt(posFinal.position);
                    distance = Vector3.Distance(transform.position, posFinal.position);
                    if (distance < 1f)
                    {
                        myAnim.SetBool("walking", false);
                        bossStage = 7;
                    }
                    break;
                case 7: // attack player
                    transform.LookAt(mainGC.posGameOn.position);
                    if (!flyingProjectile)
                    {
                        myAnim.SetBool("throw", true);
                    }
                    else
                    {
                        myAnim.SetBool("throw", false);
                    }
                    break;
                default: // wake up
                    break;
            }
        }
    }
}
