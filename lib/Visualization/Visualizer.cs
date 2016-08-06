using lib.Graphs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lib
{
    public class GraphVisualizer<TEdge,TNode>
    {
        public Func<Node<TEdge, TNode>, double> GetX;
        public Func<Node<TEdge, TNode>, double> GetY;
        public Func<Edge<TEdge, TNode>, Color> EdgeColor = e => Color.Black;
        public Func<Node<TEdge, TNode>, Color> NodeColor = e => Color.Red;
        public Func<Edge<TEdge, TNode>, string> EdgeCaption;
        public Func<Node<TEdge, TNode>, string> NodeCaption;
        public bool AsSymmetric = true;

        public Font Font = new Font("Arial", 14f);

        public void Window(int size, Graph<TEdge, TNode> graph, string name="")
        {
            var form = new Form();
            form.ClientSize = new Size(size, size);
            form.Paint += (s, a) =>
              {
                  Draw(size, graph, a.Graphics);
              };
            form.Text = name;
            Application.Run(form);
        }

        public void Draw(int size, Graph<TEdge,TNode> graph, Graphics g)
        {
            var minX = graph.Nodes.Select(z => GetX(z)).Min();
            var maxX = graph.Nodes.Select(z => GetX(z)).Max();
            var minY = graph.Nodes.Select(z => GetY(z)).Min();
            var maxY = graph.Nodes.Select(z => GetY(z)).Max();

            var marg = 80;
            Func<Node<TEdge,TNode>, Point> Projector =
                node => new Point(
                    marg+(int)((size-2*marg) * (GetX(node) - minX) / (maxX - minX)), 
                    marg+(int)((size-2*marg) * (GetY(node)- minY) / (maxY - minY)));

            foreach (var e in graph.Edges)
            {
                if (AsSymmetric && e.From.NodeNumber > e.To.NodeNumber) continue;

                var p1 = Projector(e.From);
                var p2 = Projector(e.To);
                Pen pen = new Pen(EdgeColor(e));
                g.DrawLine(pen, p1, p2);

                if (EdgeCaption == null) continue;
                var p = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
                g.DrawString(EdgeCaption(e), Font, Brushes.Black, p);
            }

            foreach (var e in graph.Nodes)
            {
                var p = Projector(e);
                Brush b = new SolidBrush(NodeColor(e));
                g.FillEllipse(b, p.X - 5, p.Y - 5, 10, 10);

                if (NodeCaption == null) continue;
                g.DrawString(NodeCaption(e),
                    Font,
                    Brushes.Black,
                    new Rectangle(p.X - 50, p.Y - 50, 100, 100),
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
            

        }
    }
}
