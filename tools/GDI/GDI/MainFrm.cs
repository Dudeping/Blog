using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GDIPlus
{
    public partial class MainFrm : Form
    {
        public MainFrm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 创建GDI+对象
            Graphics g = this.CreateGraphics(); // 使用窗体创建
            // 创建画笔
            Pen pen = new Pen(Brushes.Red);

            // 创建两个点
            Point pt1 = new Point(50, 50);
            Point pt2 = new Point(250, 250);

            g.DrawLine(pen, pt1, pt2);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Graphics g = this.CreateGraphics();
            g.DrawRectangle(new Pen(Brushes.Red), 20, 20, 50, 30);
        }
    }
}
