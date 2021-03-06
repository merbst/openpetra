//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       wolfgangb
//
// Copyright 2004-2016 by OM International
//
// This file is part of OpenPetra.org.
//
// OpenPetra.org is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// OpenPetra.org is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with OpenPetra.org.  If not, see <http://www.gnu.org/licenses/>.
//
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using GNU.Gettext;
using Ict.Petra.Shared;
using Ict.Petra.Shared.MReporting;
using Ict.Petra.Shared.Interfaces.MPartner;
using System.Resources;
using System.Collections.Specialized;
using Ict.Common;
using Ict.Common.Verification;
using Ict.Petra.Client.App.Core;
using Ict.Common.Controls;
using Ict.Petra.Client.CommonForms;
using Ict.Petra.Client.MReporting.Logic;

namespace Ict.Petra.Client.MReporting.Gui.MPartner
{
    /// <summary>
    /// manual code for TFrmBriefAddressReport class
    /// </summary>
    public partial class TFrmBriefAddressReport
    {
        /// <summary>
        /// Called during loading of the form
        /// </summary>
        private void RunOnceOnActivationManual()
        {
            FPetraUtilsObject.FFastReportsPlugin.SetDataGetter(LoadReportData);
        }

        private bool LoadReportData(TRptCalculator ACalc)
        {
            return FPetraUtilsObject.FFastReportsPlugin.LoadReportData("BriefAddressReport",
                true,
                new string[] { "Partners", "Locations" },
                ACalc,
                this,
                true,
                false);
        }
    }
}