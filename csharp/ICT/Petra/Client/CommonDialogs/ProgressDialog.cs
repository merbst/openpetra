//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop, christiank
//
// Copyright 2004-2014 by OM International
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
using System.Threading;
using System.Windows.Forms;

using Ict.Common;
using Ict.Petra.Client.App.Core.RemoteObjects;
using Ict.Petra.Shared;
using Ict.Petra.Shared.MCommon;
using Ict.Petra.Shared.Interfaces.MCommon;

namespace Ict.Petra.Client.CommonDialogs
{
    /// <summary>
    /// Dialog for showing the progress of a webconnector method that is executed in a Thread, giving the option to cancel the job.
    /// </summary>
    public partial class TProgressDialog : System.Windows.Forms.Form
    {
        private bool FConfirmedClosing = false;
        private bool FShowCancellationConfirmationQuestion = false;
        private bool FCancelled = false;
        private string FMessage, FCaption;
        private int FTotal, FCurrentProgress = 0;
        private bool FFinished = false;
        private bool FQueryServerForProgress = true;
        private Thread FWorkerThread = null;
        private bool FWaitForThreadComplete = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="AWorkerThread">The Thread that performs the work that the progress dialog shows the progress of.</param>
        /// <param name="AShowCancellationConfirmationQuestion">In case the user requests a cancellation: should
        /// a Yes/No MessageBox for the confirmation of the cancellation be shown, or not? NOTE: If that
        /// MessageBox should be shown then the consequence of doing this is that the Thread will still be continuing
        /// the work it is performing until the user chooses 'Yes', which can result in the work being finished
        /// before the user had a chance to read the message of the MessageBox and press 'Yes' - and that might
        /// well not be what the user wants!!! So, in general this Argument should be set to true only for
        /// Threads that are running a substantial amount of time. (Default=false).</param>
        /// <param name="AQueryServerForProgress">does this track a process on the server</param>
        /// <param name="AWaitForThreadComplete">Keep dialog box open until the worker thread is complete, even if server-side processing is finished.
        /// This was added because there is a delay (10-15 seconds in my testing) between this dialog closing and the FastReports progress window appears.</param>
        public TProgressDialog(Thread AWorkerThread,
            bool AShowCancellationConfirmationQuestion = false,
            bool AQueryServerForProgress = true,
            bool AWaitForThreadComplete = false) : base()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            #region CATALOGI18N

            // this code has been inserted by GenerateI18N, all changes in this region will be overwritten by GenerateI18N
            this.btnCancel.Text = Catalog.GetString("Cancel");
            this.Text = Catalog.GetString("Progress Dialog");
            #endregion

            FShowCancellationConfirmationQuestion = AShowCancellationConfirmationQuestion;

            TRemote.MCommon.WebConnectors.Reset();
            AWorkerThread.Start();
            FWorkerThread = AWorkerThread;
            FWaitForThreadComplete = AWaitForThreadComplete;

            FQueryServerForProgress = AQueryServerForProgress;

            timer1.Start();
        }

        /// <summary>
        /// Determines whether the Cancel Button is (or: should be) enabled, or not.
        /// </summary>
        public bool AllowCancellation
        {
            get
            {
                return btnCancel.Enabled;
            }

            set
            {
                btnCancel.Enabled = value;
            }
        }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            DialogResult CancelConfirmationResult = DialogResult.Yes;

            if (FShowCancellationConfirmationQuestion)
            {
                CancelConfirmationResult =
                    MessageBox.Show(Catalog.GetString("Do you really want to cancel?\r\n\r\nNote: Execution is continuing until 'Yes' is chosen!"),
                        Catalog.GetString("Confirm Cancellation"),
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
            }

            if (CancelConfirmationResult == DialogResult.Yes)
            {
                FCancelled = true;

                if (FQueryServerForProgress)
                {
                    try
                    {
                        TRemote.MCommon.WebConnectors.CancelJob();
                        FWorkerThread.Join();
                    }
                    catch (Exception Exc)
                    {
                        TLogging.Log("While cancelling a job from a Progress Dialog we got the following Exception:\r\n" + Exc.ToString());
                    }
                }
            }
        }

        /// the message to be displayed
        public string Message
        {
            set
            {
                FMessage = value;
            }
        }

        /// the caption to be displayed
        public string Caption
        {
            set
            {
                FCaption = value;
            }
        }

        /// the total amount
        public int Total
        {
            set
            {
                FTotal = value;
            }
        }

        /// the current Progress
        public int CurrentProgress
        {
            set
            {
                FCurrentProgress = value;
            }
        }

        /// has the job finished successfully
        public bool Finished
        {
            set
            {
                FFinished = value;
            }
        }

        /// has the job been cancelled
        public bool Cancelled
        {
            get
            {
                return FCancelled;
            }
        }

        private int FCountConsecutiveErrors = 0;

        private void Timer1Tick(object sender, EventArgs e)
        {
            string caption;
            string message;
            int percentage;
            bool finished;

            if (FQueryServerForProgress)
            {
                try
                {
                    if (TRemote.MCommon.WebConnectors.GetCurrentState(out caption,
                            out message,
                            out percentage,
                            out finished))
                    {
                        FCountConsecutiveErrors = 0;
                        this.Text = caption;
                        this.lblMessage.Text = message;
                        this.progressBar.Value = percentage;

                        if (finished)
                        {
                            // don't close the dialog yet if FWaitForThreadComplete=true and FWorkerThread.IsAlive=true
                            if (!FWaitForThreadComplete || !FWorkerThread.IsAlive)
                            {
                                // wait till the thread finishes
                                FWorkerThread.Join();
                                this.DialogResult = FCancelled ? DialogResult.Cancel : DialogResult.OK;
                                FConfirmedClosing = true;
                                Close();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    FCountConsecutiveErrors++;

                    if (FCountConsecutiveErrors > 3)
                    {
                        throw ex;
                    }
                }
            }
            else
            {
                this.Text = FCaption;
                this.lblMessage.Text = FMessage;

                if (FTotal > 0)
                {
                    this.progressBar.Value = Convert.ToInt32((100.0m * FCurrentProgress) / FTotal);
                }

                if (FFinished)
                {
                    // wait till the thread finishes
                    FWorkerThread.Join();
                    this.DialogResult = FCancelled ? DialogResult.Cancel : DialogResult.OK;
                    FConfirmedClosing = true;
                    Close();
                }
            }
        }

        private void TProgressDialogFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!FConfirmedClosing)
            {
                e.Cancel = true;
                BtnCancelClick(null, null);
            }
        }

        /// <summary>
        /// Sets the Refresh Interval
        /// </summary>
        /// <param name="AInterval"></param>
        public void SetRefreshInterval(int AInterval)
        {
            timer1.Interval = AInterval;
        }
    }
}
