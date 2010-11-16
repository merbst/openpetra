﻿//
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
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using Ict.Tools.CodeGeneration;
using Ict.Common.IO;
using Ict.Common;
using Ict.Tools.DBXML;

namespace Ict.Tools.CodeGeneration.ExtJs
{
    public class TextFieldGenerator : TControlGenerator
    {
        public TextFieldGenerator()
            : base("txt", "textfield")
        {
            FDefaultWidth = -1;
        }

        public override bool ControlFitsNode(XmlNode curNode)
        {
            if (base.ControlFitsNode(curNode))
            {
                if ((TYml2Xml.GetAttribute(curNode, "Multiline") != "true"))
                {
                    return true;
                }
            }

            return false;
        }
    }
    public class TextAreaGenerator : TControlGenerator
    {
        public TextAreaGenerator()
            : base("txt", "textarea")
        {
            FDefaultWidth = -1;
        }

        public override bool ControlFitsNode(XmlNode curNode)
        {
            if (base.ControlFitsNode(curNode))
            {
                if ((TYml2Xml.GetAttribute(curNode, "Multiline") == "true"))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class UploadGenerator : TControlGenerator
    {
        public UploadGenerator()
            : base("upl", "fileuploadfield")
        {
            FControlDefinitionSnippetName = "FILEUPLOADDEFINITION";
        }

        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ctrl)
        {
            ProcessTemplate ctrlSnippet = base.SetControlProperties(writer, ctrl);

            writer.FTemplate.SetCodelet("CONTAINSFILEUPLOAD", "true");

            return ctrlSnippet;
        }
    }
    public class LabelGenerator : TControlGenerator
    {
        public LabelGenerator()
            : base("lbl", "displayfield")
        {
            FControlDefinitionSnippetName = "LABELDEFINITION";
        }
    }
    public class FieldSetGenerator : TControlGenerator
    {
        public FieldSetGenerator()
            : base("pnl", "fieldset")
        {
        }
    }
    public class ButtonGenerator : TControlGenerator
    {
        public ButtonGenerator()
            : base("btn", "button")
        {
            FControlDefinitionSnippetName = "SUBMITBUTTONDEFINITION";
        }

        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ctrl)
        {
            ProcessTemplate ctrlSnippet = base.SetControlProperties(writer, ctrl);

            if (ctrl.HasAttribute("AjaxRequestUrl"))
            {
                ((TExtJsFormsWriter)writer).AddResourceString(ctrlSnippet, "VALIDATIONERRORTITLE", ctrl, ctrl.GetAttribute("ValidationErrorTitle"));
                ((TExtJsFormsWriter)writer).AddResourceString(ctrlSnippet, "VALIDATIONERRORMESSAGE", ctrl, ctrl.GetAttribute("ValidationErrorMessage"));
                ((TExtJsFormsWriter)writer).AddResourceString(ctrlSnippet, "SENDINGDATATITLE", ctrl, ctrl.GetAttribute("SendingMessageTitle"));
                ((TExtJsFormsWriter)writer).AddResourceString(ctrlSnippet, "SENDINGDATAMESSAGE", ctrl, ctrl.GetAttribute("SendingMessage"));
                ((TExtJsFormsWriter)writer).AddResourceString(ctrlSnippet, "REQUESTSUCCESSTITLE", ctrl, ctrl.GetAttribute("SuccessMessageTitle"));
                ((TExtJsFormsWriter)writer).AddResourceString(ctrlSnippet, "REQUESTSUCCESSMESSAGE", ctrl, ctrl.GetAttribute("SuccessMessage"));
                ((TExtJsFormsWriter)writer).AddResourceString(ctrlSnippet, "REQUESTFAILURETITLE", ctrl, ctrl.GetAttribute("FailureMessageTitle"));
                ((TExtJsFormsWriter)writer).AddResourceString(ctrlSnippet, "REQUESTFAILUREMESSAGE", ctrl, ctrl.GetAttribute("FailureMessage"));
            }

            ctrlSnippet.SetCodelet("REQUESTURL", ctrl.GetAttribute("AjaxRequestUrl"));
            ctrlSnippet.SetCodelet("REDIRECTURLONSUCCESS", ctrl.GetAttribute("RedirectURLOnSuccess"));

            XmlNode AjaxParametersNode = TYml2Xml.GetChild(ctrl.xmlNode, "AjaxRequestParameters");

            if (AjaxParametersNode != null)
            {
                string ParameterString = String.Empty;

                foreach (XmlAttribute attr in AjaxParametersNode.Attributes)
                {
                    if (!attr.Name.Equals("depth"))
                    {
                        ParameterString += attr.Name + ": '" + attr.Value + "', ";
                    }
                }

                ctrlSnippet.SetCodelet("REQUESTPARAMETERS", ParameterString);
                writer.FTemplate.SetCodelet("REQUESTPARAMETERS", "true");
            }

            return ctrlSnippet;
        }
    }
    public class ComboboxGenerator : TControlGenerator
    {
        public ComboboxGenerator()
            : base("cmb", "checkbox")
        {
            FControlDefinitionSnippetName = "COMBOBOXDEFINITION";
        }

        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ctrl)
        {
            ProcessTemplate ctrlSnippet = base.SetControlProperties(writer, ctrl);

            string valuesArray = "[";
            StringCollection optionalValues =
                TYml2Xml.GetElements(TXMLParser.GetChild(ctrl.xmlNode, "OptionalValues"));

            // DefaultValue with = sign before control name
            for (int counter = 0; counter < optionalValues.Count; counter++)
            {
                if (optionalValues[counter].StartsWith("="))
                {
                    optionalValues[counter] = optionalValues[counter].Substring(1).Trim();
                }

                if (counter > 0)
                {
                    valuesArray += ", ";
                }

                ((TExtJsFormsWriter)writer).AddResourceString(ctrlSnippet, "OPTION" + counter.ToString(), ctrl, optionalValues[counter]);

                string strName = "this." + ctrl.controlName + "OPTION" + counter.ToString();

                valuesArray += "[" + strName + ", " + strName + "]";
            }

            valuesArray += "]";

            ctrlSnippet.SetCodelet("OPTIONALVALUESARRAY", valuesArray);

            return ctrlSnippet;
        }
    }
    public class CheckboxGenerator : TControlGenerator
    {
        public CheckboxGenerator()
            : base("chk", "checkbox")
        {
            FControlDefinitionSnippetName = "CHECKBOXDEFINITION";
        }

        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ctrl)
        {
            ProcessTemplate ctrlSnippet = base.SetControlProperties(writer, ctrl);

            ctrlSnippet.SetCodelet("BOXLABEL", ctrlSnippet.FCodelets["LABEL"].ToString());
            ctrlSnippet.SetCodelet("LABEL", "strEmpty");
            return ctrlSnippet;
        }
    }
    public class RadioButtonGenerator : TControlGenerator
    {
        public RadioButtonGenerator()
            : base("rbt", "radio")
        {
            FControlDefinitionSnippetName = "CHECKBOXDEFINITION";
        }

        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ctrl)
        {
            ProcessTemplate ctrlSnippet = base.SetControlProperties(writer, ctrl);

            if (TXMLParser.HasAttribute(ctrl.xmlNode, "RadioChecked"))
            {
                ctrlSnippet.SetCodelet("CHECKED", "true");
            }

            ctrlSnippet.SetCodelet("BOXLABEL", ctrlSnippet.FCodelets["LABEL"].ToString());
            ctrlSnippet.SetCodelet("LABEL", "strEmpty");

            return ctrlSnippet;
        }
    }
    public class DateTimePickerGenerator : TControlGenerator
    {
        public DateTimePickerGenerator()
            : base("dtp", "datefield")
        {
            FDefaultWidth = -1;
        }

        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ACtrl)
        {
            ProcessTemplate ctrlSnippet = base.SetControlProperties(writer, ACtrl);

            // TODO: adjust date format to localisation? see http://dev.sencha.com/deploy/dev/docs/?class=Date
            ctrlSnippet.SetCodelet("CUSTOMATTRIBUTES", "format: 'd.m.Y'," +
                Environment.NewLine +
                "boxMaxWidth: 175,");

            return ctrlSnippet;
        }
    }

    public class InlineGenerator : TControlGenerator
    {
        public InlineGenerator()
            : base("inl", "displayfield")
        {
        }

        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ACtrl)
        {
            ProcessTemplate ctrlSnippet = base.SetControlProperties(writer, ACtrl);

            ((TExtJsFormsWriter)writer).AddResourceString(ctrlSnippet,
                "DESCRIPTIONDOCUMENT",
                ACtrl,
                TYml2Xml.GetAttribute(ACtrl.xmlNode, "description"));
            ((TExtJsFormsWriter)writer).AddResourceString(ctrlSnippet,
                "URL",
                ACtrl,
                TYml2Xml.GetAttribute(ACtrl.xmlNode, "url"));
            ((TExtJsFormsWriter)writer).AddResourceString(ctrlSnippet,
                "BROWSERMISSINGIFRAMESUPPORT",
                ACtrl,
                "Your browser is not able to display embedded documents. Please click on the following link to read the text: ");

            ctrlSnippet.SetCodelet("HTML",
                String.Format("<iframe src=\"' + {0} + '\" width=\"100%\" height=\"250\"><p>{1}<a href=\"' + {0} + '\">' + {2} + '</a></p></iframe>",
                    "this." + ACtrl.controlName + "URL",
                    "' + this." + ACtrl.controlName + "BROWSERMISSINGIFRAMESUPPORT + '",
                    "this." + ACtrl.controlName + "DESCRIPTIONDOCUMENT"));

            return ctrlSnippet;
        }
    }

    public class CompositeGenerator : GroupBoxBaseGenerator
    {
        public CompositeGenerator()
            : base("cmp")
        {
            FControlDefinitionSnippetName = "COMPOSITEDEFINITION";
        }
    }

    public class GroupBoxGenerator : GroupBoxBaseGenerator
    {
        public GroupBoxGenerator()
            : base("grp")
        {
            FControlDefinitionSnippetName = "GROUPBOXDEFINITION";
        }
    }

    public class AssistantGenerator : GroupBoxBaseGenerator
    {
        public AssistantGenerator()
            : base("ass")
        {
            FControlDefinitionSnippetName = "ASSISTANTDEFINITION";
        }

        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ctrl)
        {
            ProcessTemplate ctrlSnippet = base.SetControlProperties(writer, ctrl);

            writer.FTemplate.SetCodelet("ASSISTANT", "true");

            string AssistantHeader = "true";

            if (ctrl.HasAttribute("AssistantHeader"))
            {
                AssistantHeader = ctrl.GetAttribute("AssistantHeader");
            }

            ctrlSnippet.SetCodelet("ASSISTANTHEADER", AssistantHeader);

            return ctrlSnippet;
        }
    }

    public class AssistantPageGenerator : GroupBoxBaseGenerator
    {
        private static int PageCounter = 0;

        public AssistantPageGenerator()
            : base("asp")
        {
            FControlDefinitionSnippetName = "ASSISTANTPAGEDEFINITION";
        }

        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ACtrl)
        {
            ProcessTemplate ctrlSnippet = base.SetControlProperties(writer, ACtrl);

            ctrlSnippet.SetCodelet("PAGENUMBER", PageCounter.ToString());
            PageCounter++;

            return ctrlSnippet;
        }
    }

    public class RadioGroupSimpleGenerator : GroupBoxBaseGenerator
    {
        public RadioGroupSimpleGenerator()
            : base("rgr")
        {
            FControlDefinitionSnippetName = "RADIOGROUPDEFINITION";
        }

        public override bool ControlFitsNode(XmlNode curNode)
        {
            if (base.ControlFitsNode(curNode))
            {
                if (TXMLParser.GetChild(curNode, "OptionalValues") != null)
                {
                    return true;
                }
            }

            return false;
        }

        public override StringCollection FindContainedControls(TFormWriter writer, XmlNode curNode)
        {
            StringCollection optionalValues =
                TYml2Xml.GetElements(TXMLParser.GetChild(curNode, "OptionalValues"));
            string DefaultValue = optionalValues[0];

            if (TXMLParser.HasAttribute(curNode, "DefaultValue"))
            {
                DefaultValue = TXMLParser.GetAttribute(curNode, "DefaultValue");
            }
            else
            {
                // DefaultValue with = sign before control name
                for (int counter = 0; counter < optionalValues.Count; counter++)
                {
                    if (optionalValues[counter].StartsWith("="))
                    {
                        optionalValues[counter] = optionalValues[counter].Substring(1).Trim();
                        DefaultValue = optionalValues[counter];
                    }
                }
            }

            // add the radiobuttons on the fly
            StringCollection Controls = new StringCollection();

            foreach (string optionalValue in optionalValues)
            {
                string radioButtonName = "rbt" +
                                         StringHelper.UpperCamelCase(optionalValue.Replace("'", "").Replace(" ",
                        "_").Replace("&",
                        ""), false, false);
                TControlDef newCtrl = writer.CodeStorage.FindOrCreateControl(radioButtonName, curNode.Name);
                newCtrl.Label = optionalValue;

                if (StringHelper.IsSame(DefaultValue, optionalValue))
                {
                    newCtrl.SetAttribute("RadioChecked", "true");
                }

                Controls.Add(radioButtonName);
            }

            return Controls;
        }
    }
}