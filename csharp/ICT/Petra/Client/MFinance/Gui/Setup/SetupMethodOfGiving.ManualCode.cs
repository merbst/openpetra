//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
//
// Copyright 2004-2010 by OM International
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
using Ict.Common;
using Ict.Petra.Client.App.Gui;
using Ict.Petra.Shared;
using Ict.Petra.Shared.MFinance.Account.Data;
using Ict.Petra.Client.CommonDialogs;

namespace Ict.Petra.Client.MFinance.Gui.Setup
{
    public partial class TFrmSetupMethodOfGiving
    {
        private void NewRow(System.Object sender, EventArgs e)
        {
            CreateNewAMethodOfGiving();
        }

        private void NewRowManual(ref Ict.Petra.Shared.MFinance.Account.Data.AMethodOfGivingRow ARow)
        {
            string newName = Ict.Common.Catalog.GetString("NEWTYPE");
            Int32 countNewDetail = 0;

            if (FMainDS.AMethodOfGiving.Rows.Find(new object[] { newName }) != null)
            {
                while (FMainDS.AMethodOfGiving.Rows.Find(new object[] { newName + countNewDetail.ToString() }) != null)
                {
                    countNewDetail++;
                }

                newName += countNewDetail.ToString();
            }

            ARow.MethodOfGivingCode = newName;
            ARow.MethodOfGivingDesc = Catalog.GetString("PLEASE ENTER DESCRIPTION");
        }

        private void PrintGrid(TStandardFormPrint.TPrintUsing APrintApplication, bool APreviewMode)
        {
            TFrmSelectPrintFields.SelectAndPrintGridFields(this, APrintApplication, APreviewMode, TModule.mPartner, this.Text, grdDetails,
                new int[]
                {
                    AMethodOfGivingTable.ColumnMethodOfGivingCodeId,
                    AMethodOfGivingTable.ColumnMethodOfGivingDescId,
                    AMethodOfGivingTable.ColumnActiveId
                });
        }
    }
}