using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NetWorkProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ac = new Actor();
            ac.OnSetWindowLocation += SetLocation;
            ac.OnSetPuaseResum += (b) =>
            {
                if (b)
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        PauseRec.Visibility = Visibility.Hidden;
                        PauseText.Visibility = Visibility.Hidden;
                        PuaseButton.Data = Geometry.Parse("M17.90004,0L29.099999,0 29.099999,32 17.90004,32z M0,0L11.200022,0 11.200022,32 0,32z");
                    }));

                }
                else
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        PauseRec.Visibility = Visibility.Visible;
                        PauseText.Visibility = Visibility.Visible;
                        PuaseButton.Data = Geometry.Parse("M0, 0L15.825011, 8.0009766 31.650999, 15.997986 15.825011, 23.998993 0, 32 0, 15.997986z");
                    }));
                    
                }
            };
            //SetLocation(ac.Name);
        }
        private void SetLocation(int row,int col)
        {
            //double Width = SystemParameters.PrimaryScreenWidth;
            //double Height = SystemParameters.PrimaryScreenHeight;
            //int row = (int)Width / 335;
            //int thisrow = Number % row;
            //int col = Number / row;
            //Console.WriteLine(row+"aaaaaaaaaaaaaaaaa"+col);
            this.Dispatcher.Invoke((Action)(() =>
            {
                //coll.Text = "col "+col.ToString();
                //roww.Text = "row "+row.ToString();
                this.Left = (row * 322.5);
                this.Top = (col * 322);
                try
                {
                    SetWall(ac.THis.Left, Leftt);
                    SetWall(ac.THis.Top, Topp);
                    SetWall(ac.THis.Buttom, Down);
                    SetWall(ac.THis.Right, Right);
                }
                catch (Exception){}
            }));
            //MessageBox.Show(row + "aaaaa" + col);
        }
        private void SetWall(WallStatus ws,object element1)
        {
            DropShadowEffect MakeDSE(WallStatus wws)
            {
                DropShadowEffect DSE = new DropShadowEffect
                {
                    Direction = 0,
                    BlurRadius = 25,
                    ShadowDepth = 0
                };
                switch (wws)
                {
                    case WallStatus.AcceleratorHard:
                        DSE.Color = Colors.Red;
                        break;
                    case WallStatus.DeceleratorHard:
                        DSE.Color = Colors.Red;
                        break;
                    case WallStatus.AcceleratorSoft:
                        DSE.Color = Colors.Blue;
                        break;
                    case WallStatus.DeceleratorSoft:
                        DSE.Color = Colors.Blue;
                        break;
                    default:
                        break;
                }
                return DSE;
            }
            Rectangle rc = element1 as Rectangle;
            switch (ws)
            {
                case WallStatus.AcceleratorHard:
                    rc.Fill = Brushes.Red;
                    break;
                case WallStatus.DeceleratorHard:
                    rc.Fill = Brushes.Red;
                    break;
                case WallStatus.AcceleratorSoft:
                    rc.Fill = Brushes.Blue;
                    break;
                case WallStatus.DeceleratorSoft:
                    rc.Fill = Brushes.Blue;
                    break;
                default:
                    break;
            }
            rc.Effect = MakeDSE(ws);
        }
        private Actor ac;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ac.Set();
            this.Title += ac.Name;
            ac.OnBallCordinationChanged += (x, y) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    Canvas.SetLeft(Ball, x);
                    Canvas.SetTop(Ball, y);
                }));
            };
            //if (ac.Name != 0) SetLocation(ac.THis.Row, ac.THis.Col);
        }
        private void Ball_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double xpos = e.GetPosition(sender as Rectangle).X;
            double ypos = e.GetPosition(sender as Rectangle).Y;
            if (Actor.SystemStatus == BallStatus.DoesntHaveBall)
            {
                xpos -= 25;
                ypos -= 25;
                if (ac != null)
                {
                    if (e.ChangedButton == MouseButton.Left) ac.init(xpos, ypos, 1);
                    else ac.init(xpos, ypos, 2);
                }
                //Ball.Visibility = Visibility.Visible;
                Canvas.SetLeft(Ball, xpos);
                Canvas.SetTop(Ball, ypos);
                Actor.SystemStatus = BallStatus.HasBall;
                ac.Status = BallStatus.HasBall;
                if (ac.THis.IsServer) ac.Server.Server.SendAll("ballInitiated");
                else ac.Client.Send(ac.Name + ";ballInitiated");
            }
            else
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    Rectangle rct = new Rectangle()
                    {
                        Width = 50,
                        Height = 50,
                        Stroke = Brushes.Black,
                        StrokeThickness = 0.5,
                        Effect = new DropShadowEffect
                        {
                            Color = Colors.Red,
                            Direction = 0,
                            ShadowDepth = 0,
                            BlurRadius = 25
                        },
                        Fill = Brushes.DarkRed
                    };
                    rct.MouseDown += (obje, eve) =>
                    {
                        ac.SOL.Remove(ac.SOL.First(x => x.container.Equals(obje as UIElement)));
                        MainContainer.Children.Remove(obje as UIElement);
                    };
                    Canvas.SetLeft(rct, xpos);
                    Canvas.SetTop(rct, ypos);
                    MainContainer.Children.Add(rct);
                    ac.SOL.Add(new ScreenObject
                    {
                        X = (int)xpos,
                        Y = (int)ypos,
                        Orient = Orientation.Top,
                        Type = ScreenObjectType.Decelerator,
                        container=rct
                    });
                }
                if (e.ChangedButton == MouseButton.Right)
                {
                    Rectangle rct = new Rectangle()
                    {
                        Width = 50,
                        Height = 50,
                        Stroke = Brushes.Black,
                        StrokeThickness = 0.5,
                        Effect = new DropShadowEffect
                        {
                            Color = Colors.Blue,
                            Direction = 0,
                            ShadowDepth = 0,
                            BlurRadius = 25
                        },
                        Fill = Brushes.Blue,
                        Opacity = 0.6
                    };
                    rct.MouseDown += (obje, eve) =>
                    {
                        ac.SOL.Remove(ac.SOL.First(x => x.container.Equals(obje as UIElement)));
                        MainContainer.Children.Remove(obje as UIElement);
                    };
                    Canvas.SetLeft(rct, xpos);
                    Canvas.SetTop(rct, ypos);
                    MainContainer.Children.Add(rct);
                    ac.SOL.Add(new ScreenObject
                    {
                        X = (int)xpos,
                        Y = (int)ypos,
                        Orient = Orientation.Top,
                        Type = ScreenObjectType.Accelerator,
                        container=rct
                    });
                }
            }
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (!ac.THis.IsServer) ac.Client.Disconnect();
                else ac.Server.Server.Stop();
            }
            catch { }
            ac.ActionFlag = false;
            Thread.Sleep(1);
            Application.Current.Shutdown();
            Environment.Exit(0);
        }
        int forceclose = 0;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            forceclose++;
            if (forceclose < 5 && ac.Status == BallStatus.HasBall)
            {
                e.Cancel = true;
                return;
            }
            try
            {
                if (!ac.THis.IsServer) ac.Client.Send(ac.Name + ";ClientRemoved");
                else
                {
                    ac.Server.Server.SendAll(ac.Server.GetNext() + ";ServerClosed");
                    Console.WriteLine("XXXXXXXXXXXXXXXXXX"+ac.Server.GetNext());
                }
            }
            catch { }
        }
        public static bool pause = true;
        private void PuaseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (pause)
            {
                if (ac.THis.IsServer) ac.Server.Server.SendAll("Pause");
                else ac.Client.Send("Pause");
                pause = !pause;

            }
            else
            {
                if (ac.THis.IsServer) ac.Server.Server.SendAll("Resume");
                else ac.Client.Send("Resume");
                pause = !pause;
            }
        }

        private void Ball_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                ac.b.ResetVX(-5);
                ac.b.ResetVY(-5);
            }
            if (e.ChangedButton == MouseButton.Right)
            {
                ac.b.ResetVX(5);
                ac.b.ResetVY(5);
            }
        }
    }
}
