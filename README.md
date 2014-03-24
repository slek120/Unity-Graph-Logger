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
I cannot seem to get the lines to be different colors.
You can check "Append Log File" to not lose past log files.
Table format is for viewing all types of data being tracked in a single table.
Column format is to view each type of data separately.
You can increase or decrease the number of max points per type of data to track (too much can get slow).
You can turn off the graph (for better speed) if you only need the saving function.
"Size" is the pixel size in screen space and "offset" is the pixel distance from bottom left.
Any suggestions can be emailed to me at [slek120@gmail.com](mailto:slek120@gmail.com?Subject=Unity%20Graph%20Logger)
