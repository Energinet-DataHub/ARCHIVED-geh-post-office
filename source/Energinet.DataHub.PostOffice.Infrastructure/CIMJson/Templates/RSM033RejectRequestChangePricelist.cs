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

internal class RSM033RejectRequestChangePricelist : BaseTemplate
{
    public RSM033RejectRequestChangePricelist(RecyclableMemoryStreamManager manager, Stream xmlData)
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

            ReadRejectRequestChangePriceList();
        }

        JsonWriter.WriteEndObject();
    }

    private void ReadRejectRequestChangePriceList()
    {
        using var rootElement = new CimObjectElement(ElementNames.RootElement, 11);
        var activityArray = rootElement.AddArray(ElementNames.Root.MktActivityRecordElement, 4, 10);
        do
        {
            if (CimReader.CurrentNodeType != NodeType.StartElement) continue;
            switch (CimReader.CurrentNodeName)
            {
                case ElementNames.Root.MRID:
                    rootElement.AddString(ElementNames.Root.MRID, 0, CimReader);
                    break;
                case ElementNames.Root.Type:
                    rootElement.AddObjectWithValueString(ElementNames.Root.ProcessProcessType, 1, CimReader);
                    break;
                case ElementNames.Root.ProcessProcessType:
                    rootElement.AddObjectWithValueString(ElementNames.Root.ProcessProcessType, 2, CimReader);
                    break;
                case ElementNames.Root.BusinessSectorType:
                    rootElement.AddObjectWithValueString(ElementNames.Root.BusinessSectorType, 3, CimReader);
                    break;
                case ElementNames.Root.SenderMarketParticipantmRID:
                    rootElement.AddObjectWithCodingScheme(ElementNames.Root.SenderMarketParticipantmRID, 4,  CimReader);
                    break;
                case ElementNames.Root.SenderMarketParticipantMarketRoleType:
                    rootElement.AddObjectWithValueString(ElementNames.Root.SenderMarketParticipantMarketRoleType, 5,  CimReader);
                    break;
                case ElementNames.Root.ReceiverMarketParticipantmRID:
                    rootElement.AddObjectWithCodingScheme(ElementNames.Root.ReceiverMarketParticipantmRID, 6,  CimReader);
                    break;
                case ElementNames.Root.ReceiverMarketParticipantMarketRoleType:
                    rootElement.AddObjectWithValueString(ElementNames.Root.ReceiverMarketParticipantMarketRoleType, 7,  CimReader);
                    break;
                case ElementNames.Root.CreatedDateTime:
                    rootElement.AddString(ElementNames.Root.CreatedDateTime, 8,  CimReader);
                    break;
                case ElementNames.Root.ReasonCode:
                    rootElement.AddObjectWithValueString(ElementNames.Root.ReasonCode, 9,  CimReader);
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
        var reasonArray = activityArray.AddArray(ElementNames.Root.MktActivityRecord.ReasonElement, 2, 4);
        do
        {
            if (CimReader.CurrentNodeType != NodeType.StartElement) continue;
            switch (CimReader.CurrentNodeName)
            {
                case ElementNames.Root.MktActivityRecord.MRID:
                    activityArray.AddString(ElementNames.Root.MktActivityRecord.MRID, 0, CimReader);
                    break;
                case ElementNames.Root.MktActivityRecord.BusinessProcessReferenceMktActivityRecordmRID:
                    activityArray.AddString(ElementNames.Root.MktActivityRecord.BusinessProcessReferenceMktActivityRecordmRID, 1, CimReader);
                    break;
                case ElementNames.Root.MktActivityRecord.OriginalTransactionIDReferenceMktActivityRecordmRID:
                    activityArray.AddString(ElementNames.Root.MktActivityRecord.OriginalTransactionIDReferenceMktActivityRecordmRID, 2, CimReader);
                    break;
                case ElementNames.Root.MktActivityRecord.ReasonElement:
                    ReadReason(reasonArray);
                    break;
            }
        }
        while (CimReader.AdvanceUntilClosed(ElementNames.Root.MktActivityRecordElement));
    }

    private void ReadReason(CimArrayElement reasonArray)
    {
        reasonArray.BeginNewArrayElement();
        do
        {
            if (CimReader.CurrentNodeType != NodeType.StartElement) continue;
            switch (CimReader.CurrentNodeName)
            {
                case ElementNames.Root.MktActivityRecord.Reason.Code:
                    reasonArray.AddObjectWithValueString(ElementNames.Root.MktActivityRecord.Reason.Code, 0, CimReader);
                    break;
                case ElementNames.Root.MktActivityRecord.Reason.Text:
                    reasonArray.AddString(ElementNames.Root.MktActivityRecord.Reason.Text, 1, CimReader);
                    break;
            }
        }
        while (CimReader.AdvanceUntilClosed(ElementNames.Root.MktActivityRecord.ReasonElement));
    }

    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass", Justification = "Names matches those from xml for ease of identification")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Names matches those from xml for ease of identification")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Names matches those from xml for ease of identification")]
    private static class ElementNames
    {
        public const string RootElement = "RejectRequestChangeOfPriceList_MarketDocument";

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
                public const string OriginalTransactionIDReferenceMktActivityRecordmRID = "originalTransactionIDReference_MktActivityRecord.mRID";
                public const string BusinessProcessReferenceMktActivityRecordmRID = "businessProcessReference_MktActivityRecord.mRID";
                public const string ReasonElement = "Reason";

                public static class Reason
                {
                    public const string Code = "code";
                    public const string Text = "text";
                }
            }
        }
    }
}
