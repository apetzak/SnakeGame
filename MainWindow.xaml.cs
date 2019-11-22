/* Snake Game
Rules
- Game starts with 1 chunk snake and 1 pellet
- Press any arrow key to start game and move snake
- Collecting a pellet adds 2 chunks to snake
- Collecting a pellet makes a new pellet appear in a random position
- Game over when the snake overlaps or goes out of bounds
- Press space to pause when game is active
- Press space to restart when game is over
- Score label updates as game progresses
- High score label updates when game ends
- High score is not saved when game is closed
Note: Game runs slow in debug mode
*/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Data.SqlClient;
using System.Data.SQLite;

namespace SnakeGame
{
    public partial class MainWindow : Window
    {
        // game objects
        public static List<Chunk> snake = new List<Chunk>();
        public static Chunk pellet = new Chunk(0, 0);

        // game variables
        public static DispatcherTimer timer = new DispatcherTimer();
        public static int keyDirection = 4; // 4 != valid key direction
        public static bool keyPressed = false;
        public static bool gameIsPaused = false;
        public static bool gameIsActive = false;
        public static bool gameHasStarted = false;

        // controls
        public static Label lblPaused = new Label();
        public static Label lblGameOver = new Label();

        public MainWindow()
        {
            InitializeComponent();

            // set icon
            Uri iconUri = new Uri("pack://application:,,,/Icon1.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);

            //string path = @"..\..\HighScore.xml";

            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
            timer.Interval = TimeSpan.FromMilliseconds(100);         // (.1 second)
            timer.Tick += Timer_Tick;                                

            // create colors
            SolidColorBrush black = new SolidColorBrush();
            black.Color = Color.FromRgb(0, 0, 0);
            SolidColorBrush gray = new SolidColorBrush();
            gray.Color = Color.FromRgb(30, 30, 30);

            SetLabelProperties(lblPaused, "Paused", 60, 350, "C", "C", 3);
            SetLabelProperties(lblGameOver, "Game Over", 60, 350, "C", "C", 3);

            AddSnake();
            AddPellet();
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (!keyPressed)
            {
                if ((e.Key == Key.Up || e.Key == Key.W) && keyDirection != 2) // only change keyDirection if direction != opposite direction
                    keyDirection = 0;

                if ((e.Key == Key.Right || e.Key == Key.D) && keyDirection != 3)
                    keyDirection = 1;

                if ((e.Key == Key.Down || e.Key == Key.S) && keyDirection != 0)
                    keyDirection = 2;

                if ((e.Key == Key.Left || e.Key == Key.A) && keyDirection != 1)
                    keyDirection = 3;

                if (keyDirection == 0 || keyDirection == 1 || keyDirection == 2 || keyDirection == 3)
                    keyPressed = true;
            }

            if (e.Key == Key.Space)
            {
                if (gameIsActive)
                {
                    if (!gameIsPaused) // pause
                    {
                        timer.Stop();
                        gameIsPaused = true;
                        canvas.Children.Add(lblPaused);
                    }
                    else               // unpause
                    {
                        timer.Start();
                        gameIsPaused = false;
                        canvas.Children.Remove(lblPaused);
                    }
                }
                else if (gameHasStarted)
                    Restart();
            }

            if (gameHasStarted == false)
                if (keyPressed)
                {
                    gameHasStarted = true;
                    gameIsActive = true;
                    timer.Start();
                }
        }

        public void Timer_Tick(object source, EventArgs e)
        {
            Chunk head = new Chunk(snake[0].Top, snake[0].Left);

            // check key direction then adjust new head position accordingly
            if (keyDirection == 0) // up
                head.Top = snake[0].Top - 20;
            else if (keyDirection == 1) // right
                head.Left = snake[0].Left + 20;
            else if (keyDirection == 2) // down
                head.Top = snake[0].Top + 20;
            else if (keyDirection == 3) // left
                head.Left = snake[0].Left - 20;

            keyPressed = false; // this ensures that keyDirection can only be changed once per frame

            snake.Insert(0, head);                     // insert head into snake
            snake.RemoveAt(snake.Count - 1);           // remove tail from snake
            canvas.Children.RemoveAt(snake.Count - 1); // remove tail from canvas
            canvas.Children.Insert(0, Square(new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.White), 1, 20, 20, snake[0].Left, snake[0].Top, 2)); // insert head into canvas
            lblLength.Content = "Length: " + snake.Count; // update lblLength;

            // check for pellet /////////////////
            bool temp = false;
            if (snake[0].Left.Equals(pellet.Left) && snake[0].Top.Equals(pellet.Top))
            {
                PlaySound("gulp");            
                Chunk ch = new Chunk(pellet.Top, pellet.Left);
                snake.Insert(0, ch);               // insert at head index
                Chunk newTail = snake[snake.Count - 1];
                snake.Insert(snake.Count - 1, ch); // insert at tail index
                canvas.Children.Insert(snake.Count - 1, Square(new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.White), 1, 20, 20, snake[0].Left, snake[0].Top, 2));
                AddPellet();
                temp = true;
            }
            ResetPellet(); // resets pellet if it spawned under the snake

