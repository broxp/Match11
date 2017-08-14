using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class Match3
{
	Ruby ruby;

	public void init (int w, int h)
	{
		ruby = new Ruby ();
		ruby.init (w, h);
		ruby.prepare ();
	}
	
	public int get (int y, int x)
	{
		return ruby.field [y] [x];
	}
	
	public void swap (ITileData select, ITileData neighbour)
	{
		ruby.swap_fields (select.y, select.x, neighbour.y, neighbour.x);
	}

	public bool validSwap (TileData select, TileData neighbour)
	{
		ruby.swap_fields (select.y, select.x, neighbour.y, neighbour.x);
		var del = ruby.get_deletions ();
		ruby.swap_fields (select.y, select.x, neighbour.y, neighbour.x);
		return del.Count >= 1;
	}

	public void Print ()
	{
		ruby.puts_field ();
	}

	void mark (ITileData[][] arr, List<int[]> del, bool value)
	{
		foreach (var pair in ruby.get_indices_of_deletions (del)) {
			var y = pair [0];
			var x = pair [1];
			arr [y] [x].on = true;
		}
	}
	
	/// returns whether done
	public bool check ()
	{
		var del = ruby.get_deletions ();
		return del.Count == 0;
	}
	
	/// returns whether done
	public int deleteRefill ()
	{
		var del = ruby.get_deletions ();
		ruby.exec_deletions (del);
		var res  = del.Count;
		ruby.fall_down ();
		ruby.refill ();
		return res;
	}
}
