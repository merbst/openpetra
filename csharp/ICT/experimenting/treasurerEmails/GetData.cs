/*************************************************************************
 *
 * DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 *
 * @Authors:
 *       timop
 *
 * Copyright 2004-2009 by OM International
 *
 * This file is part of OpenPetra.org.
 *
 * OpenPetra.org is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * OpenPetra.org is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with OpenPetra.org.  If not, see <http://www.gnu.org/licenses/>.
 *
 ************************************************************************/
using System;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Mail;
using Mono.Unix;
using Ict.Common.DB;
using Ict.Common;
using Ict.Common.Printing;
using Ict.Petra.Shared.MPartner;
using Ict.Petra.Shared.MPartner.Partner.Data;
using Ict.Petra.Server.MPartner.Partner.Data.Access;
using Ict.Petra.Shared.MFinance.Account.Data;
using Ict.Petra.Server.MFinance.Account.Data.Access;

namespace treasurerEmails
{
public class TGetTreasurerData
{
    public const string GIFTSTABLE = "giftsums";
    public const string TREASURERTABLE = "treasurer";

    /// <summary>
    /// open the db connection, and retrieve all sums of donations for each treasurer for the last x months
    /// <param name="ADBUsername"></param>
    /// <param name="ADBPassword"></param>
    /// <param name="ALedgerNumber"></param>
    /// <param name="AMotivationGroup"></param>
    /// <param name="AMotivationDetail"></param>
    /// <param name="ALastDonationDate"></param>
    /// <param name="ANumberMonths"></param>
    /// <returns></returns>
    public static DataSet GetTreasurerData(
        string ADBUsername, string ADBPassword,
        Int32 ALedgerNumber,
        string AMotivationGroup,
        string AMotivationDetail,
        DateTime ALastDonationDate, Int16 ANumberMonths)
    {
        // establish connection to database
        TAppSettingsManager settings = new TAppSettingsManager();

        TDataBase db = new TDataBase();

        TDBType dbtype = CommonTypes.ParseDBType(settings.GetValue("Server.RDBMSType"));

        if (dbtype != TDBType.ProgressODBC)
        {
            throw new Exception("at the moment only Progress ODBC db is supported");
        }

        db.EstablishDBConnection(dbtype,
            settings.GetValue("Server.ODBC_DSN"),
            "",
            ADBUsername,
            ADBPassword,
            "");
        DBAccess.GDBAccessObj = db;

        //db.DebugLevel = 10;

        // calculate the first and the last days of the months range
        // last donation date covers the full month
        DateTime EndDate =
            new DateTime(ALastDonationDate.Year, ALastDonationDate.Month, DateTime.DaysInMonth(ALastDonationDate.Year, ALastDonationDate.Month));

        DateTime StartDate = EndDate.AddMonths(-1 * (ANumberMonths - 1));
        StartDate = new DateTime(StartDate.Year, StartDate.Month, 1);

        DataTable GiftsTable = GetAllGiftsForRecipientPerMonthByMotivation(ALedgerNumber, AMotivationGroup, AMotivationDetail, StartDate, EndDate);

        DataTable TreasurerTable = new DataTable(TREASURERTABLE);
        TreasurerTable.Columns.Add(new DataColumn("TreasurerKey", typeof(Int64)));
        TreasurerTable.Columns.Add(new DataColumn("TreasurerName", typeof(string)));
        TreasurerTable.Columns.Add(new DataColumn("RecipientKey", typeof(Int64)));
        TreasurerTable.Columns.Add(new DataColumn("Transition", typeof(bool)));
        TreasurerTable.Columns.Add(new DataColumn("ExWorker", typeof(bool)));

        DataSet ResultDataset = new DataSet();

        ResultDataset.Tables.Add(GiftsTable);
        ResultDataset.Tables.Add(TreasurerTable);

        // add the last date of the month to the table
        AddMonthDate(ref GiftsTable, ALedgerNumber);

        // get the treasurer(s) for each recipient; get their name and partner key
        AddTreasurer(ref ResultDataset, EndDate, ALedgerNumber);

        // get the name of each recipient
        AddRecipientName(ref TreasurerTable);

        // use GetBestAddress to get the email address of the treasurer
        AddTreasurerEmailOrPostalAddress(ref TreasurerTable);

        return ResultDataset;
    }

