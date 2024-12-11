using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace lab5
{
    /// <summary>
    /// Логика взаимодействия для TaskThreeFourWindow.xaml
    /// </summary>
    public partial class TaskThreeFourWindow : Window
    {
        private readonly List<(Ellipse, TextBlock)> _nodes = new(); // Узлы: эллипс и текст внутри
        private readonly List<(Line, TextBlock, int)> _edges = new(); // Ребра: линия, текст веса и вес

        private bool _isAddingNode = false;
        private Ellipse _selectedNode;

        public TaskThreeFourWindow()
        {
            InitializeComponent();
        }

        private void AddNode_Click(object sender, RoutedEventArgs e)
        {
            _isAddingNode = true;
            LogBox.Text += "Режим добавления узла: кликните на канвасе, чтобы создать узел.\n";
        }

        private void RemoveNode_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNode == null)
            {
                LogBox.Text += "Ошибка: не выбран узел для удаления.\n";
                return;
            }

            // Удалить ребра, связанные с узлом
            var relatedEdges = _edges.Where(edge =>
                edge.Item1.X1 == GetNodeCenter(_selectedNode).X && edge.Item1.Y1 == GetNodeCenter(_selectedNode).Y ||
                edge.Item1.X2 == GetNodeCenter(_selectedNode).X && edge.Item1.Y2 == GetNodeCenter(_selectedNode).Y).ToList();

            foreach (var edge in relatedEdges)
            {
                GraphCanvas.Children.Remove(edge.Item1);
                GraphCanvas.Children.Remove(edge.Item2);
                _edges.Remove(edge);
            }

            // Удалить узел и текст
            var nodeText = _nodes.FirstOrDefault(n => n.Item1 == _selectedNode).Item2;
            GraphCanvas.Children.Remove(_selectedNode);
            GraphCanvas.Children.Remove(nodeText);
            _nodes.RemoveAll(n => n.Item1 == _selectedNode);

            LogBox.Text += "Узел удален.\n";
            _selectedNode = null;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isAddingNode)
            {
                var point = e.GetPosition(GraphCanvas);
                AddNode(point);
                _isAddingNode = false;
                LogBox.Text += $"Узел добавлен в точке ({point.X}, {point.Y}).\n";
            }
            else
            {
                SelectNode(e.GetPosition(GraphCanvas));
            }
        }

        private void AddNode(Point point)
        {
            var node = new Ellipse
            {
                Width = 30,
                Height = 30,
                Fill = Brushes.Black
            };

            Canvas.SetLeft(node, point.X - 15);
            Canvas.SetTop(node, point.Y - 15);

            GraphCanvas.Children.Add(node);

            var nodeText = new TextBlock
            {
                Text = ($"{_nodes.Count + 1}"),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Canvas.SetLeft(nodeText, point.X - 10);
            Canvas.SetTop(nodeText, point.Y - 10);
            GraphCanvas.Children.Add(nodeText);

            _nodes.Add((node, nodeText));
        }

        private void SelectNode(Point clickPosition)
        {
            _selectedNode = null;

            foreach (var (node, _) in _nodes)
            {
                var nodeCenter = GetNodeCenter(node);
                var distance = Math.Sqrt(Math.Pow(nodeCenter.X - clickPosition.X, 2) +
                                         Math.Pow(nodeCenter.Y - clickPosition.Y, 2));

                if (distance <= node.Width / 2)
                {
                    _selectedNode = node;
                    HighlightNode(node);
                    LogBox.Text += $"Узел выбран: {GetNodeLabel(node)}.\n";
                    return;
                }
            }

            LogBox.Text += "Узел не выбран (щелчок вне узлов).\n";
        }

        private void HighlightNode(Ellipse node)
        {
            foreach (var (n, _) in _nodes)
                n.Stroke = Brushes.Transparent;

            node.Stroke = Brushes.Red;
            node.StrokeThickness = 2;
        }

        private string GetNodeLabel(Ellipse node)
        {
            var nodeText = _nodes.FirstOrDefault(n => n.Item1 == node).Item2;
            return nodeText.Text;
        }

        private Point GetNodeCenter(Ellipse node)
        {
            var x = Canvas.GetLeft(node) + node.Width / 2;
            var y = Canvas.GetTop(node) + node.Height / 2;
            return new Point(x, y);
        }

        /*private void AddEdge_Click(object sender, RoutedEventArgs e)
        {
            EdgeInputPanel.Visibility = Visibility.Visible;
            LogBox.Text += "Введите начальную и конечную вершины, а также вес ребра.\n";
        }

        private void AddEdge(Ellipse startNode, Ellipse endNode, int weight)
        {
            var line = new Line
            {
                X1 = GetNodeCenter(startNode).X,
                Y1 = GetNodeCenter(startNode).Y,
                X2 = GetNodeCenter(endNode).X,
                Y2 = GetNodeCenter(endNode).Y,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            GraphCanvas.Children.Add(line);

            var weightLabel = new TextBlock
            {
                Text = weight.ToString(),
                Foreground = Brushes.Black
            };

            double midX = (line.X1 + line.X2) / 2;
            double midY = (line.Y1 + line.Y2) / 2;

            Canvas.SetLeft(weightLabel, midX);
            Canvas.SetTop(weightLabel, midY);

            GraphCanvas.Children.Add(weightLabel);
            _edges.Add((line, weightLabel, weight));
        }*/

        private Ellipse FindNodeByName(string name)
        {
            return _nodes.FirstOrDefault(n => n.Item2.Text == name).Item1;
        }

        private void ClearGraph_Click(object sender, RoutedEventArgs e)
        {
            foreach (var (node, label) in _nodes)
            {
                GraphCanvas.Children.Remove(node);
                GraphCanvas.Children.Remove(label);
            }

            foreach (var (edge, weightLabel, _) in _edges)
            {
                GraphCanvas.Children.Remove(edge);
                GraphCanvas.Children.Remove(weightLabel);
            }

            _nodes.Clear();
            _edges.Clear();

            LogBox.Text += "Граф полностью очищен.\n";
        }

        private void ClearLogBox_Click(object sender, RoutedEventArgs e)
        {
            LogBox.Clear();
        }

        private void FindMinimumSpanningTree_Click(object sender, RoutedEventArgs e)
        {
            if (_nodes.Count == 0 || _edges.Count == 0)
            {
                LogBox.Text += "Ошибка: Граф пуст, невозможно найти МОД.\n";
                return;
            }

            var mstEdges = new List<(Line, TextBlock, int)>();
            var visitedNodes = new HashSet<Ellipse>();
            var priorityQueue = new SortedSet<(int, Ellipse, Ellipse)>();

            var startNode = _nodes.First().Item1;
            visitedNodes.Add(startNode);

            foreach (var edge in _edges)
            {
                if (edge.Item1.X1 == GetNodeCenter(startNode).X && edge.Item1.Y1 == GetNodeCenter(startNode).Y)
                {
                    var endNode = FindNodeAtPosition(edge.Item1.X2, edge.Item1.Y2);
                    priorityQueue.Add((edge.Item3, startNode, endNode));
                }
            }

            while (priorityQueue.Count > 0)
            {
                var (weight, start, end) = priorityQueue.First();
                priorityQueue.Remove(priorityQueue.First());

                if (visitedNodes.Contains(end)) continue;

                visitedNodes.Add(end);
                mstEdges.Add(_edges.First(e => e.Item3 == weight && e.Item1.X1 == GetNodeCenter(start).X && e.Item1.Y1 == GetNodeCenter(start).Y));

                foreach (var edge in _edges)
                {
                    if (edge.Item1.X1 == GetNodeCenter(end).X && edge.Item1.Y1 == GetNodeCenter(end).Y && !visitedNodes.Contains(FindNodeAtPosition(edge.Item1.X2, edge.Item1.Y2)))
                    {
                        var nextNode = FindNodeAtPosition(edge.Item1.X2, edge.Item1.Y2);
                        priorityQueue.Add((edge.Item3, end, nextNode));
                    }
                }
            }

            HighlightEdges(mstEdges);
        }

        private Ellipse FindNodeAtPosition(double x, double y)
        {
            return _nodes.FirstOrDefault(n => Math.Abs(GetNodeCenter(n.Item1).X - x) < 0.1 &&
                                               Math.Abs(GetNodeCenter(n.Item1).Y - y) < 0.1).Item1;
        }

        private void HighlightEdges(IEnumerable<(Line, TextBlock, int)> edges)
        {
            foreach (var edge in _edges)
            {
                edge.Item1.Stroke = Brushes.Gray;
            }

            foreach (var (line, _, _) in edges)
            {
                line.Stroke = Brushes.Red;
                line.StrokeThickness = 3;
            }

            LogBox.Text += "МОД найден и выделен красным цветом.\n";
        }

        private void RemoveEdge_Click(object sender, RoutedEventArgs e)
        {
            if (_edges.Count == 0)
            {
                LogBox.Text += "Ошибка: Нет доступных ребер для удаления.\n";
                return;
            }

            EdgeInputPanel.Visibility = Visibility.Visible;
            LogBox.Text += "Введите начальную и конечную вершины для удаления ребра.\n";
        }

        /*private void ConfirmEdgeInput_Click(object sender, RoutedEventArgs e)
        {
            var startNodeName = StartNodeTextBox.Text;
            var endNodeName = EndNodeTextBox.Text;

            var startNode = FindNodeByName(startNodeName);
            var endNode = FindNodeByName(endNodeName);

            if (startNode == null || endNode == null)
            {
                LogBox.Text += "Ошибка: Указанные вершины не найдены.\n";
                return;
            }

            var edgeToRemove = _edges.FirstOrDefault(edge =>
                (edge.Item1.X1 == GetNodeCenter(startNode).X && edge.Item1.Y1 == GetNodeCenter(startNode).Y &&
                 edge.Item1.X2 == GetNodeCenter(endNode).X && edge.Item1.Y2 == GetNodeCenter(endNode).Y) ||
                (edge.Item1.X1 == GetNodeCenter(endNode).X && edge.Item1.Y1 == GetNodeCenter(endNode).Y &&
                 edge.Item1.X2 == GetNodeCenter(startNode).X && edge.Item1.Y2 == GetNodeCenter(startNode).Y));

            if (edgeToRemove == default)
            {
                LogBox.Text += "Ошибка: Ребро не найдено.\n";
                return;
            }

            GraphCanvas.Children.Remove(edgeToRemove.Item1);
            GraphCanvas.Children.Remove(edgeToRemove.Item2);
            _edges.Remove(edgeToRemove);

            LogBox.Text += "Ребро удалено.\n";
        }*/

        //

        private void AddEdge_Click(object sender, RoutedEventArgs e)
        {
            EdgeInputPanel.Visibility = Visibility.Visible;
            LogBox.Text += "Введите начальную и конечную вершины, а также вес ребра.\n";
        }
        private void AddEdge(Ellipse startNode, Ellipse endNode, int weight)
        {
            var line = new Line
            {
                X1 = GetNodeCenter(startNode).X,
                Y1 = GetNodeCenter(startNode).Y,
                X2 = GetNodeCenter(endNode).X,
                Y2 = GetNodeCenter(endNode).Y,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            GraphCanvas.Children.Add(line);

            var weightLabel = new TextBlock
            {
                Text = weight.ToString(),
                Foreground = Brushes.Black
            };

            double midX = (line.X1 + line.X2) / 2;
            double midY = (line.Y1 + line.Y2) / 2;

            Canvas.SetLeft(weightLabel, midX);
            Canvas.SetTop(weightLabel, midY);

            GraphCanvas.Children.Add(weightLabel);
            _edges.Add((line, weightLabel, weight));
        }

        private void ConfirmEdgeInput_Click(object sender, RoutedEventArgs e)
        {
            string startNodeName = StartNodeTextBox.Text;
            string endNodeName = EndNodeTextBox.Text;
            string weightText = WeightTextBox.Text;

            if (string.IsNullOrEmpty(startNodeName) || string.IsNullOrEmpty(endNodeName) || string.IsNullOrEmpty(weightText))
            {
                LogBox.Text += "Ошибка: не все поля заполнены.\n";
                return;
            }

            if (!int.TryParse(weightText, out int weight))
            {
                LogBox.Text += "Ошибка: вес ребра должен быть числом.\n";
                return;
            }

            Ellipse startNode = FindNodeByName(startNodeName);
            Ellipse endNode = FindNodeByName(endNodeName);

            if (startNode == null || endNode == null)
            {
                LogBox.Text += "Ошибка: указанные вершины не найдены.\n";
                return;
            }

            // Добавление ребра
            AddEdge(startNode, endNode, weight);
            LogBox.Text += $"Ребро добавлено между {startNodeName} и {endNodeName} с весом {weight}.\n";

            // Очистка текстовых полей
            StartNodeTextBox.Clear();
            EndNodeTextBox.Clear();
            WeightTextBox.Clear();

            // Скрытие панели ввода
            EdgeInputPanel.Visibility = Visibility.Collapsed;
        }
    }
}