//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
//       Tim Ingham
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
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;
using Ict.Common;
using Ict.Common.Data;
using Ict.Common.Verification;
using Ict.Common.Controls;
using Ict.Petra.Shared.MFinance.AP.Data;
using Ict.Petra.Client.App.Core;
using Ict.Petra.Client.App.Core.RemoteObjects;
using Ict.Petra.Client.CommonControls;
using Ict.Petra.Client.CommonForms;
using Ict.Petra.Client.MCommon;
using Ict.Petra.Client.MFinance.Gui.GL;
using Ict.Petra.Client.MFinance.Gui.Setup;
using Ict.Petra.Shared.MFinance;
using Ict.Petra.Shared.Security;
using Ict.Petra.Shared.Interfaces.MFinance;
using System.Threading;
using Ict.Petra.Client.MReporting.Gui.MFinance;
using Ict.Petra.Client.MFinance.Logic;

namespace Ict.Petra.Client.MFinance.Gui.AP
{
    public partial class TUC_SupplierTransactions
    {
        private Int32 FLedgerNumber = -1;
        private Int64 FPartnerKey = -1;
        private AApSupplierRow FSupplierRow = null;
        private IAPUIConnectorsFind FFindObject = null;
        private bool FKeepUpSearchFinishedCheck = false;
        AccountsPayableTDS FMainDS = new AccountsPayableTDS();

        private TFrmAPMain FMainForm = null;

        private Boolean FRequireApprovalBeforePosting = false;
        private bool FIsInvoiceDataChanged = false;

        /// <summary>DataTable that holds all Pages of data (also empty ones that are not retrieved yet!)</summary>
        private DataTable FPagedDataTable;


        private String FTypeFilter = "";    // filter which types of transactions are shown
        private String FStatusFilter = "";  // filter the status of invoices
        private String FHistoryFilter = "";  // filter the status of history
        private string FAgedOlderThan;

        private TSgrdDataGrid grdDetails;
        private int FPrevRowChangedRow = -1;
        private DataRow FPreviouslySelectedDetailRow = null;

        /// <summary>
        /// Set this to true to notify the class that the invoice data has changed in some way
        /// </summary>
        public bool IsInvoiceDataChanged
        {
            set
            {
                FIsInvoiceDataChanged = value;
            }
        }

        /// <summary>
        /// Standard method
        /// </summary>
        public void GetDataFromControls()
        {
        }

        private void InitializeManualCode()
        {
            grdDetails = grdResult;

            grdResult.MouseClick += new MouseEventHandler(grdResult_Click);
            grdResult.DataPageLoaded += new TDataPageLoadedEventHandler(grdResult_DataPageLoaded);
            grdResult.Selection.SelectionChanged += new SourceGrid.RangeRegionChangedEventHandler(grdResult_SelectionChanged);
        }

        /// <summary>
        /// Initialize GUI after the constructor
        /// </summary>
        /// <param name="AMainForm"></param>
        public void InitializeGUI(TFrmAPMain AMainForm)
        {
            FMainForm = AMainForm;

            TFrmPetraUtils utils = FMainForm.GetPetraUtilsObject();
            utils.SetStatusBarText(grdDetails,
                Catalog.GetString("Use the navigation keys to select a transaction.  Double-click to view the details"));
            utils.SetStatusBarText(btnReloadList, Catalog.GetString("Click to re-load the transactions list"));
            utils.SetStatusBarText(btnTagAll, Catalog.GetString("Click to tag all the displayed items"));
            utils.SetStatusBarText(btnUntagAll, Catalog.GetString("Click to un-tag all the displayed items"));
            utils.SetStatusBarText(chkToggleFilter, Catalog.GetString("Click to show/hide the Filter/Find panel"));
        }

        private void grdResult_SelectionChanged(object sender, SourceGrid.RangeRegionChangedEventArgs e)
        {
            FPrevRowChangedRow = grdResult.Selection.ActivePosition.Row;
            SetEnabledStates();
        }

        private void SetEnabledStates()
        {
            DataRowView[] SelectedGridRow = grdResult.SelectedDataRowsAsDataRowView;

            if (SelectedGridRow.Length >= 1)
            {
                string status = SelectedGridRow[0]["Status"].ToString();
                bool canReverse = (status == MFinanceConstants.AP_DOCUMENT_POSTED) || (status.Length == 0);
                bool canCancel = (status == MFinanceConstants.AP_DOCUMENT_OPEN) || (status == MFinanceConstants.AP_DOCUMENT_APPROVED);

                // Payments can be reversed as can posted invoices
                ActionEnabledEvent(null, new ActionEventArgs("actReverseSelected", canReverse));
                ActionEnabledEvent(null, new ActionEventArgs("actCancelSelected", canCancel));

                FMainForm.ActionEnabledEvent(null, new ActionEventArgs("actTransactionReverseSelected", canReverse));
                FMainForm.ActionEnabledEvent(null, new ActionEventArgs("actTransactionCancelSelected", canCancel));
            }
            else
            {
                ActionEnabledEvent(null, new ActionEventArgs("actReverseSelected", false));
                ActionEnabledEvent(null, new ActionEventArgs("actCancelSelected", false));
            }
        }

        /// <summary>
        /// Method required by IGridBase.
        /// </summary>
        public void SelectRowInGrid(int ARowNumber)
        {
            SelectAndFocus(ARowNumber);
        }

        /// <summary>Use this to avoid the form pulling forward when the data is loaded.</summary>
        /// <param name="ARowNumber"></param>
        /// <param name="AndFocus"></param>
        public void SelectAndFocus(int ARowNumber, Boolean AndFocus = true)
        {
            if (ARowNumber >= grdDetails.Rows.Count)
            {
                ARowNumber = grdDetails.Rows.Count - 1;
            }

            if ((ARowNumber < 1) && (grdDetails.Rows.Count > 1))
            {
                ARowNumber = 1;
            }

            // Note:  We need to be sure to focus column 1 in this case because sometimes column 0 is not visible!!
            if (AndFocus)
            {
                grdDetails.Selection.Focus(new SourceGrid.Position(ARowNumber, 1), true);
            }
            else
            {
                grdDetails.SelectRowWithoutFocus(ARowNumber);
            }

            FPrevRowChangedRow = ARowNumber;
        }