    private static string ReadSqlFile(string ASqlFilename)
    {
        string path = TAppSettingsManager.GetValueStatic("SqlFiles.Path", ".");

        StreamReader reader = new StreamReader(path + Path.DirectorySeparatorChar + ASqlFilename);
        string line = null;
        string stmt = "";

        while ((line = reader.ReadLine()) != null)
        {
            if (!line.Trim().StartsWith("--"))
            {
                stmt += line.Trim() + " ";
            }
        }

        reader.Close();
        return stmt;
    }

    /// <summary>
    /// Get the sum of all gifts per recipient per month, by specified motivation and time span
    /// </summary>
    /// <param name="ALedgerNumber"></param>
    /// <param name="AMotivationGroup"></param>
    /// <param name="AMotivationDetail"></param>
    /// <param name="AStartDate"></param>
    /// <param name="AEndDate"></param>
    /// <returns> returns a table with columns:
    ///    RecipientKey, MonthAmount, FinancialYear, FinancialPeriod</returns>
    private static DataTable GetAllGiftsForRecipientPerMonthByMotivation(
        Int32 ALedgerNumber,
        string AMotivationGroup,
        string AMotivationDetail,
        DateTime AStartDate,
        DateTime AEndDate)
    {
        TDBTransaction transaction = DBAccess.GDBAccessObj.BeginTransaction(IsolationLevel.ReadUncommitted);

        string stmt = ReadSqlFile("GetAllGiftsForRecipientPerMonthByMotivation.sql");

        OdbcParameter[] parameters = new OdbcParameter[6];
        parameters[0] = new OdbcParameter("Ledger", ALedgerNumber);
        parameters[1] = new OdbcParameter("MotivationGroup", AMotivationGroup);
        parameters[2] = new OdbcParameter("MotivationDetail", AMotivationDetail);
        parameters[3] = new OdbcParameter("StartDate", AStartDate);
        parameters[4] = new OdbcParameter("EndDate", AEndDate);
        parameters[5] = new OdbcParameter("HomeOffice", ALedgerNumber * 1000000L);
        DataTable ResultTable = DBAccess.GDBAccessObj.SelectDT(stmt, GIFTSTABLE, transaction,
            parameters);


        DBAccess.GDBAccessObj.RollbackTransaction();

        return ResultTable;
    }

    /// <summary>
    /// The previous query only retrieves the financial year number and period number,
    /// but we want the last date of that period to be added to the result table
    /// </summary>
    /// <param name="ResultTable"></param>
    /// <param name="ALedgerNumber"></param>
    private static void AddMonthDate(ref DataTable ResultTable, Int32 ALedgerNumber)
    {
        TDBTransaction transaction = DBAccess.GDBAccessObj.BeginTransaction(IsolationLevel.ReadUncommitted);

        // get the accounting period table to know the month
        AAccountingPeriodTable periods = AAccountingPeriodAccess.LoadAll(transaction);

        OdbcParameter[] parameters = new OdbcParameter[1];
        parameters[0] = new OdbcParameter("Ledger", ALedgerNumber);
        Int32 currentFinancialYear =
            Convert.ToInt32(DBAccess.GDBAccessObj.ExecuteScalar("SELECT a_current_financial_year_i FROM PUB_a_ledger WHERE a_ledger_number_i = ?",
                    transaction, parameters));

        ResultTable.Columns.Add("MonthDate", typeof(DateTime));

        foreach (DataRow row in ResultTable.Rows)
        {
            Int32 yearNr = Convert.ToInt32(row["FinancialYear"]);
            Int32 periodNr = Convert.ToInt32(row["FinancialPeriod"]);

            DateTime monthDate = DateTime.MinValue;

            foreach (AAccountingPeriodRow period in periods.Rows)
            {
                if ((period.LedgerNumber == ALedgerNumber) && (period.AccountingPeriodNumber == periodNr))
                {
                    monthDate = period.PeriodEndDate;
                }
            }

            if (yearNr != currentFinancialYear)
            {
                // substract the years to get the right date
                TLogging.Log("substract year " + yearNr.ToString() + " " + currentFinancialYear.ToString() + " " + monthDate.Year.ToString() + " " +
                    row["RecipientKey"].ToString());
                monthDate = monthDate.AddYears(-1 * (currentFinancialYear - yearNr));
            }

            row["MonthDate"] = monthDate;
        }

        DBAccess.GDBAccessObj.RollbackTransaction();
    }

