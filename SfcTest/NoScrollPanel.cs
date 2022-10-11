using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SfcTest
{
    public delegate void MouseWheelEventHandler(object? sender, MouseEventArgs e);

    public class NoScrollPanel : Panel
    {
        public event MouseWheelEventHandler? OnMouseWheelZoom;

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            OnMouseWheelZoom?.Invoke(this, e);
        }
    }
}
