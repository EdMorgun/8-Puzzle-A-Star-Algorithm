using System;
using System.Collections.Generic;
using System.Linq;
using static System.DateTime;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main()
        {

            int[] example =   
            {
                4, 0, 3,
                7, 1, 6,
                5, 2, 8,
            };

            var mainNode1 = new Node(example);
            var aStarSearch = new AStarSearch();
            aStarSearch.Search(mainNode1);
        }
    }

    public class Node
    {
        public readonly int[] Goal =
        {
            1, 2, 3,
            4, 5, 6,
            7, 8, 0
        };

        public List<Node> Children = new List<Node>();
        public Node Parent;
        public int[] Puzzle = new int[9];
        public int X;
        public int Col = 3;
        public int Score;

        public Node(int[] p)
        {
            SetPuzzle(p);
        }

        public void SetPuzzle(int[] p)
        {
            p.CopyTo(Puzzle, 0);
        }

        public void MoveRight(int[] p, int i)
        {
            if (i % Col >= Col - 1) return;

            var pc = new int[9];
            p.CopyTo(pc, 0);

            pc[i + 1] = p[i];
            pc[i] = p[i + 1];

            var child = new Node(pc);
            Children.Add(child);
            child.Parent = this;
        }

        public void MoveLeft(int[] p, int i)
        {
            if (i % Col <= 0) return;

            var pc = new int[9];
            p.CopyTo(pc, 0);

            pc[i - 1] = p[i];
            pc[i] = p[i - 1];

            var child = new Node(pc);
            Children.Add(child);
            child.Parent = this;
        }

        public void MoveUp(int[] p, int i)
        {
            if (i - Col < 0) return;

            var pc = new int[9];
            p.CopyTo(pc, 0);

            pc[i - Col] = p[i];
            pc[i] = p[i - Col];

            var child = new Node(pc);
            Children.Add(child);
            child.Parent = this;
        }

        public void MoveDown(int[] p, int i)
        {
            if (i + Col >= p.Length) return;
            
            var pc = new int[9];
            p.CopyTo(pc, 0);

            pc[i + Col] = p[i];
            pc[i] = p[i + Col];

            var child = new Node(pc);
            Children.Add(child);
            child.Parent = this;
        }

        public bool IsEqual(int[] p)
        {
            var equal = true;
            for (var i = 0; i < p.Length; i++)
                if (p[i] != Puzzle[i])
                    equal = false;

            return equal;
        }

        public void ExpandNode()
        {
            for (var i = 0; i < Puzzle.Length; i++)
                if (Puzzle[i] == 0)
                    X = i;

            MoveRight(Puzzle, X);
            MoveLeft(Puzzle, X);
            MoveUp(Puzzle, X);
            MoveDown(Puzzle, X);
        }

        public bool GoalTest()
        {
            var isGoal = true;

            for (var i = 0; i < Puzzle.Length; i++)
                if (Puzzle[i] != Goal[i]) isGoal = false;

            return isGoal;
        }

        public void Manhattan()
        {
            var distance = 0;
            var vectors = new int[3, 3];
            var vectorGoal = new int[3, 3];

            Buffer.BlockCopy(Puzzle, 0, vectors, 0, sizeof(int) * Puzzle.Length);
            Buffer.BlockCopy(Goal, 0, vectorGoal, 0, sizeof(int) * Goal.Length);

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    if (vectors[i, j] == 0) continue;
                    for (var k = 0; k < 3; k++)
                    {
                        for (var l = 0; l < 3; l++)
                            if (vectorGoal[i, j] == vectors[k, l])
                                distance += Math.Abs(i - k) + Math.Abs(j - l);
                    }
                }
            }

            Score += distance;
        }
    }

    public class AStarSearch
    {
        private const long MaxTime = 60 * 10000000; // 10 000 000 ticks = 1 second 
        private const long MaxNodesCount = 50000;
        private int _createdNodes;
        private long _timeAtStart;
        private double _finalTime;

        public bool Search(Node root)
        {
            _createdNodes = 0;

            _timeAtStart = Now.Ticks;

            var openList = new List<Node>();
            var closedList = new List<Node>();

            root.Manhattan();
            openList.Add(root);
            var goalFound = false;

            while (openList.Count > 0 && Now.Ticks - _timeAtStart < MaxTime 
                                      && openList.Count + closedList.Count < MaxNodesCount)
            {
                var currentNode = FindNodeWithMinScore(openList);
                if (currentNode.GoalTest())
                {
                    goalFound = true;
                    break;
                }

                currentNode.ExpandNode();

                foreach (var currentChild in currentNode.Children.Where(currentChild => !Contains(openList, currentChild) 
                    && !Contains(closedList, currentChild)))
                {
                    currentChild.Score = currentNode.Score;
                    currentChild.Manhattan();
                    openList.Add(currentChild);
                    _createdNodes++;
                }

                closedList.Add(currentNode);
                openList.Remove(currentNode);
            }

            _finalTime = (double) (Now.Ticks - _timeAtStart) / 10000000;
            
            if (goalFound) Console.WriteLine("ASTAR has found the answer!\nTime: " 
                                             + _finalTime + "; Created Nodes: " + _createdNodes);
            else  Console.WriteLine("The answer hasn't found. Time: " + _finalTime);
            
            return goalFound;
        }

        public bool Contains(List<Node> list, Node node) => list.Any(n => n.IsEqual(node.Puzzle));

        public Node FindNodeWithMinScore(List<Node> list)
        {
            var minScore = Int32.MaxValue;
            Node minNode = null;
            foreach (var node in list.Where(node => node.Score < minScore))
            {
                minNode = node;
                minScore = node.Score;
            }
            return minNode;
        }
    }
}