    /// <summary>
    /// get the name of the recipient and add to the table
    /// </summary>
    /// <param name="ResultTable"></param>
    private static void AddRecipientName(ref DataTable ResultTable)
    {
        TDBTransaction transaction = DBAccess.GDBAccessObj.BeginTransaction(IsolationLevel.ReadUncommitted);

        ResultTable.Columns.Add("RecipientName", typeof(string));

        foreach (DataRow row in ResultTable.Rows)
        {
            OdbcParameter[] parameters = new OdbcParameter[1];
            parameters[0] = new OdbcParameter("PartnerKey", row["RecipientKey"]);
            string shortname = DBAccess.GDBAccessObj.ExecuteScalar(
                "SELECT p_partner_short_name_c FROM PUB_p_partner WHERE p_partner_key_n = ?",
                transaction, parameters).ToString();

            row["RecipientName"] = shortname;
        }

        DBAccess.GDBAccessObj.RollbackTransaction();
    }

    /// <summary>
    /// get the treasurer(s) for each recipient;
    /// get their name and partner key
    /// </summary>
    private static void AddTreasurer(ref DataSet Result, DateTime AEndDate, Int32 ALedgerNumber)
    {
        TDBTransaction transaction = DBAccess.GDBAccessObj.BeginTransaction(IsolationLevel.ReadUncommitted);

        // to check if the recipient has been processed already
        List <Int64>recipients = new List <Int64>();

        DataTable GiftSumsTable = Result.Tables[GIFTSTABLE];

        foreach (DataRow row in GiftSumsTable.Rows)
        {
            Int64 recipientKey = Convert.ToInt64(row["RecipientKey"]);

            if (recipients.Contains(recipientKey))
            {
                continue;
            }

            recipients.Add(recipientKey);

            // first check if the worker has a valid commitment period, or is in TRANSITION
            bool bTransition = false;
            bool bExWorker = false;

            OdbcParameter[] parameters = new OdbcParameter[4];
            parameters[0] = new OdbcParameter("PartnerKey", recipientKey);
            parameters[1] = new OdbcParameter("HomeOffice", ALedgerNumber * 1000000L);
            parameters[2] = new OdbcParameter("EndOfPeriod", AEndDate);
            parameters[3] = new OdbcParameter("EndOfPeriod", AEndDate);

            string stmt = ReadSqlFile("CommitmentsOfWorker.sql");

            DataTable CommitmentsTable = DBAccess.GDBAccessObj.SelectDT(stmt,
                "temp", transaction,
                parameters);

            if (CommitmentsTable.Rows.Count == 0)
            {
                bExWorker = true;
            }
            else if (CommitmentsTable.Rows.Count == 1)
            {
                if (CommitmentsTable.Rows[0][0].ToString() == "TRANSITION")
                {
                    bTransition = true;
                }
            }

            parameters = new OdbcParameter[1];
            parameters[0] = new OdbcParameter("PartnerKey", recipientKey);

            stmt = ReadSqlFile("TreasurerOfWorker.sql");

            DataTable TreasurerTable = DBAccess.GDBAccessObj.SelectDT(stmt,
                TREASURERTABLE, transaction,
                parameters);

            if (TreasurerTable.Rows.Count >= 1)
            {
                TreasurerTable.Columns.Add(new DataColumn("Transition", typeof(bool)));
                TreasurerTable.Columns.Add(new DataColumn("ExWorker", typeof(bool)));

                foreach (DataRow r in TreasurerTable.Rows)
                {
                    r["Transition"] = bTransition;
                    r["ExWorker"] = bExWorker;
                }

                Result.Tables[TREASURERTABLE].Merge(TreasurerTable);
            }
            else
            {
                DataRow InvalidTreasurer = Result.Tables[TREASURERTABLE].NewRow();
                InvalidTreasurer["RecipientKey"] = row["RecipientKey"];
                InvalidTreasurer["Transition"] = bTransition;
                InvalidTreasurer["ExWorker"] = bExWorker;
                Result.Tables[TREASURERTABLE].Rows.Add(InvalidTreasurer);
            }
        }

        DBAccess.GDBAccessObj.RollbackTransaction();
    }

