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

        // Логируем действие
        Console.WriteLine($"Canvas Mouse Left Button Down at {clickPosition}.");

        if (Keyboard.IsKeyDown(Key.LeftShift))
        {
            AddNode(clickPosition);
        }
        else if (selectedNode != null)
        {
            Node targetNode = GetNodeAtPosition(clickPosition);
            if (targetNode != null && targetNode != selectedNode)
            {
                InputBox inputBox = new InputBox("Enter edge weight:");
                if (inputBox.ShowDialog() == true)
                {
                    int weight = int.Parse(inputBox.Result);  // Преобразуем введённое значение в число
                    AddEdge(selectedNode, targetNode, weight);
                }
            }
            selectedNode = null;
        }
        else
        {
            selectedNode = GetNodeAtPosition(clickPosition);
            Console.WriteLine($"Node selected at {clickPosition}.");
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


    private void AddEdge(Node node1, Node node2, int weight)
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

        // Добавляем текст веса ребра
        TextBlock weightText = new TextBlock
        {
            Text = weight.ToString(),
            FontSize = 14,
            Background = Brushes.White
        };
        Canvas.SetLeft(weightText, (node1.Position.X + node2.Position.X) / 2);
        Canvas.SetTop(weightText, (node1.Position.Y + node2.Position.Y) / 2);
        GraphCanvas.Children.Add(weightText);

        // Сохраняем ребро
        edges.Add(new Edge { Node1 = node1, Node2 = node2, Line = line, Weight = weight, WeightText = weightText });

        // Обновляем смежный список
        int index1 = nodes.IndexOf(node1);
        int index2 = nodes.IndexOf(node2);

        if (!adjList.ContainsKey(index1))
            adjList[index1] = new List<Tuple<int, int>>();
        if (!adjList.ContainsKey(index2))
            adjList[index2] = new List<Tuple<int, int>>();

        adjList[index1].Add(new Tuple<int, int>(index2, weight));
        adjList[index2].Add(new Tuple<int, int>(index1, weight)); // Для неориентированного графа
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
        Console.WriteLine($"Deleting Node at {node.Position}.");
        foreach (var edge in edges.FindAll(e => e.Node1 == node || e.Node2 == node))
        {
            GraphCanvas.Children.Remove(edge.Line);      // Удаляем линию ребра
            GraphCanvas.Children.Remove(edge.WeightText); // Удаляем текст веса ребра
        }

        edges.RemoveAll(e => e.Node1 == node || e.Node2 == node);

        int index = nodes.IndexOf(node);
        if (adjList.ContainsKey(index))
        {
            adjList.Remove(index);
        }

        foreach (var key in adjList.Keys.ToList())
        {
            adjList[key].RemoveAll(e => e.Item1 == index);
        }

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

                // Логируем посещённую вершину
                string stepMessage = $"DFS: Visiting Node {vertex}.";
                StepsListBox.Items.Add(stepMessage); // Добавляем шаг в ListBox

                var neighbors = adjList[vertex];
                foreach (var neighbor in neighbors.OrderByDescending(n => n.Item2)) // По убыванию веса
                {
                    if (!visited.Contains(neighbor.Item1))
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

            // Логируем посещённую вершину
            string stepMessage = $"BFS: Visiting Node {vertex}.";
            StepsListBox.Items.Add(stepMessage); // Добавляем шаг в ListBox

            var neighbors = adjList[vertex];
            foreach (var neighbor in neighbors.OrderBy(n => n.Item2)) // По возрастанию веса
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

    
    public Dictionary<int, int> Dijkstra(int start, out Dictionary<int, int> previous)
    {
        var distances = new Dictionary<int, int>();
        var priorityQueue = new SortedSet<(int Distance, int Vertex)>();
        previous = new Dictionary<int, int>();

        foreach (var vertex in adjList.Keys)
        {
            distances[vertex] = int.MaxValue; // Инициализируем расстояния бесконечностью
            previous[vertex] = -1; // Нет предшественников
        }
        distances[start] = 0;
        priorityQueue.Add((0, start));

        while (priorityQueue.Count > 0)
        {
            var (currentDistance, currentVertex) = priorityQueue.Min;
            priorityQueue.Remove(priorityQueue.Min);

            if (currentDistance > distances[currentVertex])
                continue;

            foreach (var (neighbor, weight) in adjList[currentVertex])
            {
                int newDist = currentDistance + weight;
                if (newDist < distances[neighbor])
                {
                    priorityQueue.Remove((distances[neighbor], neighbor));
                    distances[neighbor] = newDist;
                    previous[neighbor] = currentVertex;
                    priorityQueue.Add((newDist, neighbor));
                }
            }
        }

        return distances;
    }

    private void ClearGraph()
    {
        // Удаляем все рёбра с Canvas
        foreach (var edge in edges)
        {
            GraphCanvas.Children.Remove(edge.Line);      // Удаляем линию ребра
            GraphCanvas.Children.Remove(edge.WeightText); // Удаляем текст веса ребра
        }

        // Удаляем все узлы с Canvas
        foreach (var node in nodes)
        {
            GraphCanvas.Children.Remove(node.Ellipse);   // Удаляем узел
        }

        // Очищаем список рёбер и узлов
        edges.Clear();
        nodes.Clear();
        adjList.Clear(); // Очистить смежный список
    }





    private async Task VisualizeDFS(List<int> visitedVertices)
    {
        foreach (var vertexIndex in visitedVertices)
        {
            var node = nodes[vertexIndex];
            node.Ellipse.Fill = Brushes.LightGreen;  // Цвет для посещённых вершин
            string stepMessage = $"DFS: Visiting Node {vertexIndex} at position {node.Position}.";
            Console.WriteLine(stepMessage);
            StepsListBox.Items.Add(stepMessage); // Добавляем шаг в ListBox
            await Task.Delay(500); // Задержка в 500 миллисекунд
        }
    }

    private async Task VisualizeBFS(List<int> visitedVertices)
    {
        foreach (var vertexIndex in visitedVertices)
        {
            var node = nodes[vertexIndex];
            node.Ellipse.Fill = Brushes.Yellow;  // Цвет для посещённых вершин
            string stepMessage = $"BFS: Visiting Node {vertexIndex} at position {node.Position}.";
            Console.WriteLine(stepMessage);
            StepsListBox.Items.Add(stepMessage); // Добавляем шаг в ListBox
            await Task.Delay(500); // Задержка в 500 миллисекунд
        }
    }
    
    private async Task VisualizeDijkstra(Dictionary<int, int> distances, Dictionary<int, int> previous, int start, int target)
    {
        // Визуализация всех вершин с рассчитанными расстояниями
        foreach (var nodeIndex in distances.Keys)
        {
            if (distances[nodeIndex] == int.MaxValue)
                continue;

            var node = nodes[nodeIndex];
            node.Ellipse.Fill = Brushes.LightBlue; // Цвет для всех посещённых вершин
            await Task.Delay(500);
        }

        // Визуализация кратчайшего пути
        int current = target;
        while (current != start && current != -1)
        {
            var node = nodes[current];
            node.Ellipse.Fill = Brushes.Red; // Цвет для вершин на кратчайшем пути
            current = previous[current];
            await Task.Delay(500);
        }

        // Выделяем стартовую вершину другим цветом
        nodes[start].Ellipse.Fill = Brushes.Green;
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
                writer.WriteLine($"Edge {index1} {index2} {edge.Weight}");
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
                    int weight = int.Parse(parts[3]);

                    // Передаём вес ребра
                    AddEdge(nodes[index1], nodes[index2], weight);
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
    
    private async void DijkstraButton_Click(object sender, RoutedEventArgs e)
    {
        if (nodes.Count > 0)
        {
            int startVertex = 0; // Начальная вершина
            int targetVertex = nodes.Count - 1; // Конечная вершина (можно выбрать другую)
        
            var distances = Dijkstra(startVertex, out var previous);
            await VisualizeDijkstra(distances, previous, startVertex, targetVertex);
        }
    }
    private void ClearGraphButton_Click(object sender, RoutedEventArgs e)
    {
        ClearGraph();
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
    public int Weight { get; set; } // Вес ребра
    public TextBlock WeightText { get; set; } // Добавляем ссылку на TextBlock для веса ребра
}


public class InputBox : Window
{
    private TextBox inputBox;
    public string Result { get; private set; }

    public InputBox(string prompt)
    {
        Title = "Input";
        Width = 300;
        Height = 150;

        StackPanel panel = new StackPanel { Margin = new Thickness(10) };
        panel.Children.Add(new TextBlock { Text = prompt, Margin = new Thickness(0, 0, 0, 10) });

        inputBox = new TextBox();
        panel.Children.Add(inputBox);

        Button okButton = new Button { Content = "OK", IsDefault = true, Width = 75, Margin = new Thickness(0, 10, 0, 0) };
        okButton.Click += (sender, e) => { Result = inputBox.Text; DialogResult = true; };
        panel.Children.Add(okButton);

        Content = panel;
    }
}
