using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

class Vector
{
    public int x { get; set; }
    public int y { get; set; }

    public Vector(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static Vector operator +(Vector a, Vector b)
        => new Vector(a.x + b.x, a.y + b.y);

    public static Vector operator -(Vector a, Vector b)
       => new Vector(a.x - b.x, a.y - b.y);

    public static Vector operator *(Vector a, Vector b)
       => new Vector(a.x * b.x, a.y * b.y);

    public static Vector operator *(Vector a, int b)
       => new Vector(a.x * b, a.y * b);

    public double GetMagnitude()
    {
        return Math.Sqrt(Math.Pow(this.x, 2) + Math.Pow(this.y, 2));
    }
}
class Node
{
    public Vector Current { get; set;}
    public Node Previous { get; set; }
    public int cheapest { get; set; } = int.MaxValue;
    public int CostScore { get; set; } = 0;
    public int DistanceScore { get; set; } = 0;
    public int CostDistanceScore => CostScore + DistanceScore;


    public Node(int x, int y, bool start = false, bool goal = false, bool solid = false, Node Parent = null)
    {
        this.Current = new Vector(x, y);

        if (start)
            this.cheapest = 0;
        if(Parent != null)
            Previous = Parent;

    }

    public void SetDistance(Node target)
    {
        this.DistanceScore = Math.Abs(target.Current.x - this.Current.x) + Math.Abs(target.Current.y - this.Current.y);
    }

}

namespace A_Star
{
    class Program
    {

        static void Main(string[] args)
        {
            Random r = new Random();

            bool Visual;

            List<string> map = new List<string>
            {
                "A     |   |         ",
                "      | | |    B    ",
                "--- --| | |         ",
                "      | | |         ",
                "----- | | |         ",
                "      | | |         ",
                "        |           "
            };

            Console.Write("Enable Visualisation? (true or false) :> ");
            Visual = bool.Parse(Console.ReadLine());

            DateTime Initial = DateTime.Now;

            /*
            Console.Write("Map Height :> "); int MapHeight = int.Parse(Console.ReadLine());
            Console.WriteLine("Map Must have An A (Start) on a Row And a B (Goal) on Another, \na Blank Space is fine and anything else will be taken as a wall,");
            Console.WriteLine("Every Row MUST be the same length");

            for (int i = 0; i < MapHeight; i++)
            {
                Console.Write($"Row {i + 1} :> ");
                if (i < map.Count - 1)
                    map[i] = Console.ReadLine();
                else
                    map.Add(Console.ReadLine());
            }

            while (map.Count > MapHeight)
            {
                map.RemoveAt(map.Count - 1);
            }
            */

            int Size = r.Next(5, 30);
            bool a = false;
            bool b = false;
            char[] possiblechar = new char[5] { ' ', '|', ' ', '|', 'A' };
            char[] possiblecharB = new char[5] { ' ', '|', ' ', '|',  'B' };

            for (int x = 0; x < Size; x++)
            {
                List<char> Line = new List<char>();
                for (int y = 0; y < Size; y++)
                {
                    if (a && !b && x > Size /2)
                    {
                        char Choice = possiblecharB[Math.Abs(r.Next(0, 50)/10)];
                        if (Choice == 'B') b = true;
                        Line.Add(Choice);

                    }
                    else if (!a)
                    {
                        char Choice = possiblechar[Math.Abs(r.Next(0, 50) / 10)];
                        if (Choice == 'A') a = true;
                        Line.Add(Choice);
                    }
                    else
                    {
                        char Choice = possiblechar[r.Next(0, 3)];
                        Line.Add(Choice);
                    }
                }

                if (x < map.Count - 1)
                    map[x] = String.Join(' ', Line.ToArray());
                else
                    map.Add(String.Join(' ', Line.ToArray()));
            }

            while (map.Count > Size)
            {
                map.RemoveAt(map.Count - 1);
            }

            Vector Start = new Vector(map[map.FindIndex(x => x.Contains("A"))].IndexOf("A"), map.FindIndex(x => x.Contains("A"))); //Finds start in map
            Vector Goal = new Vector(map[map.FindIndex(x => x.Contains("B"))].IndexOf("B"), map.FindIndex(x => x.Contains("B")));  //Finds goal in map

            Node StartNode = new Node(Start.x, Start.y, true);     //Sets start and goal Nodes using vectors above
            Node GoalNode = new Node(Goal.x, Goal.y, false, true);

            StartNode.SetDistance(GoalNode); //Sets initial distance

            List<Node> OpenNodes = new List<Node>();
            OpenNodes.Add(StartNode);                   //Adds start node to open list
            List<Node> VisitedNodes = new List<Node>();

            while (OpenNodes.Any()) //Runs while there are any items in List Opennodes
            {

                var CheckNode = OpenNodes.OrderBy(Node => Node.CostDistanceScore).First(); //Get Next node to check based on distance score

                if(CheckNode.Current.x == GoalNode.Current.x && CheckNode.Current.y == GoalNode.Current.y) //Check if it is goal node
                {
                    Console.WriteLine("We are at our destination");

                    //Loop and draw map and tiles

                    var Node = CheckNode;
                    Console.WriteLine("Retracing Steps");
                    while (true)
                    {
                        Console.WriteLine($"{Node.Current.x} : {Node.Current.y}");
                        if(map[Node.Current.y][Node.Current.x] == ' ' || map[Node.Current.y][Node.Current.x] == 'X') //Plan to show Path and checked here
                        {

                            var newMapRow = map[Node.Current.y].ToCharArray();
                            newMapRow[Node.Current.x] = '*';
                            map[Node.Current.y] = new string(newMapRow);

                        }
                        Node = Node.Previous;
                        if(Node == null)
                        {
                            Console.WriteLine("Path Found:");
                            DrawColouredMap(map);
                            DateTime End = DateTime.Now;
                            Console.WriteLine("Done");
                            Console.WriteLine("Simulation Of Grid Size {0} Took {1} Seconds With Visualisation {2}", Size, End - Initial, Visual ? "On" : "Off");
                            Console.ReadLine();
                            return;
                        }
                    }

                    return; //End
                }

                VisitedNodes.Add(CheckNode); //Adds node to visited
                OpenNodes.Remove(CheckNode); //Removes from needs top be checked

                if(Visual) DrawCurrentChecked(map, VisitedNodes, OpenNodes);  //Visualises map in console

                var NextNodes = GetNeigbourNodes(map, CheckNode, GoalNode); //Gets Neighbour nodes

                foreach (var NeighbourNode in NextNodes)
                {
                    if(VisitedNodes.Any(Node => Node.Current.x == NeighbourNode.Current.x && Node.Current.y == NeighbourNode.Current.y)) //Sees if any node in visited is the one were on now, if so we dont want ot do it again
                    {
                        continue;
                    }

                    if(OpenNodes.Any(Node => Node.Current.x == NeighbourNode.Current.x && Node.Current.y == NeighbourNode.Current.y)) //sees if node is open but not visited
                    {
                        var ExistingNode = OpenNodes.First(Node => Node.Current.x == NeighbourNode.Current.x && Node.Current.y == NeighbourNode.Current.y); //Gets first node we've been to 

                        if(ExistingNode.CostDistanceScore > CheckNode.CostDistanceScore)
                        {
                            OpenNodes.Remove(ExistingNode);
                            VisitedNodes.Add(NeighbourNode);
                        }

                    }
                    else
                    {
                        OpenNodes.Add(NeighbourNode);
                    }
                }

            }

            Console.WriteLine("No Path Found!");

            foreach (var Node in VisitedNodes)
            {
                if (map[Node.Current.y][Node.Current.x] == ' ')
                {

                    var newMapRow = map[Node.Current.y].ToCharArray();
                    newMapRow[Node.Current.x] = 'X';
                    map[Node.Current.y] = new string(newMapRow);
                }
            }

            map.ForEach(Line => Console.WriteLine(Line));
            Console.WriteLine("Done, Failed");
            Console.ReadLine();


        }