    /// <summary>
    /// get the email address or the postal address of the treasurer and add to the table
    /// </summary>
    /// <param name="ResultTable"></param>
    private static void AddTreasurerEmailOrPostalAddress(ref DataTable ResultTable)
    {
        ResultTable.Columns.Add("TreasurerEmail", typeof(string));
        ResultTable.Columns.Add("ValidAddress", typeof(bool));
        ResultTable.Columns.Add("TreasurerLocality", typeof(string));
        ResultTable.Columns.Add("TreasurerStreetName", typeof(string));
        ResultTable.Columns.Add("TreasurerBuilding1", typeof(string));
        ResultTable.Columns.Add("TreasurerBuilding2", typeof(string));
        ResultTable.Columns.Add("TreasurerAddress3", typeof(string));
        ResultTable.Columns.Add("TreasurerCountryCode", typeof(string));
        ResultTable.Columns.Add("TreasurerPostalCode", typeof(string));
        ResultTable.Columns.Add("TreasurerCity", typeof(string));

        foreach (DataRow row in ResultTable.Rows)
        {
            if (row["TreasurerKey"] != DBNull.Value)
            {
                PLocationTable Address;
                string emailAddress = GetBestEmailAddress(Convert.ToInt64(row["TreasurerKey"]), out Address);

                if (emailAddress.Length > 0)
                {
                    row["TreasurerEmail"] = emailAddress;
                }

                row["ValidAddress"] = (Address != null);

                if (Address == null)
                {
                    // no best address; only report if emailAddress is empty as well???
                    continue;
                }

                if (!Address[0].IsLocalityNull())
                {
                    row["TreasurerLocality"] = Address[0].Locality;
                }

                if (!Address[0].IsStreetNameNull())
                {
                    row["TreasurerStreetName"] = Address[0].StreetName;
                }

                if (!Address[0].IsBuilding1Null())
                {
                    row["TreasurerBuilding1"] = Address[0].Building1;
                }

                if (!Address[0].IsBuilding2Null())
                {
                    row["TreasurerBuilding2"] = Address[0].Building2;
                }

                if (!Address[0].IsAddress3Null())
                {
                    row["TreasurerAddress3"] = Address[0].Address3;
                }

                if (!Address[0].IsCountryCodeNull())
                {
                    row["TreasurerCountryCode"] = Address[0].CountryCode;
                }

                if (!Address[0].IsPostalCodeNull())
                {
                    row["TreasurerPostalCode"] = Address[0].PostalCode;
                }

                if (!Address[0].IsCityNull())
                {
                    row["TreasurerCity"] = Address[0].City;
                }
            }
        }
    }

    private static string GetBestEmailAddress(Int64 APartnerKey, out PLocationTable AAddress)
    {
        string EmailAddress = "";
        TDBTransaction Transaction = DBAccess.GDBAccessObj.BeginTransaction(IsolationLevel.ReadUncommitted);

        AAddress = null;

        DataSet PartnerLocationsDS = new DataSet();

        PartnerLocationsDS.Tables.Add(new PPartnerLocationTable());
        DataTable PartnerLocationTable = PartnerLocationsDS.Tables[PPartnerLocationTable.GetTableName()];

        // add special column BestAddress and Icon
        PartnerLocationTable.Columns.Add(new System.Data.DataColumn("BestAddress", typeof(Boolean)));
        PartnerLocationTable.Columns.Add(new System.Data.DataColumn("Icon", typeof(Int32)));

        // find all locations of the partner, put it into a dataset
        PPartnerLocationAccess.LoadViaPPartner(PartnerLocationsDS, APartnerKey, Transaction);

        Ict.Petra.Shared.MPartner.Calculations.DeterminePartnerLocationsDateStatus(PartnerLocationsDS);
        Ict.Petra.Shared.MPartner.Calculations.DetermineBestAddress(PartnerLocationsDS);

        foreach (PPartnerLocationRow row in PartnerLocationTable.Rows)
        {
            // find the row with BestAddress = 1
            if (Convert.ToInt32(row["BestAddress"]) == 1)
            {
                if (!row.IsEmailAddressNull())
                {
                    EmailAddress = row.EmailAddress;
                }

                // we also want the post address, need to load the p_location table:
                AAddress = PLocationAccess.LoadByPrimaryKey(row.SiteKey, row.LocationKey, Transaction);
            }
        }

        DBAccess.GDBAccessObj.RollbackTransaction();

        return EmailAddress;
    }

