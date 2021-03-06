﻿//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       wolfgangb
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
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using System.Threading;
using GNU.Gettext;
using Ict.Common;
using Ict.Common.Controls;
using Ict.Common.Data;
using Ict.Common.DB;
using Ict.Common.Verification;
using Ict.Petra.Client.App.Core;
using Ict.Petra.Client.App.Core.RemoteObjects;
using Ict.Petra.Client.App.Gui;
using Ict.Petra.Shared;
using Ict.Petra.Shared.MPartner;
using Ict.Petra.Shared.MPartner.Partner.Data;
using Ict.Petra.Shared.MPartner.Mailroom.Data;
using Ict.Petra.Client.MCommon;
using Ict.Petra.Client.MPartner;
using Ict.Petra.Client.CommonControls;
using Ict.Petra.Shared.MPartner.Validation;

namespace Ict.Petra.Client.MPartner.Gui.Extracts
{
    /// manual methods for the generated window
    public partial class TFrmUpdateExtractChangeSubscriptionDialog : System.Windows.Forms.Form
    {
        private PartnerEditTDS FMainDS;
        //private System.Drawing.Color ChangeControlBackgroundColor = System.Drawing.Color.Pink;
        private String ExtractName;

        /// <summary>
        /// set the initial value for passport name in the dialog
        /// </summary>
        /// <param name="AExtractName"></param>
        public void SetExtractName(String AExtractName)
        {
            ExtractName = AExtractName;
            lblExtractNameAndCreator.Text = Catalog.GetString("Extract Name: ") + AExtractName;
            lblExtractNameAndCreator.Font = new System.Drawing.Font(lblExtractNameAndCreator.Font.FontFamily.Name, 10, System.Drawing.FontStyle.Bold);
        }

        private void InitializeManualCode()
        {
            // show this dialog in center of screen
            this.StartPosition = FormStartPosition.CenterScreen;

            // remove validation handler for controls as we only want validation when clicking the ok button
            cmbPSubscriptionPublicationCode.Validated -= new System.EventHandler(this.ControlValidatedHandler);
            cmbPSubscriptionSubscriptionStatus.Validated -= new System.EventHandler(this.ControlValidatedHandler);
            chkPSubscriptionGratisSubscription.Validated -= new System.EventHandler(this.ControlValidatedHandler);

            txtPSubscriptionNumberComplimentary.Validated -= new System.EventHandler(this.ControlValidatedHandler);
            txtPSubscriptionPublicationCopies.Validated -= new System.EventHandler(this.ControlValidatedHandler);
            cmbPSubscriptionReasonSubsGivenCode.Validated -= new System.EventHandler(this.ControlValidatedHandler);
            cmbPSubscriptionReasonSubsCancelledCode.Validated -= new System.EventHandler(this.ControlValidatedHandler);
            txtPSubscriptionGiftFromKey.Validated -= new System.EventHandler(this.ControlValidatedHandler);

            dtpPSubscriptionStartDate.Validated -= new System.EventHandler(this.ControlValidatedHandler);
            dtpPSubscriptionExpiryDate.Validated -= new System.EventHandler(this.ControlValidatedHandler);
            dtpPSubscriptionDateCancelled.Validated -= new System.EventHandler(this.ControlValidatedHandler);
            dtpPSubscriptionDateNoticeSent.Validated -= new System.EventHandler(this.ControlValidatedHandler);
            dtpPSubscriptionSubscriptionRenewalDate.Validated -= new System.EventHandler(this.ControlValidatedHandler);
            dtpPSubscriptionFirstIssue.Validated -= new System.EventHandler(this.ControlValidatedHandler);
            dtpPSubscriptionLastIssue.Validated -= new System.EventHandler(this.ControlValidatedHandler);

            txtPSubscriptionNumberIssuesReceived.Validated -= new System.EventHandler(this.ControlValidatedHandler);


            FMainDS = new PartnerEditTDS();

            // now add the one subscription row to the DS that we are working with
            PSubscriptionTable SubscriptionTable = new PSubscriptionTable();
            FMainDS.Merge(SubscriptionTable);
            PSubscriptionRow SubscriptionRow = FMainDS.PSubscription.NewRowTyped(true);
            SubscriptionRow.PublicationCode = ""; // avoid NOT NULL error message
            FMainDS.PSubscription.Rows.Add(SubscriptionRow);

            FPetraUtilsObject.HasChanges = false;

            // initialize all check box sections so fields are disabled
            OnTickChangeItem(chkChangeSubscriptionStatus, null);
            OnTickChangeItem(chkChangeGratisSubscription, null);

            OnTickChangeItem(chkChangeNumberComplimentary, null);
            OnTickChangeItem(chkChangePublicationCopies, null);
            OnTickChangeItem(chkChangeReasonSubsGivenCode, null);
            OnTickChangeItem(chkChangeReasonSubsCancelledCode, null);
            OnTickChangeItem(chkChangeGiftFromKey, null);

            OnTickChangeItem(chkChangeStartDate, null);
            OnTickChangeItem(chkChangeExpiryDate, null);
            OnTickChangeItem(chkChangeRenewalDate, null);
            OnTickChangeItem(chkChangeDateNoticeSent, null);
            OnTickChangeItem(chkChangeDateCancelled, null);

            OnTickChangeItem(chkChangeNumberIssuesReceived, null);
            OnTickChangeItem(chkChangeFirstIssue, null);
            OnTickChangeItem(chkChangeLastIssue, null);

            // Hook up this event manually here after all initalisation has happened as otherwise
            // Bug #2481 would occur if the first Publication in the 'Publication Code' ComboBox is not Valid
            cmbPSubscriptionPublicationCode.SelectedValueChanged += new System.EventHandler(this.PublicationCodeChanged);
        }

