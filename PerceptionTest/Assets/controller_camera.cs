using UnityEngine;
using System.Collections;

public class controller_camera : MonoBehaviour {
	private Camera camera;
	private GameObject light;// = new GameObject("The Light");
	private Light light_comp;// = lightGameObject.AddComponent<Light>();
	public Vector3 pivot;
	public float pivot_distance;

	public Vector3 rotate;
	public float rotate_speed;
	public float rotate_max;
	public float angle_xz;
	public float angle_xy;

	// mouse
	public Vector3 mouse_pos;
	public bool left_down = false;

	public Vector3 last_click;

	// Use this for initialization
	void Start () {
		camera = Camera.main;
		camera.transform.position = new Vector3 (10, 10, -10);

		light = new GameObject("The Light");
		light_comp = light.AddComponent<Light>();
		light_comp.color = Color.white;
		light_comp.type = LightType.Directional;
		light_comp.intensity = 0.5f;
	}

	// Update is called once per frame
	void Update () {
		mouse_pos = Input.mousePosition;

		float x = 0, y = 0, z = 0;
		float temp_xy = angle_xy;
		float temp_xz = angle_xz;

		// start of click
		if (Input.GetMouseButtonDown (0) && left_down == false) {
			left_down = true;
			last_click = new Vector3 (mouse_pos.x, mouse_pos.y, mouse_pos.z);
		}
		// middle of click
		if (left_down) {
			Vector3 difference = new Vector3 (last_click.x - mouse_pos.x, last_click.y - mouse_pos.y, last_click.z - mouse_pos.z);

			temp_xy += difference.y * rotate_speed;
			temp_xz -= difference.x * rotate_speed;

			if (Input.GetMouseButton (0) == false) {
				angle_xy = temp_xy;
				angle_xz = temp_xz;

				left_down = false;		
			}
		}

		camera.orthographicSize = Mathf.Max(camera.orthographicSize - Input.GetAxis ("Mouse ScrollWheel"), 0.1f);

		y = Mathf.Sin(temp_xy*Mathf.Deg2Rad)*pivot_distance;
		z = Mathf.Sqrt(
			(Mathf.Pow(pivot_distance, 2) - Mathf.Pow(Mathf.Sin(temp_xy*Mathf.Deg2Rad), 2) * Mathf.Pow(pivot_distance, 2))
			/
			(1 + Mathf.Pow(Mathf.Tan(temp_xz*Mathf.Deg2Rad), 2))
		);
		x = Mathf.Sqrt(Mathf.Pow(pivot_distance, 2) - Mathf.Pow(z, 2) - Mathf.Pow(y, 2) + 0.001f);

		if (temp_xz % 360 >= 90 && temp_xz % 360 < 270)
			z = -z;

		if (temp_xz % 360 >= 180 && temp_xz % 360 < 360)
			x = -x;

		if (temp_xz <= 0)
			temp_xz = 360 + temp_xz;

		temp_xz = temp_xz + 180;

		camera.transform.position = new Vector3 (pivot.x + x, pivot.y + y, pivot.z + z);
		camera.transform.eulerAngles = new Vector3 (temp_xy, temp_xz, 0);

		light.transform.position = new Vector3 (pivot.x + x, pivot.y + y, pivot.z + z);
		light.transform.eulerAngles = new Vector3 (temp_xy, temp_xz, 0);
	}
}