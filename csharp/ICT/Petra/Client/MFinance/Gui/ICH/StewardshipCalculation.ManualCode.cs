﻿//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       christophert
//       Tim Ingham
//
// Copyright 2004-2012 by OM International
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
using System.Data;
using System.IO;
using System.Globalization;
using System.Windows.Forms;
using System.Threading;
using GNU.Gettext;
using Ict.Common;
using Ict.Common.Controls;
using Ict.Common.Data;
using Ict.Common.DB;
using Ict.Common.Verification;
using Ict.Common.Remoting.Shared;
using Ict.Common.Remoting.Client;
using Ict.Petra.Client.App.Core;
using Ict.Petra.Client.App.Core.RemoteObjects;
using Ict.Petra.Shared;
using Ict.Petra.Client.MCommon;
using Ict.Petra.Client.CommonControls;
using Ict.Petra.Client.MFinance.Logic;

using Ict.Petra.Shared.MFinance;
using Ict.Petra.Shared.MFinance.Account.Data;
using Ict.Petra.Shared.Interfaces.MFinance;
using System.Collections.Generic;
using Ict.Petra.Client.MReporting.Gui.MFinance;

namespace Ict.Petra.Client.MFinance.Gui.ICH
{
    /// manual methods for the generated window
    public partial class TFrmStewardshipCalculation : System.Windows.Forms.Form
    {
        Int32 FLedgerNumber = 0;

        /// <summary>
        /// Write-only Ledger number property
        /// </summary>
        public Int32 LedgerNumber
        {
            set
            {
                FLedgerNumber = value;

                //btnOK.Visible = false;

                TFinanceControls.InitialiseOpenFinancialPeriodsList(
                    ref cmbReportPeriod,
                    FLedgerNumber);

                cmbReportPeriod.SelectedIndex = 0;
            }
        }

        private bool ValidReportPeriod()
        {
            if (cmbReportPeriod.SelectedIndex > -1)
            {
                return true;
            }
            else if (cmbReportPeriod.SelectedIndex == -1)
            {
                MessageBox.Show(Catalog.GetString("Please select a valid reporting period first."));
                cmbReportPeriod.Focus();
                return false;
            }
            else
            {
                return false;
            }
        }

        private void StewardshipCalculation(Object Sender, EventArgs e)
        {
            if (!ValidReportPeriod())
            {
                return;
            }

            TVerificationResultCollection VerificationResult = null;

            try
            {
                Cursor = Cursors.WaitCursor;
                List <Int32>glBatchNumbers;

                Boolean retVal = TRemote.MFinance.ICH.WebConnectors.PerformStewardshipCalculation(
                    FLedgerNumber,
                    cmbReportPeriod.GetSelectedInt32(),
                    out glBatchNumbers,
                    out VerificationResult);

                TFrmBatchPostingRegister ReportGui = new TFrmBatchPostingRegister(null);

                foreach (Int32 glBatchNumber in glBatchNumbers)
                {
                    if (glBatchNumber > 0)
                    {
                        ReportGui.PrintReportNoUi(FLedgerNumber, glBatchNumber);
                    }
                }

                Cursor = Cursors.Default;
                String ResultMsg =
                    (retVal ? Catalog.GetString("Stewardship Calculation Completed Successfully")
                     : Catalog.GetString("UNSUCCESSFUL Stewardship Calculation!"));

                MessageBox.Show(Messages.BuildMessageFromVerificationResult(ResultMsg, VerificationResult),
                    Catalog.GetString("Stewardship Calculation"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                if (retVal)
                {
                    Close();
                }
                else
                {
                    btnCancel.Text = "Close";
                }
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                TLogging.LogException(ex, Utilities.GetMethodSignature());
                MessageBox.Show(String.Format(Catalog.GetString(
                            "An unexpected error occurred when trying to perform the Stewardship Calculation!{0}{0}Error: {1}"),
                        Environment.NewLine, ex.Message),
                    Catalog.GetString("Stewardship Calculation"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnOK_Click(Object Sender, EventArgs e)
        {
        }
    }
}