        private void OnTickChangeItem(System.Object sender, EventArgs e)
        {
            CheckBox CheckBoxSender;

            //Control ChangeControl = null;

            // if check box is unticked then disable field, reset value and background colour,
            // otherwise enable field and emphasize background colour

            if (sender == null)
            {
                return;
            }

            CheckBoxSender = (CheckBox)sender;

            if (sender == chkChangeSubscriptionStatus)
            {
                //ChangeControl = cmbPSubscriptionSubscriptionStatus;

                if (!CheckBoxSender.Checked)
                {
                    cmbPSubscriptionSubscriptionStatus.SetSelectedString("", -1);
                }

                cmbPSubscriptionSubscriptionStatus.Enabled = CheckBoxSender.Checked;
            }
            else if (sender == chkChangeGratisSubscription)
            {
                //ChangeControl = chkPSubscriptionGratisSubscription;

                if (!CheckBoxSender.Checked)
                {
                    chkPSubscriptionGratisSubscription.Checked = false;
                }

                chkPSubscriptionGratisSubscription.Enabled = CheckBoxSender.Checked;
            }
            else if (sender == chkChangeNumberComplimentary)
            {
                //ChangeControl = txtPSubscriptionNumberComplimentary;

                if (!CheckBoxSender.Checked)
                {
                    txtPSubscriptionNumberComplimentary.NumberValueInt = null;
                }

                txtPSubscriptionNumberComplimentary.Enabled = CheckBoxSender.Checked;
            }
            else if (sender == chkChangePublicationCopies)
            {
                //ChangeControl = txtPSubscriptionPublicationCopies;

                if (!CheckBoxSender.Checked)
                {
                    txtPSubscriptionPublicationCopies.NumberValueInt = null;
                }

                txtPSubscriptionPublicationCopies.Enabled = CheckBoxSender.Checked;
            }
            else if (sender == chkChangeReasonSubsGivenCode)
            {
                //ChangeControl = cmbPSubscriptionReasonSubsGivenCode.cmbCombobox;

                if (!CheckBoxSender.Checked)
                {
                    cmbPSubscriptionReasonSubsGivenCode.SetSelectedString("", -1);
                }

                cmbPSubscriptionReasonSubsGivenCode.Enabled = CheckBoxSender.Checked;
            }
            else if (sender == chkChangeReasonSubsCancelledCode)
            {
                //ChangeControl = cmbPSubscriptionReasonSubsCancelledCode.cmbCombobox;

                if (!CheckBoxSender.Checked)
                {
                    cmbPSubscriptionReasonSubsCancelledCode.SetSelectedString("", -1);
                }

                cmbPSubscriptionReasonSubsCancelledCode.Enabled = CheckBoxSender.Checked;
            }
            else if (sender == chkChangeGiftFromKey)
            {
                //ChangeControl = txtPSubscriptionGiftFromKey;

                if (!CheckBoxSender.Checked)
                {
                    txtPSubscriptionGiftFromKey.Text = "0000000000";
                }

                txtPSubscriptionGiftFromKey.Enabled = CheckBoxSender.Checked;
            }
            else if (sender == chkChangeStartDate)
            {
                //ChangeControl = dtpPSubscriptionStartDate;

                if (!CheckBoxSender.Checked)
                {
                    dtpPSubscriptionStartDate.Clear();
                }

                dtpPSubscriptionStartDate.Enabled = CheckBoxSender.Checked;
            }
            else if (sender == chkChangeExpiryDate)
            {
                //ChangeControl = dtpPSubscriptionExpiryDate;

                if (!CheckBoxSender.Checked)
                {
                    dtpPSubscriptionExpiryDate.Clear();
                }

                dtpPSubscriptionExpiryDate.Enabled = CheckBoxSender.Checked;
            }
            else if (sender == chkChangeRenewalDate)
            {
                //ChangeControl = dtpPSubscriptionSubscriptionRenewalDate;

                if (!CheckBoxSender.Checked)
                {
                    dtpPSubscriptionSubscriptionRenewalDate.Clear();
                }

                dtpPSubscriptionSubscriptionRenewalDate.Enabled = CheckBoxSender.Checked;
            }
            else if (sender == chkChangeDateNoticeSent)
            {
                //ChangeControl = dtpPSubscriptionDateNoticeSent;

                if (!CheckBoxSender.Checked)
                {
                    dtpPSubscriptionDateNoticeSent.Clear();
                }

                dtpPSubscriptionDateNoticeSent.Enabled = CheckBoxSender.Checked;
            }
            else if (sender == chkChangeDateCancelled)
            {
                //ChangeControl = dtpPSubscriptionDateCancelled;

                if (!CheckBoxSender.Checked)
                {
                    dtpPSubscriptionDateCancelled.Clear();
                }

                dtpPSubscriptionDateCancelled.Enabled = CheckBoxSender.Checked;
            }
            else if (sender == chkChangeNumberIssuesReceived)
            {
                //ChangeControl = txtPSubscriptionNumberIssuesReceived;

                if (!CheckBoxSender.Checked)
                {
                    txtPSubscriptionNumberIssuesReceived.NumberValueInt = null;
                }

                txtPSubscriptionNumberIssuesReceived.Enabled = CheckBoxSender.Checked;
            }
            else if (sender == chkChangeFirstIssue)
            {
                //ChangeControl = dtpPSubscriptionFirstIssue;

                if (!CheckBoxSender.Checked)
                {
                    dtpPSubscriptionFirstIssue.Clear();
                }

                dtpPSubscriptionFirstIssue.Enabled = CheckBoxSender.Checked;
            }
            else if (sender == chkChangeLastIssue)
            {
                //ChangeControl = dtpPSubscriptionLastIssue;

                if (!CheckBoxSender.Checked)
                {
                    dtpPSubscriptionLastIssue.Clear();
                }

                dtpPSubscriptionLastIssue.Enabled = CheckBoxSender.Checked;
            }

            // don't change background colour at the moment (left code here in case we need to change it later)
            // now change background colour of selected field
            //if (ChangeControl != null)
            //{
            //    if (!CheckBoxSender.Checked)
            //    {
            //        ChangeControl.ResetBackColor();
            //    }
            //    else
            //    {
            //        ChangeControl.BackColor = ChangeControlBackgroundColor;
            //    }
            //}
        }

