using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid <T>
{
    public List<T> items = new List<T>();

    Vector2Int size;

    public Grid(int x, int y)
    {
        size = new Vector2Int(x,y);
    }
    public Grid(Vector2Int size_in)
    {
        size = size_in;
    }
    public T this[int i]
    {
        get
        {
            return items[i];
        }
        set
        {
            items[i] = value;
        }
    }
    public T this[int x, int y]
    {
        get
        {
            return items[x + size.x * y];
        }
        set
        {
            items[x + size.x * y] = value;
        }
    }
    public T this[Vector2Int v]
    {
        get
        {
            Debug.Log(v + " size: " + size);
            return items[v.x + size.x * v.y];
        }
        set
        {
            items[v.x + size.x * v.y] = value;
        }
    }    

    public void Add(T value)
    {
        items.Add(value);
    }
    public int Count()
    {
        return items.Count;
    }
    public Grid<T> RotateClockwise()
    {
        List<T> list = new List<T>();
        for(int x = size.x - 1; x >= 0; x--)
        {
            for(int y = 0; y < size.y; y++)
            {
                list.Add(items[x + size.x * y]);
            }
        }
        Grid<T> grid = new Grid<T>(size);
        grid.items = list;
        return grid;
    }
    public Grid<T> FlipVertically()
    {
        List<T> list = new List<T>();
        for(int y = size.y - 1; y >= 0; y--)
        {
            for(int x = 0; x < size.x; x++)
            {
                list.Add(items[x + size.x * y]);
            }
        }
        Grid<T> grid = new Grid<T>(size);
        grid.items = list;
        return grid;
    }
    
    public bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < size.x && y <= 0 && y > -size.y );
    }

    public bool IsPositionWithinBounds(Vector2Int pos)
    {
        if(pos.x >= 0 && pos.x < size.x && pos.y <= 0 && pos.y > -size.y )
        {
            return true;
        }
        // Debug.Log(pos + " <color=red>is not within bounds</color>");
        return false;
    }
    public int[] GetValidConstraints(int x, int y)
    {
        int i = x + size.x * y;
        int targY = (i / size.x);
        int startY = (targY - 1) % size.y; //! POS - 1 
        int targX = i % size.x;
        if (startY < 0) { startY = 0; }
        int startX = (targX - 1) % size.x; //! POS - 1
        if (startX < 0) { startX = 0; }
        int yLimit = targY + 2;
        int xLimit = targX + 2;

        if (xLimit > size.x) { xLimit = size.x; }
        if (yLimit > size.y) { yLimit = size.y; }

        return new int[4]{startX, startY, xLimit, yLimit };
    }
    public int[] GetValidConstraints(int x, int y, int range)
    {
        int i = x + size.x * y;
        int targY = (i / size.x);
        int startY = (targY - range) % size.y; //! POS - 1 
        int targX = i % size.x;
        if (startY < 0) { startY = 0; }
        int startX = (targX - range) % size.x; //! POS - 1
        if (startX < 0) { startX = 0; }
        int yLimit = targY + range + 2;
        int xLimit = targX + range + 2;

        if (xLimit > size.x) { xLimit = size.x; }
        if (yLimit > size.y) { yLimit = size.y; }

        return new int[4]{startX, startY, xLimit, yLimit };
    }
    public int[] GetValidConstraints(Vector2Int pos)
    {
        int i = pos.x + size.x * pos.y;
        int targY = (i / size.x);
        int startY = (targY - 1) % size.y; //! POS - 1 
        int targX = i % size.x;
        if (startY < 0) { startY = 0; }
        int startX = (targX - 1) % size.x; //! POS - 1
        if (startX < 0) { startX = 0; }
        int yLimit = targY + 2;
        int xLimit = targX + 2;

        if (xLimit > size.x) { xLimit = size.x; }
        if (yLimit > size.y) { yLimit = size.y; }

        return new int[4]{startX, startY, xLimit, yLimit };
    }
    public int[] GetValidConstraints(int i)
    {
        int targY = (i / size.x);
        int startY = (targY - 1) % size.y; //! POS - 1 
        int targX = i % size.x;
        if (startY < 0) { startY = 0; }
        int startX = (targX - 1) % size.x; //! POS - 1
        if (startX < 0) { startX = 0; }
        int yLimit = targY + 2;
        int xLimit = targX + 2;

        if (xLimit > size.x) { xLimit = size.x; }
        if (yLimit > size.y) { yLimit = size.y; }

        return new int[4]{startX, startY, xLimit, yLimit };
    }
}
