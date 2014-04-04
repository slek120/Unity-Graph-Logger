Unity-Graph-Logger
==================

A debug class that can keep track of multiple continuously changing floats and display it as a graph and save to csv on exit

How To Use:
-------------------
1. Add the script to a camera
2. Use the method "GraphLogger.AddPoint(float x,[string name])" or "GraphLogger.AddPoint(string x,[string name])" anywhere in your scripts
3. Use the method "GraphLogger.SaveLog()" to save log.csv to the persistent data folder (SaveLog() is called automatically on Application Quit, Pause, and Focus)
4. Open log.csv in the persistent data folder (path is logged in Console)

Comments:
-------------------
* The script must be added to a camera to view the graph
* You can check "Append Log File" to not overwrite previous log files.
* Table format is for viewing all types of data being tracked in a single table.
* Column format is to view each type of data separately.
* "Graph On" can turn off the graph if you only need the saving function.
* "Max Count" can increase or decrease the number of points displayed on the graph.
* "Size" is the pixel size in screen space and "offset" is the pixel distance from bottom left.
* Any suggestions can be emailed to me at [slek120@gmail.com](mailto:slek120@gmail.com?Subject=Unity%20Graph%20Logger)