        private void CustomClosingHandler(System.Object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Needs to be set to false because it got set to true in ancestor Form!
            e.Cancel = false;

            // Need to call the following method in the Base Form to remove this Form from the Open Forms List
            FPetraUtilsObject.HasChanges = false; // this has to be set as otherwise the following call won't work
            FPetraUtilsObject.TFrmPetra_Closing(this, null);
        }

        /// <summary>
        /// Called by the instantiator of this Dialog to retrieve the values of Fields
        /// on the screen.
        /// </summary>
        /// <param name="ARow"></param>
        /// <param name="AFieldsToChange"></param>
        /// <returns>Boolean</returns>
        public Boolean GetReturnedParameters(ref PSubscriptionRow ARow, ref List <String>AFieldsToChange)
        {
            Boolean ReturnValue = true;

            ARow.InitValues();
            AFieldsToChange.Clear();

            // publication code needs to be set, otherwise change can not be performed
            if (cmbPSubscriptionPublicationCode.GetSelectedString() == "")
            {
                return false;
            }

            ARow.PublicationCode = cmbPSubscriptionPublicationCode.GetSelectedString();

            if (chkChangeSubscriptionStatus.Checked)
            {
                AFieldsToChange.Add(PSubscriptionTable.GetSubscriptionStatusDBName());
                ARow.SubscriptionStatus = cmbPSubscriptionSubscriptionStatus.Text;
            }

            if (chkChangeGratisSubscription.Checked)
            {
                AFieldsToChange.Add(PSubscriptionTable.GetGratisSubscriptionDBName());
                ARow.GratisSubscription = chkPSubscriptionGratisSubscription.Checked;
            }

            if (chkChangeNumberComplimentary.Checked
                && txtPSubscriptionNumberComplimentary.NumberValueInt.HasValue)
            {
                AFieldsToChange.Add(PSubscriptionTable.GetNumberComplimentaryDBName());
                ARow.NumberComplimentary = txtPSubscriptionNumberComplimentary.NumberValueInt.Value;
            }

            if (chkChangePublicationCopies.Checked
                && txtPSubscriptionPublicationCopies.NumberValueInt.HasValue)
            {
                AFieldsToChange.Add(PSubscriptionTable.GetPublicationCopiesDBName());
                ARow.PublicationCopies = txtPSubscriptionPublicationCopies.NumberValueInt.Value;
            }

            if (chkChangeReasonSubsGivenCode.Checked)
            {
                AFieldsToChange.Add(PSubscriptionTable.GetReasonSubsGivenCodeDBName());
                ARow.ReasonSubsGivenCode = cmbPSubscriptionReasonSubsGivenCode.GetSelectedString();
            }

            if (chkChangeReasonSubsCancelledCode.Checked)
            {
                AFieldsToChange.Add(PSubscriptionTable.GetReasonSubsCancelledCodeDBName());
                ARow.ReasonSubsCancelledCode = cmbPSubscriptionReasonSubsCancelledCode.GetSelectedString();
            }

            if (chkChangeGiftFromKey.Checked)
            {
                AFieldsToChange.Add(PSubscriptionTable.GetGiftFromKeyDBName());
                ARow.GiftFromKey = Convert.ToInt64(txtPSubscriptionGiftFromKey.Text);
            }

            if (chkChangeStartDate.Checked
                && dtpPSubscriptionStartDate.Date.HasValue)
            {
                AFieldsToChange.Add(PSubscriptionTable.GetStartDateDBName());
                ARow.StartDate = dtpPSubscriptionStartDate.Date.Value;
            }

            if (chkChangeExpiryDate.Checked)
            {
                AFieldsToChange.Add(PSubscriptionTable.GetExpiryDateDBName());
                ARow.ExpiryDate = dtpPSubscriptionExpiryDate.Date;
            }

            if (chkChangeRenewalDate.Checked)
            {
                AFieldsToChange.Add(PSubscriptionTable.GetSubscriptionRenewalDateDBName());
                ARow.SubscriptionRenewalDate = dtpPSubscriptionSubscriptionRenewalDate.Date;
            }

            if (chkChangeDateNoticeSent.Checked)
            {
                AFieldsToChange.Add(PSubscriptionTable.GetDateNoticeSentDBName());
                ARow.DateNoticeSent = dtpPSubscriptionDateNoticeSent.Date;
            }

            if (chkChangeDateCancelled.Checked)
            {
                AFieldsToChange.Add(PSubscriptionTable.GetDateCancelledDBName());
                ARow.DateCancelled = dtpPSubscriptionDateCancelled.Date;
            }

            if (chkChangeNumberIssuesReceived.Checked
                && txtPSubscriptionNumberIssuesReceived.NumberValueInt.HasValue)
            {
                AFieldsToChange.Add(PSubscriptionTable.GetNumberIssuesReceivedDBName());
                ARow.NumberIssuesReceived = txtPSubscriptionNumberIssuesReceived.NumberValueInt.Value;
            }

            if (chkChangeFirstIssue.Checked)
            {
                AFieldsToChange.Add(PSubscriptionTable.GetFirstIssueDBName());
                ARow.FirstIssue = dtpPSubscriptionFirstIssue.Date;
            }

            if (chkChangeLastIssue.Checked)
            {
                AFieldsToChange.Add(PSubscriptionTable.GetLastIssueDBName());
                ARow.LastIssue = dtpPSubscriptionLastIssue.Date;
            }

            return ReturnValue;
        }

