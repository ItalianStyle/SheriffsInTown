using UnityEngine;

public class Shooting : MonoBehaviour
{
    public GameObject bullet;
    public PlayerMovement player;
    private float timer;
    public float maxTimer;
    public float shootSpeed;
    public Transform gunPosition;
    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        timer = maxTimer;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer <= 0f && Input.GetKey(KeyCode.Mouse0))
        {
            Shoot();
            timer = maxTimer;
        }
        else timer -= Time.deltaTime;
    }

    void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else targetPoint = ray.GetPoint(75);

        Vector3 targetDir = (targetPoint - gunPosition.position).normalized;

        GameObject temp = Instantiate(bullet, gunPosition.position, Quaternion.identity);
        Destroy(temp, 5f);
        temp.transform.forward = targetDir;
        temp.GetComponent<Rigidbody>().AddForce(targetDir * shootSpeed, ForceMode.Impulse);

        player.FaceCamera();
    }
}
