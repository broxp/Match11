using System;
using System.Collections.Generic;
using System.Linq;

static class RubyExt
{
	public static string to_s (this object o)
	{
		return o == null ? "null" : o.ToString ();
	}

	public static IEnumerable<int> up_to (this int o, int upper)
	{
		return Enumerable.Range (o, upper - o + 1);
	}

	public static IEnumerable<int> downto (this int o, int upper)
	{
		return o < upper ? Enumerable.Empty<int> () : upper.up_to (o).Reverse ();
	}

	public static int to_i (this String o)
	{
		int res;
		return int.TryParse (o, out res) ? res : 0;
	}
}

public class BoardState
{
	public int[][] field;
	public int points;
}

public class Ruby : BoardState
{
	// Constants:
	readonly int[] invalid = new int[0];
	readonly object[] no_moves = new object[0];
	readonly int[] COLOURS = new[]{1, 2, 3, 4, 5};
	readonly string[] DIR = new []{"r", "l", "u", "d"};
	readonly int MIN_NEIGHBOURS = 2;
	readonly Random rnd = new Random ();
	readonly int column = 0;
	readonly int row = 1;
	// 
	String INPUT;
	int select_row;
	int select_col;
	String direction;
	object[] pos;
	//
	int HEIGHT = 5;
	int WIDTH = 5;

	public void init (int w, int h)
	{
		HEIGHT = h;
		WIDTH = w;
	}
	
	public int rand (int max)
	{
		return rnd.Next (max);
	}

	public static void Main (string[] args)
	{
		new Ruby ().main ();
	}

	public void puts (String args)
	{
		Console.WriteLine (args);
	}

	public void gets ()
	{
		INPUT = Console.ReadLine ();
	}

	/// Prints a field and points.
	public void puts_field ()
	{
		var i = 1;
		var res = "column  ";
		foreach (var x in 1.up_to(WIDTH)) {
			res += x.to_s ();
			res += "  ";
		}
		res += "    points\n      ";
		var len = WIDTH - 1;
		for (var counter = 0; counter < len; counter++) {
			res += " ..";
		}
		res += "      ";
		res += points.to_s ();
		res += "\nrow ";
		foreach (var row in field) {
			if (i > 1) {
				res += "    ";
			}
			res += i.to_s ();
			res += ":  ";
			i += 1;
			foreach (var x in row) {
				res += x.to_s ();
				res += "  ";
			}
			res += "\n";
		}
		res += "\n";
		puts (res);
	}

	/// Gets all deletions foreach (var the field as a self-explanatory list of lists.
	/// A deletion has this structure [row | column, index, 0, start, 0, count].
	/// The returned deletion list may be empty.
	public List<int[]> get_deletions ()
	{
		var deletions = new List<int[]> ();
		foreach (var x in 0.up_to(WIDTH-1)) {
			var following = 0;
			var start = 0;
			foreach (var y in 0.up_to(HEIGHT-2)) {
				if (field [y] [x] == field [y + 1] [x]) {
					following += 1;
				} else if (following < MIN_NEIGHBOURS) {
					following = 0;
					start = y + 1;
				} 
			}
			if (following >= MIN_NEIGHBOURS) {
				deletions.Add (new []{column, x, 0, start, 0, following});
			}
		}

		foreach (var y in 0.up_to(HEIGHT-1)) {
			var following = 0;
			var start = 0;
			foreach (var x in 0.up_to(WIDTH-2)) {
				if (field [y] [x] == field [y] [x + 1]) {
					following += 1;
				} else if (following < MIN_NEIGHBOURS) {
					following = 0;
					start = x + 1;
				}
			}
			if (following >= MIN_NEIGHBOURS) {
				deletions.Add (new[]{row, y, 0, start, 0, following});
			}
		}
		return deletions;
	}
	
	/// Executes the deletions by setting deleted values to 0.
	public void exec_deletions (List<int[]> deletions)
	{
		foreach (var pairs in get_indices_of_deletions(deletions)) {
			var y = pairs [0];
			var x = pairs [1];

			field [y] [x] = 0;
		}
	}
	
	/// Executes the deletions by setting deleted values to 0.
	public IEnumerable<int[]> get_indices_of_deletions (List<int[]> deletions)
	{
		foreach (var deletion in deletions) {
			var start = deletion [3];
			var count = deletion [5];
			if (deletion.First () == row) {
				var y = deletion [1];
				foreach (var x in start.up_to(start+count)) {
					yield return new []{y,x};
				}
			} else if (deletion.First () == column) {
				var x = deletion [1];
				foreach (var y in start.up_to(start+count)) {
					yield return new []{y,x};
				}
			}
		}
	}

