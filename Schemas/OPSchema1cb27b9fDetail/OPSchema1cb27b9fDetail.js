define("OPSchema1cb27b9fDetail", ["ServiceHelper"], function(ServiceHelper) {
	return {
		entitySchemaName: "OPVehicleModel",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"parentName": "Detail",
				"propertyName": "tools",
				"name": "ImportVehicleButton",
				"values": {
					"itemType": BPMSoft.ViewItemType.BUTTON,
					"caption": {"bindTo": "Resources.Strings.ImportButtonCaption"},
					"click": {"bindTo": "onImportButtonClick"},
				}
			}
		]/**SCHEMA_DIFF*/,
		methods: {

			onImportButtonClick: function()	{
				
				var masterRecordId = this.get("MasterRecordId");
				var externalId = this.sandbox.publish("GetColumnsValues", ["OPExternalId"], [this.sandbox.id]);

				var serviceData = {
					"brandId": this.get("MasterRecordId"),
    				"externalBrandId": externalId.OPExternalId
				};
				
				this.showBodyMask();

				ServiceHelper.callService("OPVehicleService", "ImportModels",
                    function(response) {
						this.hideBodyMask();
						
                        var result = response.ImportModelsResult;

						if(result && result.isSuccess){
                        	this.showInformationDialog(result.value);
							this.reloadGridData();
						}

						else
							this.showInformationDialog(result.error.message);

                    }, serviceData, this);
			}

		}
	};
});
