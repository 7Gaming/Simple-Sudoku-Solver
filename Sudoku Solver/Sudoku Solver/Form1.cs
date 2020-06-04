using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sudoku_Solver
{
    public partial class Form1 : Form
    {
        List<Box> Boxes = new List<Box>();
        int BoxesSolved = 0;
        DateTime Start;
        DateTime Stop;

        public Form1()
        {
            InitializeComponent();
        }

        private void Clear(object sender, EventArgs e)
        {
            foreach (Control c in this.Controls)
            {
                if (c is Label && c.Text != "  ")
                {
                    c.Text = "  ";
                    c.Font = new Font(FontFamily.GenericSansSerif, 32);
                }
            }
        }

        private void Btn_Solve(object sender, EventArgs e)
        {
            Boxes.Clear();
            foreach (Control c in this.Controls)
            {
                if (c is Label)
                {
                    c.Font = new Font(FontFamily.GenericSansSerif, 32);

                    Box b = new Box();
                    b.Text = (Label)c;
                    if (c.Text.Length > 1)
                        c.Text = "  ";
                    if(c.Text != "  ")
                    {
                        b.Possible.Clear();
                        b.Possible.Add(Convert.ToInt32(c.Text));
                        BoxesSolved++;
                    }
                    Boxes.Insert(0, b);
                }
            }

            Start = DateTime.Now;
            Solve();
            Stop = DateTime.Now;
            MessageBox.Show("Done: " + (Stop - Start).Milliseconds + "ms");

            //Display the possibilities for boxes that were left unsolved
            for(int x = 0; x < Boxes.Count; x++)
            {
                if (Boxes[x].OnBoard)
                    continue;
                Boxes[x].Text.Font = new Font(FontFamily.GenericSansSerif, 6);
                for(int y = 0; y < Boxes[x].Possible.Count; y++)
                {
                    Boxes[x].Text.Text += Boxes[x].Possible[y] + (y + 1 == Boxes[x].Possible.Count ? "" : ", ");
                }
            }

        }

        void Solve()
        {
            bool updated = true;

            while (updated)
            {
                updated = false;
                for (int twice = 0; twice < 2; twice++)
                {
                    for (int y = 0; y < 9; y++)
                    {
                        //HORIZONTAL
                        RefreshBoxPos(GetRow(y));
                        //X-Pos Reliant
                        for (int x = 0; x < 9; x++)
                        {
                            //VERTICAL
                            RefreshBoxPos(GetColumn(x));
                            //LOCAL
                            RefreshBoxPos(GetLocal(x, y));
                        }
                    }
                }
                //Update boxes that have been discovered
                updated = UpdateBoard();
            }
        }

        bool UpdateBoard()
        {
            bool updated = false;
            for (int i = 0; i < Boxes.Count; i++)
            {
                if (Boxes[i].Possible.Count == 1 && !Boxes[i].OnBoard)
                {
                    Boxes[i].OnBoard = true;
                    BoxesSolved++;

                    Boxes[i].SetValue();
                    Application.DoEvents();
                    //System.Threading.Thread.Sleep(300);
                    //System.Threading.Thread.Sleep(25);
                    updated = true;
                }
            }
            return updated;
        }

        void RefreshBoxPos(List<Box> axis)
        {
            List<int> used = new List<int>();
            //Read axis and write values that have neen used
            for (int x = 0; x < axis.Count; x++)
            {
                int value = axis[x].GetValue();
                if (value != -1)
                {
                    used.Add(value);
                    if (axis[x].Possible.Count > 1)
                    {
                        axis[x].Possible.Clear();
                        axis[x].Possible.Add(value);
                    }
                    axis.RemoveAt(x);
                    x--;
                }
            }
            //Removes used values from empty box's possibilities
            for (int x = 0; x < axis.Count; x++)
            {
                for (int y = 0; y < used.Count; y++)
                {
                    axis[x].Possible.Remove(used[y]);
                }
            }
            //Compare each box's possibilities against each other
            for (int x = 0; x < axis.Count; x++)
            {
                List<int> possible = new List<int>(axis[x].Possible);
                for(int y = 0; y < axis.Count; y++)
                {
                    if (x == y)
                        continue;
                    if (possible.Count == 0)
                        break;
                    List<int> diff = possible.Intersect(axis[y].Possible).ToList();
                    for(int z = 0; z < diff.Count; z++)
                    {
                        possible.Remove(diff[z]);
                    }
                }
                if(possible.Count == 1)
                {
                    axis[x].Possible.Clear();
                    axis[x].Possible.Add(possible[0]);
                }
            }
        }

        Box GetBox(int x, int y)
        {
            return Boxes[(y * 9) + x];
        }

        List<Box> GetLocal(int x, int y)
        {
            List<Box> b = new List<Box>();
            int localX = (int)Math.Floor(x / 3f) * 3;
            int localY = (int)Math.Floor(y / 3f) * 3;

            for(int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    b.Add(GetBox(localX + i, localY + j));
                }
            }

            return b;
        }

        List<Box> GetColumn(int x)
        {
            List<Box> b = new List<Box>();
            for (int y = 0; y < 9; y++)
            {
                b.Add(GetBox(x, y));
            }
            return b;
        }

        List<Box> GetRow(int y)
        {
            List<Box> b = new List<Box>();
            for(int x = 0; x < 9; x++)
            {
                b.Add(GetBox(x, y));
            }
            return b;
        }

        private void Box_Click(object sender, EventArgs e)
        {
            int add = 0;
            if (((MouseEventArgs)e).Button == MouseButtons.Left)
                add++;
            else if (((MouseEventArgs)e).Button == MouseButtons.Right)
                add--;
            else
                return;

            Label c = ((Label)sender);
            if (c.Text.Length > 1)
            {
                c.Text = "  ";
                c.Font = new Font(FontFamily.GenericSansSerif, 32);
            }

            if (c.Text == "  ")
            {
                if (add > 0)
                    c.Text = "1";
                else
                    c.Text = "9";
            }
            else if (( ((Label)sender).Text == "1" && add < 0) || (((Label)sender).Text == "9" && add > 0))
            {
                c.Text = "  ";
            }
            else
            {
                c.Text = (Convert.ToInt32(c.Text) + add).ToString();
            }
        }
    }

    public class Box
    {
        public Label Text;
        public List<int> Possible = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        public bool OnBoard = false;

        public void SetValue(int x = 0)
        {
            Text.Text = Possible[x].ToString();
        }

        public int GetValue()
        {
            if (Possible.Count != 1)
                return -1;
            return Possible[0];
        }
    }
}
