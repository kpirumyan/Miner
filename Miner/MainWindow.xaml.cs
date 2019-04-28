using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Miner.Classes;
using Miner.CustomUI;

namespace Miner
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private int _width;
    private int _height;
    private int _minesCount;
    private int _currentMinesCount;
    private DispatcherTimer timeDispatcher = new DispatcherTimer();
    private string _mineSource;
    private string _markAsMineSource;
    private string _markAsUnknownSource;
    private IEnumerable<ButtonAdvanced> cells;
    private event EventHandler<EventArgs> IsWon;

    public MainWindow()
    {
      InitializeComponent();
      Start();
    }

    private void Start()
    {
      InputData();
      BuildGrid();
      SetMinesCount();
      AddMines();
      CalcCellValues();
      SetCellClickHandlers();
      StartTime();
    }

    // Set input data
    private void InputData()
    {
      _width = 10;
      _height = 10;
      _minesCount = 10;
      _currentMinesCount = _minesCount;
      _mineSource = $@"{System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory())}\..\..\Images\Mine.png";
      _markAsMineSource = $@"{System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory())}\..\..\Images\flag.png";
      _markAsUnknownSource = $@"{System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory())}\..\..\Images\question-sign.png";

      IsWon += IsWonHandler;
    }

    // The method creates buttons and place them into a grid
    private void BuildGrid()
    {
      for (int w = 0; w < _width; w++)
      {
        areaGrid.ColumnDefinitions.Add(new ColumnDefinition());
      }
      for (int h = 0; h < _height; h++)
      {
        areaGrid.RowDefinitions.Add(new RowDefinition());
      }

      int index = 0;
      for (int i = 0; i < _height; i++)
      {
        for (int j = 0; j < _width; j++)
        {
          ButtonAdvanced btn = new ButtonAdvanced();
          btn.Index = index++;
          areaGrid.Children.Add(btn);
          Grid.SetRow(btn, i);
          Grid.SetColumn(btn, j);
        }
      }
      cells = areaGrid.Children.Cast<ButtonAdvanced>();
    }

    // Set current mines count to the 'mines' field
    private void SetMinesCount()
    {
      mines.Text = _currentMinesCount.ToString();
    }

    // Add mines to the grid
    private void AddMines()
    {
      // Get randomly indecies where mines will placed
      var rndArray = Enumerable.Range(0, 100).OrderBy(el => Guid.NewGuid()).Take(_minesCount).OrderBy(p => p).ToArray();

      // Set mines
      for (int i = 0; i < _minesCount; i++)
      {
        int index = rndArray[i];
        Image img = new Image();
        img.Source = new BitmapImage(new Uri(_mineSource));
        ButtonAdvanced btn = cells.ElementAt(index);
        btn.Value = img;
        btn.ContentType = FieldContentType.Mine;
      }
    }

    // Calculate values for non-mine fields
    private void CalcCellValues()
    {
      var notMines = cells.Where(btn => btn.ContentType != FieldContentType.Mine);

      foreach (var cell in notMines)
      {
        // Calculate how many mines are in neighbour cells
        var neighbourMinesCount = cells
          .Where(
            btn => (Grid.GetRow(btn) == Grid.GetRow(cell) - 1
                || Grid.GetRow(btn) == Grid.GetRow(cell)
                || Grid.GetRow(btn) == Grid.GetRow(cell) + 1)
                && (Grid.GetColumn(btn) == Grid.GetColumn(cell) - 1
                || Grid.GetColumn(btn) == Grid.GetColumn(cell)
                || Grid.GetColumn(btn) == Grid.GetColumn(cell) + 1)
                && btn.Index != cell.Index
                && btn.ContentType == FieldContentType.Mine)
           .Count();

        // Set numbers or null into cells
        if (neighbourMinesCount != 0)
        {
          cell.Value = neighbourMinesCount.ToString();
          cell.ContentType = FieldContentType.Number;
        }
        else
        {
          cell.Value = null;
          cell.ContentType = FieldContentType.None;
        }
      }
    }

    // Set click event handlers for each field
    private void SetCellClickHandlers()
    {
      foreach (var btn in cells)
      {
        btn.Click += Left_Click;
        btn.MouseRightButtonDown += Right_Click;
      }
    }

    // Left-click events
    private void Left_Click(object sender, RoutedEventArgs e)
    {
      ButtonAdvanced btn = sender as ButtonAdvanced;

      if (btn.Label == MarkedField.None)
      {
        switch (btn.ContentType)
        {
          case FieldContentType.None:
            ShowNeighboursOfEmptyCell(btn);
            break;
          case FieldContentType.Mine:
            MessageBox.Show("Game over! Try again.");
            EndGame();
            break;
          case FieldContentType.Number:
            btn.Content = btn.Value;
            btn.IsEnabled = false;
            break;
          default:
            break;
        }
      }

      // Check if won and game over
      if (_currentMinesCount == 0)
      {
        IsWon?.Invoke(this, new EventArgs());
      }
    }

    // Right-click events
    private void Right_Click(object sender, RoutedEventArgs e)
    {
      ButtonAdvanced btn = sender as ButtonAdvanced;
      Image img = new Image();

      switch (btn.Label)
      {
        case MarkedField.None:
          img.Source = new BitmapImage(new Uri(_markAsMineSource));
          btn.Content = img;
          btn.Label = MarkedField.Flag;
          mines.Text = (--_currentMinesCount).ToString();
          break;
        case MarkedField.Flag:
          img.Source = new BitmapImage(new Uri(_markAsUnknownSource));
          btn.Content = img;
          btn.Label = MarkedField.Unknown;
          mines.Text = (++_currentMinesCount).ToString();
          break;
        case MarkedField.Unknown:
          btn.Content = null;
          btn.Label = MarkedField.None;
          break;
        default:
          break;
      }

      // Check if won and game over
      if (_currentMinesCount == 0)
      {
        IsWon?.Invoke(this, new EventArgs());
      }
    }

    // Show neighbour field values if the cell is empty
    private void ShowNeighboursOfEmptyCell(ButtonAdvanced cell)
    {
      var neighbourCells = cells.Where(
          btn => btn.IsEnabled == true
              && (Grid.GetRow(btn) == Grid.GetRow(cell) - 1
              || Grid.GetRow(btn) == Grid.GetRow(cell)
              || Grid.GetRow(btn) == Grid.GetRow(cell) + 1)
              && (Grid.GetColumn(btn) == Grid.GetColumn(cell) - 1
              || Grid.GetColumn(btn) == Grid.GetColumn(cell)
              || Grid.GetColumn(btn) == Grid.GetColumn(cell) + 1));

      List<ButtonAdvanced> emptyCells = new List<ButtonAdvanced>();
      foreach (var item in neighbourCells)
      {
        if (item.Label == MarkedField.Flag)
        {
          mines.Text = (++_currentMinesCount).ToString();
        }
        item.Content = item.Value;
        if (item.ContentType == FieldContentType.None) emptyCells.Add(item);
        item.IsEnabled = false;
      }
      foreach (var emptyCell in emptyCells)
      {
        ShowNeighboursOfEmptyCell(emptyCell);
      }
    }

    // The method sets a timer with 1 second update period to 'time' field
    private void StartTime()
    {
      time.Text = "00:00";
      timeDispatcher.Interval = new TimeSpan(0, 0, 0, 1, 0);
      timeDispatcher.Tick += time_Tick;
      timeDispatcher.Start();
    }

    // The method ends the game
    private void EndGame()
    {
      foreach (var btn in cells)
      {
        btn.Content = btn.Value;
        btn.IsEnabled = false;
        timeDispatcher.Tick -= time_Tick;
      }
    }

    private void IsWonHandler(object sender, EventArgs e)
    {
      var cellsRemain = cells.Where(c => c.IsEnabled);
      if (cellsRemain.Count() == _minesCount)
      {
        MessageBox.Show("Congratulations! You won!");
        foreach (var cell in cellsRemain)
        {
          cell.IsEnabled = false;
        }
        timeDispatcher.Tick -= time_Tick;
      }
    }

    // Time tick handler
    private void time_Tick(object sender, EventArgs e)
    {
      DateTime t = DateTime.ParseExact(time.Text, "mm:ss", CultureInfo.CurrentCulture);
      t = t.AddSeconds(1);
      time.Text = t.ToString("mm:ss");
    }

    // Reset button click handler
    private void reset_Click(object sender, RoutedEventArgs e)
    {
      // Reset all fields
      areaGrid.Children.Clear();
      areaGrid.ColumnDefinitions.Clear();
      areaGrid.RowDefinitions.Clear();
      mines.Text = string.Empty;
      time.Text = string.Empty;
      timeDispatcher.Tick -= time_Tick;

      Start();
    }
  }
}