        private static List<Node> GetNeigbourNodes(List<string> map, Node CurrentNode, Node TargetNode)
        {

            List<Node> PossibleNodes = new List<Node>()
            {
                new Node(CurrentNode.Current.x,CurrentNode.Current.y - 1, false, false, false, CurrentNode),
                new Node(CurrentNode.Current.x,CurrentNode.Current.y + 1, false, false, false, CurrentNode),   //generates 4 neighbouring tiles arround our current tile
                new Node(CurrentNode.Current.x - 1 ,CurrentNode.Current.y, false, false, false, CurrentNode),
                new Node(CurrentNode.Current.x + 1,CurrentNode.Current.y, false, false, false, CurrentNode),
            };

            PossibleNodes.ForEach(Node => Node.SetDistance(TargetNode)); //Sets distance for each node in list
            PossibleNodes.ForEach(Node => Node.CostScore = CurrentNode.CostScore + 1); //Sets CostScore for all nodes

            var maxX = map.Count - 1;//(map[0].Length - 1);  //Finds bounds of map 
            var maxY = map.Count - 1;

            return PossibleNodes
                .Where(Node => Node.Current.x >= 0 && Node.Current.x <= maxX)
                .Where(Node => Node.Current.y >= 0 && Node.Current.y <= maxY) //Checks Tiles Are Within Bounds
                .Where(Node => map[Node.Current.y][Node.Current.x] == ' ' || map[Node.Current.y][Node.Current.x] == 'B') // Checks Tiles Are spaces or the goal
                .ToList();

        }

        static void DrawCurrentChecked(List<string> map, List<Node> Checked, List<Node> OpenNodes)
        {
            Console.Clear();
            List<string> CheckedMap = new List<string>();
            foreach (var item in map)
            {
                CheckedMap.Add(item);
            }


            foreach (var Node in Checked)
            {
                var newMapRow = CheckedMap[Node.Current.y].ToCharArray();

                if(newMapRow[Node.Current.x] != 'A') newMapRow[Node.Current.x] = 'X';
                //if(newMapRow[Node.Current.x] != 'A' && !OpenNodes.Any(Node2 => Node2.Current.x == Node.Current.x && Node2.Current.y == Node.Current.y)) newMapRow[Node.Current.x] = 'K';
                else newMapRow[Node.Current.x] = 'A';
                CheckedMap[Node.Current.y] = new string(newMapRow);
            }

            DrawColouredMap(CheckedMap);
            Thread.Sleep(5);
        }

        static void DrawColouredMap(List<string> map)
        {

            var DefautColour = Console.BackgroundColor;

            foreach(var Line in map)
            {
                foreach (var Char in Line)
                {
                    switch (Char)
                    {
                        case 'B':
                            Console.BackgroundColor = ConsoleColor.DarkMagenta;
                            Console.Write(' ');
                            break;
                        case ' ':
                            Console.BackgroundColor = DefautColour;
                            Console.Write(' ');
                            break;
                        case 'X':
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.Write(' ');
                            break;
                        case 'K':
                            Console.BackgroundColor = ConsoleColor.Magenta;
                            Console.Write(' ');
                            break;
                        case '*':
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.Write(' ');
                            break;
                        case '|':
                            Console.BackgroundColor = ConsoleColor.DarkCyan;
                            Console.Write(' ');
                            break;
                    }
                }
                Console.WriteLine();
            }
            Console.BackgroundColor = DefautColour;
        }

    }
}
