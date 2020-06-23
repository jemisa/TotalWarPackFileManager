using Filetypes;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct ToolTipRegion
{
    private string name;
    private int x1;
    private int y1;
    private int x2;
    private int y2;
    public ToolTipRegion(AtlasObject aO)
    {
        this.name = aO.Container1;
        this.x1 = (int) aO.X1;
        this.y1 = (int) aO.Y1;
        this.x2 = (int) (aO.X1 + aO.X3);
        this.y2 = (int) (aO.Y1 + aO.Y3);
    }

    public string Name
    {
        get
        {
            return this.name;
        }
        set
        {
            this.name = value;
        }
    }
    public int X1
    {
        get
        {
            return this.x1;
        }
        set
        {
            this.x1 = value;
        }
    }
    public int Y1
    {
        get
        {
            return this.y1;
        }
        set
        {
            this.y1 = value;
        }
    }
    public int X2
    {
        get
        {
            return this.x2;
        }
        set
        {
            this.x2 = value;
        }
    }
    public int Y2
    {
        get
        {
            return this.y2;
        }
        set
        {
            this.y2 = value;
        }
    }
}

