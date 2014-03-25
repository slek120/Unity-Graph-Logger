using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GraphLogger : MonoBehaviour
{
	// A List with min and max properties
	class Line
	{
		public List<Vector2> points = new List<Vector2> ();
		public float minPoint;
		public float maxPoint;
		public float ySize;
	}
	
	// Settings
	public enum Format
	{
		Column,
		Table
	}
	;
	public static GraphLogger graphLogger;
	public bool appendLogFile = false;
	public Format format = Format.Table;
	public int maxCount = 6000;
	public bool graphOn = true;
	public Color[] colors = new Color[6] {
		Color.blue,
		Color.cyan,
		Color.green,
		Color.magenta,
		Color.red,
		Color.yellow
	};
	public Vector2 size = new Vector2 (300, 150);
	public Vector2 offset = new Vector2 (10, 10);

	// Material for lines (Default-Diffuse)
	Material mat;
	// Dictionary of properties being debugged
	Dictionary<string, Line> lines = new Dictionary<string, Line> ();
	// Conversion factor for screen to viewport space
	float xScale;
	float yScale;
	// Scale factor for x-axis
	float xSize;

	void Awake ()
	{
		graphLogger = this;
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

	public void AddPoint (float point)
	{
		AddPoint (point, "default");
	}

	public void AddPoint (float point, string name)
	{
		float time = Time.frameCount;

		if (!lines.ContainsKey (name))
			lines.Add (name, new Line ());

		Line line = lines [name];

		if (line.points.Count == 0) {
			line.minPoint = point;
			line.maxPoint = point;
			xSize = size.x;
			line.ySize = size.y;
		}

		line.points.Add (new Vector2 (time, point));
		
		xSize = size.x / time;

		if (point < line.minPoint) {
			line.minPoint = point;
			if (line.minPoint != line.maxPoint)
				line.ySize = size.y / (line.maxPoint - line.minPoint);
		} else if (point > line.maxPoint) {
			line.maxPoint = point;
			if (line.minPoint != line.maxPoint)
				line.ySize = size.y / (line.maxPoint - line.minPoint);
		}

		if (line.points.Count > maxCount) {
			line.points.RemoveAt (0);
		}
	}

	// Save then quit
	void OnApplicationQuit ()
	{
		DestroyImmediate (mat);
		Application.CancelQuit ();
		SaveLog ();
		Application.Quit ();
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
			foreach (Vector2 point in line.points) {
				file.WriteLine (point.x + "," + point.y);
			}
			file.WriteLine ();
		}
	}
	
	void writeTable (StreamWriter file)
	{
		// Get all framecounts
		SortedDictionary<int,string> rows = new SortedDictionary<int, string> ();
		foreach (KeyValuePair<string,Line> element in lines) {
			foreach (Vector2 point in element.Value.points) {
				if (!rows.ContainsKey ((int)point.x))
					rows.Add ((int)point.x, "");
			}
		}
		
		// Add data if framecount exists
		List<int> keys = new List<int> (rows.Keys);
		foreach (KeyValuePair<string,Line> element in lines) {
			foreach (int key in keys) {
				rows [key] += ",";
			}
			foreach (Vector2 point in element.Value.points) {
				if (rows.ContainsKey ((int)point.x))
					rows [(int)point.x] += point.y.ToString ();
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
			float prevX = offset.x * xScale;
			float prevY = offset.y * yScale;
			GL.Color (colors [i % 6]);
			foreach (Vector2 point in line.points) {
				// Stretch points so that min is at offset and max is at offset+size
				// Then convert to viewport space
				float x = (point.x * xSize + offset.x) * xScale;
				float y = ((point.y - line.minPoint) * line.ySize + offset.y) * yScale;
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
