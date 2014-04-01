using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class GraphLogger : MonoBehaviour
{
	// A List to track a property with min and max properties
	class Line
	{
		public SortedDictionary<int, string> strings;	// Strings with frame number for key
		public SortedDictionary<int, float> points;		// Numbers with frame number for key
		public bool isNumber;							// Tracked parameter is number or string
		public float minPoint;							// Smallest number in points
		public float maxPoint;							// Largest number in points
		public float ySize;								// Scale factor for y-axis

		public Line (int t, float x, float y)			// Constructor for number tracker
		{
			isNumber = true;
			minPoint = x;
			maxPoint = x;
			ySize = y;
			points = new SortedDictionary<int, float> ();
			points.Add (t, x);
		}

		public Line (int t, string s)					// Constructor for string tracker
		{
			isNumber = false;
			strings = new SortedDictionary<int, string> ();
			strings.Add (t, s);
		}
	}
	
	// Static instance of the class
	public static GraphLogger graphLogger;
	
	// Settings
	public enum Format
	{
		Column,
		Table
	}
	;
	public bool appendLogFile = false;				// Append or overwrite log file
	public Format format = Format.Table;			// Format for saved log file
	public int maxCount = 6000;						// Number of points logged per property
	public bool graphOn = true;						// Display a graph on screen
	public Color[] colors = new Color[6] {			// Colors of lines for graph
		Color.blue,
		Color.cyan,
		Color.green,
		Color.magenta,
		Color.red,
		Color.yellow
	};
	public Vector2 size = new Vector2 (300, 150);	// Area of graph (px)
	public Vector2 offset = new Vector2 (10, 10);	// Distance from bottom left corner (px)
	
	// Saved information
	Material mat;									// Material for lines (Default-Diffuse)
	Dictionary<string, Line> lines					// Dictionary of properties being debugged
		= new Dictionary<string, Line> ();
	float xScale;									// Conversion factor for screen to viewport space
	float yScale;
	float xSize;									// Scale factor for x-axis
	
	void Awake ()
	{
		// Only have one instance of graphLogger
		if (graphLogger == null) {
			graphLogger = this;
		} else {
			Destroy (this);
		}
		
		xScale = 1 / (float)Screen.width;
		yScale = 1 / (float)Screen.height;
		mat = new Material ("Shader \"Lines/Colored Blended\" {" +
			"SubShader { Pass { " +
			"    Blend SrcAlpha OneMinusSrcAlpha " +
			"    ZWrite Off Cull Off Fog { Mode Off } " +
			"    BindChannels {" +
			"      Bind \"vertex\", vertex Bind \"color\", color }" +
			"} } }");
		mat.hideFlags = HideFlags.HideAndDontSave;
		mat.shader.hideFlags = HideFlags.HideAndDontSave;
	}

	public void AddPoint (int point)
	{
		AddPoint ((float)point, "default");
	}

	public void AddPoint (int point, string name)
	{
		AddPoint ((float)point, name);
	}

	public void AddPoint (float point)
	{
		AddPoint (point, "default");
	}
	
	public void AddPoint (float point, string name)
	{
		int time = Time.frameCount;
		xSize = size.x / time;
		
		if (!lines.ContainsKey (name))
			lines.Add (name, new Line (time, point, size.y));
		else
			lines [name].points.Add (time, point);
		
		Line line = lines [name];	
		if (point < line.minPoint) {
			line.minPoint = point;
			if (line.minPoint != line.maxPoint)
				line.ySize = size.y / (line.maxPoint - line.minPoint);
		} else if (point > line.maxPoint) {
			line.maxPoint = point;
			if (line.minPoint != line.maxPoint)
				line.ySize = size.y / (line.maxPoint - line.minPoint);
		}
		
		if (line.points.Count > maxCount)
			line.points.Remove (line.points.Keys.First ());
	}

	public void AddPoint (string point)
	{
		AddPoint (point, "default");
	}
	
	public void AddPoint (string point, string name)
	{
		int time = Time.frameCount;
		
		if (!lines.ContainsKey (name))
			lines.Add (name, new Line (time, point));
		else
			lines [name].strings.Add (time, point);

		if (lines [name].strings.Count > maxCount)
			lines [name].strings.Remove (lines [name].strings.Keys.First ());
	}
	
	// Save then quit
	void OnApplicationQuit ()
	{
		Application.CancelQuit ();
		DestroyImmediate (mat);
		SaveLog ();
		Application.Quit ();
	}
	
	void OnApplicationPause (bool paused)
	{
		if (paused)
			SaveLog ();
	}
	
	void OnApplicationFocus (bool paused)
	{
		if (paused)
			SaveLog ();
	}
	
	public void SaveLog ()
	{
		try {
			using (StreamWriter file = new StreamWriter(Application.persistentDataPath + "/log.csv", appendLogFile)) {
				if (format == Format.Column)
					writeColumn (file);
				else if (format == Format.Table)
					writeTable (file);
				Debug.Log ("Log file saved: " + Application.persistentDataPath + "/log.csv");
			}
		} catch {
			Debug.LogError ("Could not save to path: " + Application.persistentDataPath + "/log.csv");
		}
	}
	
	void writeColumn (StreamWriter file)
	{
		foreach (KeyValuePair<string,Line> element in lines) {
			file.WriteLine ("Frame," + element.Key);
			Line line = element.Value;
			if (line.isNumber) {
				foreach (KeyValuePair<int, float> kvp in line.points) {
					file.WriteLine (kvp.Key + "," + kvp.Value);
				}
			} else {
				foreach (KeyValuePair<int, string> kvp in line.strings) {
					file.WriteLine (kvp.Key + "," + kvp.Value);
				}
			}
			file.WriteLine ();
		}
	}
	
	void writeTable (StreamWriter file)
	{
		// Get all framecounts
		SortedDictionary<int, string> rows = new SortedDictionary<int, string> ();
		foreach (KeyValuePair<string, Line> element in lines) {
			Line line = element.Value;
			if (line.isNumber) {
				foreach (int key in line.points.Keys) {
					if (!rows.ContainsKey (key))
						rows.Add (key, "");
				}
			} else {
				foreach (int key in line.strings.Keys) {
					if (!rows.ContainsKey (key))
						rows.Add (key, "");
				}
			}
		}
		
		// Add data if framecount exists
		List<int> keys = new List<int> (rows.Keys);
		foreach (KeyValuePair<string,Line> element in lines) {
			foreach (int key in keys) {
				rows [key] += ",";
			}
			Line line = element.Value;
			if (line.isNumber) {
				foreach (KeyValuePair<int, float> kvp in line.points) {
					if (rows.ContainsKey (kvp.Key))
						rows [kvp.Key] += kvp.Value.ToString ();
				}
			} else {
				foreach (KeyValuePair<int, string> kvp in line.strings) {
					if (rows.ContainsKey (kvp.Key))
						rows [kvp.Key] += kvp.Value.ToString ();
				}
			}
		}
		
		// Write first row
		file.Write ("Frame");
		foreach (KeyValuePair<string,Line> element in lines) {
			file.Write ("," + element.Key);
		}
		file.WriteLine ();
		// Write data
		foreach (KeyValuePair<int,string> element in rows) {
			file.WriteLine (element.Key + element.Value);
		}
		file.WriteLine ();
	}
	
	// Draw graph
	void OnPostRender ()
	{
		if (!graphOn)
			return;
		
		GL.PushMatrix ();
		mat.SetPass (0);
		GL.LoadOrtho ();
		GL.Begin (GL.LINES);

		int i = 0;
		foreach (KeyValuePair<string,Line> element in lines) {
			Line line = element.Value;
			if (!line.isNumber)
				break;
			float prevX = (line.points.Keys.First () * xSize + offset.x) * xScale;
			float prevY = ((line.points [line.points.Keys.First ()] - line.minPoint) * line.ySize + offset.y) * yScale;
			GL.Color (colors [i % 6]);
			foreach (KeyValuePair<int, float> kvp in line.points) {
				// Stretch points so that min is at offset and max is at offset+size
				// Then convert to viewport space
				float x = (kvp.Key * xSize + offset.x) * xScale;
				float y = ((kvp.Value - line.minPoint) * line.ySize + offset.y) * yScale;
				GL.Vertex3 (prevX, prevY, 0);
				GL.Vertex3 (x, y, 0);
				prevX = x;
				prevY = y;
			}
			i++;
		}

		GL.End ();
		GL.PopMatrix ();
	}
}
