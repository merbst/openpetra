//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       christiank, timop
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
using System.Windows.Forms;
using Ict.Petra.Client.App.Gui;
using Ict.Petra.Shared;
using Ict.Petra.Shared.MPartner;
using Ict.Petra.Shared.MCommon;
using Ict.Petra.Client.CommonForms;
using Ict.Petra.Client.App.Core.RemoteObjects;
using Ict.Common;
using Ict.Common.IO;
using Ict.Common.Verification;
using System.Xml;
using System.Collections.Generic;
using System.Collections;
using Ict.Petra.Client.App.Core;
using Ict.Petra.Client.MPartner.Logic;
using Ict.Petra.Client.CommonControls.Logic;
using Ict.Common.Exceptions;

namespace Ict.Petra.Client.MPartner.Gui
{
    /// <summary>
    /// Methods previously in TFrmPartnerMain that don't have any other home
    /// </summary>
    public class TPartnerMain
    {
        /// <summary>
        /// Opens the Partner Find screen (or activates it in case a non-modal instance was already open and
        /// ARestrictToPartnerClasses is null). If ARestrictToPartnerClasses isn't null then the screen is opened modally.
        /// </summary>
        /// <remarks>
        /// For NUnit tests that just try to open the Partner Find screen but which don't instantiate a Main Form
        /// we need to ignore the <paramref name="AParentForm" /> Argument as it will be null in those cases!
        /// </remarks>
        /// <returns>void</returns>
        public static void FindPartnerOfClass(Form AParentForm,
            string ARestrictToPartnerClasses = null,
            TPartnerEditTabPageEnum APreferredInitialTabPage = TPartnerEditTabPageEnum.petpAddresses)
        {
            if (ARestrictToPartnerClasses == null)
            {
                // No Cursor change if run from within NUnit Test without Main Form instance...
                if (AParentForm != null)
                {
                    AParentForm.Cursor = Cursors.WaitCursor;
                }

                TPartnerFindScreen PartnerFindForm = new TPartnerFindScreen(AParentForm);
                PartnerFindForm.SetParameters(false, -1);
                PartnerFindForm.Show();

                // No Cursor change if run from within NUnit Test without Main Form instance...
                if (AParentForm != null)
                {
                    AParentForm.Cursor = Cursors.Default;
                }
            }
            else
            {
                long PartnerKey;
                string ShortName;
                TPartnerClass? PartnerClass;
                TLocationPK LocationPK;

                if (TPartnerFindScreenManager.OpenModalForm(ARestrictToPartnerClasses, out PartnerKey, out ShortName, out PartnerClass,
                        out LocationPK, AParentForm))
                {
                    // Open the Partner Edit screen
                    TFrmPartnerEdit PartnerEditForm;

                    // No Cursor change if run from within NUnit Test without Main Form instance...
                    if (AParentForm != null)
                    {
                        AParentForm.Cursor = Cursors.WaitCursor;
                    }

                    PartnerEditForm = new TFrmPartnerEdit(AParentForm);

                    if (APreferredInitialTabPage == TPartnerEditTabPageEnum.petpAddresses)
                    {
                        PartnerEditForm.SetParameters(TScreenMode.smEdit, PartnerKey, LocationPK.SiteKey, LocationPK.LocationKey);
                    }
                    else
                    {
                        PartnerEditForm.SetParameters(TScreenMode.smEdit, PartnerKey, APreferredInitialTabPage);
                    }

                    PartnerEditForm.Show();

                    if (ARestrictToPartnerClasses.Split(new Char[] { (',') })[0] == "PERSON")
                    {
                        TUserDefaults.SetDefault(TUserDefaults.USERDEFAULT_LASTPERSONPERSONNEL, PartnerKey);
                    }
                    else
                    {
                        TUserDefaults.SetDefault(TUserDefaults.USERDEFAULT_LASTPARTNERMAILROOM, PartnerKey);
                    }

                    // No Cursor change if run from within NUnit Test without Main Form instance...
                    if (AParentForm != null)
                    {
                        AParentForm.Cursor = Cursors.Default;
                    }
                }
            }
        }

