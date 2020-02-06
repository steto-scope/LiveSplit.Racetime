using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.Racetime.View
{
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color, bool bold=false)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            
            box.SelectionColor = color;
            if(bold) box.SelectionFont = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
            box.AppendText(text);
            if (bold) box.SelectionFont = box.Font;
            box.SelectionColor = box.ForeColor;
        }
    }
}
