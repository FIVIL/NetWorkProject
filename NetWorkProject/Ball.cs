using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetWorkProject
{
    public class Ball
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double VX { get; set; }
        public double VY { get; set; }
        public int Area { get; set; }
        public Ball(string s, int area, out int col, out int row, out BallCrossingStatus bcs)
        {
            var temp = s.Split(';');
            X = double.Parse(temp[0]);
            Y = double.Parse(temp[1]);
            VX = double.Parse(temp[2]);
            VY = double.Parse(temp[3]);
            Area = area;
            col = int.Parse(temp[4]);
            row = int.Parse(temp[5]);
            Enum.TryParse(temp[6], out bcs);
            switch (bcs)
            {
                case BallCrossingStatus.Top:
                    Y = 299;
                    break;
                case BallCrossingStatus.Left:
                    X = 329;
                    break;
                case BallCrossingStatus.Right:
                    X = -50;
                    break;
                case BallCrossingStatus.bottom:
                    Y = -50;
                    break;
                case BallCrossingStatus.TopRight:
                    Y = 299;
                    X = -50;
                    break;
                case BallCrossingStatus.TopLeft:
                    X = 329;
                    Y = 299;
                    break;
                case BallCrossingStatus.ButtomRight:
                    X = -50;
                    Y = -50;
                    break;
                case BallCrossingStatus.ButtomLeft:
                    X = 329;
                    Y = -50;
                    break;
                default:
                    break;
            }
        }
        public Ball(string s)
        {
            var temp = s.Split(';');
            X = double.Parse(temp[0]);
            Y = double.Parse(temp[1]);
            VX = double.Parse(temp[2]);
            VY = double.Parse(temp[3]);
            Area = int.Parse(temp[4]);
        }
        public Ball(string s, int area)
        {
            var temp = s.Split(';');
            X = double.Parse(temp[0]);
            Y = double.Parse(temp[1]);
            VX = double.Parse(temp[2]);
            VY = double.Parse(temp[3]);
            Area = area;
        }
        public string ToString(int col, int row,BallCrossingStatus bcs)
        {
            return X + ";" + Y + ";" + VX + ";" + VY + ";" + col + ";" + row + ";" + bcs.ToString() + ";BallReport";
        }
        public void Xstep()
        {
            X += VX;
        }
        public void Ystep()
        {
            Y += VY;
        }
        public void ResetVX(double Ratio)
        {
            VX *= Ratio;
        }
        public void ResetVY(double Ratio)
        {
            VY *= Ratio;

        }
    }
}
