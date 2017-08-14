using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public interface ITileData
{
	int x { get; set; }

	int y { get; set; }

	int colour { get; set; }

	bool on { get; set; }
}

public class TileData : ITileData
{
	public int x { get; set; }

	public int y { get; set; }

	public int colour { get; set; }

	public bool on { get; set; }

	public Button btn;

	public override string ToString ()
	{
		return "row " + y + ", col " + x + " in " + colour;
	}
}

public class Board : MonoBehaviour
{
	public int w = 20;
	public int h = 10;
	public Vector3 offset = new Vector3 (-320, -120, 0);
	public Vector3 offset2 = new Vector3 (-320, -120, 0);
	public float scale = 8;
	public int size = 32;
	public int waitTime = 25;
	//
	public Button proto;
	public Text output;
	float lastScale;
	//Vector3 screen;
	Match3 match3;
	string text;
	TileData first;
	TileData[][] data;
	bool hasChanges;
	int waitCounter;
	int comboCounter;
	int points;
	static Color[] colours = new [] {
		Color.white,
		Color.Lerp(Color.cyan, Color.blue, 0.8f),
		Color.Lerp(Color.green, Color.black, 0.5f),
		Color.yellow,
		Color.Lerp(Color.red, Color.black, 0.2f),
		Color.Lerp(Color.yellow, Color.red, 0.5f)
	};
	static Color[] coloursHighlight = Array.ConvertAll(colours, x => Color.Lerp(x, Color.white, 0.33f));

	void Update ()
	{
		if (hasChanges && match3 != null) {

			if (waitCounter++ < waitTime) {
				return;
			}
			waitCounter = 0;
			
			hasChanges = !match3.check ();
			if (hasChanges) {
				deleteRefill();
				comboCounter++;
				text = "Points: " + points + "   Combo: x" + comboCounter;
			} else {
				comboCounter = 0;
				text = "Points: " + points;
			}
		}
		output.text = text;

		//Resolution res = Screen.currentResolution;
		//screen = new Vector3 (res.width, res.height);

		if (lastScale == scale) {
			return;
		}
		match3 = new Match3 ();
		match3.init (w, h);
		hasChanges = true;
		lastScale = scale;
		foreach (var item in gameObject.GetComponentsInChildren<Button>()) {
			if (item.tag != "keep") {
				Destroy (item.gameObject);
			}
		}
		data = new TileData[h][];
		//size = (int) (scale * 3);
		for (int y = 0; y < h; y++) {
			var row = new TileData[w];
			for (int x = 0; x < w; x++) {
				row [x] = new TileData { y = y, x = x };
			}
			data [y] = row;
		}
		for (int tempY = 0; tempY < h; tempY++) {
			for (int tempX = 0; tempX < w; tempX++) {
				createButton (tempY, tempX);
			}
		}
		refreshColours ();
	}

	void createButton (int y, int x)
	{
		int rndNum = match3.get (y, x);
		var newBtn = Instantiate (proto, Vector3.zero, Quaternion.identity) as Button;
		var tile = data [y] [x];
		tile.btn = newBtn;
		tile.colour = rndNum;
		newBtn.image.color = colours [tile.colour];
		var btnObj = newBtn.gameObject;
		btnObj.name = "tile" + y + "," + x;
		btnObj.transform.SetParent (gameObject.transform);
		var trans = newBtn.transform as RectTransform;
		trans.localPosition = pos (x, y);
		trans.sizeDelta = Vector2.one * scale;
		newBtn.onClick.AddListener (() => {
			OnClick (y, x);
		});
	}

	void OnClick (int y, int x)
	{
		var tile = data [y] [x];
		if (!tile.on && first == null) {
			tile.btn.image.color = coloursHighlight[tile.colour];
			tile.on = true;
			first = tile;
			return;
		} 

		if (tile.on && first == tile) {
			tile.btn.image.color = colours[tile.colour];
			tile.on = false;
			first = null;
			return;
		}

		if (first != null && first != tile) {
			var dx = Mathf.Abs(first.x - tile.x);
			var dy = Mathf.Abs(first.y - tile.y);
			if(!(dx == 0 && dy == 1) && !(dx == 1 && dy == 0)) {
				text = "Can only swap adjacent tiles.";
				first.on = false;
				tile.on = false;
				first = null;
				refreshColours();
				return;
			} else {
				if(!match3.validSwap (first, tile)){
					text = "There is not match after swapping.";
					first.on = false;
					tile.on = false;
					first = null;
					refreshColours();
					return;
				}
			}

			match3.swap (first, tile);
			var temp = first.colour;
			first.colour = tile.colour;
			tile.colour = temp;

			hasChanges = true;

			first.on = false;
			tile.on = false;
			first = null;
		}
	}
	
	Vector3 pos (int x, int y)
	{
		return new Vector3 (x + 1, -y + 1, 0) * size + offset;
	}

	void refreshColours ()
	{
		for (int tempY = 0; tempY < h; tempY++) {
			for (int tempX = 0; tempX < w; tempX++) {
				var d = data [tempY] [tempX];
				var c = match3.get (tempY, tempX);
				d.colour = c;
				var arr = d.on ? coloursHighlight : colours;
				d.btn.image.color = arr [c];
				d.btn.image.transform.localScale = Vector3.one * 4f;
			}
		}
	}

	void deleteRefill ()
	{
		int newPoints = match3.deleteRefill ();
		points += newPoints;
		refreshColours ();
	}
}
