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
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Elements;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Factories;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Reader;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Templates;

internal class RSM033RejectRequestChangePricelist : BaseTemplate
{
    protected override void Convert()
    {
        JsonWriter().WriteStartObject();
        while (CimReader().Advance())
        {
            if (!CimReader().CurrentNodeName.Equals(
                    ElementNames.RootElement,
                    StringComparison.OrdinalIgnoreCase)) continue;

            if (CimReader().CurrentNodeType == NodeType.EndElement) continue;

            ReadRejectRequestChangePriceList();
        }

        JsonWriter().WriteEndObject();
    }

    private void ReadRejectRequestChangePriceList()
    {
        var rootElement = CimJsonObjectPools.GetObjectElement(ElementNames.RootElement, 11);
        var activityArray = CimJsonObjectPools.GetArrayElement(ElementNames.Root.MktActivityRecordElement, 4);
        rootElement.AddElement(10, activityArray);
        do
        {
            if (CimReader().CurrentNodeType != NodeType.StartElement) continue;
            switch (CimReader().CurrentNodeName)
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
                    ReadMktActivityRecord(activityArray);
                    break;
            }
        }
        while (CimReader().AdvanceUntilClosed(ElementNames.RootElement));
        rootElement.WriteJson(JsonWriter());
    }

    private void ReadMktActivityRecord(CimArrayElement activityArray)
    {
        activityArray.BeginNewArrayElement();
        var reasonArray = CimJsonObjectPools.GetArrayElement(ElementNames.Root.MktActivityRecord.ReasonElement, 2);
        activityArray.AddElement(4, reasonArray);
        do
        {
            if (CimReader().CurrentNodeType != NodeType.StartElement) continue;
            switch (CimReader().CurrentNodeName)
            {
                case ElementNames.Root.MktActivityRecord.MRID:
                    activityArray.AddElement(0, WriteAsString(ElementNames.Root.MktActivityRecord.MRID));
                    break;
                case ElementNames.Root.MktActivityRecord.BusinessProcessReferenceMktActivityRecordmRID:
                    activityArray.AddElement(1, WriteAsString(ElementNames.Root.MktActivityRecord.BusinessProcessReferenceMktActivityRecordmRID));
                    break;
                case ElementNames.Root.MktActivityRecord.OriginalTransactionIDReferenceMktActivityRecordmRID:
                    activityArray.AddElement(2, WriteAsString(ElementNames.Root.MktActivityRecord.OriginalTransactionIDReferenceMktActivityRecordmRID));
                    break;
                case ElementNames.Root.MktActivityRecord.ReasonElement:
                    ReadReason(reasonArray);
                    break;
            }
        }
        while (CimReader().AdvanceUntilClosed(ElementNames.Root.MktActivityRecordElement));
    }

    private void ReadReason(CimArrayElement reasonArray)
    {
        reasonArray.BeginNewArrayElement();
        do
        {
            if (CimReader().CurrentNodeType != NodeType.StartElement) continue;
            switch (CimReader().CurrentNodeName)
            {
                case ElementNames.Root.MktActivityRecord.Reason.Code:
                    reasonArray.AddElement(0, WriteAsStringValueObject(ElementNames.Root.MktActivityRecord.Reason.Code));
                    break;
                case ElementNames.Root.MktActivityRecord.Reason.Text:
                    reasonArray.AddElement(1, WriteAsString(ElementNames.Root.MktActivityRecord.Reason.Text));
                    break;
            }
        }
        while (CimReader().AdvanceUntilClosed(ElementNames.Root.MktActivityRecord.ReasonElement));
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
