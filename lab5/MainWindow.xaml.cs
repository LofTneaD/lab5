﻿using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lab5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            TaskThreeFourWindow taskThreeFourWindow = new TaskThreeFourWindow();
            taskThreeFourWindow.Show();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            TaskOne taskOne = new TaskOne();
            taskOne.Show();
        }
    }
}