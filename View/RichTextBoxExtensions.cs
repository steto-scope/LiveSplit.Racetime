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
        public static Font NormalFont = new Font("Microsoft Sans Serif",10, FontStyle.Regular);
        public static Font BoldFont = new Font("Microsoft Sans Serif",10, FontStyle.Bold);
        public static Font ItalicFont = new Font("Microsoft Sans Serif", 10, FontStyle.Italic);


        public static void AppendText(this RichTextBox box, string text, Color color, Color highlightcolor,bool highlight=false, bool bold=false)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            
            box.SelectionColor = color;
            if (highlight) box.SelectionColor = highlightcolor;
            if (bold) box.SelectionFont = BoldFont;
            box.AppendText(text);
            if (bold) box.SelectionFont = box.Font;
            if (highlight) box.SelectionColor = box.BackColor;
            box.SelectionColor = box.ForeColor;
        }
    }
}