	/// Lets all numbers fall down (0 denotes an empty field)
	public void fall_down ()
	{
		foreach (var x in 0.up_to(WIDTH-1)) {
			foreach (var y in (HEIGHT-1).downto (0)) {
				if (field [y] [x] == 0) {
					//puts("At "+ y+ ","+ x+ " == 0, above:");
					var set = false;
					foreach (var index in (y-1).downto (0)) {
						if (!set) {
							set = true;
							var elem = field [index] [x];
							field [y] [x] = elem;
							//puts("  " + elem + " at (" + index + "," + x + ") to " + y + ","+x);
						}
						if (index >= 1) {
							//puts("  moved (" + (index - 1) +","+ x + " to (" + index + "," + x + ")");
							field [index] [x] = field [index - 1] [x];
						} else {
							field [index] [x] = 0;
							//puts("  cleared (" + index + "," + x + ")");
						}
					}
				}
			}
		}
	}

	/// Replaces all 0s with random elements from COLOURS.
	public void refill ()
	{
		foreach (var y in 0.up_to(HEIGHT-1)) {
			foreach (var x in 0.up_to(WIDTH-1)) {
				if (field [y] [x] == 0) {
					field [y] [x] = COLOURS [rand (COLOURS.Length)];
				}
			}
		}
	}

	public void clear ()
	{
		foreach (var y in 0.up_to(HEIGHT-1)) {
			foreach (var x in 0.up_to(WIDTH-1)) {
				field [y] [x] = 0;
			}
		}
	}

	/// Gets the neighbor index as [y, x].
	/// Returns invalid if an invalid direction was passed.
	/// Returns null if the resulting index were outside.
	public int[] index_near (int select_row, int select_col, String direction)
	{
		if (!DIR.Contains (direction)) {
			return invalid;
		} else if (direction == DIR [0] && select_col + 1 < WIDTH) {
			return new []{select_row, select_col + 1};
		} else if (direction == DIR [1] && select_col - 1 >= 0) {
			return new []{select_row, select_col - 1};
		} else if (direction == DIR [2] && select_row - 1 >= 0) {
			return new []{select_row - 1, select_col};
		} else if (direction == DIR [3] && select_row + 1 < HEIGHT) {
			return new []{select_row + 1, select_col};
		}
		return invalid;
	}

	/// Checks if( there are any possible moves.
	/// Returns a possible move as [y, x, dir] or null if there are none.
	public object[] no_more_moves ()
	{
		foreach (var row in 0.up_to(HEIGHT-1)) {
			foreach (var col in 0.up_to(HEIGHT-1)) {
				foreach (var direction in DIR) {
					var pair = index_near (row, col, direction);
					if (pair != invalid) {
						swap_fields (row, col, pair [0], pair [1]);
						var list = get_deletions ();
						swap_fields (pair [0], pair [1], row, col);
						if (list.Any ()) {
							return new object[]{row, col, direction};
						}
					}
				}
			}
		}
		return no_moves;
	}

	/// Swaps two items in the field by passing their indices.
	public void swap_fields (int select_row, int select_col, int neighbour_row, int neighbour_col)
	{
		var temp = field [select_row] [select_col];
		field [select_row] [select_col] = field [neighbour_row] [neighbour_col];
		field [neighbour_row] [neighbour_col] = temp;
	}

	/// Deletes all matches in the field, keeping their score.
	/// Then let elements fall down and replaces them.
	/// Returns the points gotten by deletions during this process.
	public int delete_and_refill ()
	{
		var points = 0;
		var done = false;
		var first = true;
		while (!done) {
			var deletions = get_deletions ();
			//puts ("<-");
			//puts_field();
			//puts_del(deletions);
			//puts ("->");
			done = !deletions.Any ();
			if (deletions.Any () && !first) {
				//puts_field();
				//puts ("combo!");
				notify ("combo");
			}
			foreach (var delition in deletions) {
				points += 1 + delition [5];
			}
			exec_deletions (deletions);
			fall_down ();
			refill ();
			first = false;
			//puts_field ();
		}
		return points;
	}

// Useful testing samples:
// delete_t_shape = [
//     [2,6,6,6,1],
//     [2,5,5,5,5],
//     [7,7,7,2,1],
//     [1,8,5,2,1],
//     [3,3,2,2,2]
// ]
//
// no_moves = [
//     [2,3,1,2,4],
//     [1,5,5,4,2],
//     [1,2,1,3,1],
//     [2,4,5,4,1],
//     [3,3,2,5,2]
// ]
	void notify (String name)
	{

	}