        /// <summary>
        /// Returns text to be displayed to user with details about fields to be changed.
        /// Will return empty string if no boxes were ticked.
        /// </summary>
        /// <returns>String</returns>
        public String GetFieldsToChangeText()
        {
            String MessageText = "";

            if (chkChangeSubscriptionStatus.Checked)
            {
                MessageText += "* " + Catalog.GetString("Subscription Status") + " => " +
                               cmbPSubscriptionSubscriptionStatus.Text + "\r\n";
            }

            if (chkChangeGratisSubscription.Checked)
            {
                MessageText += "* " + Catalog.GetString("Free Subscription") + " => ";

                if (chkPSubscriptionGratisSubscription.Checked)
                {
                    MessageText += Catalog.GetString("Yes") + "\r\n";
                }
                else
                {
                    MessageText += Catalog.GetString("No") + "\r\n";
                }
            }

            if (chkChangeNumberComplimentary.Checked
                && txtPSubscriptionNumberComplimentary.NumberValueInt.HasValue)
            {
                MessageText += "* " + Catalog.GetString("Complimentary") + " => " +
                               txtPSubscriptionNumberComplimentary.NumberValueInt.ToString() + "\r\n";
            }

            if (chkChangePublicationCopies.Checked
                && txtPSubscriptionPublicationCopies.NumberValueInt.HasValue)
            {
                MessageText += "* " + Catalog.GetString("Copies") + " => " +
                               txtPSubscriptionPublicationCopies.NumberValueInt.ToString() + "\r\n";
            }

            if (chkChangeReasonSubsGivenCode.Checked)
            {
                MessageText += "* " + Catalog.GetString("Reason Given") + " => " +
                               cmbPSubscriptionReasonSubsGivenCode.GetSelectedString() + "\r\n";
            }

            if (chkChangeReasonSubsCancelledCode.Checked)
            {
                MessageText += "* " + Catalog.GetString("Reason Ended") + " => " +
                               cmbPSubscriptionReasonSubsCancelledCode.GetSelectedString() + "\r\n";
            }

            if (chkChangeGiftFromKey.Checked)
            {
                MessageText += "* " + Catalog.GetString("Gift Given By") + " => " +
                               txtPSubscriptionGiftFromKey.LabelText + "\r\n";
            }

            if (chkChangeStartDate.Checked
                && dtpPSubscriptionStartDate.Date.HasValue)
            {
                MessageText += "* " + Catalog.GetString("Start Date") + " => " +
                               dtpPSubscriptionStartDate.Text + "\r\n";
            }

            if (chkChangeExpiryDate.Checked)
            {
                MessageText += "* " + Catalog.GetString("Expiry Date") + " => " +
                               dtpPSubscriptionExpiryDate.Text + "\r\n";
            }

            if (chkChangeRenewalDate.Checked)
            {
                MessageText += "* " + Catalog.GetString("Date Renewed") + " => " +
                               dtpPSubscriptionSubscriptionRenewalDate.Text + "\r\n";
            }

            if (chkChangeDateNoticeSent.Checked)
            {
                MessageText += "* " + Catalog.GetString("Notice Sent") + " => " +
                               dtpPSubscriptionDateNoticeSent.Text + "\r\n";
            }

            if (chkChangeDateCancelled.Checked)
            {
                MessageText += "* " + Catalog.GetString("Date Ended") + " => " +
                               dtpPSubscriptionDateCancelled.Text + "\r\n";
            }

            if (chkChangeNumberIssuesReceived.Checked
                && txtPSubscriptionNumberIssuesReceived.NumberValueInt.HasValue)
            {
                MessageText += "* " + Catalog.GetString("Issues Received") + " => " +
                               txtPSubscriptionNumberIssuesReceived.NumberValueInt.ToString() + "\r\n";
            }

            if (chkChangeFirstIssue.Checked)
            {
                MessageText += "* " + Catalog.GetString("First Issue Sent") + " => " +
                               dtpPSubscriptionFirstIssue.Text + "\r\n";
            }

            if (chkChangeLastIssue.Checked)
            {
                MessageText += "* " + Catalog.GetString("Last Issue Sent") + " => " +
                               dtpPSubscriptionLastIssue.Text + "\r\n";
            }

            if (MessageText.Length > 0)
            {
                MessageText = "You are about to make the following changes to the Subscriptions " +
                              "for Publication '" + cmbPSubscriptionPublicationCode.GetSelectedString() +
                              "' for Partners in Extract '" + ExtractName + "':" + "\r\n\r\n" +
                              MessageText + "\r\n\r\n" + "Do you really want to do this?";
            }

            return MessageText;
        }

