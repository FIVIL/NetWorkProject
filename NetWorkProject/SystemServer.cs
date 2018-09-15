using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCP;
namespace NetWorkProject
{
    public delegate void SetBool(bool b);
    public enum BallCrossingStatus
    {
        Top,
        Left,
        Right,
        bottom,
        TopRight,
        TopLeft,
        ButtomRight,
        ButtomLeft
    }
    public class SystemServer
    {
        public event SetBool OnSetBool;
        public event SetBool OnSetPauseResume;
        private IPEndPoint ServerAddress;
        public TCPServer Server { get; set; }
        public int Count { get; set; }
        private int Matrixsize;
        private Dictionary<IPEndPoint,ScreenCondition> MatrixList { get; set; }
        private Random rnd;
        public ScreenCondition GetFirst()
        {
            return MatrixList.First().Value;
        }
        public SystemServer(int port)
        {
            Server = new TCPServer(port);
            Server.Start();
            Server.OnMessageRecive += (sender, mess) =>
            {
                Console.WriteLine(mess);
                if (mess.Contains("IsConnected"))
                {
                    Add((sender as Socket).RemoteEndPoint as IPEndPoint);
                    foreach (var item in MatrixList.Values)
                    {
                        if (!item.IsServer) Server.Send(item.address, item.ToString(Actor.SystemStatus));
                    }
                }
                if (mess.Contains("ClientRemoved"))
                {
                    var splitedmess = mess.Split(';');
                    Remove((sender as Socket).RemoteEndPoint as IPEndPoint);
                    foreach (var item in MatrixList.Values)
                    {
                        Console.WriteLine(item.ToString());
                        if (!item.IsServer) Server.Send(item.address, item.ToString(Actor.SystemStatus));
                    }
                }
                if (mess.Contains("ballInitiated"))
                {
                    OnSetBool?.Invoke(true);
                    Server.SendAll("ballInitiated");
                }
                if (mess.Equals("Pause"))
                {
                    OnSetPauseResume?.Invoke(false);
                    Server.SendAll("Pause");
                }
                if (mess.Equals("Resume"))
                {
                    OnSetPauseResume?.Invoke(true);
                    Server.SendAll("Resume");
                }
                if (mess.Contains("BallReport"))
                {
                    Ball b = new Ball(mess, 0, out int coll, out int roww, out BallCrossingStatus bcs);
                    switch (bcs)
                    {
                        case BallCrossingStatus.Top:
                            coll--;
                            break;
                        case BallCrossingStatus.Left:
                            roww--;
                            break;
                        case BallCrossingStatus.Right:
                            roww++;
                            break;
                        case BallCrossingStatus.bottom:
                            coll++;
                            break;
                        case BallCrossingStatus.TopRight:
                            coll--;
                            roww++;
                            break;
                        case BallCrossingStatus.TopLeft:
                            coll--;
                            roww--;
                            break;
                        case BallCrossingStatus.ButtomRight:
                            coll++;
                            roww++;
                            break;
                        case BallCrossingStatus.ButtomLeft:
                            coll++;
                            roww--;
                            break;
                        default:
                            break;
                    }
                    ScreenCondition sc = MatrixList.Values.Where(x => x.Col == coll && x.Row == roww).First();
                    if (sc.IsServer)
                    {
                        Server.Send(Server.First, b.ToString(coll, roww, bcs));
                    }
                    else
                    {
                        Server.Send(sc.address, b.ToString(coll, roww, bcs));
                    }
                }
            };
            rnd = new Random();
            Count = 1;
            Matrixsize = (int)Math.Ceiling(Math.Sqrt(Count));
            MatrixList = new Dictionary<IPEndPoint, ScreenCondition>();
            ServerAddress = new IPEndPoint(IPAddress.Any, 1010);
            MatrixList.Add(ServerAddress, new ScreenCondition
            {
                HasBall = false,
                Name = 0,
                IsServer = true,
                Top = WallStatus.AcceleratorHard,
                Right = WallStatus.DeceleratorHard,
                Buttom = WallStatus.AcceleratorHard,
                Left = WallStatus.DeceleratorHard,
                Col = 0,
                Row = 0
            });

        }
        public int GetNext()
        {
            return MatrixList.Values.ElementAt(0).Name;
        }
        public void Add(IPEndPoint add)
        {
            lock (this)
            {
                Count++;
                MatrixList.Add(add,new ScreenCondition
                {
                    HasBall = false,
                    Name = Count,
                    IsServer = false,
                    address = add
                });
                Reorder();
            }
        }
        public void Remove(IPEndPoint add)
        {
            lock (this)
            {
                Server.Remove(add);
                Count--;
                MatrixList.Remove(add);
                Reorder();
            }
        }
        private void Reorder()
        {
            int Matrixsize = (int)Math.Ceiling(Math.Sqrt(MatrixList.Count));
            int row = 0, col = 0;
            int index = 0;
            foreach (var item in MatrixList.Values)
            {
                //item.Name = MatrixList.IndexOf(item);
                item.Row = row;
                item.Col = col;
                if (rnd.Next(0, 2) == 0) item.Top =  WallStatus.AcceleratorSoft;
                else item.Top = WallStatus.DeceleratorSoft;
                if (rnd.Next(0, 2) == 0) item.Right = WallStatus.AcceleratorSoft;
                else item.Right = WallStatus.DeceleratorSoft;
                if (rnd.Next(0, 2) == 0) item.Buttom = WallStatus.AcceleratorSoft;
                else item.Buttom = WallStatus.DeceleratorSoft;
                if (rnd.Next(0, 2) == 0) item.Left = WallStatus.AcceleratorSoft;
                else item.Left = WallStatus.DeceleratorSoft;
                if (row == 0)
                {
                    if (col % 2 == 0) item.Left  = WallStatus.AcceleratorHard;
                    else item.Left = WallStatus.DeceleratorHard;
                }
                if (row == Matrixsize - 1) 
                {
                    if (col % 2 == 0) item.Right = WallStatus.DeceleratorHard;
                    else item.Right = WallStatus.AcceleratorHard;
                }
                if (col == 0)
                {
                    if (row % 2 == 0) item.Top = WallStatus.AcceleratorHard;
                    else item.Top = WallStatus.DeceleratorHard;
                }
                if (col == Matrixsize - 1) 
                {
                    if (row % 2 == 0) item.Buttom = WallStatus.DeceleratorHard;
                    else item.Buttom = WallStatus.AcceleratorHard;
                }
                if (MatrixList.Values.Count <= Matrixsize * (Matrixsize - 1)) 
                {
                    if (col == Matrixsize - 2)
                    {
                        if (row % 2 == 0) item.Buttom = WallStatus.DeceleratorHard;
                        else item.Buttom = WallStatus.AcceleratorHard;
                    }
                }
                if (MatrixList.Values.Count <= index + Matrixsize) 
                {
                    if (row % 2 == 0) item.Buttom = WallStatus.DeceleratorHard;
                    else item.Buttom = WallStatus.AcceleratorHard;

                }
                if (item.Equals(MatrixList.Last().Value))
                {
                    item.Right = WallStatus.AcceleratorHard;
                    item.Buttom = WallStatus.DeceleratorHard;
                }
                row++;
                if(row==Matrixsize)
                {
                    col++;
                    row = 0;
                }
                index++;
            }
            Server.Send(Server.First, MatrixList.Values.ElementAt(0).ToString(Actor.SystemStatus));
        }
    }
    public class ScreenCondition
    {
        public int Name { get; set; }
        public bool HasBall { get; set; }
        public bool IsServer { get; set; }
        public int Col { get; set; }
        public int Row { get; set; }
        public WallStatus Top { get; set; }
        public WallStatus Right { get; set; }
        public WallStatus Buttom { get; set; }
        public WallStatus Left { get; set; }
        public IPEndPoint address { get; set; }
        public ScreenCondition(string s,out BallStatus bs)
        {
            //Console.WriteLine(s);
            var splitedmess = s.Split(';');
            Col = int.Parse(splitedmess[0]);
            Row = int.Parse(splitedmess[1]);
            Enum.TryParse(splitedmess[2], out WallStatus top);
            Enum.TryParse(splitedmess[3], out WallStatus right);
            Enum.TryParse(splitedmess[4], out WallStatus buttom);
            Enum.TryParse(splitedmess[5], out WallStatus left);
            Enum.TryParse(splitedmess[6], out BallStatus b);
            bs = b;
            Top = top;
            Right = right;
            Buttom = buttom;
            Left = left;
            IsServer = false;
            //Console.WriteLine("XXXXXXXXXXXXXXXXXX");
            //Console.WriteLine(ToString(bs));
        }
        public void Update(string s)
        {
            var splitedmess = s.Split(';');
            Enum.TryParse(splitedmess[2], out WallStatus top);
            Enum.TryParse(splitedmess[3], out WallStatus right);
            Enum.TryParse(splitedmess[4], out WallStatus buttom);
            Enum.TryParse(splitedmess[5], out WallStatus left);
            Enum.TryParse(splitedmess[6], out BallStatus b);
            Top = top;
            Right = right;
            Buttom = buttom;
            Left = left;
        }
        public ScreenCondition()
        {

        }
        public string ToString(BallStatus bs)
        {
            return Col + ";" + Row + ";" + Top.ToString() + ";" +
                Right.ToString() + ";" + Buttom.ToString() + ";" + Left.ToString() +
                ";" + bs.ToString()+
                ";relocate";
        }
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
