using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControllerGame : MonoBehaviour 
{
	// all edit mode code
	private DrawGiz drawGiz;
	public List<Block> block_list;
	public Block start_block;
	public bool EDIT_MODE;
	public bool MMENU_MODE;
	public void TOGGLE_EDIT_MODE()
	{
		EDIT_MODE = (EDIT_MODE == false);
		player.mesh.GetComponent<MeshRenderer>().enabled = player.mesh.GetComponent<MeshRenderer>().enabled == false;
		drawGiz.cursor.GetComponent<MeshRenderer>().enabled = player.mesh.GetComponent<MeshRenderer>().enabled == false;

		foreach (Path p in Path.paths_list)
			if (p.anchor_line_connect)
				p.anchor_line_connect.GetComponent<MeshRenderer>().enabled = 
					drawGiz.cursor.GetComponent<MeshRenderer>().enabled;
	}

	// unity public prefabs
	public GameObject block_normal;
	public GameObject block_twist;
	public GameObject block_edge;
	public GameObject block_stairs;
	public GameObject block_stairs_hide;
	public GameObject player_prefab;
	public GameObject anchor_line;
	public GameObject anchor_point;

	// camera
	private Camera camera;
	private GameObject light;
	public Material mat_hide;
	public Material mat_show;
	public Material mat_fade;
	public Material mat_anchor;
	public Material mat_anchor_down;
	public Vector3 pivot;
	public Vector3 rotate;
	public Color the_color;
	public Color progress_color;
	public float precision;
	public float pivot_distance;
	public float rotate_speed;
	public float angle_xz;
	public float angle_xy;

	// mouse
	private Vector3 mouse_pos;
	private Vector3 last_click;
	private bool left_down = false;
	public GameObject anchor_down;
	public GameObject anchor_drag_line;
	public Block anchor_down_block;

	// player
	public static Player player;

	private class Stack<T>
	{
		private bool locked;
		private List<T> list;

		public Stack()
		{
			this.locked = false;
			this.list = new List<T>();
		}

		public int push(T data)
		{
			if (locked) 
			{
				return -1;
			}
			else
			{
				this.list.Add(data);
				return 1;
			}
		}

		public int pop(ref T data)
		{
			if (locked)
			{
				return -1;
			}
			else
			{
				if (list.Count > 0)
				{
					data = list[list.Count - 1];
					this.list.RemoveAt(list.Count - 1);
					return 1;
				}
				else
				{
					return -1;
				}
			}
		}

		public void toggle_locked()
		{
			this.locked = (this.locked == false);
		}

		public bool is_locked()
		{
			return this.locked;
		}
	}
		
	public abstract class Block
	{
		public static readonly int TOP = 0;
		public static readonly int BOTTOM = 1;
		public static readonly int LEFT = 2;
		public static readonly int RIGHT = 3;
		public static readonly int BACK = 4;
		public static readonly int FRONT = 5;

		// handle fading
		public static Material mat_fade;
		public static List<Block> fade_list = new List<Block>();

		public GameObject mesh;
		public Vector3 position;
		public Vector3 rotation;
		public List<Path> paths;

		private bool falls;
		private bool fallen;

		public GameObject[] anchors;

		public Block(GameObject mesh, GameObject anchor_line, GameObject anchor_point, Vector3 position, bool falls)
		{
			this.mesh = Instantiate(mesh);
			this.position = position;
			this.rotation = new Vector3(0, 0, 0);
			this.paths = new List<Path>();
			this.mesh.transform.position = this.position;
			this.mesh.transform.eulerAngles = this.rotation;
			this.falls = falls;
			this.fallen = false;

			if (falls)
			{
				Block.fade_list.Add(this);
			}

			this.anchors = new GameObject[6];
			this.anchors[TOP] = Instantiate(anchor_point);
			this.anchors[BOTTOM] = Instantiate(anchor_point);
			this.anchors[LEFT] = Instantiate(anchor_point);
			this.anchors[RIGHT] = Instantiate(anchor_point);
			this.anchors[BACK] = Instantiate(anchor_point);
			this.anchors[FRONT] = Instantiate(anchor_point);

			this.anchors[TOP].transform.position = this.face_to_pos(TOP).position;
			this.anchors[BOTTOM].transform.position = this.face_to_pos(BOTTOM).position;
			this.anchors[LEFT].transform.position = this.face_to_pos(LEFT).position;
			this.anchors[RIGHT].transform.position = this.face_to_pos(RIGHT).position;
			this.anchors[BACK].transform.position = this.face_to_pos(BACK).position;
			this.anchors[FRONT].transform.position = this.face_to_pos(FRONT).position;
		}

		public Block(GameObject mesh, GameObject anchor_line, GameObject anchor_point, Vector3 position, Vector3 rotation, bool falls)
		{
			this.mesh = Instantiate(mesh);
			this.position = position;
			this.rotation = rotation;
			this.paths = new List<Path>();
			this.mesh.transform.position = this.position;
			this.mesh.transform.eulerAngles = this.rotation;
			this.falls = falls;
			this.fallen = false;

			if (falls)
			{
				Block.fade_list.Add(this);
			}

			this.anchors = new GameObject[6];
			this.anchors[TOP] = Instantiate(anchor_point);
			this.anchors[BOTTOM] = Instantiate(anchor_point);
			this.anchors[LEFT] = Instantiate(anchor_point);
			this.anchors[RIGHT] = Instantiate(anchor_point);
			this.anchors[BACK] = Instantiate(anchor_point);
			this.anchors[FRONT] = Instantiate(anchor_point);

			this.anchors[TOP].transform.position = this.face_to_pos(TOP).position;
			this.anchors[BOTTOM].transform.position = this.face_to_pos(BOTTOM).position;
			this.anchors[LEFT].transform.position = this.face_to_pos(LEFT).position;
			this.anchors[RIGHT].transform.position = this.face_to_pos(RIGHT).position;
			this.anchors[BACK].transform.position = this.face_to_pos(BACK).position;
			this.anchors[FRONT].transform.position = this.face_to_pos(FRONT).position;
		}

		public abstract PlayerTransform face_to_pos(int face);
		public void fall()
		{
			if (this.falls && this.fallen == false)
			{
				this.mesh.GetComponent<MeshRenderer>().material = Block.mat_fade;
				this.fallen = true;

				Debug.Log("Fallen");
			}
		}

		public bool is_fallen()
		{
			return this.fallen;
		}

		public static void fade_list_update(float deltatime)
		{
			for (int i = 0; i < Block.fade_list.Count; i++)
			{
				Block current_block = Block.fade_list[i];
				if (current_block.is_fallen())
				{
					Color temp_color = current_block.mesh.GetComponent<MeshRenderer>().material.color;
					temp_color.a -= deltatime*2.0f;
					current_block.mesh.GetComponent<MeshRenderer>().material.color = temp_color;
				}
			}
		}

		public void remove_existance(ControllerGame context)
		{
			if (this.falls)
				Block.fade_list.Remove(this);
		
			if (context.block_list.IndexOf(this) >= 0)
				context.block_list.Remove(this);

			foreach (Path path in Path.paths_list)
			{
				if (path.b1.block == this || path.b2.block == this)
				{
					if (path.anchor_line_connect != null)
						Destroy(path.anchor_line_connect);

					Path.paths_list.Remove(path);
				}
			}

			foreach (GameObject anchor in this.anchors)
			{
				Destroy(anchor);
			}

			Destroy(this.mesh);
		}
	}

	public class BlockNormal : Block
	{	
		// TODO rotations
		override public PlayerTransform face_to_pos(int face)
		{
			PlayerTransform offset = new PlayerTransform
			(
				new Vector3(0.9f, 0.9f, 0.9f) + this.mesh.transform.position,
				new Vector3(0, 0, 0) + this.mesh.transform.eulerAngles
			);

			if (face == TOP)
			{
				offset.position = new Vector3 (0, 0.9f, 0);
			}
			else if (face == BOTTOM)
			{
				offset.position = new Vector3 (0, -0.9f, 0);
			}
			else if (face == LEFT)
			{
				offset.position = new Vector3 (0.9f, 0, 0);
			}
			else if (face == RIGHT)
			{
				offset.position = new Vector3 (-0.9f, 0, 0);
			}
			else if (face == FRONT)
			{
				offset.position = new Vector3 (0, 0, 0.9f);
			}
			else if (face == BACK)
			{
				offset.position = new Vector3 (0, 0, -0.9f);
			}

			offset.position = Quaternion.Euler(this.mesh.transform.eulerAngles) * offset.position;
			offset.position = offset.position + this.mesh.transform.position;

			return offset;
		}

		public BlockNormal(GameObject mesh, GameObject anchor_line, GameObject anchor_point, Vector3 position, bool falls) : base(mesh, anchor_line, anchor_point, position, falls) {}
		public BlockNormal(GameObject mesh, GameObject anchor_line, GameObject anchor_point, Vector3 position, Vector3 rotation, bool falls) : base(mesh, anchor_line, anchor_point, position, rotation, falls) {}
	}

	public class BlockTwist : Block
	{	
		// TODO rotations
		override public PlayerTransform face_to_pos(int face)
		{
			PlayerTransform offset = new PlayerTransform
			(
				new Vector3(0.9f, 0.9f, 0.9f) + this.mesh.transform.position,
				new Vector3(0, 0, 0)  + this.mesh.transform.eulerAngles
			);

			if (face == TOP)
			{
				offset.position = new Vector3 (-0.63f, 0.63f, 0);
			}
			else if (face == BOTTOM)
			{
				offset.position = new Vector3 (0.63f, -0.63f, 0);
			}
			else if (face == LEFT)
			{
				offset.position = new Vector3 (0.63f, 0.63f, 0);
			}
			else if (face == RIGHT)
			{
				offset.position = new Vector3 (-0.63f, -0.63f, 0);
			}
			else if (face == FRONT)
			{
				offset.position = new Vector3 (0, 0, 1.4f);
			}
			else if (face == BACK)
			{
				offset.position = new Vector3 (0, 0, -1.4f);
			}

			offset.position = Quaternion.Euler(this.mesh.transform.eulerAngles) * offset.position;
			offset.position = offset.position + this.mesh.transform.position;

			return offset;
		}

		public BlockTwist(GameObject mesh, GameObject anchor_line, GameObject anchor_point, Vector3 position, bool falls) : base(mesh, anchor_line, anchor_point, position, falls) {}
		public BlockTwist(GameObject mesh, GameObject anchor_line, GameObject anchor_point, Vector3 position, Vector3 rotation, bool falls) : base(mesh, anchor_line, anchor_point, position, rotation, falls) {}
	}

	public class BlockEdge : Block
	{	
		// TODO rotations
		override public PlayerTransform face_to_pos(int face)
		{
			PlayerTransform offset = new PlayerTransform
				(
					new Vector3(0.9f, 0.9f, 0.9f) + this.mesh.transform.position,
					new Vector3(0, 0, 0)  + this.mesh.transform.eulerAngles
				);

			if (face == TOP || face == FRONT)
			{
				offset.position = new Vector3 (-0.6f, 0.6f, 0);
			}
			else if (face == BOTTOM)
			{
				offset.position = new Vector3 (0, -0.9f, 0);
			}
			else if (face == LEFT)
			{
				offset.position = new Vector3 (0.9f, 0, 0);
			}
			else if (face == RIGHT)
			{
				offset.position = new Vector3 (-0.9f, 0, 0);
			}
			else if (face == BACK)
			{
				offset.position = new Vector3 (0, 0, -0.9f);
			}

			offset.position = Quaternion.Euler(this.mesh.transform.eulerAngles) * offset.position;
			offset.position = offset.position + this.mesh.transform.position;

			return offset;
		}

		public BlockEdge(GameObject mesh, GameObject anchor_line, GameObject anchor_point, Vector3 position, bool falls) : base(mesh, anchor_line, anchor_point, position, falls) {}
		public BlockEdge(GameObject mesh, GameObject anchor_line, GameObject anchor_point, Vector3 position, Vector3 rotation, bool falls) : base(mesh, anchor_line, anchor_point, position, rotation, falls) {}
	}

	public class BlockStairs : Block
	{	
		public GameObject hide_mesh;
		// TODO rotations
		override public PlayerTransform face_to_pos(int face)
		{
			PlayerTransform offset = new PlayerTransform
				(
					new Vector3(0.9f, 0.9f, 0.9f) + this.mesh.transform.position,
					new Vector3(0, 0, 0) + this.mesh.transform.eulerAngles
				);

			if (face == TOP)
			{
				offset.position = new Vector3 (0, 0.9f, 0);
			}
			else if (face == BOTTOM)
			{
				offset.position = new Vector3 (0, -0.9f, 0);
			}
			else if (face == LEFT)
			{
				offset.position = new Vector3 (0.9f, 0, 0);
			}
			else if (face == RIGHT)
			{
				offset.position = new Vector3 (-0.9f, 0, 0);
			}
			else if (face == FRONT)
			{
				offset.position = new Vector3 (0, 0, 0.9f);
			}
			else if (face == BACK)
			{
				offset.position = new Vector3 (0, 0, -0.9f);
			}

			offset.position = Quaternion.Euler(this.mesh.transform.eulerAngles) * offset.position;
			offset.position = offset.position + this.mesh.transform.position;

			return offset;
		}

		public BlockStairs(GameObject mesh, GameObject anchor_line, GameObject anchor_point, Vector3 position, GameObject hide_mesh) : base(mesh, anchor_line, anchor_point, position, false) 
		{
			this.hide_mesh = Instantiate(hide_mesh);
			this.hide_mesh.transform.position = this.mesh.transform.position - new Vector3(-3.5f, 0, 0);
			this.hide_mesh.GetComponent<MeshRenderer>().enabled = false;
		}
		public BlockStairs(GameObject mesh, GameObject anchor_line, GameObject anchor_point, Vector3 position, Vector3 rotation, GameObject hide_mesh) : base(mesh, anchor_line, anchor_point, position, rotation, false) 
		{
			this.hide_mesh = Instantiate(hide_mesh);
			this.hide_mesh.transform.position = this.mesh.transform.position - new Vector3(-3.5f, 0, 0);
			this.hide_mesh.GetComponent<MeshRenderer>().enabled = false;
		}
	}

	public class VectorBlock3
	{
		public Block block;
		public int face;

		public VectorBlock3(Block block, int face)
		{
			this.block = block;
			this.face = face;
		}
		public PlayerTransform to_pos()
		{
			return this.block.face_to_pos (this.face);
		}
	}

	public static void triggerWin()
	{
		Debug.Log("you win!");
	}

	public class Path
	{
		public VectorBlock3 b1, b2;
		public float xy, xz;
		private bool any_angle;
		private bool falls; // true if block falls AFTER walking on
		private bool fallen;

		/*
		 * 
		 * Move the code that makes the anchor_line_connect here dummy
		 * 
		 * 
		 */

		public GameObject anchor_line_connect;
		public static List<Path> paths_list = new List<Path>();

		public static void join_path(Path path)
		{
			path.b1.block.paths.Add(path);
			path.b2.block.paths.Add(path);
			Path.paths_list.Add(path);
		}

		public Path(VectorBlock3 b1, VectorBlock3 b2, float xy, float xz)
		{
			this.b1 = b1;
			this.b2 = b2;
			this.xy = xy;
			this.xz = xz;
			this.any_angle = false;
			this.falls = falls;
			this.fallen = false;
		}

		public Path(VectorBlock3 b1, VectorBlock3 b2)
		{
			this.b1 = b1;
			this.b2 = b2;
			this.xy = 0;
			this.xz = 0;
			this.any_angle = true;
			this.falls = falls;
			this.fallen = false;
		}

		public bool evaluate(float current_xy, float current_xz)
		{
			/*return this.any_angle || 
				(current_xy % 360 == this.xy % 360 && current_xz % 360 == this.xz % 360) ||
				(-current_xy % 360 == this.xy % 360 && (current_xz + 180) % 360 == this.xz % 360);*/
			
			/*Vector3 b1world = b1.to_pos().position;
			Vector3 b2world = b2.to_pos().position;
			Vector3 b1screen = Camera.allCameras[0].WorldToScreenPoint(b1world);
			Vector3 b2screen = Camera.allCameras[0].WorldToScreenPoint(b2world);
			b1screen = new Vector3(b1screen.x, b1screen.y, 0);
			b2screen = new Vector3(b2screen.x, b2screen.y, 0);

			if ((b1world - b2world).magnitude <= 1.01f)
				return true;

			float mag = (Camera.allCameras[0].ScreenToWorldPoint(b1screen) - Camera.allCameras[0].ScreenToWorldPoint(b2screen)).magnitude;

			return 
				(((b1world.y >= b2world.y && b1screen.y <= b2screen.y) || 
				  (b1world.y <= b2world.y && b1screen.y >= b2screen.y)) &&
					mag >= 0.8f &&
					mag <= 0.9f);
			return false;*/

			// doesn't work lol

			bool eval = true;

			if (eval && b1.block is BlockStairs || b2.block is BlockStairs)
				triggerWin();

			return eval;
		}
	}

	public class PlayerTransform
	{
		public Vector3 position;
		public Vector3 rotation;

		public PlayerTransform(Vector3 position, Vector3 rotation)
		{
			this.position = position;
			this.rotation = rotation;
		}

		public PlayerTransform()
		{
			this.position = new Vector3();
			this.rotation = new Vector3();
		}
	}

	public abstract class Action
	{
		protected bool started;
		protected bool finished;
		protected float duration;
		protected float accumulator;

		public Action(float duration)
		{
			this.duration = duration;
			this.accumulator = 0.0f;
			this.started = false;
		}
			
		//Time.deltaTime
		public void accumulate(float deltatime)
		{
			if (this.started) 
			{
				this.accumulator += deltatime;
				this.update();
			}
		}

		public void start()
		{
			this.started = true;
		}

		public bool is_started()
		{
			return this.started;
		}

		public bool is_finished()
		{	
			if (this.finished || this.duration <= this.accumulator)
			{
				if (this.finished == false)
				{
					this.finished = true;
					this.finish();
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		public abstract void update();
		public abstract void finish();
	}

	public class ActionMove : Action
	{
		private Block next_block;
		private int next_face;

		public ActionMove(Block next_block, int next_face) : base(0.2f) 
		{
			this.next_block = next_block;
			this.next_face = next_face;
		}

		override public void update()
		{
			float progress = this.accumulator/this.duration;
			Vector3 b1 = player.position.to_pos().position;
			Vector3 b2 = next_block.face_to_pos(next_face).position;

			player.position_offset = new Vector3
			(
				(b2.x - b1.x)*progress, 
				(b2.y - b1.y)*progress, 
				(b2.z - b1.z)*progress
			);
		}

		override public void finish()
		{
			player.position = new VectorBlock3(next_block, next_face);
			player.position_offset = new Vector3(0, 0, 0);
		}
	}
		
	public class Player
	{
		public static readonly int FORWARD = 1;
		public static readonly int BACKWARD = 2;
		public static readonly int LEFT = 3;
		public static readonly int RIGHT = 4;

		public VectorBlock3 position;
		public Vector3 position_offset;
		public Vector3 rotation_offset;
		public GameObject mesh;

		private Stack<Action> action_stack;
		private Action action_overflow;

		public Player(GameObject mesh, VectorBlock3 position)
		{
			this.action_stack = new Stack<Action>();
			this.action_overflow = null;
			this.position = position;
			this.mesh = Instantiate(mesh);
		}

		public void push_action(Action action)
		{
			if (this.action_stack.is_locked())
			{
				this.action_overflow = action;
			}
			else
			{
				this.action_stack.push(action);
				this.action_stack.toggle_locked();
			}
		}

		public Vector3 get_pos()
		{
			return this.mesh.transform.position;
		}

		public void update(float deltatime)
		{
			PlayerTransform transform = position.to_pos();
			mesh.transform.position = transform.position + this.position_offset;
			mesh.transform.eulerAngles = transform.rotation + this.rotation_offset;

			bool locked = this.action_stack.is_locked();

			if (locked) this.action_stack.toggle_locked();

			Action current_action = default(Action);
			int status = this.action_stack.pop(ref current_action);

			if (status > 0)
			{
				if (current_action.is_started() == false)
				{
					current_action.start();
				}

				if (current_action.is_finished())
				{
					update(deltatime);
				}
				else
				{
					current_action.accumulate(deltatime);
					// TODO check errors
					this.action_stack.push(current_action);
				}
			}
			else // it's empty
			{
				if (this.action_overflow != null)
				{
					this.action_stack.push(this.action_overflow);
					this.action_overflow = null;
					update(deltatime);
				}
			}

			if (locked) this.action_stack.toggle_locked();
		}

		public void evaluate_key(int key, Camera camera, float angle_xy, float angle_xz)
		{
			for (int i = 0; i < this.position.block.paths.Count; i++) 
			{
				Path path = this.position.block.paths[i];
				if (path.evaluate(angle_xy, angle_xz))
				{
					if (path.b1.block == this.position.block && path.b1.face == this.position.face && path.b2.block.is_fallen() == false)
					{
						Vector3 diff = camera.WorldToScreenPoint(path.b1.block.position) - camera.WorldToScreenPoint(path.b2.block.position);

						if ((key == FORWARD && Mathf.Abs(diff.y) >= Mathf.Abs(diff.x) && diff.y < 0) ||
							(key == LEFT && Mathf.Abs(diff.x) >= Mathf.Abs(diff.y) && diff.x >= 0) ||
							(key == RIGHT && Mathf.Abs(diff.x) > Mathf.Abs(diff.y) && diff.x < 0) ||
							(key == BACKWARD && Mathf.Abs(diff.y) >= Mathf.Abs(diff.x) && diff.y >= 0))
						{
							this.push_action(new ActionMove(path.b2.block, path.b2.face));
							path.b1.block.fall();
						}
					}
					else if (path.b2.block == this.position.block && path.b2.face == this.position.face && path.b1.block.is_fallen() == false)
					{
						Vector3 diff = camera.WorldToScreenPoint(path.b2.block.position) - camera.WorldToScreenPoint(path.b1.block.position);

						if ((key == FORWARD && Mathf.Abs(diff.y) >= Mathf.Abs(diff.x) && diff.y < 0) ||
							(key == LEFT && Mathf.Abs(diff.x) >= Mathf.Abs(diff.y) && diff.x >= 0) ||
							(key == RIGHT && Mathf.Abs(diff.x) > Mathf.Abs(diff.y) && diff.x < 0) ||
							(key == BACKWARD && Mathf.Abs(diff.y) >= Mathf.Abs(diff.x) && diff.y >= 0))
						{
							this.push_action(new ActionMove(path.b1.block, path.b1.face));
							path.b2.block.fall();
						}
					}
					else
					{
						Debug.Log("Malformatted Path");
					}
				}
			}
		}
	}

	void Start () 
	{
		camera = Camera.main;
		camera.transform.position = new Vector3 (10, 10, -10);

		drawGiz = camera.GetComponent<DrawGiz>();
		anchor_drag_line = Instantiate(anchor_line);
		anchor_drag_line.GetComponent<MeshRenderer>().enabled = false;

		light = new GameObject("The Light");

		Light light_component = light.AddComponent<Light>();
		light_component.color = Color.white;
		light_component.type = LightType.Directional;
		light_component.intensity = 0.5f;
		light_component.shadows = LightShadows.Soft;
		light_component.shadowBias = 1;
		light_component.shadowNormalBias = 1;

		Block.mat_fade = mat_fade;
		block_list = new List<Block>();

		if (MMENU_MODE)
		{
			Block block0 = new BlockNormal(block_normal, anchor_line, anchor_point, new Vector3(0, 0, 0), false);
			Block block1 = new BlockEdge(block_edge, anchor_line, anchor_point, new Vector3(1, 0, 0), new Vector3(0, 180, 0), false);
			Block block2 = new BlockTwist(block_twist, anchor_line, anchor_point, new Vector3(1, -2, 0), new Vector3(90, 180, 0), false);
			Block block3 = new BlockNormal(block_normal, anchor_line, anchor_point, new Vector3(2, -5, -1), false);
			Block block4 = new BlockEdge(block_edge, anchor_line, anchor_point, new Vector3(3, -5, -1), new Vector3(90, 0, 180), false);
			Block block5 = new BlockNormal(block_normal, anchor_line, anchor_point, new Vector3(3, -5, 0), false);

			block_list.Add(block0);
			block_list.Add(block1);
			block_list.Add(block2);
			block_list.Add(block3);
			block_list.Add(block4);
			block_list.Add(block5);

			player = new Player(player_prefab, new VectorBlock3(block0, Block.TOP));

			foreach (Block block_ref in block_list)
				foreach (GameObject anchor_ref in block_ref.anchors)
					Destroy(anchor_ref);
			/**/
		}
		else
		{
			start_block = new BlockNormal(block_normal, anchor_line, anchor_point, new Vector3(0, 0, 0), false);
			player = new Player(player_prefab, new VectorBlock3(start_block, Block.TOP));

			block_list.Add(start_block);

			if (EDIT_MODE)
				EDIT_MODE = false;

			TOGGLE_EDIT_MODE();
		}
	}

	void Update () 
	{
		BlockStairs block_stairs_temp = null;

		foreach (Block block in block_list)
		{
			if (block is BlockStairs)
			{
				block_stairs_temp = (BlockStairs) block;
				break;
			}
		}

		player.update(Time.deltaTime);
		mouse_pos = Input.mousePosition;

		// todo change to exit block
		Vector3 delta = player.get_pos() - (block_stairs_temp != null ? block_stairs_temp.mesh.transform.position : new Vector3(0, 0, 0));
		float r = the_color.r + (1 - the_color.r)*delta.magnitude*0.02f - 0.2f;
		float g = the_color.g + (1 - the_color.g)*delta.magnitude*0.02f - 0.2f;
		float b = the_color.b + (1 - the_color.b)*delta.magnitude*0.02f - 0.2f;
		r = r < 0 ? 0 : (r > 1 ? 1 : r);
		g = g < 0 ? 0 : (g > 1 ? 1 : g);
		b = b < 0 ? 0 : (b > 1 ? 1 : b);
		progress_color = new Color(r, g, b, the_color.a);

		mat_hide.color = progress_color;
		camera.backgroundColor = progress_color;

		float x = 0, y = 0, z = 0;
		float temp_xy = angle_xy;
		float temp_xz = angle_xz;

		if (!MMENU_MODE)
		{
			camera.orthographicSize = Mathf.Max (camera.orthographicSize - Input.GetAxis ("Mouse ScrollWheel"), 0.1f);

			if (EDIT_MODE)
			{
				foreach (Block block in block_list)
					foreach (GameObject anchor in block.anchors)
						anchor.GetComponent<MeshRenderer>().enabled = true;
			}
			else
			{
				foreach (Block block in block_list)
					foreach (GameObject anchor in block.anchors)
						anchor.GetComponent<MeshRenderer>().enabled = false;
			}

			// start of click
			if (Input.GetMouseButtonDown (0) && left_down == false)
			{
				left_down = true;
				last_click = new Vector3 (mouse_pos.x, mouse_pos.y, mouse_pos.z);
				anchor_down = null;
				anchor_down_block = null;

				if (EDIT_MODE)
				{
					foreach (Block block in block_list)
					{
						foreach (GameObject anchor in block.anchors)
						{
							if ((camera.WorldToScreenPoint(anchor.transform.position) - mouse_pos).magnitude < (40/Mathf.Sqrt(camera.orthographicSize)))
							{
								anchor_down = anchor;
								anchor_down_block = block;
								anchor.GetComponent<MeshRenderer>().material = mat_anchor_down;
								break;
							}
						}

						if (anchor_down != null)
							break;
					}
				}
			}
			// middle of click
			if (left_down)
			{
				if (anchor_down == null)
				{
					Vector3 difference = new Vector3 (last_click.x - mouse_pos.x, last_click.y - mouse_pos.y, last_click.z - mouse_pos.z);

					temp_xy += difference.y * rotate_speed;
					temp_xz -= difference.x * rotate_speed;

					if (temp_xy % 360 < -85)
						temp_xy = -85;

					if (temp_xy % 360 > 85)
						temp_xy = 85;

					if (Input.GetMouseButton (0) == false)
					{
						angle_xy = temp_xy;
						angle_xz = temp_xz;

						angle_xy = Mathf.Round(angle_xy/precision)*precision;
						angle_xz = Mathf.Round(angle_xz/precision)*precision;

						if (angle_xy % 360 < -85)
							angle_xy = -85;

						if (angle_xy % 360 > 85)
							angle_xy = 85;

						left_down = false;		
					}
				}
				else if (EDIT_MODE)
				{
					if (Input.GetMouseButton (0) == false)
					{
						foreach (Block block in block_list)
						{
							foreach (GameObject anchor in block.anchors)
							{
								if (anchor != anchor_down && (camera.WorldToScreenPoint(anchor.transform.position) - mouse_pos).magnitude < (40/Mathf.Sqrt(camera.orthographicSize)))
								{
									GameObject perm_line = Instantiate(anchor_drag_line);
									perm_line.transform.position = (anchor.transform.position + anchor_down.transform.position)/2;
									perm_line.GetComponent<MeshRenderer>().enabled = true;
									perm_line.transform.rotation = Quaternion.FromToRotation(Vector3.up, anchor.transform.position - anchor_down.transform.position);
									perm_line.transform.localScale = new Vector3(
										perm_line.transform.localScale.x*0.2f, 
										(anchor.transform.position - anchor_down.transform.position).magnitude/2, 
										perm_line.transform.localScale.z*0.2f
									);

									int face1 = 
										(anchor == block.anchors[Block.TOP] ? Block.TOP : 
											(anchor == block.anchors[Block.BOTTOM] ? Block.BOTTOM : 
												(anchor == block.anchors[Block.LEFT] ? Block.LEFT : 
													(anchor == block.anchors[Block.RIGHT] ? Block.RIGHT : 
														(anchor == block.anchors[Block.BACK] ? Block.BACK : 
															(anchor == block.anchors[Block.FRONT] ? Block.FRONT : 
																-1))))));
									int face2 = 
										(anchor_down == anchor_down_block.anchors[Block.TOP] ? Block.TOP : 
											(anchor_down == anchor_down_block.anchors[Block.BOTTOM] ? Block.BOTTOM : 
												(anchor_down == anchor_down_block.anchors[Block.LEFT] ? Block.LEFT : 
													(anchor_down == anchor_down_block.anchors[Block.RIGHT] ? Block.RIGHT : 
														(anchor_down == anchor_down_block.anchors[Block.BACK] ? Block.BACK : 
															(anchor_down == anchor_down_block.anchors[Block.FRONT] ? Block.FRONT : 
																-1))))));
									
									Path new_path = new Path(new VectorBlock3(block, face1), new VectorBlock3(anchor_down_block, face2));
									new_path.anchor_line_connect = perm_line;
									Path.join_path(new_path);

									break;
								}

								anchor_down.GetComponent<MeshRenderer>().material = mat_anchor;
								anchor_drag_line.GetComponent<MeshRenderer>().enabled = false;
							}
						}

						left_down = false;		
					}
					else
					{
						if (anchor_drag_line.GetComponent<MeshRenderer>().enabled == false)
							anchor_drag_line.GetComponent<MeshRenderer>().enabled = true;	

						Vector3 temp_mouse = camera.ScreenToWorldPoint(mouse_pos);
						anchor_drag_line.transform.position = (temp_mouse + anchor_down.transform.position)/2;
						anchor_drag_line.transform.rotation = Quaternion.FromToRotation(Vector3.up, temp_mouse - anchor_down.transform.position);
					}
				}
			}

			if (EDIT_MODE == false)
			{
				Block.fade_list_update(Time.deltaTime);

				if (Input.GetKey("up"))
				{
					player.evaluate_key(Player.FORWARD, camera, angle_xy, angle_xz);
				}
				else if (Input.GetKey("down"))
				{
					player.evaluate_key(Player.BACKWARD, camera, angle_xy, angle_xz);
				}
				else if (Input.GetKey("left"))
				{
					player.evaluate_key(Player.LEFT, camera, angle_xy, angle_xz);
				}
				else if (Input.GetKey("right"))
				{
					player.evaluate_key(Player.RIGHT, camera, angle_xy, angle_xz);
				}
			}
			else
			{
				//todo
			}
		}

		y = Mathf.Sin (temp_xy * Mathf.Deg2Rad) * pivot_distance;
		z = Mathf.Sqrt
			(
				(Mathf.Pow (pivot_distance, 2) - Mathf.Pow (Mathf.Sin (temp_xy * Mathf.Deg2Rad), 2) * Mathf.Pow (pivot_distance, 2))
				/
				(1 + Mathf.Pow (Mathf.Tan (temp_xz * Mathf.Deg2Rad), 2))
			);
		x = Mathf.Sqrt (Mathf.Pow (pivot_distance, 2) - Mathf.Pow (z, 2) - Mathf.Pow (y, 2) + 0.001f);

		if (temp_xz % 360 >= 90 && temp_xz % 360 < 270)
			z = -z;

		if (temp_xz % 360 >= 180 && temp_xz % 360 < 360)
			x = -x;

		temp_xz = temp_xz + 180;

		Vector3 position = new Vector3 (pivot.x + x, pivot.y + y, pivot.z + z);

		camera.transform.position = position;
		camera.transform.LookAt(pivot);
		light.transform.position = position;
		light.transform.LookAt(pivot);

		if (MMENU_MODE)
		{
			angle_xz += Time.deltaTime*10;
			angle_xz = angle_xz > 135 ? 35 : angle_xz;
		}

		/*if (EDIT_MODE)
		{
			foreach (Block block in block_list)
			{
				if (block is BlockStairs)
				{
					((BlockStairs) block).hide_mesh.GetComponent<MeshRenderer>().enabled = false;
					((BlockStairs) block).hide_mesh.transform.position = block.position;// - new Vector3(3.5f, 0, 0);
				}
			}
		}
		else
		{
			foreach (Block block in block_list)
			{
				if (block is BlockStairs)
				{
					((BlockStairs) block).hide_mesh.GetComponent<MeshRenderer>().enabled = false;
				}
			}
		}*/
	}

	void OnGUI()
	{
		if (!MMENU_MODE)
		{
			float mag = Mathf.Min(Mathf.Min(progress_color.r, progress_color.g), progress_color.b);
			mag = mag*mag*mag;
			mag = mag < 0 ? 0 : (mag > 1 ? 1 : mag);

			int x = Screen.width;

			if (EDIT_MODE)
			{
				GUI.color = new Color(mag, mag, mag, progress_color.a);
				GUI.Label(new Rect(x - 10 - 100, 10, 200, 20), "XY Angle: " + angle_xy);
				GUI.Label(new Rect(x - 10 - 100, 30, 200, 20), "XZ Angle: " + angle_xz);
			}
		}
	}
}