    public static List <LetterMessage>GenerateMessages(DataSet ATreasurerData, string ASenderEmailAddress, bool AForceLetters, DateTime AEndDate)
    {
        List <LetterMessage>messages = new List <LetterMessage>();

        DataView view = ATreasurerData.Tables[TREASURERTABLE].DefaultView;
        view.Sort = "TreasurerName ASC";

        foreach (DataRowView rowview in view)
        {
            DataRow row = rowview.Row;

            string treasurerName = "";
            Int64 treasurerKey = -1;
            string errorMessage = "NOTREASURER";

            if (row["TreasurerKey"] != System.DBNull.Value)
            {
                treasurerName = row["TreasurerName"].ToString();
                treasurerKey = Convert.ToInt64(row["TreasurerKey"]);
                errorMessage = String.Empty;
            }

            if (Convert.ToBoolean(row["ExWorker"]) == true)
            {
                errorMessage = "EXWORKER";
                bool bRecentGift = false;

                // check if there has been a gift in the last month of the reporting period
                DataRow[] rowGifts = ATreasurerData.Tables[GIFTSTABLE].Select("RecipientKey = " + row["RecipientKey"].ToString(), "MonthDate");

                if (rowGifts.Length > 0)
                {
                    DateTime month = Convert.ToDateTime(rowGifts[rowGifts.Length - 1]["MonthDate"]);
                    bRecentGift = (month.Month == AEndDate.Month);
                }

                if (!bRecentGift)
                {
                    continue;
                }
            }

            LetterMessage letter = new LetterMessage(
                treasurerName,
                String.Format(Catalog.GetString("Gifts for {0}"), row["RecipientName"]),
                GenerateSimpleDebugString(ATreasurerData, row));

            if (AForceLetters
                || (row["TreasurerEmail"] == System.DBNull.Value)
                || (row["TreasurerKey"] == System.DBNull.Value))
            {
                if ((row["TreasurerKey"] != System.DBNull.Value)
                    && (row["TreasurerCity"] == System.DBNull.Value)
                    && (errorMessage.Length == 0))
                {
                    errorMessage = "NOADDRESS";
                }

                letter.HtmlMessage = GenerateLetterText(ATreasurerData, row);
            }
            else
            {
                letter.EmailMessage = GenerateMailMessage(ATreasurerData, row, ASenderEmailAddress);
            }

            letter.MessageRecipientKey = treasurerKey;
            letter.MessageRecipientShortName = treasurerName;
            letter.SubjectShortName = row["RecipientName"].ToString();
            letter.SubjectKey = Convert.ToInt64(row["RecipientKey"]);
            letter.ErrorMessage = errorMessage;
            letter.Transition = Convert.ToBoolean(row["Transition"]);
            messages.Add(letter);
        }

        return messages;
    }

    private enum eShortNameFormat
    {
        eShortname, eReverseShortname, eOnlyTitle, eReverseWithoutTitle
    };