	void puts_del (List<int[]> deletions)
	{
		foreach (var deletion in deletions) {
			String type = deletion [0] == column ? "col " : "row ";
			var index = deletion [1];
			var start = deletion [3];
			var count = deletion [5];
			puts (type + " " + index + "starting at " + start + " with items: " + count);
		}
	}

	public void main ()
	{
		//puts (String.Join(", ", 1.up_to (1).Select (x => x.to_s ()).ToArray()));
		// 1
		//puts (String.Join(", ", 1.downto (1).Select (x => x.to_s ()).ToArray()));
		// 1
		//puts (String.Join(", ", 0.up_to (5).Select (x => x.to_s ()).ToArray()));
		// 0,1,2,3,4,5
		//puts (String.Join(", ", 6.downto (5).Select (x => x.to_s ()).ToArray()));
		// 6,5
		//puts (String.Join(", ", 6.downto (7).Select (x => x.to_s ()).ToArray()));
		// <empty>
		//puts (String.Join(", ", 6.up_to (5).Select (x => x.to_s ()).ToArray()));
		// <empty>

// Program starts here:
		puts ("Welcome to match3 (broxp.github.com/match3), a ruby app in your browser (using opal).\n");
		puts ("The target is to gain points by matching 3 or more numbers of the same value.\n");
		puts ("You will see the field displayed and select a row and column as well as a direction.\n");
		puts ("){, the fields are swapped and 3 or more neighbours will disappear gaining you points.\n");
		puts ("Directions are: r foreach (var right, l foreach (var left, u foreach (var up and d foreach (var down.\n");
		puts ("The game will } if( there are no more valid moves.\n");
		puts ("Entering ? when asked foreach (var a row will show a hint. Good luck!\n");
		puts ("\n");

		prepare ();
		main_loop ();
	}

	public void prepare ()
	{
		initialize ();
		
		refill ();
		delete_and_refill ();
		// puts_field ();
	}

	void initialize ()
	{
		field = new int[HEIGHT][];
		for (int i = 0; i < HEIGHT; i++) {
			field [i] = new int[WIDTH];
		}
		points = 0;
	}

	int safetyCounter = 0;

	void main_loop ()
	{
		pos = no_more_moves ();
		
		if (pos == no_moves) {
			//puts ("...");
			notify ("no moves");
			if (safetyCounter++ > 4) {
				safetyCounter = 0;
				clear ();
				notify ("reset");
			}
			refill ();
			delete_and_refill ();
			main_loop ();
			// puts ("There are no more moves. Thank you foreach (var playing.\n");
		} else {
			// Input processing
			puts ("Row> ");
			gets ();
			after_row_input ();
		}
	}

	void after_row_input ()
	{
		var row_str = INPUT .Trim ();
		select_row = row_str.to_i () - 1;

		if (row_str == "?") {
			puts ("Hint: " + ((int)pos [0] + 1) + " " + ((int)pos [1] + 1) + " " + (pos [2]));
			main_loop ();
		} else if (!(0 <= select_row && select_row < HEIGHT)) {
			puts ("invalid choice (expecting a number from 1 to " + HEIGHT + "\n");
			main_loop ();
		} else {
			puts ("Column> ");
			gets ();
			after_col_input ();
		}
	}

	void after_col_input ()
	{
		select_col = INPUT.Trim ().to_i () - 1;
		if (!(0 <= select_col && select_col < WIDTH)) {
			//puts ("invalid choice (expecting a number from 1 to //{HEIGHT})\n");
			main_loop ();
		} else {
			puts ("Direction> ");
			gets ();
			after_direction_input ();
		}
	}
	
	void after_direction_input ()
	{
		var direction = INPUT.Trim ();
		
		var neighbor_pos = index_near (select_row, select_col, direction);
		if (neighbor_pos == invalid) {
			puts ("invalid choice (valid directions are r, l, u or d)\n");
			main_loop ();
		} else if (neighbor_pos == null) {
			puts ("invalid choice (neighbour is out of field)\n");
			main_loop ();
		} else {
			swap_fields (select_row, select_col, neighbor_pos [0], neighbor_pos [1]);
			//puts_field(sample)
			var deletions = get_deletions ();
			//puts ("<-");
			//puts_field ();
			//puts_del(deletions);
			//puts ("->");
			if (!deletions.Any ()) {
				swap_fields (neighbor_pos [0], neighbor_pos [1], select_row, select_col);
				puts ("invalid choice (will not result in match)\n");
				main_loop ();
			} else {
				// Score points
				points += delete_and_refill ();
				// Clear and redraw
				puts_field ();
				main_loop ();
			}
		}
	}
}