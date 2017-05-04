using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtmosphericsMain : MonoBehaviour {
    public List<List<Tile>> Grid = new List<List<Tile>>();
    public int WorldSizeX, WorldSizeY;
    public float TileSize;
    [Range(0.0f, 1.0f)]
    public float rateOfTemperatureChange; // 1.0f is instant equalization between temperatures.
    public float planckTemperature;
    public float timePerAtmosTick;
    public float timeOfNextAtmosTick;

    public bool DrawTemperature;
	void Start () {
        timeOfNextAtmosTick = Time.time;
        Random.InitState((int) System.DateTime.Now.Ticks);
	    for(int i=0; i<WorldSizeY; i++) {
            Grid.Add(new List<Tile>());
            print("Created Row " + i);
            for(int j=0; j<WorldSizeX; j++) {
                Grid[i].Insert(j, new Tile());
                Grid[i][j].x = j;
                Grid[i][j].y = i;
                Grid[i][j].temperature = Random.Range(0.0f, 1000.0f);
                print("Created Tile("+j+", "+i+")");
            }
        }	
	}
	
	void Update () {
        if(Input.GetMouseButton(0)) {
            Tile tile = findClosestTile(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            tile.temperature = 1000.0f;
        }
        if(timeOfNextAtmosTick <= Time.time) {
            // To spread temperature, we have a sorted list of the temperatures of every tile
            // The tiles are spread in order and then discarded by checking the neighboring tiles of them and spreading heat
            // Tiles that are higher temperature than a neighboring tile should spread heat relative to the difference in temperature. 
            List<Tile> sortedList = SortTemperatures(Grid);
            // Now that we have a sorted list of temperatures, we want to start with the highest one and go downwards, comparing them to their neighbors, IE x+-1, y+-1 as well as x+-1&y+-1

            List<List<Tile>> newGrid = CopyGrid(Grid); ;
            for(int i = 0; i < sortedList.Count; i++) {
                Tile tile = Grid[sortedList[i].y][sortedList[i].x];
                float totalTempChange = 0.0f;
                //0 = x+1
                if(tile.x + 1 < Grid[tile.y].Count) {
                    Tile tile2 = Grid[tile.y][tile.x + 1];
                    if(tile.temperature > tile2.temperature) {
                        totalTempChange += equalizeTiles(tile, ref tile2);
                    }
                    newGrid[tile.y][tile.x + 1] = tile2;
                }
                //1 = x-1
                if(tile.x - 1 >= 0) {
                    Tile tile2 = Grid[tile.y][tile.x - 1];
                    if(tile.temperature > Grid[tile.y][tile.x - 1].temperature) {
                        totalTempChange += equalizeTiles(tile, ref tile2);
                    }
                    newGrid[tile.y][tile.x - 1] = tile2;
                }
                // 2 = y+1
                if(tile.y + 1 < Grid.Count) {
                    Tile tile2 = Grid[tile.y + 1][tile.x];
                    if(tile.temperature > Grid[tile.y + 1][tile.x].temperature) {
                        totalTempChange += equalizeTiles(tile, ref tile2);
                    }
                    newGrid[tile.y + 1][tile.x] = tile2;
                }
                //3 = y-1
                if(tile.y - 1 >= 0) {
                    Tile tile2 = Grid[tile.y - 1][tile.x];
                    if(tile.temperature > Grid[tile.y - 1][tile.x].temperature) {
                        totalTempChange += equalizeTiles(tile, ref tile2);
                    }
                    newGrid[tile.y - 1][tile.x] = tile2;
                }
                //4 = xy+1
                if(tile.x + 1 < Grid[tile.y].Count && tile.y + 1 < Grid.Count) {
                    Tile tile2 = Grid[tile.y + 1][tile.x + 1];
                    if(tile.temperature > Grid[tile.y + 1][tile.x + 1].temperature) {
                        totalTempChange += equalizeTiles(tile, ref tile2);
                    }
                    newGrid[tile.y + 1][tile.x + 1] = tile2;
                }
                //5 = x-1y+1
                if(tile.x - 1 >= 0 && tile.y + 1 < Grid.Count) {
                    Tile tile2 = Grid[tile.y + 1][tile.x - 1];
                    if(tile.temperature > Grid[tile.y + 1][tile.x - 1].temperature) {
                        totalTempChange += equalizeTiles(tile, ref tile2);
                    }
                    newGrid[tile.y + 1][tile.x - 1] = tile2;
                }
                //6 = xy-1
                if(tile.x - 1 >= 0 && tile.y - 1 >= 0) {
                    Tile tile2 = Grid[tile.y - 1][tile.x - 1];
                    if(tile.temperature > Grid[tile.y - 1][tile.x - 1].temperature) {
                        totalTempChange += equalizeTiles(tile, ref tile2);
                    }
                    newGrid[tile.y - 1][tile.x - 1] = tile2;
                }
                //7 = x+1y-1
                if(tile.x + 1 < Grid[tile.y].Count && tile.y - 1 >= 0) {
                    Tile tile2 = Grid[tile.y - 1][tile.x + 1];
                    if(tile.temperature > Grid[tile.y - 1][tile.x + 1].temperature) {
                        totalTempChange += equalizeTiles(tile, ref tile2);
                    }
                    newGrid[tile.y - 1][tile.x + 1] = tile2;
                }
                tile.temperature -= totalTempChange;
            }
            Grid = newGrid;
            timeOfNextAtmosTick = Time.time + timePerAtmosTick;
        }
    }
    public Tile findClosestTile(Vector3 position) {
        //We'll do this by taking the position and first finding the closest y-axis and then the closest tile on that axis by just comparing it to the mouse position. 
        Tile bestMatch = null;
        float bestDistanceMatch = Vector3.Distance(new Vector3(Grid[0][0].x*TileSize, 0.0f, Grid[0][0].y*TileSize), position);

        foreach(List<Tile> y in Grid) {
            foreach(Tile x in y) {
                float testDistance = Vector3.Distance(new Vector3(x.x*TileSize, 0.0f, x.y*TileSize), position);
                if(testDistance < bestDistanceMatch) {
                    bestDistanceMatch = testDistance;
                    bestMatch = x;
                }
            }
        }

        return bestMatch;
    }

    public float equalizeTiles(Tile x, ref Tile y) {
        if(x.temperature > y.temperature) { // Neighbor is hotter, equalize by rate of heat transfer
            //float temperatureToExchange = (x.temperature - y.temperature) * rateOfTemperatureChange;
            float temperatureToExchange = rateOfTemperatureChange;
            if(temperatureToExchange < planckTemperature) {
                y.temperature = x.temperature;
            }
            y.temperature += temperatureToExchange;
            //            x.temperature -= temperatureToExchange;
            return temperatureToExchange;
        }
        print("shit is fucked yo");
        return 0.0f;
    }

    public List<List<Tile>> CopyGrid(List<List<Tile>> grid) {
        List<List<Tile>> newGrid = new List<List<Tile>>();
        for(int i=0; i<grid.Count; i++) {
            newGrid.Add(new List<Tile>());
            for(int j=0; j<grid[i].Count; j++) {
                newGrid[i].Add(new Tile());
                newGrid[i][j].x = grid[i][j].x;
                newGrid[i][j].y = grid[i][j].y;
                newGrid[i][j].temperature = grid[i][j].temperature;
            }
        }
        return newGrid;
    }

    public void OnDrawGizmos() {
        if(DrawTemperature) {
            foreach(List<Tile> y in Grid) {
                foreach(Tile x in y) {
                    if(x.temperature >= 273.0f) {
                        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, Mathf.InverseLerp(273.0f, 1000.0f, x.temperature));
                        Gizmos.DrawCube(new Vector3(x.x * TileSize, 0.0f, x.y * TileSize), new Vector3(TileSize, TileSize, TileSize));
                    }
                    else if(x.temperature < 273.0f) {
                        Gizmos.color = new Color(0.0f, 0.0f, 1.0f, Mathf.InverseLerp(273.0f, 0.0f, x.temperature));
                        Gizmos.DrawCube(new Vector3(x.x * TileSize, 0.0f, x.y * TileSize), new Vector3(TileSize, TileSize, TileSize));
                    }
                }
            }
        } 
    }

    public List<Tile> SortTemperatures(List<List<Tile>> grid) {
        List<Tile> unsortedList = new List<Tile>();
        foreach(List<Tile> y in grid) {
            foreach(Tile x in y) {
                unsortedList.Add(x);
            }
        }
        unsortedList.Sort(delegate (Tile x, Tile y) {
            return y.temperature.CompareTo(x.temperature); 
        }); 

        return unsortedList;
    }
}

public class Tile {
    public int x { get; set; }
    public int y { get; set; }
    public float temperature { get; set; }
}