    /// <summary>
    /// convert shortname from Lastname, firstname, title to title firstname lastname
    /// TODO: use partner key to get to the full name, resolve issues with couples that have different family names etc
    /// TODO: move this function to a central place in Ict.Petra.Shared?
    /// </summary>
    private static string FormatShortName(string AShortname, eShortNameFormat AFormat)
    {
        StringCollection names = StringHelper.StrSplit(AShortname, ",");
        string resultValue = "";

        if (AFormat == eShortNameFormat.eShortname)
        {
            return AShortname;
        }
        else if (AFormat == eShortNameFormat.eReverseShortname)
        {
            foreach (string name in names)
            {
                if (resultValue.Length > 0)
                {
                    resultValue = " " + resultValue;
                }

                resultValue = name + resultValue;
            }

            return resultValue;
        }
        else if (AFormat == eShortNameFormat.eOnlyTitle)
        {
            return names[names.Count - 1];
        }
        else if (AFormat == eShortNameFormat.eReverseWithoutTitle)
        {
            names.RemoveAt(names.Count - 1);

            foreach (string name in names)
            {
                if (resultValue.Length > 0)
                {
                    resultValue = " " + resultValue;
                }

                resultValue = name + resultValue;
            }

            return resultValue;
        }

        return "";
    }

    private static string GetStringOrEmpty(object obj)
    {
        if (obj == System.DBNull.Value)
        {
            return "";
        }

        return obj.ToString();
    }

    /// <summary>
    /// generate the printed letter for one treasurer, one worker
    /// </summary>
    /// <param name="ATreasurerData"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    private static string GenerateLetterText(DataSet ATreasurerData, DataRow row)
    {
        string treasurerName = row["TreasurerName"].ToString();
        Int64 recipientKey = Convert.ToInt64(row["RecipientKey"]);

        string letterTemplateFilename = TAppSettingsManager.GetValueStatic("LetterTemplate.File");

        // message body from HTML template
        StreamReader reader = new StreamReader(letterTemplateFilename);

        string msg = reader.ReadToEnd();

        msg = msg.Replace("#RECIPIENTNAME", FormatShortName(row["RecipientName"].ToString(), eShortNameFormat.eReverseWithoutTitle));
        msg = msg.Replace("#TREASURERTITLE", FormatShortName(treasurerName, eShortNameFormat.eOnlyTitle));
        msg = msg.Replace("#TREASURERNAME", FormatShortName(treasurerName, eShortNameFormat.eReverseWithoutTitle));
        msg = msg.Replace("#STREETNAME", GetStringOrEmpty(row["TreasurerStreetName"]));
        msg = msg.Replace("#LOCATION", GetStringOrEmpty(row["TreasurerLocality"]));
        msg = msg.Replace("#ADDRESS3", GetStringOrEmpty(row["TreasurerAddress3"]));
        msg = msg.Replace("#BUILDING1", GetStringOrEmpty(row["TreasurerBuilding1"]));
        msg = msg.Replace("#BUILDING2", GetStringOrEmpty(row["TreasurerBuilding2"]));
        msg = msg.Replace("#CITY", GetStringOrEmpty(row["TreasurerCity"]));
        msg = msg.Replace("#POSTALCODE", GetStringOrEmpty(row["TreasurerPostalCode"]));
        msg = msg.Replace("#COUNTRYCODE", GetStringOrEmpty(row["TreasurerCountryCode"]));
        msg = msg.Replace("#DATE", DateTime.Now.ToLongDateString());

        bool bTransition = Convert.ToBoolean(row["Transition"]);

        if (bTransition)
        {
            msg = TPrinterHtml.RemoveDivWithName(msg, "normal");
        }
        else
        {
            msg = TPrinterHtml.RemoveDivWithName(msg, "transition");
        }

        // recognise detail lines automatically
        string RowTemplate;
        msg = TPrinterHtml.GetTableRow(msg, "#MONTH", out RowTemplate);
        string rowTexts = "";
        DataRow[] rows = ATreasurerData.Tables[GIFTSTABLE].Select("RecipientKey = " + recipientKey.ToString(), "MonthDate");

        foreach (DataRow rowGifts in rows)
        {
            DateTime month = Convert.ToDateTime(rowGifts["MonthDate"]);

            rowTexts += RowTemplate.
                        Replace("#MONTH", month.ToString("MMMM yyyy")).
                        Replace("#AMOUNT", String.Format("{0:C}", Convert.ToDouble(rowGifts["MonthAmount"]))).
                        Replace("#NUMBERGIFTS", rowGifts["MonthCount"].ToString());
        }

        return msg.Replace("#ROWTEMPLATE", rowTexts);
    }

