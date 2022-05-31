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

internal class RSM031NotifyBillingMasterData : BaseTemplate
{
    public RSM031NotifyBillingMasterData(RecyclableMemoryStreamManager manager, Stream xmlData)
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

            ReadNotifyBillingMasterData();
        }

        JsonWriter.WriteEndObject();
    }

    private void ReadNotifyBillingMasterData()
    {
        using var rootElement = new CimObjectElement(ElementNames.RootElement, 10); // PoolManager.GetObjectElement(ElementNames.RootElement, 10);
        var activityArray = rootElement.AddArray(ElementNames.Root.MktActivityRecordElement, 5, 9); //PoolManager.GetArrayElement(ElementNames.Root.MktActivityRecordElement, 5);
        do
        {
            if (CimReader.CurrentNodeType != NodeType.StartElement) continue;
            switch (CimReader.CurrentNodeName)
            {
                case ElementNames.Root.MRID:
                    rootElement.AddString(ElementNames.Root.MRID, 0, CimReader);
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
                case ElementNames.Root.SenderMarketParticipantmRID:
                    rootElement.AddObjectWithCodingScheme(ElementNames.Root.SenderMarketParticipantmRID, 4, CimReader);
                    break;
                case ElementNames.Root.SenderMarketParticipantMarketRoleType:
                    rootElement.AddObjectWithValueString(ElementNames.Root.SenderMarketParticipantMarketRoleType, 5, CimReader);
                    break;
                case ElementNames.Root.ReceiverMarketParticipantmRID:
                    rootElement.AddObjectWithCodingScheme(ElementNames.Root.ReceiverMarketParticipantmRID, 6, CimReader);
                    break;
                case ElementNames.Root.ReceiverMarketParticipantMarketRoleType:
                    rootElement.AddObjectWithValueString(ElementNames.Root.ReceiverMarketParticipantMarketRoleType, 7, CimReader);
                    break;
                case ElementNames.Root.CreatedDateTime:
                    rootElement.AddString(ElementNames.Root.CreatedDateTime, 8, CimReader);
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
        var chargeGroup = activityArray.AddObject(ElementNames.Root.MktActivityRecord.ChargeGroupElement, 1, 4);
        do
        {
            if (CimReader.CurrentNodeType != NodeType.StartElement) continue;
            switch (CimReader.CurrentNodeName)
            {
                case ElementNames.Root.MktActivityRecord.MRID:
                    activityArray.AddString(ElementNames.Root.MktActivityRecord.MRID, 0, CimReader);
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
                case ElementNames.Root.MktActivityRecord.ChargeGroupElement:
                    ReadChargeGroup(chargeGroup);
                    break;
            }
        }
        while (CimReader.AdvanceUntilClosed(ElementNames.Root.MktActivityRecordElement));
    }

    private void ReadChargeGroup(CimObjectElement chargeGroup)
    {
        var chargeTypeArray = chargeGroup.AddArray(ElementNames.Root.MktActivityRecord.ChargeGroup.ChargeTypeElement, 6, 0);
        do
        {
            if (CimReader.CurrentNodeType != NodeType.StartElement) continue;
            switch (CimReader.CurrentNodeName)
            {
                case ElementNames.Root.MktActivityRecord.ChargeGroup.ChargeTypeElement:
                    ReadChargeType(chargeTypeArray);
                    break;
            }
        }
        while (CimReader.AdvanceUntilClosed(ElementNames.Root.MktActivityRecord.ChargeGroupElement));
    }

    private void ReadChargeType(CimArrayElement chargeTypeArray)
    {
        chargeTypeArray.BeginNewArrayElement();
        do
        {
            if (CimReader.CurrentNodeType != NodeType.StartElement) continue;
            switch (CimReader.CurrentNodeName)
            {
                case ElementNames.Root.MktActivityRecord.ChargeGroup.ChargeType.MRID:
                    chargeTypeArray.AddString(ElementNames.Root.MktActivityRecord.ChargeGroup.ChargeType.MRID, 0, CimReader);
                    break;
                case ElementNames.Root.MktActivityRecord.ChargeGroup.ChargeType.ChargeTypeOwnerMarketParticipantmRID:
                    chargeTypeArray.AddObjectWithCodingScheme(ElementNames.Root.MktActivityRecord.ChargeGroup.ChargeType.ChargeTypeOwnerMarketParticipantmRID, 1, CimReader);
                    break;
                case ElementNames.Root.MktActivityRecord.ChargeGroup.ChargeType.EffectiveDate:
                    chargeTypeArray.AddString(ElementNames.Root.MktActivityRecord.ChargeGroup.ChargeType.EffectiveDate, 2, CimReader);
                    break;
                case ElementNames.Root.MktActivityRecord.ChargeGroup.ChargeType.Factor:
                    chargeTypeArray.AddString(ElementNames.Root.MktActivityRecord.ChargeGroup.ChargeType.Factor, 3, CimReader);
                    break;
                case ElementNames.Root.MktActivityRecord.ChargeGroup.ChargeType.TerminationDate:
                    chargeTypeArray.AddString(ElementNames.Root.MktActivityRecord.ChargeGroup.ChargeType.TerminationDate, 4, CimReader);
                    break;
                case ElementNames.Root.MktActivityRecord.ChargeGroup.ChargeType.Type:
                    chargeTypeArray.AddObjectWithValueString(ElementNames.Root.MktActivityRecord.ChargeGroup.ChargeType.Type, 5, CimReader);
                    break;
            }
        }
        while (CimReader.AdvanceUntilClosed(ElementNames.Root.MktActivityRecord.ChargeGroup.ChargeTypeElement));
    }

    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass", Justification = "Names matches those from xml for ease of identification")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Names matches those from xml for ease of identification")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Names matches those from xml for ease of identification")]
    private static class ElementNames
    {
        public const string RootElement = "NotifyBillingMasterData_MarketDocument";

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
            public const string MktActivityRecordElement = "MktActivityRecord";

            public static class MktActivityRecord
            {
                public const string MRID = "mRID";
                public const string BusinessProcessReferenceMktActivityRecordmRID = "businessProcessReference_MktActivityRecord.mRID";
                public const string OriginalTransactionIDReferenceMktActivityRecordmRID = "originalTransactionIDReference_MktActivityRecord.mRID";
                public const string MarketEvaluationPointmRID = "marketEvaluationPoint.mRID";
                public const string ChargeGroupElement = "ChargeGroup";

                public static class ChargeGroup
                {
                    public const string ChargeTypeElement = "ChargeType";

                    public static class ChargeType
                    {
                        public const string ChargeTypeOwnerMarketParticipantmRID = "chargeTypeOwner_MarketParticipant.mRID";
                        public const string Type = "type";
                        public const string MRID = "mRID";
                        public const string Factor = "factor";
                        public const string EffectiveDate = "effectiveDate";
                        public const string TerminationDate = "terminationDate";
                    }
                }
            }
        }
    }
}
