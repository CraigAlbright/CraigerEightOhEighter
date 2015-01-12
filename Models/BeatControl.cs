using System;
using System.Drawing;
using System.Windows.Forms;


namespace CraigerEightOhEighter.Models
{

    /// <summary>
    /// This class implements a simple UI control for selefting beat patterns
    /// </summary>
    public class BeatControl : Control
    {
        public BeatControl()
        {
            Count = 8;
            using (var gfx = CreateGraphics())
            {
                if (_mTicks.Length > 0)
                {
                    for (int i = 0; i < _mTicks.Length; i++)
                    {
                        PaintTick(i, gfx);
                    }
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            for (var i = 0; i < _mTicks.Length; i++)
                PaintTick(i, e.Graphics);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            for (var i = 0; i < _mTicks.Length; i++)
                PaintTick(i, e.Graphics);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var w = _mTicks.Length*e.X/Width;
                if (w >= 0 && w < _mTicks.Length)
                    this[w] = !this[w];
            }
            base.OnMouseDown(e);
        }

        public int Count
        {
            get { return _mTicks.Length; }
            set
            {
                _mTicks = new bool[value];
                Invalidate();
            }
        }

        public bool this[int ndx]
        {
            get { return _mTicks[ndx]; }
            set
            {
                if (value != _mTicks[ndx])
                {
                    var e = new SelectEventArgs(ndx, value);
                    OnBeatControlSelect(e);
                    if (!e.Cancel)
                    {
                        _mTicks[ndx] = value;
                        using (var g = CreateGraphics())
                            PaintTick(ndx, g);
                    }
                }
            }
        }

        public event ControlSelectEvent BeatControlSelect;

        private bool[] _mTicks;

        private void PaintTick(int ndx, Graphics g)
        {
            var w = Width/_mTicks.Length;
            var r = new Rectangle(ndx*w, 0, w, Height);
            r.Height--;
            if (ndx < 4)
            {
                BackColor = Color.Red;
            }
            if (ndx >= 4 && ndx < 8)
            {
                BackColor = Color.Yellow;
            }
            if (ndx >= 8 && ndx < 12)
            {
                BackColor = Color.White;
            }
            if (ndx >= 12)
            {
                BackColor = Color.OrangeRed;
            }
            ForeColor = Color.PowderBlue;
            g.FillRectangle(new SolidBrush(_mTicks[ndx] ? ForeColor : BackColor), r);
            g.DrawRectangle(new Pen(Color.Black), r);
        }

        protected void OnBeatControlSelect(SelectEventArgs e)
        {
            if (BeatControlSelect != null)
                BeatControlSelect(this, e);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var parms = base.CreateParams;
                parms.Style &= ~0x02000000; // Turn off WS_CLIPCHILDREN
                return parms;
            }
        }

        public class SelectEventArgs : EventArgs
        {
            public SelectEventArgs(int index, bool state)
            {
                Index = index;
                State = state;
            }

            public readonly int Index;
            public readonly bool State;
            public bool Cancel;
        }

        public delegate void ControlSelectEvent(object sender, SelectEventArgs e);

    }
}
