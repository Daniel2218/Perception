using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class controller_camera : MonoBehaviour {
	private class Block {
		public static readonly int PATH = 0;
		public static readonly int IGNORE = 1;

		public Face[] faces;
		public GameObject block;
		public int state;

		public Block(GameObject prefab, Vector3 position, Vector3 rotation, int state) {
			block = Instantiate (prefab);
			block.transform.position = position;
			block.transform.eulerAngles = rotation;
			this.state = state;

			faces = new Face[6];
			// top
			faces[0] = new Face(
				new Vector3(0.5f, 0.5f, -0.5f),
				new Vector3(-0.5f, 0.5f, -0.5f),
				new Vector3(0.5f, 0.5f, 0.5f),
				new Vector3(-0.5f, 0.5f, 0.5f)
			);
			// bot
			faces[1] = new Face(
				new Vector3(0.5f, -0.5f, -0.5f),
				new Vector3(-0.5f, -0.5f, -0.5f),
				new Vector3(0.5f, -0.5f, 0.5f),
				new Vector3(-0.5f, -0.5f, 0.5f)
			);
			// left
			faces[2] = new Face(
				new Vector3(0.5f, 0.5f, -0.5f),
				new Vector3(0.5f, 0.5f, 0.5f),
				new Vector3(0.5f, -0.5f, -0.5f),
				new Vector3(0.5f, -0.5f, 0.5f)
			);
			// right
			faces[3] = new Face(
				new Vector3(-0.5f, 0.5f, 0.5f),
				new Vector3(-0.5f, 0.5f, -0.5f),
				new Vector3(-0.5f, -0.5f, 0.5f),
				new Vector3(-0.5f, -0.5f, -0.5f)
			);
			// front
			faces[4] = new Face(
				new Vector3(0.5f, 0.5f, 0.5f),
				new Vector3(-0.5f, 0.5f, 0.5f),
				new Vector3(0.5f, -0.5f, 0.5f),
				new Vector3(-0.5f, -0.5f, 0.5f)
			);
			// back
			faces[5] = new Face(
				new Vector3(-0.5f, 0.5f, -0.5f),
				new Vector3(0.5f, 0.5f, -0.5f),
				new Vector3(-0.5f, -0.5f, -0.5f),
				new Vector3(0.5f, -0.5f, -0.5f)
			);
		}
	}

	private class Face {
		public Vector3 v1;
		public Vector3 v2;
		public Vector3 v3;
		public Vector3 v4;

		public Face(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;
			this.v4 = v4;
		}
	}

	private Camera camera;
	private GameObject light;
	private Light light_comp;

	public Vector3 pivot;
	public float pivot_distance;
	public Color the_color;
	public Material mat_hide;
	public Material mat_show;

	public GameObject player;
	public GameObject block_normal;
	public GameObject block_exit;
	public GameObject block_exit_hide;

	private List<Block> blocks = new List<Block>();

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
		light_comp.shadows = LightShadows.Soft;
		light_comp.shadowBias = 1;
		light_comp.shadowNormalBias = 1;

		// build test level
		blocks.Add (new Block (block_exit, new Vector3 (0, 0, 0), new Vector3 (0, 0, 0), Block.PATH));
		blocks.Add (new Block (block_exit_hide, new Vector3 (0, 0, 0.5f), new Vector3 (0, 0, 0), Block.IGNORE));

		blocks.Add (new Block (block_normal, new Vector3 (1, 0, 0), new Vector3 (0, 0, 0), Block.PATH));
		blocks.Add (new Block (block_normal, new Vector3 (2, 0, 0), new Vector3 (0, 0, 0), Block.PATH));

		blocks.Add (new Block (block_normal, new Vector3 (1, -2, 3), new Vector3 (0, 0, 0), Block.PATH));
		blocks.Add (new Block (block_normal, new Vector3 (2, -2, 3), new Vector3 (0, 0, 0), Block.PATH));
		blocks.Add (new Block (block_normal, new Vector3 (3, -2, 3), new Vector3 (0, 0, 0), Block.PATH));

		blocks.Add (new Block (block_normal, new Vector3 (-3, -2, 2), new Vector3 (0, 0, 0), Block.PATH));
		blocks.Add (new Block (block_normal, new Vector3 (3, -2, 1), new Vector3 (0, 0, 0), Block.PATH));
		blocks.Add (new Block (block_normal, new Vector3 (3, -2, 0), new Vector3 (0, 0, 0), Block.PATH));
		blocks.Add (new Block (block_normal, new Vector3 (3, -2, -1), new Vector3 (0, 0, 0), Block.PATH));
		blocks.Add (new Block (block_normal, new Vector3 (3, -2, -2), new Vector3 (0, 0, 0), Block.PATH));

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

		Vector3 position = new Vector3 (pivot.x + x, pivot.y + y, pivot.z + z);
		Vector3 rotation = new Vector3 (temp_xy, temp_xz, 0);

		camera.transform.position = position;
		camera.transform.eulerAngles = rotation;

		light.transform.position = position;
		light.transform.eulerAngles = rotation;
		light.transform.RotateAround (pivot, new Vector3 (temp_xy, temp_xz, 0), -90);

		mat_hide.color = the_color;
		camera.backgroundColor = the_color;

		/*for (int i = 0; i < blocks.Count; i++) {
			Block block = blocks [i];

			for (int f = 0; f < 6; f++) {
				Vector3 v1 = camera.WorldToScreenPoint (block.block.transform.position + block.faces [f].v1);
				Vector3 v2 = camera.WorldToScreenPoint (block.block.transform.position + block.faces [f].v2);
				Vector3 v3 = camera.WorldToScreenPoint (block.block.transform.position + block.faces [f].v3);
				Vector3 v4 = camera.WorldToScreenPoint (block.block.transform.position + block.faces [f].v4);
				GUI.Label(new Rect(v1.x, Screen.height - v1.y, 20, 20), "v1");
				GUI.Label(new Rect(v2.x, Screen.height - v2.y, 20, 20), "v2");
				GUI.Label(new Rect(v3.x, Screen.height - v3.y, 20, 20), "v3");
				GUI.Label(new Rect(v4.x, Screen.height - v4.y, 20, 20), "v4");
			}
		}*/
	}

	void OnGUI() {
		int precision = 5;
		GUI.Label(new Rect(10, 10, 100, 20), "A_XY: " + (Mathf.Round(angle_xy/precision)*precision % 360));
		GUI.Label(new Rect(10, 30, 100, 20), "A_XZ: " + (Mathf.Round(angle_xz/precision)*precision % 360));
	}
}