        /// <summary>
        /// Opens the Partner Find screen (or activates it in case a non-modal instance was already open).
        /// </summary>
        public static void FindPartner(Form AParentForm)
        {
            FindPartnerOfClass(AParentForm, null);
        }

        /// <summary>
        /// Opens the Partner Find screen for searching for PERSONs. The screen is opened modally.
        /// </summary>
        public static void FindPartnerOfClassPERSON(Form AParentForm)
        {
            FindPartnerOfClass(AParentForm, "PERSON", TPartnerEditTabPageEnum.petpPersonnelIndividualData);
        }

        /// <summary>
        /// delete partner selected with the Partner Find Screen
        /// </summary>
        public static Boolean DeletePartnerRecord(Form AParentForm)
        {
            Boolean ResultValue = false;
            Int64 PartnerKey = -1;
            String ShortName;
            TPartnerClass? PartnerClass;
            TLocationPK ResultLocationPK;

            // the user has to select an existing partner to make that partner a supplier
            if (TPartnerFindScreenManager.OpenModalForm("",
                    out PartnerKey,
                    out ShortName,
                    out PartnerClass,
                    out ResultLocationPK,
                    AParentForm))
            {
                ResultValue = DeletePartner(PartnerKey, AParentForm);
            }

            return ResultValue;
        }

