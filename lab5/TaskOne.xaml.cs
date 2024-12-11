using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace lab5;

public partial class TaskOne : Window
{
    private Dictionary<int, List<Tuple<int, int>>> adjList = new Dictionary<int, List<Tuple<int, int>>>();
    private List<Node> nodes = new List<Node>();
    private List<Edge> edges = new List<Edge>();
    private Node selectedNode;
    
    public TaskOne()
    {
        InitializeComponent();
    }

    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Point clickPosition = e.GetPosition(GraphCanvas);

        if (Keyboard.IsKeyDown(Key.LeftShift))
        {
            AddNode(clickPosition);
        }
        else if (selectedNode != null)
        {
            Node targetNode = GetNodeAtPosition(clickPosition);
            if (targetNode != null && targetNode != selectedNode)
            {
                AddEdge(selectedNode, targetNode);
            }
            selectedNode = null;
        }
        else
        {
            selectedNode = GetNodeAtPosition(clickPosition);
        }
    }

    private void AddNode(Point position)
    {
        Node newNode = new Node { Position = position };
        nodes.Add(newNode);

        Ellipse ellipse = new Ellipse
        {
            Width = 30,
            Height = 30,
            Fill = Brushes.LightBlue,
            Stroke = Brushes.Black,
            StrokeThickness = 2
        };

        Canvas.SetLeft(ellipse, position.X - 15);
        Canvas.SetTop(ellipse, position.Y - 15);
        GraphCanvas.Children.Add(ellipse);

        newNode.Ellipse = ellipse;
    }


    private void AddEdge(Node node1, Node node2)
    {
        // Добавляем линию на Canvas
        Line line = new Line
        {
            X1 = node1.Position.X,
            Y1 = node1.Position.Y,
            X2 = node2.Position.X,
            Y2 = node2.Position.Y,
            Stroke = Brushes.Black,
            StrokeThickness = 2
        };

        GraphCanvas.Children.Add(line);
        edges.Add(new Edge { Node1 = node1, Node2 = node2, Line = line });

        // Обновляем смежный список для обеих вершин
        int index1 = nodes.IndexOf(node1);
        int index2 = nodes.IndexOf(node2);

        if (!adjList.ContainsKey(index1))
            adjList[index1] = new List<Tuple<int, int>>();
        if (!adjList.ContainsKey(index2))
            adjList[index2] = new List<Tuple<int, int>>();

        // Здесь мы добавляем соседа в список
        adjList[index1].Add(new Tuple<int, int>(index2, 0)); // 0 — вес, если он не используется
        adjList[index2].Add(new Tuple<int, int>(index1, 0)); // добавляем обратно для неориентированного графа
    }


    private Node GetNodeAtPosition(Point position)
    {
        foreach (var node in nodes)
        {
            double dx = position.X - node.Position.X;
            double dy = position.Y - node.Position.Y;
            if (Math.Sqrt(dx * dx + dy * dy) <= 15)
            {
                return node;
            }
        }
        return null;
    }

    private void DeleteNode(Node node)
    {
        foreach (var edge in edges.FindAll(e => e.Node1 == node || e.Node2 == node))
        {
            GraphCanvas.Children.Remove(edge.Line);
        }
        edges.RemoveAll(e => e.Node1 == node || e.Node2 == node);

        GraphCanvas.Children.Remove(node.Ellipse);
        nodes.Remove(node);
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && selectedNode != null)
        {
            DeleteNode(selectedNode);
            selectedNode = null;
        }
    }
    
    public List<int> DFS(int start)
    {
        var visited = new HashSet<int>();
        var result = new List<int>();
        Stack<int> stack = new Stack<int>();
        stack.Push(start);

        while (stack.Count > 0)
        {
            int vertex = stack.Pop();
            if (!visited.Contains(vertex))
            {
                visited.Add(vertex);
                result.Add(vertex);

                foreach (var neighbor in adjList[vertex])
                {
                    if (!visited.Contains(neighbor.Item1)) // Используем Item1 для доступа к индексу соседа
                    {
                        stack.Push(neighbor.Item1);
                    }
                }
            }
        }

        return result;
    }


    public List<int> BFS(int start)
    {
        var visited = new HashSet<int>();
        var result = new List<int>();
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            int vertex = queue.Dequeue();
            result.Add(vertex);

            foreach (var neighbor in adjList[vertex])
            {
                if (!visited.Contains(neighbor.Item1))
                {
                    visited.Add(neighbor.Item1);
                    queue.Enqueue(neighbor.Item1);
                }
            }
        }

        return result;
    }


    private async Task VisualizeDFS(List<int> visitedVertices)
    {
        foreach (var vertexIndex in visitedVertices)
        {
            var node = nodes[vertexIndex];
            node.Ellipse.Fill = Brushes.LightGreen;  // Цвет для посещённых вершин
            await Task.Delay(500); // Задержка в 500 миллисекунд
        }
    }

    private async Task VisualizeBFS(List<int> visitedVertices)
    {
        foreach (var vertexIndex in visitedVertices)
        {
            var node = nodes[vertexIndex];
            node.Ellipse.Fill = Brushes.Yellow;  // Цвет для посещённых вершин
            await Task.Delay(500); // Задержка в 500 миллисекунд
        }
    }

    
    private void SaveGraph(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var node in nodes)
            {
                writer.WriteLine($"Node {node.Position.X} {node.Position.Y}");
            }
            foreach (var edge in edges)
            {
                int index1 = nodes.IndexOf(edge.Node1);
                int index2 = nodes.IndexOf(edge.Node2);
                writer.WriteLine($"Edge {index1} {index2}");
            }
        }
    }
    
    private void LoadGraph(string filePath)
    {
        nodes.Clear();
        edges.Clear();
        GraphCanvas.Children.Clear();

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(' ');
                if (parts[0] == "Node")
                {
                    Point position = new Point(double.Parse(parts[1]), double.Parse(parts[2]));
                    AddNode(position);
                }
                else if (parts[0] == "Edge")
                {
                    int index1 = int.Parse(parts[1]);
                    int index2 = int.Parse(parts[2]);
                    AddEdge(nodes[index1], nodes[index2]);
                }
            }
        }
    }
    
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = "Graph Files (*.graph)|*.graph|All Files (*.*)|*.*"
        };
        if (saveFileDialog.ShowDialog() == true)
        {
            SaveGraph(saveFileDialog.FileName);
        }
    }

    private void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "Graph Files (*.graph)|*.graph|All Files (*.*)|*.*"
        };
        if (openFileDialog.ShowDialog() == true)
        {
            LoadGraph(openFileDialog.FileName);
        }
    }
    
    private async void BFSButton_Click(object sender, RoutedEventArgs e)
    {
        if (nodes.Count > 0)
        {
            int startVertex = 0;  // Здесь можно выбрать стартовую вершину, например, первую
            var visitedVertices = BFS(startVertex);
            await VisualizeBFS(visitedVertices); // Ожидание завершения анимации BFS
        }
    }

    private async void DFSButton_Click(object sender, RoutedEventArgs e)
    {
        if (nodes.Count > 0)
        {
            int startVertex = 0;  // Здесь можно выбрать стартовую вершину, например, первую
            var visitedVertices = DFS(startVertex);
            await VisualizeDFS(visitedVertices); // Ожидание завершения анимации DFS
        }
    }
}

public class Node
{
    public Point Position { get; set; }
    public Ellipse Ellipse { get; set; }
}

public class Edge
{
    public Node Node1 { get; set; }
    public Node Node2 { get; set; }
    public Line Line { get; set; }
}
