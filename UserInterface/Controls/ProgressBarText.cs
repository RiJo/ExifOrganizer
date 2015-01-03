//
// ProgressBarText.cs: User control to render progress text upon a ProgressBar.
//
// Copyright (C) 2014 Rikard Johansson
//
// This program is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option) any
// later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// this program. If not, see http://www.gnu.org/licenses/.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExifOrganizer.UI.Controls
{
    public partial class ProgressBarText : ProgressBar
    {
        public ProgressBarText()
            : base()
        {
            SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Render base
            Graphics graphics = e.Graphics;
            Rectangle targetRectangle = ClientRectangle;
            ProgressBarRenderer.DrawHorizontalBar(graphics, targetRectangle);

            // Render progress bar
            if (Value > 0)
            {
                targetRectangle.Inflate(-3, -3);
                Rectangle clip = new Rectangle(targetRectangle.X, targetRectangle.Y, (int)Math.Round(((float)Value / Maximum) * targetRectangle.Width), targetRectangle.Height);
                ProgressBarRenderer.DrawHorizontalChunks(graphics, clip);
            }

            // Render text
            string text = GetProgressText();
            if (!String.IsNullOrEmpty(text))
            {
                SizeF textLength = graphics.MeasureString(text, Font);
                int pointY = (int)((Height / 2.0) - (textLength.Height / 2.0));
                int pointX;
                if (CenterText)
                    pointX = (int)((Width / 2.0) - (textLength.Width / 2.0));
                else
                    pointX = 10;
                Point textLocation = new Point(pointX, pointY);
                graphics.DrawString(text, Font, Brush ?? Brushes.Black, textLocation);
            }
        }

        #region Properties

        /// <summary>
        /// Brush used to render progress text.
        /// </summary>
        public Brush Brush
        {
            get;
            set;
        }

        /// <summary>
        /// If progress text should be centered horizontally on bar.
        /// </summary>
        public bool CenterText
        {
            get;
            set;
        }

        /// <summary>
        /// Override progress text (progress in percent) rendered on bar. Default value is null.
        /// </summary>
        public string ProgressText
        {
            get;
            set;
        }

        /// <summary>
        /// Factor (0.0-1.0) of current progress.
        /// </summary>
        public double ProgressFactor
        {
            get { return ((double)(Value - Minimum) / (double)(Maximum - Minimum)).Clamp(0.0, 1.0); }
        }

        /// <summary>
        /// Percent (0.0-100.0) of current progress.
        /// </summary>
        public double ProgressPercent
        {
            get { return (ProgressFactor * 100.0).Clamp(0.0, 100.0); }
        }

        #endregion

        /// <summary>
        /// Update current progress value and message. Used to prevent unecessary repaints
        /// of this control.
        /// </summary>
        /// <param name="factor">Factor (0.0-1.0) of current progress</param>
        /// <param name="message">Override progress text (progress in percent) rendered on bar.</param>
        public void SetProgress(double factor, string message = null)
        {
            int interval = (Maximum - Minimum);
            int value = Minimum + (int)Math.Round(factor * interval);
            if (value > Maximum)
                value = Maximum;
            if (value < Minimum)
                value = Minimum;

            SetProgress(value, message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">Percent (0.0-100.0) of current progress</param>
        /// <param name="message">Override progress text (progress in percent) rendered on bar.</param>
        public void SetProgress(int value, string message = null)
        {
            if (value == Value && message == ProgressText)
                return;

            ProgressText = message;
            Value = value; // Forces repaint
        }

        public string GetProgressText()
        {
            if (ProgressText != null)
                return ProgressText;

            return String.Format("{0}%", Math.Round(ProgressPercent, 1));
        }
    }
}
