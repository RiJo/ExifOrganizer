﻿//
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
        {
        }

        public string ProgressText
        {
            get;
            set;
        }

        public double Progress
        {
            get { return ((double)(Value - Minimum) / (double)(Maximum - Minimum)); }
        }

        public string GetProgressText()
        {
            if (!String.IsNullOrEmpty(ProgressText))
                return ProgressText;

            return String.Format("{0}%", Math.Round(Progress, 1));
        }

        public Brush Brush
        {
            get;
            set;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // TODO
        }
    }
}
