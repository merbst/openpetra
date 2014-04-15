DELETE FROM s_report_template WHERE s_readonly_l=TRUE;
INSERT INTO s_report_template (s_template_id_i,s_report_type_c,s_report_variant_c,s_author_c,s_default_l,s_readonly_l,s_xml_text_c)
VALUES(3,'Account Detail','OpenPetra default template','System',True,True,
'﻿<?xml version="1.0" encoding="utf-8"?>
<Report ScriptLanguage="CSharp" DoublePass="true" ReportInfo.Created="11/05/2013 15:46:27" ReportInfo.Modified="03/17/2014 15:14:22" ReportInfo.CreatorVersion="2013.4.4.0">
  <ScriptText>using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using System.Globalization;
using FastReport;
using FastReport.Data;
using FastReport.Dialog;
using FastReport.Barcode;
using FastReport.Table;
using FastReport.Utils;

namespace FastReport
{
  public class ReportScript
  {
    String OmCurrency(Decimal Amount)
    {
      return Amount.ToString((String)Report.GetParameterValue(&quot;param_currency_formatter&quot;), CultureInfo.InvariantCulture);
    }
    
    String OmDate(DateTime fld)
    {
      return fld.ToString(&quot;dd-MMM-yyyy&quot;);
    }
    
    String GroupHeader ()
    {
      switch ((String)Report.GetParameterValue(&quot;param_sortby&quot;))
      {
        case &quot;Cost Centre&quot; :
        case &quot;Account&quot; : return 
             (String)Report.GetColumnValue(&quot;a_transaction.a_cost_centre_code_c&quot;) + &quot;-&quot;
            +(String)Report.GetColumnValue(&quot;a_transaction.a_account_code_c&quot;) + &quot; &quot;
            +(String)Report.GetColumnValue(&quot;a_transaction.a_account.a_account_code_long_desc_c&quot;) + &quot;, &quot;
            +(String)Report.GetColumnValue(&quot;a_transaction.a_costCentre.a_cost_centre_name_c&quot;);
        case &quot;Reference&quot; : return
            &quot;Reference : &quot; + ((String)Report.GetColumnValue(&quot;a_transaction.a_reference_c&quot;));
        case &quot;Analysis Type&quot; : return
             (String)Report.GetColumnValue(&quot;a_transaction.a_analysis_type_code_c&quot;) + &quot;: &quot;
            +(String)Report.GetColumnValue(&quot;a_transaction.a_analysis_type_description_c&quot;);
        default: return &quot;Group&quot;;
      }
    }
    
    Decimal OpeningBalance()
    {
      String Account = (String)Report.GetColumnValue(&quot;a_transaction.a_account_code_c&quot;);
      string CostCentre = (String)Report.GetColumnValue(&quot;a_transaction.a_cost_centre_code_c&quot;);
      return 0;
    }
  }
}
</ScriptText>
  <Dictionary>
    <TableDataSource Name="a_analysis_type" ReferenceName="a_analysis_type" DataType="System.Int32" Enabled="true">
      <Column Name="a_analysis_type_code_c" DataType="System.String"/>
      <Column Name="a_analysis_type_description_c" DataType="System.String"/>
      <Column Name="a_analysis_mode_l" DataType="System.String"/>
      <Column Name="a_analysis_store_c" DataType="System.String"/>
      <Column Name="a_analysis_element_c" DataType="System.String"/>
      <Column Name="a_system_analysis_type_l" DataType="System.String"/>
      <Column Name="s_date_created_d" DataType="System.DateTime"/>
      <Column Name="s_created_by_c" DataType="System.String"/>
      <Column Name="s_date_modified_d" DataType="System.DateTime"/>
      <Column Name="s_modified_by_c" DataType="System.String"/>
      <Column Name="s_modification_id_t" DataType="System.DateTime"/>
    </TableDataSource>
    <TableDataSource Name="a_trans_anal_attrib" ReferenceName="a_trans_anal_attrib" DataType="System.Int32" Enabled="true">
      <Column Name="a_ledger_number_i" DataType="System.Int32"/>
      <Column Name="a_batch_number_i" DataType="System.Int32"/>
      <Column Name="a_journal_number_i" DataType="System.Int32"/>
      <Column Name="a_transaction_number_i" DataType="System.Int32"/>
      <Column Name="a_account_code_c" DataType="System.String"/>
      <Column Name="a_cost_centre_code_c" DataType="System.String"/>
      <Column Name="a_analysis_type_code_c" DataType="System.String"/>
      <Column Name="a_analysis_attribute_value_c" DataType="System.String"/>
      <Column Name="s_date_created_d" DataType="System.DateTime"/>
      <Column Name="s_created_by_c" DataType="System.String"/>
      <Column Name="s_date_modified_d" DataType="System.DateTime"/>
      <Column Name="s_modified_by_c" DataType="System.String"/>
      <Column Name="s_modification_id_t" DataType="System.DateTime"/>
    </TableDataSource>
    <TableDataSource Name="a_transaction" ReferenceName="a_transaction" DataType="System.Int32" Enabled="true">
      <Column Name="a_ledger_number_i" DataType="System.Int32"/>
      <Column Name="a_batch_number_i" DataType="System.Int32"/>
      <Column Name="a_journal_number_i" DataType="System.Int32"/>
      <Column Name="a_transaction_number_i" DataType="System.Int32"/>
      <Column Name="a_account_code_c" DataType="System.String"/>
      <Column Name="a_primary_account_code_c" DataType="System.String"/>
      <Column Name="a_cost_centre_code_c" DataType="System.String"/>
      <Column Name="a_primary_cost_centre_code_c" DataType="System.String"/>
      <Column Name="a_transaction_date_d" DataType="System.DateTime"/>
      <Column Name="a_transaction_amount_n" DataType="System.Decimal"/>
      <Column Name="a_amount_in_base_currency_n" DataType="System.Decimal"/>
      <Column Name="a_analysis_indicator_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_reconciled_status_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_narrative_c" DataType="System.String"/>
      <Column Name="a_debit_credit_indicator_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_transaction_status_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_header_number_i" DataType="System.Int32"/>
      <Column Name="a_detail_number_i" DataType="System.Int32"/>
      <Column Name="a_sub_type_c" DataType="System.String"/>
      <Column Name="a_to_ilt_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_source_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_reference_c" DataType="System.String"/>
      <Column Name="a_source_reference_c" DataType="System.String"/>
      <Column Name="a_system_generated_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_amount_in_intl_currency_n" DataType="System.Decimal"/>
      <Column Name="a_ich_number_i" DataType="System.Int32"/>
      <Column Name="a_key_ministry_key_n" DataType="System.Int64"/>
      <Column Name="s_date_created_d" DataType="System.DateTime"/>
      <Column Name="s_created_by_c" DataType="System.String"/>
      <Column Name="s_date_modified_d" DataType="System.DateTime"/>
      <Column Name="s_modified_by_c" DataType="System.String"/>
      <Column Name="s_modification_id_t" DataType="System.DateTime"/>
      <Column Name="a_analysis_type_code_c" DataType="System.String"/>
      <Column Name="a_analysis_type_description_c" DataType="System.String"/>
      <Column Name="a_analysis_attribute_value_c" DataType="System.String"/>
    </TableDataSource>
    <TableDataSource Name="a_account" ReferenceName="a_account" DataType="System.Int32" Enabled="true">
      <Column Name="a_ledger_number_i" DataType="System.Int32"/>
      <Column Name="a_account_code_c" DataType="System.String"/>
      <Column Name="a_account_type_c" DataType="System.String"/>
      <Column Name="a_account_code_long_desc_c" DataType="System.String"/>
      <Column Name="a_account_code_short_desc_c" DataType="System.String"/>
      <Column Name="a_eng_account_code_short_desc_c" DataType="System.String"/>
      <Column Name="a_eng_account_code_long_desc_c" DataType="System.String"/>
      <Column Name="a_debit_credit_indicator_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_account_active_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_analysis_attribute_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_standard_account_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_consolidation_account_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_intercompany_account_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_budget_type_code_c" DataType="System.String"/>
      <Column Name="a_posting_status_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_system_account_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_budget_control_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_valid_cc_combo_c" DataType="System.String"/>
      <Column Name="a_foreign_currency_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_foreign_currency_code_c" DataType="System.String"/>
      <Column Name="p_banking_details_key_i" DataType="System.Int32"/>
      <Column Name="s_date_created_d" DataType="System.DateTime"/>
      <Column Name="s_created_by_c" DataType="System.String"/>
      <Column Name="s_date_modified_d" DataType="System.DateTime"/>
      <Column Name="s_modified_by_c" DataType="System.String"/>
      <Column Name="s_modification_id_t" DataType="System.DateTime"/>
    </TableDataSource>
    <TableDataSource Name="a_costCentre" ReferenceName="a_costCentre" DataType="System.Int32" Enabled="true">
      <Column Name="a_ledger_number_i" DataType="System.Int32"/>
      <Column Name="a_cost_centre_code_c" DataType="System.String"/>
      <Column Name="a_cost_centre_to_report_to_c" DataType="System.String"/>
      <Column Name="a_cost_centre_name_c" DataType="System.String"/>
      <Column Name="a_posting_cost_centre_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_cost_centre_active_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_project_status_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_project_constraint_date_d" DataType="System.DateTime"/>
      <Column Name="a_project_constraint_amount_n" DataType="System.Decimal"/>
      <Column Name="a_system_cost_centre_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_cost_centre_type_c" DataType="System.String"/>
      <Column Name="a_key_focus_area_c" DataType="System.String"/>
      <Column Name="s_date_created_d" DataType="System.DateTime"/>
      <Column Name="s_created_by_c" DataType="System.String"/>
      <Column Name="s_date_modified_d" DataType="System.DateTime"/>
      <Column Name="s_modified_by_c" DataType="System.String"/>
      <Column Name="s_modification_id_t" DataType="System.DateTime"/>
    </TableDataSource>
    <TableDataSource Name="a_ledger" ReferenceName="a_ledger" DataType="System.Int32" Enabled="true">
      <Column Name="a_ledger_number_i" DataType="System.Int32"/>
      <Column Name="a_ledger_name_c" DataType="System.String"/>
      <Column Name="a_ledger_status_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_last_batch_number_i" DataType="System.Int32"/>
      <Column Name="a_last_recurring_batch_number_i" DataType="System.Int32"/>
      <Column Name="a_last_gift_number_i" DataType="System.Int32"/>
      <Column Name="a_last_ap_inv_number_i" DataType="System.Int32"/>
      <Column Name="a_last_header_r_number_i" DataType="System.Int32"/>
      <Column Name="a_last_po_number_i" DataType="System.Int32"/>
      <Column Name="a_last_so_number_i" DataType="System.Int32"/>
      <Column Name="a_max_gift_aid_amount_n" DataType="System.Decimal"/>
      <Column Name="a_min_gift_aid_amount_n" DataType="System.Decimal"/>
      <Column Name="a_number_of_gifts_to_display_i" DataType="System.Int32"/>
      <Column Name="a_tax_type_code_c" DataType="System.String"/>
      <Column Name="a_ilt_gl_account_code_c" DataType="System.String"/>
      <Column Name="a_profit_loss_gl_account_code_c" DataType="System.String"/>
      <Column Name="a_current_accounting_period_i" DataType="System.Int32"/>
      <Column Name="a_number_of_accounting_periods_i" DataType="System.Int32"/>
      <Column Name="a_country_code_c" DataType="System.String"/>
      <Column Name="a_base_currency_c" DataType="System.String"/>
      <Column Name="a_transaction_account_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_year_end_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_forex_gains_losses_account_c" DataType="System.String"/>
      <Column Name="a_system_interface_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_suspense_account_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_bank_accounts_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_delete_ledger_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_new_financial_year_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_recalculate_gl_master_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_installation_id_c" DataType="System.String"/>
      <Column Name="a_budget_control_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_budget_data_retention_i" DataType="System.Int32"/>
      <Column Name="a_cost_of_sales_gl_account_c" DataType="System.String"/>
      <Column Name="a_creditor_gl_account_code_c" DataType="System.String"/>
      <Column Name="a_current_financial_year_i" DataType="System.Int32"/>
      <Column Name="a_current_period_i" DataType="System.Int32"/>
      <Column Name="a_date_cr_dr_balances_d" DataType="System.DateTime"/>
      <Column Name="a_debtor_gl_account_code_c" DataType="System.String"/>
      <Column Name="a_fa_depreciation_gl_account_c" DataType="System.String"/>
      <Column Name="a_fa_gl_account_code_c" DataType="System.String"/>
      <Column Name="a_fa_pl_on_sale_gl_account_c" DataType="System.String"/>
      <Column Name="a_fa_prov_for_depn_gl_account_c" DataType="System.String"/>
      <Column Name="a_ilt_account_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_last_ap_dn_number_i" DataType="System.Int32"/>
      <Column Name="a_last_po_ret_number_i" DataType="System.Int32"/>
      <Column Name="a_last_so_del_number_i" DataType="System.Int32"/>
      <Column Name="a_last_so_ret_number_i" DataType="System.Int32"/>
      <Column Name="a_last_special_gift_number_i" DataType="System.Int32"/>
      <Column Name="a_number_fwd_posting_periods_i" DataType="System.Int32"/>
      <Column Name="a_periods_per_financial_year_i" DataType="System.Int32"/>
      <Column Name="a_discount_allowed_pct_n" DataType="System.Decimal"/>
      <Column Name="a_discount_received_pct_n" DataType="System.Decimal"/>
      <Column Name="a_po_accrual_gl_account_code_c" DataType="System.String"/>
      <Column Name="a_provisional_year_end_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_purchase_gl_account_code_c" DataType="System.String"/>
      <Column Name="a_ret_earnings_gl_account_c" DataType="System.String"/>
      <Column Name="a_sales_gl_account_code_c" DataType="System.String"/>
      <Column Name="a_so_accrual_gl_account_code_c" DataType="System.String"/>
      <Column Name="a_stock_accrual_gl_account_c" DataType="System.String"/>
      <Column Name="a_stock_adj_gl_account_code_c" DataType="System.String"/>
      <Column Name="a_stock_gl_account_code_c" DataType="System.String"/>
      <Column Name="a_tax_excl_incl_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_tax_excl_incl_indicator_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_tax_input_gl_account_code_c" DataType="System.String"/>
      <Column Name="a_tax_input_gl_cc_code_c" DataType="System.String"/>
      <Column Name="a_tax_output_gl_account_code_c" DataType="System.String"/>
      <Column Name="a_terms_of_payment_code_c" DataType="System.String"/>
      <Column Name="a_last_po_rec_number_i" DataType="System.Int32"/>
      <Column Name="a_tax_gl_account_number_i" DataType="System.Int32"/>
      <Column Name="a_actuals_data_retention_i" DataType="System.Int32"/>
      <Column Name="p_partner_key_n" DataType="System.Int64"/>
      <Column Name="a_calendar_mode_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_year_end_process_status_i" DataType="System.Int32"/>
      <Column Name="a_last_header_p_number_i" DataType="System.Int32"/>
      <Column Name="a_ilt_processing_centre_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_last_gift_batch_number_i" DataType="System.Int32"/>
      <Column Name="a_intl_currency_c" DataType="System.String"/>
      <Column Name="a_last_rec_gift_batch_number_i" DataType="System.Int32"/>
      <Column Name="a_gift_data_retention_i" DataType="System.Int32"/>
      <Column Name="a_recalculate_all_periods_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_last_ich_number_i" DataType="System.Int32"/>
      <Column Name="a_branch_processing_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_consolidation_ledger_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="s_date_created_d" DataType="System.DateTime"/>
      <Column Name="s_created_by_c" DataType="System.String"/>
      <Column Name="s_date_modified_d" DataType="System.DateTime"/>
      <Column Name="s_modified_by_c" DataType="System.String"/>
      <Column Name="s_modification_id_t" DataType="System.DateTime"/>
    </TableDataSource>
    <TableDataSource Name="balances" ReferenceName="balances" DataType="System.Int32" Enabled="true">
      <Column Name="a_cost_centre_code_c" DataType="System.String"/>
      <Column Name="a_account_code_c" DataType="System.String"/>
      <Column Name="OpeningBalance" DataType="System.Decimal"/>
      <Column Name="ClosingBalance" DataType="System.Decimal"/>
    </TableDataSource>
    <Relation Name="a_account_a_transaction" ParentDataSource="a_account" ChildDataSource="a_transaction" ParentColumns="a_account_code_c" ChildColumns="a_account_code_c" Enabled="true"/>
    <Relation Name="a_costCentre_a_transaction" ParentDataSource="a_costCentre" ChildDataSource="a_transaction" ParentColumns="a_cost_centre_code_c" ChildColumns="a_cost_centre_code_c" Enabled="true"/>
    <Relation Name="a_trans_anal_attrib_a_transaction" ParentDataSource="a_trans_anal_attrib" ChildDataSource="a_transaction" ParentColumns="a_ledger_number_i&#13;&#10;a_batch_number_i&#13;&#10;a_journal_number_i&#13;&#10;a_transaction_number_i" ChildColumns="a_ledger_number_i&#13;&#10;a_batch_number_i&#13;&#10;a_journal_number_i&#13;&#10;a_transaction_number_i" Enabled="true"/>
    <Relation Name="a_analysis_type_a_trans_anal_attrib" ParentDataSource="a_analysis_type" ChildDataSource="a_trans_anal_attrib" ParentColumns="a_analysis_type_code_c" ChildColumns="a_analysis_type_code_c" Enabled="true"/>
    <Relation Name="balances_a_transaction" ParentDataSource="balances" ChildDataSource="a_transaction" ParentColumns="a_cost_centre_code_c&#13;&#10;a_account_code_c" ChildColumns="a_cost_centre_code_c&#13;&#10;a_account_code_c" Enabled="true"/>
    <Parameter Name="param_diff_period_i" DataType="System.Int32"/>
    <Parameter Name="param_account_hierarchy_c" DataType="System.String"/>
    <Parameter Name="param_currency" DataType="System.String"/>
    <Parameter Name="param_period" DataType="System.Boolean"/>
    <Parameter Name="param_date_checked" DataType="System.Boolean"/>
    <Parameter Name="param_start_period_i" DataType="System.Int32"/>
    <Parameter Name="param_end_period_i" DataType="System.Int32"/>
    <Parameter Name="param_year_i" DataType="System.Int32"/>
    <Parameter Name="param_start_date" DataType="System.DateTime"/>
    <Parameter Name="param_end_date" DataType="System.DateTime"/>
    <Parameter Name="param_sortby" DataType="System.String"/>
    <Parameter Name="param_account_list_title" DataType="System.String"/>
    <Parameter Name="param_account_codes" DataType="System.String"/>
    <Parameter Name="param_account_code_start" DataType="System.Int32"/>
    <Parameter Name="param_account_code_end" DataType="System.Int32"/>
    <Parameter Name="param_rgrAccounts" DataType="System.String"/>
    <Parameter Name="param_cost_centre_list_title" DataType="System.String"/>
    <Parameter Name="param_cost_centre_codes" DataType="System.String"/>
    <Parameter Name="param_cost_centre_code_start" DataType="System.String"/>
    <Parameter Name="param_cost_centre_code_end" DataType="System.String"/>
    <Parameter Name="param_rgrCostCentres" DataType="System.String"/>
    <Parameter Name="param_depth" DataType="System.String"/>
    <Parameter Name="param_ledger_number_i" DataType="System.Int32"/>
    <Parameter Name="param_with_analysis_attributes" DataType="System.Boolean"/>
    <Parameter Name="param_quarter" DataType="System.String"/>
    <Parameter Name="param_daterange" DataType="System.String"/>
    <Parameter Name="param_groupfield" DataType="System.String"/>
    <Parameter Name="param_currency_formatter" DataType="System.String"/>
    <Parameter Name="param_ledger_name" DataType="System.String"/>
    <Parameter Name="param_analyis_type_start" DataType="System.String"/>
    <Parameter Name="param_analyis_type_end" DataType="System.String"/>
    <Parameter Name="param_currency_name" DataType="System.String"/>
    <Parameter Name="param_real_year" DataType="System.Int32"/>
    <Parameter Name="param_period_breakdown" DataType="System.Boolean"/>
    <Total Name="GroupDebit" Expression="Debits.Value" Evaluator="Transaction" PrintOn="GroupFooter2"/>
    <Total Name="GroupCredit" Expression="Credits.Value" Evaluator="Transaction" PrintOn="GroupFooter2"/>
    <Total Name="OuterGroupDebit" Expression="Debits.Value" Evaluator="Transaction" PrintOn="GroupFooter1"/>
    <Total Name="OuterGroupCredit" Expression="Credits.Value" Evaluator="Transaction" PrintOn="GroupFooter1"/>
  </Dictionary>
  <ReportPage Name="Page1">
    <ReportTitleBand Name="ReportTitle1" Width="718.2" Height="85.05">
      <TextObject Name="Text1" Left="245.7" Width="207.9" Height="18.9" Text="Account Detail" HorzAlign="Center" Font="Arial, 14pt, style=Bold"/>
      <TextObject Name="Text9" Left="453.6" Width="122.85" Height="18.9" Text="Printed :" HorzAlign="Right"/>
      <TextObject Name="Text8" Left="576.45" Width="141.75" Height="18.9" Text="[OmDate([Date])]"/>
      <TextObject Name="Text20" Width="75.6" Height="18.9" Text="Ledger :" HorzAlign="Right"/>
      <TextObject Name="Text10" Left="75.6" Width="170.1" Height="18.9" Text="[param_ledger_number_i] [param_ledger_name]"/>
      <TextObject Name="Text11" Left="576.45" Top="18.9" Width="141.75" Height="18.9" Text="[param_cost_centre_list_title]"/>
      <TextObject Name="Text12" Left="453.6" Top="18.9" Width="122.85" Height="18.9" Text="Cost Centres :" HorzAlign="Right"/>
      <TextObject Name="Text14" Left="453.6" Top="37.8" Width="122.85" Height="18.9" Text="Accounts :" HorzAlign="Right"/>
      <TextObject Name="Text13" Left="576.45" Top="37.8" Width="141.75" Height="18.9" Text="[param_account_list_title]"/>
      <TextObject Name="Text21" Top="18.9" Width="75.6" Height="18.9" Text="Currency :" HorzAlign="Right"/>
      <TextObject Name="Text18" Left="75.6" Top="18.9" Width="170.1" Height="18.9" Text="[param_currency_name]"/>
      <TextObject Name="Text22" Top="37.8" Width="75.6" Height="18.9" Text="[IIf([param_period],&quot;Period :&quot;,&quot;Date :&quot;)]" HorzAlign="Right"/>
      <TextObject Name="Text19" Left="75.6" Top="37.8" Width="198.45" Height="18.9" Text="[IIf([param_period],ToString([param_start_period_i])+&quot; - &quot;+ToString([param_end_period_i]), OmDate([param_start_date]) + &quot; - &quot; + OmDate([param_end_date]))]" AutoShrink="FontSize" AutoShrinkMinSize="5" WordWrap="false"/>
      <TextObject Name="Text42" Left="245.7" Top="18.9" Width="207.9" Height="18.9" Text="[param_ledger_name]" HorzAlign="Center"/>
      <TextObject Name="Text43" Left="274.05" Top="37.8" Width="179.55" Height="18.9"/>
      <LineObject Name="Line1" Left="718.2" Top="75.6" Width="-718.2"/>
      <TextObject Name="Text50" Left="453.6" Top="56.7" Width="122.85" Height="18.9" Text="Ordered By :" HorzAlign="Right"/>
      <TextObject Name="Text51" Left="576.45" Top="56.7" Width="141.75" Height="18.9" Text="[param_sortby]"/>
      <TextObject Name="Text56" Left="75.6" Top="56.7" Width="198.45" Height="18.9" Text="[OmDate([param_start_date])+&quot; - &quot;+OmDate([param_end_date])]" AutoShrink="FontSize" AutoShrinkMinSize="5" WordWrap="false"/>
    </ReportTitleBand>
    <PageHeaderBand Name="PageHeader1" Top="88.38" Width="718.2"/>
    <GroupHeaderBand Name="GroupHeader1" Top="91.72" Width="718.2" Condition="Switch(&quot;Account&quot;==[param_sortby],[a_transaction.a_account_code_c], &quot;Cost Centre&quot;==[param_sortby], [a_transaction.a_cost_centre_code_c])" SortOrder="None">
      <GroupHeaderBand Name="GroupHeader2" Top="95.05" Width="718.2" Height="47.25" KeepChild="true" KeepWithData="true" Condition="Switch(&quot;Account&quot;==[param_sortby],[a_transaction.a_cost_centre_code_c], &quot;Cost Centre&quot;==[param_sortby], [a_transaction.a_account_code_c], &quot;Reference&quot;==[param_sortby], [a_transaction.a_reference_c], &quot;Analysis Type&quot;==[param_sortby],[a_transaction.a_analysis_type_code_c])" SortOrder="None" KeepTogether="true">
        <TextObject Name="Text15" Top="9.45" Width="718.2" Height="18.9" CanShrink="true" CanBreak="false" Text="[GroupHeader ()]" AutoShrink="FontSize" AutoShrinkMinSize="7" WordWrap="false" Font="Arial, 10pt, style=Bold"/>
        <TextObject Name="Text3" Left="113.4" Top="28.35" Width="85.05" Height="18.9" Text="Date" HorzAlign="Right" Font="Arial, 10pt, style=Bold, Italic"/>
        <TextObject Name="Text5" Left="198.45" Top="28.35" Width="85.05" Height="18.9" Text="Debits" HorzAlign="Right" Font="Arial, 10pt, style=Bold, Italic"/>
        <TextObject Name="Text7" Left="368.55" Top="28.35" Width="103.95" Height="18.9" Text="Narrative" Font="Arial, 10pt, style=Bold, Italic"/>
        <TextObject Name="Text23" Left="283.5" Top="28.35" Width="85.05" Height="18.9" Text="Credits" HorzAlign="Right" Font="Arial, 10pt, style=Bold, Italic"/>
        <TextObject Name="Text40" Top="28.35" Width="9.45" Height="18.9"/>
        <TextObject Name="Text41" Left="9.45" Top="28.35" Width="103.95" Height="18.9" Text="Ref" HorzAlign="Right" Font="Arial, 10pt, style=Bold, Italic"/>
        <DataBand Name="Transaction" Top="145.63" Width="718.2" Height="18.9" CanGrow="true" KeepChild="true" DataSource="a_transaction" KeepDetail="true">
          <TextObject Name="Text30" Width="9.45" Height="18.9" Text="[a_transaction.a_cost_centre_code_c]" TextFill.Color="White"/>
          <TextObject Name="TransRef" Left="9.45" Width="103.95" Height="18.9" Text="[a_transaction.a_reference_c]" AutoShrink="FontSize" AutoShrinkMinSize="7" HorzAlign="Right" WordWrap="false" Font="Arial, 9pt" Clip="false"/>
          <TextObject Name="TransDate" Left="113.4" Width="85.05" Height="18.9" Text="[OmDate([a_transaction.a_transaction_date_d])]" HorzAlign="Right" WordWrap="false" Font="Arial, 9pt"/>
          <TextObject Name="Debits" Left="198.45" Width="85.05" Height="18.9" Text="[IIf ([a_transaction.a_debit_credit_indicator_l]==true,Switch ([param_currency]==&quot;Base&quot;,[a_transaction.a_amount_in_base_currency_n],[param_currency]==&quot;Transaction&quot;,[a_transaction.a_transaction_amount_n],[param_currency]==&quot;International&quot;,[a_transaction.a_amount_in_intl_currency_n]),0)]" HorzAlign="Right" WordWrap="false" Font="Arial, 10pt, style=Bold" Trimming="EllipsisCharacter">
            <Formats>
              <NumberFormat UseLocale="false" NegativePattern="1"/>
              <GeneralFormat/>
            </Formats>
            <Highlight>
              <Condition Expression="Value == 0" TextFill.Color="White"/>
            </Highlight>
          </TextObject>
          <TextObject Name="Credits" Left="283.5" Width="85.05" Height="18.9" Text="[IIf ([a_transaction.a_debit_credit_indicator_l]==false,Switch ([param_currency]==&quot;Base&quot;,[a_transaction.a_amount_in_base_currency_n],[param_currency]==&quot;Transaction&quot;,[a_transaction.a_transaction_amount_n],[param_currency]==&quot;International&quot;,[a_transaction.a_amount_in_intl_currency_n]),0)]" HorzAlign="Right" WordWrap="false" Font="Arial, 10pt, style=Bold" Trimming="EllipsisCharacter">
            <Formats>
              <NumberFormat UseLocale="false" NegativePattern="1"/>
              <GeneralFormat/>
            </Formats>
            <Highlight>
              <Condition Expression="Value == 0" TextFill.Color="White"/>
            </Highlight>
          </TextObject>
          <TextObject Name="Narrative" Left="368.55" Width="349.65" Height="18.9" CanGrow="true" GrowToBottom="true" Text="[a_transaction.a_narrative_c]" Font="Arial, 9pt" Clip="false"/>
          <DataBand Name="Attributes" Top="167.87" Width="718.2" Height="9.45" CanGrow="true" CanShrink="true">
            <TextObject Name="AttrDescr" Left="368.55" Width="349.65" Height="9.45" CanGrow="true" CanShrink="true" CanBreak="false" Text="[a_transaction.a_trans_anal_attrib.a_analysis_type.a_analysis_type_description_c][a_transaction.a_trans_anal_attrib.a_analysis_attribute_value_c]" Font="Arial, 9pt" TextFill.Color="Green"/>
          </DataBand>
        </DataBand>
        <GroupFooterBand Name="GroupFooter2" Top="180.65" Width="718.2" Height="37.8" KeepChild="true" KeepWithData="true">
          <TextObject Name="GroupDebitTotal" Left="198.45" Width="85.05" Height="18.9" Text="[GroupDebit]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="1" HorzAlign="Right" Font="Arial, 9pt, style=Bold"/>
          <TextObject Name="GroupCreditTotal" Left="283.5" Width="85.05" Height="18.9" Text="[GroupCredit]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="1" HorzAlign="Right" Font="Arial, 9pt, style=Bold"/>
          <TextObject Name="Text24" Left="103.95" Width="94.5" Height="18.9" Text="Sub-Total :" HorzAlign="Right" Font="Arial, 9pt, style=Italic"/>
          <TextObject Name="Text4" Left="198.45" Top="18.9" Width="85.05" Height="18.9" Text="[ToDecimal([GroupDebit]-[GroupCredit])]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="1" HorzAlign="Right" Font="Arial, 9pt, style=Bold">
            <Highlight>
              <Condition Expression="Value &lt;= 0" TextFill.Color="White"/>
            </Highlight>
          </TextObject>
          <TextObject Name="Text25" Left="283.5" Top="18.9" Width="85.05" Height="18.9" Text="[ToDecimal([GroupCredit]-[GroupDebit])]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="1" HorzAlign="Right" Font="Arial, 9pt, style=Bold">
            <Highlight>
              <Condition Expression="Value &lt; 0" TextFill.Color="White"/>
            </Highlight>
          </TextObject>
          <TextObject Name="Text26" Left="103.95" Top="18.9" Width="94.5" Height="18.9" Text="Balance :" HorzAlign="Right" Font="Arial, 9pt, style=Italic"/>
          <TextObject Name="Text32" Width="9.45" Height="18.9" TextFill.Color="White"/>
          <TextObject Name="Text33" Left="9.45" Width="9.45" Height="18.9" TextFill.Color="White"/>
          <TextObject Name="Text34" Top="18.9" Width="9.45" Height="18.9" TextFill.Color="White"/>
          <TextObject Name="Text35" Left="9.45" Top="18.9" Width="9.45" Height="18.9" TextFill.Color="White"/>
          <TextObject Name="Text54" Left="453.6" Top="18.9" Width="103.95" Height="18.9" Text="[a_transaction.balances.OpeningBalance]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="1" HorzAlign="Right" Font="Arial, 9pt, style=Bold"/>
          <TextObject Name="Text55" Left="557.55" Top="18.9" Width="103.95" Height="18.9" Text="[a_transaction.balances.ClosingBalance]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="1" HorzAlign="Right" Font="Arial, 9pt, style=Bold"/>
          <TextObject Name="Text52" Left="453.6" Width="103.95" Height="18.9" Text="Opening Balance" HorzAlign="Right" Font="Arial, 8pt, style=Bold"/>
          <TextObject Name="Text53" Left="557.55" Width="103.95" Height="18.9" Text="Closing Balance" HorzAlign="Right" Font="Arial, 8pt, style=Bold"/>
          <LineObject Name="Line2" Left="661.5" Width="-567" Border.Width="1.5"/>
        </GroupFooterBand>
      </GroupHeaderBand>
      <GroupFooterBand Name="GroupFooter1" Top="221.78" Width="718.2" Height="47.25" KeepChild="true" KeepWithData="true">
        <TextObject Name="Text27" Left="198.45" Top="18.9" Width="85.05" Height="18.9" Text="[ToDecimal([OuterGroupDebit]-[OuterGroupCredit])]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="1" HorzAlign="Right" Font="Arial, 9pt, style=Bold" TextFill.Color="Blue">
          <Highlight>
            <Condition Expression="Value &lt;= 0" TextFill.Color="White"/>
          </Highlight>
        </TextObject>
        <TextObject Name="Text28" Left="283.5" Top="18.9" Width="85.05" Height="18.9" Text="[ToDecimal([OuterGroupCredit]-[OuterGroupDebit])]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="1" HorzAlign="Right" Font="Arial, 9pt, style=Bold" TextFill.Color="Blue">
          <Highlight>
            <Condition Expression="Value &lt; 0" TextFill.Color="White"/>
          </Highlight>
        </TextObject>
        <TextObject Name="f36" Width="9.45" Height="18.9"/>
        <TextObject Name="f37" Left="9.45" Width="9.45" Height="18.9"/>
        <TextObject Name="Text16" Left="103.95" Width="94.5" Height="18.9" CanShrink="true" CanBreak="false" Text="[Switch(&quot;Account&quot;==[param_sortby],[a_transaction.a_account_code_c]+&quot; Total :&quot;,&quot;Cost Centre&quot;==[param_sortby], [a_transaction.a_cost_centre_code_c]+&quot; Total :&quot;,&quot;Reference&quot;==[param_sortby], &quot;Total :&quot;, &quot;Analysis Type&quot;==[param_sortby], &quot;Total :&quot;)]" AutoShrink="FontSize" AutoShrinkMinSize="7" HorzAlign="Right" WordWrap="false" Font="Arial, 9pt, style=Italic" TextFill.Color="Blue"/>
        <TextObject Name="OuterGroupDebitTotal" Left="198.45" Width="85.05" Height="18.9" Text="[OuterGroupDebit]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="1" HorzAlign="Right" Font="Arial, 9pt, style=Bold" TextFill.Color="Blue"/>
        <TextObject Name="OuterGroupCreditTotal" Left="283.5" Width="85.05" Height="18.9" Text="[OuterGroupCredit]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="1" HorzAlign="Right" Font="Arial, 9pt, style=Bold" TextFill.Color="Blue"/>
        <LineObject Name="Line3" Left="378" Top="36.8" Width="-283.5" Border.Color="Blue" Border.Width="1.5"/>
        <TextObject Name="f38" Top="18.9" Width="9.45" Height="18.9"/>
        <TextObject Name="f39" Left="9.45" Top="18.9" Width="9.45" Height="18.9"/>
        <TextObject Name="f29" Left="18.9" Top="18.9" Width="85.05" Height="18.9"/>
      </GroupFooterBand>
    </GroupHeaderBand>
    <PageFooterBand Name="PageFooter1" Top="272.37" Width="718.2" Height="18.9">
      <TextObject Name="Text44" Width="9.45" Height="18.9"/>
      <TextObject Name="Text45" Left="9.45" Width="9.45" Height="18.9"/>
      <TextObject Name="Text46" Left="18.9" Width="9.45" Height="18.9"/>
      <TextObject Name="Text47" Left="28.35" Width="9.45" Height="18.9"/>
      <TextObject Name="Text48" Left="37.8" Width="9.45" Height="18.9"/>
      <TextObject Name="Text49" Left="47.25" Width="9.45" Height="18.9"/>
      <TextObject Name="Text17" Left="548.1" Width="170.1" Height="18.9" Text="[PageNofM]" HorzAlign="Right" Font="Arial, 9pt, style=Bold"/>
    </PageFooterBand>
  </ReportPage>
</Report>
');
INSERT INTO s_report_template (s_template_id_i,s_report_type_c,s_report_variant_c,s_author_c,s_default_l,s_readonly_l,s_xml_text_c)
VALUES(1,'HOSA','OpenPetra default template','System',True,True,
'﻿<?xml version="1.0" encoding="utf-8"?>
<Report ScriptLanguage="CSharp" DoublePass="true" ReportInfo.Created="11/05/2013 15:46:27" ReportInfo.Modified="03/25/2014 15:56:40" ReportInfo.CreatorVersion="2013.4.4.0">
  <ScriptText>using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using System.Globalization;
using FastReport;
using FastReport.Data;
using FastReport.Dialog;
using FastReport.Barcode;
using FastReport.Table;
using FastReport.Utils;

namespace FastReport
{
  public class ReportScript
  {
    String OmDate(DateTime fld)
    {
      return fld.ToString(&quot;dd-MMM-yyyy&quot;);
    }
  }
}
</ScriptText>
  <Dictionary>
    <TableDataSource Name="a_costCentre" ReferenceName="a_costCentre" DataType="System.Int32" Enabled="true">
      <Column Name="a_ledger_number_i" DataType="System.Int32"/>
      <Column Name="a_cost_centre_code_c" DataType="System.String"/>
      <Column Name="a_cost_centre_to_report_to_c" DataType="System.String"/>
      <Column Name="a_cost_centre_name_c" DataType="System.String"/>
      <Column Name="a_posting_cost_centre_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_cost_centre_active_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_project_status_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_project_constraint_date_d" DataType="System.DateTime"/>
      <Column Name="a_project_constraint_amount_n" DataType="System.Decimal"/>
      <Column Name="a_system_cost_centre_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_cost_centre_type_c" DataType="System.String"/>
      <Column Name="a_key_focus_area_c" DataType="System.String"/>
      <Column Name="s_date_created_d" DataType="System.DateTime"/>
      <Column Name="s_created_by_c" DataType="System.String"/>
      <Column Name="s_date_modified_d" DataType="System.DateTime"/>
      <Column Name="s_modified_by_c" DataType="System.String"/>
      <Column Name="s_modification_id_t" DataType="System.DateTime"/>
    </TableDataSource>
    <TableDataSource Name="Gifts" ReferenceName="Gifts" DataType="System.Int32" Enabled="true">
      <Column Name="costcentre" DataType="System.String"/>
      <Column Name="accountcode" DataType="System.String"/>
      <Column Name="giftbaseamount" DataType="System.Decimal"/>
      <Column Name="gifttransactionamount" DataType="System.Decimal"/>
      <Column Name="recipientkey" DataType="System.Int64"/>
      <Column Name="recipientshortname" DataType="System.String"/>
      <Column Name="narrative" DataType="System.String"/>
      <Column Name="GiftIntlAmount" DataType="System.Decimal"/>
      <Column Name="Reference" DataType="System.String"/>
    </TableDataSource>
    <TableDataSource Name="a_account" ReferenceName="a_account" DataType="System.Int32" Enabled="true">
      <Column Name="a_ledger_number_i" DataType="System.Int32"/>
      <Column Name="a_account_code_c" DataType="System.String"/>
      <Column Name="a_account_type_c" DataType="System.String"/>
      <Column Name="a_account_code_long_desc_c" DataType="System.String"/>
      <Column Name="a_account_code_short_desc_c" DataType="System.String"/>
      <Column Name="a_eng_account_code_short_desc_c" DataType="System.String"/>
      <Column Name="a_eng_account_code_long_desc_c" DataType="System.String"/>
      <Column Name="a_debit_credit_indicator_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_account_active_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_analysis_attribute_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_standard_account_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_consolidation_account_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_intercompany_account_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_budget_type_code_c" DataType="System.String"/>
      <Column Name="a_posting_status_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_system_account_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_budget_control_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_valid_cc_combo_c" DataType="System.String"/>
      <Column Name="a_foreign_currency_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_foreign_currency_code_c" DataType="System.String"/>
      <Column Name="p_banking_details_key_i" DataType="System.Int32"/>
      <Column Name="s_date_created_d" DataType="System.DateTime"/>
      <Column Name="s_created_by_c" DataType="System.String"/>
      <Column Name="s_date_modified_d" DataType="System.DateTime"/>
      <Column Name="s_modified_by_c" DataType="System.String"/>
      <Column Name="s_modification_id_t" DataType="System.DateTime"/>
    </TableDataSource>
    <TableDataSource Name="a_transaction" ReferenceName="a_transaction" DataType="System.Int32" Enabled="true">
      <Column Name="a_ledger_number_i" DataType="System.Int32"/>
      <Column Name="a_batch_number_i" DataType="System.Int32"/>
      <Column Name="a_journal_number_i" DataType="System.Int32"/>
      <Column Name="a_transaction_number_i" DataType="System.Int32"/>
      <Column Name="a_account_code_c" DataType="System.String"/>
      <Column Name="a_primary_account_code_c" DataType="System.String"/>
      <Column Name="a_cost_centre_code_c" DataType="System.String"/>
      <Column Name="a_primary_cost_centre_code_c" DataType="System.String"/>
      <Column Name="a_transaction_date_d" DataType="System.DateTime"/>
      <Column Name="a_transaction_amount_n" DataType="System.Decimal"/>
      <Column Name="a_amount_in_base_currency_n" DataType="System.Decimal"/>
      <Column Name="a_analysis_indicator_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_reconciled_status_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_narrative_c" DataType="System.String"/>
      <Column Name="a_debit_credit_indicator_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_transaction_status_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_header_number_i" DataType="System.Int32"/>
      <Column Name="a_detail_number_i" DataType="System.Int32"/>
      <Column Name="a_sub_type_c" DataType="System.String"/>
      <Column Name="a_to_ilt_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_source_flag_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_reference_c" DataType="System.String"/>
      <Column Name="a_source_reference_c" DataType="System.String"/>
      <Column Name="a_system_generated_l" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="a_amount_in_intl_currency_n" DataType="System.Decimal"/>
      <Column Name="a_ich_number_i" DataType="System.Int32"/>
      <Column Name="a_key_ministry_key_n" DataType="System.Int64"/>
      <Column Name="s_date_created_d" DataType="System.DateTime"/>
      <Column Name="s_created_by_c" DataType="System.String"/>
      <Column Name="s_date_modified_d" DataType="System.DateTime"/>
      <Column Name="s_modified_by_c" DataType="System.String"/>
      <Column Name="s_modification_id_t" DataType="System.DateTime"/>
      <Column Name="a_analysis_type_code_c" DataType="System.String"/>
      <Column Name="a_analysis_type_description_c" DataType="System.String"/>
      <Column Name="a_analysis_attribute_value_c" DataType="System.String"/>
    </TableDataSource>
    <Relation Name="a_account_Gifts" ParentDataSource="a_account" ChildDataSource="Gifts" ParentColumns="a_account_code_c" ChildColumns="accountcode" Enabled="true"/>
    <Relation Name="a_account_a_transaction" ParentDataSource="a_account" ChildDataSource="a_transaction" ParentColumns="a_account_code_c" ChildColumns="a_account_code_c" Enabled="true"/>
    <Parameter Name="param_diff_period_i" DataType="System.Int32"/>
    <Parameter Name="param_start_period_i" DataType="System.Int32"/>
    <Parameter Name="param_end_period_i" DataType="System.Int32"/>
    <Parameter Name="param_account_hierarchy_c" DataType="System.String"/>
    <Parameter Name="param_currency" DataType="System.String"/>
    <Parameter Name="param_period" DataType="System.Boolean"/>
    <Parameter Name="param_date_checked" DataType="System.Boolean"/>
    <Parameter Name="param_year_i" DataType="System.Int32"/>
    <Parameter Name="param_start_date" DataType="System.DateTime"/>
    <Parameter Name="param_end_date" DataType="System.DateTime"/>
    <Parameter Name="param_filter_cost_centres" DataType="System.String"/>
    <Parameter Name="param_ExcludeInactiveCostCentres" DataType="System.Boolean"/>
    <Parameter Name="param_cost_centre_codes" DataType="System.String"/>
    <Parameter Name="param_ledger_number_i" DataType="System.Int32"/>
    <Parameter Name="param_daterange" DataType="System.String"/>
    <Parameter Name="param_rgrAccounts" DataType="System.String"/>
    <Parameter Name="param_rgrCostCentres" DataType="System.String"/>
    <Parameter Name="param_ich_number" DataType="System.Int32"/>
    <Parameter Name="param_quarter" DataType="System.Boolean"/>
    <Parameter Name="param_ledger_name" DataType="System.String"/>
    <Parameter Name="param_currency_formatter" DataType="System.String"/>
    <Parameter Name="param_date_title" DataType="System.String"/>
    <Parameter Name="param_currency_name" DataType="System.String"/>
    <Parameter Name="param_period_breakdown" DataType="System.Boolean"/>
    <Parameter Name="param_real_year" DataType="System.Int32"/>
    <Total Name="GiftDebitsTotal" Expression="GiftDebits.Value" Evaluator="Data1" PrintOn="GiftsFooter" ResetOnReprint="true"/>
    <Total Name="GiftCreditsTotal" Expression="GiftCredits.Value" Evaluator="Data1" PrintOn="GiftsFooter" ResetOnReprint="true"/>
    <Total Name="TransDebitsTotal" Expression="TransDebits.Value" Evaluator="Data3" PrintOn="TransFooter" ResetOnReprint="true"/>
    <Total Name="TransCreditsTotal" Expression="TransCredits.Value" Evaluator="Data3" PrintOn="TransFooter" ResetOnReprint="true"/>
    <Total Name="AllGiftDebits" Expression="GiftDebits.Value" Evaluator="Data1" PrintOn="PageFooter" ResetOnReprint="true"/>
    <Total Name="AllGiftCredits" Expression="GiftCredits.Value" Evaluator="Data1" PrintOn="PageFooter" ResetOnReprint="true"/>
    <Total Name="AllTransDebits" Expression="TransDebits.Value" Evaluator="Data3" PrintOn="PageFooter" ResetOnReprint="true"/>
    <Total Name="AllTransCredits" Expression="TransCredits.Value" Evaluator="Data3" PrintOn="PageFooter" ResetOnReprint="true"/>
  </Dictionary>
  <ReportPage Name="Page1">
    <GroupHeaderBand Name="PageHeader" Width="718.2" Height="66.15" StartNewPage="true" Condition="[a_costCentre.a_cost_centre_code_c]" SortOrder="None">
      <TextObject Name="Text22" Top="37.8" Width="75.6" Height="18.9" Text="[IIf([param_period],&quot;Period :&quot;,&quot;Date :&quot;)]" HorzAlign="Right"/>
      <TextObject Name="Text19" Left="75.6" Top="37.8" Width="245.7" Height="18.9" Text="[param_date_title]" AutoShrink="FontSize" AutoShrinkMinSize="5" WordWrap="false"/>
      <TextObject Name="Text11" Left="576.45" Top="18.9" Width="141.75" Height="18.9" Text="[a_costCentre.a_cost_centre_code_c]"/>
      <TextObject Name="Text12" Left="453.6" Top="18.9" Width="122.85" Height="18.9" Text="Cost Centre :" HorzAlign="Right"/>
      <TextObject Name="Text21" Top="18.9" Width="75.6" Height="18.9" Text="Currency :" HorzAlign="Right"/>
      <TextObject Name="Text18" Left="75.6" Top="18.9" Width="151.2" Height="18.9" Text="[param_currency]"/>
      <TextObject Name="Text10" Left="75.6" Width="151.2" Height="18.9" Text="[param_ledger_number_i] [param_ledger_name]"/>
      <TextObject Name="Text1" Left="226.8" Width="226.8" Height="18.9" Text="HOSA" HorzAlign="Center" Font="Arial, 14pt, style=Bold"/>
      <TextObject Name="Text9" Left="453.6" Width="122.85" Height="18.9" Text="Printed :" HorzAlign="Right"/>
      <TextObject Name="Text8" Left="576.45" Width="141.75" Height="18.9" Text="[OmDate([Date])]"/>
      <TextObject Name="Text20" Width="75.6" Height="18.9" Text="Ledger :" HorzAlign="Right"/>
      <LineObject Name="Line1" Top="56.7" Width="718.2" Border.Width="2"/>
      <TextObject Name="Text88" Left="226.8" Top="18.9" Width="226.8" Height="18.9" Text="[param_ledger_name] " HorzAlign="Center"/>
      <DataBand Name="GiftsReport" Top="69.48" Width="718.2" DataSource="a_costCentre">
        <GroupHeaderBand Name="GiftsHeader" Top="72.82" Width="718.2" Height="18.9" Condition="[Gifts.accountcode]" SortOrder="None">
          <TextObject Name="Text64" Width="75.6" Height="18.9" Text="[Gifts.costcentre]-[Gifts.accountcode]" AutoShrink="FontSize" AutoShrinkMinSize="7" Font="Arial, 10pt, style=Bold, Italic"/>
          <TextObject Name="Text65" Left="75.6" Width="633.15" Height="18.9" Text="[Gifts.a_account.a_account_code_long_desc_c], [a_costCentre.a_cost_centre_name_c]" WordWrap="false" Font="Arial, 10pt, style=Bold, Italic"/>
          <DataBand Name="Data1" Top="95.05" Width="718.2" Height="18.9" DataSource="Gifts" Filter="[Gifts.costcentre]==[a_costCentre.a_cost_centre_code_c]">
            <TextObject Name="Text66" Left="113.4" Width="113.4" Height="18.9" CanGrow="true" Text="[Gifts.Reference]"/>
            <TextObject Name="GiftCredits" Left="321.3" Width="94.5" Height="18.9" Text="[IIf([Gifts.giftbaseamount] &gt; 0,[Gifts.giftbaseamount], 0)]" HorzAlign="Right" WordWrap="false" Font="Arial, 10pt, style=Bold" Trimming="EllipsisCharacter">
              <Formats>
                <NumberFormat UseLocale="false" NegativePattern="1"/>
                <GeneralFormat/>
              </Formats>
              <Highlight>
                <Condition Expression="Value == 0" TextFill.Color="White"/>
              </Highlight>
            </TextObject>
            <TextObject Name="Text68" Left="415.8" Width="302.4" Height="18.9" CanGrow="true" Text="[Gifts.narrative]"/>
            <TextObject Name="GiftDebits" Left="226.8" Width="94.5" Height="18.9" Text="[IIf ([Gifts.giftbaseamount] &lt; 0, 0 - [Gifts.giftbaseamount], 0)]" HorzAlign="Right" WordWrap="false" Font="Arial, 10pt, style=Bold" Trimming="EllipsisCharacter">
              <Formats>
                <NumberFormat UseLocale="false" NegativePattern="1"/>
                <GeneralFormat/>
              </Formats>
              <Highlight>
                <Condition Expression="Value == 0" TextFill.Color="White"/>
              </Highlight>
            </TextObject>
            <TextObject Name="Text3" Width="113.4" Height="18.9" Text=" "/>
          </DataBand>
          <GroupFooterBand Name="GiftsFooter" Top="117.28" Width="718.2" Height="28.35">
            <TextObject Name="Text70" Left="321.3" Width="94.5" Height="18.9" Text="[GiftCreditsTotal]" HorzAlign="Right" Font="Arial, 10pt, style=Bold" TextFill.Color="Blue" Trimming="EllipsisCharacter">
              <Formats>
                <NumberFormat UseLocale="false" NegativePattern="1"/>
                <GeneralFormat/>
              </Formats>
              <Highlight>
                <Condition Expression="Value == 0" TextFill.Color="White"/>
              </Highlight>
            </TextObject>
            <TextObject Name="Text71" Left="226.8" Width="94.5" Height="18.9" Text="[GiftDebitsTotal]" HorzAlign="Right" Font="Arial, 10pt, style=Bold" TextFill.Color="Blue" Trimming="EllipsisCharacter">
              <Formats>
                <NumberFormat UseLocale="false" NegativePattern="1"/>
                <GeneralFormat/>
              </Formats>
              <Highlight>
                <Condition Expression="Value == 0" TextFill.Color="White"/>
              </Highlight>
            </TextObject>
            <LineObject Name="Line2" Left="226.8" Width="189" Border.Color="Blue"/>
            <TextObject Name="Text83" Left="113.4" Width="113.4" Height="18.9" Text="Sub Total :" HorzAlign="Right"/>
            <TextObject Name="Text84" Width="113.4" Height="18.9" Text=" "/>
          </GroupFooterBand>
        </GroupHeaderBand>
        <GroupHeaderBand Name="TransHeader" Top="148.97" Width="718.2" Height="18.9" Condition="[a_transaction.a_account_code_c]">
          <TextObject Name="Text72" Left="75.6" Width="633.15" Height="18.9" Text="[a_transaction.a_account.a_account_code_long_desc_c], [a_costCentre.a_cost_centre_name_c]" WordWrap="false" Font="Arial, 10pt, style=Bold, Italic"/>
          <TextObject Name="Text73" Width="75.6" Height="18.9" Text="[a_costCentre.a_cost_centre_code_c]-[a_transaction.a_account.a_account_code_c]" AutoShrink="FontSize" AutoShrinkMinSize="7" Font="Arial, 10pt, style=Bold, Italic"/>
          <DataBand Name="Data3" Top="171.2" Width="718.2" Height="18.9" DataSource="a_transaction" Filter="[a_costCentre.a_cost_centre_code_c]==[a_transaction.a_cost_centre_code_c]">
            <TextObject Name="TransDate" Width="113.4" Height="18.9" Text="[OmDate([a_transaction.a_transaction_date_d])]"/>
            <TextObject Name="Text75" Left="415.8" Width="302.4" Height="18.9" CanGrow="true" Text="[a_transaction.a_narrative_c]"/>
            <TextObject Name="Text76" Left="113.4" Width="113.4" Height="18.9" CanGrow="true" Text="[a_transaction.a_reference_c]"/>
            <TextObject Name="TransDebits" Left="226.8" Width="94.5" Height="18.9" Text="[IIf([a_transaction.a_debit_credit_indicator_l]==true,[a_transaction.a_amount_in_base_currency_n],0)]" HorzAlign="Right" Font="Arial, 10pt, style=Bold" Trimming="EllipsisCharacter">
              <Formats>
                <NumberFormat UseLocale="false" NegativePattern="1"/>
                <GeneralFormat/>
              </Formats>
              <Highlight>
                <Condition Expression="Value == 0" TextFill.Color="White"/>
              </Highlight>
            </TextObject>
            <TextObject Name="TransCredits" Left="321.3" Width="94.5" Height="18.9" Text="[IIf([a_transaction.a_debit_credit_indicator_l]==false,[a_transaction.a_amount_in_base_currency_n],0)]" HorzAlign="Right" Font="Arial, 10pt, style=Bold" Trimming="EllipsisCharacter">
              <Formats>
                <NumberFormat UseLocale="false" NegativePattern="1"/>
                <GeneralFormat/>
              </Formats>
              <Highlight>
                <Condition Expression="Value == 0" TextFill.Color="White"/>
              </Highlight>
            </TextObject>
          </DataBand>
          <GroupFooterBand Name="TransFooter" Top="193.43" Width="718.2" Height="28.35">
            <TextObject Name="Text79" Left="226.8" Width="94.5" Height="18.9" Text="[TransDebitsTotal]" Format="Currency" Format.UseLocale="true" HorzAlign="Right" Font="Arial, 10pt, style=Bold" TextFill.Color="Blue" Trimming="EllipsisCharacter">
              <Highlight>
                <Condition Expression="Value == 0" TextFill.Color="White"/>
              </Highlight>
            </TextObject>
            <TextObject Name="Text80" Left="321.3" Width="94.5" Height="18.9" Text="[TransCreditsTotal]" Format="Currency" Format.UseLocale="true" HorzAlign="Right" Font="Arial, 10pt, style=Bold" TextFill.Color="Blue" Trimming="EllipsisCharacter">
              <Highlight>
                <Condition Expression="Value == 0" TextFill.Color="White"/>
              </Highlight>
            </TextObject>
            <LineObject Name="Line3" Left="226.8" Width="189"/>
            <TextObject Name="Text82" Left="113.4" Width="113.4" Height="18.9" Text="Sub Total :" HorzAlign="Right"/>
          </GroupFooterBand>
        </GroupHeaderBand>
      </DataBand>
      <GroupFooterBand Name="PageFooter" Top="225.12" Width="718.2" Height="47.25">
        <TextObject Name="AllDebits" Left="226.8" Top="9.45" Width="94.5" Height="18.9" Text="[[AllGiftDebits]+[AllTransDebits]]" HorzAlign="Right" Font="Arial, 10pt, style=Bold" TextFill.Color="Blue" Trimming="EllipsisCharacter">
          <Formats>
            <NumberFormat UseLocale="false" NegativePattern="1"/>
            <GeneralFormat/>
          </Formats>
        </TextObject>
        <TextObject Name="AllCredits" Left="321.3" Top="9.45" Width="94.5" Height="18.9" Text="[[AllGiftCredits]+[AllTransCredits]]" HorzAlign="Right" Font="Arial, 10pt, style=Bold" TextFill.Color="Blue" Trimming="EllipsisCharacter">
          <Formats>
            <NumberFormat UseLocale="false" NegativePattern="1"/>
            <GeneralFormat/>
          </Formats>
        </TextObject>
        <LineObject Name="Line4" Width="718.2" Border.Width="2"/>
        <TextObject Name="OutstandingDebit" Left="226.8" Top="28.35" Width="94.5" Height="18.9" Text="[[AllGiftDebits]+[AllTransDebits]-[AllGiftCredits]-[AllTransCredits]]" HorzAlign="Right" Font="Arial, 10pt, style=Bold" TextFill.Color="Red" Trimming="EllipsisCharacter">
          <Formats>
            <NumberFormat UseLocale="false" NegativePattern="1"/>
            <GeneralFormat/>
          </Formats>
          <Highlight>
            <Condition Expression="Value &lt; 0.01" TextFill.Color="White"/>
          </Highlight>
        </TextObject>
        <TextObject Name="OutstandingCredit" Left="321.3" Top="28.35" Width="94.5" Height="18.9" Text="[[AllGiftCredits]+[AllTransCredits]-[AllGiftDebits]-[AllTransDebits]]" HorzAlign="Right" Font="Arial, 10pt, style=Bold" TextFill.Color="Red" Trimming="EllipsisCharacter">
          <Formats>
            <NumberFormat UseLocale="false" NegativePattern="1"/>
            <GeneralFormat/>
            <GeneralFormat/>
          </Formats>
          <Highlight>
            <Condition Expression="Value &lt; 0.01" TextFill.Color="Blue"/>
            <Condition Expression="Value &lt; 0" TextFill.Color="White"/>
          </Highlight>
        </TextObject>
        <TextObject Name="Text2" Left="113.4" Top="9.45" Width="113.4" Height="18.9" Text="Grand Total :" HorzAlign="Right"/>
        <TextObject Name="Text81" Left="113.4" Top="28.35" Width="113.4" Height="18.9" Text="Balance :" HorzAlign="Right"/>
      </GroupFooterBand>
    </GroupHeaderBand>
  </ReportPage>
</Report>
');
INSERT INTO s_report_template (s_template_id_i,s_report_type_c,s_report_variant_c,s_author_c,s_default_l,s_readonly_l,s_xml_text_c)
VALUES(2,'Balance Sheet Standard','OpenPetra default template','System',True,True,
'﻿<?xml version="1.0" encoding="utf-8"?>
<Report ScriptLanguage="CSharp" DoublePass="true" ReportInfo.Created="11/05/2013 15:46:27" ReportInfo.Modified="03/25/2014 16:06:08" ReportInfo.CreatorVersion="2013.4.4.0">
  <ScriptText>using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using System.Globalization;
using FastReport;
using FastReport.Data;
using FastReport.Dialog;
using FastReport.Barcode;
using FastReport.Table;
using FastReport.Utils;

namespace FastReport
{
  public class ReportScript
  {
    String OmDate(DateTime fld)
    {
      return fld.ToString(&quot;dd-MMM-yyyy&quot;);
    }
    
    String variance(Decimal left, Decimal right)
    {
      if(right!= 0)
        return (Math.Floor(((decimal)left/(decimal)right)*100 - (Decimal)99.5)).ToString() + &quot;%&quot;;
      else
        return &quot; &quot;;
    }
    
    private void Data_BeforePrint(object sender, EventArgs e)
    {
      Int32 AccountLevel = ((Int32)Report.GetColumnValue(&quot;BalanceSheet.accountlevel&quot;));
      HeaderBand.Visible = false;
      TransactionBand.Visible = false;
      FooterBand.Visible = false;
      FooterLevel1.Visible = false;

      if(((Boolean)Report.GetColumnValue(&quot;BalanceSheet.haschildren&quot;)))
      {
        HeaderBand.Visible = (AccountLevel &gt; 0);
        HeaderAccountNameField.Left = Units.Millimeters * (AccountLevel * 4);
      }
      else
      {
        if(((Boolean)Report.GetColumnValue(&quot;BalanceSheet.parentfooter&quot;)))
        {
          FooterBand.Visible = (AccountLevel &gt; 1);
          FooterLevel1.Visible = (AccountLevel &lt;= 1);
          FooterAccountCodeField.Left = Units.Millimeters * (AccountLevel * 4);
          FooterAccountNameField.Left = Units.Millimeters * (15 + (AccountLevel * 4));
        }
        else
        {
          TransactionBand.Visible = true;
          AccountCodeField.Left = Units.Millimeters * (AccountLevel * 4);
          AccountNameField.Left = Units.Millimeters * (15 + (AccountLevel * 4));
        }
      }
    }
  }
}
</ScriptText>
  <Dictionary>
    <TableDataSource Name="BalanceSheet" ReferenceName="BalanceSheet" DataType="System.Int32" Enabled="true">
      <Column Name="seq" DataType="System.Int32"/>
      <Column Name="accountlevel" DataType="System.Int32"/>
      <Column Name="accountpath" DataType="System.String"/>
      <Column Name="accounttype" DataType="System.String"/>
      <Column Name="year" DataType="System.Int32"/>
      <Column Name="period" DataType="System.Int32"/>
      <Column Name="accountcode" DataType="System.String"/>
      <Column Name="accountname" DataType="System.String"/>
      <Column Name="yearstart" DataType="System.Decimal"/>
      <Column Name="actual" DataType="System.Decimal"/>
      <Column Name="actualytd" DataType="System.Decimal"/>
      <Column Name="actuallastyear" DataType="System.Decimal"/>
      <Column Name="haschildren" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="parentfooter" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="accountissummary" DataType="System.Boolean" BindableControl="CheckBox"/>
      <Column Name="accounttypeorder" DataType="System.Int32"/>
      <Column Name="costcentrecode" DataType="System.String"/>
      <Column Name="debitcredit" DataType="System.Boolean" BindableControl="CheckBox"/>
    </TableDataSource>
    <Parameter Name="param_diff_period_i" DataType="System.Int32"/>
    <Parameter Name="param_start_period_i" DataType="System.Int32"/>
    <Parameter Name="param_end_period_i" DataType="System.Int32"/>
    <Parameter Name="param_account_hierarchy_c" DataType="System.String"/>
    <Parameter Name="param_currency" DataType="System.String"/>
    <Parameter Name="param_period" DataType="System.Boolean"/>
    <Parameter Name="param_date_checked" DataType="System.Boolean"/>
    <Parameter Name="param_year_i" DataType="System.Int32"/>
    <Parameter Name="param_start_date" DataType="System.DateTime"/>
    <Parameter Name="param_end_date" DataType="System.DateTime"/>
    <Parameter Name="param_costcentreoptions" DataType="System.String"/>
    <Parameter Name="param_cost_centre_codes" DataType="System.String"/>
    <Parameter Name="param_cost_centre_list_title" DataType="System.Int32"/>
    <Parameter Name="param_cost_centre_summary" DataType="System.Boolean"/>
    <Parameter Name="param_cost_centre_breakdown" DataType="System.Boolean"/>
    <Parameter Name="param_depth" DataType="System.String"/>
    <Parameter Name="param_ytd" DataType="System.String"/>
    <Parameter Name="param_currency_format" DataType="System.String"/>
    <Parameter Name="param_ledger_number_i" DataType="System.Int32"/>
    <Parameter Name="param_quarter" DataType="System.String"/>
    <Parameter Name="param_ledger_name" DataType="System.String"/>
    <Parameter Name="param_currency_name" DataType="System.String"/>
    <Parameter Name="param_real_year" DataType="System.Int32"/>
    <Parameter Name="param_period_breakdown" DataType="System.Boolean"/>
    <Total Name="TotalLiabPlusEq" Expression="[BalanceSheet.actual]" Evaluator="TransactionBand" EvaluateCondition="[BalanceSheet.accountlevel]==1 &amp;&amp; ([BalanceSheet.accounttype]==&quot;Liability&quot;||[BalanceSheet.accounttype]==&quot;Equity&quot;)&amp;&amp;[BalanceSheet.parentfooter]"/>
    <Total Name="TotalLiabPlusEqLastYear" Expression="[BalanceSheet.actuallastyear]" Evaluator="TransactionBand" EvaluateCondition="[BalanceSheet.accountlevel]==1 &amp;&amp; ([BalanceSheet.accounttype]==&quot;Liability&quot;||[BalanceSheet.accounttype]==&quot;Equity&quot;)&amp;&amp;[BalanceSheet.parentfooter]"/>
  </Dictionary>
  <ReportPage Name="Page1" RawPaperSize="9">
    <ReportTitleBand Name="ReportTitle1" Width="718.2" Height="66.15">
      <TextObject Name="Text1" Left="245.7" Width="226.8" Height="18.9" Text="Balance Sheet" HorzAlign="Center" Font="Arial, 14pt, style=Bold"/>
      <TextObject Name="Text9" Left="510.3" Width="85.05" Height="18.9" Text="Printed :" HorzAlign="Right"/>
      <TextObject Name="Text8" Left="595.35" Width="122.85" Height="18.9" Text="[OmDate([Date])]"/>
      <TextObject Name="Text20" Width="94.5" Height="18.9" Text="Ledger :" HorzAlign="Right"/>
      <TextObject Name="Text10" Left="94.5" Width="113.4" Height="18.9" Text="[param_ledger_number_i] [param_ledger_name]"/>
      <TextObject Name="Text21" Top="18.9" Width="94.5" Height="18.9" Text="Currency :" HorzAlign="Right"/>
      <TextObject Name="Text18" Left="94.5" Top="18.9" Width="141.75" Height="18.9" Text="[param_currency_name]"/>
      <TextObject Name="Text22" Left="510.3" Top="18.9" Width="85.05" Height="18.9" Text="Balance at " HorzAlign="Right"/>
      <TextObject Name="Text42" Left="245.7" Top="18.9" Width="226.8" Height="18.9" Text="[param_ledger_name]" HorzAlign="Center"/>
      <LineObject Name="Line1" Left="718.2" Top="56.7" Width="-718.2"/>
      <TextObject Name="Text50" Left="595.35" Top="18.9" Width="207.9" Height="18.9" Text="[OmDate([param_end_date])]"/>
    </ReportTitleBand>
    <PageHeaderBand Name="PageHeader1" Top="69.48" Width="718.2" Height="18.9">
      <TextObject Name="Text3" Left="113.4" Width="151.2" Height="18.9" Text="Description" Font="Arial, 10pt, style=Bold, Italic"/>
      <TextObject Name="Text5" Left="349.65" Width="103.95" Height="18.9" Text="Actual" HorzAlign="Right" Font="Arial, 10pt, style=Bold, Italic"/>
      <TextObject Name="Text41" Left="37.8" Width="56.7" Height="18.9" Text="Acc" HorzAlign="Right" Font="Arial, 10pt, style=Bold, Italic"/>
      <TextObject Name="Text43" Left="463.05" Width="122.85" Height="18.9" Text="Actual Last Year" HorzAlign="Right" Font="Arial, 10pt, style=Bold, Italic"/>
    </PageHeaderBand>
    <DataBand Name="TransactionBand" Top="91.72" Width="718.2" Height="18.9" BeforePrintEvent="Data_BeforePrint" KeepChild="true" DataSource="BalanceSheet">
      <TextObject Name="ActualField" Left="340.2" Width="113.4" Height="18.9" Text="[BalanceSheet.actual]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="0" HorzAlign="Right" WordWrap="false" Font="Arial, 9pt" Trimming="EllipsisCharacter"/>
      <TextObject Name="ActualYTDField" Left="472.5" Width="113.4" Height="18.9" Text="[BalanceSheet.actuallastyear]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="0" HorzAlign="Right" WordWrap="false" Font="Arial, 9pt" Trimming="EllipsisCharacter"/>
      <TextObject Name="AccountCodeField" Left="45.36" Width="56.7" Height="18.9" Text="[BalanceSheet.accountcode]" Padding="0, 0, 0, 0" AutoShrink="FontSize" AutoShrinkMinSize="7" WordWrap="false" Font="Arial, 9pt"/>
      <TextObject Name="AccountNameField" Left="102.06" Width="236.25" Height="18.9" Text="[BalanceSheet.accountname]" Padding="0, 0, 2, 0" Font="Arial, 9pt"/>
      <ChildBand Name="HeaderBand" Top="113.95" Width="718.2" Height="18.9" Visible="false">
        <TextObject Name="HeaderAccountNameField" Left="30.24" Width="217.35" Height="18.9" Text="[BalanceSheet.accountname]:" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="0" Font="Arial, 9pt, style=Bold" Trimming="EllipsisCharacter"/>
        <ChildBand Name="FooterBand" Top="136.18" Width="718.2" Height="18.9" Visible="false">
          <TextObject Name="FooterActualField" Left="340.2" Width="113.4" Height="18.9" Text="[BalanceSheet.actual]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="0" HorzAlign="Right" WordWrap="false" Font="Arial, 9pt, style=Bold" TextFill.Color="DarkGray" Trimming="EllipsisCharacter"/>
          <TextObject Name="FooterActualYTDField" Left="472.5" Width="113.4" Height="18.9" Text="[BalanceSheet.actuallastyear]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="0" HorzAlign="Right" WordWrap="false" Font="Arial, 9pt, style=Bold" TextFill.Color="DarkGray" Trimming="EllipsisCharacter"/>
          <TextObject Name="FooterAccountNameField" Left="56.7" Width="274.05" Height="18.9" Text="Total [BalanceSheet.accountname]" Padding="0, 0, 2, 0" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="0" WordWrap="false" Font="Arial, 9pt, style=Bold" TextFill.Color="DarkGray" Trimming="EllipsisCharacter"/>
          <TextObject Name="FooterAccountCodeField" Width="56.7" Height="18.9" Text="[BalanceSheet.accountcode]" Padding="0, 0, 0, 0" AutoShrink="FontSize" AutoShrinkMinSize="7" WordWrap="false" Font="Arial, 9pt" TextFill.Color="DarkGray"/>
          <ChildBand Name="FooterLevel1" Top="158.42" Width="718.2" Height="37.8">
            <TextObject Name="Level1ActualField" Left="340.2" Width="113.4" Height="18.9" Text="[BalanceSheet.actual]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="0" HorzAlign="Right" WordWrap="false" Font="Arial, 9pt, style=Bold, Underline" Trimming="EllipsisCharacter"/>
            <TextObject Name="Level1ActualYTDField" Left="472.5" Width="113.4" Height="18.9" Text="[BalanceSheet.actuallastyear]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="0" HorzAlign="Right" WordWrap="false" Font="Arial, 9pt, style=Bold, Underline" Trimming="EllipsisCharacter"/>
            <TextObject Name="Level1AccountNameField" Width="207.9" Height="18.9" Text="[BalanceSheet.accountname]" Padding="0, 0, 2, 0" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="0" WordWrap="false" Font="Arial, 9pt, style=Bold" Trimming="EllipsisCharacter"/>
          </ChildBand>
        </ChildBand>
      </ChildBand>
    </DataBand>
    <ReportSummaryBand Name="ReportSummary1" Top="199.55" Width="718.2" Height="18.9">
      <TextObject Name="Text2" Left="340.2" Width="113.4" Height="18.9" Text="[TotalLiabPlusEq]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="0" HorzAlign="Right" Font="Arial, 9pt, style=Bold" Trimming="EllipsisCharacter"/>
      <TextObject Name="Text52" Width="217.35" Height="18.9" Text="Total Equity + Liabilities" Padding="0, 0, 2, 0" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="0" WordWrap="false" Font="Arial, 9pt, style=Bold" Trimming="EllipsisCharacter"/>
      <TextObject Name="Text4" Left="472.5" Width="113.4" Height="18.9" Text="[TotalLiabPlusEqLastYear]" Format="Number" Format.UseLocale="false" Format.DecimalDigits="2" Format.DecimalSeparator="." Format.GroupSeparator="," Format.NegativePattern="0" HorzAlign="Right" Font="Arial, 9pt, style=Bold" Trimming="EllipsisCharacter"/>
    </ReportSummaryBand>
    <PageFooterBand Name="PageFooter1" Top="221.78" Width="718.2" Height="18.9">
      <TextObject Name="Text17" Left="557.55" Width="160.65" Height="18.9" Text="[PageNofM]" HorzAlign="Right" Font="Arial, 9pt, style=Bold"/>
    </PageFooterBand>
  </ReportPage>
</Report>
');

SELECT TRUE;