            // check for overlap ////////////////       
            if (!temp) // check only if pellet position != head position
                for (int i = 1; i < snake.Count; i++)
                    if (snake[0].Left.Equals(snake[i].Left) && snake[0].Top.Equals(snake[i].Top))
                        GameOver();

            // check for out of bounds //////////
            if (snake[0].Left >= 700 || snake[0].Left < 0 || snake[0].Top >= 500 || snake[0].Top < 0)
                GameOver();
        }

        public void ResetPellet()
        {
            bool valid = true;
            while (valid)
            {
                for (int i = 0; i < snake.Count; i++)                                          // loop through snake chunks
                    if (snake[i].Left.Equals(pellet.Left) && snake[i].Top.Equals(pellet.Top))  // check for match
                    {
                        canvas.Children.RemoveAt(canvas.Children.Count - 1); // remove pellet
                        AddPellet();   // add pellet to random position
                        valid = false; // restart loop
                    }
                if (valid)
                    break; // exit loop
            }
        }

        public void AddPellet()
        {
            Random rand = new Random();
            pellet = new Chunk(rand.Next(0, 25) * 20, rand.Next(0, 34) * 20);
            canvas.Children.Add(Square(new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.White), 1, 20, 20, pellet.Left, pellet.Top, 1));
        }

        public void AddSnake()
        {
            canvas.Children.Add(Square(new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.White), 1, 20, 20, 40, 40, 2));
            Chunk c = new Chunk(40, 40);
            snake.Add(c);
        }

        public Rectangle Square(SolidColorBrush fill, SolidColorBrush stroke, int strokeThickness, int width, int height, int left, int top, int zIndex)
        {
            Rectangle r = new Rectangle();
            r.Fill = fill;      // color
            r.Stroke = stroke;  // border color
            r.StrokeThickness = strokeThickness;
            r.Width = width;
            r.Height = height;
            Canvas.SetLeft(r, left);     // set position relative to left of canvas
            Canvas.SetTop(r, top);       // set position relative to top of canvas
            Canvas.SetZIndex(r, zIndex); // set canvas display index (higher zIndex has higher display priority)
            return r;
        }

        public bool SoundEnabled = true;

        public void PlaySound(string file)
        {
            if (SoundEnabled)
            {
                string path = @"C:\Users\Alec\Documents\Visual Studio 2015\Projects\Games\SnakeGame\SnakeGame\Sounds\" + file + ".wav";
                SoundPlayer player = new SoundPlayer(path);
                player.Load();
                player.Play();
            }
        }

        private void GameOver()
        {
            PlaySound("game-over");
            string highScore = lblHighScore.Content.ToString();
            int score = Convert.ToInt32(highScore.Substring(11)); // take number off of label content
            if (snake.Count > score)                              // if score > label number                              
                score = snake.Count;                              // set new high score                               
            lblHighScore.Content = "High Score: " + score;        // update label        

            // stop game
            gameIsActive = false;
            keyPressed = false;
            timer.Stop();
            canvas.Children.Add(lblGameOver);
        }

        public void Restart()
        {
            canvas.Children.Clear(); // remove all children from canvas
            snake.Clear();           // remove all chunks from snake
            AddSnake();
            AddPellet();
            lblLength.Content = "Length: " + snake.Count;
            gameHasStarted = false;
            keyPressed = false;
            keyDirection = 4;
            canvas.Children.Remove(lblGameOver);
        }

        public void SetLabelProperties(Label l, string content, int fontSize, int width, string verticalAlign, string horizontalAlign, int zIndex)
        {
            SolidColorBrush white = new SolidColorBrush();
            white.Color = Color.FromRgb(180, 180, 180);

            l.Content = content;
            l.Foreground = white;
            l.FontSize = fontSize;
            l.Width = width;
            Canvas.SetZIndex(l, zIndex);

            VerticalAlignment v = new VerticalAlignment();
            if (verticalAlign == "T")
                v = VerticalAlignment.Top;
            if (verticalAlign == "C")
                v = VerticalAlignment.Center;
            if (verticalAlign == "B")
                v = VerticalAlignment.Bottom;

            HorizontalAlignment h = new HorizontalAlignment();
            if (horizontalAlign == "L")
                h = HorizontalAlignment.Left;
            if (horizontalAlign == "C")
                h = HorizontalAlignment.Center;
            if (horizontalAlign == "R")
                h = HorizontalAlignment.Right;

            l.VerticalAlignment = v;
            l.HorizontalAlignment = h;
        }
    }
}

public class Chunk
{
    public int Top;  // position relative to top of canvas
    public int Left; // position relative to left of canvas

    // constructor
    public Chunk(int top, int left)
    {
        this.Top = top;
        this.Left = left;
    }
}