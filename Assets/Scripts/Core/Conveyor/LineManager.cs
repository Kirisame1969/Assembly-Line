using System.Collections;
using System.Collections.Generic;

//该脚本用来管理各种线路实例

public static class LineManager
{
    public static List<ConveyorLine> allLines = new List<ConveyorLine>();
    private static int nextId = 0;

    public static ConveyorLine CreateLine()
    {
        var line = new ConveyorLine { id = nextId++ };
        allLines.Add(line);
        return line;
    }

    public static void RemoveLine(ConveyorLine line)
    {
        allLines.Remove(line);
    }
}