        private void ApplyFilterManual(ref string AFilter)
        {
            if (grdDetails.Columns.Count == 0)
            {
                return;
            }

            if (FPagedDataTable != null)
            {
                FPagedDataTable.DefaultView.RowFilter = AFilter;
            }

            bool gotRows = (grdDetails.Rows.Count > 1);
            bool canApprove = ((RadioButton)FFilterAndFindObject.FilterPanelControls.FindControlByName("rbtForApproval")).Checked && gotRows;
            bool canPost = ((RadioButton)FFilterAndFindObject.FilterPanelControls.FindControlByName("rbtForPosting")).Checked && gotRows;
            bool canPay = ((RadioButton)FFilterAndFindObject.FilterPanelControls.FindControlByName("rbtForPaying")).Checked && gotRows;

            bool canTag = canApprove || canPost || canPay;

            ActionEnabledEvent(null, new ActionEventArgs("actOpenSelected", gotRows));
            ActionEnabledEvent(null, new ActionEventArgs("actOpenTagged", canTag));
            ActionEnabledEvent(null, new ActionEventArgs("actRunTagAction", canTag));
            ActionEnabledEvent(null, new ActionEventArgs("actTagAll", canTag));
            ActionEnabledEvent(null, new ActionEventArgs("actUntagAll", canTag));

            FMainForm.ActionEnabledEvent(null, new ActionEventArgs("actTransactionOpenSelected", gotRows));
            FMainForm.ActionEnabledEvent(null, new ActionEventArgs("actTransactionOpenTagged", canTag));
            FMainForm.ActionEnabledEvent(null, new ActionEventArgs("actTransactionApproveTagged", canApprove));
            FMainForm.ActionEnabledEvent(null, new ActionEventArgs("actTransactionPostTagged", canPost));
            FMainForm.ActionEnabledEvent(null, new ActionEventArgs("actTransactionAddTaggedToPayment", canPay));

            grdDetails.Columns[0].Visible = canTag;

            if (canTag)
            {
                grdDetails.ShowCell(new SourceGrid.Position(grdDetails.Selection.ActivePosition.Row, 0), true);
            }

            if (canApprove)
            {
                btnRunTagAction.Text = "Appro&ve Tagged";
                FPetraUtilsObject.SetStatusBarText(btnRunTagAction, Catalog.GetString("Click to approve the tagged items"));
            }
            else if (canPost)
            {
                btnRunTagAction.Text = "&Post Tagged";
                FPetraUtilsObject.SetStatusBarText(btnRunTagAction, Catalog.GetString("Click to post the tagged items"));
            }
            else if (canPay)
            {
                btnRunTagAction.Text = "Pa&y Tagged";
                FPetraUtilsObject.SetStatusBarText(btnRunTagAction, Catalog.GetString("Click to pay the tagged items"));
            }
            else
            {
                btnRunTagAction.Text = "Pa&y Tagged";
            }

            UpdateDisplayedBalance();
        }

        private bool IsMatchingRowManual(DataRow ARow)
        {
            string transactionType = ((TCmbAutoComplete)FFilterAndFindObject.FindPanelControls.FindControlByName("cmbTransactionType")).Text;

            if (transactionType != String.Empty)
            {
                if (!ARow["Type"].ToString().Contains(transactionType))
                {
                    return false;
                }
            }

            string status = ((TCmbAutoComplete)FFilterAndFindObject.FindPanelControls.FindControlByName("cmbStatus")).Text;

            if (status != String.Empty)
            {
                if (!ARow["Status"].ToString().Contains(status))
                {
                    return false;
                }
            }

            DateTime dt;
            TtxtPetraDate fromDate = (TtxtPetraDate)FFilterAndFindObject.FindPanelControls.FindControlByName("dtpDate-1");

            if ((fromDate.Text != String.Empty) && DateTime.TryParse(fromDate.Text, out dt))
            {
                if (Convert.ToDateTime(ARow["Date"]) < dt.Date)
                {
                    return false;
                }
            }

            TtxtPetraDate toDate = (TtxtPetraDate)FFilterAndFindObject.FindPanelControls.FindControlByName("dtpDate-2");

            if ((toDate.Text != String.Empty) && DateTime.TryParse(toDate.Text, out dt))
            {
                if (Convert.ToDateTime(ARow["Date"]) > dt.Date)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Load the supplier and all the transactions (invoices and payments) that relate to it.
        /// </summary>
        /// <param name="ALedgerNumber"></param>
        /// <param name="APartnerKey"></param>
        public void LoadSupplier(Int32 ALedgerNumber, Int64 APartnerKey)
        {
            if ((ALedgerNumber == FLedgerNumber) && (APartnerKey == FPartnerKey) && !FIsInvoiceDataChanged)
            {
                // We already have the details loaded for this supplier, so we just put the focus on the grid...
                // However we have to do that after a short delay because we won't be starting a thread to get the data in the grid
                System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(Timer_Elapsed), null, 0, 250);
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            FLedgerNumber = ALedgerNumber;
            FPartnerKey = APartnerKey;
            FMainDS = TRemote.MFinance.AP.WebConnectors.LoadAApSupplier(ALedgerNumber, APartnerKey);

            FSupplierRow = FMainDS.AApSupplier[0];

            txtFilteredBalance.CurrencyCode = FSupplierRow.CurrencyCode;
            txtSupplierBalance.CurrencyCode = FSupplierRow.CurrencyCode;
            txtTaggedBalance.CurrencyCode = FSupplierRow.CurrencyCode;
            lblExcludedItems.Text = string.Format(lblExcludedItems.Text, FSupplierRow.CurrencyCode);

            // Get our AP ledger settings and enable/disable the corresponding search option on the filter panel
            TFrmLedgerSettingsDialog settings = new TFrmLedgerSettingsDialog(this.ParentForm, ALedgerNumber);
            FRequireApprovalBeforePosting = settings.APRequiresApprovalBeforePosting;
            Control rbtForApproval = FFilterAndFindObject.FilterPanelControls.FindControlByName("rbtForApproval");
            rbtForApproval.Enabled = FRequireApprovalBeforePosting;

            //
            // Transactions older than
            DateTime AgedOlderThan = DateTime.Now;

            if (!FSupplierRow.IsPreferredScreenDisplayNull())
            {
                AgedOlderThan = AgedOlderThan.AddMonths(0 - FSupplierRow.PreferredScreenDisplay);
            }

            FAgedOlderThan = AgedOlderThan.ToString("u");

            grpOutstandingTotals.Text = "Outstanding Totals for " + FMainDS.PPartner[0].PartnerShortName;
            txtSupplierName.Text = FMainDS.PPartner[0].PartnerShortName;
            txtSupplierCurrency.Text = FSupplierRow.CurrencyCode;
            FFindObject = TRemote.MFinance.AP.UIConnectors.Find();

            FFindObject.FindSupplierTransactions(FLedgerNumber, FPartnerKey);

            // Start thread that checks for the end of the search operation on the PetraServer
            FKeepUpSearchFinishedCheck = true;
            Thread FinishedCheckThread = new Thread(new ThreadStart(SearchFinishedCheckThread));
            FinishedCheckThread.Start();
        }

        //
        // Called from LoadSupplier after a short delay of 250ms.

        private void Timer_Elapsed(object state)
        {
            if (this.IsDisposed)
            {
                return;
            }

            // Now we can focus the grid
            if (InvokeRequired)
            {
                Invoke(new SimpleDelegate(FocusGrid), null);
            }
            else
            {
                FocusGrid();
            }
        }

        private void FocusGrid()
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.Focused) // I want to hand my focus to the grid,
            {                 // but if another form is currently in focus, don't usurp that.
                grdDetails.Focus();
            }
        }

