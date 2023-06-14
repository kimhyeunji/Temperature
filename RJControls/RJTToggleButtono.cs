using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Management.Instrumentation;


namespace WindowsFormsApp1.RJControls
{
    public class RJTToggleButtono: CheckBox
    {
        //Fields
        private Color onBackColor = Color.ForestGreen; //초록색
        private Color onToggleColor = Color.WhiteSmoke;
        private Color offBackColor = Color.Gray; //회색
        private Color offToggleColor = Color.Gainsboro;

        public Color OnBackColor
        {
            get => onBackColor;
            set
            {
                onBackColor = value;
                this.Invalidate();
            }
        }

        public Color OnToggleColor
        {
            get => onToggleColor;
            set => onToggleColor = value;
        }

        public Color OffBackColor
        {
            get => offBackColor;
            set
            {
                offBackColor = value;
                this.Invalidate();
            }

        }

        public Color OffToggleColor
        {
            get => offToggleColor;
            set
            {
                offToggleColor = value;
                this.Invalidate();
            }
        }

        public override string Text { get => base.Text; set { } }

        //Constructor
        public RJTToggleButtono()
        {
            this.MinimumSize = new Size(45, 22);
        }

        //Methods
        private GraphicsPath getFigurePath()
        {
            int arcSize = this.Height - 1;
            Rectangle leftArc = new Rectangle(0,0,arcSize, arcSize);
            Rectangle rightArc = new Rectangle(this.Width - arcSize -2, 0, arcSize, arcSize);

            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(leftArc, 90, 180);
            path.AddArc(rightArc, 270, 180);
            path.CloseAllFigures();

            return path;

        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            int toggleSize = (int)(this.Height * 0.8); // Adjust the multiplier as desired
            //int toggleSize = this.Height - 5;
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            pevent.Graphics.Clear(this.Parent.BackColor);

            if(this.Checked)
            {

                //Draw the control surface
                pevent.Graphics.FillPath(new SolidBrush(onBackColor),getFigurePath());
                //Draw the toggle
                pevent.Graphics.FillEllipse(new SolidBrush(onToggleColor), new Rectangle(this.Width - this.Height+1, 2, toggleSize, toggleSize));
            }
            else //OFF
            {
                //Draw the control surface
                pevent.Graphics.FillPath(new SolidBrush(offBackColor), getFigurePath());
                //Draw the toggle
                pevent.Graphics.FillEllipse(new SolidBrush(offToggleColor), new Rectangle(2, 2, toggleSize, toggleSize));


            }
        }

    }
}
