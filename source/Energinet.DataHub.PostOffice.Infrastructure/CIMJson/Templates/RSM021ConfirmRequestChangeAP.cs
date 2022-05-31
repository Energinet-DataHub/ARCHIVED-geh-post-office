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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Elements;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Elements.Containers;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Reader;
using Microsoft.IO;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Templates;

internal class Rsm021ConfirmRequestChangeAp : BaseTemplate
{
    public Rsm021ConfirmRequestChangeAp(RecyclableMemoryStreamManager manager, Stream xmlData)
        : base(manager, xmlData)
    {
    }

    protected override void Convert()
    {
        JsonWriter.WriteStartObject();
        while (CimReader.Advance())
        {
            if (!CimReader.CurrentNodeName.Equals(
                    ElementNames.RootElement,
                    StringComparison.OrdinalIgnoreCase)) continue;

            if (CimReader.CurrentNodeType == NodeType.EndElement) continue;

            ReadConfirmRequestChangeAccountingPointCharacteristics();
        }

        JsonWriter.WriteEndObject();
    }

    private void ReadConfirmRequestChangeAccountingPointCharacteristics()
    {
        using var rootElement = new CimObjectElement(ElementNames.RootElement, 11); //PoolManager.GetObjectElement(ElementNames.RootElement, 11);
        var activityArray = rootElement.AddArray(ElementNames.Root.MktActivityRecordElement, 4, 10);
        do
        {
            if (CimReader.CurrentNodeType != NodeType.StartElement) continue;
            switch (CimReader.CurrentNodeName)
            {
                case ElementNames.Root.Mrid:
                    rootElement.AddString(ElementNames.Root.Mrid, 0, CimReader);
                    break;
                case ElementNames.Root.Type:
                    rootElement.AddObjectWithValueString(ElementNames.Root.Type, 1, CimReader);
                    break;
                case ElementNames.Root.ProcessProcessType:
                    rootElement.AddObjectWithValueString(ElementNames.Root.ProcessProcessType, 2, CimReader);
                    break;
                case ElementNames.Root.BusinessSectorType:
                    rootElement.AddObjectWithValueString(ElementNames.Root.BusinessSectorType, 3, CimReader);
                    break;
                case ElementNames.Root.SenderMarketParticipantmRid:
                    rootElement.AddObjectWithCodingScheme(ElementNames.Root.SenderMarketParticipantmRid, 4, CimReader);
                    break;
                case ElementNames.Root.SenderMarketParticipantMarketRoleType:
                    rootElement.AddObjectWithValueString(ElementNames.Root.SenderMarketParticipantMarketRoleType, 5, CimReader);
                    break;
                case ElementNames.Root.ReceiverMarketParticipantmRid:
                    rootElement.AddObjectWithCodingScheme(ElementNames.Root.ReceiverMarketParticipantmRid, 6, CimReader);
                    break;
                case ElementNames.Root.ReceiverMarketParticipantMarketRoleType:
                    rootElement.AddObjectWithValueString(ElementNames.Root.ReceiverMarketParticipantMarketRoleType, 7, CimReader);
                    break;
                case ElementNames.Root.CreatedDateTime:
                    rootElement.AddString(ElementNames.Root.CreatedDateTime, 8, CimReader);
                    break;
                case ElementNames.Root.ReasonCode:
                    rootElement.AddObjectWithValueString(ElementNames.Root.ReasonCode, 9, CimReader);
                    break;
                case ElementNames.Root.MktActivityRecordElement:
                    ReadMktActivityRecord(activityArray);
                    break;
            }
        }
        while (CimReader.AdvanceUntilClosed(ElementNames.RootElement));

        rootElement.WriteJson(JsonWriter);
    }

    private void ReadMktActivityRecord(CimArrayElement activityArray)
    {
        activityArray.BeginNewArrayElement();
        do
        {
            if (CimReader.CurrentNodeType != NodeType.StartElement) continue;
            switch (CimReader.CurrentNodeName)
            {
                case ElementNames.Root.MktActivityRecord.Mrid:
                    activityArray.AddString(ElementNames.Root.MktActivityRecord.Mrid, 0, CimReader);
                    break;
                case ElementNames.Root.MktActivityRecord.MarketEvaluationPointmRID:
                    activityArray.AddObjectWithCodingScheme(ElementNames.Root.MktActivityRecord.MarketEvaluationPointmRID, 1, CimReader);
                    break;
                case ElementNames.Root.MktActivityRecord.BusinessProcessReferenceMktActivityRecordmRID:
                    activityArray.AddString(ElementNames.Root.MktActivityRecord.BusinessProcessReferenceMktActivityRecordmRID, 2, CimReader);
                    break;
                case ElementNames.Root.MktActivityRecord.OriginalTransactionIDReferenceMktActivityRecordmRID:
                    activityArray.AddString(ElementNames.Root.MktActivityRecord.OriginalTransactionIDReferenceMktActivityRecordmRID, 3, CimReader);
                    break;
            }
        }
        while (CimReader.AdvanceUntilClosed(ElementNames.Root.MktActivityRecordElement));
    }

    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass", Justification = "Names matches those from xml for ease of identification")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Names matches those from xml for ease of identification")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Names matches those from xml for ease of identification")]
    private static class ElementNames
    {
        public const string RootElement = "ConfirmRequestChangeAccountingPointCharacteristics_MarketDocument";

        public static class Root
        {
            public const string Mrid = "mRID";
            public const string Type = "type";
            public const string ProcessProcessType = "process.processType";
            public const string BusinessSectorType = "businessSector.type";
            public const string SenderMarketParticipantmRid = "sender_MarketParticipant.mRID";
            public const string SenderMarketParticipantMarketRoleType = "sender_MarketParticipant.marketRole.type";
            public const string ReceiverMarketParticipantmRid = "receiver_MarketParticipant.mRID";
            public const string ReceiverMarketParticipantMarketRoleType = "receiver_MarketParticipant.marketRole.type";
            public const string CreatedDateTime = "createdDateTime";
            public const string ReasonCode = "reason.code";
            public const string MktActivityRecordElement = "MktActivityRecord";

            public static class MktActivityRecord
            {
                public const string Mrid = "mRID";
                public const string BusinessProcessReferenceMktActivityRecordmRID = "businessProcessReference_MktActivityRecord.mRID";
                public const string OriginalTransactionIDReferenceMktActivityRecordmRID = "originalTransactionIDReference_MktActivityRecord.mRID";
                public const string MarketEvaluationPointmRID = "marketEvaluationPoint.mRID";
            }
        }
    }
}
