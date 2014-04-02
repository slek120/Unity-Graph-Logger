Unity-Graph-Logger
==================

A debug class that can keep track of multiple continuously changing floats and display it as a graph and save to csv on exit

How To Use:
-------------------
1. Add the script to a camera
2. Use the method "GraphLogger.graphLogger.AddPoint (float x,[string name])" anywhere in your scripts
3. After quitting, go to the persistent data folder to view log.csv

Comments:
-------------------
* You can check "Append Log File" to not overwrite previous log files.
* Table format is for viewing all types of data being tracked in a single table.
* Column format is to view each type of data separately.
* "Graph On" can turn off the graph if you only need the saving function.
* "Max Count" can increase or decrease the number of points displayed on the graph.
* "Size" is the pixel size in screen space and "offset" is the pixel distance from bottom left.
* Any suggestions can be emailed to me at [slek120@gmail.com](mailto:slek120@gmail.com?Subject=Unity%20Graph%20Logger)
