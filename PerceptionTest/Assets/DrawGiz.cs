using UnityEngine;
using System.Collections;
using System.IO;

public class DrawGiz : MonoBehaviour { 
    public float movRate = 0.0f;
    float nextMove;
    public GameObject cursor;
    GameObject startCube;
    static int width = 50;
    Material lineMat;
	ControllerGame.Block[, ,] grid = new ControllerGame.Block[width, width, width]; //x,y,z and cube type
    //GameObject[,,] cubes = new GameObject[width,width,width];
    public GameObject block_normal;
    public GameObject block_edge;
    public GameObject block_twist;
	public GameObject block_stairs;
    public GameObject start_prfb;
    public Material cursMat;
    public GameObject play_prfb;
    public GameObject fall_prfb;

    GameObject player;
    GameObject[] blockTypes = new GameObject[4]; //Number of block types
    bool testMode = false;
    Camera cam;
	ControllerGame controllerGame;

    int offset = width / 2;
    int cursorIndex = 0;
    int typeNum;
    public bool valid = false; //for testing
    Vector3 prevPos = new Vector3(0,1,0);
    
    enum BlockType { Cube, Twist, Edge };
    BlockType bType;
    
	void Start () {
        blockTypes[0] = block_normal;
        blockTypes[1] = block_edge;
        blockTypes[2] = block_twist;
		blockTypes[3] = block_stairs;
        typeNum = blockTypes.Length;

        cursor = (GameObject)Instantiate(blockTypes[cursorIndex % typeNum], Vector3.zero, new Quaternion(0,0,0,0));
        cursMat = new Material(cursor.GetComponent<Renderer>().sharedMaterial);  //Save the material set in the inspector for the cursor;
        cursMat.color = new Color(0.0f, 0.4f, 0.4f, 0.1f); //Shake my head
         //Set cursor to block_normal;
		ChangeBlock();
        
        cam = Camera.main;
		controllerGame = cam.GetComponent<ControllerGame>();

		grid[width/2, width/2, width/2] = controllerGame.start_block;
	}

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(width, width, width));
    }

    void currBlock() // Get current block player is on
    {

    }
    
	void Update () { 
		if (!controllerGame.MMENU_MODE)
		{
	        if (Time.time > nextMove)
	        {
	            int x = (int)cursor.transform.position.x;
	            int y = (int)cursor.transform.position.y;
	            int z = (int)cursor.transform.position.z;

				if (Input.GetKeyDown(KeyCode.Q)) controllerGame.TOGGLE_EDIT_MODE(); 
	            
				if (controllerGame.EDIT_MODE)
	            {
	                if (Input.GetKeyDown(KeyCode.UpArrow) && z > -offset) cursor.transform.position += new Vector3(0, 0, -1); 
	                if (Input.GetKeyDown(KeyCode.DownArrow) && z < offset - 1) cursor.transform.position += new Vector3(0, 0, 1);
	                if (Input.GetKeyDown(KeyCode.LeftArrow) && x < offset - 1) cursor.transform.position += new Vector3(1, 0, 0);
	                if (Input.GetKeyDown(KeyCode.RightArrow) && x > -offset) cursor.transform.position += new Vector3(-1, 0, 0);
	                if (Input.GetKeyDown(KeyCode.U) && y < offset - 1) cursor.transform.position += new Vector3(0, 1, 0);
	                if (Input.GetKeyDown(KeyCode.J) && y > -offset) cursor.transform.position += new Vector3(0, -1, 0);
	                if (Input.GetKeyDown(KeyCode.Space)) ToggleSquare();
	                if (Input.GetKeyDown(KeyCode.S)) SaveMap();
	                if (Input.GetKeyDown(KeyCode.L)) LoadMap();
	                if (Input.GetKeyDown(KeyCode.P)) Application.CaptureScreenshot("Screenshot.png", 5);
	                if (Input.GetKeyDown(KeyCode.LeftControl)) ChangeBlock();
					if (Input.GetKeyDown(KeyCode.K)) ToggleFall();
	                if (Input.GetKeyDown(KeyCode.Alpha1)) cursor.transform.Rotate(new Vector3(90, 0, 0));
	                if (Input.GetKeyDown(KeyCode.Alpha2)) cursor.transform.Rotate(new Vector3(0, 0, 90));
	                if (Input.GetKeyDown(KeyCode.Alpha3)) cursor.transform.Rotate(new Vector3(0, 90, 0));
	                nextMove = Time.time + movRate;
	            }
	        }
		}
	}

	void ToggleFall()
	{
		int x = (int)cursor.transform.position.x + offset;
		int y = (int)cursor.transform.position.y + offset;
		int z = (int)cursor.transform.position.z + offset;

		if (grid[x, y, z] != null)
		{
			if (ControllerGame.Block.fade_list.Contains(grid[x, y, z]))
			{
				ControllerGame.Block.fade_list.Remove(grid[x, y, z]);
				grid[x, y, z].mesh.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			}
			else
			{
				ControllerGame.Block.fade_list.Add(grid[x, y, z]);
				grid[x, y, z].mesh.GetComponent<Renderer>().material.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
			}
		}
		else Debug.Log("Not a valid Block!");
	}
		
    void ChangeBlock()
    {
        DestroyObject(cursor);
        cursor = (GameObject)Instantiate(blockTypes[cursorIndex % typeNum],cursor.transform.position,cursor.transform.rotation);
        cursor.name = "Cursor";
		cursor.transform.localScale *= 1.01f;

        Renderer rend = cursor.GetComponent<Renderer>();
        rend.sharedMaterial = cursMat;
        cursorIndex++; //CusorIndex maintaints the NEXT blocktype. -1 to get the current blockType

    }

    void ShiftLeft()
    {
		foreach(ControllerGame.Block c in grid){
            //if (c.activeInHierarchy) c.transform.position += new Vector3(1, 0, 0);
			c.mesh.transform.position += new Vector3(1, 0, 0);
        }
    }

    void CreateGrid()
    {
        float w = width; /// 2 -0.5f;
        for (float d = -w; d <= w; d++)
        {
            for (float x = -w; x <= w; x++)
            {
                ////Debug.Log(i);
                CreateLine(new Vector3(-w, x, d), new Vector3(w, x, d));
                CreateLine(new Vector3(x, -w, d), new Vector3(x, w, d));
                CreateLine(new Vector3(d, x, -w), new Vector3(d, x, w));
            }
        }
    }

    public void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject line = new GameObject("myLine");
        line.AddComponent<LineRenderer>();
        LineRenderer lr = line.GetComponent<LineRenderer>();
        //lr.material = lineMat;
        lr.SetColors(Color.blue, Color.blue);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.SetWidth(0.1f, 0.1f);
    }
		
    void ToggleSquare()
    {
        int x = (int)cursor.transform.position.x + offset;
        int y = (int)cursor.transform.position.y + offset;
        int z = (int)cursor.transform.position.z + offset;

		if (grid[x, y, z] != null)
		{
			grid[x, y, z].remove_existance(controllerGame);
			grid[x, y, z] = null;
		}
		else
		{
			if (blockTypes[(cursorIndex-1 )% typeNum] == block_normal)
				grid[x, y, z] = new ControllerGame.BlockNormal(
					controllerGame.block_normal, 
					controllerGame.anchor_line, 
					controllerGame.anchor_point, 
					new Vector3(x - offset, y - offset, z - offset),
					cursor.transform.eulerAngles, false);

			else if (blockTypes[(cursorIndex-1 )% typeNum] == block_twist)
				grid[x, y, z] = new ControllerGame.BlockTwist(
					controllerGame.block_twist,
					controllerGame.anchor_line, 
					controllerGame.anchor_point, 
					new Vector3(x - offset, y - offset, z - offset), 
					cursor.transform.eulerAngles, false);

			else if (blockTypes[(cursorIndex-1 )% typeNum] == block_edge)
				grid[x, y, z] = new ControllerGame.BlockEdge(
					controllerGame.block_edge,
					controllerGame.anchor_line, 
					controllerGame.anchor_point, 
					new Vector3(x - offset, y - offset, z - offset), 
					cursor.transform.eulerAngles, false);

			else if (blockTypes[(cursorIndex-1 )% typeNum] == block_stairs)
				grid[x, y, z] = new ControllerGame.BlockStairs(
					controllerGame.block_stairs,
					controllerGame.anchor_line,
					controllerGame.anchor_point,
					new Vector3(x - offset, y - offset, z - offset),
					cursor.transform.eulerAngles, 
					controllerGame.block_stairs_hide);
			
			controllerGame.block_list.Add(grid[x, y, z]);
		}
    }

	void ToggleSquare(int x, int y, int z, Quaternion r, int t, int f)
	{
		if (grid[x, y, z] == null)
		{
			if (blockTypes[t] == block_normal)
				grid[x, y, z] = new ControllerGame.BlockNormal(
					controllerGame.block_normal,
					controllerGame.anchor_line,
					controllerGame.anchor_point,
					new Vector3(x - offset, y - offset, z - offset),
					r.eulerAngles, (f == 1));

			else if (blockTypes[t] == block_twist)
				grid[x, y, z] = new ControllerGame.BlockTwist(
					controllerGame.block_twist,
					controllerGame.anchor_line,
					controllerGame.anchor_point,
					new Vector3(x - offset, y - offset, z - offset),
					r.eulerAngles, (f == 1));

			else if (blockTypes[t] == block_edge)
				grid[x, y, z] = new ControllerGame.BlockEdge(
					controllerGame.block_edge,
					controllerGame.anchor_line,
					controllerGame.anchor_point,
					new Vector3(x - offset, y - offset, z - offset),
					r.eulerAngles, (f == 1));

			else if (blockTypes[t] == block_stairs)
				grid[x, y, z] = new ControllerGame.BlockStairs(
					controllerGame.block_stairs,
					controllerGame.anchor_line,
					controllerGame.anchor_point,
					new Vector3(x - offset, y - offset, z - offset),
					r.eulerAngles, 
					controllerGame.block_stairs_hide);

			controllerGame.block_list.Add(grid[x, y, z]);
		}
		else
		{
			//grid[x, y, z].remove_existance(controllerGame);
		}
	}

	void SaveMap()
	{
		if (UnityEditor.EditorUtility.DisplayDialog("Save?", "Would you like to save the map?", "Save","Cancel"))
		{
			string mapData = "";
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < width; y++)
				{
					for (int z = 0; z < width; z++)
					{
						if (grid[x, y, z] != null)
						{
							int n = 0;
							int f = ControllerGame.Block.fade_list.Contains(grid[x, y, z]) ? 1 : 0;
							if (grid[x, y, z] is ControllerGame.BlockNormal) n = 0;
							if (grid[x, y, z] is ControllerGame.BlockEdge) n = 1;
							if (grid[x, y, z] is ControllerGame.BlockTwist) n = 2;
							if (grid[x, y, z] is ControllerGame.BlockStairs) n = 3;
							string pstr = "|";

							foreach (ControllerGame.Path p in grid[x, y, z].paths) 
							{
								if (p.b1.block == grid[x, y, z])
									pstr += p.b1.block.position + "/" + p.b1.face + "/" + p.b2.block.position + "/" + p.b2.face + "/" + (p.xz) + "/" + (p.xy) + "+";
								else if (p.b2.block == grid[x, y, z])
									pstr += p.b2.block.position + "/" + p.b2.face + "/" + p.b1.block.position + "/" + p.b1.face + "/" + (p.xz) + "/" + (p.xy) + "+";
								else
									Debug.Log("wtf!");
							}

							pstr = pstr.TrimEnd('+');
							if (grid[x, y, z] != null) { mapData += "" + (x) + "|" + (y) + "|" + (z) + "|" + grid[x, y, z].mesh.transform.rotation + "|" + n + "|" + f + pstr + "\r\n"; }
						}
					}
				}
			}

			mapData = mapData.ToString().TrimEnd( '\r', '\n' );
			StreamWriter file = new StreamWriter("mapData.txt");
			file.WriteLine(mapData);
			file.Close();
		}
	}

	public void reset()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < width; y++)
			{
				for (int z = 0; z < width; z++)
				{
					if (grid[x, y, z] != controllerGame.start_block && grid[x, y, z] != null)
					{
						grid[x, y, z].remove_existance(controllerGame);
					}
				}
			}
		}

		ControllerGame.player.position = new ControllerGame.VectorBlock3(controllerGame.start_block, ControllerGame.Block.TOP);
	}

	void LoadMap(){ 
		reset();

		string path = UnityEditor.EditorUtility.OpenFilePanel("Select Map", "", "txt");
		string[] pos;
		ArrayList links = new ArrayList();
		StreamReader file = new StreamReader(path);
		while (!file.EndOfStream)
		{
			pos = file.ReadLine().Split('|');
			ToggleSquare((int.Parse(pos[0])), int.Parse(pos[1]), int.Parse(pos[2]), ParseQuat(pos[3]), int.Parse(pos[4]), int.Parse(pos[5])); //0-2 = x,y,z, | 3 = rotation | 4 = block type
			//ParsePaths(pos[6], (int.Parse(pos[0])), int.Parse(pos[1]), int.Parse(pos[2]));
			links.Add(pos[6]);
		}
		foreach (string x in links)
		{
			ParsePaths(x);
		}
	}

	void LoadMap(string map)
	{
		reset();

		string[] pos;
		ArrayList links = new ArrayList();
		StreamReader file = new StreamReader(map);
		while (!file.EndOfStream)
		{
			pos = file.ReadLine().Split('|');
			ToggleSquare((int.Parse(pos[0])), int.Parse(pos[1]), int.Parse(pos[2]), ParseQuat(pos[3]), int.Parse(pos[4]), int.Parse(pos[5])); //0-2 = x,y,z, | 3 = rotation | 4 = block type
			//ParsePaths(pos[6], (int.Parse(pos[0])), int.Parse(pos[1]), int.Parse(pos[2]));
			links.Add(pos[6]);
		}
		foreach (string x in links)
		{
			ParsePaths(x);
		}

	}

	Quaternion ParseQuat(string s)
	{
		char[] c = { '(', ')' };
		s = s.Trim(c);
		string[] rot = s.Split(',');
		Quaternion k = new Quaternion(float.Parse(rot[0]), (float.Parse(rot[1])), (float.Parse(rot[2])), (float.Parse(rot[3])));
		return k;
	}
	//To Add angle just increment by four, and take angles as third and fourth paramters after split (/)
	void ParsePaths(string s)
	{
		string[] j = s.Split('+');
		foreach (string k in j)
		{
			string[] l = k.Split('/');
			for (int i = 0; i < l.Length; i += 1)
			{
				Debug.Log(i.ToString()+"saeghhaseh"+l[i] .ToString());
			}
			Vector3 v1 = ParseVect(l[0]);
			Vector3 v2 = ParseVect(l[0 + 2]);
			ControllerGame.VectorBlock3 b1 = new ControllerGame.VectorBlock3(grid[(int)v1.x + offset, (int)v1.y + offset, (int)v1.z + offset], int.Parse(l[0 + 1]));
			ControllerGame.VectorBlock3 b2 = new ControllerGame.VectorBlock3(grid[(int)v2.x + offset, (int)v2.y + offset, (int)v2.z + offset], int.Parse(l[0 + 3]));
			ControllerGame.Path pat = new ControllerGame.Path(b1, b2);
			pat.xz = float.Parse(l[4]);
			pat.xy = float.Parse(l[5]);
			//grid[x, y, z].paths.Add(new ControllerGame.Path(new ControllerGame.VectorBlock3(grid[(int)v1.x + offset, (int)v1.y + offset, (int)v1.z + offset], int.Parse(l[0 + 1])), new ControllerGame.VectorBlock3(grid[(int)v2.x + offset, (int)v2.y + offset, (int)v2.z + offset], int.Parse(l[0 + 3]))));
			//ControllerGame.Path.join_path(new ControllerGame.Path(new ControllerGame.VectorBlock3(grid[(int)v1.x + offset, (int)v1.y + offset, (int)v1.z + offset], int.Parse(l[0 + 1])), new ControllerGame.VectorBlock3(grid[(int)v2.x + offset, (int)v2.y + offset, (int)v2.z + offset], int.Parse(l[0 + 3]))));
			ControllerGame.Path.join_path(pat);

		}
	}
	Vector3 ParseVect(string s)
	{
		char[] c = { '(', ')' };
		s = s.Trim(c);
		string[] pos = s.Split(',');
		Vector3 k = new Vector3(float.Parse(pos[0]), (float.Parse(pos[1])), (float.Parse(pos[2])));
		return k;
	}
}
