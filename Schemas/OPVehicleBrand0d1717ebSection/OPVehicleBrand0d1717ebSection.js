define("OPVehicleBrand0d1717ebSection", ["ServiceHelper"], function(ServiceHelper) {
	return {
		entitySchemaName: "OPVehicleBrand",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"parentName": "SeparateModeActionButtonsContainer",
				"propertyName": "items",
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
							
				var serviceData = {};
				
				ServiceHelper.callService("OPVehicleBrandService", "ImportBrands",
                    function(response) {
                        var result = response.ImportBrandsResult;

						if(result.isSuccess)
                        	this.showInformationDialog(result.value);

						else
							this.showInformationDialog(result.error);

                    }, serviceData, this);
			}
		}
	};
});