        /// <summary>
        /// delete partner with given partner key
        /// </summary>
        public static Boolean DeletePartner(Int64 APartnerKey, Form AParentForm)
        {
            Boolean ResultValue = false;
            String ShortName;
            String Message;
            TVerificationResultCollection VerificationResult;

            AParentForm.Cursor = Cursors.WaitCursor;

            if (TRemote.MPartner.Partner.WebConnectors.CanPartnerBeDeleted(APartnerKey, out Message))
            {
                // Partner can be deleted -> let user confirm
                TRemote.MPartner.Partner.WebConnectors.GetPartnerStatisticsForDeletion(APartnerKey, out ShortName, out Message);

                if (MessageBox.Show(Message,
                        Catalog.GetString("Delete Partner"),
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.Yes)
                {
                    if (TRemote.MPartner.Partner.WebConnectors.DeletePartner(APartnerKey, out VerificationResult))
                    {
                        ResultValue = true;
                        MessageBox.Show(String.Format(Catalog.GetString("Partner {0} {1} successfully deleted"), APartnerKey, ShortName),
                            Catalog.GetString("Delete Partner"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        if (VerificationResult != null)
                        {
                            // display delete error to user
                            MessageBox.Show(Messages.BuildMessageFromVerificationResult("Deletion of Partner failed!" +
                                    Environment.NewLine + "Reasons:", VerificationResult),
                                Catalog.GetString("Delete Partner"),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show(Catalog.GetString(
                                    "Unfortunately the deletion of the Partner failed for unknown reasons.\r\n\r\nPlease contact your Support Team!"),
                                Catalog.GetString("Delete Partner"),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                // Partner cannot be deleted: show message to user with reasons
                MessageBox.Show(Message,
                    Catalog.GetString("Delete Partner"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            AParentForm.Cursor = Cursors.Default;

            return ResultValue;
        }

        /// <summary>
        /// create a new Family
        /// </summary>
        public static void NewPartner_Family(Form AParentForm)
        {
            TFrmPartnerEdit frm = new TFrmPartnerEdit(AParentForm);

            frm.SetParameters(TScreenMode.smNew, "FAMILY", -1, -1, "");
            frm.Show();
        }

        /// create a new Organisation (eg. supplier)
        public static void NewPartner_Organisation(Form AParentForm)
        {
            TFrmPartnerEdit frm = new TFrmPartnerEdit(AParentForm);

            frm.SetParameters(TScreenMode.smNew, "ORGANISATION", -1, -1, "");
            frm.Show();
        }

        /// create a new Person
        public static void NewPartner_Person(Form AParentForm)
        {
            TFrmPartnerEdit frm = new TFrmPartnerEdit(AParentForm);

            frm.SetParameters(TScreenMode.smNew, "PERSON", -1, -1, String.Empty, String.Empty, false);

            frm.Show();
        }

        /// <summary>
        /// create a new Church
        /// </summary>
        public static void NewPartner_Church(Form AParentForm)
        {
            TFrmPartnerEdit frm = new TFrmPartnerEdit(AParentForm);

            frm.SetParameters(TScreenMode.smNew, "CHURCH", -1, -1, "");
            frm.Show();
        }

        /// create a new Bank
        public static void NewPartner_Bank(Form AParentForm)
        {
            TFrmPartnerEdit frm = new TFrmPartnerEdit(AParentForm);

            frm.SetParameters(TScreenMode.smNew, "BANK", -1, -1, "");
            frm.Show();
        }

        /// create a new Unit
        public static void NewPartner_Unit(Form AParentForm)
        {
            TFrmPartnerEdit frm = new TFrmPartnerEdit(AParentForm);

            frm.SetParameters(TScreenMode.smNew, "UNIT", -1, -1, "");
            frm.Show();
        }

        /// create a new Venue
        public static void NewPartner_Venue(Form AParentForm)
        {
            TFrmPartnerEdit frm = new TFrmPartnerEdit(AParentForm);

            frm.SetParameters(TScreenMode.smNew, "VENUE", -1, -1, "");
            frm.Show();
        }

        /// <summary>
        /// Checks if the the partner is merged. If so then show a dialog where the user can
        /// choose to work with the current partner or the merged partner.
        /// </summary>
        /// <param name="APartnerKey">The current partner to be checked</param>
        /// <param name="AMergedIntoPartnerKey">If the partner is merged the merged partner key.
        /// If the partner is not merged: -1</param>
        /// <param name="AParentForm">Parent Form.</param>
        /// <returns>True if the user wants to work with the merged partner, otherwise false.</returns>
        public static bool MergedPartnerHandling(Int64 APartnerKey,
            out Int64 AMergedIntoPartnerKey, Form AParentForm)
        {
            bool ReturnValue = false;

            AMergedIntoPartnerKey = -1;

            bool IsMergedPartner;
            string MergedPartnerPartnerShortName;
            string MergedIntoPartnerShortName;
            TPartnerClass MergedPartnerPartnerClass;
            TPartnerClass MergedIntoPartnerClass;
            string MergedBy;
            DateTime MergeDate;

            IsMergedPartner = TServerLookup.TMPartner.MergedPartnerDetails(APartnerKey,
                out MergedPartnerPartnerShortName,
                out MergedPartnerPartnerClass,
                out AMergedIntoPartnerKey,
                out MergedIntoPartnerShortName,
                out MergedIntoPartnerClass,
                out MergedBy,
                out MergeDate);

            if (IsMergedPartner)
            {
                // Open the 'Merged Partner Info' Dialog
                using (TPartnerMergedPartnerInfoDialog MergedPartnerInfoDialog = new TPartnerMergedPartnerInfoDialog(AParentForm))
                {
                    MergedPartnerInfoDialog.SetParameters(APartnerKey,
                        MergedPartnerPartnerShortName,
                        MergedPartnerPartnerClass,
                        AMergedIntoPartnerKey,
                        MergedIntoPartnerShortName,
                        MergedIntoPartnerClass,
                        MergedBy,
                        MergeDate);

                    if (MergedPartnerInfoDialog.ShowDialog() == DialogResult.OK)
                    {
                        ReturnValue = true;
                    }
                    else
                    {
                        ReturnValue = false;
                    }
                }
            }

            return ReturnValue;
        }

        /// <summary>
        /// Opens the partner edit screen with the last partner worked on.
        /// Checks if the partner is merged.
        /// </summary>
        public static void OpenLastUsedPartnerEditScreenByContext(Form AParentForm, string AContext = null)
        {
            string ValidContexts;
            string Context;
            long MergedPartnerKey = 0;
            long LastPartnerKey;
            string NoPartnerAvailableStr = Catalog.GetString("You have not edited a Partner yet.");

            if (AContext != null)
            {
                ValidContexts = TUserDefaults.USERDEFAULT_LASTPARTNERMAILROOM + ";" + TUserDefaults.USERDEFAULT_LASTPERSONPERSONNEL + ";" +
                                TUserDefaults.USERDEFAULT_LASTUNITPERSONNEL + ";" + TUserDefaults.USERDEFAULT_LASTPERSONCONFERENCE + ";";

                if (!ValidContexts.Contains(AContext + ";"))
                {
                    throw new ArgumentException("AContext \"" + AContext + "\" is not a valid context. Valid contexts: " + ValidContexts);
                }
                else
                {
                    Context = AContext;
                }
            }
            else
            {
                Context = TUserDefaults.USERDEFAULT_LASTPARTNERMAILROOM;
            }

            LastPartnerKey = TUserDefaults.GetInt64Default(Context, 0);

            // we don't need to validate the partner key
            // because it's done in the mnuFile_Popup function.
            // If we don't have a valid partner key, this code can't be called from the file menu.

            // now that this function is called from the main menu, we need to check for LastPartnerKey != 0
            if (LastPartnerKey == 0)
            {
                if (Context == TUserDefaults.USERDEFAULT_LASTPERSONPERSONNEL)
                {
                    NoPartnerAvailableStr = Catalog.GetString("You have not yet worked with a Person in the Personnel Module.");
                }

                MessageBox.Show(Catalog.GetString(NoPartnerAvailableStr),
                    Catalog.GetString("No Last Partner"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (MergedPartnerHandling(LastPartnerKey, out MergedPartnerKey, AParentForm))
            {
                // work with the merged partner
                LastPartnerKey = MergedPartnerKey;
            }
            else if (MergedPartnerKey > 0)
            {
                // The partner is merged but user cancelled the action
                return;
            }

            // Open the Partner Edit screen
            TFrmPartnerEdit frmPEDS;

            AParentForm.Cursor = Cursors.WaitCursor;

            frmPEDS = new TFrmPartnerEdit(AParentForm);

            if (Context == TUserDefaults.USERDEFAULT_LASTPERSONPERSONNEL)
            {
                frmPEDS.SetParameters(TScreenMode.smEdit, LastPartnerKey, TPartnerEditTabPageEnum.petpPersonnelIndividualData);
            }
            else
            {
                frmPEDS.SetParameters(TScreenMode.smEdit, LastPartnerKey);
            }

            frmPEDS.Show();

            AParentForm.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Opens the partner edit screen with the last partner worked on.
        /// Checks if the partner is merged.
        /// </summary>
        public static void OpenLastUsedPartnerEditScreen(Form AParentForm)
        {
            OpenLastUsedPartnerEditScreenByContext(AParentForm, null);
        }

        /// <summary>
        /// Opens the partner edit screen with the last partner worked on from within the Personnel Module.
        /// Checks if the partner is merged.
        /// </summary>
        public static void OpenLastUsedPartnerEditScreenPersonnelModule(Form AParentForm)
        {
            OpenLastUsedPartnerEditScreenByContext(AParentForm, "PersonnelLastPerson");
        }

        /// <summary>
        /// End all subscriptions that have a past expiry date and that have no cancel date yet
        /// </summary>
        /// <param name="AParentForm">Form where this method is called from</param>
        /// <returns>void</returns>
        public static void CancelExpiredSubscriptions(Form AParentForm)
        {
            if (MessageBox.Show(Catalog.GetString("You are about to end all Subscriptions which have passed their Expiry Date.\r\n\r\n" +
                        "This will set:\r\n" +
                        "  * Status to \'Expired\'\r\n" +
                        "  * Reason Ended to \'Complete\'\r\n" +
                        "  * Date Ended to today\r\n\r\n" +
                        "for all Subscriptions where the Expiry Date has passed and the Date Ended has not been set.\r\n\r\n\r\n" +
                        "Do you want to continue?"),
                    Catalog.GetString("End Subscriptions"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (TRemote.MPartner.Partner.WebConnectors.CancelExpiredSubscriptions())
                {
                    MessageBox.Show(Catalog.GetString("Expired Subscriptions are now ended"),
                        Catalog.GetString("End Subscriptions"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(Catalog.GetString("Error while ending Subscriptions"),
                        Catalog.GetString("End Subscriptions"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Exports a single Partner to OpenPetra.
        /// </summary>
        public static void ExportSinglePartnerToOpenPetra(Form AParentForm)
        {
            bool OldPetraFormat = false;

            ExportSinglePartner(AParentForm, OldPetraFormat);
        }

        /// <summary>
        /// Exports a single Partner to Petra.
        /// </summary>
        public static void ExportSinglePartnerToPetra(Form AParentForm)
        {
            bool OldPetraFormat = true;

            ExportSinglePartner(AParentForm, OldPetraFormat);
        }

        /// <summary>
        /// Exports a single Partner to OpenPetra or Petra.
        /// </summary>
        /// <param name="AParentForm"></param>
        /// <param name="AOldPetraFormat"></param>
        public static void ExportSinglePartner(Form AParentForm, bool AOldPetraFormat)
        {
            Int64 PartnerKey = -1;
            String ShortName;
            TPartnerClass? PartnerClass;
            TLocationPK ResultLocationPK;

            if (TPartnerFindScreenManager.OpenModalForm("",
                    out PartnerKey,
                    out ShortName,
                    out PartnerClass,
                    out ResultLocationPK,
                    AParentForm))
            {
                TPartnerExportLogic.ExportSinglePartner(PartnerKey, PartnerClass.ToString(), 0, 0, AOldPetraFormat);
            }
        }

        /// <summary>
        /// Exports Partners from an Extract to Petra.
        /// </summary>
        public static void ExportPartnersInExtractToPetra(Form AParentForm)
        {
            bool OldPetraFormat = true;

            ExportPartnersInExtract(AParentForm, OldPetraFormat);
        }

        /// <summary>
        /// Exports Partners from an Extract To OpenPetra.
        /// </summary>
        public static void ExportPartnersInExtractToOpenPetra(Form AParentForm)
        {
            bool OldPetraFormat = false;

            ExportPartnersInExtract(AParentForm, OldPetraFormat);
        }

        /// <summary>
        /// Exports Partners from an Extract.
        /// </summary>
        /// <param name="AParentForm"></param>
        /// <param name="AOldPetraFormat"></param>
        public static void ExportPartnersInExtract(Form AParentForm, bool AOldPetraFormat)
        {
            int ExtractId = -1;
            String ExtractName;
            String ExtractDesc;
            int ExtractKeyCount;

            if (TCommonScreensForwarding.OpenExtractFindScreen != null)
            {
                while (TCommonScreensForwarding.OpenExtractFindScreen.Invoke(out ExtractId,
                           out ExtractName,
                           out ExtractDesc,
                           out ExtractKeyCount,
                           AParentForm))
                {
                    // Indicates that the user cancelled the operation...
                    if (ExtractId < 0)
                    {
                        return;
                    }

                    if (ExtractKeyCount > 0)
                    {
                        TPartnerExportLogic.ExportPartnersInExtract(ExtractId, AOldPetraFormat);
                        break;
                    }
                    else
                    {
                        DialogResult result;
                        result =
                            MessageBox.Show(Catalog.GetString(
                                    "The selected extract doesn't contain any partners. Would you like to choose a different extract?"),
                                Catalog.GetString(
                                    "Export Partners"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (result == DialogResult.No)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                throw new EOPAppException(
                    "DEVELOPER ERROR: TDelegateOpenExtractFindScreen Delegate must be assigned on TCommonScreensForwarding.OpenExtractFindScreen to be able to open an Extract find dialog!");
            }
        }
    }
}