    /// <summary>
    /// generates a simple string with the list of donations per month, for one treasuer and one worker;
    /// this is useful for debugging, and looking at data of exworkers etc
    /// </summary>
    /// <param name="ATreasurerData"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    private static string GenerateSimpleDebugString(DataSet ATreasurerData, DataRow row)
    {
        string result = String.Empty;

        result += "Treasurer: " + row["TreasurerName"].ToString() + Environment.NewLine;
        result += "Treasurer Key: " + row["TreasurerKey"].ToString() + Environment.NewLine;
        result += "Recipient: " + row["RecipientName"].ToString() + Environment.NewLine;
        result += "Recipient Key: " + row["RecipientKey"].ToString() + Environment.NewLine;
        result += Environment.NewLine;

        DataRow[] rows = ATreasurerData.Tables[GIFTSTABLE].Select("RecipientKey = " + row["RecipientKey"].ToString(), "MonthDate");

        foreach (DataRow rowGifts in rows)
        {
            DateTime month = Convert.ToDateTime(rowGifts["MonthDate"]);
            result += month.ToString("MMMM yyyy") + ": ";
            result += String.Format("{0:C}", Convert.ToDouble(rowGifts["MonthAmount"])) + "   ";
            result += String.Format(", Number of Gifts: {0}", Convert.ToDouble(rowGifts["MonthCount"]));
            result += Environment.NewLine;
        }

        return result;
    }

    /// <summary>
    /// generate the email text for one treasurer, one worker
    /// </summary>
    private static MailMessage GenerateMailMessage(DataSet ATreasurerData, DataRow row, string ASenderEmailAddress)
    {
        string treasurerName = row["TreasurerName"].ToString();
        Int64 recipientKey = Convert.ToInt64(row["RecipientKey"]);

        // TODO: message body from HTML template; recognise detail lines automatically; drop title tag, because it is the subject
        string msg = String.Format(
            "<html><body>Hello {0}, <br/> This is a test. <br/> Donations so far: <br/>",
            treasurerName);

        msg += "<table>";

        DataRow[] rows = ATreasurerData.Tables[GIFTSTABLE].Select("RecipientKey = " + recipientKey.ToString(), "MonthDate");

        foreach (DataRow rowGifts in rows)
        {
            DateTime month = Convert.ToDateTime(rowGifts["MonthDate"]);
            msg += "<tr><td>" + month.ToString("MMMM yyyy") + "</td>";
            msg += "<td align=\"right\">" + String.Format("{0:C}", Convert.ToDouble(rowGifts["MonthAmount"])) + "</td>";
            msg += "<td>" + String.Format("  {0}", Convert.ToDouble(rowGifts["MonthCount"])) + "</td>";
            msg += "</tr>";
        }

        msg += "</table><br/>All the best, </body></html>";

        string treasurerEmail = row["TreasurerEmail"].ToString();

        // TODO: subject also from HTML template, title tag
        return new MailMessage(ASenderEmailAddress,
            treasurerEmail,
            String.Format(Catalog.GetString("Gifts for {0}"), row["RecipientName"]),
            msg);
    }
}

public class LetterMessage
{
    public string MessageRecipientShortName;
    public Int64 MessageRecipientKey;
    public string SubjectShortName;
    public Int64 SubjectKey;
    public string Subject;
    public string HtmlMessage;
    public MailMessage EmailMessage;
    public string ErrorMessage = String.Empty;
    public string SimpleDebuggingText;

    /// <summary>
    ///  Worker has left, but is still in transition, gifts are still coming in
    /// </summary>
    public bool Transition = false;
    public LetterMessage(string ARecipientShortName, string ASubject, string ASimpleText)
    {
        MessageRecipientShortName = ARecipientShortName;
        Subject = ASubject;
        SimpleDebuggingText = ASimpleText;
    }

    public bool SendAsEmail()
    {
        return EmailMessage != null && ErrorMessage == String.Empty;
    }

    public bool SendAsLetter()
    {
        return HtmlMessage != null && ErrorMessage == String.Empty;
    }

    public override string ToString()
    {
        return SimpleDebuggingText;
    }
}
}