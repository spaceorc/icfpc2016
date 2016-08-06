using System;
using System.Drawing;
using System.Windows.Forms;

namespace lib
{
    public class PolygonsAndSegmentsForm : Form
    {
        private Polygon[] polygons;
        private Segment[] segments;
        private Panel panel1;
        private readonly Painter painter = new Painter();

        public PolygonsAndSegmentsForm()
        {
            InitializeComponent();
        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            painter.Paint(e.Graphics, Math.Min(panel1.ClientSize.Width, panel1.ClientSize.Height),
                polygons ?? new Polygon[0], segments ?? new Segment[0]);
        }

        public void SetData(Polygon[] polygons, Segment[] segments)
        {
            this.polygons = polygons;
            this.segments = segments;
            panel1.Invalidate();
        }

        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(508, 435);
            this.panel1.TabIndex = 0;
            this.panel1.Paint += Panel1_Paint;
            // 
            // PolygonsAndSegmentsForm
            // 
            this.ClientSize = new System.Drawing.Size(508, 435);
            this.Controls.Add(this.panel1);
            this.Name = "PolygonsAndSegmentsForm";
            this.ResumeLayout(false);
        }
    }
}