        private void BtnOK_Click(Object Sender, EventArgs e)
        {
            String MessageText;

            // validate data (outside the norm, therefore done manually here)
            GetDataFromControls(FMainDS.PSubscription[0]);

            FPetraUtilsObject.VerificationResultCollection.Clear();

            ValidateData(FMainDS.PSubscription[0]);
            ValidateDataManual(FMainDS.PSubscription[0]);

            if (!TDataValidation.ProcessAnyDataValidationErrors(false, FPetraUtilsObject.VerificationResultCollection,
                    this.GetType(), null, true))
            {
                return;
            }

            MessageText = GetFieldsToChangeText();

            if (MessageText.Length > 0)
            {
                if (MessageBox.Show(MessageText,
                        Catalog.GetString("Change Subscription"),
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Nothing to be changed. You need to tick at least one box!",
                    Catalog.GetString("Change Subscription"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void ValidateDataManual(PSubscriptionRow ARow)
        {
            DataColumn ValidationColumn;
            DataColumn ValidationColumn2;
            TValidationControlsData ValidationControlsData;
            TValidationControlsData ValidationControlsData2;
            TVerificationResult VerificationResult = null;

            string reasonEndedErrorSuffix = Catalog.GetString("  Please un-check the 'Reason Ended' box or set an appropriate 'Subscription Status'.");
            string dateEndedErrorSuffix = Catalog.GetString("  Please un-check the 'Date Ended' box or set an appropriate 'Subscription Status'.");

            TVerificationResultCollection VerificationResultCollection = FPetraUtilsObject.VerificationResultCollection;

            if (!chkChangeReasonSubsGivenCode.Checked)
            {
                if (VerificationResultCollection.Contains(ARow.Table.Columns[PSubscriptionTable.ColumnReasonSubsGivenCodeId]))
                {
                    VerificationResultCollection.Remove(ARow.Table.Columns[PSubscriptionTable.ColumnReasonSubsGivenCodeId]);
                }
            }

            if (!chkChangeStartDate.Checked)
            {
                if (VerificationResultCollection.Contains(ARow.Table.Columns[PSubscriptionTable.ColumnStartDateId]))
                {
                    VerificationResultCollection.Remove(ARow.Table.Columns[PSubscriptionTable.ColumnStartDateId]);
                }
            }

            // if 'SubscriptionStatus' is CANCELLED or EXPIRED then 'Reason Ended' and 'End Date' must be set
            ValidationColumn = ARow.Table.Columns[PSubscriptionTable.ColumnSubscriptionStatusId];

            if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn, out ValidationControlsData))
            {
                if ((!ARow.IsSubscriptionStatusNull())
                    && ((ARow.SubscriptionStatus == MPartnerConstants.SUBSCRIPTIONS_STATUS_CANCELLED)
                        || (ARow.SubscriptionStatus == MPartnerConstants.SUBSCRIPTIONS_STATUS_EXPIRED)))
                {
                    if (ARow.IsReasonSubsCancelledCodeNull()
                        || (ARow.ReasonSubsCancelledCode == String.Empty))
                    {
                        VerificationResult = new TScreenVerificationResult(new TVerificationResult(this,
                                ErrorCodes.GetErrorInfo(PetraErrorCodes.ERR_SUBSCRIPTION_REASONENDEDMANDATORY_WHEN_EXPIRED)),
                            ValidationColumn, ValidationControlsData.ValidationControl);
                        VerificationResult.OverrideResultText(VerificationResult.ResultText +
                            Catalog.GetString("  Please check the 'Reason Ended' box and choose one of the available reasons."));
                    }
                    else if (ARow.IsDateCancelledNull())
                    {
                        VerificationResult = new TScreenVerificationResult(new TVerificationResult(this,
                                ErrorCodes.GetErrorInfo(PetraErrorCodes.ERR_SUBSCRIPTION_DATEENDEDMANDATORY_WHEN_EXPIRED)),
                            ValidationColumn, ValidationControlsData.ValidationControl);
                        VerificationResult.OverrideResultText(VerificationResult.ResultText +
                            Catalog.GetString("  Please check the 'Date Ended' box and enter a relevant date."));
                    }
                }
                else
                {
                    // if 'SubscriptionStatus' is not CANCELLED or EXPIRED then 'Reason Ended' and 'End Date' must NOT be set
                    // Note: - if Subscription Status was not checked then it will have a value 'PERMANENT'
                    //   So this code runs and checks that reason and date are not set when subscription status is not checked
                    // Note also: the server code takes care of ensuring that Reason and Date are forced to NULL when
                    //   Subscription Status is not CANCELLED or EXPIRED.
                    //   This means that the user does not have to explicitly set them to NULL in the GUI.
                    // So all we have to do here is make sure the user did not explicitly set them when they should be un-set
                    if ((!ARow.IsReasonSubsCancelledCodeNull())
                        && (ARow.ReasonSubsCancelledCode != String.Empty))
                    {
                        VerificationResult = new TScreenVerificationResult(new TVerificationResult(this,
                                ErrorCodes.GetErrorInfo(PetraErrorCodes.ERR_SUBSCRIPTION_REASONENDEDSET_WHEN_ACTIVE)),
                            ValidationColumn, ValidationControlsData.ValidationControl);
                        VerificationResult.OverrideResultText(VerificationResult.ResultText + reasonEndedErrorSuffix);
                    }
                    else if (!ARow.IsDateCancelledNull())
                    {
                        VerificationResult = new TScreenVerificationResult(new TVerificationResult(this,
                                ErrorCodes.GetErrorInfo(PetraErrorCodes.ERR_SUBSCRIPTION_DATEENDEDSET_WHEN_ACTIVE)),
                            ValidationColumn, ValidationControlsData.ValidationControl);
                        VerificationResult.OverrideResultText(VerificationResult.ResultText + dateEndedErrorSuffix);
                    }
                }

                // Handle addition/removal to/from TVerificationResultCollection
                VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);

                // perform checks that include 'Start Date' ----------------------------------------------------------------
                if (chkChangeStartDate.Checked)
                {
                    ValidationColumn = ARow.Table.Columns[PSubscriptionTable.ColumnStartDateId];

                    if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn, out ValidationControlsData))
                    {
                        // 'Start Date' must not be later than 'Expiry Date'
                        if (chkChangeExpiryDate.Checked)
                        {
                            ValidationColumn2 = ARow.Table.Columns[PSubscriptionTable.ColumnExpiryDateId];

                            if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn2, out ValidationControlsData2))
                            {
                                VerificationResult = TDateChecks.FirstLesserOrEqualThanSecondDate
                                                         (ARow.StartDate,
                                                         ARow.ExpiryDate,
                                                         ValidationControlsData.ValidationControlLabel,
                                                         ValidationControlsData2.ValidationControlLabel,
                                                         this,
                                                         ValidationColumn,
                                                         ValidationControlsData.ValidationControl);

                                // Handle addition to/removal from TVerificationResultCollection
                                VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);
                            }
                        }

                        // 'Start Date' must not be later than 'Renewal Date'
                        if (chkChangeRenewalDate.Checked)
                        {
                            ValidationColumn2 = ARow.Table.Columns[PSubscriptionTable.ColumnSubscriptionRenewalDateId];

                            if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn2, out ValidationControlsData2))
                            {
                                VerificationResult = TDateChecks.FirstLesserOrEqualThanSecondDate
                                                         (ARow.StartDate,
                                                         ARow.SubscriptionRenewalDate,
                                                         ValidationControlsData.ValidationControlLabel,
                                                         ValidationControlsData2.ValidationControlLabel,
                                                         this,
                                                         ValidationColumn,
                                                         ValidationControlsData.ValidationControl);

                                // Handle addition to/removal from TVerificationResultCollection
                                VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);
                            }
                        }

                        // 'Start Date' must not be later than 'End Date'
                        if (chkChangeDateCancelled.Checked)
                        {
                            ValidationColumn2 = ARow.Table.Columns[PSubscriptionTable.ColumnDateCancelledId];

                            if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn2, out ValidationControlsData2))
                            {
                                VerificationResult = TDateChecks.FirstLesserOrEqualThanSecondDate
                                                         (ARow.StartDate,
                                                         ARow.DateCancelled,
                                                         ValidationControlsData.ValidationControlLabel,
                                                         ValidationControlsData2.ValidationControlLabel,
                                                         this,
                                                         ValidationColumn,
                                                         ValidationControlsData.ValidationControl);

                                // Handle addition to/removal from TVerificationResultCollection
                                VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);
                            }
                        }

                        // 'Start Date' must not be later than 'Notice Sent'
                        if (chkChangeDateNoticeSent.Checked)
                        {
                            ValidationColumn2 = ARow.Table.Columns[PSubscriptionTable.ColumnDateNoticeSentId];

                            if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn2, out ValidationControlsData2))
                            {
                                VerificationResult = TDateChecks.FirstLesserOrEqualThanSecondDate
                                                         (ARow.StartDate,
                                                         ARow.DateNoticeSent,
                                                         ValidationControlsData.ValidationControlLabel,
                                                         ValidationControlsData2.ValidationControlLabel,
                                                         this,
                                                         ValidationColumn,
                                                         ValidationControlsData.ValidationControl);

                                // Handle addition to/removal from TVerificationResultCollection
                                VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);
                            }
                        }

                        // 'Start Date' must not be later than 'First Sent'
                        if (chkChangeFirstIssue.Checked)
                        {
                            ValidationColumn2 = ARow.Table.Columns[PSubscriptionTable.ColumnFirstIssueId];

                            if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn2, out ValidationControlsData2))
                            {
                                VerificationResult = TDateChecks.FirstLesserOrEqualThanSecondDate
                                                         (ARow.StartDate,
                                                         ARow.FirstIssue,
                                                         ValidationControlsData.ValidationControlLabel,
                                                         ValidationControlsData2.ValidationControlLabel,
                                                         this,
                                                         ValidationColumn,
                                                         ValidationControlsData.ValidationControl);

                                // Handle addition to/removal from TVerificationResultCollection
                                VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);
                            }
                        }

                        // 'Start Date' must not be later than 'Last Date'
                        if (chkChangeLastIssue.Checked)
                        {
                            ValidationColumn2 = ARow.Table.Columns[PSubscriptionTable.ColumnLastIssueId];

                            if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn2, out ValidationControlsData2))
                            {
                                VerificationResult = TDateChecks.FirstLesserOrEqualThanSecondDate
                                                         (ARow.StartDate,
                                                         ARow.LastIssue,
                                                         ValidationControlsData.ValidationControlLabel,
                                                         ValidationControlsData2.ValidationControlLabel,
                                                         this,
                                                         ValidationColumn,
                                                         ValidationControlsData.ValidationControl);

                                // Handle addition to/removal from TVerificationResultCollection
                                VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);
                            }
                        }
                    }
                }

                // perform checks that include 'Date Renewed' ----------------------------------------------------------------
                if (chkChangeRenewalDate.Checked)
                {
                    ValidationColumn = ARow.Table.Columns[PSubscriptionTable.ColumnSubscriptionRenewalDateId];

                    if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn, out ValidationControlsData))
                    {
                        // 'Date Renewed' must not be later than today
                        VerificationResult = TDateChecks.FirstLesserOrEqualThanSecondDate
                                                 (ARow.SubscriptionRenewalDate, DateTime.Today,
                                                 ValidationControlsData.ValidationControlLabel, Catalog.GetString("Today's Date"),
                                                 this, ValidationColumn, ValidationControlsData.ValidationControl);

                        // Handle addition to/removal from TVerificationResultCollection
                        VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);

                        // 'Date Renewed' must not be later than 'Date Expired'
                        if (chkChangeExpiryDate.Checked)
                        {
                            ValidationColumn2 = ARow.Table.Columns[PSubscriptionTable.ColumnExpiryDateId];

                            if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn2, out ValidationControlsData2))
                            {
                                VerificationResult = TDateChecks.FirstLesserOrEqualThanSecondDate
                                                         (ARow.SubscriptionRenewalDate,
                                                         ARow.ExpiryDate,
                                                         ValidationControlsData.ValidationControlLabel,
                                                         ValidationControlsData2.ValidationControlLabel,
                                                         this,
                                                         ValidationColumn,
                                                         ValidationControlsData.ValidationControl);

                                // Handle addition to/removal from TVerificationResultCollection
                                VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);
                            }
                        }

                        // 'Date Renewed' must not be later than 'Date Notice Sent'
                        if (chkChangeDateNoticeSent.Checked)
                        {
                            ValidationColumn2 = ARow.Table.Columns[PSubscriptionTable.ColumnDateNoticeSentId];

                            if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn2, out ValidationControlsData2))
                            {
                                VerificationResult = TDateChecks.FirstLesserOrEqualThanSecondDate
                                                         (ARow.SubscriptionRenewalDate,
                                                         ARow.DateNoticeSent,
                                                         ValidationControlsData.ValidationControlLabel,
                                                         ValidationControlsData2.ValidationControlLabel,
                                                         this,
                                                         ValidationColumn,
                                                         ValidationControlsData.ValidationControl);

                                // Handle addition to/removal from TVerificationResultCollection
                                VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);
                            }
                        }
                    }
                }

                // Info: AlanP - we decided that there would be no validation on Date Cancelled because Petra did not have it and potentially
                //  there appear to be arguments for not restricting the date cancelled.  We need more input from real users on this!
                //  See also Partner.Validation.cs in Shared
                //// 'Date Cancelled' must not be before today
                //if (chkChangeDateCancelled.Checked)
                //{
                //    ValidationColumn = ARow.Table.Columns[PSubscriptionTable.ColumnDateCancelledId];

                //    if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn, out ValidationControlsData))
                //    {
                //        VerificationResult = TDateChecks.FirstGreaterOrEqualThanSecondDate
                //                                 (ARow.DateCancelled, DateTime.Today,
                //                                 ValidationControlsData.ValidationControlLabel, Catalog.GetString("Today's Date"),
                //                                 this, ValidationColumn, ValidationControlsData.ValidationControl);

                //        // Handle addition to/removal from TVerificationResultCollection
                //        VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);
                //    }
                //}

                // 'First Sent' must not be later than 'Last Sent'
                if (chkChangeFirstIssue.Checked
                    && chkChangeLastIssue.Checked)
                {
                    ValidationColumn = ARow.Table.Columns[PSubscriptionTable.ColumnFirstIssueId];

                    if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn, out ValidationControlsData))
                    {
                        ValidationColumn2 = ARow.Table.Columns[PSubscriptionTable.ColumnLastIssueId];

                        if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn2, out ValidationControlsData2))
                        {
                            VerificationResult = TDateChecks.FirstLesserOrEqualThanSecondDate
                                                     (ARow.FirstIssue, ARow.LastIssue,
                                                     ValidationControlsData.ValidationControlLabel, ValidationControlsData2.ValidationControlLabel,
                                                     this, ValidationColumn, ValidationControlsData.ValidationControl);

                            // Handle addition to/removal from TVerificationResultCollection
                            VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);
                        }
                    }
                }

                // 'First Sent' must not be later than today
                if (chkChangeFirstIssue.Checked)
                {
                    ValidationColumn = ARow.Table.Columns[PSubscriptionTable.ColumnFirstIssueId];

                    if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn, out ValidationControlsData))
                    {
                        VerificationResult = TDateChecks.FirstLesserOrEqualThanSecondDate
                                                 (ARow.FirstIssue, DateTime.Today,
                                                 ValidationControlsData.ValidationControlLabel, Catalog.GetString("Today's Date"),
                                                 this, ValidationColumn, ValidationControlsData.ValidationControl);

                        // Handle addition to/removal from TVerificationResultCollection
                        VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);
                    }
                }

                // 'Date Started' must not be later than 'First Sent'
                if (chkChangeFirstIssue.Checked
                    && chkChangeStartDate.Checked)
                {
                    ValidationColumn = ARow.Table.Columns[PSubscriptionTable.ColumnStartDateId];

                    if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn, out ValidationControlsData))
                    {
                        ValidationColumn2 = ARow.Table.Columns[PSubscriptionTable.ColumnFirstIssueId];

                        if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn2, out ValidationControlsData2))
                        {
                            VerificationResult = TDateChecks.FirstLesserOrEqualThanSecondDate
                                                     (ARow.StartDate, ARow.FirstIssue,
                                                     ValidationControlsData.ValidationControlLabel, ValidationControlsData2.ValidationControlLabel,
                                                     this, ValidationColumn, ValidationControlsData.ValidationControl);

                            // Handle addition to/removal from TVerificationResultCollection
                            VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);
                        }
                    }
                }

                // 'Last Sent' must not be later than today
                if (chkChangeLastIssue.Checked)
                {
                    ValidationColumn = ARow.Table.Columns[PSubscriptionTable.ColumnLastIssueId];

                    if (FPetraUtilsObject.ValidationControlsDict.TryGetValue(ValidationColumn, out ValidationControlsData))
                    {
                        VerificationResult = TDateChecks.FirstLesserOrEqualThanSecondDate
                                                 (ARow.LastIssue, DateTime.Today,
                                                 ValidationControlsData.ValidationControlLabel, Catalog.GetString("Today's Date"),
                                                 this, ValidationColumn, ValidationControlsData.ValidationControl);

                        // Handle addition to/removal from TVerificationResultCollection
                        VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);
                    }
                }
            }
        }

        private void PublicationCodeChanged(object sender, EventArgs e)
        {
            TUCPartnerSubscriptionsLogic.CheckPublicationComboValidValue(cmbPSubscriptionPublicationCode);
        }
    }

    // in addition derive class from IFrmPetraEdit so TFrmPetraEditUtils can be created
    public partial class TFrmUpdateExtractChangeSubscriptionDialog : Ict.Petra.Client.CommonForms.IFrmPetraEdit
    {
        /// <summary>
        /// save the changes on the screen
        /// </summary>
        /// <returns></returns>
        public bool SaveChanges()
        {
            // method needs to be provided here for interface but will never be called
            return false;
        }

        /// <summary>
        /// Get the number of changed records and specify a message to incorporate into the 'Do you want to save?' message box
        /// </summary>
        /// <param name="AMessage">An optional message to display.  If the parameter is an empty string a default message will be used</param>
        /// <returns>The number of changed records.  Return -1 to imply 'unknown'.</returns>
        public int GetChangedRecordCount(out string AMessage)
        {
            AMessage = String.Empty;
            return -1;
        }
    }
}