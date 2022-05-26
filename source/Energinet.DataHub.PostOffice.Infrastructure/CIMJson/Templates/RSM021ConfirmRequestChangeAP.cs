// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using TestJSonConversion.SimpleCimJson.Elements;
using TestJSonConversion.SimpleCimJson.Factories;
using TestJSonConversion.SimpleCimJson.Reader;

namespace TestJSonConversion.SimpleCimJson.Templates;

internal class Rsm021ConfirmRequestChangeAp : BaseTemplate
{
    protected override void Convert()
    {
        _jsonWriter.WriteStartObject();
        while (_reader.Advance())
        {
            if (!_reader.CurrentNodeName.Equals(ElementNames.RootElement,
                    StringComparison.OrdinalIgnoreCase)) continue;
            if (_reader.CurrentNodeType == NodeType.EndElement) continue;
                ReadConfirmRequestChangeAccountingPointCharacteristics();
        }
        _jsonWriter.WriteEndObject();
    }

    private void ReadConfirmRequestChangeAccountingPointCharacteristics()
    {
        var rootElement = CimJsonObjectPools.GetObjectElement(ElementNames.RootElement, 11);
        var mkActivityArray = new CimArrayElement( ElementNames.Root.MktActivityRecordElement, 4);
        do
        {
            if (_reader.CurrentNodeType != NodeType.StartElement ) continue;
            switch (_reader.CurrentNodeName)
            {
                case ElementNames.Root.MRID:
                    rootElement.AddElement(0, WriteAsString(ElementNames.Root.MRID));
                    break;
                case ElementNames.Root.Type:
                    rootElement.AddElement(1, WriteAsStringValueObject(ElementNames.Root.Type));
                    break;
                case ElementNames.Root.ProcessProcessType:
                    rootElement.AddElement(2, WriteAsStringValueObject(ElementNames.Root.ProcessProcessType));
                    break;
                case ElementNames.Root.BusinessSectorType:
                    rootElement.AddElement(3, WriteAsStringValueObject(ElementNames.Root.BusinessSectorType));
                    break;
                case ElementNames.Root.SenderMarketParticipantmRID:
                    rootElement.AddElement(4,  WriteObjectWithCodingSchemeElement(ElementNames.Root.SenderMarketParticipantmRID));
                    break;
                case ElementNames.Root.SenderMarketParticipantMarketRoleType:
                    rootElement.AddElement(5,  WriteAsStringValueObject(ElementNames.Root.SenderMarketParticipantMarketRoleType));
                    break;
                case ElementNames.Root.ReceiverMarketParticipantmRID:
                    rootElement.AddElement(6,  WriteObjectWithCodingSchemeElement(ElementNames.Root.ReceiverMarketParticipantmRID));
                    break;
                case ElementNames.Root.ReceiverMarketParticipantMarketRoleType:
                    rootElement.AddElement(7,  WriteAsStringValueObject(ElementNames.Root.ReceiverMarketParticipantMarketRoleType));
                    break;
                case ElementNames.Root.CreatedDateTime:
                    rootElement.AddElement(8,  WriteAsString(ElementNames.Root.CreatedDateTime));
                    break;
                case ElementNames.Root.ReasonCode:
                    rootElement.AddElement(9,  WriteAsStringValueObject(ElementNames.Root.ReasonCode));
                    break;
                 case ElementNames.Root.MktActivityRecordElement:
                     ReadMktActivityRecord(mkActivityArray);
                     break;
            }
        } while (_reader.AdvanceUntilClosed(ElementNames.RootElement));
        rootElement.AddElement(10, mkActivityArray);
        rootElement.WriteJson(_jsonWriter);
        rootElement.ReturnToPool();
    }

    private void ReadMktActivityRecord(CimArrayElement mkActivityArray)
    {
        mkActivityArray.BeginNewArrayElement();
        do
        {
            if (_reader.CurrentNodeType != NodeType.StartElement ) continue;
            switch (_reader.CurrentNodeName)
            {
                case ElementNames.Root.MktActivityRecord.MRID:
                    mkActivityArray.AddElement(0, WriteAsString(ElementNames.Root.MktActivityRecord.MRID));
                    break;
                case ElementNames.Root.MktActivityRecord.MarketEvaluationPointmRID:
                    mkActivityArray.AddElement(1, WriteObjectWithCodingSchemeElement(ElementNames.Root.MktActivityRecord.MarketEvaluationPointmRID));
                    break;
                case ElementNames.Root.MktActivityRecord.BusinessProcessReferenceMktActivityRecordmRID:
                    mkActivityArray.AddElement(2, WriteAsString(ElementNames.Root.MktActivityRecord.BusinessProcessReferenceMktActivityRecordmRID));
                    break;
                case ElementNames.Root.MktActivityRecord.OriginalTransactionIDReferenceMktActivityRecordmRID:
                    mkActivityArray.AddElement(3, WriteAsString(ElementNames.Root.MktActivityRecord.OriginalTransactionIDReferenceMktActivityRecordmRID));
                    break;
            }
        }
        while (_reader.AdvanceUntilClosed(ElementNames.Root.MktActivityRecordElement));
    }
    private static class ElementNames
    {
        public const string RootElement = "ConfirmRequestChangeAccountingPointCharacteristics_MarketDocument";

        public static class Root
        {
            public const string MRID = "mRID";
            public const string Type = "type";
            public const string ProcessProcessType = "process.processType";
            public const string BusinessSectorType = "businessSector.type";
            public const string SenderMarketParticipantmRID = "sender_MarketParticipant.mRID";
            public const string SenderMarketParticipantMarketRoleType = "sender_MarketParticipant.marketRole.type";
            public const string ReceiverMarketParticipantmRID = "receiver_MarketParticipant.mRID";
            public const string ReceiverMarketParticipantMarketRoleType = "receiver_MarketParticipant.marketRole.type";
            public const string CreatedDateTime = "createdDateTime";
            public const string ReasonCode = "reason.code";
            public const string MktActivityRecordElement = "MktActivityRecord";

            public static class MktActivityRecord
            {
                public const string MRID = "mRID";
                public const string BusinessProcessReferenceMktActivityRecordmRID = "businessProcessReference_MktActivityRecord.mRID";
                public const string OriginalTransactionIDReferenceMktActivityRecordmRID = "originalTransactionIDReference_MktActivityRecord.mRID";
                public const string MarketEvaluationPointmRID = "marketEvaluationPoint.mRID";
            }
        }
    }

}