        private void grdResult_DataPageLoaded(object Sender, TDataPageLoadEventArgs e)
        {
            // This is where we end up after querying the database and loading the first data into the grid
            // We are back in our main thread here
            this.Cursor = Cursors.Default;
        }

        private delegate void SimpleDelegate();

        /// <summary>
        /// Thread for the search operation. Monitor's the Server System.Object's
        /// AsyncExecProgress.ProgressState and invokes UI updates from that.
        ///
        /// </summary>
        /// <returns>void</returns>
        private void SearchFinishedCheckThread()
        {
            TProgressState ThreadStatus;

            // Check whether this thread should still execute
            while (FKeepUpSearchFinishedCheck)
            {
                // Wait and see if anything has changed
                Thread.Sleep(200);

                try
                {
                    /* The next line of code calls a function on the PetraServer
                     * > causes a bit of data traffic everytime! */
                    ThreadStatus = FFindObject.Progress;
                }
                catch (NullReferenceException)
                {
                    // The form is closing on the main thread ...
                    return;         // end this thread
                }
                catch (Exception)
                {
                    throw;
                }

                if (ThreadStatus.JobFinished)
                {
                    FKeepUpSearchFinishedCheck = false;

                    try
                    {
                        // see also http://stackoverflow.com/questions/6184/how-do-i-make-event-callbacks-into-my-win-forms-thread-safe
                        if (InvokeRequired)
                        {
                            Invoke(new SimpleDelegate(FinishThread));
                        }
                        else
                        {
                            FinishThread();
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // Another exception that can be caused when the main screen is closed while running this thread
                        return;
                    }
                }
                else if (ThreadStatus.CancelJob)
                {
                    FKeepUpSearchFinishedCheck = false;
                    return;
                }

                // Loop again while FKeepUpSearchFinishedCheck is true ...
            }
        }

        private void FinishThread()
        {
            // Fetch the first page of data
            try
            {
                grdResult.MinimumPageSize = 200;
                FPagedDataTable = grdResult.LoadFirstDataPage(@GetDataPagedResult);
            }
            catch (Exception E)
            {
                MessageBox.Show(E.ToString());
            }

            InitialiseGrid();

            // Fix for SQLite: it returns a DataColumn of DataType 'double' for the 'Amount' and 'OutstandingAmount' DataColumns,
            // which the 'AddCurrencyColumn' of TSgrdDataGrid can't deal with!
            if (FPagedDataTable.Columns["Amount"].DataType == typeof(double))
            {
                DataUtilities.ChangeDataColumnDataType(FPagedDataTable, "Amount", typeof(decimal));
            }

            if ((FPagedDataTable.Columns["OutstandingAmount"].DataType == typeof(double))
                || (FPagedDataTable.Columns["OutstandingAmount"].DataType == typeof(System.Int64)))  // Int64 only when no outstanding amount, else double
            {
                DataUtilities.ChangeDataColumnDataType(FPagedDataTable, "OutstandingAmount", typeof(decimal));
            }

            DataView myDataView = FPagedDataTable.DefaultView;
            myDataView.AllowNew = false;

            grdResult.DataSource = new DevAge.ComponentModel.BoundDataView(myDataView);
            grdResult.Visible = true;
            UpdateRowFilter();
            string currentFilter = FFilterAndFindObject.CurrentActiveFilter;
            ApplyFilterManual(ref currentFilter);

            if (grdResult.TotalPages > 0)
            {
                //
                // I don't want to do either of these things, if I'm being called from a child form
                // that's just been saved..
                //              grdResult.BringToFront();
                //              grdResult.Focus();

                if (grdResult.TotalPages > 1)
                {
                    grdResult.LoadAllDataPages();
                }

                // Highlight first Row
                SelectAndFocus(1, false);
            }

            grdResult.AutoResizeGrid();

            grdResult.SetHeaderTooltip(0, Catalog.GetString("Check to Tag"));
            grdResult.SetHeaderTooltip(1, Catalog.GetString("Inv#"));
            grdResult.SetHeaderTooltip(2, Catalog.GetString("Type"));
            grdResult.SetHeaderTooltip(3, Catalog.GetString("Amount"));
            grdResult.SetHeaderTooltip(4, Catalog.GetString("Outstanding"));
            grdResult.SetHeaderTooltip(5, Catalog.GetString("Currency"));
            grdResult.SetHeaderTooltip(6, Catalog.GetString("Status"));
            grdResult.SetHeaderTooltip(7, Catalog.GetString("Date"));

            UpdateSupplierBalance();
            UpdateDisplayedBalance();
            UpdateRecordNumberDisplay();
            RefreshSumTagged(null, null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ANeededPage"></param>
        /// <param name="APageSize"></param>
        /// <param name="ATotalRecords"></param>
        /// <param name="ATotalPages"></param>
        /// <returns></returns>
        private DataTable GetDataPagedResult(Int16 ANeededPage, Int16 APageSize, out Int32 ATotalRecords, out Int16 ATotalPages)
        {
            ATotalRecords = 0;
            ATotalPages = 0;
            DataTable ResultsTable = new DataTable();

            ResultsTable = FFindObject.GetDataPagedResult(ANeededPage, APageSize, out ATotalRecords, out ATotalPages);
//          ResultsTable.Columns.Add("DiscountMsg", typeof(string));
            ResultsTable.Columns.Add("Tagged", typeof(bool));

            foreach (DataRow Row in ResultsTable.Rows)
            {
                Row["Tagged"] = false;

                if ((Row["Type"].ToString() == "Invoice") && (Row["CreditNote"].Equals(true)))
                {
                    Row["Type"] = "Credit Note";
                    Row["Amount"] = -Convert.ToDecimal(Row["Amount"]);
                    Row["OutstandingAmount"] = -Convert.ToDecimal(Row["OutstandingAmount"]);
                }

/*
 *                  Int32 DiscountDays = (Int32)Row["DiscountDays"];
 *
 *                  if (DiscountDays > 0)
 *                  {
 *                      DateTime DiscountUntil = (DateTime)Row["Date"];
 *                      DiscountUntil = DiscountUntil.AddDays(DiscountDays);
 *                      Row["DiscountMsg"] =
 *                          String.Format("{0:n0}% until {1}", (Decimal)Row["DiscountPercent"], TDate.DateTimeToLongDateString2(DiscountUntil));
 *                  }
 *                  else
 *                  {
 *                      Row["DiscountMsg"] = "None";
 *                  }
 */
            }

            return ResultsTable;
        }

        private void InitialiseGrid()
        {
            grdResult.Columns.Clear();
            grdResult.AddCheckBoxColumn("", FPagedDataTable.Columns["Tagged"], -1, false);
//          grdResult.AddTextColumn("AP#", FPagedDataTable.Columns["ApNum"]);
            grdResult.AddTextColumn(Catalog.GetString("Inv#"), FPagedDataTable.Columns["InvNum"]);
            grdResult.AddTextColumn(Catalog.GetString("Type"), FPagedDataTable.Columns["Type"]);
            grdResult.AddCurrencyColumn(Catalog.GetString("Amount"), FPagedDataTable.Columns["Amount"]);
            grdResult.AddCurrencyColumn(Catalog.GetString("Outstanding"), FPagedDataTable.Columns["OutstandingAmount"]);
            grdResult.AddTextColumn(Catalog.GetString("Currency"), FPagedDataTable.Columns["Currency"]);
//          grdResult.AddTextColumn("Discount", FPagedDataTable.Columns["DiscountMsg"]);
            grdResult.AddTextColumn(Catalog.GetString("Status"), FPagedDataTable.Columns["Status"]);
            grdResult.AddDateColumn(Catalog.GetString("Date"), FPagedDataTable.Columns["Date"]);
        }

        private void UpdateDisplayedBalance()
        {
            DevAge.ComponentModel.BoundDataView dv = (DevAge.ComponentModel.BoundDataView)grdResult.DataSource;
            txtFilteredBalance.NumberValueDecimal = UpdateBalance(dv.DataView);
        }

        private void UpdateSupplierBalance()
        {
            DataView dv = new DataView(FPagedDataTable);

            txtSupplierBalance.NumberValueDecimal = UpdateBalance(dv);
        }

        private Decimal UpdateBalance(DataView ADataView)
        {
            Decimal balance = 0.0m;

            if (FPagedDataTable != null)
            {
                foreach (DataRowView rv in ADataView)
                {
                    DataRow Row = rv.Row;

                    if ((Row["Currency"].ToString() == txtSupplierCurrency.Text)
                        && (Row["OutstandingAmount"].GetType() == typeof(Decimal))
                        && (Row["Status"].ToString() != MFinanceConstants.AP_DOCUMENT_CANCELLED))
                    {
                        balance += (Decimal)Row["OutstandingAmount"];
                    }
                }
            }

            return balance;
        }

        private void UpdateRowFilter()
        {
            if (FPagedDataTable != null)
            {
                string filter = String.Format("(Currency='{0}')", txtSupplierCurrency.Text);
                string filterJoint = " AND ";

                if ((FStatusFilter.Length > 0) && (filter.Length > 0))
                {
                    filter += filterJoint;
                }

                filter += FStatusFilter;

                if ((FTypeFilter.Length > 0) && (filter.Length > 0))
                {
                    filter += filterJoint;
                }

                filter += FTypeFilter;

                if ((FHistoryFilter.Length > 0) && (filter.Length > 0))
                {
                    filter += filterJoint;
                }

                filter += FHistoryFilter;

                FFilterAndFindObject.FilterPanelControls.SetBaseFilter(filter, filter.Length == 0);
            }
        }

        /// <summary>
        /// This will re-draw the form, so that any data changes are shown.
        /// </summary>
        public void Reload()
        {
            FMainForm.IsInvoiceDataChanged = true;
            LoadSupplier(FLedgerNumber, FPartnerKey);
        }

        private void DoRefreshButton(Object Sender, EventArgs e)
        {
            Reload();
        }

        // Called from a timer, below, so that the default processing of
        // the grid control completes before I get called.
        private void RefreshSumTagged(Object Sender, EventArgs e)
        {
            // If I was called from a timer, kill that now:
            if (Sender != null)
            {
                ((System.Windows.Forms.Timer)Sender).Stop();
            }

            // if there's no results table yet, I can't do this...
            if (FPagedDataTable == null)
            {
                return;
            }

            FPrevRowChangedRow = grdResult.Selection.ActivePosition.Row;

            Decimal TotalSelected = 0;
            int TaggedItemCount = 0;

            foreach (DataRowView rv in FPagedDataTable.DefaultView)
            {
                DataRow Row = rv.Row;

                if (Row["Tagged"].Equals(true))
                {
                    TaggedItemCount++;

                    if (Row["Type"].ToString() != "Payment")
                    {
                        TotalSelected += (Decimal)(Row["OutstandingAmount"]);
                    }
                }
            }

            txtTaggedBalance.NumberValueDecimal = TotalSelected;
            txtTaggedCount.NumberValueInt = TaggedItemCount;
        }

        private void grdResult_Click(object sender, EventArgs e)
        {
            // I want to update the total tagged field,
            // but it needs to be performed AFTER the default processing so I'm using a timer.
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

            timer.Tick += new EventHandler(RefreshSumTagged);
            timer.Interval = 100;
            timer.Start();
        }

        private void StatusFilterChange(object sender, EventArgs e)
        {
            FStatusFilter = String.Empty;
            string filterJoint = " AND ";

            String SelectedItem = ((TCmbAutoComplete)FFilterAndFindObject.FilterPanelControls.FindControlByName("cmbStatus")).Text;

            if (SelectedItem != String.Empty)
            {
                FStatusFilter = "(Status='" + SelectedItem + "')";
            }

            RadioButton rbtForApproval = (RadioButton)FFilterAndFindObject.FilterPanelControls.FindControlByName("rbtForApproval");

            if (rbtForApproval.Checked)
            {
                if (FStatusFilter != String.Empty)
                {
                    FStatusFilter += filterJoint;
                }

                FStatusFilter += ("(Status='OPEN')");
            }

            RadioButton rbtForPosting = (RadioButton)FFilterAndFindObject.FilterPanelControls.FindControlByName("rbtForPosting");

            if (rbtForPosting.Checked)
            {
                if (FStatusFilter != String.Empty)
                {
                    FStatusFilter += filterJoint;
                }

                if (FRequireApprovalBeforePosting)
                {
                    FStatusFilter += ("(Status='APPROVED')");
                }
                else
                {
                    FStatusFilter += ("(Status='OPEN' OR Status='APPROVED')");
                }
            }

            RadioButton rbtForPaying = (RadioButton)FFilterAndFindObject.FilterPanelControls.FindControlByName("rbtForPaying");

            if (rbtForPaying.Checked)
            {
                if (FStatusFilter != String.Empty)
                {
                    FStatusFilter += filterJoint;
                }

                FStatusFilter += ("(Status='POSTED' OR Status='PARTPAID')");
            }

            UpdateRowFilter();
        }

        private void TypeFilterChange(object sender, EventArgs e)
        {
            FTypeFilter = "";

            String SelectedItem = ((TCmbAutoComplete)FFilterAndFindObject.FilterPanelControls.FindControlByName("cmbTransactionType")).Text;

            if (SelectedItem != String.Empty)
            {
                FTypeFilter = "Type='" + SelectedItem + "'";
            }

            UpdateRowFilter();
        }

        private void HistoryFilterChange(object sender, EventArgs e)
        {
            FHistoryFilter = String.Empty;

            RadioButton rbtRecent = (RadioButton)FFilterAndFindObject.FilterPanelControls.FindControlByName("rbtRecent");

            if (rbtRecent.Checked)
            {
                FHistoryFilter = ("(Date >'" + FAgedOlderThan + "')");
            }

            RadioButton rbtQuarter = (RadioButton)FFilterAndFindObject.FilterPanelControls.FindControlByName("rbtLastQuarter");

            if (rbtQuarter.Checked)
            {
                if (FHistoryFilter != "")
                {
                    FHistoryFilter += " AND ";
                }

                FHistoryFilter += ("(Date > #" + DateTime.Now.AddMonths(-3).ToString("d", System.Globalization.CultureInfo.InvariantCulture) + "#)");
            }

            RadioButton rbtHalf = (RadioButton)FFilterAndFindObject.FilterPanelControls.FindControlByName("rbtLastSixMonths");

            if (rbtHalf.Checked)
            {
                if (FHistoryFilter != "")
                {
                    FHistoryFilter += " AND ";
                }

                FHistoryFilter += ("(Date > #" + DateTime.Now.AddMonths(-6).ToString("d", System.Globalization.CultureInfo.InvariantCulture) + "#)");
            }

            RadioButton rbtYear = (RadioButton)FFilterAndFindObject.FilterPanelControls.FindControlByName("rbtLastYear");

            if (rbtYear.Checked)
            {
                if (FHistoryFilter != "")
                {
                    FHistoryFilter += " AND ";
                }

                FHistoryFilter += ("(Date > #" + DateTime.Now.AddMonths(-12).ToString("d", System.Globalization.CultureInfo.InvariantCulture) + "#)");
            }

            UpdateRowFilter();
        }

        private void TagAll(object sender, EventArgs e)
        {
            // Untag everything
            UntagAll(null, null);

            // Now tag all the rows in the current view
            foreach (DataRowView rv in FPagedDataTable.DefaultView)
            {
                rv.Row["Tagged"] = true;
            }

            RefreshSumTagged(null, null);
        }

        private void UntagAll(object sender, EventArgs e)
        {
            // We do this for all tags in the complete data table
            foreach (DataRow Row in FPagedDataTable.Rows)
            {
                Row["Tagged"] = false;
            }

            RefreshSumTagged(null, null);
        }

        // Opens an individual document or payment
        private void OpenADocumentOrPayment(DataRowView ADataRow)
        {
            if (ADataRow["Status"].ToString().Length > 0) // invoices have status, and payments don't.
            {
                Int32 DocumentId = Convert.ToInt32(ADataRow["ApDocumentId"]);
                TFrmAPEditDocument frm = new TFrmAPEditDocument(this.ParentForm);

                if (frm.LoadAApDocument(FLedgerNumber, DocumentId))
                {
                    frm.Show();
                }
            }
            else
            {
                Int32 PaymentNumber = Convert.ToInt32(ADataRow["ApNum"]);
                TFrmAPPayment frm = new TFrmAPPayment(this.ParentForm);
                frm.ReloadPayment(FLedgerNumber, PaymentNumber);
                frm.Show();
            }
        }

        /// Opens all tagged documents
        public void OpenTaggedDocuments(System.Object sender, EventArgs args)
        {
            if (FPagedDataTable.DefaultView.Count > 0)
            {
                this.Cursor = Cursors.WaitCursor;

                foreach (DataRowView rv in FPagedDataTable.DefaultView)
                {
                    if (rv.Row["Tagged"].Equals(true))
                    {
                        OpenADocumentOrPayment(rv);
                    }
                }

                this.Cursor = Cursors.Default;
            }
        }

        /// Open the highlighted transaction - called by menu or grid click etc
        public void OpenSelectedTransaction(System.Object sender, EventArgs args)
        {
            DataRowView[] SelectedGridRow = grdResult.SelectedDataRowsAsDataRowView;

            if (SelectedGridRow.Length >= 1)
            {
                this.Cursor = Cursors.WaitCursor;
                OpenADocumentOrPayment(SelectedGridRow[0]);
                this.Cursor = Cursors.Default;
            }
        }

        /// Cancel the selected transaction.  This actually sets the document status to CANCELLED
        public void CancelSelected(object sender, EventArgs e)
        {
            DataRowView[] SelectedGridRow = grdResult.SelectedDataRowsAsDataRowView;

            if (SelectedGridRow.Length >= 1)
            {
                if (MessageBox.Show(Catalog.GetString("Cancel the selected invoice?"),
                        Catalog.GetString("Cancel Invoice"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    return;
                }

                // I can only cancel invoices that are not posted already.
                // This method is only enabled when the grid shows items for Posting
                List <int>CancelTheseDocs = new List <int>();

                string status = SelectedGridRow[0]["Status"].ToString();

                if ((status == MFinanceConstants.AP_DOCUMENT_OPEN) || (status == MFinanceConstants.AP_DOCUMENT_APPROVED))
                {
                    Int32 DocumentId = Convert.ToInt32(SelectedGridRow[0]["ApDocumentId"]);
                    CancelTheseDocs.Add(DocumentId);

                    this.Cursor = Cursors.WaitCursor;
                    TRemote.MFinance.AP.WebConnectors.CancelAPDocuments(FLedgerNumber, CancelTheseDocs, false);
                    Reload();
                    this.Cursor = Cursors.Default;
                }
            }
        }

        /// Reverse the selected transaction
        public void ReverseSelected(object sender, EventArgs e)
        {
            // This will throw an exception if insufficient permissions
            TSecurityChecks.CheckUserModulePermissions("FINANCE-2", "ReverseSelected [raised by Client Proxy for ModuleAccessManager]");

            DataRowView[] SelectedGridRow = grdResult.SelectedDataRowsAsDataRowView;

            if (SelectedGridRow.Length >= 1)
            {
                if (SelectedGridRow[0]["Status"].ToString().Length > 0) // invoices have status, and payments don't.
                {  // Reverse invoice to a previous (unposted) state
                    string barstatus = "|" + SelectedGridRow[0]["Status"].ToString();

                    if (barstatus == "|POSTED")
                    {
                        TVerificationResultCollection Verifications;
                        Int32 DocumentId = Convert.ToInt32(SelectedGridRow[0]["ApDocumentId"]);
                        List <Int32>ApDocumentIds = new List <Int32>();
                        ApDocumentIds.Add(DocumentId);

                        TDlgGLEnterDateEffective dateEffectiveDialog = new TDlgGLEnterDateEffective(
                            FLedgerNumber,
                            Catalog.GetString("Select reversal date"),
                            Catalog.GetString("The date effective for this reversal") + ":");

                        if (dateEffectiveDialog.ShowDialog() != DialogResult.OK)
                        {
                            MessageBox.Show(Catalog.GetString("Reversal was cancelled."), Catalog.GetString(
                                    "No Success"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        DateTime PostingDate = dateEffectiveDialog.SelectedDate;
                        Int32 glBatchNumber;

                        if (TRemote.MFinance.AP.WebConnectors.PostAPDocuments(
                                FLedgerNumber,
                                ApDocumentIds,
                                PostingDate,
                                true,
                                out glBatchNumber,
                                out Verifications))
                        {
                            if (glBatchNumber >= 0)
                            {
                                TFrmBatchPostingRegister ReportGui = new TFrmBatchPostingRegister(null);
                                ReportGui.PrintReportNoUi(FLedgerNumber, glBatchNumber);
                            }

                            System.Windows.Forms.MessageBox.Show("Invoice reversed to Approved status.", Catalog.GetString("Reversal"));
                            Reload();
                            return;
                        }
                        else
                        {
                            string ErrorMessages = String.Empty;

                            foreach (TVerificationResult verif in Verifications)
                            {
                                ErrorMessages += "[" + verif.ResultContext + "] " +
                                                 verif.ResultTextCaption + ": " +
                                                 verif.ResultText + Environment.NewLine;
                            }

                            System.Windows.Forms.MessageBox.Show(ErrorMessages, Catalog.GetString("Reversal"));
                        }

                        return;
                    } // reverse posted invoice

                    if ("|PAID|PARTPAID".IndexOf(barstatus) >= 0)
                    {
                        MessageBox.Show("Can't reverse a paid invoice. Reverse the payment instead.", "Reverse");
                    }
                }
                else // Reverse payment
                {
                    Int32 PaymentNum = (Int32)SelectedGridRow[0]["ApNum"];
                    TFrmAPPayment PaymentScreen = new TFrmAPPayment(this.ParentForm);
                    PaymentScreen.ReversePayment(FLedgerNumber, PaymentNum);
                }
            }
        }

        /// <summary>
        /// Create a new invoice
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CreateInvoice(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            TFrmAPEditDocument frm = new TFrmAPEditDocument(this.ParentForm);

            frm.CreateAApDocument(FLedgerNumber, FPartnerKey, false);
            frm.Show();

            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Create a new credit note
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CreateCreditNote(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            TFrmAPEditDocument frm = new TFrmAPEditDocument(this.ParentForm);

            frm.CreateAApDocument(FLedgerNumber, FPartnerKey, true);
            frm.Show();

            this.Cursor = Cursors.Default;
        }

        /// Run the tag action depending on the state of the filter panel radio button
        public void RunTagAction(object sender, EventArgs e)
        {
            if (((RadioButton)FFilterAndFindObject.FilterPanelControls.FindControlByName("rbtForApproval")).Checked)
            {
                ApproveTaggedDocuments(sender, e);
            }
            else if (((RadioButton)FFilterAndFindObject.FilterPanelControls.FindControlByName("rbtForPosting")).Checked)
            {
                PostTaggedDocuments(sender, e);
            }
            else if (((RadioButton)FFilterAndFindObject.FilterPanelControls.FindControlByName("rbtForPaying")).Checked)
            {
                AddTaggedToPayment(sender, e);
            }
        }

        /// <summary>
        /// Approve all tagged documents
        /// Uses static functions from TFrmAPEditDocument??
        /// Not yet implemented
        /// </summary>
        public void ApproveTaggedDocuments(object sender, EventArgs e)
        {
            string MsgTitle = Catalog.GetString("Document Approval");

            List <Int32>TaggedDocuments = new List <Int32>();

            foreach (DataRowView rv in FPagedDataTable.DefaultView)
            {
                if ((rv.Row["Tagged"].Equals(true)) && (rv.Row["Status"].ToString().Length > 0)   // Invoices have status, Payments don't.
                    && (rv.Row["Status"].ToString() == MFinanceConstants.AP_DOCUMENT_OPEN)
                    && (rv.Row["Currency"].ToString() == txtSupplierCurrency.Text)
                    )
                {
                    TaggedDocuments.Add(Convert.ToInt32(rv.Row["ApDocumentId"]));
                }
            }

            if (TaggedDocuments.Count > 0)
            {
                string msg = String.Format(Catalog.GetString(
                        "Are you sure that you want to approve the {0} tagged document(s)?"), TaggedDocuments.Count);

                if (MessageBox.Show(msg, MsgTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    return;
                }

                this.Cursor = Cursors.WaitCursor;
                TVerificationResultCollection verificationResult;

                if (TRemote.MFinance.AP.WebConnectors.ApproveAPDocuments(FLedgerNumber, TaggedDocuments, out verificationResult))
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show(Catalog.GetString("The tagged documents have been approved successfully!"), MsgTitle);
                    Reload();
                }
                else
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show(verificationResult.BuildVerificationResultString(), MsgTitle);
                }
            }
            else
            {
                MessageBox.Show(Catalog.GetString("There we no tagged documents to be approved."), MsgTitle);
            }
        }

        /// <summary>
        /// Post all tagged documents in one GL Batch
        /// Uses static functions from TFrmAPEditDocument
        /// </summary>
        public void PostTaggedDocuments(object sender, EventArgs e)
        {
            // This will throw an exception if insufficient permissions
            TSecurityChecks.CheckUserModulePermissions("FINANCE-2", "PostTaggedDocuments [raised by Client Proxy for ModuleAccessManager]");

            List <Int32>TaggedDocuments = new List <Int32>();
            AccountsPayableTDS TempDS = new AccountsPayableTDS();

            foreach (DataRowView rv in FPagedDataTable.DefaultView)
            {
                if ((rv.Row["Tagged"].Equals(true)) && (rv.Row["Status"].ToString().Length > 0)   // Invoices have status, Payments don't.
                    && ("|CANCELLED|POSTED|PARTPAID|PAID".IndexOf("|" + rv.Row["Status"].ToString()) < 0)
                    && (rv.Row["Currency"].ToString() == txtSupplierCurrency.Text)
                    )
                {
                    Int32 DocumentId = Convert.ToInt32(rv.Row["ApDocumentId"]);
                    TempDS.Merge(TRemote.MFinance.AP.WebConnectors.LoadAApDocument(FLedgerNumber, DocumentId));

                    // I've loaded this record in my DS, but I was not given a handle to it, so I need to find it!
                    TempDS.AApDocument.DefaultView.Sort = "a_ap_document_id_i";
                    Int32 Idx = TempDS.AApDocument.DefaultView.Find(DocumentId);
                    AApDocumentRow DocumentRow = TempDS.AApDocument[Idx];

                    if (TFrmAPEditDocument.ApDocumentCanPost(TempDS, DocumentRow))
                    {
                        TaggedDocuments.Add(DocumentId);
                    }
                }
            }

            if (TaggedDocuments.Count == 0)
            {
                return;
            }

            if (TFrmAPEditDocument.PostApDocumentList(TempDS, FLedgerNumber, TaggedDocuments, this.ParentForm))
            {
                // TODO: print reports on successfully posted batch
                MessageBox.Show(Catalog.GetString("The AP documents have been posted successfully!"));

                // TODO: show posting register of GL Batch?
                Reload();
            }
        }

        /// Add all selected invoices to the payment list and show that list so that the user can make the payment
        public void AddTaggedToPayment(object sender, EventArgs e)
        {
            // This will throw an exception if insufficient permissions
            TSecurityChecks.CheckUserModulePermissions("FINANCE-2", "AddTaggedToPayment [raised by Client Proxy for ModuleAccessManager]");

            List <Int32>TaggedDocuments = new List <Int32>();
            AccountsPayableTDS TempDS = new AccountsPayableTDS();

            foreach (DataRowView rv in FPagedDataTable.DefaultView)
            {
                if (
                    (rv.Row["Tagged"].Equals(true))
                    && (rv.Row["Currency"].ToString() == txtSupplierCurrency.Text)
                    && ("|POSTED|PARTPAID|".IndexOf("|" + rv.Row["Status"].ToString()) >= 0)
                    )
                {
                    Int32 DocumentId = Convert.ToInt32(rv.Row["ApDocumentId"]);
                    TempDS.Merge(TRemote.MFinance.AP.WebConnectors.LoadAApDocument(FLedgerNumber, DocumentId));

                    // I've loaded this record in my DS, but I was not given a handle to it, so I need to find it!
                    TempDS.AApDocument.DefaultView.Sort = AApDocumentTable.GetApDocumentIdDBName();
                    Int32 Idx = TempDS.AApDocument.DefaultView.Find(DocumentId);
                    AApDocumentRow DocumentRow = TempDS.AApDocument[Idx];

                    if ("|POSTED|PARTPAID|".IndexOf("|" + DocumentRow["a_document_status_c"].ToString()) >= 0)
                    {
                        TaggedDocuments.Add(DocumentId);
                    }
                }
            }

            if (TaggedDocuments.Count == 0)
            {
                return;
            }

            TFrmAPPayment frm = new TFrmAPPayment(this.ParentForm);

            if (frm.AddDocumentsToPayment(TempDS, FLedgerNumber, TaggedDocuments))
            {
                frm.Show();
            }
        }

        private void PaymentReport(object sender, EventArgs e)
        {
            Int32 PaymentNumStart = -1;
            Int32 PaymentNumEnd = -1;

            DataRowView[] SelectedGridRows = grdResult.SelectedDataRowsAsDataRowView;

            foreach (DataRowView RowView in SelectedGridRows)
            {
                DataRow Row = RowView.Row;
                Int32 PaymentNum = Convert.ToInt32(Row["ApNum"]);

                if ((PaymentNumStart == -1) || (PaymentNum < PaymentNumStart))
                {
                    PaymentNumStart = PaymentNum;
                }

                if (PaymentNum > PaymentNumEnd)
                {
                    PaymentNumEnd = PaymentNum;
                }
            }

            TFrmAP_PaymentReport reporter = new TFrmAP_PaymentReport(this.ParentForm);
            reporter.LedgerNumber = FLedgerNumber;
            reporter.SetPaymentNumber(PaymentNumStart, PaymentNumEnd);
            reporter.Show();
        }

        private bool ProcessCmdKeyManual(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.L | Keys.Control))
            {
                grdDetails.Focus();
                return true;
            }

            if (keyData == (Keys.Home | Keys.Control))
            {
                SelectRowInGrid(1);
                return true;
            }

            if (keyData == ((Keys.Up | Keys.Control)))
            {
                SelectRowInGrid(FPrevRowChangedRow - 1);
                return true;
            }

            if (keyData == (Keys.Down | Keys.Control))
            {
                SelectRowInGrid(FPrevRowChangedRow + 1);
                return true;
            }

            if (keyData == ((Keys.End | Keys.Control)))
            {
                SelectRowInGrid(grdDetails.Rows.Count - 1);
                return true;
            }

            return false;
        }

        #region Forms Messaging Interface Implementation

        /// <summary>
        /// Will be called by TFormsList to inform any Form that is registered in TFormsList
        /// about any 'Forms Messages' that are broadcasted.
        /// </summary>
        /// <remarks>The Partner Edit 'listens' to such 'Forms Message' broadcasts by
        /// implementing this virtual Method. This Method will be called each time a
        /// 'Forms Message' broadcast occurs.
        /// </remarks>
        /// <param name="AFormsMessage">An instance of a 'Forms Message'. This can be
        /// inspected for parameters in the Method Body and the Form can use those to choose
        /// to react on the Message, or not.</param>
        /// <returns>Returns True if the Form reacted on the specific Forms Message,
        /// otherwise false.</returns>
        public bool ProcessFormsMessage(TFormsMessage AFormsMessage)
        {
            bool MessageProcessed = false;

            if (AFormsMessage.MessageClass == TFormsMessageClassEnum.mcAPTransactionChanged)
            {
                // The message is relevant if the supplier name is empty (=any supplier) or our specific supplier
                if ((((TFormsMessage.FormsMessageAP)AFormsMessage.MessageObject).SupplierName == this.lblSupplierName.Text)
                    || (((TFormsMessage.FormsMessageAP)AFormsMessage.MessageObject).SupplierName == String.Empty))
                {
                    // Reload the screen data
                    Reload();
                }

                MessageProcessed = true;
            }

            return MessageProcessed;
        }

        #endregion
    }
}
