using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCP;
using System.Windows;
namespace NetWorkProject
{
    public delegate void BallCordinationChanged(double x, double y);
    public delegate void SetWindowsLocation(int x, int y);
    public enum BallStatus
    {
        HasBall,
        DoesntHaveBall
    }
    public enum WallStatus
    {
        AcceleratorHard,
        DeceleratorHard,
        AcceleratorSoft,
        DeceleratorSoft
    }
    public class Actor
    {
        public event SetBool OnSetPuaseResum;
        public List<ScreenObject> SOL;
        public bool IsTransfering { get; set; }
        public bool IsEntering { get; set; }
        public ManualResetEvent Pause;
        public event BallCordinationChanged OnBallCordinationChanged;
        public ScreenCondition THis { get; set; }
        public SystemServer Server { get; set; }
        public string Mess;
        public int Name { get; set; }
        public BallStatus Status { get; set; }
        public static BallStatus SystemStatus { get; set; }
        public TCPClientt Client { get; set; }
        public bool ActionFlag { get; set; }
        public Ball b;
        public Random rnd { get; set; }
        public event SetWindowsLocation OnSetWindowLocation;
        public Actor()
        {

        }
        private void Reset(int ServerName)
        {
            SOL = new List<ScreenObject>();
            IsTransfering = false;
            IsEntering = false;
            SystemStatus = BallStatus.DoesntHaveBall;
            Status = BallStatus.DoesntHaveBall;
            if (Name == ServerName)
            {
                Server = new SystemServer(2020 + Client.Port);
                Server.OnSetBool += (b) =>
                {
                    if (b) SystemStatus = BallStatus.HasBall;
                };
                //Server.OnSetPauseResume += (b) =>
                //{
                //    if (b) Pause.Set();
                //    else Pause.Reset();
                //    MainWindow.pause = b;
                //    OnSetPuaseResum?.Invoke(b);
                //};
                THis = new ScreenCondition
                {
                    address = null,
                    Row = 0,
                    Col = 0,
                    HasBall = false,
                    IsServer = true,
                    Top = WallStatus.AcceleratorHard,
                    Buttom = WallStatus.AcceleratorHard,
                    Left = WallStatus.DeceleratorHard,
                    Right = WallStatus.DeceleratorHard,
                };
                OnSetWindowLocation?.Invoke(0, 0);
            }
            //else
            //  {
            Client = new TCPClientt(2020);
            Client.Connect();
            Client.OnMessageReciveClient += (mess) =>
            {
                if (Name != ServerName)
                {
                    if (mess.Contains("relocate"))
                    {
                        THis = new ScreenCondition(mess, out BallStatus bs);
                        Actor.SystemStatus = bs;
                        Mess = mess;
                        OnSetWindowLocation?.Invoke(THis.Row, THis.Col);
                    }
                    if (mess.Equals("ballInitiated"))
                    {
                        SystemStatus = BallStatus.HasBall;
                    }
                }
                if (mess.Contains("ServerClosed"))
                {
                    Client.Disconnect();
                    Thread.Sleep(20);
                    if (mess.Contains(Name.ToString()))
                    {
                        this.Reset(Name);
                        Server.Server.Start();
                        THis.IsServer = true;
                        Client.Reconnect(2020);
                    }
                    else
                    {
                        Thread.Sleep(15);
                        Client.Reconnect(2020);
                        Client.Send(Name + ";IsConnected");
                    }
                }
                if (mess.Equals("Pause"))
                {
                    Pause.Reset();
                    MainWindow.pause = false;
                    OnSetPuaseResum?.Invoke(false);
                }
                if (mess.Equals("Resume"))
                {
                    Pause.Set();
                    MainWindow.pause = true;
                    OnSetPuaseResum?.Invoke(true);
                }
                if (mess.Contains("BallReport"))
                {
                    Console.WriteLine(mess + "   asClient");
                    b = new Ball(mess, Name);
                    THis.HasBall = true;
                    Status = BallStatus.HasBall;
                    ActionFlag = true;
                    IsTransfering = true;
                    //if (mess.Contains("ButtomRight"))
                    CornerCross = false;
                    Pause.Set();
                    Action();
                }
                if (mess.Contains("relocate"))
                {
                    THis.Update(mess);
                    OnSetWindowLocation?.Invoke(THis.Row, THis.Col);
                    //new ScreenCondition(mess,out BallStatus bs)
                }
            };
            if (Name != ServerName) Client.Send(Name + ";IsConnected");
            // }
            ActionFlag = false;
            Pause = new ManualResetEvent(false);

        }
        public void Set()
        {
            SOL = new List<ScreenObject>();
            IsTransfering = false;
            IsEntering = false;
            SystemStatus = BallStatus.DoesntHaveBall;
            Status = BallStatus.DoesntHaveBall;
            ReadFile(out int name);
            Name = name;
            if (Name == 0)
            {
                Server = new SystemServer(2020);
                Server.OnSetBool += (b) =>
                {
                    if (b) SystemStatus = BallStatus.HasBall;
                };
                //Server.OnSetPauseResume += (b) =>
                //{
                //    if (b) Pause.Set();
                //    else Pause.Reset();
                //    MainWindow.pause = b;
                //    OnSetPuaseResum?.Invoke(b);
                //};
                THis = new ScreenCondition
                {
                    address = null,
                    Row = 0,
                    Col = 0,
                    HasBall = false,
                    IsServer = true,
                    Top = WallStatus.AcceleratorHard,
                    Buttom = WallStatus.AcceleratorHard,
                    Left = WallStatus.DeceleratorHard,
                    Right = WallStatus.DeceleratorHard,
                };
                OnSetWindowLocation?.Invoke(0, 0);
            }
            //else
            //  {
            Client = new TCPClientt(2020);
            Client.Connect();
            Client.OnMessageReciveClient += (mess) =>
            {
                if (name != 0)
                {
                    if (mess.Contains("relocate"))
                    {
                        THis = new ScreenCondition(mess, out BallStatus bs);
                        Actor.SystemStatus = bs;
                        Mess = mess;
                        OnSetWindowLocation?.Invoke(THis.Row, THis.Col);
                    }
                    if (mess.Equals("ballInitiated"))
                    {
                        SystemStatus = BallStatus.HasBall;
                    }
                }
                if (mess.Contains("ServerClosed"))
                {
                    Client.Disconnect();
                    Thread.Sleep(20);
                    if (mess.Contains(Name.ToString()))
                    {
                        Server.Server.Start();
                        THis.IsServer = true;
                        Client.Reconnect(2020 + Client.Port);
                    }
                    else
                    {
                        Client.Reconnect(2020 + Client.Port);
                        Client.Send(Name + ";IsConnected");
                    }
                }
                if (mess.Equals("Pause"))
                {
                    Pause.Reset();
                    MainWindow.pause = false;
                    OnSetPuaseResum?.Invoke(false);
                }
                if (mess.Equals("Resume"))
                {
                    Pause.Set();
                    MainWindow.pause = true;
                    OnSetPuaseResum?.Invoke(true);
                }
                if (mess.Contains("BallReport"))
                {
                    Console.WriteLine(mess + "   asClient");
                    b = new Ball(mess, Name);
                    THis.HasBall = true;
                    Status = BallStatus.HasBall;
                    ActionFlag = true;
                    IsTransfering = true;
                    //if (mess.Contains("ButtomRight"))
                        CornerCross = false;
                    Pause.Set();
                    Action();
                }
                if (mess.Contains("relocate"))
                {
                    THis.Update(mess);
                    OnSetWindowLocation?.Invoke(THis.Row, THis.Col);
                    //new ScreenCondition(mess,out BallStatus bs)
                }
            };
            if (name != 0) Client.Send(Name + ";IsConnected");
            // }
            ActionFlag = false;
            Pause = new ManualResetEvent(false);
        }
        private bool CornerCross = true;
        private void Action()
        {
            Task.Run(() =>
            {
                while (ActionFlag)
                {
                    Pause.WaitOne();
                    if (Math.Abs(b.VX) > 1)
                    {
                        if (b.VX < 0) b.VX ++;
                        else b.VX --;
                    }
                    if (Math.Abs(b.VY) > 1)
                    {
                        if (b.VY < 0) b.VY ++;
                        else b.VY --;
                    }
                    b.Xstep();
                    b.Ystep();
                    if (IsTransfering)
                    {
                        if (b.X > 1 && b.X < 277 && b.Y > 1 && b.Y < 247)
                        {
                            IsTransfering = false;
                            CornerCross = true;
                        }
                    }
                    foreach (var item in SOL)
                    {
                        double distance = Math.Sqrt(Math.Pow((b.X - item.X), 2) +
                            Math.Pow((b.Y - item.Y), 2));
                        if (distance < (50 * Math.Sqrt(2)))
                        {
                            if ((int)b.X + 50 == item.X || (int)b.X == item.X + 50)
                            {
                                if (item.Type == ScreenObjectType.Decelerator) b.ResetVX(-1);
                            }
                            if ((int)b.Y + 50 == item.Y || (int)b.Y == item.Y + 50)
                            {
                                if (item.Type == ScreenObjectType.Decelerator) b.ResetVY(-1);
                            }
                            if (item.Type == ScreenObjectType.Accelerator)
                            {
                                if (Math.Abs(b.VX) == 1)
                                {
                                    b.ResetVX(10);
                                    b.ResetVY(10);
                                }
                            }
                        }

                    }
                    //cornerCheck
                    if (b.X <= 0 && b.Y <=0 )
                    {
                        if (THis.Left.ToString().Contains("Soft") && THis.Top.ToString().Contains("Soft"))
                        {
                            if (CornerCross)
                            {
                                CornerCross = false;
                                Client.Send(b.ToString(THis.Col, THis.Row, BallCrossingStatus.TopLeft));
                            }
                        }
                    }
                    if (b.X <= 0 && b.Y >= 248)
                    {
                        if (THis.Left.ToString().Contains("Soft") && THis.Buttom.ToString().Contains("Soft"))
                        {
                            if (CornerCross)
                            {
                                CornerCross = false;
                                Client.Send(b.ToString(THis.Col, THis.Row, BallCrossingStatus.ButtomLeft));
                            }
                        }
                    }
                    if (b.X >= 278 && b.Y <= 0)
                    {
                        if (THis.Right.ToString().Contains("Soft") && THis.Top.ToString().Contains("Soft"))
                        {
                            if (CornerCross)
                            {
                                CornerCross = false;
                                Client.Send(b.ToString(THis.Col, THis.Row, BallCrossingStatus.TopRight));
                            }
                        }
                    }
                    if (b.X >=278 && b.Y >=248)
                    {
                        if (THis.Right.ToString().Contains("Soft")&&THis.Buttom.ToString().Contains("Soft"))
                        {
                            if (CornerCross)
                            {
                                CornerCross = false;
                                Client.Send(b.ToString(THis.Col, THis.Row, BallCrossingStatus.ButtomRight));
                            }
                        }
                    }
                    //Console.WriteLine(b.ToString(0,0,BallCrossingStatus.bottom));
                    if (b.X <= 0)
                    {
                        switch (THis.Left)
                        {
                            case WallStatus.AcceleratorHard:
                                b.ResetVX(-1);
                                break;
                            case WallStatus.DeceleratorHard:
                                b.ResetVX(-1);
                                break;
                            case WallStatus.AcceleratorSoft:
                                if (IsTransfering == false)
                                {
                                    IsTransfering = true;
                                    Client.Send(b.ToString(THis.Col, THis.Row, BallCrossingStatus.Left));
                                }
                                if (b.X < -50)
                                {
                                    ActionFlag = false;
                                    IsTransfering = false;
                                    THis.HasBall = false;
                                    Status = BallStatus.DoesntHaveBall;
                                }
                                break;
                            case WallStatus.DeceleratorSoft:
                                if (IsTransfering == false)
                                {
                                    IsTransfering = true;
                                    Client.Send(b.ToString(THis.Col, THis.Row, BallCrossingStatus.Left));
                                }
                                if (b.X < -50)
                                {
                                    ActionFlag = false;
                                    IsTransfering = false;
                                    THis.HasBall = false;
                                    Status = BallStatus.DoesntHaveBall;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    if (b.X >= 278)
                    {
                        switch (THis.Right)
                        {
                            case WallStatus.AcceleratorHard:
                                b.ResetVX(-1);
                                break;
                            case WallStatus.DeceleratorHard:
                                b.ResetVX(-1);
                                break;
                            case WallStatus.AcceleratorSoft:
                                if (IsTransfering == false)
                                {
                                    IsTransfering = true;
                                    Client.Send(b.ToString(THis.Col, THis.Row, BallCrossingStatus.Right));
                                }
                                if (b.X > 330)
                                {
                                    ActionFlag = false;
                                    IsTransfering = false;
                                    THis.HasBall = false;
                                    Status = BallStatus.DoesntHaveBall;
                                }
                                break;
                            case WallStatus.DeceleratorSoft:
                                if (IsTransfering == false)
                                {
                                    IsTransfering = true;
                                    Client.Send(b.ToString(THis.Col, THis.Row, BallCrossingStatus.Right));
                                }
                                if (b.X > 330)
                                {
                                    ActionFlag = false;
                                    IsTransfering = false;
                                    THis.HasBall = false;
                                    Status = BallStatus.DoesntHaveBall;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    if (b.Y <= 0)
                    {
                        switch (THis.Top)
                        {
                            case WallStatus.AcceleratorHard:
                                b.ResetVY(-1);
                                break;
                            case WallStatus.DeceleratorHard:
                                b.ResetVY(-1);
                                break;
                            case WallStatus.AcceleratorSoft:
                                if (IsTransfering == false)
                                {
                                    IsTransfering = true;
                                    Client.Send(b.ToString(THis.Col, THis.Row, BallCrossingStatus.Top));
                                }
                                if (b.Y < -50)
                                {
                                    ActionFlag = false;
                                    IsTransfering = false;
                                    THis.HasBall = false;
                                    Status = BallStatus.DoesntHaveBall;
                                }
                                break;
                            case WallStatus.DeceleratorSoft:
                                if (IsTransfering == false)
                                {
                                    IsTransfering = true;
                                    Client.Send(b.ToString(THis.Col, THis.Row, BallCrossingStatus.Top));
                                }
                                if (b.Y < -50)
                                {
                                    ActionFlag = false;
                                    IsTransfering = false;
                                    THis.HasBall = false;
                                    Status = BallStatus.DoesntHaveBall;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    if (b.Y >= 248)
                    {
                        switch (THis.Buttom)
                        {
                            case WallStatus.AcceleratorHard:
                                b.ResetVY(-1);
                                break;
                            case WallStatus.DeceleratorHard:
                                b.ResetVY(-1);
                                break;
                            case WallStatus.AcceleratorSoft:
                                if (IsTransfering == false)
                                {
                                    IsTransfering = true;
                                    Client.Send(b.ToString(THis.Col, THis.Row, BallCrossingStatus.bottom));
                                }
                                if (b.Y > 300)
                                {
                                    ActionFlag = false;
                                    IsTransfering = false;
                                    THis.HasBall = false;
                                    Status = BallStatus.DoesntHaveBall;
                                }
                                break;
                            case WallStatus.DeceleratorSoft:
                                if (IsTransfering == false)
                                {
                                    IsTransfering = true;
                                    Client.Send(b.ToString(THis.Col, THis.Row, BallCrossingStatus.bottom));
                                }
                                if (b.Y > 300)
                                {
                                    ActionFlag = false;
                                    IsTransfering = false;
                                    THis.HasBall = false;
                                    Status = BallStatus.DoesntHaveBall;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    OnBallCordinationChanged?.Invoke(b.X, b.Y);
                    Thread.Sleep(5);
                }
            }
            );
        }
        public void init(double x, double y, int ck)
        {
            ActionFlag = true;
            if (ck == 1) b = new Ball(x + ";" + y + ";1;1;" + Name);
            else if (ck == 2) b = new Ball(x + ";" + y + ";-1;-1;" + Name);
            Pause.Set();
            if (b.X <= 0) b.X = 0;
            if (b.Y <= 0) b.Y = 0;
            if (b.X >= 278) b.X = 277;
            if (b.Y >= 248) b.Y = 247;
            Action();
        }
        private void ReadFile(out int name)
        {
            name = 0;
            var lines = File.ReadAllLines("config.txt");
            string[] res = new string[1];
            if (lines.Length == 0)
            {
                name = 0;
                res[0] = "1";
            }
            else
            {
                name = int.Parse(lines[0]);
                name++;
                res[0] = name.ToString();
            }
            File.WriteAllLines("config.txt", res);
        }
    }
    public enum ScreenObjectType
    {
        Accelerator,
        Decelerator
    }
    public enum Orientation
    {
        Top,
        Right,
        Button,
        Left
    }
    public class ScreenObject
    {
        public int X { get; set; }
        public int Y { get; set; }
        public ScreenObjectType Type { get; set; }
        public Orientation Orient { get; set; }
        public UIElement container { get; set; }
    }
}
